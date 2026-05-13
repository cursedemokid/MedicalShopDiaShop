namespace MedicalShopDiaShop.AppData
{
    /// <summary>
    /// Цены при оформлении поставки: справочная розница из каталога и оптовая закупка у поставщика.
    /// В карточке товара <see cref="Database.Product.Price"/> в этом сценарии трактуется так же, как в ChooseProductsPage (деление на 10 для цены за 1 шт.).
    /// </summary>
    public static class SupplyPricing
    {
        /// <summary>
        /// Доля от справочной розничной цены за единицу, по которой считается оптовая закупка (0–1).
        /// Например 0,70 = закупка на 30 % ниже справочной розницы за штуку.
        /// Измените здесь, чтобы задать политику закупок для всех поставок.
        /// </summary>
        public const decimal WholesaleFromRetailFactor = 0.70m;

        public static decimal RetailUnitPrice(decimal productPriceAsInSupplyStep) =>
            productPriceAsInSupplyStep / 10m;

        public static decimal WholesaleUnitPrice(decimal productPriceAsInSupplyStep) =>
            RetailUnitPrice(productPriceAsInSupplyStep) * WholesaleFromRetailFactor;

        /// <summary>Сумма строки поставки по опту (то, что уйдёт в SupplyProduct.TotalPrice).</summary>
        public static decimal WholesaleLineTotal(decimal productPriceAsInSupplyStep, int quantity) =>
            WholesaleUnitPrice(productPriceAsInSupplyStep) * quantity;

        /// <summary>Справочная сумма строки «как по розничной цене за штуку» (до скидки опта).</summary>
        public static decimal RetailReferenceLineTotal(decimal productPriceAsInSupplyStep, int quantity) =>
            RetailUnitPrice(productPriceAsInSupplyStep) * quantity;
    }
}
