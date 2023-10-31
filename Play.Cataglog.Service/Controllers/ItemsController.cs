using Microsoft.AspNetCore.Mvc;
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
    private static int requestCounter = 0;

    public ItemsController(IRepository<Item> itemsRepository)
    {
        this.itemsRepository = itemsRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
    {
        requestCounter++;
        Console.WriteLine($"Request {requestCounter}: Starting...");

        if (requestCounter <= 2)
        {
            Console.WriteLine($"Request {requestCounter}: Delaing...");
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        if (requestCounter <= 4)
        {
            Console.WriteLine($"Request {requestCounter}: 500 (Internal Server Error).");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        var items = (await itemsRepository.GetAllAsync())
            .Select(item => item.AsDto());

        Console.WriteLine($"Request {requestCounter}: 200 (Ok).");
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

        return NoContent();
    }

}
