# STAGE 1: Base (Runtime)
# On utilise l'image "aspnet" qui est optimisée pour l'exécution (légère)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# STAGE 2: Build (SDK)
# On utilise l'image "sdk" qui contient les compilateurs (lourde)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# On copie les fichiers .csproj d'abord pour optimiser le cache Docker
COPY ["src/Api/Ecommerce.Api.csproj", "src/Api/"]
COPY ["src/Shared/Ecommerce.Shared.csproj", "src/Shared/"]
COPY ["src/Infrastructure/Ecommerce.Infrastructure.csproj", "src/Infrastructure/"]
# Ajoute ici les .csproj de tes modules au fur et à mesure (Catalog, Orders, etc.)

RUN dotnet restore "src/Api/Ecommerce.Api.csproj"
COPY . .
WORKDIR "/src/src/Api"
RUN dotnet build "Ecommerce.Api.csproj" -c Release -o /app/build

# STAGE 3: Publish
FROM build AS publish
RUN dotnet publish "Ecommerce.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# STAGE 4: Final
# On récupère le binaire compilé et on le met dans l'image "base"
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Api.dll"]