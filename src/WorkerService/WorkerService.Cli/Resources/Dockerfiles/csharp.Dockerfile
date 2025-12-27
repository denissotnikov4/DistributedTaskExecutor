ARG SDK_IMAGE
ARG RUNTIME_IMAGE
ARG FRAMEWORK_VERSION
ARG LANGUAGE_VERSION

FROM ${SDK_IMAGE} AS build
WORKDIR /src
RUN dotnet new console --framework "${FRAMEWORK_VERSION}" --langVersion "${LANGUAGE_VERSION}" --name App
WORKDIR /src/App
COPY . .
RUN dotnet publish -c Release -o /out /p:UseAppHost=false

FROM ${RUNTIME_IMAGE} AS runtime
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "App.dll"]