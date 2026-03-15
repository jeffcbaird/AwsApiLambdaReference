using LambdaApiReference.Models;
using LambdaApiReference.Repositories;
using LambdaApiReference.Services;
using Microsoft.AspNetCore.Mvc;

namespace LambdaApiReference.Controllers;

/// <summary>
/// CRUD endpoints for <see cref="Item"/> resources.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ItemsController(IItemService service, ILogger<ItemsController> logger)
    : ControllerBase
{
    /// <summary>Returns all items ordered by creation date.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Item>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Fetching all items");
        var items = await service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Returns a single item by its ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Item), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var item = await service.GetByIdAsync(id);
        if (item is null)
        {
            logger.LogWarning("Item {ItemId} not found", id);
            return NotFound();
        }
        return Ok(item);
    }

    /// <summary>Creates a new item and returns it with its generated ID.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Item), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateItemRequest request)
    {
        var item = await service.CreateAsync(request);
        logger.LogInformation("Created item {ItemId}", item.Id);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    /// <summary>Replaces an existing item's fields.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Item), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateItemRequest request)
    {
        var item = await service.UpdateAsync(id, request);
        if (item is null)
        {
            logger.LogWarning("Item {ItemId} not found for update", id);
            return NotFound();
        }
        return Ok(item);
    }

    /// <summary>Deletes an item by its ID.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await service.DeleteAsync(id);
        if (!deleted)
        {
            logger.LogWarning("Item {ItemId} not found for deletion", id);
            return NotFound();
        }
        logger.LogInformation("Deleted item {ItemId}", id);
        return NoContent();
    }
}
