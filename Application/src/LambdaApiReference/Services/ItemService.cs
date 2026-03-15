using System.Text.Json.Serialization;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using LambdaApiReference.Models;
using LambdaApiReference.Repositories;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LambdaApiReference.Services;

public interface IItemService
{
    Task<IEnumerable<Item>> GetAllAsync();
    Task<Item?> GetByIdAsync(string id);
    Task<Item> CreateAsync(CreateItemRequest request);
    Task<Item?> UpdateAsync(string id, UpdateItemRequest request);
    Task<bool> DeleteAsync(string id);
}

public class ItemService(IItemRepository itemRepository, IAmazonSimpleNotificationService snsService, IOptions<ReferenceOptions> options) : IItemService
{
    private readonly ReferenceOptions _referenceOptions = options.Value;
    public async Task<IEnumerable<Item>> GetAllAsync() => await itemRepository.GetAllAsync();

    public async Task<Item?> GetByIdAsync(string id) => await itemRepository.GetByIdAsync(id);

    public async Task<Item> CreateAsync(CreateItemRequest request)
    {
        Item retVal = await itemRepository.CreateAsync(request);
        await snsService.PublishAsync(new PublishRequest
        {
            TopicArn = _referenceOptions.ReferencePublishSnsTopic,
            Message = JsonConvert.SerializeObject(new
            {
                messageType = "CreateItem",
                id = retVal.Id,
                value = retVal
            })
        });
        return retVal;
    }

    public async Task<Item?> UpdateAsync(string id, UpdateItemRequest request)
    {
        Item? retVal = await itemRepository.UpdateAsync(id, request);
        if (retVal != null)
        {
            await snsService.PublishAsync(new PublishRequest
            {
                TopicArn = _referenceOptions.ReferencePublishSnsTopic,
                Message = JsonConvert.SerializeObject(new
                {
                    messageType = "UpdateItem",
                    id = retVal.Id,
                    value = retVal
                })
            });
        }
        return retVal;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        bool retVal = await itemRepository.DeleteAsync(id);
        if (retVal)
        {
            await snsService.PublishAsync(new PublishRequest
            {
                TopicArn = _referenceOptions.ReferencePublishSnsTopic,
                Message = JsonConvert.SerializeObject(new
                {
                    messageType = "DeleteItem",
                    id = id
                })
            });
        }
        return retVal;
    }
}
