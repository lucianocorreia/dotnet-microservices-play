using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;

namespace Play.Catalog.Service.Extensions;

public static class Extenseions
{
    public static ItemDto AsDto(this Item item)
        => new ItemDto(item.Id, item.Name, item.Description, item.Price, item.CreatedDate);

}
