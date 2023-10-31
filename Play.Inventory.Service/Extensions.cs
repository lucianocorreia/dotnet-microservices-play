using Play.Common.Entities;
using Play.Inventory.Service.Dtos;

namespace Play.Common;

public static class Extensions
{
    public static InventoryItemDto AsDto(this InventoryItem entity)
    {
        return new InventoryItemDto(entity.CatalogItemId, entity.Quantity, entity.AcquiredDate);
    }
}
