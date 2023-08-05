<##################################################################################################

This section of the script contains miscellaneous types for later use. Refer to the individual
documentation of each type.

##################################################################################################>

<#
.SYNOPSIS
    Enumerates known kinds of tools; for use in a variety of commands.
#>
enum ToolType {
    Generator
    Analyzer
}

<##################################################################################################

This section of the script contains hashtables and arrays for later use. When adding, removing or 
renaming tools, this section must be kept up to date.

##################################################################################################>

[string[]]$CoreGenerators = @(
    "generate-concrete-objects"
)

[string[]]$CoreAnalyzers = @()

[string[]] $Generators = $CoreGenerators

[string[]] $Analyzers = $CoreAnalyzers

[hashtable]$CoreGeneratorTable = [ordered]@{
    "generate-concrete-objects" = "Tools.Generators.GenerateConcreteObjects"
}

# word of warning: don't rely on GeneratorTable and AnalyzerTable to be ordered correctly
[hashtable]$GeneratorTable = $CoreGeneratorTable

[hashtable]$AnalyzerTable = @{}

[string[]]$AllSubsets = @(
    "core"
)

<##################################################################################################

This section of the script contains functions, to be used and re-used so as to keep the code
more readable. Refer to the individual documentation of each function.

##################################################################################################>

function Get-ScriptHelp {
    Write-Host "This is the primary script controlling the DSharpPlus build."
    Write-Host ""
    Write-Host "Usage: dsharpplus [action] <group> <options>"
    Write-Host ""
    Write-Host "Actions:"
    Write-Host "  run           Runs the given tools or subset of tools."
    Write-Host "  build         Builds the given tools or subset of tools."
    Write-Host "  test          Tests the given tools or subset of tools."
    Write-Host "  publish       Publishes the given subset of the library."
    Write-Host ""
    Write-Host "Groups:"
    Write-Host "Note that groups are only valid when operating on tools" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  tools         Indiscriminately operates on all kinds of tools."
    Write-Host "  generators    Operates on tools intended to generate code or metadata."
    Write-Host "  analyzers     Operates on tools intended to analyze and communicate the validity" +
        "of existing code or metadata"
    Write-Host ""
    Write-Host "Options:"
    Write-Host ""
    Write-Host "  -s|-subset    Specifies one or more subsets to operate on"
    Write-Host "  -n|-names     Specifies the individual names of tools to operate on."
    Write-Host ""
    Write-Host "Examples:"
    Write-Host ""
    Write-Host "The following command will run all core tools:"
    Write-Host "dsharpplus run tools -subset core" -ForegroundColor Gray
    Write-Host ""
    Write-Host "The following command will run all core generators:"
    Write-Host "dsharpplus run generators -subset core" -ForegroundColor Gray
    Write-Host ""
    Write-Host "The following command will only run a single tool, generate-concrete-objects:"
    Write-Host "dsharpplus run tools -name generate-concrete-objects" -ForegroundColor Gray
    Write-Host ""
    Write-Host "The following command will build the core library as well as the caching logic:"
    Write-Host "dsharpplus publish -subset core,cache"
    Write-Host ""
}

function Publish-Subset {
    <#
    .SYNOPSIS
        Publishes and packs a subset of the library, ready to be used.

    .PARAMETER Subset
        The identifier of the subset.

    .INPUTS
        This function accepts the subset from the pipeline.
    #>

    param (
        [Parameter(Position = 0, Mandatory = $true, ValueFromPipeline = $true)]
        [string]$Subset
    )

    dotnet pack "$PSScriptRoot/src/$Subset" --tl
}

function Build-Tools {
    <#
    .SYNOPSIS
        Builds all tools in a given list, in order of specification.
    
    .NOTES
        This function will optionally only rebuild tools that haven't been built yet. This can
        speed up development iteration, but may cause side-effects.

    .PARAMETER Tools
        The table of tools to build. This may be any basic collection or a composite.

    .PARAMETER BuildMissing
        If this switch is set, this function will only build tools that aren't present in the
        tool directory.
    #>

    param (
        [Parameter(Position = 0, Mandatory = $true)]
        [string[]]$Tools,

        [switch]$BuildMissing = $false
    )

    foreach ($Key in $Tools) {
        if ($BuildMissing -and (Test-ToolBuilt -Name $Key)) {
            continue
        }

        if ($Generators.Contains($Key)) {
            Build-Tool -Type Generator -CSProjectName $GeneratorTable[$Key] -OutputName $Key
        }
        elseif ($Analyzers.Contains($Key)) {
            Build-Tool -Type Analyzer -CSProjectName $AnalyzerTable[$Key] -OutputName $Key
        }
        else {
            Write-Error "The tool $Key could not be found."
            continue
        }
    }
}

function Test-Tools {
    <#
    .SYNOPSIS
        Tests all tools in a given list, in order of specification.
    
    .NOTES
        This function relies on Test-Tool to do the heavy lifting, and therefore exhibits all of
        its behavioural side effects.

    .PARAMETER Tools
        The table of tools to test. This may be any basic collection or a composite.
    #>

    param (
        [Parameter(Position = 0, Mandatory = $true)]
        [string[]]$Tools
    )

    process {
        [int]$Passed = 0
        [int]$Failed = 0
        [int]$Skipped = 0
        [int]$Count = $Tools.Count

        foreach ($Key in $Tools) {
            [bool]$Result = $false

            # runs the tool and keeps track of the result, so we can later print it
            if ($Generators.Contains($Key)) {
                $Result = Test-Tool -Type Generator -CSProjectName $GeneratorTable[$Key] -OutputName $Key
            }
            elseif ($Analyzers.Contains($Key)) {
                $Result = Test-Tool -Type Analyzer -CSProjectName $AnalyzerTable[$Key] -OutputName $Key
            }
            else {
                Write-Error "The tool $Key could not be found."
                $Skipped++
                continue
            }

            if ($Result) {
                $Passed++
            }
            else {
                $Failed++
            }
        }
    }

    end {
        Write-Host "-------------------------------------------------------------------------"
        Write-Host ""
        Write-Host "Tool Test Results for $Count tests:"
        Write-Host "$Passed passed" -ForegroundColor Green
        Write-Host "$Failed failed" -ForegroundColor Red
        Write-Host "$Skipped skipped" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "-------------------------------------------------------------------------"
    }
}

function Invoke-Tools {
    <#
    .SYNOPSIS
        Invokes all tools in a given list, in order of specification.
    
    .NOTES
        This function is incapable of passing additional arguments to tools. If you need to
        invoke a tool with additional arguments, you must invoke it directly via Invoke-Tool.

        Because of its reliance on Invoke-Tool, it will similarly build all tools not built yet.

    .PARAMETER Tools
        The table of tools to execute. This may be any basic collection or a composite.
    #>

    param (
        [Parameter(Position = 0, Mandatory = $true)]
        [string[]]$Tools
    )

    foreach($Key in $Tools) {
        Invoke-Tool -ToolName $Key
    }
}

function Invoke-Tool {
    <#
    .SYNOPSIS
        Runs the specified tool, building if it not yet built.
    
    .PARAMETER ToolName
        The common name of this tool.

    .PARAMETER Arguments
        Additional CLI arguments to be passed to the tool, if applicable.

    .INPUTS
        This function accepts the tool name from the pipeline.
    #>

    param (
        [Parameter(Position = 0, ValueFromPipeline = $true, Mandatory = $true)]
        [string]$ToolName,

        [Parameter(Position = 1, ValueFromRemainingArguments = $true)]
        [string]$Arguments
    )

    if (!(Test-ToolBuilt -Name $ToolName)) {
        if ($Generators.Contains($ToolName)) {
            Write-Verbose "The generator $ToolName was not built, attempting to build..."
            Build-Tool -Type Generator -CSProjectName $GeneratorTable[$ToolName] -OutputName $ToolName
        }
        elseif ($Analyzers.Contains($ToolName)) {
            Write-Verbose "The analyzer $ToolName was not built, attempting to build..."
            Build-Tool -Type Analyzer -CSProjectName $AnalyzerTable[$ToolName] -OutputName $ToolName
        }
        else {
            Write-Error "The specified tool does not exist."
            exit
        }
    }

    Invoke-Expression "$PSScriptRoot/tools/bin/$ToolName $Arguments"
}

function Test-Tool {
    <#
    .SYNOPSIS
        Runs the specified test project for a given tool

    .NOTES
        Since tools may choose to take input from the main codebase, but we do not want to operate on
        the main codebase, we allow their test projects to simulate a test environment in the testing
        folder. This call may have unexpected side-effects, and each tool should specify side-effects
        and assumptions by itself.

        Test project names are assumed to be `ToolName.Tests`, for a given tool `ToolName`.

        Because of powershell limitations, all tests must contain a class `ToolName.Tests.Main` with 
        the following static parameterless methods:
        - bool Prepare
        - bool Execute

        It utilizes /tools/.testenv to run the tests; and /tools/.testenvhost to hold the test
        assemblies. /tools/.testenv is cleared after every run.
    
    .PARAMETER Type
        The type of the tool to test.

    .PARAMETER CSProjectName
        The name of the C# project holding the source code for this tool.

    .PARAMETER OutputName
        The 'common' name of this tool; and the name by which it is passed to the test.

    .INPUTS
        This tool accepts the C# project name from the pipeline.
    #>

    [OutputType(bool)]
    param (
        [Parameter(Position = 0, Mandatory = $true)]
        [ToolType]$Type,

        [Parameter(Position = 1, ValueFromPipeline = $true, Mandatory = $true)]
        [string]$CSProjectName,

        [Parameter(Position = 2, Mandatory = $true)]
        [string]$OutputName
    )

    begin {
        # set up the testing environment and prepare the environment
        if (!(Test-Path -Path "$PSScriptRoot/tools/.testenv")) {
            New-Item -ItemType Directory -Path "$PSScriptRoot/tools/.testenv"
        }
        else {
            Remove-Item -Path "$PSScriptRoot/tools/.testenv/*" -Force -Recurse

            Write-Verbose "Cleared the testenv folder of leftover files."
        }

        dotnet build "$PSScriptRoot/tools/tests/$CSProjectName.Tests/$CSProjectName.Tests.csproj" -p:PublishAot=false

        Copy-Item -Path "$PSScriptRoot/tools/artifacts/bin/$CSProjectName.Tests/debug/$CSProjectName.Tests.dll" `
            -Destination "$PSScriptRoot/tools/.testenvhost/"

        Add-Type -Path "$PSScriptRoot/tools/.testenvhost/$CSProjectName.Tests.dll"

        [System.Environment]::CurrentDirectory = "$PSScriptRoot/tools/.testenv/"

        Write-Verbose "Prepared the testing environment; ready to build and test tooling."

        $env:DATA_DIRECTORY = "$PSScriptRoot/tools/.testenv/data"
        $env:SRC_DIRECTORY = "$PSScriptRoot/tools/.testenv/src"
        $env:OUT_DIRECTORY = "$PSScriptRoot/tools/.testenv/out"

        # this should be the last statement in begin, we don't want to ignore prelude errors
        $ErrorActionPreference = 'Continue'
    }

    process {
        [bool]$Success = Invoke-Expression "[$CSProjectName.Tests.Main]::Prepare()"

        if (!($Success)) {
            Write-Verbose "Preparation failed, the test $CSProjectName.Tests will not be executed."

            return $false
        }

        Build-Tool -Type $Type -CSProjectName $CSProjectName -OutputName $OutputName

        if ($IsWindows) {
            Copy-Item -Path "$PSScriptRoot/tools/bin/$OutputName.exe" -Destination "$PSScriptRoot/tools/.testenv/$OutputName.exe"
        }
        else {
            Copy-Item -Path "$PSScriptRoot/tools/bin/$OutputName" -Destination "$PSScriptRoot/tools/.testenv/$OutputName"
        }

        [bool]$Success = Invoke-Expression "[$CSProjectName.Tests.Main]::Execute()"

        if (!($Success)) {
            Write-Verbose "The test $CSProjectName failed."

            return $false
        }

        Write-Verbose "The test $CSProjectName passed."
    }

    end {
        # this should be the first statement in end, we don't want to ignore cleanup errors
        $ErrorActionPreference = 'Stop'

        [System.Environment]::CurrentDirectory = "$PSScriptRoot"
        
        Remove-Item -Path "$PSScriptRoot/tools/.testenv/*" -Force -Recurse

        Write-Verbose "Cleaned up the testing environment."

        return $true
    }
}

function Build-Tool {
    <#
    .SYNOPSIS
        Builds a certain tool and prepares it for use, either manually or by the script.

    .PARAMETER Type
        Specifies the type of this tool.

    .PARAMETER CSProjectName
        The name of the C# project holding the source code for this tool.

    .PARAMETER OutputName
        The 'common' name of this tool, by which it is to be referred to in further use.

    .INPUTS 
        This function accepts the C# project name from the pipeline.

    .NOTES
        This function will automatically create potentially missing directories.
    #>

    param (
        [Parameter(Position = 0, Mandatory = $true)]
        [ToolType]$Type,

        [Parameter(Position = 1, ValueFromPipeline = $true, Mandatory = $true)]
        [string]$CSProjectName,

        [Parameter(Position = 2, Mandatory = $true)]
        [string]$OutputName
    )

    process {
        if (!(Test-Path -Path "$PSScriptRoot/tools/bin")) {
            New-Item -ItemType Directory -Path "$PSScriptRoot/tools/bin"
        }

        switch ($Type) {
            Generator {
                # --ucr == use current runtime; that is, build natively for this system
                dotnet publish "$PSScriptRoot/tools/generators/$CSProjectName/$CSProjectName.csproj" --ucr --tl
            }
            Analyzer {
                dotnet publish "$PSScriptRoot/tools/analyzers/$CSProjectName/$CSProjectName.csproj" --ucr --tl
            }
        }
        
        if ($IsWindows) {
            Copy-Item -Path "$PSScriptRoot/tools/artifacts/publish/$CSProjectName/release/$CSProjectName.exe" `
                -Destination "$PSScriptRoot/tools/bin/$OutputName.exe"
        }
        else {
            Copy-Item -Path "$PSScriptRoot/tools/artifacts/publish/$CSProjectName/release/$CSProjectName" `
                -Destination "$PSScriptRoot/tools/bin/$OutputName"
        }
    }

    end {
        Write-Debug "The tool $OutputName was successfully built from $CSProjectName."
        Write-Verbose "Successfully built tool $OutputName."

        if (Test-Path "$PSScriptRoot/artifacts/hashes/$OutputName.json") {
            Remove-Item -Path "$PSScriptRoot/artifacts/hashes/$OutputName.json"
        }
    }
}


function Test-ToolBuilt {
    <#
    .SYNOPSIS
        Tests whether a certain tool has been built and is ready to run.
    
    .PARAMETER Name
        The 'common' name of this tool, and simultaneously its file name in /tools/bin.

    .INPUTS
        This function accepts the tool name from the pipeline.

    .OUTPUTS
        A boolean indicating whether the tool has been built or not.
    #>

    [OutputType([bool])]
    param (
        [Parameter(Position = 0, ValueFromPipeline = $true, Mandatory = $true)]
        [string]$Name
    )

    process {
        if ($IsWindows) {
            return Test-Path -Path "$PSScriptRoot/tools/bin/$Name.exe"
        }
        return Test-Path -Path "$PSScriptRoot/tools/bin/$Name"
    }
}