using Grand.Business.Catalog.Services.Directory;
using Grand.Business.Catalog.Services.Products;
using Grand.Business.Catalog.Services.Tax;
using Grand.Business.Checkout.Services.Shipping;
using Grand.Business.Common.Services.Directory;
using Grand.Business.Common.Services.Localization;
using Grand.Business.Core.Interfaces.Catalog.Categories;
using Grand.Business.Core.Interfaces.Catalog.Collections;
using Grand.Business.Core.Interfaces.Catalog.Directory;
using Grand.Business.Core.Interfaces.Catalog.Prices;
using Grand.Business.Core.Interfaces.Catalog.Products;
using Grand.Business.Core.Interfaces.Catalog.Tax;
using Grand.Business.Core.Interfaces.Checkout.Shipping;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Business.Core.Interfaces.Common.Seo;
using Grand.Business.Core.Interfaces.Common.Stores;
using Grand.Business.Core.Interfaces.Customers;
using Grand.Business.Core.Interfaces.Storage;
using Grand.Domain.Catalog;
using Grand.Domain.Directory;
using Grand.Domain.Tax;
using Grand.Infrastructure;
using Grand.Web.Common.Localization;
using Grand.Web.Vendor.Interfaces;
using Grand.Web.Vendor.Models.Catalog;
using Grand.Web.Vendor.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Leo.ReviewProductPublish.Services
{
    public class VendorProductViewModelService : ProductViewModelService, IProductViewModelService
    {
        private readonly IAuctionService _auctionService;
        private readonly ICategoryService _categoryService;
        private readonly ICollectionService _collectionService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDeliveryDateService _deliveryDateService;
        private readonly IInventoryManageService _inventoryManageService;
        private readonly ILanguageService _languageService;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly IOutOfStockSubscriptionService _outOfStockSubscriptionService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductCategoryService _productCategoryService;
        private readonly IProductCollectionService _productCollectionService;
        private readonly IProductLayoutService _productLayoutService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStockQuantityService _stockQuantityService;
        private readonly IStoreService _storeService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly TaxSettings _taxSettings;
        private readonly ITranslationService _translationService;
        private readonly IWarehouseService _warehouseService;
        private readonly IContextAccessor _contextAccessor;
        private readonly ISeNameService _seNameService;
        private readonly IEnumTranslationService _enumTranslationService;


        public VendorProductViewModelService(IProductService productService, IInventoryManageService inventoryManageService, IPictureService pictureService, 
            IProductAttributeService productAttributeService, ICurrencyService currencyService, IMeasureService measureService, IDateTimeService dateTimeService, 
            ICollectionService collectionService, IProductCollectionService productCollectionService, ICategoryService categoryService, 
            IProductCategoryService productCategoryService, ITranslationService translationService, IProductLayoutService productLayoutService, 
            ISpecificationAttributeService specificationAttributeService, IContextAccessor contextAccessor, IWarehouseService warehouseService, 
            IDeliveryDateService deliveryDateService, ITaxCategoryService taxCategoryService, ICustomerService customerService, IStoreService storeService, 
            IOutOfStockSubscriptionService outOfStockSubscriptionService, ILanguageService languageService, IProductAttributeFormatter productAttributeFormatter, 
            IStockQuantityService stockQuantityService, IAuctionService auctionService, IPriceFormatter priceFormatter, CurrencySettings currencySettings, 
            MeasureSettings measureSettings, TaxSettings taxSettings, ISeNameService seNameService, IEnumTranslationService enumTranslationService) : 
                base(productService, inventoryManageService, pictureService, productAttributeService, currencyService, measureService, dateTimeService, 
                    collectionService, productCollectionService, categoryService, productCategoryService, translationService, productLayoutService, 
                    specificationAttributeService, contextAccessor, warehouseService, deliveryDateService, taxCategoryService, customerService, storeService, 
                    outOfStockSubscriptionService, languageService, productAttributeFormatter, stockQuantityService, auctionService, priceFormatter, currencySettings, 
                    measureSettings, taxSettings, seNameService, enumTranslationService)
        {
            _productService = productService;
            _inventoryManageService = inventoryManageService;
            _pictureService = pictureService;
            _productAttributeService = productAttributeService;
            _currencyService = currencyService;
            _measureService = measureService;
            _dateTimeService = dateTimeService;
            _collectionService = collectionService;
            _productCollectionService = productCollectionService;
            _categoryService = categoryService;
            _productCategoryService = productCategoryService;
            _translationService = translationService;
            _productLayoutService = productLayoutService;
            _specificationAttributeService = specificationAttributeService;
            _contextAccessor = contextAccessor;
            _warehouseService = warehouseService;
            _deliveryDateService = deliveryDateService;
            _taxCategoryService = taxCategoryService;
            _customerService = customerService;
            _storeService = storeService;
            _outOfStockSubscriptionService = outOfStockSubscriptionService;
            _stockQuantityService = stockQuantityService;
            _languageService = languageService;
            _productAttributeFormatter = productAttributeFormatter;
            _auctionService = auctionService;
            _priceFormatter = priceFormatter;
            _currencySettings = currencySettings;
            _measureSettings = measureSettings;
            _taxSettings = taxSettings;
            _seNameService = seNameService;
            _enumTranslationService = enumTranslationService;
        }


        public override async Task PrepareProductModel(ProductModel model, Product product,
        bool setPredefinedValues)
        {

            ArgumentNullException.ThrowIfNull(model);
            model.PrimaryStoreCurrencyCode =
                (await _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode;
            model.BaseWeightIn = (await _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId))?.Name;
            model.BaseDimensionIn =
                (await _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId))?.Name;

            if (product != null)
            {
                //date
                model.CreatedOn = _dateTimeService.ConvertToUserTime(product.CreatedOnUtc, DateTimeKind.Utc);
                model.UpdatedOn = product.UpdatedOnUtc.HasValue
                    ? _dateTimeService.ConvertToUserTime(product.UpdatedOnUtc.Value, DateTimeKind.Utc)
                    : null;

                //parent grouped product
                var parentGroupedProduct = await _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct != null)
                {
                    model.AssociatedToProductId = product.ParentGroupedProductId;
                    model.AssociatedToProductName = parentGroupedProduct.Name;
                }

                //reservation
                model.CalendarModel.ProductId = product.Id;
                model.CalendarModel.Interval = product.Interval;
                model.CalendarModel.IntervalUnit = (int)product.IntervalUnitId;
                model.CalendarModel.IncBothDate = product.IncBothDate;

                model.AutoAddRequiredProducts = product.AutoAddRequiredProducts;
                //product attributes
                foreach (var productAttribute in await _productAttributeService.GetAllProductAttributes())
                    model.AvailableProductAttributes.Add(new SelectListItem {
                        Text = productAttribute.Name,
                        Value = productAttribute.Id
                    });
            }

            //copy product
            if (product != null)
            {
                model.CopyProductModel.Id = product.Id;
                model.CopyProductModel.Name = "Copy of " + product.Name;
                model.CopyProductModel.Published = false;
                model.CopyProductModel.CopyImages = true;
            }

            //layouts
            var layouts = await _productLayoutService.GetAllProductLayouts();
            foreach (var layout in layouts)
                model.AvailableProductLayouts.Add(new SelectListItem {
                    Text = layout.Name,
                    Value = layout.Id
                });

            //delivery dates
            model.AvailableDeliveryDates.Add(new SelectListItem {
                Text = _translationService.GetResource("Vendor.Catalog.Products.Fields.DeliveryDate.None"),
                Value = ""
            });
            var deliveryDates = await _deliveryDateService.GetAllDeliveryDates();
            foreach (var deliveryDate in deliveryDates)
                model.AvailableDeliveryDates.Add(new SelectListItem {
                    Text = deliveryDate.Name,
                    Value = deliveryDate.Id
                });

            //warehouses
            var warehouses = await _warehouseService.GetAllWarehouses();
            model.AvailableWarehouses.Add(new SelectListItem {
                Text = _translationService.GetResource("Vendor.Catalog.Products.Fields.Warehouse.None"),
                Value = ""
            });
            foreach (var warehouse in warehouses)
                model.AvailableWarehouses.Add(new SelectListItem {
                    Text = warehouse.Name,
                    Value = warehouse.Id
                });

            //multiple warehouses
            foreach (var warehouse in warehouses)
            {
                var pwiModel = new ProductModel.ProductWarehouseInventoryModel {
                    WarehouseId = warehouse.Id,
                    WarehouseName = warehouse.Name,
                    WarehouseCode = warehouse.Code
                };
                if (product != null)
                {
                    var pwi = product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == warehouse.Id);
                    if (pwi != null)
                    {
                        pwiModel.WarehouseUsed = true;
                        pwiModel.StockQuantity = pwi.StockQuantity;
                        pwiModel.ReservedQuantity = pwi.ReservedQuantity;
                    }
                }

                model.ProductWarehouseInventoryModels.Add(pwiModel);
            }

            //tax categories
            var taxCategories = await _taxCategoryService.GetAllTaxCategories();
            model.AvailableTaxCategories.Add(new SelectListItem {
                Text = _translationService.GetResource("Vendor.Configuration.Tax.Settings.TaxCategories.None"),
                Value = ""
            });
            foreach (var tc in taxCategories)
                model.AvailableTaxCategories.Add(new SelectListItem {
                    Text = tc.Name,
                    Value = tc.Id,
                    Selected = product != null && !setPredefinedValues && tc.Id == product.TaxCategoryId
                });

            //base-price units
            var measureWeights = await _measureService.GetAllMeasureWeights();
            foreach (var mw in measureWeights)
                model.AvailableBasepriceUnits.Add(new SelectListItem {
                    Text = mw.Name,
                    Value = mw.Id,
                    Selected = product != null && !setPredefinedValues && mw.Id == product.BasepriceUnitId
                });
            foreach (var mw in measureWeights)
                model.AvailableBasepriceBaseUnits.Add(new SelectListItem {
                    Text = mw.Name,
                    Value = mw.Id,
                    Selected = product != null && !setPredefinedValues && mw.Id == product.BasepriceBaseUnitId
                });

            //units
            var units = await _measureService.GetAllMeasureUnits();
            model.AvailableUnits.Add(new SelectListItem { Text = "---", Value = "" });
            foreach (var un in units)
                model.AvailableUnits.Add(new SelectListItem { Text = un.Name, Value = un.Id, Selected = product != null && un.Id == product.UnitId });

            //default values
            if (setPredefinedValues)
            {
                model.MaxEnteredPrice = 1000;
                model.RecurringCycleLength = 100;
                model.RecurringTotalCycles = 10;
                model.StockQuantity = 0;
                model.NotifyAdminForQuantityBelow = 1;
                model.OrderMinimumQuantity = 1;
                model.OrderMaximumQuantity = 10000;
                model.TaxCategoryId = _taxSettings.DefaultTaxCategoryId;
                model.IsShipEnabled = true;
                model.AllowCustomerReviews = true;
                model.Published = false;
                model.VisibleIndividually = true;
            }

        }


    }
}
