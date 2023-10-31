namespace Play.Inventory.Service.Dtos;

public record GrantItemsSto(
    Guid UserId,
    Guid CatalogItemId,
    int Quantity
);

public record InventoryItemDto(
    Guid CatalogItemId,
    string Name,
    string Description,
    int Quantity,
    DateTimeOffset AcquiredDate
);

public record CatalogItemDto(
    Guid Id,
    string Name,
    string Description
)
{ }
