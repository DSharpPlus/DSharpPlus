#!/usr/bin/env pwsh
# Rebuild-all
#
# Rebuilds the DSharpPlus project and its documentation, and places artifacts in specified directories.
# Not specifying documentation options will skip documentation build.
# 
# Author:       Emzi0767
# Version:      2017-09-11 14:20
#
# Arguments:
#   .\rebuild-docs.ps1 <output path> <version suffix> [path to docfx] [output path] [docs project path]
#
# Run as:
#   .\rebuild-lib.ps1 .\path\to\artifact\location version-suffix .\path\to\docfx\project .\path\to\output project-docs

param
(
    [parameter(Mandatory = $true)]
    [string] $ArtifactLocation,

    [parameter(Mandatory = $false)]
    [string] $VersionSuffix,
    
    [parameter(Mandatory = $false)]
    [string] $DocsPath,
    
    [parameter(Mandatory = $false)]
    [string] $DocsPackageName
)

# Check if we have a version prefix
if (-not $VersionSuffix)
{
    # Nope
    Write-Host "Building production packages"
}
else
{
    # Yup
    Write-Host "Building beta packages"
}

# Invoke the build script
& .\rebuild-lib.ps1 -artifactlocation "$ArtifactLocation" -versionsuffix "$VersionSuffix" | Out-Host

# Check if it failed
if ($LastExitCode -ne 0)
{
    Write-Host "Build failed with code $LastExitCode"
    $host.SetShouldExit($LastExitCode)
    Exit $LastExitCode
}

# Check if we're building docs
if ($DocsPath -and $DocsPackageName)
{
    # Yup
    Write-Host "Building documentation"
    & .\rebuild-docs.ps1 -docspath "$DocsPath" -outputpath "$ArtifactLocation" -packagename "$DocsPackageName"
    
    # Check if it failed
    if ($LastExitCode -ne 0)
    {
        Write-Host "Documentation build failed with code $LastExitCode"
        $host.SetShouldExit($LastExitCode)
        Exit $LastExitCode
    }
}
else
{
    # Nope
    Write-Host "Not building documentation"
}
