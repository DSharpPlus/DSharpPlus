#!/usr/bin/env pwsh
# One-click rebuild
#
# Rebuilds all DSharpPlus components with default values.
#
# Author: Emzi0767
# Version: 2017-09-11 14:20
#
# Arguments: none
#
# Run as: just run it

# ------------ Defaults -----------

# Output path for bots binaries and documentation.
$target = "..\dsp-artifacts"

# Version suffix. Default is none. General format is 5-digit number.
$suffix = ""

# Documentation build. Set either to empty string to skip documentation build.
$docs_path = ".\docs"
$docs_pkgname = "dsharpplus-docs"

# --------- Execute build ---------
& .\rebuild-all.ps1 -ArtifactLocation "$target" -VersionSuffix "$suffix" -DocsPath "$docs_path" -DocsPackageName "$docs_pkgname" | Out-Host
if ($LastExitCode -ne 0)
{
    Write-Host "----------------------------------------------------------------"
    Write-Host "                          BUILD FAILED"
    Write-Host "----------------------------------------------------------------"
}

Write-Host "Press any key to continue..."
$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
Exit $LastExitCode
