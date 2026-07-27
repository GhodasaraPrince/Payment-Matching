using PaymentsMatchingTool.Application.Csv;
using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Application.Matching;

public class PaymentMatchingService : IPaymentMatchingService
{
    public List<PaymentMatch> Match(IReadOnlyList<CsvPaymentRow> systemRows, IReadOnlyList<CsvPaymentRow> providerRows)
    {
        var systemByKey = ToDictionary(systemRows);
        var providerByKey = ToDictionary(providerRows);

        var keys = systemByKey.Keys.Union(providerByKey.Keys);

        var results = new List<PaymentMatch>();
        foreach (var key in keys)
        {
            var hasSystem = systemByKey.TryGetValue(key, out var systemAmount);
            var hasProvider = providerByKey.TryGetValue(key, out var providerAmount);

            var status = (hasSystem, hasProvider) switch
            {
                (true, false) => MatchStatus.OnlySystem,
                (false, true) => MatchStatus.OnlyProvider,
                (true, true) when AmountsEqual(systemAmount, providerAmount) => MatchStatus.Matched,
                (true, true) => MatchStatus.AmountMismatch,
                _ => throw new InvalidOperationException("Key must exist in at least one file."),
            };

            results.Add(new PaymentMatch
            {
                Id = Guid.NewGuid(),
                OrderId = key.OrderId,
                Currency = key.Currency,
                SystemAmount = hasSystem ? systemAmount : null,
                ProviderAmount = hasProvider ? providerAmount : null,
                Status = status,
                Resolved = false,
            });
        }

        return results
            .OrderBy(r => r.OrderId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.Currency, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool AmountsEqual(decimal a, decimal b) => Math.Round(a, 2) == Math.Round(b, 2);

    // Duplicate orderId+currency rows within the same file: last row wins.
    private static Dictionary<(string OrderId, string Currency), decimal> ToDictionary(
        IReadOnlyList<CsvPaymentRow> rows)
    {
        var dict = new Dictionary<(string, string), decimal>();
        foreach (var row in rows)
        {
            dict[(row.OrderId, row.Currency)] = row.Amount;
        }

        return dict;
    }
}
