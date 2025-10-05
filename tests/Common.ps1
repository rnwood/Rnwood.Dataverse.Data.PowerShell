BeforeAll {
    $global:__TestsDebugLog = {
        param([string]$Message)
        $ts = Get-Date -Format o
        Write-Host "[TESTDEBUG] $ts - $Message"
    }

    & $global:__TestsDebugLog "BeforeAll starting"
    # Enable WriteVerbose output from cmdlets for debugging
    $global:__OldVerbosePreference = $VerbosePreference
    $VerbosePreference = 'Continue'

    if ($env:TESTMODULEPATH) {
        $source = $env:TESTMODULEPATH
    }
    else {
        $source = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/"
    }

    # Use a cached module folder to avoid repeated copying between isolated test runspaces.
    $moduleCacheRoot = Join-Path -Path ([IO.Path]::GetTempPath()) -ChildPath 'Rnwood.Dataverse.Data.PowerShell_module_cache'
    if (-not (Test-Path $moduleCacheRoot)) {
        New-Item -ItemType Directory -Path $moduleCacheRoot | Out-Null
    }

    # Use a cache key based on build folder last write time so cache refreshes when build changes
    $buildTime = (Get-Item -LiteralPath $source).LastWriteTimeUtc.Ticks
    $cachedModuleFolder = Join-Path $moduleCacheRoot "cache_$buildTime"

    if (-not (Test-Path $cachedModuleFolder)) {
        $copyStart = Get-Date
        New-Item -ItemType Directory -Path $cachedModuleFolder | Out-Null
        & $global:__TestsDebugLog "Copying module from '$source' to cached module folder '$cachedModuleFolder'"
        Copy-Item -Recurse -LiteralPath $source -Destination $cachedModuleFolder\Rnwood.Dataverse.Data.PowerShell
        $copyDuration = (Get-Date) - $copyStart
        & $global:__TestsDebugLog "Module copy completed in $($copyDuration.TotalMilliseconds) ms"
    }

    # Expose cached module to PSModulePath so imports in isolated runspaces find the module
    $tempmodulefolder = $cachedModuleFolder
    $env:PSModulePath = $tempmodulefolder;
    $env:ChildProcessPSModulePath = $tempmodulefolder
     
    # Cache of loaded metadata sets keyed by selection (e.g. 'contact' or 'all')
    $metadataCache = @{}

    function getMockConnection([string[]] $Entities) {
        # Entities: optional list of logical names to load metadata for (e.g. 'contact'). If omitted, all metadata files are loaded.
        $key = if ($Entities -and $Entities.Count -gt 0) { ($Entities | ForEach-Object { $_.ToLowerInvariant() } | Sort-Object) -join ',' } else { 'all' }

        if (-not $metadataCache.ContainsKey($key)) {
            & $global:__TestsDebugLog "getMockConnection invoked and metadata for key '$key' not yet loaded"

            if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)){
                $importStart = Get-Date
                & $global:__TestsDebugLog "Importing module Rnwood.Dataverse.Data.PowerShell"
                Import-Module Rnwood.Dataverse.Data.PowerShell
                $importDuration = (Get-Date) - $importStart
                & $global:__TestsDebugLog "Import-Module completed in $($importDuration.TotalMilliseconds) ms"
            }

            Add-Type -AssemblyName "System.Runtime.Serialization"
            $serializer = New-Object System.Runtime.Serialization.DataContractSerializer([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])

            $allMetadataFiles = Get-Item $PSScriptRoot/metadata/*.xml

            if ($key -ne 'all') {
                # Map requested logical names to filenames (e.g. contact -> contact.xml)
                $requestedNames = $Entities | ForEach-Object { $_.ToLowerInvariant() + '.xml' }
                $metadataFiles = $allMetadataFiles | Where-Object { $requestedNames -contains $_.Name.ToLowerInvariant() }

                if ($metadataFiles.Count -eq 0) {
                    Throw "No metadata files matched requested entities: $($Entities -join ',')"
                }
            }
            else {
                $metadataFiles = $allMetadataFiles
            }

            & $global:__TestsDebugLog "Found $($metadataFiles.Count) metadata files to load for key '$key'"

            $loaded = @()
            $metaLoadStart = Get-Date
            foreach ($f in $metadataFiles) {
                & $global:__TestsDebugLog "Loading metadata file '$($f.Name)' (size: $([Math]::Round(($f.Length/1KB),2)) KB)"
                $stream = [IO.File]::OpenRead($f.FullName)
                $loaded += $serializer.ReadObject($stream)
                $stream.Close()
            }
            $metaLoadDuration = (Get-Date) - $metaLoadStart
            & $global:__TestsDebugLog "Metadata load completed in $($metaLoadDuration.TotalMilliseconds) ms (loaded $($loaded.Count) entities) for key '$key'"

            $metadataCache[$key] = $loaded
        }

        $mock = $metadataCache[$key]
        get-dataverseconnection -url https://fake.crm.dynamics.com/ -mock $mock
    }

    function newPwsh([scriptblock] $scriptblock) {
        if ([System.Environment]::OSVersion.Platform -eq "Unix") {
            pwsh -noninteractive -noprofile -command $scriptblock
        } else {
            cmd /c pwsh -noninteractive -noprofile -command $scriptblock
        }
    }

    AfterEach {
        & $global:__TestsDebugLog "AfterEach invoked"
        # Removing the module after every test forces module re-import and reinitialisation of the mock context
        # which is slow. Make removal conditional; set TEST_REMOVE_MODULES=true to force removal between Its.
        if ($env:TEST_REMOVE_MODULES -eq 'true') {
            & $global:__TestsDebugLog "Removing module Rnwood.Dataverse.Data.PowerShell as TEST_REMOVE_MODULES is true"
            if (Get-Module Rnwood.Dataverse.Data.PowerShell) {
                Remove-Module Rnwood.Dataverse.Data.PowerShell
                & $global:__TestsDebugLog "Module removed"
            }
        }
        else {
            & $global:__TestsDebugLog "Skipping module removal between tests to preserve mock context and speed tests. Set env TEST_REMOVE_MODULES=true to change this behaviour."
        }
    }
    AfterAll {
        # Restore previous verbose preference
        if ($global:__OldVerbosePreference -ne $null) {
            $VerbosePreference = $global:__OldVerbosePreference
            & $global:__TestsDebugLog "VerbosePreference restored to '$VerbosePreference'"
        }
    }
}


