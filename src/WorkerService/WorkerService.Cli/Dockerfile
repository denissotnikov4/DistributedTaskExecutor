FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base

RUN apt-get update && apt-get install -y \
    ca-certificates \
    curl \
    gnupg \
    lsb-release \
    && curl -fsSL https://download.docker.com/linux/debian/gpg | tee /etc/apt/trusted.gpg.d/docker.asc

RUN echo "deb [arch=amd64] https://download.docker.com/linux/debian $(lsb_release -cs) stable" > etc/apt/sources.list.d/docker.list
RUN apt-get update && apt-get install -y docker-ce-cli

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WorkerService.Cli.csproj", "WorkerService.Cli/"]
RUN dotnet restore "WorkerService.Cli/WorkerService.Cli.csproj"
COPY [".", "WorkerService.Cli/"]
WORKDIR "/src/WorkerService.Cli"
RUN dotnet publish "./WorkerService.Cli.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkerService.Cli.dll"]