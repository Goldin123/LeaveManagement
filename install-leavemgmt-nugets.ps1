param(
  [string]$Name = "LeaveMgmt",
  [string]$Path = "D:\Sandbox\ASP"
)

$root = Join-Path $Path $Name
$src  = Join-Path $root "src"

$domProj = Join-Path $src "$Name.Domain\$Name.Domain.csproj"
$appProj = Join-Path $src "$Name.Application\$Name.Application.csproj"
$infProj = Join-Path $src "$Name.Infrastructure\$Name.Infrastructure.csproj"
$apiProj = Join-Path $src "$Name.Api\$Name.Api.csproj"

Write-Host "==> Installing NuGet packages..."

# ---------------------------
# Domain
# (keep pure: no packages here)

# ---------------------------
# Application
dotnet add $appProj package FluentValidation --version 11.9.0
dotnet add $appProj package Mapster --version 7.4.0

# ---------------------------
# Infrastructure
dotnet add $infProj package Dapper
dotnet add $infProj package Microsoft.Data.SqlClient
dotnet add $infProj package Microsoft.EntityFrameworkCore
dotnet add $infProj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add $infProj package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add $infProj package StackExchange.Redis
dotnet add $infProj package Microsoft.Extensions.Caching.StackExchangeRedis
dotnet add $infProj package Microsoft.Extensions.Options.ConfigurationExtensions

# ---------------------------
# API
dotnet add $apiProj package FastEndpoints
dotnet add $apiProj package FastEndpoints.Swagger
dotnet add $apiProj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add $apiProj package Microsoft.AspNetCore.Identity
dotnet add $apiProj package Microsoft.AspNetCore.Identity.UI

# ---------------------------
# (Optional) Test projects later
# dotnet add tests\LeaveMgmt.UnitTests\LeaveMgmt.UnitTests.csproj package FluentAssertions
# dotnet add tests\LeaveMgmt.FunctionalTests\LeaveMgmt.FunctionalTests.csproj package Microsoft.AspNetCore.Mvc.Testing
# dotnet add tests\LeaveMgmt.IntegrationTests\LeaveMgmt.IntegrationTests.csproj package FluentAssertions

Write-Host "✅ Packages installed."
