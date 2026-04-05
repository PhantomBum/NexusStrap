# Requires: GitHub CLI (`gh`) — run `gh auth login` once.
# Usage from repo root: .\scripts\publish-release.ps1 -Version v1.0.0

param(
    [Parameter(Mandatory = $true)]
    [string] $Version
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root

$publishDir = Join-Path $root "publish\NexusStrap-win-x64"
$zipName = Join-Path $root "NexusStrap-$Version-win-x64.zip"

Write-Host "Publishing to $publishDir ..."
dotnet publish src\NexusStrap\NexusStrap.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o $publishDir

if (Test-Path $zipName) { Remove-Item $zipName -Force }
Compress-Archive -Path $publishDir -DestinationPath $zipName -CompressionLevel Optimal

Write-Host "Creating GitHub release $Version ..."
gh release create $Version $zipName --repo PhantomBum/NexusStrap --title "NexusStrap $Version" --notes "Windows x64 self-contained build. Extract the zip and run NexusStrap.exe."

Write-Host "Done: https://github.com/PhantomBum/NexusStrap/releases"
