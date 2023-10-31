using Microsoft.AspNetCore.Mvc;
using Play.Common.Entities;
using Play.Inventory.Service.Dtos;

namespace Play.Common.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<InventoryItem> repository;

    public ItemsController(IRepository<InventoryItem> repository)
    {
        this.repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest();
        }

        var items = (await repository.GetAllAsync(item => item.UserId == userId))
            .Select(item => item.AsDto());

        return Ok(items);
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
