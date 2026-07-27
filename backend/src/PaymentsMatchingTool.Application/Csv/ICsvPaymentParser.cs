namespace PaymentsMatchingTool.Application.Csv;

public interface ICsvPaymentParser
{
    /// <summary>
    /// Parses a "orderId,amount,currency" CSV. Throws <see cref="CsvValidationException"/>
    /// with a row-level message on any structural or data problem.
    /// </summary>
    List<CsvPaymentRow> Parse(Stream csvStream, string fileLabel);
}
