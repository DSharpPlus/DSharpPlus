#!/usr/bin/env pwsh
# Rebuild-docs
#
# Rebuilds the documentation for DSharpPlus project, and places artifacts in specified directory.
#
# Author:       Emzi0767
# Version:      2017-09-11 14:20
#
# Arguments:
#   .\rebuild-docs.ps1 <path to docfx> <output path> <docs project path>
#
# Run as:
#   .\rebuild-docs.ps1 .\path\to\docfx\project .\path\to\output project-docs

param
(
    [parameter(Mandatory = $true)]
    [string] $DocsPath,

    [parameter(Mandatory = $true)]
    [string] $OutputPath,

    [parameter(Mandatory = $true)]
    [string] $PackageName
)

# Backup the environment
$current_path = $Env:PATH
$current_location = Get-Location

# Tool paths
$docfx_path = Join-Path "$current_location" "docfx"
$sevenzip_path = Join-Path "$current_location" "7zip"

# Restores the environment
function Restore-Environment()
{
    Write-Host "Restoring environment variables"
    $Env:PATH = $current_path
    Set-Location -path "$current_location"

    if (Test-Path "$docfx_path")
    {
        Remove-Item -recurse -force "$docfx_path"
    }

    if (Test-Path "$sevenzip_path")
    {
        Remove-Item -recurse -force "$sevenzip_path"
    }
}

# Downloads and installs latest version of DocFX
function Install-DocFX([string] $target_dir_path)
{
    Write-Host "Installing DocFX"

    # Check if the target directory exists
    # If it does, remove it
    if (Test-Path "$target_dir_path")
    {
        Write-Host "Target directory exists, deleting"
        Remove-Item -recurse -force "$target_dir_path"
    }

    # Create target directory
    $target_dir = New-Item -type directory "$target_dir_path"
    $target_fn = "docfx.zip"

    # Form target path
    $target_dir = $target_dir.FullName
    $target_path = Join-Path "$target_dir" "$target_fn"

    # Download release info from Chocolatey API
    try
    {
        Write-Host "Getting latest DocFX release"
        $release_json = Invoke-WebRequest -UseBasicParsing -uri "https://chocolatey.org/api/v2/package-versions/docfx" | ConvertFrom-JSON
        $release_json = $release_json | % { [System.Version]::Parse($_) } | Sort-Object -Descending
    }
    catch
    {
        Return 1
    }

    # Set TLS version to the system default, which should be TLS 1.3
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::SystemDefault

    # Download the release
    # Since GH releases are unreliable, we have to try up to 3 times
    $tries = 0
    $fail = $true
    while ($tries -lt 3)
    {
        # Prepare the assets
        $release = $release_json[$tries]        # Pick the next available release
        $release_version = "2.59.2"  # Convert to string
        $release_asset = "https://github.com/dotnet/docfx/releases/download/v$release_version/docfx.zip"

        # increment try counter
        $tries = $tries + 1

        try
        {
            Write-Host "Downloading DocFX $release_version to $target_path"
            Invoke-WebRequest -UseBasicParsing -uri "$release_asset" -outfile "$target_path"

            # No failure, carry on
            Write-Host "DocFX version $release_version downloaded successfully"
            $fail = $false
            Break
        }
        catch
        {
            Write-Host "Downloading DocFX version $release_version failed, trying next ($tries / 3)"
            #Return 1
        }
    }

    # Check if we succedded in downloading
    if ($fail)
    {
        Return 1
    }

    # Switch directory
    Set-Location -Path "$target_dir"

    # Extract the release
    try
    {
        Write-Host "Extracting DocFX"
        Expand-Archive -path "$target_path" -destinationpath "$target_dir"
    }
    catch
    {
        Return 1
    }

    # Remove the downloaded zip
    Write-Host "Removing temporary files"
    Remove-Item "$target_path"

    # Add DocFX to PATH
    Write-Host "Adding DocFX to PATH"
    if ($Env:OS -eq $null)
    {
        $Env:DOCFX_PATH = "$target_dir"
    }
    else
    {
        $Env:PATH = "$target_dir;$current_path"
    }
    Set-Location -path "$current_location"

    Return 0
}

# Downloads and installs latest version of 7-zip CLI
function Install-7zip([string] $target_dir_path)
{
    # First, download 7-zip 9.20 CLI to extract latest CLI
    # http://www.7-zip.org/a/7za920.zip

    Write-Host "Installing 7-zip"

    # Check if the target directory exists
    # If it does, remove it
    if (Test-Path "$target_dir_path")
    {
        Write-Host "Target directory exists, deleting"
        Remove-Item -recurse -force "$target_dir_path"
    }

    # Create target directory
    $target_dir = New-Item -type directory "$target_dir_path"
    $target_fn = "7za920.zip"

    # Form target path
    $target_dir = $target_dir.FullName
    $target_path = Join-Path "$target_dir" "v920"
    $target_dir_920 = New-Item -type directory "$target_path"

    $target_dir_920 = $target_dir_920.FullName
    $target_path = Join-Path "$target_dir_920" "$target_fn"

    # Download the 9.20 CLI
    try
    {
        Write-Host "Downloading 7-zip 9.20 CLI to $target_path"
        Invoke-WebRequest -UseBasicParsing -uri "http://www.7-zip.org/a/7za920.zip" -outfile "$target_path"
        Set-Location -Path "$target_dir_920"
    }
    catch
    {
        Return 1
    }

    # Extract the 9.20 CLI
    try
    {
        Write-Host "Extracting 7-zip latest CLI"
        Expand-Archive -path "$target_path" -destinationpath "$target_dir_920"
    }
    catch
    {
        Return 1
    }

    # Temporarily add the 9.20 CLI to PATH
    Write-Host "Adding 7-zip 9.20 CLI to PATH"
    $old_path = $Env:PATH
    $Env:PATH = "$target_dir_920;$old_path"

    # Next, download latest CLI
    # http://www.7-zip.org/a/7z1604-extra.7z

    # Form target path
    $target_version = "19.00"
    $target_fn = "7z1900-extra.7z"
    $target_path = Join-Path "$target_dir" "$target_fn"

    # Download the latest CLI
    try
    {
        Write-Host "Downloading 7-zip $target_version CLI to $target_path"
        Invoke-WebRequest -UseBasicParsing -uri "http://www.7-zip.org/a/$target_fn" -outfile "$target_path"
        Set-Location -Path "$target_dir"
    }
    catch
    {
        Return 1
    }

    # Extract the latest CLI
    Write-Host "Extracting 7-zip $target_version CLI"
    & 7za x "$target_path" | Out-Host
    if ($LastExitCode -ne 0)
    {
        Return $LastExitCode
    }

    # Remove the 9.20 CLI from PATH
    Write-Host "Removing 7-zip 9.20 CLI from PATH"
    $Env:PATH = "$old_path"

    # Remove temporary files and 9.20 CLI
    Write-Host "Removing temporary files"
    Remove-Item -recurse -force "$target_dir_920"
    Remove-Item -recurse -force "$target_path"

    # Add the latest CLI to PATH
    Write-Host "Adding 7-zip $target_version CLI to PATH"
    $target_dir = Join-Path "$target_dir" "x64"
    $Env:PATH = "$target_dir;$old_path"
    Set-Location -path "$current_location"

    Return 0
}

# Builds the documentation using available DocFX
function Build-Docs([string] $target_dir_path)
{
    # Check if documentation source path exists
    if (-not (Test-Path "$target_dir_path"))
    {
        Write-Host "Specified path does not exist"
        Return 65536
    }

    # Check if documentation source path is a directory
    $target_path = Get-Item "$target_dir_path"
    if (-not ($target_path -is [System.IO.DirectoryInfo]))
    {
        Write-Host "Specified path is not a directory"
        Return 65536
    }

    # Form target path
    $target_path = $target_path.FullName

    # Form component paths
    $docs_site = Join-Path "$target_path" "_site"
    $docs_api = Join-Path "$target_path" "api"
    $docs_obj = Join-Path "$target_path" "obj"

    # Check if API documentation source path exists
    if (-not (Test-Path "$docs_api"))
    {
        Write-Host "API build target directory does not exist"
        Return 32768
    }

    # Check if API documentation source path is a directory
    $docs_api_dir = Get-Item "$docs_api"
    if (-not ($docs_api_dir -is [System.IO.DirectoryInfo]))
    {
        Write-Host "API build target directory is not a directory"
        Return 32768
    }

    # Purge old API documentation
    Write-Host "Purging old API documentation"
    Set-Location -path "$docs_api"
    Remove-Item "*.yml"
    Set-Location -path "$current_location"

    # Check if old built site exists
    # If it does, remove it
    if (Test-Path "$docs_site")
    {
        Write-Host "Purging old products"
        Remove-Item -recurse -force "$docs_site"
    }

    # Create target directory for the built site
    $docs_site = New-Item -type directory "$docs_site"
    $docs_site = $docs_site.FullName

    # Check if old object cache exists
    # If it does, remove it
    if (Test-Path "$docs_obj")
    {
        Write-Host "Purging object cache"
        Remove-Item -recurse -force "$docs_obj"
    }

    # Create target directory for the object cache
    $docs_obj = New-Item -type directory "$docs_obj"
    $docs_obj = $docs_obj.FullName

    # Enter the documentation directory
    Set-Location -path "$target_path"

    # Check OS
    # Null means non-Windows
    if ($Env:OS -eq $null)
    {
        # Generate new API documentation
        & mono "$Env:DOCFX_PATH/docfx.exe" docfx.json | Out-Host

        # Check if successful
        if ($LastExitCode -eq 0)
        {
            # Build new documentation site
            & mono "$Env:DOCFX_PATH/docfx.exe" build docfx.json | Out-Host
        }
    }
    else
    {
        # Generate new API documentation
        & docfx docfx.json | Out-Host

        # Check if successful
        if ($LastExitCode -eq 0)
        {
            # Build new documentation site
            & docfx build docfx.json | Out-Host
        }
    }

    # Exit back
    Set-Location -path "$current_location"

    # Check if building was a success
    if ($LastExitCode -eq 0)
    {
        Return 0
    }
    else
    {
        Return $LastExitCode
    }
}

# Packages the build site to a .tar.xz archive
function Package-Docs([string] $target_dir_path, [string] $output_dir_path, [string] $pack_name)
{
    # Form target path
    $target_path = Get-Item "$target_dir_path"
    $target_path = $target_path.FullName
    $target_path = Join-Path "$target_path" "_site"

    # Form output path
    $output_path_dir = Get-Item "$output_dir_path"
    $output_path_dir = $output_path_dir.FullName
    $output_path = Join-Path "$output_path_dir" "$pack_name"

    # Enter target path
    Set-Location -path "$target_path"

    # Check if target .tar exists
    # If it does, remove it
    if (Test-Path "$output_path.tar")
    {
        Write-Host "$output_path.tar exists, deleting"
        Remove-Item "$output_path.tar"
    }

    # Package .tar archive
    Write-Host "Packaging docs to $output_path.tar"
    & 7za -r a "$output_path.tar" * | Out-Host

    # Check if prepackaging was a success
    if ($LastExitCode -ne 0)
    {
        Return $LastExitCode
    }

    # Go to package's location
    Set-Location -path "$output_path_dir"

    # Check if target .tar.xz exists
    # If it does, remove it
    if (Test-Path "$output_path.tar.xz")
    {
        Write-Host "$output_path.tar.xz exists, deleting"
        Remove-Item "$output_path.tar.xz"
    }

    # Package .tar.xz
    Write-Host "Packaging docs to $output_path.tar.xz"
    & 7za -sdel -mx9 a "$pack_name.tar.xz" "$pack_name.tar" | Out-Host

    # Exit back
    Set-Location -path "$current_location"

    # Check if packaging was a success
    if ($LastExitCode -eq 0)
    {
        Return 0
    }
    else
    {
        Return $LastExitCode
    }
}

# Install DocFX
$result = Install-DocFX "$docfx_path"
if ($result -ne 0)
{
    Write-Host "Installing DocFX failed"
    Restore-Environment
    $host.SetShouldExit(1)
    Exit 1
}

# Install 7-zip, if Windows
if ($Env:OS -ne $null)
{
    $result = Install-7zip "$sevenzip_path"
    if ($result -ne 0)
    {
        Write-Host "Installing 7-zip failed"
        Restore-Environment
        $host.SetShouldExit(1)
        Exit 1
    }
}

# Build and package docs
# At this point nothing should fail as everything is already set up
$result = Build-Docs "$DocsPath"
if ($result -eq 0)
{
    $result = Package-Docs "$DocsPath" "$OutputPath" "$PackageName"
    if ($result -ne 0)
    {
        Write-Host "Packaging API documentation failed"
    }
}
else
{
    Write-Host "Building API documentation failed"
}

# Restore the environment
Restore-Environment
