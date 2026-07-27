namespace PaymentsMatchingTool.Api.Contracts;

public class RunMatchRequest
{
    public IFormFile? SystemFile { get; set; }
    public IFormFile? ProviderFile { get; set; }
}
