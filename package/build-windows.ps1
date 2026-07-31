# Builds a self-contained win-x64 installer for QuickCopyTags using Inno Setup.
#
# Usage:
#   ./package/build-windows.ps1 [-Version 1.2.0]
#
# Output:
#   dist/quickcopytags_<version>_win-x64_setup.exe
#
# Requires: .NET SDK, Inno Setup 6 (ISCC.exe on PATH or in a well-known install location).

param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$ProjectDir = Join-Path $RepoRoot "QuickCopyTags"
$PublishDir = Join-Path $ProjectDir "bin\Release\net10.0\win-x64\publish"
$DistDir = Join-Path $RepoRoot "dist"
$IssFile = Join-Path $ScriptDir "windows.iss"

Write-Host "==> Publishing self-contained win-x64 build"
dotnet publish $ProjectDir -c Release -r win-x64 --self-contained `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $PublishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

$Iscc = Get-Command iscc.exe -ErrorAction SilentlyContinue
if (-not $Iscc) {
    $Candidates = @(
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
    )
    $Found = $Candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $Found) {
        throw "ISCC.exe (Inno Setup) not found. Install Inno Setup 6 or add it to PATH."
    }
    $IsccPath = $Found
} else {
    $IsccPath = $Iscc.Source
}

New-Item -ItemType Directory -Force -Path $DistDir | Out-Null

Write-Host "==> Building installer with Inno Setup"
& $IsccPath "/DAppVersion=$Version" "/DPublishDir=$PublishDir" $IssFile
if ($LASTEXITCODE -ne 0) { throw "ISCC.exe failed" }

Write-Host "==> Done: $DistDir\quickcopytags_${Version}_win-x64_setup.exe"
