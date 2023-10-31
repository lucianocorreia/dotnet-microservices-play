using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Common;

public static class Extensions
{
    public static InventoryItemDto AsDto(this InventoryItem entity, string name, string description)
    {
        return new InventoryItemDto(
            entity.CatalogItemId,
            name,
            description,
            entity.Quantity,
            entity.AcquiredDate);
    }
}
