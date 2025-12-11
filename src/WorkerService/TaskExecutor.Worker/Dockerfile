FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TaskExecutor.Worker/TaskExecutor.Worker.csproj", "TaskExecutor.Worker/"]
COPY ["../TaskService/TaskManagement.Domain/TaskManagement.Domain.csproj", "../TaskService/TaskManagement.Domain/"]
COPY ["../TaskService/TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj", "../TaskService/TaskManagement.Infrastructure/"]

RUN dotnet restore "TaskExecutor.Worker/TaskExecutor.Worker.csproj"
COPY . .
WORKDIR "/src/TaskExecutor.Worker"
RUN dotnet build "TaskExecutor.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskExecutor.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskExecutor.Worker.dll"]

