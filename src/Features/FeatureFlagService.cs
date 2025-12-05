using Microsoft.FeatureManagement;

namespace ZavaStorefront.Features
{
    /// <summary>
    /// Interface for feature flag management
    /// </summary>
    public interface IFeatureFlagService
    {
        /// <summary>
        /// Check if a feature flag is enabled
        /// </summary>
        Task<bool> IsFeatureEnabledAsync(string featureName);

        /// <summary>
        /// Execute a function if feature is enabled, otherwise return default value
        /// </summary>
        Task<T> ExecuteIfEnabledAsync<T>(string featureName, Func<Task<T>> featureAction, T defaultValue);

        /// <summary>
        /// Execute an action if feature is enabled
        /// </summary>
        Task ExecuteIfEnabledAsync(string featureName, Func<Task> featureAction);
    }

    /// <summary>
    /// Implementation of feature flag management using Microsoft.FeatureManagement
    /// </summary>
    public class FeatureFlagService : IFeatureFlagService
    {
        private readonly IFeatureManager _featureManager;
        private readonly ILogger<FeatureFlagService> _logger;

        public FeatureFlagService(IFeatureManager featureManager, ILogger<FeatureFlagService> logger)
        {
            _featureManager = featureManager;
            _logger = logger;
        }

        public async Task<bool> IsFeatureEnabledAsync(string featureName)
        {
            try
            {
                var isEnabled = await _featureManager.IsEnabledAsync(featureName);
                _logger.LogInformation($"Feature flag '{featureName}' is {(isEnabled ? "enabled" : "disabled")}");
                return isEnabled;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking feature flag '{featureName}'");
                return false;
            }
        }

        public async Task<T> ExecuteIfEnabledAsync<T>(string featureName, Func<Task<T>> featureAction, T defaultValue)
        {
            try
            {
                if (await _featureManager.IsEnabledAsync(featureName))
                {
                    _logger.LogInformation($"Executing feature '{featureName}'");
                    return await featureAction();
                }

                _logger.LogInformation($"Feature '{featureName}' is disabled, returning default value");
                return defaultValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing feature '{featureName}'");
                return defaultValue;
            }
        }

        public async Task ExecuteIfEnabledAsync(string featureName, Func<Task> featureAction)
        {
            try
            {
                if (await _featureManager.IsEnabledAsync(featureName))
                {
                    _logger.LogInformation($"Executing feature '{featureName}'");
                    await featureAction();
                }
                else
                {
                    _logger.LogInformation($"Feature '{featureName}' is disabled");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing feature '{featureName}'");
            }
        }
    }
}
