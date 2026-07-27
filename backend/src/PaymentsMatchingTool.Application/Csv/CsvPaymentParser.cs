using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace PaymentsMatchingTool.Application.Csv;

public class CsvPaymentParser : ICsvPaymentParser
{
    private static readonly HashSet<string> AllowedCurrencies =
        new(StringComparer.OrdinalIgnoreCase) { "USD", "EUR", "INR", "GBP" };

    public List<CsvPaymentRow> Parse(Stream csvStream, string fileLabel)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            MissingFieldFound = null,
            HeaderValidated = null,
        };

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<CsvPaymentRowRawMap>();

        List<CsvPaymentRowRaw> rawRows;
        try
        {
            rawRows = csv.GetRecords<CsvPaymentRowRaw>().ToList();
        }
        catch (CsvHelperException ex)
        {
            throw new CsvValidationException(
                $"{fileLabel}: could not parse CSV — {ex.Message}");
        }

        var rows = new List<CsvPaymentRow>(rawRows.Count);
        for (var i = 0; i < rawRows.Count; i++)
        {
            var raw = rawRows[i];
            var rowNumber = i + 2; // header occupies row 1

            if (string.IsNullOrWhiteSpace(raw.OrderId))
            {
                throw new CsvValidationException($"{fileLabel}: row {rowNumber} is missing orderId.");
            }

            if (!decimal.TryParse(raw.Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            {
                throw new CsvValidationException(
                    $"{fileLabel}: row {rowNumber} has a non-numeric amount '{raw.Amount}'.");
            }

            var currency = raw.Currency?.Trim() ?? string.Empty;
            if (!AllowedCurrencies.Contains(currency))
            {
                throw new CsvValidationException(
                    $"{fileLabel}: row {rowNumber} has an unsupported currency '{raw.Currency}'. " +
                    "Allowed: USD, EUR, INR, GBP.");
            }

            rows.Add(new CsvPaymentRow(raw.OrderId.Trim(), currency.ToUpperInvariant(), amount));
        }

        return rows;
    }

    public class CsvPaymentRowRaw
    {
        public string OrderId { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
    }

    private sealed class CsvPaymentRowRawMap : ClassMap<CsvPaymentRowRaw>
    {
        public CsvPaymentRowRawMap()
        {
            Map(m => m.OrderId).Name("orderId");
            Map(m => m.Amount).Name("amount");
            Map(m => m.Currency).Name("currency");
        }
    }
}
