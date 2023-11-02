using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<InventoryItem> repository;
    private readonly IRepository<CatalogItem> catalogItemRepository;

    public ItemsController(IRepository<InventoryItem> repository, IRepository<CatalogItem> catalogItemRepository)
    {
        this.repository = repository;
        this.catalogItemRepository = catalogItemRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest();
        }

        var items = await repository.GetAllAsync(item => item.UserId == userId);
        var ids = items.Select(item => item.CatalogItemId);
        var catalogItems = await catalogItemRepository.GetAllAsync(item => ids.Contains(item.Id));

        var inventoryItemsDto = items.Select(inventoryItem =>
        {
            var catalogItem = catalogItems.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
            return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
        });

        return Ok(inventoryItemsDto);
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync([FromBody] GrantItemsDto grantItemsDto)
    {
        Console.WriteLine($"--> Granting items to user {grantItemsDto.UserId}");

        // TODO: rename method GetByIdAsync to GetAsync on IRepository
        var inventoryItem = await repository.GetByIdAsync(
            item => item.UserId == grantItemsDto.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId
        );

        if (inventoryItem == null)
        {
            inventoryItem = new InventoryItem
            {
                Id = Guid.NewGuid(),
                UserId = grantItemsDto.UserId,
                CatalogItemId = grantItemsDto.CatalogItemId,
                Quantity = grantItemsDto.Quantity,
                AcquiredDate = DateTimeOffset.UtcNow
            };

            await repository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += grantItemsDto.Quantity;
            await repository.UpdateAsync(inventoryItem);
        }

        return Ok();
    }
}
