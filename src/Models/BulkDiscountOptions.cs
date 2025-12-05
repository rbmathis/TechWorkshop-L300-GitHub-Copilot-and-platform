namespace ZavaStorefront.Models
{
    public class BulkDiscountOptions
    {
        public decimal ThresholdLow { get; set; } = 50m;
        public decimal RateLow { get; set; } = 0.05m; // 5%
        public decimal ThresholdHigh { get; set; } = 100m;
        public decimal RateHigh { get; set; } = 0.10m; // 10%
    }
}