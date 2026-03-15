using LambdaApiReference.Models;

namespace LambdaApiReference.Repositories;

public interface IItemRepository
{
    Task<IEnumerable<Item>> GetAllAsync();
    Task<Item?> GetByIdAsync(string id);
    Task<Item> CreateAsync(CreateItemRequest request);
    Task<Item?> UpdateAsync(string id, UpdateItemRequest request);
    Task<bool> DeleteAsync(string id);
}
