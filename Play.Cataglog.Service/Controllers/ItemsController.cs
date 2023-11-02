using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Extensions;
using Play.Common;

namespace Play.Catalog.Service.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<Item> itemsRepository;
    private readonly IPublishEndpoint publishEndpoint;

    public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint)
    {
        this.itemsRepository = itemsRepository;
        this.publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
    {
        var items = (await itemsRepository.GetAllAsync())
            .Select(item => item.AsDto());

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
    {
        var item = await itemsRepository.GetByIdAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        return item.AsDto();
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> Post(CreateItemDto createItemDto)
    {
        var item = new Item
        {
            Name = createItemDto.Name,
            Description = createItemDto.Description,
            Price = createItemDto.Price,
            CreatedDate = DateTimeOffset.UtcNow
        };

        await itemsRepository.CreateAsync(item);

        await publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

        return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
    {
        var item = await itemsRepository.GetByIdAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        item.Name = updateItemDto.Name;
        item.Description = updateItemDto.Description;
        item.Price = updateItemDto.Price;

        await itemsRepository.UpdateAsync(item);

        await publishEndpoint.Publish(new CatalogItemUpdated(item.Id, item.Name, item.Description));

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var item = await itemsRepository.GetByIdAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        await itemsRepository.DeleteAsync(id);

        await publishEndpoint.Publish(new CatalogItemDeleted(item.Id));


        return NoContent();
    }

}
