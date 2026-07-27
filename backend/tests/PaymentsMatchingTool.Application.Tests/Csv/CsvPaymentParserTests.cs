using System.Text;
using PaymentsMatchingTool.Application.Csv;

namespace PaymentsMatchingTool.Application.Tests.Csv;

public class CsvPaymentParserTests
{
    private readonly CsvPaymentParser _parser = new();

    private static Stream ToStream(string csv) => new MemoryStream(Encoding.UTF8.GetBytes(csv));

    [Fact]
    public void Parse_SampleCsvFromSpec_ReturnsFourRows()
    {
        const string csv = "orderId,amount,currency\nORD-1,100,INR\nORD-2,200,INR\nORD-3,150,USD\nORD-4,130,INR\n";

        var rows = _parser.Parse(ToStream(csv), "system.csv");

        Assert.Equal(4, rows.Count);
        Assert.Contains(rows, r => r is { OrderId: "ORD-1", Currency: "INR", Amount: 100m });
        Assert.Contains(rows, r => r is { OrderId: "ORD-3", Currency: "USD", Amount: 150m });
    }

    [Fact]
    public void Parse_UnsupportedCurrency_ThrowsCsvValidationException()
    {
        const string csv = "orderId,amount,currency\nORD-1,100,JPY\n";

        var ex = Assert.Throws<CsvValidationException>(() => _parser.Parse(ToStream(csv), "system.csv"));
        Assert.Contains("JPY", ex.Message);
    }

    [Fact]
    public void Parse_NonNumericAmount_ThrowsCsvValidationException()
    {
        const string csv = "orderId,amount,currency\nORD-1,abc,USD\n";

        var ex = Assert.Throws<CsvValidationException>(() => _parser.Parse(ToStream(csv), "system.csv"));
        Assert.Contains("non-numeric", ex.Message);
    }

    [Fact]
    public void Parse_MissingOrderId_ThrowsCsvValidationException()
    {
        const string csv = "orderId,amount,currency\n,100,USD\n";

        var ex = Assert.Throws<CsvValidationException>(() => _parser.Parse(ToStream(csv), "system.csv"));
        Assert.Contains("missing orderId", ex.Message);
    }

    [Fact]
    public void Parse_CurrencyIsCaseInsensitiveAndTrimmed_NormalizesToUppercase()
    {
        const string csv = "orderId,amount,currency\nORD-1,100, inr \n";

        var rows = _parser.Parse(ToStream(csv), "system.csv");

        Assert.Equal("INR", rows[0].Currency);
    }
}
