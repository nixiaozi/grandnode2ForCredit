﻿using Grand.Business.Core.Interfaces.Catalog.Products;
using Grand.Domain.Catalog;
using Grand.Module.Api.Commands.Models.Catalog;
using MediatR;

namespace Grand.Module.Api.Commands.Handlers.Catalog;

public class UpdateProductStockCommandHandler : IRequestHandler<UpdateProductStockCommand, bool>
{
    private readonly IInventoryManageService _inventoryManageService;
    private readonly IOutOfStockSubscriptionService _outOfStockSubscriptionService;
    private readonly IProductService _productService;
    private readonly IStockQuantityService _stockQuantityService;

    public UpdateProductStockCommandHandler(
        IProductService productService,
        IInventoryManageService inventoryManageService,
        IStockQuantityService stockQuantityService,
        IOutOfStockSubscriptionService outOfStockSubscriptionService)
    {
        _productService = productService;
        _inventoryManageService = inventoryManageService;
        _stockQuantityService = stockQuantityService;
        _outOfStockSubscriptionService = outOfStockSubscriptionService;
    }

    public async Task<bool> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductById(request.Product.Id);
        if (product != null)
        {
            var prevStockQuantity = _stockQuantityService.GetTotalStockQuantity(product);
            var prevMultiWarehouseStock = product.ProductWarehouseInventory.Select(i => new ProductWarehouseInventory {
                WarehouseId = i.WarehouseId,
                StockQuantity = i.StockQuantity,
                ReservedQuantity = i.ReservedQuantity
            })
                .ToList();

            if (string.IsNullOrEmpty(request.WarehouseId))
            {
                product.StockQuantity = request.Stock;
                await _inventoryManageService.UpdateStockProduct(product, false);
            }
            else
            {
                if (product.UseMultipleWarehouses)
                {
                    var existingPwI =
                        product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == request.WarehouseId);
                    if (existingPwI != null)
                    {
                        existingPwI.StockQuantity = request.Stock;
                        await _productService.UpdateProductWarehouseInventory(existingPwI, product.Id);
                    }
                    else
                    {
                        var newPwI = new ProductWarehouseInventory {
                            WarehouseId = request.WarehouseId,
                            StockQuantity = request.Stock,
                            ReservedQuantity = 0
                        };
                        await _productService.InsertProductWarehouseInventory(newPwI, product.Id);
                    }

                    product.StockQuantity = product.ProductWarehouseInventory.Sum(x => x.StockQuantity);
                    product.ReservedQuantity = product.ProductWarehouseInventory.Sum(x => x.ReservedQuantity);
                    await _inventoryManageService.UpdateStockProduct(product, false);
                }
                else
                {
                    throw new ArgumentException(
                        "Product don't support multiple warehouses (warehouseId should be null or empty)");
                }
            }

            await OutOfStockNotifications(product, prevStockQuantity, prevMultiWarehouseStock);
        }

        return true;
    }

    protected async Task OutOfStockNotifications(Product product, int prevStockQuantity,
        List<ProductWarehouseInventory> prevMultiWarehouseStock)
    {
        if (product.ManageInventoryMethodId == ManageInventoryMethod.ManageStock &&
            product.BackorderModeId == BackorderMode.NoBackorders &&
            product.AllowOutOfStockSubscriptions &&
            _stockQuantityService.GetTotalStockQuantity(product) > 0 &&
            prevStockQuantity <= 0 && !product.UseMultipleWarehouses &&
            product.Published)
            await _outOfStockSubscriptionService.SendNotificationsToSubscribers(product, "");
        if (product.ManageInventoryMethodId == ManageInventoryMethod.ManageStock &&
            product.BackorderModeId == BackorderMode.NoBackorders &&
            product.AllowOutOfStockSubscriptions &&
            product.UseMultipleWarehouses &&
            product.Published)
        {
            foreach (var prevstock in prevMultiWarehouseStock)
                if (prevstock.StockQuantity - prevstock.ReservedQuantity <= 0)
                {
                    var actualStock =
                        product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == prevstock.WarehouseId);
                    if (actualStock != null)
                        if (actualStock.StockQuantity - actualStock.ReservedQuantity > 0)
                            await _outOfStockSubscriptionService.SendNotificationsToSubscribers(product,
                                prevstock.WarehouseId);
                }

            if (product.ProductWarehouseInventory.Sum(x => x.StockQuantity - x.ReservedQuantity) > 0)
                if (prevMultiWarehouseStock.Sum(x => x.StockQuantity - x.ReservedQuantity) <= 0)
                    await _outOfStockSubscriptionService.SendNotificationsToSubscribers(product, "");
        }
    }
}