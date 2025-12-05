using Microsoft.AspNetCore.Http;

namespace ZavaStorefront.Services
{
    /// <summary>
    /// Implementation of ISessionManager using AspNetCore session.
    /// </summary>
    public class SessionManager : ISessionManager
    {
        private readonly ISession _session;

        public SessionManager(ISession session)
        {
            _session = session;
        }

        public string? GetString(string key)
        {
            _session.TryGetValue(key, out byte[]? value);
            return value == null ? null : System.Text.Encoding.UTF8.GetString(value);
        }

        public void SetString(string key, string value)
        {
            _session.SetString(key, value);
        }

        public void Remove(string key)
        {
            _session.Remove(key);
        }

        public string GetSessionId()
        {
            return _session.Id;
        }
    }
}
