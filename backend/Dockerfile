# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CarTracking.sln ./
COPY src/CarTracking.Domain/CarTracking.Domain.csproj             src/CarTracking.Domain/
COPY src/CarTracking.Application/CarTracking.Application.csproj   src/CarTracking.Application/
COPY src/CarTracking.Infrastructure/CarTracking.Infrastructure.csproj src/CarTracking.Infrastructure/
COPY src/CarTracking.API/CarTracking.API.csproj                   src/CarTracking.API/

# Restore in a separate layer so Docker caches it when only source changes
RUN dotnet restore

COPY src/ src/
RUN dotnet publish src/CarTracking.API/CarTracking.API.csproj \
    -c Release -o /app/publish --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CarTracking.API.dll"]
