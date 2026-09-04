param(
    [string]$MigrationName = "InitialCreate",
    [string]$EnvFile = ".env"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $EnvFile)) {
    throw "Environment file '$EnvFile' was not found. Copy one of the Docker environment examples to .env first."
}

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw "Docker is required to resolve the Compose environment."
}

try {
    dotnet ef --version | Out-Null
}
catch {
    throw "dotnet-ef is required. Install it with: dotnet tool install --global dotnet-ef"
}

$resolved = @{}
docker compose --env-file $EnvFile config --environment | ForEach-Object {
    $index = $_.IndexOf('=')
    if ($index -gt 0) {
        $key = $_.Substring(0, $index)
        $value = $_.Substring($index + 1)
        $resolved[$key] = $value
    }
}

$provider = $resolved["DATABASE_PROVIDER"]
$connectionString = $resolved["DATABASE_CONNECTION_STRING"]

if ([string]::IsNullOrWhiteSpace($provider) -or [string]::IsNullOrWhiteSpace($connectionString)) {
    throw "DATABASE_PROVIDER and DATABASE_CONNECTION_STRING must resolve from '$EnvFile'."
}

Write-Host "Generating EF Core migration '$MigrationName' for provider '$provider' in the dedicated migrations project..."

$env:DatabaseSettings__Provider = $provider
$env:DatabaseSettings__ConnectionStringName = "appDatabase"
$env:ConnectionStrings__appDatabase = $connectionString

try {
    dotnet ef migrations add $MigrationName `
        --project Migrations/T3mmyvsa.Migrations.csproj `
        --startup-project Migrations/T3mmyvsa.Migrations.csproj `
        --output-dir Data/Migrations
}
finally {
    Remove-Item Env:DatabaseSettings__Provider -ErrorAction SilentlyContinue
    Remove-Item Env:DatabaseSettings__ConnectionStringName -ErrorAction SilentlyContinue
    Remove-Item Env:ConnectionStrings__appDatabase -ErrorAction SilentlyContinue
}
