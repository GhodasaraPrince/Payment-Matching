using PaymentsMatchingTool.Application.Matching;
using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Api.Tests.Controllers;

internal sealed class FakeMatchRunService : IMatchRunService
{
    public MatchBatch? BatchToReturn { get; set; }
    public (List<MatchBatch> Batches, int TotalCount) BatchesToReturn { get; set; } = (new List<MatchBatch>(), 0);
    public (List<PaymentMatch> Items, int TotalCount) ItemsToReturn { get; set; } = (new List<PaymentMatch>(), 0);
    public PaymentMatch? ResolvedItemToReturn { get; set; }
    public Exception? RunMatchThrows { get; set; }

    public Task<MatchBatch> RunMatchAsync(
        Stream systemCsv,
        string systemFileName,
        Stream providerCsv,
        string providerFileName,
        CancellationToken cancellationToken = default)
    {
        if (RunMatchThrows is not null)
        {
            throw RunMatchThrows;
        }

        return Task.FromResult(BatchToReturn ?? throw new InvalidOperationException("BatchToReturn not set"));
    }

    public Task<MatchBatch?> GetBatchAsync(Guid batchId, CancellationToken cancellationToken = default) =>
        Task.FromResult(BatchToReturn);

    public Task<(List<MatchBatch> Batches, int TotalCount)> GetAllBatchesAsync(
        int page = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        Task.FromResult(BatchesToReturn);

    public Task<(List<PaymentMatch> Items, int TotalCount)> GetItemsAsync(
        Guid batchId, MatchFilter filter, int page = 1, int pageSize = 100, CancellationToken cancellationToken = default) =>
        Task.FromResult(ItemsToReturn);

    public Task<PaymentMatch?> ResolveAsync(
        Guid itemId, ResolutionSide resolutionSide, CancellationToken cancellationToken = default) =>
        Task.FromResult(ResolvedItemToReturn);
}
