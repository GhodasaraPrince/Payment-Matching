namespace PaymentsMatchingTool.Application.Csv;

public record CsvPaymentRow(string OrderId, string Currency, decimal Amount);
