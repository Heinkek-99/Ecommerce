FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Api/Ecommerce.Api.csproj", "src/Api/"]
COPY ["src/Shared/Ecommerce.Shared.csproj", "src/Shared/"]
COPY ["src/Infrastructure/Ecommerce.Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Modules/Catalog/Ecommerce.Catalog.csproj", "src/Modules/Catalog/"]
COPY ["src/Modules/Orders/Ecommerce.Orders.csproj", "src/Modules/Orders/"]
COPY ["src/Modules/Promotions/Ecommerce.Promotions.csproj", "src/Modules/Promotions/"]
COPY ["src/Modules/Loyalty/Ecommerce.Loyalty.csproj", "src/Modules/Loyalty/"]
COPY ["src/Modules/Notifications/Ecommerce.Notifications.csproj", "src/Modules/Notifications/"]
RUN dotnet restore "src/Api/Ecommerce.Api.csproj"
COPY . .
RUN dotnet build "src/Api/Ecommerce.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Api/Ecommerce.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Api.dll"]
