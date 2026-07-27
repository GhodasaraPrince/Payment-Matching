using PaymentsMatchingTool.Application.Csv;
using PaymentsMatchingTool.Application.Matching;
using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Application.Tests.Matching;

public class PaymentMatchingServiceTests
{
    private readonly PaymentMatchingService _service = new();

    [Fact]
    public void Match_SameOrderIdAndAmountBothFiles_IsMatched()
    {
        var system = new[] { new CsvPaymentRow("ORD-1", "INR", 100m) };
        var provider = new[] { new CsvPaymentRow("ORD-1", "INR", 100m) };

        var result = _service.Match(system, provider);

        var row = Assert.Single(result);
        Assert.Equal(MatchStatus.Matched, row.Status);
        Assert.Equal(100m, row.SystemAmount);
        Assert.Equal(100m, row.ProviderAmount);
    }

    [Fact]
    public void Match_DifferentAmounts_IsAmountMismatch()
    {
        var system = new[] { new CsvPaymentRow("ORD-2", "INR", 200m) };
        var provider = new[] { new CsvPaymentRow("ORD-2", "INR", 180m) };

        var result = _service.Match(system, provider);

        var row = Assert.Single(result);
        Assert.Equal(MatchStatus.AmountMismatch, row.Status);
    }

    [Fact]
    public void Match_OnlyInSystemFile_IsOnlySystem()
    {
        var system = new[] { new CsvPaymentRow("ORD-4", "INR", 130m) };
        var provider = Array.Empty<CsvPaymentRow>();

        var result = _service.Match(system, provider);

        var row = Assert.Single(result);
        Assert.Equal(MatchStatus.OnlySystem, row.Status);
        Assert.Equal(130m, row.SystemAmount);
        Assert.Null(row.ProviderAmount);
    }

    [Fact]
    public void Match_OnlyInProviderFile_IsOnlyProvider()
    {
        var system = Array.Empty<CsvPaymentRow>();
        var provider = new[] { new CsvPaymentRow("ORD-5", "INR", 130m) };

        var result = _service.Match(system, provider);

        var row = Assert.Single(result);
        Assert.Equal(MatchStatus.OnlyProvider, row.Status);
        Assert.Null(row.SystemAmount);
        Assert.Equal(130m, row.ProviderAmount);
    }

    [Fact]
    public void Match_SameOrderIdDifferentCurrency_TreatedAsSeparateKeys()
    {
        var system = new[]
        {
            new CsvPaymentRow("ORD-1", "INR", 100m),
            new CsvPaymentRow("ORD-1", "USD", 50m),
        };
        var provider = new[] { new CsvPaymentRow("ORD-1", "INR", 100m) };

        var result = _service.Match(system, provider);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Currency == "INR" && r.Status == MatchStatus.Matched);
        Assert.Contains(result, r => r.Currency == "USD" && r.Status == MatchStatus.OnlySystem);
    }

    [Fact]
    public void Match_DuplicateKeyWithinSameFile_LastRowWins()
    {
        var system = new[]
        {
            new CsvPaymentRow("ORD-1", "INR", 100m),
            new CsvPaymentRow("ORD-1", "INR", 999m),
        };
        var provider = new[] { new CsvPaymentRow("ORD-1", "INR", 999m) };

        var result = _service.Match(system, provider);

        var row = Assert.Single(result);
        Assert.Equal(MatchStatus.Matched, row.Status);
        Assert.Equal(999m, row.SystemAmount);
    }

    [Fact]
    public void Match_SampleFromSpec_ProducesExpectedSummaryCounts()
    {
        var system = new[]
        {
            new CsvPaymentRow("ORD-1", "INR", 100m),
            new CsvPaymentRow("ORD-2", "INR", 200m),
            new CsvPaymentRow("ORD-3", "USD", 150m),
            new CsvPaymentRow("ORD-4", "INR", 130m),
        };
        var provider = new[]
        {
            new CsvPaymentRow("ORD-1", "INR", 100m),
            new CsvPaymentRow("ORD-2", "INR", 180m),
            new CsvPaymentRow("ORD-3", "USD", 150m),
            new CsvPaymentRow("ORD-5", "INR", 130m),
        };

        var result = _service.Match(system, provider);

        Assert.Equal(5, result.Count);
        Assert.Equal(2, result.Count(r => r.Status == MatchStatus.Matched));
        Assert.Equal(1, result.Count(r => r.Status == MatchStatus.AmountMismatch));
        Assert.Equal(1, result.Count(r => r.Status == MatchStatus.OnlySystem));
        Assert.Equal(1, result.Count(r => r.Status == MatchStatus.OnlyProvider));
    }
}
