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
# Use global variable for metadata cache - stores metadata by entity name
if (-not $global:TestMetadataCache) {
    $global:TestMetadataCache = @{}
}

function global:getMockConnection([ScriptBlock]$RequestInterceptor = $null, [string[]]$Entities = @("contact")) {
    # Load metadata for requested entities if not already loaded
    $metadata = @()
    foreach ($entityName in $Entities) {
        if (-not $global:TestMetadataCache.ContainsKey($entityName)) {
            LoadTestMetadata -EntityName $entityName
        }
        $metadata += $global:TestMetadataCache[$entityName]
    }
   
    # Create the connection (no caching for test isolation)
    $mockService = Get-DataverseConnection -url https://fake.crm.dynamics.com/ -mock $metadata -RequestInterceptor $RequestInterceptor
    return $mockService
}

function global:LoadTestMetadata {
    param(
        [Parameter(Mandatory=$true)]
        [string]$EntityName
    )
    
    if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
        Import-Module Rnwood.Dataverse.Data.PowerShell
    }
    
    Add-Type -AssemblyName "System.Runtime.Serialization"

    # Define the DataContractSerializer
    $serializer = New-Object System.Runtime.Serialization.DataContractSerializer([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])

    $xmlFile = "$PSScriptRoot/$EntityName.xml"
    if (Test-Path $xmlFile) {
        Write-Verbose "Loading metadata for entity: $EntityName..."
        $stream = [IO.File]::OpenRead($xmlFile)
        $metadata = $serializer.ReadObject($stream)
        $stream.Close()
        $global:TestMetadataCache[$EntityName] = $metadata
        Write-Verbose "Metadata loaded for entity: $EntityName"
    }
    else {
        throw "Metadata file not found: $xmlFile - Entity '$EntityName' metadata must be available for testing. Available entities: $(Get-ChildItem $PSScriptRoot/*.xml | ForEach-Object { [System.IO.Path]::GetFileNameWithoutExtension($_.Name) } | Join-String -Separator ', ')"
    }
}

function global:newPwsh([scriptblock] $scriptblock) {
    if ([System.Environment]::OSVersion.Platform -eq "Unix") {
        pwsh -noninteractive -noprofile -command $scriptblock
    }
    else {
        cmd /c pwsh -noninteractive -noprofile -command $scriptblock
    }
}
