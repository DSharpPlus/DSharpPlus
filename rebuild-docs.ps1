param
(
    [parameter(Mandatory = $true)]
    [string] $docs_path,
    
    [parameter(Mandatory = $true)]
    [string] $output_path
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
    
    # Download release info from GitHub
    Write-Host "Getting latest DocFX release"
    $release_json = Invoke-WebRequest -uri "https://api.github.com/repos/dotnet/docfx/releases" | ConvertFrom-JSON | Sort-Object published_at
    $release = $release_json[0]             # Pick the top (latest release)
    $release_asset = $release.assets[0]     # Pick the first file
    
    # Download the release
    Write-Host "Downloading DocFX to $target_path"
    Invoke-WebRequest -uri "$($release_asset.browser_download_url)" -outfile "$target_path"
    Set-Location -Path "$target_dir"
    
    # Extract the release
    Write-Host "Extracting DocFX"
    Expand-Archive -path "$target_path" -destinationpath "$target_dir"
    
    # Remove the downloaded zip
    Write-Host "Removing temporary files"
    Remove-Item "$target_path"
    
    # Add DocFX to PATH
    Write-Host "Adding DocFX to PATH"
    $Env:PATH = "$target_dir;$current_path"
    Set-Location -path "$current_location"
}

# Downloads and installs latest version of 7-zip CLI
function Install-7zip([string] $target_dir_path)
{
    # First, download 7-zip 9.20 CLI to extract 16.04 CLI
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
    Write-Host "Downloading 7-zip 9.20 CLI to $target_path"
    Invoke-WebRequest -uri "http://www.7-zip.org/a/7za920.zip" -outfile "$target_path"
    Set-Location -Path "$target_dir_920"
    
    # Extract the 9.20 CLI
    Write-Host "Extracting 7-zip 16.04 CLI"
    Expand-Archive -path "$target_path" -destinationpath "$target_dir_920"
    
    # Temporarily add the 9.20 CLI to PATH
    Write-Host "Adding 7-zip 9.20 CLI to PATH"
    $old_path = $Env:PATH
    $Env:PATH = "$target_dir_920;$old_path"
    
    # Next, download 16.04 CLI
    # http://www.7-zip.org/a/7z1604-extra.7z
    
    # Form target path
    $target_fn = "7z1604-extra.7z"
    $target_path = Join-Path "$target_dir" "$target_fn"
    
    # Download the 16.04 CLI
    Write-Host "Downloading 7-zip 16.04 CLI to $target_path"
    Invoke-WebRequest -uri "http://www.7-zip.org/a/7z1604-extra.7z" -outfile "$target_path"
    Set-Location -Path "$target_dir"
    
    # Extract the 16.04 CLI
    Write-Host "Extracting 7-zip 16.04 CLI"
    7za x "$target_path"
    
    # Remove the 9.20 CLI from PATH
    Write-Host "Removing 7-zip 9.20 CLI from PATH"
    $Env:PATH = "$old_path"
    
    # Remove temporary files and 9.20 CLI
    Write-Host "Removing temporary files"
    Remove-Item -recurse -force "$target_dir_920"
    Remove-Item -recurse -force "$target_path"
    
    # Add the 16.04 CLI to PATH
    Write-Host "Adding 7-zip 16.04 CLI to PATH"
    $target_dir = Join-Path "$target_dir" "x64"
    $Env:PATH = "$target_dir;$old_path"
    Set-Location -path "$current_location"
}

# Builds the documentation using available DocFX
function Build-Docs([string] $target_dir_path)
{
    # Check if documentation source path exists
    if (-not (Test-Path "$target_dir_path"))
    {
        throw "Specified path does not exist"
    }
    
    # Check if documentation source path is a directory
    $target_path = Get-Item "$target_dir_path"
    if (-not ($target_path -is [System.IO.DirectoryInfo]))
    {
        throw "Specified path is not a directory"
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
        throw "API build target directory does not exist"
    }
    
    # Check if API documentation source path is a directory
    $docs_api_dir = Get-Item "$docs_api"
    if (-not ($docs_api_dir -is [System.IO.DirectoryInfo]))
    {
        throw "API build target directory is not a directory"
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
    
    # Generate new API documentation
    docfx docfx.json
    
    # Build new documentation site
    docfx build docfx.json
    
    # Exit back
    Set-Location -path "$current_location"
}

# Packages the build site to a .tar.xz archive
function Package-Docs([string] $target_dir_path, [string] $output_dir_path)
{
    # Form target path
    $target_path = Get-Item "$target_dir_path"
    $target_path = $target_path.FullName
    $target_path = Join-Path "$target_path" "_site"
    
    # Form output path
    $output_path_dir = Get-Item "$output_dir_path"
    $output_path_dir = $output_path_dir.FullName
    $output_path = Join-Path "$output_path_dir" "dsharpplus-docs"
    
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
    7za -r a "$output_path.tar" *
    
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
    7za -sdel -mx9 a dsharpplus-docs.tar.xz dsharpplus-docs.tar
    
    # Exit back
    Set-Location -path "$current_location"
}

Install-DocFX "$docfx_path"
Install-7zip "$sevenzip_path"
Build-Docs "$docs_path"
Package-Docs "$docs_path" "$output_path"

Write-Host "All operations completed"
Restore-Environment