# 🛒 Zava Storefront — L300 Tech Workshop

> **Cloud-Native E-Commerce Meets AI-Powered Developer Productivity**

Welcome to the **Zava Storefront** workshop — an immersive, hands-on deep dive where you'll transform a humble .NET storefront into a cloud-native powerhouse running on Azure, supercharged by GitHub Copilot, and fortified with enterprise-grade security and observability.

![Azure](https://img.shields.io/badge/Azure-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white)
![.NET](https://img.shields.io/badge/.NET_6-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![GitHub Copilot](https://img.shields.io/badge/GitHub_Copilot-000?style=for-the-badge&logo=github&logoColor=white)
![Bicep](https://img.shields.io/badge/Bicep-IaC-orange?style=for-the-badge)

---

## 🔥 What You'll Build

| Layer | Tech | Outcome |
|-------|------|---------|
| **App** | ASP.NET Core MVC, Docker | Containerised storefront with responsive UI |
| **Infra** | Bicep, Azure Developer CLI | One-command provisioning of App Service, ACR, AI Foundry & more |
| **CI/CD** | GitHub Actions | Automated build → push → deploy pipeline |
| **Security** | GitHub Advanced Security, Managed Identity | Secret scanning, dependency review, zero-credential deployments |
| **AI** | Azure AI Foundry, GitHub Copilot | Model governance, inline code suggestions, chat-driven development |
| **Observability** | Application Insights, Log Analytics | End-to-end tracing, adaptive sampling, smart alerting |

---

## 🚀 Quick Start

```bash
# Clone & open in dev container (batteries included)
gh repo clone rbmathis/TechWorkshop-L300-GitHub-Copilot-and-platform
code TechWorkshop-L300-GitHub-Copilot-and-platform

# Authenticate to Azure
az login && azd auth login

# Deploy everything — infrastructure + app — in one shot
azd up
```

Within minutes you'll have a fully operational Azure environment with:
- 🏗️ Resource Group, Log Analytics, Application Insights
- 📦 Azure Container Registry (RBAC-only, no admin keys)
- 🌐 Linux App Service running your containerised storefront
- 🤖 Azure AI Foundry workspace ready for model experimentation

---

## 📚 Workshop Modules

| # | Module | Focus |
|---|--------|-------|
| 01 | [Development Environment Setup](docs/01_development_environment_setup/01_development_environment_setup.md) | Dev containers, toolchain, Copilot activation |
| 02 | [Implement Infrastructure with Copilot](docs/02_implement_infrastructure_with_copilot/02_implement_infrastructure_with_copilot.md) | Bicep authoring with AI pair-programming |
| 03 | [GitHub Actions Pipeline](docs/03_github_actions_pipeline/03_github_actions_pipeline.md) | CI/CD from commit to cloud |
| 04 | [GitHub Advanced Security](docs/04_github_advanced_security/04_github_advanced_security.md) | Code scanning, secret detection, dependency review |
| 05 | [Integrate GitHub Copilot](docs/05_integrate_github_copilot_for_developer_productivity/05_integrate_github_copilot_for_developer_productivity.md) | Productivity tips, chat, inline suggestions |
| 06 | [AI Governance & Model Observability](docs/06_ai_governance_and_model_observability/06_ai_governance_and_model_observability.md) | Responsible AI, model tracking, drift detection |
| 07 | [Resource Cleanup](docs/07_resource_cleanup/07_resource_cleanup.md) | Tear down gracefully, avoid surprise bills |

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
