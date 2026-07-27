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

    Task<List<PaymentMatch>> GetItemsAsync(
        Guid batchId,
        MatchFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the updated item, or null if no item with that id exists.</summary>
    Task<PaymentMatch?> ResolveAsync(
        Guid itemId,
        ResolutionSide resolutionSide,
        CancellationToken cancellationToken = default);
}
