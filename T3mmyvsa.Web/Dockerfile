# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["T3mmyvsa.csproj", "./"]
RUN dotnet restore "T3mmyvsa.csproj"

FROM restore AS build
ARG BUILD_CONFIGURATION=Release
COPY . .
RUN dotnet build "T3mmyvsa.csproj" \
    -c "$BUILD_CONFIGURATION" \
    --no-restore \
    -p:OpenApiGenerateDocuments=false

FROM build AS migrations
ARG BUILD_CONFIGURATION=Release
ARG DOTNET_EF_VERSION=10.0.11
RUN dotnet tool install --tool-path /tools dotnet-ef --version "$DOTNET_EF_VERSION"
ENV PATH="/tools:${PATH}"
COPY --chmod=755 docker/apply-migrations.sh /usr/local/bin/apply-migrations
ENTRYPOINT ["/usr/local/bin/apply-migrations"]

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "T3mmyvsa.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/publish \
    --no-build \
    -p:UseAppHost=false \
    -p:OpenApiGenerateDocuments=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_EnableDiagnostics=0 \
    HOME=/home/app

EXPOSE 8080
STOPSIGNAL SIGTERM

RUN mkdir -p /app/Logs /app/data /home/app/.aspnet/DataProtection-Keys \
    && chown -R app:app /app /home/app/.aspnet

COPY --from=publish --chown=app:app /app/publish .

USER app
ENTRYPOINT ["dotnet", "T3mmyvsa.dll"]
