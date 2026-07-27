using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Api.Contracts;

public record MatchBatchDetailResponse(
    Guid BatchId,
    DateTime CreatedAtUtc,
    string SystemFileName,
    string ProviderFileName,
    MatchSummaryDto Summary,
    List<PaymentMatchDto> Items,
    int Page,
    int PageSize,
    int TotalItems)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public static MatchBatchDetailResponse From(
        MatchBatch batch, List<PaymentMatch> items, int page, int pageSize, int totalItems) => new(
        batch.Id,
        batch.CreatedAtUtc,
        batch.SystemFileName,
        batch.ProviderFileName,
        MatchSummaryDto.From(batch),
        items.Select(PaymentMatchDto.From).ToList(),
        page,
        pageSize,
        totalItems);
}
