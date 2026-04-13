#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates docs/reference/request-handlers.md from handler source files and SDK request types.

.DESCRIPTION
    Discovers request types by reflecting over Dataverse SDK assemblies resolved from
    src/Fake4Dataverse/obj/project.assets.json, then compares with implemented handlers.

.EXAMPLE
    .\scripts\Generate-HandlerDocs.ps1
    .\scripts\Generate-HandlerDocs.ps1 -Check
#>
param(
    [switch]$Check
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$handlersDir = Join-Path $repoRoot 'src/Fake4Dataverse/Handlers'
$assetsPath = Join-Path $repoRoot 'src/Fake4Dataverse/obj/project.assets.json'
$outputPath = Join-Path $repoRoot 'docs/reference/request-handlers.md'

function Normalize-Text {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return ''
    }

    $normalized = $Text -replace '\s+', ' '
    return $normalized.Trim()
}

function Escape-MarkdownTableText {
    param([string]$Text)

    if ($null -eq $Text) {
        return ''
    }

    return (($Text -replace '\|', '\|') -replace '\r?\n', ' ').Trim()
}

function Get-ImplementedRequestMetadata {
    param([string]$HandlersDirectory)

    $implemented = @{}

    foreach ($file in Get-ChildItem $HandlersDirectory -Filter '*.cs' | Sort-Object Name) {
        $content = Get-Content $file.FullName -Raw

        $nameMatches = [regex]::Matches(
            $content,
            'string\.Equals\(\s*request\.RequestName\s*,\s*"([A-Za-z][A-Za-z0-9]+)"\s*,\s*(?:System\.)?StringComparison'
        )

        $names = @()
        foreach ($match in $nameMatches) {
            $names += $match.Groups[1].Value
        }

        $names = $names | Select-Object -Unique
        if ($names.Count -eq 0) {
            continue
        }

        $fidelityMatch = [regex]::Match($content, '<strong>Fidelity:</strong>\s*(Full|Partial|Stub)')
        $fidelity = if ($fidelityMatch.Success) { $fidelityMatch.Groups[1].Value } else { '?' }

        foreach ($name in $names) {
            $implemented[$name] = @{
                Fidelity = $fidelity
                File = $file.Name
            }
        }
    }

    return $implemented
}

function Get-SdkAssemblyPaths {
    param([string]$ProjectAssetsPath)

    if (-not (Test-Path $ProjectAssetsPath)) {
        throw "Could not find project assets file at '$ProjectAssetsPath'. Run 'dotnet restore' first."
    }

    $assets = Get-Content $ProjectAssetsPath -Raw | ConvertFrom-Json
    $packagesPath = [string]$assets.project.restore.packagesPath

    if ([string]::IsNullOrWhiteSpace($packagesPath)) {
        $packageFolderProps = $assets.packageFolders.PSObject.Properties
        if ($packageFolderProps.Count -eq 0) {
            throw 'No NuGet package folders were found in project.assets.json.'
        }

        $packagesPath = [string]$packageFolderProps[0].Name
    }

    $packageRelativePaths = [System.Collections.Generic.List[string]]::new()
    foreach ($library in $assets.libraries.PSObject.Properties) {
        if ($library.Name -match '^(Microsoft\.CrmSdk\.CoreAssemblies|Microsoft\.PowerPlatform\.Dataverse\.Client)/') {
            $relativePath = [string]$library.Value.path
            if (-not [string]::IsNullOrWhiteSpace($relativePath) -and -not $packageRelativePaths.Contains($relativePath)) {
                $packageRelativePaths.Add($relativePath)
            }
        }
    }

    if ($packageRelativePaths.Count -eq 0) {
        throw 'Could not locate Dataverse SDK packages in project.assets.json.'
    }

    $targetFolders = @('lib/net8.0', 'lib/net462')
    $assemblyNames = @('Microsoft.Xrm.Sdk.dll', 'Microsoft.Crm.Sdk.Proxy.dll')
    $selectedAssemblyPathsByName = [ordered]@{}

    foreach ($relativePath in $packageRelativePaths) {
        foreach ($targetFolder in $targetFolders) {
            foreach ($assemblyName in $assemblyNames) {
                $candidate = Join-Path $packagesPath (Join-Path $relativePath (Join-Path $targetFolder $assemblyName))
                if ((Test-Path $candidate) -and -not $selectedAssemblyPathsByName.Contains($assemblyName)) {
                    $selectedAssemblyPathsByName[$assemblyName] = $candidate
                }
            }
        }
    }

    if ($selectedAssemblyPathsByName.Count -eq 0) {
        throw 'Could not resolve Microsoft.Xrm.Sdk / Microsoft.Crm.Sdk.Proxy assemblies from NuGet cache.'
    }

    return @($selectedAssemblyPathsByName.Values)
}

function Get-LoadableTypes {
    param([System.Reflection.Assembly]$Assembly)

    try {
        return $Assembly.GetTypes()
    } catch [System.Reflection.ReflectionTypeLoadException] {
        return $_.Exception.Types | Where-Object { $null -ne $_ }
    }
}

function Get-TypeSummaryMap {
    param([string[]]$AssemblyPaths)

    $summaryMap = @{}

    foreach ($assemblyPath in $AssemblyPaths) {
        $xmlPath = [System.IO.Path]::ChangeExtension($assemblyPath, '.xml')
        if (-not (Test-Path $xmlPath)) {
            continue
        }

        try {
            [xml]$xmlDoc = Get-Content $xmlPath -Raw
        } catch {
            continue
        }

        foreach ($member in $xmlDoc.doc.members.member) {
            $name = [string]$member.name
            if (-not $name.StartsWith('T:', [System.StringComparison]::Ordinal)) {
                continue
            }

            $fullTypeName = $name.Substring(2)
            $summary = Normalize-Text -Text ([string]$member.summary)

            if (-not [string]::IsNullOrWhiteSpace($summary) -and -not $summaryMap.ContainsKey($fullTypeName)) {
                $summaryMap[$fullTypeName] = $summary
            }
        }
    }

    return $summaryMap
}

function Get-RequestNameFromType {
    param([Type]$RequestType)

    $suffix = 'Request'
    if (-not $RequestType.Name.EndsWith($suffix, [System.StringComparison]::Ordinal)) {
        return $null
    }

    try {
        $defaultCtor = $RequestType.GetConstructor([Type[]]@())
        if ($null -ne $defaultCtor) {
            $instance = $defaultCtor.Invoke(@())
            if ($null -ne $instance -and -not [string]::IsNullOrWhiteSpace([string]$instance.RequestName)) {
                return [string]$instance.RequestName
            }
        }
    } catch {
        # Fallback to name convention below.
    }

    return $RequestType.Name.Substring(0, $RequestType.Name.Length - $suffix.Length)
}

function Get-SdkRequestCatalog {
    param([string[]]$AssemblyPaths)

    $loadedAssemblies = [System.Collections.Generic.List[System.Reflection.Assembly]]::new()
    foreach ($assemblyPath in $AssemblyPaths | Select-Object -Unique) {
        $assemblySimpleName = [System.IO.Path]::GetFileNameWithoutExtension($assemblyPath)
        $alreadyLoaded = [System.AppDomain]::CurrentDomain.GetAssemblies() |
            Where-Object { $_.GetName().Name -eq $assemblySimpleName } |
            Select-Object -First 1

        if ($null -ne $alreadyLoaded) {
            $loadedAssemblies.Add($alreadyLoaded) | Out-Null
            continue
        }

        $loadedAssemblies.Add([System.Reflection.Assembly]::LoadFrom($assemblyPath)) | Out-Null
    }

    $xrmAssembly = $loadedAssemblies | Where-Object { $_.GetName().Name -eq 'Microsoft.Xrm.Sdk' } | Select-Object -First 1
    if ($null -eq $xrmAssembly) {
        throw 'Failed to load Microsoft.Xrm.Sdk assembly.'
    }

    $organizationRequestType = $xrmAssembly.GetType('Microsoft.Xrm.Sdk.OrganizationRequest', $false, $false)
    if ($null -eq $organizationRequestType) {
        throw 'Failed to locate Microsoft.Xrm.Sdk.OrganizationRequest type.'
    }

    $typeSummaryMap = Get-TypeSummaryMap -AssemblyPaths $AssemblyPaths
    $catalogByRequestName = @{}

    foreach ($assembly in $loadedAssemblies) {
        foreach ($type in (Get-LoadableTypes -Assembly $assembly)) {
            if ($null -eq $type) {
                continue
            }

            if (-not $type.IsClass -or $type.IsAbstract -or $type.IsGenericTypeDefinition) {
                continue
            }

            if ($type.FullName -eq $organizationRequestType.FullName) {
                continue
            }

            if (-not $organizationRequestType.IsAssignableFrom($type)) {
                continue
            }

            if (-not $type.Name.EndsWith('Request', [System.StringComparison]::Ordinal)) {
                continue
            }

            $requestName = Get-RequestNameFromType -RequestType $type
            if ([string]::IsNullOrWhiteSpace($requestName)) {
                continue
            }

            if ($catalogByRequestName.ContainsKey($requestName)) {
                continue
            }

            $description = if ($typeSummaryMap.ContainsKey($type.FullName)) {
                $typeSummaryMap[$type.FullName]
            } else {
                '—'
            }

            $catalogByRequestName[$requestName] = [pscustomobject]@{
                RequestName = $requestName
                TypeName = $type.Name
                Namespace = $type.Namespace
                Description = $description
            }
        }
    }

    return $catalogByRequestName.Values | Sort-Object Namespace, TypeName
}

$implemented = Get-ImplementedRequestMetadata -HandlersDirectory $handlersDir
$sdkAssemblyPaths = Get-SdkAssemblyPaths -ProjectAssetsPath $assetsPath
$sdkCatalog = Get-SdkRequestCatalog -AssemblyPaths $sdkAssemblyPaths

$sdkByRequestName = @{}
foreach ($entry in $sdkCatalog) {
    $sdkByRequestName[$entry.RequestName] = $entry
}

$total = $sdkCatalog.Count
$implementedInCatalog = @($sdkCatalog | Where-Object { $implemented.ContainsKey($_.RequestName) })
$implementedCount = $implementedInCatalog.Count
$stubCount = @($implementedInCatalog | Where-Object { $implemented[$_.RequestName].Fidelity -eq 'Stub' }).Count
$notImplementedCount = $total - $implementedCount
$implementedOutsideCatalog = @($implemented.Keys | Where-Object { -not $sdkByRequestName.ContainsKey($_) } | Sort-Object)

$sb = [System.Text.StringBuilder]::new()
$null = $sb.AppendLine('# Request Handler Reference')
$null = $sb.AppendLine('')
$null = $sb.AppendLine('This reference compares Dataverse SDK request types with Fake4Dataverse handler support.')
$null = $sb.AppendLine('')
$null = $sb.AppendLine("Fake4Dataverse includes **$implementedCount implemented** request handlers out of **$total** discovered SDK request types.")
$null = $sb.AppendLine("Of those, **$stubCount** are stubs and **$notImplementedCount** are currently not implemented.")
$null = $sb.AppendLine('')

$groupedByNamespace = $sdkCatalog | Group-Object Namespace | Sort-Object Name
foreach ($group in $groupedByNamespace) {
    $namespace = if ([string]::IsNullOrWhiteSpace($group.Name)) { '(global namespace)' } else { $group.Name }

    $null = $sb.AppendLine("## $namespace")
    $null = $sb.AppendLine('')
    $null = $sb.AppendLine('| Request Type | Request Name | Status | Fidelity | Description |')
    $null = $sb.AppendLine('|---|---|---|---|---|')

    foreach ($entry in ($group.Group | Sort-Object TypeName)) {
        if ($implemented.ContainsKey($entry.RequestName)) {
            $status = '✅ Implemented'
            $fidelity = $implemented[$entry.RequestName].Fidelity
        } else {
            $status = '❌ Not implemented'
            $fidelity = '—'
        }

        $requestType = Escape-MarkdownTableText -Text $entry.TypeName
        $requestName = Escape-MarkdownTableText -Text $entry.RequestName
        $description = Escape-MarkdownTableText -Text $entry.Description

        $null = $sb.AppendLine("| ``$requestType`` | ``$requestName`` | $status | $fidelity | $description |")
    }

    $null = $sb.AppendLine('')
}

if ($implementedOutsideCatalog.Count -gt 0) {
    $null = $sb.AppendLine('## Implemented handlers not discovered in reflected SDK types')
    $null = $sb.AppendLine('')
    $null = $sb.AppendLine('| Request Name | Fidelity |')
    $null = $sb.AppendLine('|---|---|')

    foreach ($requestName in $implementedOutsideCatalog) {
        $fidelity = Escape-MarkdownTableText -Text $implemented[$requestName].Fidelity
        $displayName = Escape-MarkdownTableText -Text $requestName
        $null = $sb.AppendLine("| ``$displayName`` | $fidelity |")
    }

    $null = $sb.AppendLine('')
}

$null = $sb.AppendLine('---')
$null = $sb.AppendLine('')
$null = $sb.AppendLine('## Fidelity legend')
$null = $sb.AppendLine('')
$null = $sb.AppendLine('| Level | Meaning |')
$null = $sb.AppendLine('|---|---|')
$null = $sb.AppendLine('| **Full** | Behavior closely matches Dataverse in common scenarios |')
$null = $sb.AppendLine('| **Partial** | Core behavior is supported with some known limitations |')
$null = $sb.AppendLine('| **Stub** | Returns structurally valid but minimal placeholder behavior |')
$null = $sb.AppendLine('| **—** | No built-in handler currently available |')

$newContent = $sb.ToString()

if ($Check) {
    if (-not (Test-Path $outputPath)) {
        Write-Error "docs/reference/request-handlers.md does not exist. Run: pwsh scripts/Generate-HandlerDocs.ps1"
        exit 1
    }

    $existing = Get-Content $outputPath -Raw
    if ($existing.TrimEnd() -ne $newContent.TrimEnd()) {
        Write-Error 'docs/reference/request-handlers.md is out of date. Run: pwsh scripts/Generate-HandlerDocs.ps1'
        exit 1
    }

    Write-Host 'docs/reference/request-handlers.md is up to date.'
} else {
    $newContent | Set-Content $outputPath -Encoding UTF8 -NoNewline
    Write-Host "Generated $outputPath"
    Write-Host "   Implemented: $implementedCount / $total (Stubs: $stubCount, Not implemented: $notImplementedCount)"
}
