using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Application.Matching;

public interface IMatchRunService
{
    /// <summary>Parses both CSVs, classifies every row, and persists a new batch + its items.</summary>
    Task<MatchBatch> RunMatchAsync(
        Stream systemCsv,
        string systemFileName,
        Stream providerCsv,
        string providerFileName,
        CancellationToken cancellationToken = default);

    Task<MatchBatch?> GetBatchAsync(Guid batchId, CancellationToken cancellationToken = default);

    /// <summary>Returns one page of batches, most recently created first, plus the total batch count.</summary>
    Task<(List<MatchBatch> Batches, int TotalCount)> GetAllBatchesAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>Returns one page of items for a batch, plus the total item count matching the filter.</summary>
    Task<(List<PaymentMatch> Items, int TotalCount)> GetItemsAsync(
        Guid batchId,
        MatchFilter filter,
        int page = 1,
        int pageSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the updated item, or null if no item with that id exists.</summary>
    Task<PaymentMatch?> ResolveAsync(
        Guid itemId,
        ResolutionSide resolutionSide,
        CancellationToken cancellationToken = default);
}
