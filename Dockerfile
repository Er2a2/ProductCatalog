# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore "ProductCatalog.Api/ProductCatalog.Api.csproj"

RUN dotnet publish "ProductCatalog.Api/ProductCatalog.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore


# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ProductCatalog.Api.dll"]