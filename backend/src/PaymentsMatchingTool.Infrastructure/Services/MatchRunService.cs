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

    public async Task<(List<MatchBatch> Batches, int TotalCount)> GetAllBatchesAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _db.MatchBatches.OrderByDescending(b => b.CreatedAtUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var batches = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (batches, totalCount);
    }

    public async Task<(List<PaymentMatch> Items, int TotalCount)> GetItemsAsync(
        Guid batchId,
        MatchFilter filter,
        int page = 1,
        int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _db.PaymentMatches.Where(i => i.BatchId == batchId);

        query = filter switch
        {
            MatchFilter.Resolved => query.Where(i => i.Resolved),
            MatchFilter.Unresolved => query.Where(i => !i.Resolved),
            _ => query,
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(i => i.OrderId)
            .ThenBy(i => i.Currency)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
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
