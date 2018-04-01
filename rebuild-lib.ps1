#!/usr/bin/env pwsh
# Rebuild-lib
#
# Rebuilds the entire DSharpPlus project, and places artifacts in specified directory.
# 
# Author:       Emzi0767
# Version:      2017-09-11 14:20
#
# Arguments:
#   .\rebuild-lib.ps1 <output path> [version suffix]
#
# Run as:
#   .\rebuild-lib.ps1 .\path\to\artifact\location version-suffix
#
# or
#   .\rebuild-lib.ps1 .\path\to\artifact\location

param
(
    [parameter(Mandatory = $true)]
    [string] $ArtifactLocation,

    [parameter(Mandatory = $false)]
    [string] $VersionSuffix,

    [parameter(Mandatory = $true)]
    [string] $Configuration
)

# Check if configuration is valid
if ($Configuration -ne "Debug" -and $Configuration -ne "Release")
{
    Write-Host "Invalid configuration specified. Must be Release or Debug."
    Exit 1
}

# Restores the environment
function Restore-Environment()
{
    Write-Host "Restoring environment"
    Remove-Item ./NuGet.config
}

# Prepares the environment
function Prepare-Environment([string] $target_dir_path)
{
    # Prepare the environment
    Copy-Item ./.nuget/NuGet.config ./
    
    # Check if the target directory exists
    # If it does, remove it
    if (Test-Path "$target_dir_path")
    {
        Write-Host "Target directory exists, deleting"
        Remove-Item -recurse -force "$target_dir_path"
    }
    
    # Create target directory
    $dir = New-Item -type directory "$target_dir_path"
}

# Builds everything
function Build-All([string] $target_dir_path, [string] $version_suffix, [string] $bcfg)
{
    # Form target path
    $dir = Get-Item "$target_dir_path"
    $target_dir = $dir.FullName
    Write-Host "Will place packages in $target_dir"
    
    # Clean previous build results
    Write-Host "Cleaning previous build"
    & dotnet clean -v minimal -c "$bcfg" | Out-Host
    if ($LastExitCode -ne 0)
    {
        Write-Host "Cleanup failed"
        Return $LastExitCode
    }
    
    # Restore nuget packages
    Write-Host "Restoring NuGet packages"
    & dotnet restore -v minimal | Out-Host
    if ($LastExitCode -ne 0)
    {
        Write-Host "Restoring packages failed"
        Return $LastExitCode
    }
    
    if ($Env:OS -eq $null)
    {
        # Build and package (if Release) in specified configuration but Linux
        Write-Host "Building everything"
        if (-not $version_suffix)
        {
            & .\rebuild-linux.ps1 "$target_dir" -Configuration "$bcfg" | Out-Host
        }
        else
        {
            & .\rebuild-linux.ps1 "$target_dir" -VersionSuffix "$version_suffix" -Configuration "$bcfg" | Out-Host
        }
        if ($LastExitCode -ne 0)
        {
            Write-Host "Packaging failed"
            Return $LastExitCode
        }
    }
    else 
    {
        # Build in specified configuration
        Write-Host "Building everything"
        if (-not $version_suffix)
        {
            & dotnet build -v minimal -c "$bcfg" | Out-Host
        }
        else
        {
            & dotnet build -v minimal -c "$bcfg" --version-suffix "$version_suffix" | Out-Host
        }
        if ($LastExitCode -ne 0)
        {
            Write-Host "Build failed"
            Return $LastExitCode
        }
        
        # Package for NuGet
        Write-Host "Creating NuGet packages"
        if (-not $version_suffix)
        {
            & dotnet pack -v minimal -c "$bcfg" --no-build -o "$target_dir" --include-symbols | Out-Host
        }
        else
        {
            & dotnet pack -v minimal -c "$bcfg" --version-suffix "$version_suffix" --no-build -o "$target_dir" --include-symbols | Out-Host
        }
        if ($LastExitCode -ne 0)
        {
            Write-Host "Packaging failed"
            Return $LastExitCode
        }
    }
    
    Return 0
}

# Check if building a beta package
if ($VersionSuffix)
{
    Write-Host "Building beta package with version suffix of `"$VersionSuffix`""
}

# Prepare environment
Prepare-Environment "$ArtifactLocation"

# Build everything
$BuildResult = Build-All "$ArtifactLocation" "$VersionSuffix" "$Configuration"

# Restore environment
Restore-Environment

# Check if there were any errors
if ($BuildResult -ne 0)
{
    Write-Host "Build failed with code $BuildResult"
    $host.SetShouldExit($BuildResult)
    Exit $BuildResult
}
else
{
    Exit 0
}
