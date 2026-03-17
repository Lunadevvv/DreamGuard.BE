using System;
using System.Collections.Generic;
using System.Linq;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Utilities
{
    public static class StockCalculator
    {
        public static int CalculateComboStock(List<ComboProductVariant> comboProductVariants)
        {
            if (comboProductVariants == null || !comboProductVariants.Any())
            {
                return 0;
            }

            int minStock = int.MaxValue;

            foreach (var cpv in comboProductVariants)
            {
                if (cpv.Quantity <= 0)
                {
                    continue;
                }

                int inventoryQuantity = cpv.ProductVariant?.Inventory?.Quantity ?? 0;
                int possibleSets = inventoryQuantity / cpv.Quantity;
                minStock = Math.Min(minStock, possibleSets);
            }

            return minStock == int.MaxValue ? 0 : minStock;
        }
    }
}
