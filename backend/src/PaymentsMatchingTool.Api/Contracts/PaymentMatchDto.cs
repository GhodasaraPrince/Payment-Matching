using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Api.Contracts;

public record PaymentMatchDto(
    Guid Id,
    string OrderId,
    string Currency,
    decimal? SystemAmount,
    decimal? ProviderAmount,
    string Status,
    bool Resolved,
    string? ResolutionSide,
    DateTime? ResolvedAtUtc)
{
    public static PaymentMatchDto From(PaymentMatch match) => new(
        match.Id,
        match.OrderId,
        match.Currency,
        match.SystemAmount,
        match.ProviderAmount,
        match.Status.ToString(),
        match.Resolved,
        match.ResolutionSide?.ToString(),
        match.ResolvedAtUtc);
}
