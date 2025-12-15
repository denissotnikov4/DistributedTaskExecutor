FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TaskService.Api/TaskService.Api.csproj", "TaskService.Api/"]
COPY ["TaskService.Logic/TaskService.Logic.csproj", "TaskService.Logic/"]
COPY ["TaskService.Model/TaskService.Model.csproj", "TaskService.Model/"]

RUN dotnet restore "TaskService.Api/TaskService.Api.csproj"
COPY . .
WORKDIR "/src/TaskService.Api"
RUN dotnet build "TaskService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskService.Api.dll"]