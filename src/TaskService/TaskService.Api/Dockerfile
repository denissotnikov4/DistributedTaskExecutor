FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TaskService.Api/TaskService.Api.csproj TaskService.Api/
COPY TaskService.Client/TaskService.Client.csproj TaskService.Client/
COPY TaskService.Logic/TaskService.Logic.csproj TaskService.Logic/
COPY TaskService.Dal/TaskService.Dal.csproj TaskService.Dal/

RUN dotnet restore TaskService.Api/TaskService.Api.csproj

COPY . .

RUN dotnet publish TaskService.Api/TaskService.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskService.Api.dll"]