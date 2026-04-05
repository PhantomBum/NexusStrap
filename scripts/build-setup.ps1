# Builds a single self-contained EXE: NexusStrapSetup.exe (no zip, no extra steps for users).
# Requires: .NET 8 SDK
# Usage: .\scripts\build-setup.ps1  [optional: output directory, default: .\artifacts]

param(
    [string] $OutputDir = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root

if ([string]::IsNullOrEmpty($OutputDir)) {
    $OutputDir = Join-Path $root "artifacts"
}

$staging = Join-Path $OutputDir "staging"
if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
New-Item -ItemType Directory -Path $staging | Out-Null

Write-Host "Publishing single-file self-contained app (win-x64)..."
dotnet publish src\NexusStrap\NexusStrap.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:PublishTrimmed=false `
    -o $staging

$built = Join-Path $staging "NexusStrap.exe"
if (-not (Test-Path $built)) {
    throw "Expected output not found: $built"
}

$final = Join-Path $OutputDir "NexusStrapSetup.exe"
Copy-Item $built $final -Force
Write-Host ""
Write-Host "Done: $final"
Write-Host "Share this one file: users double-click to run (no unzip, no separate .NET install)."
Get-Item $final | Format-List FullName, Length
