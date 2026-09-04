# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["T3mmyvsa.csproj", "./"]
COPY ["Migrations/T3mmyvsa.Migrations.csproj", "Migrations/"]
COPY ["Worker/T3mmyvsa.Worker.csproj", "Worker/"]
RUN dotnet restore "T3mmyvsa.csproj" \
    && dotnet restore "Migrations/T3mmyvsa.Migrations.csproj" \
    && dotnet restore "Worker/T3mmyvsa.Worker.csproj"

FROM restore AS build
ARG BUILD_CONFIGURATION=Release
COPY . .
RUN dotnet build "T3mmyvsa.csproj" \
    -c "$BUILD_CONFIGURATION" \
    --no-restore \
    -p:OpenApiGenerateDocuments=false \
    && dotnet build "Migrations/T3mmyvsa.Migrations.csproj" \
    -c "$BUILD_CONFIGURATION" \
    --no-restore \
    -p:OpenApiGenerateDocuments=false \
    && dotnet build "Worker/T3mmyvsa.Worker.csproj" \
    -c "$BUILD_CONFIGURATION" \
    --no-restore \
    -p:OpenApiGenerateDocuments=false

FROM build AS publish-api
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "T3mmyvsa.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/api \
    --no-build \
    -p:UseAppHost=false \
    -p:OpenApiGenerateDocuments=false

FROM build AS publish-migrations
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Migrations/T3mmyvsa.Migrations.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/migrations \
    --no-build \
    -p:UseAppHost=false \
    -p:OpenApiGenerateDocuments=false

FROM build AS publish-worker
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Worker/T3mmyvsa.Worker.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/worker \
    --no-build \
    -p:UseAppHost=false \
    -p:OpenApiGenerateDocuments=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS migrations
WORKDIR /app
ENV DOTNET_EnableDiagnostics=0 HOME=/tmp
COPY --from=publish-migrations --chown=app:app /app/migrations .
USER app
ENTRYPOINT ["dotnet", "T3mmyvsa.Migrations.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS worker
WORKDIR /app
ENV DOTNET_EnableDiagnostics=0 HOME=/home/app
RUN mkdir -p /app/Logs /app/data \
    && chown -R app:app /app /home/app
COPY --from=publish-worker --chown=app:app /app/worker .
USER app
ENTRYPOINT ["dotnet", "T3mmyvsa.Worker.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_EnableDiagnostics=0 \
    HOME=/home/app

EXPOSE 8080
STOPSIGNAL SIGTERM

RUN mkdir -p /app/Logs /app/data /home/app/.aspnet/DataProtection-Keys \
    && chown -R app:app /app /home/app/.aspnet

COPY --from=publish-api --chown=app:app /app/api .

USER app
ENTRYPOINT ["dotnet", "T3mmyvsa.dll"]
