$ErrorActionPreference = "Stop"

# Only run setup once - check if already initialized
if (-not $global:TestsInitialized) {
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

    # Mark as initialized
    $global:TestsInitialized = $true
}

# Helper functions in global scope so all tests can access them
# Use global variable for metadata so it's only loaded once across all test files
if (-not $global:TestMetadata) {
    $global:TestMetadata = @()
}

function global:getMockConnection([ScriptBlock]$RequestInterceptor = $null) {
    if ($global:TestMetadata.Count -eq 0) {
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        Add-Type -AssemblyName "System.Runtime.Serialization"

        # Define the DataContractSerializer
        $serializer = New-Object System.Runtime.Serialization.DataContractSerializer([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
    
        Get-Item $PSScriptRoot/*.xml | ForEach-Object {
            $stream = [IO.File]::OpenRead($_.FullName)
            $metadata = $serializer.ReadObject($stream)
            $stream.Close()
            $global:TestMetadata = $global:TestMetadata + @($metadata)
        }
    }
   
    $mockService = Get-DataverseConnection -url https://fake.crm.dynamics.com/ -mock $global:TestMetadata -RequestInterceptor $RequestInterceptor
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
