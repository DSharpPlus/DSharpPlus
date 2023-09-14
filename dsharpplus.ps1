<##################################################################################################

This script serves as the main entrypoint for the DSharpPlus build tooling. It should, at all
times, be targeted towards windows and linux simultaneously; which means keeping the script OS-
agnostic where possible and special-casing certain points as needed.

It is tasked with the following aims:

- run all or only a selection of tools, under `run tools`
  - run all or only a selection of generating tools, under `run generators`
  - run all or only a selection of analyzing tools, under `run analyzers`
- build all or only a selection of tools without running, under `build tools`
- test all or only a selection of tools, under `test tools`

- publish specific 'areas', under `publish -subset <name>`

##################################################################################################>

#requires -Version 6

<#
.PARAMETER Help
    Indicates whether to display the help output.

.PARAMETER Action
    The primary action to perform.

.PARAMETER ToolGroup
    The group of tools to execute this action on.

.PARAMETER Subset
    The subset of libraries or tools to execute this action on.

.PARAMETER Names
    The granular names of tools to execute this action on.
#>

param (
    [Alias('h')]
    [Alias('?')]
    [switch]$Help,

    [Parameter(Position = 0)]
    [ValidateSet("run", "build", "test", "publish")]
    [string]$Action,

    [Parameter(Position = 1)]
    [ValidateSet("tools", "generators", "analyzers")]
    [string]$ToolGroup,

    [Alias('s')]
    [ValidateSet("core", "etf")]
    [string[]]$Subset,

    [Alias('n')]
    [string[]]$Names
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/script-functions.ps1"

if ($Help) {
    Get-ScriptHelp
    exit 0
}

if($Names.Count -gt 0) {
    switch ($Action) {
        "run" {
            Invoke-Tools -Tools $Names
        }
        "build" {
            Build-Tools -Tools $Names
        }
        "test" {
            Test-Tools -Tools $Names
        }
    }

    exit 0
}

if ((![System.String]::IsNullOrWhiteSpace($ToolGroup)) -and ($Subset.Count -le 0)) {
    [string[]]$ToolsToExecute = @()

    switch ($ToolGroup) {
        "tools" {
            # this must be in order, as analyzers can operate on generated code
            $ToolsToExecute = $Generators + $Analyzers
        }
        "generators" {
            $ToolsToExecute = $Generators
        }
        "analyzers" {
            $ToolsToExecute = $Analyzers
        }
    }

    switch ($Action) {
        "run" {
            Invoke-Tools -Tools $ToolsToExecute
        }
        "build" {
            Build-Tools -Tools $ToolsToExecute
        }
        "test" {
            Test-Tools -Tools $ToolsToExecute
        }
    }

    exit 0
}

[string[]]$SubsetsToExecute = @()

if ($Subset.Count -gt 0) {
   $SubsetsToExecute = $Subset
}
else {
    $SubsetsToExecute = $AllSubsets
}

if ($Action -eq "publish") {
    foreach($Target in $SubsetsToExecute) {
        Write-Host "Publishing library subset '$Target'"
        Publish-Subset $Target
    }

    exit 0
}

[string[]]$ToolsToExecute = @()

# construct the list of tools we want to execute
# this looks a little scuffed, so, explanation:
#
# firstly, we filter by tool group.
# then, the tool group dictating what tools to add,
# we foreach over the supplied subsets to add the
# tools
switch ($ToolGroup) {
    "tools" {
        foreach ($Target in $SubsetsToExecute) {
            switch ($Target) {
                "core" {
                    $ToolsToExecute += $CoreGenerators
                    $ToolsToExecute += $CoreAnalyzers
                }
            }
        }
    }
    "generators" {
        foreach ($Target in $SubsetsToExecute) {
            switch ($Target) {
                "core" {
                    $ToolsToExecute += $CoreGenerators
                }
            }
        }
    }
    "analyzers" {
        foreach ($Target in $SubsetsToExecute) {
            switch ($Target) {
                "core" {
                    $ToolsToExecute += $CoreAnalyzers
                }
            }
        }
    }
}

switch ($Action) {
    "run" {
        Invoke-Tools -Tools $ToolsToExecute
    }
    "build" {
        Build-Tools -Tools $ToolsToExecute
    }
    "test" {
        Test-Tools -Tools $ToolsToExecute
    }
}
