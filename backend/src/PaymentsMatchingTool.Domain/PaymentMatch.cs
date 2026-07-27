namespace PaymentsMatchingTool.Domain;

public class PaymentMatch
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public MatchBatch? Batch { get; set; }

    public string OrderId { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal? SystemAmount { get; set; }
    public decimal? ProviderAmount { get; set; }
    public MatchStatus Status { get; set; }

    public bool Resolved { get; set; }
    public ResolutionSide? ResolutionSide { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
}
