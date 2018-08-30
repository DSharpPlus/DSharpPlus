#!/usr/bin/env pwsh
# One-click rebuild
#
# Rebuilds all DSharpPlus components with default values.
#
# Author: Emzi0767
# Version: 2018-08-30 14:34
#
# Arguments: none
#
# Run as: just run it

# ------------ Defaults -----------

# Output path for bots binaries and documentation.
$target = "..\dsp-artifacts"

# Version suffix data. Default is none. General format is 5-digit number.
# The version will be formatted as such: $version-$suffix-$build_number
# e.g. 4.0.0-ci-00579
# If build_number is set to -1, it will be ignored.
$suffix = ""
$build_number = -1

# Documentation build. Set either to empty string to skip documentation build.
$docs_path = ".\docs"
$docs_pkgname = "dsharpplus-docs"

# --------- Execute build ---------
& .\rebuild-all.ps1 -ArtifactLocation "$target" -Configuration "Release" -VersionSuffix "$suffix" -BuildNumber $build_number -DocsPath "$docs_path" -DocsPackageName "$docs_pkgname" | Out-Host
if ($LastExitCode -ne 0)
{
    Write-Host "----------------------------------------------------------------"
    Write-Host "                          BUILD FAILED"
    Write-Host "----------------------------------------------------------------"
}

Write-Host "Press any key to continue..."
$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
Exit $LastExitCode
