# Imagen de la API para producción (Render). Se construye desde la carpeta Back/:
#   docker build -t parcelflow-api .
#
# Multi-stage: la etapa "build" tiene el SDK completo (~800 MB) para compilar;
# la imagen final solo lleva el runtime de ASP.NET y la app publicada (~250 MB).

FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src

# 1) Primero solo los .csproj y restore: Docker cachea esta capa y, si no cambian
#    las dependencias, no vuelve a descargar paquetes NuGet en cada build.
#    (Si añades un proyecto nuevo a src/, añade aquí su línea o el restore fallará.)
COPY src/ParcelFlow.BuildingBlocks.Domain/*.csproj src/ParcelFlow.BuildingBlocks.Domain/
COPY src/ParcelFlow.BuildingBlocks.Application/*.csproj src/ParcelFlow.BuildingBlocks.Application/
COPY src/ParcelFlow.BuildingBlocks.Infrastructure/*.csproj src/ParcelFlow.BuildingBlocks.Infrastructure/
COPY src/ParcelFlow.Modules.Shipments.Domain/*.csproj src/ParcelFlow.Modules.Shipments.Domain/
COPY src/ParcelFlow.Modules.Shipments.Application/*.csproj src/ParcelFlow.Modules.Shipments.Application/
COPY src/ParcelFlow.Modules.Shipments.Infrastructure/*.csproj src/ParcelFlow.Modules.Shipments.Infrastructure/
COPY src/ParcelFlow.Modules.Shipments.IntegrationEvents/*.csproj src/ParcelFlow.Modules.Shipments.IntegrationEvents/
COPY src/ParcelFlow.Modules.Tracking.Domain/*.csproj src/ParcelFlow.Modules.Tracking.Domain/
COPY src/ParcelFlow.Modules.Tracking.Application/*.csproj src/ParcelFlow.Modules.Tracking.Application/
COPY src/ParcelFlow.Modules.Tracking.Infrastructure/*.csproj src/ParcelFlow.Modules.Tracking.Infrastructure/
COPY src/ParcelFlow.Modules.Tracking.IntegrationEvents/*.csproj src/ParcelFlow.Modules.Tracking.IntegrationEvents/
COPY src/ParcelFlow.Modules.Deliveries.Domain/*.csproj src/ParcelFlow.Modules.Deliveries.Domain/
COPY src/ParcelFlow.Modules.Deliveries.Application/*.csproj src/ParcelFlow.Modules.Deliveries.Application/
COPY src/ParcelFlow.Modules.Deliveries.Infrastructure/*.csproj src/ParcelFlow.Modules.Deliveries.Infrastructure/
COPY src/ParcelFlow.Modules.Deliveries.IntegrationEvents/*.csproj src/ParcelFlow.Modules.Deliveries.IntegrationEvents/
COPY src/ParcelFlow.Api/*.csproj src/ParcelFlow.Api/
RUN dotnet restore src/ParcelFlow.Api/ParcelFlow.Api.csproj

# 2) Luego el código y publish.
COPY src/ src/
RUN dotnet publish src/ParcelFlow.Api/ParcelFlow.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS final
WORKDIR /app
COPY --from=build /app .

# La imagen de .NET ya corre con un usuario sin privilegios ("app"), no como root.
USER app

# Render indica el puerto en la variable PORT; en local se usa 8080.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
CMD ["sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-8080} exec dotnet ParcelFlow.Api.dll"]
