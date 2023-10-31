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
    private readonly CatalogClient catalogClient;

    public ItemsController(IRepository<InventoryItem> repository, CatalogClient catalogClient)
    {
        this.repository = repository;
        this.catalogClient = catalogClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest();
        }

        var catalogItems = await catalogClient.GetCatalogItemsAsync();
        var items = await repository.GetAllAsync(item => item.UserId == userId);

        var inventoryItemsDto = items.Select(inventoryItem =>
        {
            var catalogItem = catalogItems.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
            return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
        });

        return Ok(inventoryItemsDto);
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync(GrantItemsSto grantItemsSto)
    {
        // TODO: rename method GetByIdAsync to GetAsync on IRepository
        var inventoryItem = await repository.GetByIdAsync(
            item => item.UserId == grantItemsSto.UserId && item.CatalogItemId == grantItemsSto.CatalogItemId
        );

        if (inventoryItem == null)
        {
            inventoryItem = new InventoryItem
            {
                Id = Guid.NewGuid(),
                UserId = grantItemsSto.UserId,
                CatalogItemId = grantItemsSto.CatalogItemId,
                Quantity = grantItemsSto.Quantity,
                AcquiredDate = DateTimeOffset.UtcNow
            };

            await repository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += grantItemsSto.Quantity;
            await repository.UpdateAsync(inventoryItem);
        }

        return Ok();
    }
}
