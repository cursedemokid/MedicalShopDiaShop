namespace MedicalShopDiaShop.AppData
{
    /// <summary>
    /// Цены при оформлении поставки: справочная розница из каталога и оптовая закупка у поставщика.
    /// <see cref="Database.Product.Price"/> в БД — розничная цена за одну штуку (без деления на 10).
    /// </summary>
    public static class SupplyPricing
    {
        /// <summary>
        /// Доля от справочной розничной цены за единицу, по которой считается оптовая закупка (0–1).
        /// Например 0,70 = закупка на 30 % ниже справочной розницы за штуку.
        /// </summary>
        public const decimal WholesaleFromRetailFactor = 0.70m;

        /// <summary>Розничная цена за 1 шт. из каталога (как в БД).</summary>
        public static decimal RetailUnitPrice(decimal productPricePerUnitFromDb) =>
            productPricePerUnitFromDb;

        public static decimal WholesaleUnitPrice(decimal productPricePerUnitFromDb) =>
            RetailUnitPrice(productPricePerUnitFromDb) * WholesaleFromRetailFactor;

        /// <summary>Сумма строки поставки по опту (то, что уйдёт в SupplyProduct.TotalPrice).</summary>
        public static decimal WholesaleLineTotal(decimal productPricePerUnitFromDb, int quantity) =>
            WholesaleUnitPrice(productPricePerUnitFromDb) * quantity;

        /// <summary>Справочная сумма строки «как по розничной цене за штуку» (до скидки опта).</summary>
        public static decimal RetailReferenceLineTotal(decimal productPricePerUnitFromDb, int quantity) =>
            RetailUnitPrice(productPricePerUnitFromDb) * quantity;
    }
}
