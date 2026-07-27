using Microsoft.AspNetCore.Mvc;
using PaymentsMatchingTool.Api.Contracts;
using PaymentsMatchingTool.Application.Csv;
using PaymentsMatchingTool.Application.Matching;
using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Api.Controllers;

[ApiController]
[Route("api/matches")]
public class MatchesController : ControllerBase
{
    private readonly IMatchRunService _matchRunService;

    public MatchesController(IMatchRunService matchRunService)
    {
        _matchRunService = matchRunService;
    }

    [HttpPost("run")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<RunMatchResponse>> RunMatch(
        [FromForm] RunMatchRequest request,
        CancellationToken cancellationToken)
    {
        if (request.SystemFile is null || request.SystemFile.Length == 0)
        {
            return BadRequest(new { message = "System CSV file is required." });
        }

        if (request.ProviderFile is null || request.ProviderFile.Length == 0)
        {
            return BadRequest(new { message = "Provider CSV file is required." });
        }

        try
        {
            await using var systemStream = request.SystemFile.OpenReadStream();
            await using var providerStream = request.ProviderFile.OpenReadStream();

            var batch = await _matchRunService.RunMatchAsync(
                systemStream,
                request.SystemFile.FileName,
                providerStream,
                request.ProviderFile.FileName,
                cancellationToken);

            return Ok(RunMatchResponse.From(batch));
        }
        catch (CsvValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<MatchBatchSummaryDto>>> GetAllBatches(CancellationToken cancellationToken)
    {
        var batches = await _matchRunService.GetAllBatchesAsync(cancellationToken);
        return Ok(batches.Select(MatchBatchSummaryDto.From).ToList());
    }

    [HttpGet("{batchId:guid}")]
    public async Task<ActionResult<MatchBatchDetailResponse>> GetItems(
        Guid batchId,
        [FromQuery] string filter = "all",
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<MatchFilter>(filter, ignoreCase: true, out var parsedFilter))
        {
            return BadRequest(new { message = "filter must be one of: unresolved, resolved, all." });
        }

        var batch = await _matchRunService.GetBatchAsync(batchId, cancellationToken);
        if (batch is null)
        {
            return NotFound();
        }

        var items = await _matchRunService.GetItemsAsync(batchId, parsedFilter, cancellationToken);
        return Ok(MatchBatchDetailResponse.From(batch, items));
    }

    [HttpPatch("items/{id:guid}/resolve")]
    public async Task<ActionResult<PaymentMatchDto>> Resolve(
        Guid id,
        [FromBody] ResolveRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ResolutionSide>(request.ResolutionSide, ignoreCase: true, out var side))
        {
            return BadRequest(new { message = "resolutionSide must be 'System' or 'Provider'." });
        }

        var updated = await _matchRunService.ResolveAsync(id, side, cancellationToken);
        if (updated is null)
        {
            return NotFound();
        }

        return Ok(PaymentMatchDto.From(updated));
    }
}
