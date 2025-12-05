using System.Collections.Generic;

namespace ZavaStorefront.Models
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public int ItemCount { get; set; }
        public decimal Total { get; set; }
        public decimal DiscountedTotal { get; set; }
        public decimal Savings { get; set; }
        public bool IsBulkDiscountEnabled { get; set; }
        public bool DiscountApplied => IsBulkDiscountEnabled && Savings > 0;

        // Discount threshold configuration
        public decimal FirstTierThreshold { get; set; }
        public int FirstTierDiscount { get; set; }
        public decimal SecondTierThreshold { get; set; }
        public int SecondTierDiscount { get; set; }
    }
}
