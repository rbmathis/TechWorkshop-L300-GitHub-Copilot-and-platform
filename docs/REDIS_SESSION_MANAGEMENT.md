# Redis Session Management

## Overview

This document describes the Redis session management implementation in the ZavaStorefront application. Redis provides distributed session storage, enabling horizontal scaling and session persistence across multiple application instances.

## Architecture

### Components

1. **StackExchange.Redis** - NuGet package for Redis client support
2. **Microsoft.Extensions.Caching.StackExchangeRedis** - Integration with ASP.NET Core's distributed caching
3. **IDistributedCache** - Service abstraction for session storage

### Session Flow

```
User Request
    ↓
Session Middleware
    ↓
Redis Store (or in-memory fallback)
    ↓
SessionManager (ISession wrapper)
    ↓
CartService / Application Code
```

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "UseRedisCache": false,
  "UseRedisSessionStore": false,
  "Session": {
    "IdleTimeoutMinutes": 30,
    "CookieName": ".ZavaStorefront.Session",
    "CookieHttpOnly": true,
    "CookieIsEssential": true
  }
}
```

**Configuration Options:**

- `ConnectionStrings:Redis` - Redis connection string (host:port or Azure Redis connection string)
- `UseRedisCache` - Enable Redis for distributed caching (product cache, etc.)
- `UseRedisSessionStore` - Enable Redis for session storage
- `Session:IdleTimeoutMinutes` - Session idle timeout in minutes (default: 30)
- `Session:CookieName` - Session cookie name (default: ".ZavaStorefront.Session")
- `Session:CookieHttpOnly` - Whether cookie is HTTP-only (default: true for security)
- `Session:CookieIsEssential` - Whether cookie is essential for site functionality (default: true)

### Program.cs Setup

```csharp
var useRedisCache = builder.Configuration.GetValue<bool>("UseRedisCache");
var useRedisSessionStore = builder.Configuration.GetValue<bool>("UseRedisSessionStore");
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");

// Configure distributed cache (Redis or in-memory)
if ((useRedisCache || useRedisSessionStore) && !string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "ZavaStorefront:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// Add session support with Redis or in-memory store
var sessionIdleTimeoutMinutes = builder.Configuration.GetValue<int>("Session:IdleTimeoutMinutes", 30);
var sessionCookieName = builder.Configuration.GetValue<string>("Session:CookieName", ".AspNetCore.Session");
var sessionCookieHttpOnly = builder.Configuration.GetValue<bool>("Session:CookieHttpOnly", true);
var sessionCookieIsEssential = builder.Configuration.GetValue<bool>("Session:CookieIsEssential", true);

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(sessionIdleTimeoutMinutes);
    options.Cookie.Name = sessionCookieName;
    options.Cookie.HttpOnly = sessionCookieHttpOnly;
    options.Cookie.IsEssential = sessionCookieIsEssential;
});
```

## Enabling Redis Session Management

### For Development (Local Redis)

1. **Start Redis locally** (using Docker):

   ```bash
   docker run -d -p 6379:6379 redis:latest
   ```

2. **Update appsettings.json**:

   ```json
   {
     "ConnectionStrings": {
       "Redis": "localhost:6379"
     },
     "UseRedisSessionStore": true
   }
   ```

3. **Run the application**:
   ```bash
   dotnet run
   ```

### For Production (Azure Redis Cache)

1. **Create Azure Redis Cache** via Azure Portal or Bicep

2. **Update appsettings.json or User Secrets**:

   ```json
   {
     "ConnectionStrings": {
       "Redis": "your-redis-name.redis.cache.windows.net:6379,password=your-access-key,ssl=True"
     },
     "UseRedisSessionStore": true
   }
   ```

3. **Deploy and run**

## SessionManager Integration

The `SessionManager` class wraps ASP.NET Core's `ISession` interface:

```csharp
public class SessionManager : ISessionManager
{
    private readonly ISession _session;

    public SessionManager(ISession session)
    {
        _session = session;
    }

    public void SetString(string key, string value)
    {
        _session.SetString(key, value);
    }

    public string? GetString(string key)
    {
        _session.TryGetValue(key, out byte[]? value);
        return value == null ? null : Encoding.UTF8.GetString(value);
    }
}
```

This abstraction:

- Works seamlessly with Redis or in-memory session stores
- Provides string-based session operations for shopping cart data
- Encapsulates session state management

## Usage in CartService

```csharp
public List<CartItem> GetCart()
{
    var cartJson = _sessionManager.GetString("ShoppingCart");

    if (string.IsNullOrEmpty(cartJson))
        return new List<CartItem>();

    return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
}

public void SaveCart(List<CartItem> cart)
{
    var cartJson = JsonSerializer.Serialize(cart);
    _sessionManager.SetString("ShoppingCart", cartJson);
}
```

## Benefits

1. **Horizontal Scaling** - Multiple application instances can share session data
2. **Session Persistence** - Sessions survive application restarts
3. **High Performance** - Redis provides fast in-memory access
4. **Distributed Deployment** - Container orchestration (Kubernetes, Docker Swarm) friendly
5. **Flexible** - Easy toggle between Redis and in-memory storage via configuration

## Security Considerations

1. **HTTPOnly Cookies** - Enabled by default to prevent XSS attacks
2. **SSL/TLS for Redis** - Use encrypted connections in production
3. **Redis Authentication** - Configure strong passwords in Azure Redis
4. **Key Expiration** - Sessions automatically expire after idle timeout
5. **Data Encryption** - Consider encryption for sensitive session data

## Troubleshooting

### Sessions Not Persisting

1. Verify Redis connection string in appsettings.json
2. Check if `UseRedisSessionStore` is set to `true`
3. Verify Redis service is running: `redis-cli ping` (should return "PONG")
4. Check application logs for connection errors

### Performance Issues

1. Monitor Redis memory usage
2. Verify network latency to Redis
3. Consider session data size - large serialized objects slow down transfers
4. Implement session cleanup policies for inactive users

### Azure Redis Connection

1. Enable SSL/TLS: `ssl=True` in connection string
2. Use Azure Managed Identity when possible
3. Whitelist app service IP in Redis firewall rules
4. Verify Redis Premium tier for high availability

## Testing

Redis session management is tested in:

- `CartServiceTests.cs` - Session storage and retrieval
- `HomeControllerTests.cs` - Session management in controllers
- Integration tests (to be added for full session lifecycle)

**Note:** Current tests use in-memory session by default. Enable Redis-backed tests by:

1. Setting `UseRedisSessionStore: true` in test configuration
2. Starting Redis before running tests

## Next Steps

1. Configure Redis in appsettings for your environment
2. Set `UseRedisSessionStore: true` to enable Redis session storage
3. Deploy Redis infrastructure (local development or Azure production)
4. Monitor session performance and adjust timeout as needed
