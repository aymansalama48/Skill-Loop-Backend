<#
.SYNOPSIS
    Starts the Skill Loop API on this machine with no Docker and no Redis.

.DESCRIPTION
    Resolves everything the app needs before it will boot:

      1. Security secrets. The app refuses to start without a valid Jwt:Key and
         OtpSettings:HashingSecret - that is deliberate, since the values that used
         to live in appsettings.json are public. They come from .NET user-secrets
         (preferred) or a .env file, and this script verifies they are actually
         present before handing over to dotnet run, so a failure names the missing
         key instead of surfacing as a startup stack trace.
      2. SQL Server. appsettings.json points at AYMAN\MSSQLSERVER01, an instance
         that does not exist on this laptop. This script targets the default
         instance (localhost) instead, which is what is actually installed.
      3. Hangfire. Requires its own database, separate from the application one -
         sharing a database makes the background worker compete with live requests
         for the same connection pool and locks.
      4. Redis. There is no Redis and no Docker. Setting Redis__ConnectionString
         to empty makes the app register its in-memory ICacheService instead of
         the Redis one - see AddCaching.cs, which falls back to MemoryCacheService
         on an empty connection string. Nothing else in the app needs Redis:
         OtpService stores OTPs in the database, and rate limiting is in-process.

    The application database is created, migrated and seeded automatically on first
    start. That includes applying HardenRefreshTokenStorage, which clears stored
    refresh tokens - every user has to sign in again the first time.

.EXAMPLE
    .\docs\run-api-local.ps1
    .\docs\run-api-local.ps1 -Rebuild
#>
[CmdletBinding()]
param(
    [switch]$Rebuild,
    [string]$Database = "localhost",
    [string]$DatabaseName = "Skill-Loop",
    [string]$HangfireDatabaseName = "Skill-Loop-Hangfire",
    [int]$HttpsPort = 7271,
    [int]$HttpPort = 7000
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

# --- 1. secrets -----------------------------------------------------------------
# Two supported sources, in precedence order:
#
#   a) .NET user-secrets  (dotnet user-secrets set ...)  - preferred
#   b) .env                                        - optional fallback
#
# Nothing is printed. Values stay off the scrollback and out of the transcript.
$values = @{}

$envFile = Join-Path $root ".env"
if (Test-Path -LiteralPath $envFile) {
    Get-Content -LiteralPath $envFile | ForEach-Object {
        if ($_ -match '^\s*([^#=\s]+)\s*=\s*(.*)$') {
            $name = $matches[1].Trim()
            $values[$name] = $matches[2].Trim().Trim('"').Trim("'")
        }
    }
}

# .env uses flat names; .NET's binder wants the section__key form.
$envAliases = @{
    "Jwt__Key"                 = "JWT_KEY"
    "OtpSettings__HashingSecret" = "OTP_SECRET"
    "Seed__SuperAdmin__Email"  = "SUPERADMIN_EMAIL"
    "Seed__SuperAdmin__Password" = "SUPERADMIN_PASSWORD"
}
foreach ($target in $envAliases.Keys) {
    $source = $envAliases[$target]
    if ($values.ContainsKey($source) -and $values[$source]) {
        Set-Item -Path "Env:$target" -Value $values[$source]
    }
}

# user-secrets are loaded by the host itself in Development, but only for keys the
# app actually reads. These two are the ones that will abort startup, so confirm
# they resolved from *somewhere* before starting the process.
#
# The store is NOT inside the project: dotnet stores it under %APPDATA%, keyed by
# the UserSecretsId declared in the csproj. Read that id rather than hardcoding the
# GUID, so a regenerated project does not silently lose its secrets.
$apiProject = Join-Path $root "src\Skill-Loop.Api\Skill-Loop.Api.csproj"
$userSecrets = @{}
$secretsId = (Select-String -Path $apiProject -Pattern '<UserSecretsId>([^<]+)</UserSecretsId>').Matches.Groups[1].Value
if ($secretsId) {
    $secretsPath = Join-Path $env:APPDATA "Microsoft\UserSecrets\$secretsId\secrets.json"
    if (Test-Path -LiteralPath $secretsPath) {
        $json = Get-Content -Raw -LiteralPath $secretsPath | ConvertFrom-Json
        foreach ($prop in $json.PSObject.Properties) {
            $userSecrets[$prop.Name] = $prop.Value
        }
    }
}

$missing = @()
if (-not $userSecrets["Jwt:Key"] -and -not $values["JWT_KEY"]) {
    $missing += 'Jwt:Key'
}
if (-not $userSecrets["OtpSettings:HashingSecret"] -and -not $values["OTP_SECRET"]) {
    $missing += 'OtpSettings:HashingSecret'
}

if ($missing.Count -gt 0) {
    throw @"
Missing required secret(s): $($missing -join ', ')

The API will not start without them. Set them with user-secrets:

  cd src\Skill-Loop.Api
  dotnet user-secrets set "Jwt:Key"                 "`$(openssl rand -base64 48)"
  dotnet user-secrets set "OtpSettings:HashingSecret" "`$(openssl rand -base64 48)"

Optionally, to have a first SuperAdmin (there is no default password):

  dotnet user-secrets set "Seed:SuperAdmin:Email"    "admin@skillloop.com"
  dotnet user-secrets set "Seed:SuperAdmin:Password" "`$(openssl rand -base64 24)Aa1!"
"@
}

# --- 2. SQL Server --------------------------------------------------------------
# Integrated Security, so no password to store.
#
# Both databases are created up front. This is not redundant with EF Core: the app
# database would be created by MigrateAsync() on startup, but Hangfire only ever
# creates its own *tables* - it never creates the *database*. Point it at a name that
# does not exist and startup dies with "Cannot open database ... requested by the
# login", which is a confusing way to learn that.
function Initialize-SqlDatabase {
    param([string]$Server, [string]$Name)

    $master = "Server=$Server;Database=master;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=15"
    $connection = New-Object System.Data.SqlClient.SqlConnection $master
    try {
        $connection.Open()
        # Parameterised, so a database name with a quote or dash cannot break out.
        # The dynamic SQL is built in a variable first: EXEC cannot take a
        # concatenated expression directly in its argument list.
        $command = $connection.CreateCommand()
        $command.CommandText = @"
DECLARE @sql nvarchar(max);
IF DB_ID(@name) IS NULL
    SET @sql = N'CREATE DATABASE ' + QUOTENAME(@name);
ELSE
    SET @sql = N'SELECT 1';
EXEC sp_executesql @sql;
"@
        $null = $command.Parameters.Add("@name", [System.Data.SqlDbType]::NVarChar, 128)
        $command.Parameters["@name"].Value = $Name
        $null = $command.ExecuteNonQuery()
    }
    finally {
        $connection.Dispose()
    }
}

try {
    Initialize-SqlDatabase -Server $Database -Name $DatabaseName
    Initialize-SqlDatabase -Server $Database -Name $HangfireDatabaseName
}
catch {
    throw "Could not reach SQL Server on '$Database': $($_.Exception.Message)`nStart SQL Server LocalDB (sqllocaldb start) or point -Database at your instance."
}

$env:ConnectionStrings__DefaultConnection = "Server=$Database;Database=$DatabaseName;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"

# --- 3. Hangfire ----------------------------------------------------------------
# Required, and must be a different database from the application one.
$env:ConnectionStrings__HangfireConnection = "Server=$Database;Database=$HangfireDatabaseName;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"

# --- 4. Redis -------------------------------------------------------------------
# Empty string is what triggers the in-memory fallback. Leaving it as
# "localhost:6379" would make every cache call retry against nothing.
$env:Redis__ConnectionString = ""

# --- 5. ports -------------------------------------------------------------------
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "https://localhost:$HttpsPort;http://localhost:$HttpPort"

Write-Host ""
Write-Host "Starting Skill Loop API" -ForegroundColor Cyan
Write-Host "  Scalar   https://localhost:$HttpsPort/scalar/v1"
Write-Host "  OpenAPI  https://localhost:$HttpsPort/openapi/v1.json"
Write-Host "  SQL      $Database/$DatabaseName  (+ $HangfireDatabaseName for Hangfire)"
Write-Host "  Redis    disabled, using in-memory cache"
Write-Host "  Secrets  $(if ($envFile -and (Test-Path -LiteralPath $envFile)) { 'user-secrets + .env' } else { 'user-secrets' })"
Write-Host ""

$project = $apiProject
if ($Rebuild) {
    dotnet build $project -v q --nologo
    if (-not $?) { throw "Build failed." }
    dotnet run --project $project --no-build
}
else {
    dotnet run --project $project
}
