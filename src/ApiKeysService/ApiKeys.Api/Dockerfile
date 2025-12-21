FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/ApiKeysService/ApiKeys.Api/ApiKeys.Api.csproj", "src/ApiKeysService/ApiKeys.Api/"]
RUN dotnet restore "src/ApiKeysService/ApiKeysService.Api/ApiKeysService.Api.csproj"
COPY . .
WORKDIR "/src/src/ApiKeysService/ApiKeys.Api"
RUN dotnet build "./ApiKeys.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ApiKeys.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiKeys.Api.dll"]
