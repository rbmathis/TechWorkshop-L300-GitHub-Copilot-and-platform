# Contributing to Zava Storefront

We appreciate your interest in contributing! This document outlines our development process and guidelines to ensure a smooth collaboration.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Semantic Versioning](#semantic-versioning)
- [Commit Message Convention](#commit-message-convention)
- [Pull Request Process](#pull-request-process)
- [Development Workflow](#development-workflow)

## 📜 Code of Conduct

This project adheres to the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). By participating, you are expected to uphold this code.

## 🚀 Getting Started

1. **Fork the repository** and clone your fork locally
2. **Install prerequisites**: .NET 8 SDK, Docker (optional)
3. **Set up your development environment**:
   ```bash
   cd src
   dotnet restore
   dotnet build
   dotnet test
   ```
4. **Create a feature branch** from `main`:
   ```bash
   git checkout -b feat/your-feature-name
   ```

## 📦 Semantic Versioning

This project follows **[Semantic Versioning 2.0.0](https://semver.org/)** (SemVer):

- **MAJOR** version (X.0.0): Incompatible API changes or breaking changes
- **MINOR** version (0.X.0): New features that are backward-compatible
- **PATCH** version (0.0.X): Backward-compatible bug fixes

### Automatic Version Bumping

Our CI/CD pipeline **automatically determines** the version bump based on your commit messages following the [Conventional Commits](https://www.conventionalcommits.org/) specification. You don't need to manually update version numbers!

## ✍️ Commit Message Convention

We use **Conventional Commits** to enable automatic semantic versioning. Each commit message should follow this format:

```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

### Commit Types

| Type       | Version Bump | Description                                    |
|------------|--------------|------------------------------------------------|
| `feat`     | **MINOR**    | A new feature                                  |
| `fix`      | **PATCH**    | A bug fix                                      |
| `docs`     | **PATCH**    | Documentation only changes                     |
| `style`    | **PATCH**    | Code style changes (formatting, missing semi-colons, etc.) |
| `refactor` | **PATCH**    | Code change that neither fixes a bug nor adds a feature |
| `perf`     | **PATCH**    | Performance improvements                       |
| `test`     | **PATCH**    | Adding or updating tests                       |
| `build`    | **PATCH**    | Changes to build system or dependencies        |
| `ci`       | **PATCH**    | Changes to CI configuration files              |
| `chore`    | **PATCH**    | Other changes that don't modify src or test files |
| `revert`   | **PATCH**    | Reverts a previous commit                      |

### Breaking Changes

To trigger a **MAJOR** version bump, use one of these methods:

1. **Add `!` after the type/scope**:
   ```
   feat!: remove deprecated authentication API
   ```

2. **Include `BREAKING CHANGE:` in the footer**:
   ```
   feat: update authentication flow
   
   BREAKING CHANGE: The old authentication method is no longer supported.
   Users must migrate to the new OAuth2 flow.
   ```

### Examples

**Minor version bump (new feature):**
```
feat(cart): add bulk discount calculation

Implement tiered discount system that applies discounts based on cart total.
- 5% off when total exceeds $50
- 10% off when total exceeds $100
```

**Patch version bump (bug fix):**
```
fix(session): resolve Redis connection timeout issue

Update connection retry logic to handle transient failures gracefully.

Closes #42
```

**Patch version bump (documentation):**
```
docs: update deployment instructions for Azure

Add detailed steps for configuring App Service container settings.
```

**Major version bump (breaking change):**
```
feat!: migrate to .NET 8 and remove .NET 6 support

BREAKING CHANGE: This release drops support for .NET 6. 
Applications must be updated to target .NET 8 or higher.
```

## 🔄 Pull Request Process

1. **Ensure all tests pass** locally before submitting
2. **Write descriptive commit messages** following the Conventional Commits format
3. **Update documentation** if you're changing functionality
4. **Create a Pull Request** with a clear title and description
5. **The CI pipeline will**:
   - Automatically analyze your commits
   - Determine the appropriate version bump (major/minor/patch)
   - Update the version in `src/ZavaStorefront.csproj`
   - Run tests and build checks
6. **Address review feedback** if requested
7. **Once approved and merged**:
   - The version will be automatically tagged (e.g., `v1.2.3`)
   - The application will be deployed to Azure
   - A GitHub release will be created

### PR Title Convention

Your PR title should also follow Conventional Commits format:

- `feat: add new shopping cart feature`
- `fix: resolve checkout page crash`
- `docs: update README with new setup instructions`

## 🛠️ Development Workflow

### Running Locally

```bash
cd src
dotnet run
# Navigate to https://localhost:5001
```

### Running Tests

```bash
cd src
dotnet test --configuration Release
```

### Building Container Image

```bash
docker build -t zava-storefront ./src
docker run -p 8080:80 zava-storefront
```

### Code Style

- Follow C# coding conventions and .NET best practices
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and single-responsibility

## 📊 Versioning in Action

Here's how automatic versioning works:

1. **You commit** changes with conventional commit messages:
   ```bash
   git commit -m "feat: add product search functionality"
   git commit -m "fix: resolve cart quantity validation"
   ```

2. **You create a PR** from your feature branch to `main`

3. **CI analyzes commits** in your PR:
   - Detects `feat:` → Determines MINOR bump needed
   - Current version: `1.2.3` → New version: `1.3.0`

4. **CI updates version** in `ZavaStorefront.csproj` and commits to your PR branch

5. **After merge to main**:
   - Git tag `v1.3.0` is created automatically
   - Docker image is tagged with `v1.3.0`, `latest`, and commit SHA
   - Application is deployed to Azure

## ❓ Questions?

If you have questions or need clarification on the contribution process, please:

- Open an issue in the repository
- Contact the maintainers
- Check our [README](README.md) for project documentation

---

**Thank you for contributing!** 🎉
