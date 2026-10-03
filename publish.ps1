$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$out = Join-Path $root "dist"

dotnet publish (Join-Path $root "vMixSessionCleaner.csproj") `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -o $out

Write-Host ""
Write-Host "Ready: $out\vMixSessionCleaner.exe"
Write-Host "No .NET / Node / Python / VS Redistributable required for end users."
