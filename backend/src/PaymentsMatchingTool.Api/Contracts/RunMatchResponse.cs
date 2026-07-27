using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Api.Contracts;

public record RunMatchResponse(Guid BatchId, MatchSummaryDto Summary, List<PaymentMatchDto> Items)
{
    public static RunMatchResponse From(MatchBatch batch) => new(
        batch.Id,
        MatchSummaryDto.From(batch),
        batch.Items
            .OrderBy(i => i.OrderId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(i => i.Currency, StringComparer.OrdinalIgnoreCase)
            .Select(PaymentMatchDto.From)
            .ToList());
}
