# Zava Storefront — Documentation

## 1. Project Overview

- A lightweight ASP.NET Core MVC storefront that lists products, lets shoppers manage a cart, and performs a simple checkout flow. Session-based cart state with optional Redis backing; no database is required.
- Tech stack: ASP.NET Core MVC on .NET 6, Bootstrap 5, Application Insights telemetry, optional Azure Cache for Redis, Azure App Configuration + Feature Management, Docker.
- Architecture: classic MVC with dependency injection; services abstract telemetry, products, cart, sessions, and feature flags.
- **New**: Bulk discount support with tiered pricing rules applied via feature flags.

## 2. Prerequisites & Setup

- SDKs/Tools: .NET 6 SDK, Docker (optional for container run), Azure CLI + Azure Developer CLI for cloud deployment.
- Local run:
  ```bash
  git clone https://github.com/rbmathis/TechWorkshop-L300-GitHub-Copilot-and-platform.git
  cd TechWorkshop-L300-GitHub-Copilot-and-platform/src
  dotnet restore
  dotnet run
  ```
- appsettings secrets/example (`src/appsettings.json`):
  ```json
  {
    "ApplicationInsights": {
      "ConnectionString": "InstrumentationKey=..."
    },
    "ConnectionStrings": {
      "Redis": "<redis-connection-string-if-used>"
    },
    "UseRedisCache": true,
    "UseRedisSessionStore": true,
    "Session": {
      "IdleTimeoutMinutes": 30,
      "CookieName": ".ZavaStorefront.Session"
    }
  }
  ```
  Use user-secrets or environment variables to keep real keys out of source control. Azure App Configuration is optional; when configured, feature flags and Key Vault integration are used.

## 3. Folder Structure & Conventions

```
.
├── src/                    # Application code
│   ├── Controllers/        # MVC controllers
│   ├── Features/           # Feature flag constants + service
│   ├── Models/             # View models/domain models (Product, CartItem, ErrorViewModel)
│   ├── Services/           # Cart, products, session, telemetry abstractions
│   ├── Views/              # Razor views
│   ├── wwwroot/            # Static assets
│   ├── Program.cs          # DI setup, middleware pipeline
│   ├── Dockerfile          # Container image build
│   └── ZavaStorefront.Tests/ # xUnit test suite
└── infra/                  # Bicep IaC + azd config
```

## 4. Key Architectural Decisions

- Dependency Injection configured in `Program.cs` for controllers, services, telemetry, feature flags, distributed cache, and sessions.
- Telemetry: `ApplicationInsightsTelemetryClient` wraps `TelemetryClient`; `UserSessionTelemetryInitializer` adds session context.
- Feature flags: `FeatureFlagService` over `Microsoft.FeatureManagement`, optionally backed by Azure App Configuration.
- Caching/session: switches between Redis (if connection provided and flags enabled) and in-memory cache; sessions stored in distributed cache when Redis is on.
- Data: products served from an in-memory list cached via `IDistributedCache`; no database/EF Core is used.

## 5. Areas / Modules

- None defined; single MVC area.

## 6. Controllers & Actions Summary

| Area | Controller     | HTTP Verb | Route                   | Action Method   | Purpose                          | Auth Required |
| ---- | -------------- | --------- | ----------------------- | --------------- | -------------------------------- | ------------- |
| —    | HomeController | GET       | `/`                     | Index           | Show product catalog             | No            |
| —    | HomeController | POST      | `/Home/AddToCart`       | AddToCart       | Add product to cart              | No            |
| —    | HomeController | GET       | `/Home/Privacy`         | Privacy         | Static privacy page              | No            |
| —    | HomeController | GET       | `/Home/Error`           | Error           | Error page                       | No            |
| —    | CartController | GET       | `/Cart`                 | Index           | View cart                        | No            |
| —    | CartController | POST      | `/Cart/UpdateQuantity`  | UpdateQuantity  | Update item quantity             | No            |
| —    | CartController | POST      | `/Cart/RemoveFromCart`  | RemoveFromCart  | Remove item                      | No            |
| —    | CartController | POST      | `/Cart/Checkout`        | Checkout        | Clear cart and simulate checkout | No            |
| —    | CartController | GET       | `/Cart/CheckoutSuccess` | CheckoutSuccess | Confirmation page                | No            |

## 7. Important ViewModels & DTOs

- `Product`: Id, Name, Description, Price, ImageUrl — displayed in catalog and cart.
- `CartItem`: Product, Quantity — used for session cart storage.
- `ErrorViewModel`: RequestId, ShowRequestId — used by error view.

## 8. Services & Business Logic

- `IProductService` / `ProductService`: returns in-memory product list, cached via `IDistributedCache` with optional telemetry events.
- `CartService`: session-backed cart operations (add/update/remove/total), integrates telemetry, feature flags, and session abstraction. Includes `GetCartTotalWithDiscountAsync()` method for applying bulk discounts.
- `ISessionManager` / `SessionManager`: thin wrapper over `ISession` for testability.
- `ITelemetryClient` / `ApplicationInsightsTelemetryClient`: wraps Application Insights client for DI/testing.
- `IFeatureFlagService` / `FeatureFlagService`: helpers to check and execute behavior behind feature flags.
- `BulkDiscountOptions`: configuration class that defines tiered discount thresholds and rates (low tier: $50 @ 5%, high tier: $100 @ 10%).

## 9. Database & Entity Framework

- Not used. Products are static; cart state is stored in session (in-memory or Redis). No `DbContext` or migrations present.

## 9a. Feature Flags

- **BulkDiscounts**: Enables tiered discount calculation on cart totals. When enabled, `CartService.GetCartTotalWithDiscountAsync()` applies discounts based on configured thresholds and rates. Telemetry tracks original and discounted amounts.
- **NewCheckout**: Reserved for future checkout experience upgrades.
- **RecommendationEngine**: Reserved for product recommendation features.
- **AdvancedAnalytics**: Reserved for enhanced analytics tracking.
- **EarlyAccessProducts**: Reserved for early access programs.

## 10. Authentication & Authorization

- None implemented. All routes are anonymous.

## 11. API Endpoints

- None; this is an MVC app with server-rendered views.

## 12. Common Filters, Tag Helpers, Middleware

- Middleware pipeline: HTTPS redirection (prod), static files, routing, session, authorization.
- Feature Management and Azure App Configuration integration (optional) configured during startup.
- Telemetry initializer `UserSessionTelemetryInitializer` adds session context to Application Insights telemetry.

## 13. Testing Strategy

- xUnit test project targeting net8.0 with Moq and coverlet. Tests cover controllers, services, and models under `src/ZavaStorefront.Tests`.
- Run tests from `src`:
  ```bash
  dotnet test
  ```

## 14. Deployment Notes

- Container-friendly via `src/Dockerfile`.
- Azure deployment via Azure Developer CLI + Bicep (`infra/`); provision App Service (container), ACR, App Insights, Log Analytics, optional Redis, and managed identity.
- One-command provision + deploy:
  ```bash
  az login && azd auth login
  azd up
  ```

---

## 🗂️ Repository Structure

```
├── src/                  # ASP.NET Core MVC storefront
│   ├── Controllers/      # Home & Cart controllers
│   ├── Services/         # Product & Cart services
│   ├── Views/            # Razor views + layout
│   └── Dockerfile        # Multi-stage container build
├── infra/                # Bicep IaC modules
│   ├── main.bicep        # Orchestrator
│   └── modules/          # ACR, App Service, AI Foundry, etc.
├── .github/
│   ├── workflows/        # GitHub Actions CI/CD
│   └── prompts/          # Custom Copilot Chat prompts
├── docs/                 # Step-by-step workshop guides
└── azure.yaml            # Azure Developer CLI manifest
```

---

## 🧪 Run Locally

```bash
cd src
dotnet run
# Navigate to https://localhost:5001
```

Or spin up the container:

```bash
docker build -t zava-storefront ./src
docker run -p 8080:80 zava-storefront
```

---

## 🤝 Contributing

Contributions are welcome! Please review the [Code of Conduct](https://opensource.microsoft.com/codeofconduct/) before submitting a PR.

1. Fork the repo
2. Create a feature branch (`git checkout -b feat/awesome-thing`)
3. Commit your changes with a clear message
4. Open a Pull Request — the CLA bot will guide you through licensing

---

## ⚖️ Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/legal/intellectualproperty/trademarks/usage/general). Third-party trademarks are subject to their respective policies.

---

<p align="center">
  <strong>Built with ☕ and Copilot</strong><br/>
  <sub>Now go ship something awesome.</sub>
</p>
