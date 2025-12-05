namespace ZavaStorefront.Features
{
    /// <summary>
    /// Feature flags for the ZavaStorefront application
    /// </summary>
    public static class FeatureFlags
    {
        /// <summary>
        /// Enables the new checkout experience
        /// </summary>
        public const string NewCheckout = "NewCheckout";

        /// <summary>
        /// Enables the recommendation engine feature
        /// </summary>
        public const string RecommendationEngine = "RecommendationEngine";

        /// <summary>
        /// Enables advanced analytics tracking
        /// </summary>
        public const string AdvancedAnalytics = "AdvancedAnalytics";

        /// <summary>
        /// Enables early access to new products
        /// </summary>
        public const string EarlyAccessProducts = "EarlyAccessProducts";

        /// <summary>
        /// Enables bulk discount functionality
        /// </summary>
        public const string BulkDiscounts = "BulkDiscounts";
    }
}
