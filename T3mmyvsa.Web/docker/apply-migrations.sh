#!/usr/bin/env bash
set -euo pipefail

if ! find Data/Migrations -maxdepth 1 -type f -name '*.cs' ! -name '*.Designer.cs' ! -name '*ModelSnapshot.cs' | grep -q .; then
  echo "No EF Core migrations were found in Data/Migrations." >&2
  echo "Choose your database provider and generate the initial migration before starting the Docker stack." >&2
  exit 2
fi

max_attempts="${MIGRATION_MAX_ATTEMPTS:-30}"
retry_delay="${MIGRATION_RETRY_DELAY_SECONDS:-2}"

for ((attempt = 1; attempt <= max_attempts; attempt++)); do
  echo "Applying EF Core migrations (attempt ${attempt}/${max_attempts})..."

  if dotnet ef database update --no-build --configuration Release; then
    echo "EF Core migrations applied successfully."
    exit 0
  fi

  if (( attempt == max_attempts )); then
    break
  fi

  echo "Database is not ready or migration failed; retrying in ${retry_delay}s..." >&2
  sleep "$retry_delay"
done

echo "EF Core migration failed after ${max_attempts} attempts." >&2
exit 1
