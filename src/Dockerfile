# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["ZavaStorefront.csproj", "./"]
RUN dotnet restore "ZavaStorefront.csproj"

# Copy remaining source code
COPY . .

# Build the application
RUN dotnet build "ZavaStorefront.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ZavaStorefront.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

# Create a non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser -d /home/appuser -m appuser

# Create ASP.NET Core Data Protection keys directory
RUN mkdir -p /home/appuser/.aspnet/DataProtection-Keys && \
    chown -R appuser:appuser /home/appuser

# Copy published application
COPY --from=publish /app/publish .

# Set ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port 8080 (App Service expects this port)
EXPOSE 8080

# Set environment variable for ASP.NET Core to listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080

# Start the application
ENTRYPOINT ["dotnet", "ZavaStorefront.dll"]
