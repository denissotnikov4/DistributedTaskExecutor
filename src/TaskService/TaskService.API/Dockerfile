FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TaskService.API/TaskService.API.csproj TaskService.API/
COPY TaskService.Client/TaskService.Client.csproj TaskService.Client/
COPY TaskService.Logic/TaskService.Logic.csproj TaskService.Logic/
COPY TaskService.Dal/TaskService.Dal.csproj TaskService.Dal/

RUN dotnet restore TaskService.API/TaskService.API.csproj

COPY . .

RUN dotnet publish TaskService.API/TaskService.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskService.API.dll"]