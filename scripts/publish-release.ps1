# Requires: GitHub CLI (`gh`) — run `gh auth login` once.
# Builds NexusStrapSetup.exe (single-file) and creates a GitHub release.
# Usage from repo root: .\scripts\publish-release.ps1 -Version v1.0.0

param(
    [Parameter(Mandatory = $true)]
    [string] $Version
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root

$staging = Join-Path $root "publish\gh-staging"
if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
New-Item -ItemType Directory -Path $staging | Out-Null

Write-Host "Publishing single-file NexusStrapSetup..."
dotnet publish src\NexusStrap\NexusStrap.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:PublishTrimmed=false `
    -o $staging

$exe = Join-Path $root "NexusStrapSetup.exe"
Copy-Item (Join-Path $staging "NexusStrap.exe") $exe -Force

Write-Host "Creating GitHub release $Version ..."
gh release create $Version $exe --repo PhantomBum/NexusStrap --title "NexusStrap $Version" --notes "Download **NexusStrapSetup.exe** — single file, no zip. Double-click to run (self-contained)."

Write-Host "Done: https://github.com/PhantomBum/NexusStrap/releases"
