using Nexus.CustomerOrder.Api.Features.Catalog.ProductCreate;
using Nexus.CustomerOrder.Api.Features.Catalog.ProductDelete;
using Nexus.CustomerOrder.Api.Features.Catalog.ProductGet;
using Nexus.CustomerOrder.Api.Features.Catalog.ProductsGet;
using Nexus.CustomerOrder.Api.Features.Catalog.ProductUpdate;

namespace Nexus.CustomerOrder.Api.Features.Catalog;

public static class ProductsEndpoint
{
    public static void MapProducts(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapCreateProductEndpoint();
        group.MapGetProductEndpoint();
        group.MapGetProductsEndpoint();
        group.MapUpdateProductEndpoint();
        group.MapDeleteProductEndpoint();
    }
}