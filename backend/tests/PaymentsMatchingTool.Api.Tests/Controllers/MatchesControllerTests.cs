using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentsMatchingTool.Api.Contracts;
using PaymentsMatchingTool.Api.Controllers;
using PaymentsMatchingTool.Application.Csv;
using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Api.Tests.Controllers;

public class MatchesControllerTests
{
    private static IFormFile MakeFormFile(string fileName, string content = "orderId,amount,currency\nORD-1,100,USD\n")
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName);
    }

    private static MatchBatch NewBatch() => new()
    {
        Id = Guid.NewGuid(),
        CreatedAtUtc = DateTime.UtcNow,
        SystemFileName = "system.csv",
        ProviderFileName = "provider.csv",
        TotalCount = 0,
        MatchedCount = 0,
        OnlySystemCount = 0,
        OnlyProviderCount = 0,
        AmountMismatchCount = 0,
    };

    [Fact]
    public async Task RunMatch_MissingSystemFile_ReturnsBadRequest()
    {
        var controller = new MatchesController(new FakeMatchRunService());

        var result = await controller.RunMatch(
            new RunMatchRequest { SystemFile = null, ProviderFile = MakeFormFile("provider.csv") },
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task RunMatch_NonCsvExtension_ReturnsBadRequest()
    {
        var controller = new MatchesController(new FakeMatchRunService());

        var result = await controller.RunMatch(
            new RunMatchRequest
            {
                SystemFile = MakeFormFile("system.txt"),
                ProviderFile = MakeFormFile("provider.csv"),
            },
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task RunMatch_CsvValidationExceptionFromService_ReturnsBadRequestWithMessage()
    {
        var service = new FakeMatchRunService { RunMatchThrows = new CsvValidationException("bad currency") };
        var controller = new MatchesController(service);

        var result = await controller.RunMatch(
            new RunMatchRequest
            {
                SystemFile = MakeFormFile("system.csv"),
                ProviderFile = MakeFormFile("provider.csv"),
            },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var message = badRequest.Value!.GetType().GetProperty("message")!.GetValue(badRequest.Value);
        Assert.Equal("bad currency", message);
    }

    [Fact]
    public async Task RunMatch_Success_ReturnsOkWithBatch()
    {
        var batch = NewBatch();
        var service = new FakeMatchRunService { BatchToReturn = batch };
        var controller = new MatchesController(service);

        var result = await controller.RunMatch(
            new RunMatchRequest
            {
                SystemFile = MakeFormFile("system.csv"),
                ProviderFile = MakeFormFile("provider.csv"),
            },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<RunMatchResponse>(ok.Value);
        Assert.Equal(batch.Id, response.BatchId);
    }

    [Fact]
    public async Task GetAllBatches_ReturnsPagedResponse()
    {
        var batches = new List<MatchBatch> { NewBatch() };
        var service = new FakeMatchRunService { BatchesToReturn = (batches, 1) };
        var controller = new MatchesController(service);

        var result = await controller.GetAllBatches(page: 1, pageSize: 20, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PagedResponse<MatchBatchSummaryDto>>(ok.Value);
        Assert.Single(response.Items);
        Assert.Equal(1, response.TotalCount);
    }

    [Fact]
    public async Task GetItems_InvalidFilter_ReturnsBadRequest()
    {
        var controller = new MatchesController(new FakeMatchRunService());

        var result = await controller.GetItems(Guid.NewGuid(), filter: "bogus", page: 1, pageSize: 100, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetItems_BatchNotFound_ReturnsNotFound()
    {
        var service = new FakeMatchRunService { BatchToReturn = null };
        var controller = new MatchesController(service);

        var result = await controller.GetItems(Guid.NewGuid(), filter: "all", page: 1, pageSize: 100, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetItems_Found_ReturnsOkWithPagingMetadata()
    {
        var batch = NewBatch();
        var service = new FakeMatchRunService
        {
            BatchToReturn = batch,
            ItemsToReturn = (new List<PaymentMatch>(), 0),
        };
        var controller = new MatchesController(service);

        var result = await controller.GetItems(batch.Id, filter: "all", page: 1, pageSize: 100, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MatchBatchDetailResponse>(ok.Value);
        Assert.Equal(1, response.Page);
        Assert.Equal(100, response.PageSize);
    }

    [Fact]
    public async Task Resolve_InvalidResolutionSide_ReturnsBadRequest()
    {
        var controller = new MatchesController(new FakeMatchRunService());

        var result = await controller.Resolve(Guid.NewGuid(), new ResolveRequest("Bogus"), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Resolve_ItemNotFound_ReturnsNotFound()
    {
        var service = new FakeMatchRunService { ResolvedItemToReturn = null };
        var controller = new MatchesController(service);

        var result = await controller.Resolve(Guid.NewGuid(), new ResolveRequest("System"), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Resolve_Success_ReturnsOkWithUpdatedItem()
    {
        var item = new PaymentMatch
        {
            Id = Guid.NewGuid(),
            BatchId = Guid.NewGuid(),
            OrderId = "ORD-1",
            Currency = "USD",
            Status = MatchStatus.Matched,
            Resolved = true,
            ResolutionSide = ResolutionSide.System,
        };
        var service = new FakeMatchRunService { ResolvedItemToReturn = item };
        var controller = new MatchesController(service);

        var result = await controller.Resolve(item.Id, new ResolveRequest("System"), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<PaymentMatchDto>(ok.Value);
        Assert.Equal(item.Id, dto.Id);
        Assert.Equal("System", dto.ResolutionSide);
    }
}
