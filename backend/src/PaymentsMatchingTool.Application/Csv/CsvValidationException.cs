namespace PaymentsMatchingTool.Application.Csv;

public class CsvValidationException : Exception
{
    public CsvValidationException(string message) : base(message)
    {
    }
}
