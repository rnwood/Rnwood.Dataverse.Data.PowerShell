$ErrorActionPreference = "Stop"

# Module path setup before anything else
if ($env:TESTMODULEPATH) {
    $source = $env:TESTMODULEPATH
}
else {
    $source = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/"
}

$tempmodulefolder = "$([IO.Path]::GetTempPath())/$([Guid]::NewGuid())"
New-Item -ItemType Directory $tempmodulefolder | Out-Null
Copy-Item -Recurse $source $tempmodulefolder/Rnwood.Dataverse.Data.PowerShell
$env:PSModulePath = $tempmodulefolder
$env:ChildProcessPSModulePath = $tempmodulefolder

# Import module early so it's available for all tests
Import-Module Rnwood.Dataverse.Data.PowerShell -Force

# Helper functions in global scope so all tests can access them
$script:metadata = $null

function global:getMockConnection() {
    if (-not $script:metadata) {
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        Add-Type -AssemblyName "System.Runtime.Serialization"

        # Define the DataContractSerializer
        $serializer = New-Object System.Runtime.Serialization.DataContractSerializer([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
    
        Get-Item $PSScriptRoot/*.xml | ForEach-Object {
            $stream = [IO.File]::OpenRead($_.FullName)
            $script:metadata += $serializer.ReadObject($stream)
            $stream.Close()
        }
    }
   
    $mockService = Get-DataverseConnection -url https://fake.crm.dynamics.com/ -mock $script:metadata
    return $mockService
}

function global:newPwsh([scriptblock] $scriptblock) {
    if ([System.Environment]::OSVersion.Platform -eq "Unix") {
        pwsh -noninteractive -noprofile -command $scriptblock
    }
    else {
        cmd /c pwsh -noninteractive -noprofile -command $scriptblock
    }
}

# Dynamically discover and include all test files
# In this repository, test files use *.Tests.ps1 pattern
$testFiles = Get-ChildItem -Path $PSScriptRoot -Filter "*.Tests.ps1" -Recurse | 
    Where-Object { 
        $_.Name -ne "All.Tests.ps1"
    }

foreach ($testFile in $testFiles) {
    . $testFile.FullName
}
