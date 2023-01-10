# Download .NET install script
echo "Downloading .NET install script"
curl -O https://dot.net/v1/dotnet-install.sh

# make install script launchable

chmod u+x ./dotnet-install.sh
# install .NET 
echo "Installing .NET"
./dotnet-install.sh
echo "Installing DocFX"
rm -f ./dotnet-install.sh
echo "Removing temporary files..."
# install docfx preview (so it would work with .NET 6+)
dotnet tool update -g docfx --prerelease

echo "Building docs..."
docfx build ../docs/docfx.json