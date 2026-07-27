using Microsoft.EntityFrameworkCore;
using PaymentsMatchingTool.Application.Csv;
using PaymentsMatchingTool.Application.Matching;
using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Infrastructure.Services;

public class MatchRunService : IMatchRunService
{
    private readonly AppDbContext _db;
    private readonly ICsvPaymentParser _csvParser;
    private readonly IPaymentMatchingService _matchingService;

    public MatchRunService(AppDbContext db, ICsvPaymentParser csvParser, IPaymentMatchingService matchingService)
    {
        _db = db;
        _csvParser = csvParser;
        _matchingService = matchingService;
    }

    public async Task<MatchBatch> RunMatchAsync(
        Stream systemCsv,
        string systemFileName,
        Stream providerCsv,
        string providerFileName,
        CancellationToken cancellationToken = default)
    {
        var systemRows = _csvParser.Parse(systemCsv, "System CSV");
        var providerRows = _csvParser.Parse(providerCsv, "Provider CSV");

        var items = _matchingService.Match(systemRows, providerRows);

        var batch = new MatchBatch
        {
            Id = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow,
            SystemFileName = systemFileName,
            ProviderFileName = providerFileName,
            TotalCount = items.Count,
            MatchedCount = items.Count(i => i.Status == MatchStatus.Matched),
            OnlySystemCount = items.Count(i => i.Status == MatchStatus.OnlySystem),
            OnlyProviderCount = items.Count(i => i.Status == MatchStatus.OnlyProvider),
            AmountMismatchCount = items.Count(i => i.Status == MatchStatus.AmountMismatch),
        };

        foreach (var item in items)
        {
            item.BatchId = batch.Id;
        }

        batch.Items = items;

        _db.MatchBatches.Add(batch);
        await _db.SaveChangesAsync(cancellationToken);

        return batch;
    }

    public Task<MatchBatch?> GetBatchAsync(Guid batchId, CancellationToken cancellationToken = default) =>
        _db.MatchBatches.FirstOrDefaultAsync(b => b.Id == batchId, cancellationToken);

    public Task<List<MatchBatch>> GetAllBatchesAsync(CancellationToken cancellationToken = default) =>
        _db.MatchBatches.OrderByDescending(b => b.CreatedAtUtc).ToListAsync(cancellationToken);

    public async Task<List<PaymentMatch>> GetItemsAsync(
        Guid batchId,
        MatchFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = _db.PaymentMatches.Where(i => i.BatchId == batchId);

        query = filter switch
        {
            MatchFilter.Resolved => query.Where(i => i.Resolved),
            MatchFilter.Unresolved => query.Where(i => !i.Resolved),
            _ => query,
        };

        return await query
            .OrderBy(i => i.OrderId)
            .ThenBy(i => i.Currency)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentMatch?> ResolveAsync(
        Guid itemId,
        ResolutionSide resolutionSide,
        CancellationToken cancellationToken = default)
    {
        var item = await _db.PaymentMatches.FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
        if (item is null)
        {
            return null;
        }

        item.Resolved = true;
        item.ResolutionSide = resolutionSide;
        item.ResolvedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return item;
    }
}
