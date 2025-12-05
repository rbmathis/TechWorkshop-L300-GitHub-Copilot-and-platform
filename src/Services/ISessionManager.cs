namespace ZavaStorefront.Services
{
    /// <summary>
    /// Interface for session operations to enable mocking in tests.
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>
        /// Get a string value from session.
        /// </summary>
        string? GetString(string key);

        /// <summary>
        /// Set a string value in session.
        /// </summary>
        void SetString(string key, string value);

        /// <summary>
        /// Remove a value from session.
        /// </summary>
        void Remove(string key);
    }
}
