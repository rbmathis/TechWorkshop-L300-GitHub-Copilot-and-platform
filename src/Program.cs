using ZavaStorefront.Services;
using ZavaStorefront.Features;
using ZavaStorefront;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.AppConfiguration.AspNetCore;
using Microsoft.FeatureManagement;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Connect to Azure App Configuration (optional, will use local settings if not available)
var appConfigConnection = builder.Configuration.GetConnectionString("AppConfig");
if (!string.IsNullOrEmpty(appConfigConnection))
{
    try
    {
        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            options.Connect(appConfigConnection)
                .ConfigureKeyVault(kv =>
                {
                    kv.SetCredential(new DefaultAzureCredential());
                })
                .UseFeatureFlags(featureFlagOptions =>
                {
                    featureFlagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(30);
                });
        });
    }
    catch (Exception ex)
    {
        // Log warning if App Configuration is not available
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("Startup");
        logger.LogWarning($"Failed to connect to App Configuration: {ex.Message}. Using local configuration.");
    }
}

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<ITelemetryInitializer, UserSessionTelemetryInitializer>();

// Add Feature Management
builder.Services.AddFeatureManagement();
builder.Services.AddScoped<IFeatureFlagService, FeatureFlagService>();

// Add distributed cache - use Redis if configured, otherwise use in-memory cache
var useRedisCache = builder.Configuration.GetValue<bool>("UseRedisCache");
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");

if (useRedisCache && !string.IsNullOrEmpty(redisConnectionString))
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

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register application services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITelemetryClient, ApplicationInsightsTelemetryClient>();
builder.Services.AddScoped<ZavaStorefront.Services.ISessionManager>(serviceProvider =>
{
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var session = httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("HttpContext session is not available");
    return new SessionManager(session);
});
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<CartService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
