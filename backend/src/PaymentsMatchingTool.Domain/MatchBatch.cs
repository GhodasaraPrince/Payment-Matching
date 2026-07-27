namespace PaymentsMatchingTool.Domain;

public class MatchBatch
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string SystemFileName { get; set; } = string.Empty;
    public string ProviderFileName { get; set; } = string.Empty;

    public int TotalCount { get; set; }
    public int MatchedCount { get; set; }
    public int OnlySystemCount { get; set; }
    public int OnlyProviderCount { get; set; }
    public int AmountMismatchCount { get; set; }

    public ICollection<PaymentMatch> Items { get; set; } = new List<PaymentMatch>();
}
