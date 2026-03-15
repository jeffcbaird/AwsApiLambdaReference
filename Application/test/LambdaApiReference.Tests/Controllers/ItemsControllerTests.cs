using FluentAssertions;
using LambdaApiReference.Controllers;
using LambdaApiReference.Models;
using LambdaApiReference.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LambdaApiReference.Tests.Controllers;

public class ItemsControllerTests
{
    private static readonly Item SampleItem = new()
    {
        Id          = "item-1",
        Name        = "Test Item",
        Description = "Description",
        CreatedAt   = DateTime.UtcNow
    };

    private static ItemsController BuildController(Mock<IItemService> serviceMock) =>
        new(serviceMock.Object, NullLogger<ItemsController>.Instance);

    [Fact]
    public async Task GetAll_ReturnsOk_WithItems()
    {
        var service = new Mock<IItemService>();
        service.Setup(r => r.GetAllAsync()).ReturnsAsync([SampleItem]);

        var result = await BuildController(service).GetAll() as OkObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);
        result.Value.As<IEnumerable<Item>>().Should().ContainSingle();
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var service = new Mock<IItemService>();
        service.Setup(r => r.GetByIdAsync("item-1")).ReturnsAsync(SampleItem);

        var result = await BuildController(service).GetById("item-1") as OkObjectResult;

        result.Should().NotBeNull();
        result!.Value.As<Item>().Id.Should().Be("item-1");
    }

    [Fact]
    public async Task GetById_MissingId_ReturnsNotFound()
    {
        var service = new Mock<IItemService>();
        service.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Item?)null);

        var result = await BuildController(service).GetById("ghost");

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        var request = new CreateItemRequest { Name = "New", Description = "Desc" };
        var service    = new Mock<IItemService>();
        service.Setup(r => r.CreateAsync(request)).ReturnsAsync(SampleItem);

        var result = await BuildController(service).Create(request) as CreatedAtActionResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status201Created);
        result.RouteValues!["id"].Should().Be(SampleItem.Id);
        result.Value.As<Item>().Id.Should().Be(SampleItem.Id);
    }

    [Fact]
    public async Task Update_ExistingId_ReturnsOk()
    {
        var request = new UpdateItemRequest { Name = "Updated" };
        var updated = SampleItem with { Name = "Updated" };
        var service    = new Mock<IItemService>();
        service.Setup(r => r.UpdateAsync("item-1", request)).ReturnsAsync(updated);

        var result = await BuildController(service).Update("item-1", request) as OkObjectResult;

        result.Should().NotBeNull();
        result!.Value.As<Item>().Name.Should().Be("Updated");
    }

    [Fact]
    public async Task Update_MissingId_ReturnsNotFound()
    {
        var request = new UpdateItemRequest { Name = "X" };
        var service    = new Mock<IItemService>();
        service.Setup(r => r.UpdateAsync(It.IsAny<string>(), request)).ReturnsAsync((Item?)null);

        var result = await BuildController(service).Update("ghost", request);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ExistingId_ReturnsNoContent()
    {
        var service = new Mock<IItemService>();
        service.Setup(r => r.DeleteAsync("item-1")).ReturnsAsync(true);

        var result = await BuildController(service).Delete("item-1");

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_MissingId_ReturnsNotFound()
    {
        var service = new Mock<IItemService>();
        service.Setup(r => r.DeleteAsync(It.IsAny<string>())).ReturnsAsync(false);

        var result = await BuildController(service).Delete("ghost");

        result.Should().BeOfType<NotFoundResult>();
    }
}
