using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Api.Contracts;

public record MatchBatchDetailResponse(
    Guid BatchId,
    DateTime CreatedAtUtc,
    string SystemFileName,
    string ProviderFileName,
    MatchSummaryDto Summary,
    List<PaymentMatchDto> Items)
{
    public static MatchBatchDetailResponse From(MatchBatch batch, List<PaymentMatch> items) => new(
        batch.Id,
        batch.CreatedAtUtc,
        batch.SystemFileName,
        batch.ProviderFileName,
        MatchSummaryDto.From(batch),
        items.Select(PaymentMatchDto.From).ToList());
}
