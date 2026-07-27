using PaymentsMatchingTool.Application.Csv;
using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Application.Matching;

public interface IPaymentMatchingService
{
    /// <summary>
    /// Classifies every orderId+currency key from both files into a MatchStatus.
    /// Returned entities have no BatchId set — the caller assigns it before persisting.
    /// </summary>
    List<PaymentMatch> Match(IReadOnlyList<CsvPaymentRow> systemRows, IReadOnlyList<CsvPaymentRow> providerRows);
}
