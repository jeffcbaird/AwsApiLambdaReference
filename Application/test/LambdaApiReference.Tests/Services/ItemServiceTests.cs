using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using FluentAssertions;
using LambdaApiReference.Models;
using LambdaApiReference.Repositories;
using LambdaApiReference.Services;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace LambdaApiReference.Tests.Services;

public class ItemServiceTests
{
    private static readonly Item SampleItem = new()
    {
        Id = "item-1",
        Name = "Test Item",
        Description = "Test Description",
        CreatedAt = DateTime.UtcNow
    };

    private static readonly CreateItemRequest CreateRequest = new()
    {
        Name = "New Item",
        Description = "New Description"
    };

    private static readonly UpdateItemRequest UpdateRequest = new()
    {
        Name = "Updated Item",
        Description = "Updated Description"
    };

    private static readonly ReferenceOptions TestOptions = new()
    {
        Region = "us-east-1",
        ReferencePublishSnsTopic = "arn:aws:sns:us-east-1:123456789012:test-topic"
    };

    private static Mock<IOptions<ReferenceOptions>> CreateOptionsMock()
    {
        var optionsMock = new Mock<IOptions<ReferenceOptions>>();
        optionsMock.Setup(o => o.Value).Returns(TestOptions);
        return optionsMock;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsItems_FromRepository()
    {
        var repositoryMock = new Mock<IItemRepository>();
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        var optionsMock = CreateOptionsMock();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync([SampleItem]);

        var service = new ItemService(repositoryMock.Object, snsMock.Object, optionsMock.Object);
        IEnumerable<Item> result = await service.GetAllAsync();

        result.Should().ContainSingle().Which.Should().BeEquivalentTo(SampleItem);
        repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoItems()
    {
        var repositoryMock = new Mock<IItemRepository>();
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

        var service = new ItemService(repositoryMock.Object, snsMock.Object, CreateOptionsMock().Object);
        IEnumerable<Item> result = await service.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsItem()
    {
        var repositoryMock = new Mock<IItemRepository>();
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        repositoryMock.Setup(r => r.GetByIdAsync("item-1")).ReturnsAsync(SampleItem);

        var service = new ItemService(repositoryMock.Object, snsMock.Object, CreateOptionsMock().Object);
        Item? result = await service.GetByIdAsync("item-1");

        result.Should().BeEquivalentTo(SampleItem);
        repositoryMock.Verify(r => r.GetByIdAsync("item-1"), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var repositoryMock = new Mock<IItemRepository>();
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        repositoryMock.Setup(r => r.GetByIdAsync("non-existing")).ReturnsAsync((Item?)null);

        var service = new ItemService(repositoryMock.Object, snsMock.Object, CreateOptionsMock().Object);
        Item? result = await service.GetByIdAsync("non-existing");

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesItemAndPublishesNotification()
    {
        var repositoryMock = new Mock<IItemRepository>();
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        repositoryMock.Setup(r => r.CreateAsync(CreateRequest)).ReturnsAsync(SampleItem);

        var service = new ItemService(repositoryMock.Object, snsMock.Object, CreateOptionsMock().Object);
        Item result = await service.CreateAsync(CreateRequest);

        result.Should().BeEquivalentTo(SampleItem);
        repositoryMock.Verify(r => r.CreateAsync(CreateRequest), Times.Once);
        snsMock.Verify(s => s.PublishAsync(It.Is<PublishRequest>(req =>
            req.TopicArn == TestOptions.ReferencePublishSnsTopic &&
            req.Message.Contains("CreateItem") &&
            req.Message.Contains(SampleItem.Id)
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_PublishesCorrectMessage()
    {
        var repositoryMock = new Mock<IItemRepository>();
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        repositoryMock.Setup(r => r.CreateAsync(CreateRequest)).ReturnsAsync(SampleItem);

        var service = new ItemService(repositoryMock.Object, snsMock.Object, CreateOptionsMock().Object);
        await service.CreateAsync(CreateRequest);

        snsMock.Verify(s => s.PublishAsync(It.Is<PublishRequest>(req =>
            req.TopicArn == TestOptions.ReferencePublishSnsTopic &&
            ValidateCreateMessage(req.Message, SampleItem)
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingId_UpdatesItemAndPublishesNotification()
    {
        Item updatedItem = SampleItem with { Name = UpdateRequest.Name, Description = UpdateRequest.Description };
        var repositoryMock = new Mock<IItemRepository>();
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        repositoryMock.Setup(r => r.UpdateAsync("item-1", UpdateRequest)).ReturnsAsync(updatedItem);

        var service = new ItemService(repositoryMock.Object, snsMock.Object, CreateOptionsMock().Object);
        Item? result = await service.UpdateAsync("item-1", UpdateRequest);

        result.Should().BeEquivalentTo(updatedItem);
        repositoryMock.Verify(r => r.UpdateAsync("item-1", UpdateRequest), Times.Once);
        snsMock.Verify(s => s.PublishAsync(It.Is<PublishRequest>(req =>
            req.TopicArn == TestOptions.ReferencePublishSnsTopic &&
            req.Message.Contains("UpdateItem") &&
            req.Message.Contains(updatedItem.Id)
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ReturnsNullAndDoesNotPublish()
    {
        var repositoryMock = new Mock<IItemRepository>();
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        repositoryMock.Setup(r => r.UpdateAsync("non-existing", UpdateRequest)).ReturnsAsync((Item?)null);

        var service = new ItemService(repositoryMock.Object, snsMock.Object, CreateOptionsMock().Object);
        Item? result = await service.UpdateAsync("non-existing", UpdateRequest);

        result.Should().BeNull();
        repositoryMock.Verify(r => r.UpdateAsync("non-existing", UpdateRequest), Times.Once);
        snsMock.Verify(s => s.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_PublishesCorrectMessage()
    {
        Item updatedItem = SampleItem with { Name = UpdateRequest.Name };
        var repositoryMock = new Mock<IItemRepository>();
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        repositoryMock.Setup(r => r.UpdateAsync("item-1", UpdateRequest)).ReturnsAsync(updatedItem);

        var service = new ItemService(repositoryMock.Object, snsMock.Object, CreateOptionsMock().Object);
        await service.UpdateAsync("item-1", UpdateRequest);

        snsMock.Verify(s => s.PublishAsync(It.Is<PublishRequest>(req =>
            req.TopicArn == TestOptions.ReferencePublishSnsTopic &&
            ValidateUpdateMessage(req.Message, updatedItem)
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_DeletesItemAndPublishesNotification()
    {
        var repositoryMock = new Mock<IItemRepository>();
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        repositoryMock.Setup(r => r.DeleteAsync("item-1")).ReturnsAsync(true);

        var service = new ItemService(repositoryMock.Object, snsMock.Object, CreateOptionsMock().Object);
        bool result = await service.DeleteAsync("item-1");

        result.Should().BeTrue();
        repositoryMock.Verify(r => r.DeleteAsync("item-1"), Times.Once);
        snsMock.Verify(s => s.PublishAsync(It.Is<PublishRequest>(req =>
            req.TopicArn == TestOptions.ReferencePublishSnsTopic &&
            req.Message.Contains("DeleteItem") &&
            req.Message.Contains("item-1")
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalseAndDoesNotPublish()
    {
        var repositoryMock = new Mock<IItemRepository>();
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        repositoryMock.Setup(r => r.DeleteAsync("non-existing")).ReturnsAsync(false);

        var service = new ItemService(repositoryMock.Object, snsMock.Object, CreateOptionsMock().Object);
        bool result = await service.DeleteAsync("non-existing");

        result.Should().BeFalse();
        repositoryMock.Verify(r => r.DeleteAsync("non-existing"), Times.Once);
        snsMock.Verify(s => s.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_PublishesCorrectMessage()
    {
        var repositoryMock = new Mock<IItemRepository>();
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        repositoryMock.Setup(r => r.DeleteAsync("item-1")).ReturnsAsync(true);

        var service = new ItemService(repositoryMock.Object, snsMock.Object, CreateOptionsMock().Object);
        await service.DeleteAsync("item-1");

        snsMock.Verify(s => s.PublishAsync(It.Is<PublishRequest>(req =>
            req.TopicArn == TestOptions.ReferencePublishSnsTopic &&
            ValidateDeleteMessage(req.Message, "item-1")
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_RepositoryThrowsException_PropagatesException()
    {
        var repositoryMock = new Mock<IItemRepository>();
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        repositoryMock.Setup(r => r.CreateAsync(It.IsAny<CreateItemRequest>()))
                      .ThrowsAsync(new InvalidOperationException("Repository error"));

        var service = new ItemService(repositoryMock.Object, snsMock.Object, CreateOptionsMock().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(CreateRequest));
        snsMock.Verify(s => s.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_RepositoryThrowsException_PropagatesException()
    {
        var repositoryMock = new Mock<IItemRepository>();
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<UpdateItemRequest>()))
                      .ThrowsAsync(new InvalidOperationException("Repository error"));

        var service = new ItemService(repositoryMock.Object, snsMock.Object, CreateOptionsMock().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync("item-1", UpdateRequest));
        snsMock.Verify(s => s.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RepositoryThrowsException_PropagatesException()
    {
        var repositoryMock = new Mock<IItemRepository>();
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<string>()))
                      .ThrowsAsync(new InvalidOperationException("Repository error"));

        var service = new ItemService(repositoryMock.Object, snsMock.Object, CreateOptionsMock().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync("item-1"));
        snsMock.Verify(s => s.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static bool ValidateCreateMessage(string message, Item item)
    {
        dynamic? messageObj = JsonConvert.DeserializeObject<dynamic>(message);
        return messageObj?.messageType == "CreateItem" &&
               messageObj?.id == item.Id &&
               messageObj?.value != null;
    }

    private static bool ValidateUpdateMessage(string message, Item item)
    {
        dynamic? messageObj = JsonConvert.DeserializeObject<dynamic>(message);
        return messageObj?.messageType == "UpdateItem" &&
               messageObj?.id == item.Id &&
               messageObj?.value != null;
    }

    private static bool ValidateDeleteMessage(string message, string itemId)
    {
        dynamic? messageObj = JsonConvert.DeserializeObject<dynamic>(message);
        return messageObj?.messageType == "DeleteItem" &&
               messageObj?.id == itemId;
    }
}
