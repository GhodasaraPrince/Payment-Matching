using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    [EnableRateLimiting("upload")]
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

        if (!HasCsvExtension(request.SystemFile.FileName))
        {
            return BadRequest(new { message = "System CSV file must have a .csv extension." });
        }

        if (!HasCsvExtension(request.ProviderFile.FileName))
        {
            return BadRequest(new { message = "Provider CSV file must have a .csv extension." });
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
    public async Task<ActionResult<PagedResponse<MatchBatchSummaryDto>>> GetAllBatches(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (batches, totalCount) = await _matchRunService.GetAllBatchesAsync(page, pageSize, cancellationToken);
        return Ok(new PagedResponse<MatchBatchSummaryDto>(
            batches.Select(MatchBatchSummaryDto.From).ToList(),
            page,
            pageSize,
            totalCount));
    }

    [HttpGet("{batchId:guid}")]
    public async Task<ActionResult<MatchBatchDetailResponse>> GetItems(
        Guid batchId,
        [FromQuery] string filter = "all",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<MatchFilter>(filter, ignoreCase: true, out var parsedFilter))
        {
            return BadRequest(new { message = "filter must be one of: unresolved, resolved, all." });
        }

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 500);

        var batch = await _matchRunService.GetBatchAsync(batchId, cancellationToken);
        if (batch is null)
        {
            return NotFound();
        }

        var (items, totalCount) = await _matchRunService.GetItemsAsync(
            batchId, parsedFilter, page, pageSize, cancellationToken);
        return Ok(MatchBatchDetailResponse.From(batch, items, page, pageSize, totalCount));
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

    private static bool HasCsvExtension(string fileName) =>
        fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
}
