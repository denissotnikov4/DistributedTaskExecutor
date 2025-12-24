ARG SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:8.0
ARG RUNTIME_IMAGE=mcr.microsoft.com/dotnet/runtime:8.0

FROM ${SDK_IMAGE} AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /out /src/App.csproj

FROM ${RUNTIME_IMAGE} AS runtime
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "App.dll"]