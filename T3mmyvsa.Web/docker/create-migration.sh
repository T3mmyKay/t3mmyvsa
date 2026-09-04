#!/usr/bin/env bash
set -euo pipefail

env_file="${ENV_FILE:-.env}"
migration_name="${1:-InitialCreate}"

if [[ ! -f "$env_file" ]]; then
  echo "Environment file '$env_file' was not found. Copy one of the Docker environment examples to .env first." >&2
  exit 2
fi

if ! command -v docker >/dev/null 2>&1; then
  echo "Docker is required to resolve the Compose environment." >&2
  exit 2
fi

if ! dotnet ef --version >/dev/null 2>&1; then
  echo "dotnet-ef is required. Install it with: dotnet tool install --global dotnet-ef" >&2
  exit 2
fi

compose_environment="$(docker compose --env-file "$env_file" config --environment)"

read_compose_value() {
  local key="$1"
  local line
  line="$(printf '%s\n' "$compose_environment" | grep -m1 "^${key}=" || true)"
  printf '%s' "${line#*=}"
}

provider="$(read_compose_value DATABASE_PROVIDER)"
connection_string="$(read_compose_value DATABASE_CONNECTION_STRING)"

if [[ -z "$provider" || -z "$connection_string" ]]; then
  echo "DATABASE_PROVIDER and DATABASE_CONNECTION_STRING must resolve from '$env_file'." >&2
  exit 2
fi

echo "Generating EF Core migration '$migration_name' for provider '$provider' in the dedicated migrations project..."

DatabaseSettings__Provider="$provider" \
DatabaseSettings__ConnectionStringName="appDatabase" \
ConnectionStrings__appDatabase="$connection_string" \
dotnet ef migrations add "$migration_name" \
  --project Migrations/T3mmyvsa.Migrations.csproj \
  --startup-project Migrations/T3mmyvsa.Migrations.csproj \
  --output-dir Data/Migrations
