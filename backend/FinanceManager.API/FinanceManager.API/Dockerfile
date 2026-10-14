# ==============================================================================
# PRODUCTION DOCKERFILE FOR FINANCE MANAGER API (.NET 10)
# Multi-stage, non-root user, hardened security, layer caching & health checks
# ==============================================================================

# Global argument for the .NET version (defaults to 10.0-preview, configurable via build args)
ARG DOTNET_VERSION=10.0-preview

# ------------------------------------------------------------------------------
# Stage 1: Base Runtime Image
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS base
WORKDIR /app

# Expose standard ASP.NET Core port
EXPOSE 8080

# Configure production environment defaults
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8

# Install curl for robust container health checks and clean package cache
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Healthcheck probe using the ASP.NET Core /health endpoint
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# ------------------------------------------------------------------------------
# Stage 2: Build Image (SDK)
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Step 1: Cache NuGet restore layer by copying only project definition first
COPY ["FinanceManager.API.csproj", "./"]
RUN dotnet restore "FinanceManager.API.csproj"

# Step 2: Copy remaining source code and build
COPY . .
RUN dotnet build "FinanceManager.API.csproj" \
    -c ${BUILD_CONFIGURATION} \
    --no-restore \
    -o /app/build

# ------------------------------------------------------------------------------
# Stage 3: Publish Image
# ------------------------------------------------------------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FinanceManager.API.csproj" \
    -c ${BUILD_CONFIGURATION} \
    --no-restore \
    -o /app/publish \
    /p:UseAppHost=false

# ------------------------------------------------------------------------------
# Stage 4: Production Runtime Image
# ------------------------------------------------------------------------------
FROM base AS final
WORKDIR /app

# Copy compiled binaries with non-root user ownership ($APP_UID is 1654 in .NET 8+)
COPY --from=publish --chown=$APP_UID:$APP_UID /app/publish .

# Enforce Principle of Least Privilege: Run as non-root user
USER $APP_UID

# Start the Web API application
ENTRYPOINT ["dotnet", "FinanceManager.API.dll"]
