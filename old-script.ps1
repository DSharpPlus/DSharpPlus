# Old script from AppVeyor. Stored for archiving purposes.

# Version number
$VERSION = [int]::Parse($Env:APPVEYOR_BUILD_NUMBER).ToString("00000")

# Environment variables
$Env:ARTIFACT_DIR = ".\artifacts"

# Prepare the environment
Copy-Item .\.nuget.\NuGet.config .\
$dir = New-Item -type directory $env:ARTIFACT_DIR
$dir = $dir.FullName

# Verbosity
Write-Host "Build: $VERSION"
Write-Host "Artifacts will be placed in: $dir"

# Restore NuGet packages
dotnet restore -v Minimal
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode); }

# Build
dotnet build DSharpPlus.sln -v Minimal -c Release --version-suffix "$VERSION"
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode); }

# Package
dotnet pack DSharpPlus.sln -v Minimal -c Release -o "$dir" --no-build --version-suffix "$VERSION"
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode); }

# Remove Test packages
# Get-ChildItem $env:ARTIFACT_DIR | ? { $_.Name.ToLower().StartsWith("DSharpPlus.Test") } | % { $_.Delete() }

# Check if this is a PR
if (-not $Env:APPVEYOR_PULL_REQUEST_NUMBER)
{
    # Rebuild documentation
    & .\rebuild-docs.ps1 .\docs "$dir" dsharpplus-docs
}
else
{
    Write-Host "Building from PR ($Env:APPVEYOR_PULL_REQUEST_NUMBER); skipping docs build"
}