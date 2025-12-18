FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Api/Api.csproj Api/
COPY Logic/Logic.csproj Logic/
COPY Dal/Dal.csproj Dal/

RUN dotnet restore Api/Api.csproj

COPY . .

RUN dotnet publish Api/Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Api.dll"]