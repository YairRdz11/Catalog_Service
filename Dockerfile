# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# NuGet local
COPY nuget-local/ ./nuget-local/
RUN dotnet nuget remove source nuget-local || true
RUN dotnet nuget add source "/src/nuget-local" --name nuget-local

# Copiar repo y restore
COPY . .
RUN if [ -f "CatalogService.sln" ]; then \
      dotnet restore ./CatalogService.sln; \
    else \
      dotnet restore ./src/CatalogService.API/CatalogService.API.csproj; \
    fi

# Publicar API
RUN dotnet publish ./src/CatalogService.API/CatalogService.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# HTTP en 80 y entorno Development para Swagger
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_ENVIRONMENT=Development

EXPOSE 80
ENTRYPOINT ["dotnet", "CatalogService.API.dll"]