param(
  [string]$Name = "LeaveMgmt",
  [string]$Path = "D:\Sandbox\ASP"
)

$root = Join-Path $Path $Name
$src  = Join-Path $root "src"

$domProj = Join-Path $src "$Name.Domain"
$appProj = Join-Path $src "$Name.Application"
$infProj = Join-Path $src "$Name.Infrastructure"
$apiProj = Join-Path $src "$Name.Api"

function Mk($base, $folders) {
    foreach ($f in $folders) {
        $p = Join-Path $base $f
        New-Item -ItemType Directory -Force -Path $p | Out-Null
    }
}

# Domain: pure business rules
Mk $domProj @("Entities","ValueObjects","Enums","Events","Abstractions")

# Application: CQRS, DTOs, validators
Mk $appProj @("Abstractions","Common","DTOs","Features","Behaviors","Validation")

# Infrastructure: persistence, caching, messaging
Mk $infProj @("Persistence","Repositories","Caching","Messaging","Identity","Procedures")

# API: FastEndpoints, Auth
Mk $apiProj @("Endpoints","Auth","Configuration")

Write-Host "✅ Folder structure created under $src"
