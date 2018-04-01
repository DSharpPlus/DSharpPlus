#!/usr/bin/env pwsh
# Rebuild-Micro
# 
# Rebuilds the test project only.
# 
# Author:       Emzi0767
# Version:      2018-02-03 18:18
# 
# Arguments: 
#  .\rebuild-micro.ps1
# 
# Run as:
#  .\rebuild-micro.ps1

Set-Location "DSharpPlus.Test"
dotnet build -c Debug -f netcoreapp2.0
Set-Location ".."