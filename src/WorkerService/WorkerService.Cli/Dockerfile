FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY WorkerService.Cli.csproj WorkerService.Cli/
RUN dotnet restore WorkerService.Cli/WorkerService.Cli.csproj
WORKDIR /src/WorkerService.Cli
COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM docker:27-cli AS dockercli

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=dockercli /usr/local/bin/docker /usr/local/bin/docker
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WorkerService.Cli.dll"]