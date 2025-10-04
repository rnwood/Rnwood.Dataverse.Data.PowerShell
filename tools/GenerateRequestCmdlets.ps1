param(
    [string]$OutputDirectory = "$PSScriptRoot\..\Rnwood.Dataverse.Data.PowerShell.Cmdlets\Commands",
    [string]$AssemblyPath = "$PSScriptRoot\..\Rnwood.Dataverse.Data.PowerShell.Cmdlets\bin\Release\net462\Microsoft.PowerPlatform.Dataverse.Client.dll",
    [string[]]$OnlyRequests
)

# Metadata used to mark generated files/classes
$generatorName = 'GenerateRequestCmdlets.ps1'
$generatorVersion = '1.0.0'

# Ensure output directory exists
$sdkDirectory = Join-Path $OutputDirectory "sdk"
if (!(Test-Path $sdkDirectory)) {
    New-Item -ItemType Directory -Path $sdkDirectory -Force
}

# Clear existing generated files in the sdk directory before generating new ones
$deletedCount = 0
try {
    if ($OnlyRequests -and $OnlyRequests.Count -gt 0) {
        # Remove only the generated files that correspond to the specified request names
        $removed = 0
        foreach ($req in $OnlyRequests) {
            $shortName = $req -replace 'Request$',''
            $targetFile = Join-Path $sdkDirectory "InvokeDataverse${shortName}Cmdlet.cs"
            if (Test-Path $targetFile) {
                try { Remove-Item -Path $targetFile -Force -ErrorAction Stop; $removed++ } catch { Write-Warning ([string]::Format('Failed to remove {0}: {1}', $targetFile, $_.Exception.Message)) }
            }
        }
        if ($removed -gt 0) { Write-Host "Removed $removed existing generated files from $sdkDirectory"; $deletedCount = $removed }
    } else {
        $existingFiles = Get-ChildItem -Path $sdkDirectory -File -Include '*.cs' -Recurse -ErrorAction SilentlyContinue
        if ($existingFiles -and $existingFiles.Count -gt 0) {
            Write-Host "Removing $($existingFiles.Count) existing generated files from $sdkDirectory"
            foreach ($file in $existingFiles) {
                try {
                    Remove-Item -Path $file.FullName -Force -ErrorAction Stop
                    $deletedCount++
                } catch {
                    Write-Warning "Failed to remove $($file.FullName): $($_.Exception.Message)"
                }
            }
        }
    }
} catch {
    Write-Warning "Error while attempting to clear existing files: $($_.Exception.Message)"
}

    # Load the assembly with better error handling
    if (!(Test-Path $AssemblyPath)) {
        Write-Error "Assembly not found at $AssemblyPath. Please build the project first."
        exit 1
    }

    # Set up assembly resolver to handle dependencies
    $assemblyDir = Split-Path $AssemblyPath
    $Script:AssemblyDir = $assemblyDir

    try {
        Write-Host "Importing module manifest to initialize assembly resolution and load contexts"
        # Prefer the built module in bin/Release/netstandard2.0 where nested loader DLLs are present
        $builtManifestPath = Join-Path $PSScriptRoot "..\Rnwood.Dataverse.Data.PowerShell\bin\Release\netstandard2.0\Rnwood.Dataverse.Data.PowerShell.psd1"
        $moduleManifest = Resolve-Path $builtManifestPath -ErrorAction SilentlyContinue
        if (-not $moduleManifest) {
            # Fallback to project manifest location
            $moduleManifest = Resolve-Path (Join-Path $PSScriptRoot "..\Rnwood.Dataverse.Data.PowerShell\Rnwood.Dataverse.Data.PowerShell.psd1") -ErrorAction SilentlyContinue
        }
        if ($moduleManifest) {
            Write-Host "Importing module: $($moduleManifest.Path)"
            Import-Module -Name $moduleManifest.Path -Force -ErrorAction Stop
            Write-Host "Module imported successfully"
        } else {
            Write-Warning "Module manifest not found; attempting to load specified assembly directly: $AssemblyPath"
            if (Test-Path $AssemblyPath) {
                try {
                    [System.Reflection.Assembly]::LoadFrom($AssemblyPath) | Out-Null
                    Write-Host "Loaded assembly: $AssemblyPath"
                } catch {
                    Write-Error ([string]::Format('Failed to load assembly {0}: {1}', $AssemblyPath, $_))
                    exit 1
                }
            } else {
                Write-Error "Neither module manifest nor assembly were available to load. Module manifest expected at: $PSScriptRoot\..\Rnwood.Dataverse.Data.PowerShell\Rnwood.Dataverse.Data.PowerShell.psd1"
                exit 1
            }
        }

        # After import, examine currently loaded assemblies to find OrganizationRequest
        $organizationRequestType = $null
        foreach ($assembly in [System.AppDomain]::CurrentDomain.GetAssemblies()) {
            try {
                $type = $assembly.GetType('Microsoft.Xrm.Sdk.OrganizationRequest', $false, $false)
                if ($type) { $organizationRequestType = $type; Write-Host "Found OrganizationRequest type in: $($assembly.GetName().Name)"; break }
            } catch {
                # continue
            }
        }

        if (-not $organizationRequestType) {
            Write-Error "Could not find Microsoft.Xrm.Sdk.OrganizationRequest type in any loaded assembly after importing module"
            exit 1
        }
    } catch {
        Write-Error "Failed to initialize module or load assemblies: $_"
        exit 1
    }

# Load XML documentation files
$xmlDocs = @{}
$assemblyDir = Split-Path $AssemblyPath
$xmlFiles = Get-ChildItem $assemblyDir -Filter "*.xml" | Where-Object { $_.Name -like "*Xrm*" -or $_.Name -like "*Crm*" -or $_.Name -like "*Dataverse*" }
foreach ($xmlFile in $xmlFiles) {
    try {
        $xml = [xml](Get-Content $xmlFile.FullName)
        $xmlDocs[$xmlFile.BaseName] = $xml
        Write-Host "Loaded XML docs from $($xmlFile.Name)"
    } catch {
        Write-Warning "Failed to load XML docs from $($xmlFile.Name): $_"
    }
}

# Function to get help message from XML docs
function Get-HelpMessage {
    param([System.Reflection.PropertyInfo]$property)
    # Build the canonical member name used in XML docs. Normalize nested type separators
    $declaringTypeName = $property.DeclaringType.FullName -replace '\+', '.'
    $exactMemberName = "P:$declaringTypeName.$($property.Name)"

    foreach ($xml in $xmlDocs.Values) {
        # Try exact match first
        $member = $null
        try { $member = $xml.doc.members.member | Where-Object { $_.name -eq $exactMemberName } } catch { }

        # Fallback: match by suffix (type may be represented differently in XML, e.g. generics/nested types)
        if (-not $member) {
            try {
                # Prefer matches that include the declaring type's simple name or namespace
                $candidates = $xml.doc.members.member | Where-Object { $_.name -like "*.$($property.Name)" }
                if ($candidates -and $candidates.Count -gt 0) {
                    $preferred = $candidates | Where-Object { $_.name -match [regex]::Escape($property.DeclaringType.Name) -or $_.name -match [regex]::Escape($property.DeclaringType.Namespace) }
                    if ($preferred -and $preferred.Count -gt 0) {
                        $member = $preferred[0]
                    } else {
                        # Use first suffix match as last resort
                        $member = $candidates[0]
                    }
                }
            } catch { }
        }

        if ($member) {
            $summary = $member.summary
            if ($summary) {
                # Clean up the summary text - handle different XML structures
                $text = $summary.'#text'
                if (!$text -and $summary.InnerText) { 
                    $text = $summary.InnerText 
                }
                if (!$text) { 
                    $text = $summary.ToString()
                }
                if ($text) {
                    # Clean up whitespace and normalize line breaks
                    $text = $text -replace '\s+', ' '
                    $text = $text.Trim()
                    # Escape quotes for C# string literals
                    $text = $text.Replace('"', '""')
                    return $text
                }
            }
        }
    }

    return $null
}

# Function to get enhanced help message for PSObject conversion properties
function Get-EnhancedHelpMessage {
    param(
        [System.Reflection.PropertyInfo]$property,
        [string]$conversionType
    )
    
    $baseHelp = Get-HelpMessage $property
    $additionalHelp = ""
    
    switch ($conversionType) {
        "Entity" {
            $additionalHelp = "Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type."
        }
        "EntityReference" {
            $additionalHelp = "Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name."
        }
    }
    
    if ($baseHelp -and $additionalHelp) {
        return "$baseHelp $additionalHelp"
    } elseif ($baseHelp) {
        return $baseHelp
    } elseif ($additionalHelp) {
        return $additionalHelp
    }
    
    return $null
}

# Function to get class summary from XML docs
function Get-ClassSummary {
    param([Type]$type)
    $declaringTypeName = $type.FullName -replace '\+', '.'
    $exactMemberName = "T:$declaringTypeName"

    foreach ($xml in $xmlDocs.Values) {
        # Try exact match first
        $member = $null
        try { $member = $xml.doc.members.member | Where-Object { $_.name -eq $exactMemberName } } catch { }

        # Fallback: match by type name suffix
        if (-not $member) {
            try {
                $candidates = $xml.doc.members.member | Where-Object { $_.name -like "T:*$($type.Name)" }
                if ($candidates -and $candidates.Count -gt 0) {
                    $preferred = $candidates | Where-Object { $_.name -match [regex]::Escape($type.Namespace) }
                    if ($preferred -and $preferred.Count -gt 0) { $member = $preferred[0] } else { $member = $candidates[0] }
                }
            } catch { }
        }

        if ($member) {
            $summary = $member.summary
            if ($summary) {
                # Clean up the summary text - handle different XML structures
                $text = $summary.'#text'
                if (!$text -and $summary.InnerText) { 
                    $text = $summary.InnerText 
                }
                if (!$text) { 
                    $text = $summary.ToString()
                }
                if ($text) {
                    # Clean up whitespace and normalize line breaks
                    $text = $text -replace '\s+', ' '
                    $text = $text.Trim()
                    # For class summaries we want the full XML help preserved.
                    # Escape XML entities so the text can be safely inserted into generated /// <summary> comments.
                    $text = $text -replace '&', '&amp;'
                    $text = $text -replace '<', '&lt;'
                    $text = $text -replace '>', '&gt;'
                    return $text
                }
            }
        }
    }
    return $null
}

# Returns $true if the given type is marked as deprecated/obsolete.
# Temporarily disable approved verbs rule for this helper function. The function name intentionally uses the noun 'Deprecated' for readability.
# PSScriptAnalyzer disable=PSUseApprovedVerbs
function Test-DeprecatedType {
    param([Type]$type)

    if (-not $type) { return $false }

    # 1) Check for ObsoleteAttribute on the type itself

    # 1) Check for ObsoleteAttribute on the type itself
    try {
        if ($type.IsDefined([System.ObsoleteAttribute], $false)) {
            return $true
        }
    } catch {
        # Ignore any reflection errors and continue to XML doc checks
    }

    # 2) Check XML documentation for <obsolete> or <deprecated> tags or summary text mentioning deprecation
    $declaringTypeName = $type.FullName -replace '\+', '.'
    $exactMemberName = "T:$declaringTypeName"
    foreach ($xml in $xmlDocs.Values) {
        try {
            $member = $null
            try { $member = $xml.doc.members.member | Where-Object { $_.name -eq $exactMemberName } } catch { }

            if (-not $member) {
                $candidates = $xml.doc.members.member | Where-Object { $_.name -like "T:*$($type.Name)" }
                if ($candidates -and $candidates.Count -gt 0) {
                    $preferred = $candidates | Where-Object { $_.name -match [regex]::Escape($type.Namespace) }
                    if ($preferred -and $preferred.Count -gt 0) { $member = $preferred[0] } else { $member = $candidates[0] }
                }
            }

            if ($member) {
                if ($member.obsolete -or $member.deprecated) {
                    return $true
                }

                # Fallback: check summary text for words like 'deprecated' or 'obsolete'
                $summary = $member.summary
                if ($summary) {
                    $text = $summary.'#text'
                    if (!$text -and $summary.InnerText) { $text = $summary.InnerText }
                    if ($text -and ($text -match '(?i)deprecat|obsolete')) {
                        return $true
                    }
                }
            }
        } catch {
            # Ignore any XML parsing issues for a particular XML file and continue
        }
    }

    return $false
}
# PSScriptAnalyzer enable=PSUseApprovedVerbs

# Find all OrganizationRequest types
Write-Host "Searching for request types..."
$requestTypes = @()

# Get all assemblies that might contain request types
$relevantAssemblies = [System.AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { 
    $name = $_.GetName().Name
    $name -like "*Xrm*" -or $name -like "*Crm*" -or $name -like "*Dataverse*"
}

Write-Host "Searching in assemblies: $($relevantAssemblies.GetName().Name -join ', ')"

foreach ($assembly in $relevantAssemblies) {
    try {
        $types = $assembly.GetTypes() | Where-Object { 
            $_.IsSubclassOf($organizationRequestType) -and 
            !$_.IsAbstract -and 
            $_.IsPublic 
        }
        $requestTypes += $types
        if ($types.Count -gt 0) {
            Write-Host "Found $($types.Count) request types in $($assembly.GetName().Name)"
        }
    } catch [System.Reflection.ReflectionTypeLoadException] {
        Write-Warning "Could not load all types from $($assembly.GetName().Name): $($_.Exception.Message)"
        # Try to get the types that did load
        $loadedTypes = $_.Exception.Types | Where-Object { $_ -ne $null }
        $validTypes = $loadedTypes | Where-Object { 
            $_ -and 
            $_.IsSubclassOf($organizationRequestType) -and 
            !$_.IsAbstract -and 
            $_.IsPublic 
        }
        $requestTypes += $validTypes
        if ($validTypes.Count -gt 0) {
            Write-Host "Found $($validTypes.Count) request types in $($assembly.GetName().Name) (partial load)"
        }
    } catch {
        Write-Warning "Error loading types from $($assembly.GetName().Name): $($_.Exception.Message)"
    }
}

Write-Host "Found $($requestTypes.Count) request types"

$generatedCount = 0
$skippedDeprecated = 0
# Property-level help overrides for request types when XML docs are unavailable
$PropertyHelpOverrides = @{
    'SetBusinessEquipmentRequest' = @{
        'EquipmentId'    = 'Gets or sets the ID of the equipment (facility/equipment).'
        'BusinessUnitId' = 'Gets or sets the ID of the business unit.'
    }
}

# Class-level summary overrides for request types when XML class docs are unavailable
$ClassSummaryOverrides = @{
    'SetBusinessEquipmentRequest' = 'Deprecated. Contains the data that is needed to assign equipment (facility/equipment) to a specific business unit.'
}
foreach ($requestType in $requestTypes) {
    $requestName = $requestType.Name
    # Skip deprecated/obsolete request types
    if (Test-DeprecatedType $requestType) {
        Write-Verbose "Skipping $($requestType.FullName) - marked as deprecated/obsolete"
        $skippedDeprecated++
        continue
    }
    if ($requestName -notlike "*Request") { 
        Write-Verbose "Skipping $requestName - does not end with 'Request'"
        continue 
    }
    $cmdletName = $requestName -replace "Request$", ""
    $cmdletClassName = "InvokeDataverse${cmdletName}Cmdlet"

    # Use fully qualified type name to avoid ambiguities
    $requestTypeName = $requestType.FullName

    Write-Host "Generating $cmdletClassName for $requestName ($($generatedCount + 1)/$($requestTypes.Count))"
    Write-Verbose "  Request type: $requestTypeName"

    # Get public properties declared on this type (not inherited)
    if ($TestMode -and $TestRequestName) {
        $properties = $thisProps | Where-Object { $_.CanWrite -and $_.Name -ne "ExtensionData" -and $_.Name -ne "Parameters" -and $_.Name -ne "RequestId" -and $_.Name -ne "RequestName" }
    } else {
        $properties = $requestType.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) |
            Where-Object { $_.CanWrite -and $_.Name -ne "ExtensionData" -and $_.Name -ne "Parameters" -and $_.Name -ne "RequestId" -and $_.Name -ne "RequestName" }
    }

        # Generate parameter declarations
        $parameterDeclarations = @()
        # Collect structured parameter info for docs generation
        $parameterInfos = @()
    foreach ($prop in $properties) {
        $paramName = $prop.Name
        $paramType = $prop.PropertyType.FullName

        # Handle generic types
        if ($prop.PropertyType.IsGenericType) {
            $genericType = $prop.PropertyType.GetGenericTypeDefinition()
            if ($genericType -eq [System.Collections.Generic.IEnumerable`1]) {
                $elementType = $prop.PropertyType.GetGenericArguments()[0]
                $paramType = $elementType.FullName + "[]"
            } elseif ($genericType -eq [System.Nullable`1]) {
                $underlyingType = $prop.PropertyType.GetGenericArguments()[0]
                if ($underlyingType -eq [System.Boolean]) {
                    $paramType = "bool?"
                } elseif ($underlyingType -eq [System.Int32]) {
                    $paramType = "int?"
                } elseif ($underlyingType -eq [System.Guid]) {
                    $paramType = "System.Guid?"
                } else {
                    $paramType = "$($underlyingType.FullName)?"
                }
            } else {
                $paramType = $prop.PropertyType.ToString().Replace("`1[", "<").Replace("]", ">").Replace("`2[", "<").Replace("]", ">")
            }
        }

        # Make Target mandatory for common requests and allow pipeline binding
        $mandatory = ""
        $valueFromPipeline = ""
        if ($paramName -eq "Target" -or $paramName -eq "Targets") {
            $mandatory = "true"
            # Allow the Target(s) to be provided via the pipeline (by value or by property name)
            $valueFromPipeline = "true"
        }

    # Get help message from XML docs
        $helpMessage = $null
        
        if ($paramType -eq "Microsoft.Xrm.Sdk.Entity") {
            $helpMessage = Get-EnhancedHelpMessage $prop "Entity"
        } elseif ($paramType -eq "Microsoft.Xrm.Sdk.EntityReference") {
            $helpMessage = Get-EnhancedHelpMessage $prop "EntityReference"
        } else {
            $helpMessage = Get-HelpMessage $prop
        }

        # No TestMode overrides; rely on XML docs or inference

        # If no help message was found, try property-level overrides then provide a reasonable default for common patterns
        if (-not $helpMessage) {
            if ($PropertyHelpOverrides.ContainsKey($requestName) -and $PropertyHelpOverrides[$requestName].ContainsKey($paramName)) {
                $helpMessage = $PropertyHelpOverrides[$requestName][$paramName]
            }
        }

        # Provide inference-based defaults for commonly-seen parameter naming patterns so we always emit
        # an XML <summary> and a HelpMessage attribute. This reduces missing-XML-comment warnings (CS1591).
        $inferredHelp = $null
        if (-not $helpMessage) {
            # Guid Id pattern: Parameter names ending with 'Id' are commonly GUIDs that reference another record
            if ($paramType -match "Guid" -or $paramType -match "System.Guid") {
                if ($paramName -like "*Id") {
                    $inferred = $paramName -replace 'Id$',''
                    # Turn PascalCase into space-separated words
                    $inferredPretty = [regex]::Replace($inferred, '([a-z])([A-Z])', '$1 $2')
                    $inferredPretty = $inferredPretty -replace '_', ' '
                    $inferredPretty = $inferredPretty.Trim()
                    $inferredHelp = "Gets or sets the ID of the $inferredPretty."
                }
            }

            # Common parameter name heuristics
            if (-not $inferredHelp) {
                switch -Regex ($paramName) {
                    '^(Target|Targets)$' {
                        $inferredHelp = 'Gets or sets the target entity (or collection of targets) for this request.'
                    }
                    'ColumnSet$' {
                        $inferredHelp = 'Specifies which columns/attributes to retrieve.'
                    }
                    'Query$' {
                        $inferredHelp = 'Specifies the query used to select records for this operation.'
                    }
                    '^(LogicalName|EntityLogicalName|EntityName|TableName)$' {
                        $inferredHelp = 'The logical name of the table/entity.'
                    }
                    'Count|Size|PageSize|PageNumber' {
                        $inferredHelp = "Specifies the $paramName for paging or sizing results."
                    }
                    default {
                        # As a last resort, provide a neutral getter/setter style description
                        $inferredHelp = "Gets or sets the $paramName for the request."
                    }
                }
            }

            if ($inferredHelp) { $helpMessage = $inferredHelp }
        }

        # Build parameter attribute with proper syntax
        $paramAttribute = "Parameter"
        $paramAttributeContent = @()
        
        if ($mandatory -eq "true") {
            $paramAttributeContent += "Mandatory = true"
        }

        if ($valueFromPipeline -eq "true") {
            # Accept values from the pipeline (whole object) and by property name
            $paramAttributeContent += "ValueFromPipeline = true"
            $paramAttributeContent += "ValueFromPipelineByPropertyName = true"
        }
        
        if ($helpMessage) {
            # HelpMessage used in attribute must escape double quotes - generator's Get-HelpMessage already does this for XML docs; for inferred messages we ensure they are safe
            $helpMessageForAttribute = $helpMessage.Replace('"','""')
            $paramAttributeContent += "HelpMessage = `"$helpMessageForAttribute`""
        }
        
        if ($paramAttributeContent.Count -gt 0) {
            $paramAttribute += "(" + ($paramAttributeContent -join ", ") + ")"
        }

        # Prepare XML summary comment for the property based on the help message (unescape quotes and escape XML entities)
        $xmlSummary = $null
        if ($helpMessage) {
            $helpForXml = $helpMessage.Replace('""','"')
            $helpForXml = $helpForXml -replace '&', '&amp;'
            $helpForXml = $helpForXml -replace '<', '&lt;'
            $helpForXml = $helpForXml -replace '>', '&gt;'
            $xmlSummary = "        /// <summary>`n        /// $helpForXml`n        /// </summary>"
        }

        if ($paramType -eq "Microsoft.Xrm.Sdk.Entity" -or $paramType -eq "Microsoft.Xrm.Sdk.EntityReference") {
            if ($xmlSummary) { $parameterDeclarations += $xmlSummary }
            $parameterDeclarations += "        [$paramAttribute]`n        public PSObject $paramName { get; set; }"
            # Add parameter info for the PSObject-converted parameter
            $parameterInfos += @{ Name = $paramName; Type = 'PSObject'; Help = $helpMessage; Mandatory = ($mandatory -eq "true"); ValueFromPipeline = ($valueFromPipeline -eq "true") }
            
            if ($paramType -eq "Microsoft.Xrm.Sdk.Entity") {
                # Generate helper parameters for Entity conversion
                # Combine the original XML doc help (if any) with the conversion-specific helper text
                $entityBaseHelp = Get-HelpMessage $prop

                $tableNameAdditional = "The logical name of the table/entity type for the $paramName parameter."
                $ignorePropsAdditional = "Properties to ignore when converting $paramName PSObject to Entity."
                $lookupColsAdditional = "Hashtable specifying lookup columns for entity reference conversions in $paramName."

                if ($entityBaseHelp) {
                    $tableNameHelp = "$entityBaseHelp $tableNameAdditional"
                    $ignorePropsHelp = "$entityBaseHelp $ignorePropsAdditional"
                    $lookupColsHelp = "$entityBaseHelp $lookupColsAdditional"
                } else {
                    $tableNameHelp = $tableNameAdditional
                    $ignorePropsHelp = $ignorePropsAdditional
                    $lookupColsHelp = $lookupColsAdditional
                }

                # Build Parameter attributes with escaped quotes
                $tableNameAttribute = "Parameter"
                $tableNameContent = @()
                if ($mandatory -eq "true") { $tableNameContent += "Mandatory = true" }
                $tableNameHelpAttr = $tableNameHelp.Replace('"','""')
                $tableNameContent += "HelpMessage = `"$tableNameHelpAttr`""
                $tableNameAttribute += "(" + ($tableNameContent -join ", ") + ")"

                $ignorePropsHelpAttr = $ignorePropsHelp.Replace('"','""')
                $ignorePropsAttribute = "Parameter(HelpMessage = `"$ignorePropsHelpAttr`")"

                $lookupColsHelpAttr = $lookupColsHelp.Replace('"','""')
                $lookupColsAttribute = "Parameter(HelpMessage = `"$lookupColsHelpAttr`")"

                # Add XML summaries for helper parameters (escape XML entities)
                $tableNameXml = $tableNameHelp -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;'
                $ignorePropsXml = $ignorePropsHelp -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;'
                $lookupColsXml = $lookupColsHelp -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;'

                $parameterDeclarations += "        /// <summary>`n        /// $tableNameXml`n        /// </summary>"
                $parameterDeclarations += "        [$tableNameAttribute]`n        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]`n        public string ${paramName}TableName { get; set; }"
                $parameterDeclarations += "        /// <summary>`n        /// $ignorePropsXml`n        /// </summary>"
                # Attach column-name completion for ignore-properties helper so users get column name suggestions
                $parameterDeclarations += "        [$ignorePropsAttribute]`n        [ArgumentCompleter(typeof(Rnwood.Dataverse.Data.PowerShell.Commands.PSObjectPropertyNameArgumentCompleter))]`n        public string[] ${paramName}IgnoreProperties { get; set; }"
                $parameterDeclarations += "        /// <summary>`n        /// $lookupColsXml`n        /// </summary>"
                # Do NOT attach ColumnNamesArgumentCompleter to Hashtable parameters.
                # Hashtable values are complex key/value mappings and PowerShell completion for
                # ColumnNamesArgumentCompleter is not appropriate when the parameter is a Hashtable
                # (completion would run for the whole hashtable value which is confusing).
                # Keep the LookupColumns helper parameter but do not add the completer attribute.
                $parameterDeclarations += "        [$lookupColsAttribute]`n        public Hashtable ${paramName}LookupColumns { get; set; }"
                # Add helper parameters to parameter info so docs include them
                $parameterInfos += @{ Name = "${paramName}TableName"; Type = "String"; Help = $tableNameHelp; Mandatory = ($mandatory -eq "true"); ValueFromPipeline = $false }
                $parameterInfos += @{ Name = "${paramName}IgnoreProperties"; Type = "String[]"; Help = $ignorePropsHelp; Mandatory = $false; ValueFromPipeline = $false }
                $parameterInfos += @{ Name = "${paramName}LookupColumns"; Type = "Hashtable"; Help = $lookupColsHelp; Mandatory = $false; ValueFromPipeline = $false }
            }
        } else {
            if ($xmlSummary) { $parameterDeclarations += $xmlSummary }
            $parameterDeclarations += "        [$paramAttribute]`n        public $paramType $paramName { get; set; }"
            # Add parameter info for non-entity parameters
            $displayType = $paramType
            if ($displayType -like "System.*") {
                # Simplify common system type names for docs
                switch -Regex ($displayType) {
                    "System\.String(\[\])?" { $displayType = $displayType -replace 'System\.',''; $displayType = $displayType -replace '\[\]','[]' }
                    "System\.Guid" { $displayType = 'Guid' }
                    "System\.Int32" { $displayType = 'Int32' }
                    "System\.Boolean" { $displayType = 'Boolean' }
                    default { $displayType = ($displayType.Split('.') | Select-Object -Last 1) }
                }
            } elseif ($displayType -like "Microsoft.PowerPlatform.Dataverse.Client*" -or $displayType -like "ServiceClient") {
                $displayType = 'ServiceClient'
            } else {
                # Use the last token of the type name as a friendly name
                $displayType = ($displayType.Split('.') | Select-Object -Last 1)
            }
            $parameterInfos += @{ Name = $paramName; Type = $displayType; Help = $helpMessage; Mandatory = ($mandatory -eq "true"); ValueFromPipeline = ($valueFromPipeline -eq "true") }
        }
    }

    # Generate property assignments
    $assignments = @()
    foreach ($prop in $properties) {
        if ($prop.PropertyType.FullName -eq "Microsoft.Xrm.Sdk.Entity" -or $prop.PropertyType.FullName -eq "Microsoft.Xrm.Sdk.EntityReference") {
            if ($prop.PropertyType.FullName -eq "Microsoft.Xrm.Sdk.Entity") {
                $assignments += "            { var options = new ConvertToDataverseEntityOptions(); if ($($prop.Name)IgnoreProperties != null) { foreach (string prop in $($prop.Name)IgnoreProperties) { options.IgnoredPropertyName.Add(prop); } } if ($($prop.Name)LookupColumns != null) { foreach (DictionaryEntry entry in $($prop.Name)LookupColumns) { options.ColumnOptions[(string)entry.Key] = new ConvertToDataverseEntityColumnOptions() { LookupColumn = (string)entry.Value }; } } request.$($prop.Name) = entityConverter.ConvertToDataverseEntity($($prop.Name), $($prop.Name)TableName, options); }"
            } else {
                $assignments += "            request.$($prop.Name) = DataverseEntityConverter.ConvertPSObjectToEntityReference($($prop.Name));"
            }
        } else {
            # Direct assignment without null checks
            $assignments += "            request.$($prop.Name) = $($prop.Name);"
        }
    }

    # Check if any properties require entity conversion (Entity type, not EntityReference)
    $needsEntityConverter = $false
    foreach ($prop in $properties) {
        if ($prop.PropertyType.FullName -eq "Microsoft.Xrm.Sdk.Entity") {
            $needsEntityConverter = $true
            break
        }
    }

    # Check if any properties are EntityReference (for using statements)
    $needsEntityReference = $false
    foreach ($prop in $properties) {
        if ($prop.PropertyType.FullName -eq "Microsoft.Xrm.Sdk.EntityReference") {
            $needsEntityReference = $true
            break
        }
    }

    # Generate using statements
    $usingStatements = @(
        "using System;",
        "using System.Text;",
        "using System.Management.Automation;",
        "using Microsoft.Xrm.Sdk;",
        "using Microsoft.Xrm.Sdk.Messages;",
        "using Microsoft.Crm.Sdk.Messages;",
        "using Microsoft.PowerPlatform.Dataverse.Client;",
        "using Rnwood.Dataverse.Data.PowerShell.Commands;"
    )
    
    if ($needsEntityConverter -or $needsEntityReference) {
        $usingStatements += "using Microsoft.Xrm.Sdk.Metadata;"
        $usingStatements += "using System.Collections;"
    }

    # Generate field declarations
    $fieldDeclarations = @()
    if ($needsEntityConverter) {
        $fieldDeclarations += "        private EntityMetadataFactory entityMetadataFactory;"
        $fieldDeclarations += "        private DataverseEntityConverter entityConverter;"
    }

    # Generate BeginProcessing method
    $beginProcessingMethod = @()
    if ($needsEntityConverter) {
        $beginProcessingMethod += "        protected override void BeginProcessing()"
        $beginProcessingMethod += "        {"
        $beginProcessingMethod += "            base.BeginProcessing();"
        $beginProcessingMethod += "            entityMetadataFactory = new EntityMetadataFactory(Connection);"
        $beginProcessingMethod += "            entityConverter = new DataverseEntityConverter(Connection, entityMetadataFactory);"
        $beginProcessingMethod += "        }"
    }

    $summaryComment = "" 
    $summary = Get-ClassSummary $requestType
    if (-not $summary -and $ClassSummaryOverrides.ContainsKey($requestName)) {
        # Use the override and escape XML entities
        $summary = $ClassSummaryOverrides[$requestName] -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;'
    }
    if ($summary) {
        $summaryComment = @"
    /// <summary>
    /// $summary
    /// </summary>
"@
    } else {
        # Provide a default summary if no XML documentation is found
        $summaryComment = @"
    /// <summary>
    /// Executes a $requestName against the Dataverse organization service.
    /// </summary>
"@
    }

    # Ensure the XML summary is placed before attributes (required by C# XML comment rules)
    $classSummary = $summaryComment
    if (-not $classSummary) { $classSummary = "" }

    # Connection property XML comment - use double quotes so `n becomes an actual newline
    $connectionSummary = "        /// <summary>`n        /// DataverseConnection instance obtained from Get-DataverseConnection cmdlet`n        /// </summary>"

    

    # ProcessRecord XML comment
    # ProcessRecord XML comment - create with actual newlines
    $processRecordSummary = "        /// <summary>`n        /// Processes the cmdlet request and writes the response to the pipeline.`n        /// </summary>"

    $code = @"
// <auto-generated>
//     This code was generated by $generatorName (version $generatorVersion) on $(Get-Date -Format o).
//     Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

$($usingStatements -join "`n")

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Sdk
{
$classSummary
    [System.CodeDom.Compiler.GeneratedCode("$generatorName", "$generatorVersion")]
    [System.Diagnostics.DebuggerNonUserCode]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Cmdlet("Invoke", "Dataverse$cmdletName", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class $cmdletClassName : OrganizationServiceCmdlet
    {
        $connectionSummary
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        $($fieldDeclarations -join "`n")

        $($beginProcessingMethod -join "`n")

        $($parameterDeclarations -join "`n`n")

        $processRecordSummary
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new $requestTypeName();
            $($assignments -join "`n")

            // Build a short serialized summary of the request for ShouldProcess output
            var requestSummary = new StringBuilder();
            try
            {
                var props = request.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var p in props)
                {
                    object val = null;
                    try { val = p.GetValue(request); } catch { val = null; }
                    var vstr = val == null ? "null" : val.ToString();
                    requestSummary.Append(p.Name).Append("=").Append(vstr).Append("; ");
                }
            }
            catch
            {
                // Ignore any errors while serializing the request for what-if messaging
            }

            // Emit verbose output of the serialized request when -Verbose is used
            try { WriteVerbose(requestSummary.ToString()); } catch { }

            // Support -WhatIf and -Confirm; include request summary in the action description
            if (!ShouldProcess(request.GetType().Name, requestSummary.ToString()))
            {
                return;
            }

            var response = Connection.Execute(request);
            WriteObject(response);
        }
    }
}

#pragma warning restore CS1591
"@

    # Write to file
    $outputFile = Join-Path $sdkDirectory "$cmdletClassName.cs"
    try {
        $code | Out-File -FilePath $outputFile -Encoding UTF8
        Write-Verbose "  Generated: $outputFile"
        $generatedCount++
    } catch {
        Write-Error "Failed to write $outputFile`: $_"
    }
    
    # --- Generate markdown help file for this cmdlet ---
    try {
        $toolsRoot = $PSScriptRoot
        $docsDirectory = Resolve-Path (Join-Path $toolsRoot "..\Rnwood.Dataverse.Data.PowerShell\docs") -ErrorAction SilentlyContinue
        if (-not $docsDirectory) { New-Item -ItemType Directory -Path (Join-Path $toolsRoot "..\Rnwood.Dataverse.Data.PowerShell\docs") -Force | Out-Null; $docsDirectory = Resolve-Path (Join-Path $toolsRoot "..\Rnwood.Dataverse.Data.PowerShell\docs") }
        $docsDirectory = $docsDirectory.Path

        $docFileName = "Invoke-Dataverse$cmdletName.md"
        $docPath = Join-Path $docsDirectory $docFileName

        # Build SYNOPSIS and DESCRIPTION from class summary if available
        $synopsisText = $summary -replace '\s+',' '
        if (-not $synopsisText) { $synopsisText = "Executes a $requestName against the Dataverse organization service." }

        # Ensure Connection parameter is present in parameter infos
        if (-not ($parameterInfos | Where-Object { $_.Name -eq 'Connection' })) {
            $parameterInfos = ,@{ Name = 'Connection'; Type = 'ServiceClient'; Help = 'DataverseConnection instance obtained from Get-DataverseConnection cmdlet'; Mandatory = $true; ValueFromPipeline = $false } + $parameterInfos
        }

        # Build SYNTAX line
        $syntaxParams = @()
        # Always include Connection first
        $syntaxParams += "-Connection <ServiceClient>"
        foreach ($p in $parameterInfos) {
            if ($p.Name -eq 'Connection') { continue }
            $syntaxParams += "-$($p.Name) <$($p.Type)>"
        }
        $syntaxLine = "Invoke-Dataverse$cmdletName " + ($syntaxParams -join ' ')

        $docLines = @()
        $docLines += '---'
        $docLines += 'external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml'
        $docLines += 'Module Name: Rnwood.Dataverse.Data.PowerShell'
        $docLines += 'online version:'
        $docLines += 'schema: 2.0.0'
        $docLines += '---'
        $docLines += ''
        $docLines += "# Invoke-Dataverse$cmdletName"
        $docLines += ''
        $docLines += '## SYNOPSIS'
        $docLines += $synopsisText
        # Add link to Microsoft Learn for the request type
        try {
            $requestFullName = $requestType.FullName -replace '\+','.'
            $docLines += ''
            $docLines += "[Microsoft Learn: $requestFullName](https://learn.microsoft.com/dotnet/api/$requestFullName)"
        } catch {
            # ignore if request type cannot be determined
        }
        $docLines += ''
        $docLines += '## SYNTAX'
        $docLines += ''
        $docLines += '```'
        $docLines += $syntaxLine
        $docLines += '```'
        $docLines += ''
        $docLines += '## DESCRIPTION'
        $docLines += $synopsisText
        $docLines += ''
        $docLines += '## EXAMPLES'
        $docLines += ''
        $docLines += '### Example 1'
        $docLines += '```powershell'
        $docLines += "PS C:\> $($syntaxLine)"
        $docLines += '```'
        $docLines += ''
        $docLines += '## PARAMETERS'
        $docLines += ''
        foreach ($p in $parameterInfos) {
            $docLines += "### -$($p.Name)"
            $helpText = $p.Help
            if (-not $helpText) { $helpText = "{{ Fill $($p.Name) Description }}" }
            $docLines += $helpText
            $docLines += ''
            $docLines += '```yaml'
            $docLines += "Type: $($p.Type)"
            $docLines += "Parameter Sets: (All)"
            $docLines += 'Aliases:'
            $required = if ($p.Mandatory) { 'True' } else { 'False' }
            $docLines += "`nRequired: $required"
            $docLines += 'Position: Named'
            $docLines += 'Default value: None'
            $pipelineInput = if ($p.ValueFromPipeline) { 'True' } else { 'False' }
            $docLines += "Accept pipeline input: $pipelineInput"
            $docLines += 'Accept wildcard characters: False'
            $docLines += '```'
            $docLines += ''
        }
    $docLines += '### CommonParameters'
    $docLines += 'This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).'
    $docLines += ''
    $docLines += 'Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.'
        $docLines += ''
        $docLines += '## INPUTS'
        $docLines += ''
        $docLines += '### None'
        $docLines += '## OUTPUTS'
        $docLines += ''
        # Try to determine the response type for this request (e.g., CreateRequest -> CreateResponse)
        $responseTypeName = $requestName -replace 'Request$','Response'
        $responseType = $null
        # 1) Try the same assembly/namespace as the request type
        try {
            $possibleFullName = "$($requestType.Namespace).$responseTypeName"
            $responseType = $requestType.Assembly.GetType($possibleFullName, $false, $false)
        } catch {
            $responseType = $null
        }

        # 2) If not found, try a lightweight GetType(fullname) across loaded assemblies
        if (-not $responseType) {
            foreach ($asm in [System.AppDomain]::CurrentDomain.GetAssemblies()) {
                try {
                    $candidate = $asm.GetType(("$($requestType.Namespace).$responseTypeName"), $false, $false)
                    if ($candidate) { $responseType = $candidate; break }
                } catch {
                    # ignore assemblies that cannot be queried
                }
            }
        }

        # 3) Fallback to OrganizationResponse or System.Object
        if (-not $responseType) {
            try { $responseType = [Type]::GetType('Microsoft.Xrm.Sdk.OrganizationResponse') } catch { $responseType = $null }
        }

        if ($responseType) {
            $responseFullName = $responseType.FullName -replace '\+','.'
            $docLines += "### $responseFullName"
            $docLines += "[Microsoft Learn: $responseFullName](https://learn.microsoft.com/dotnet/api/$responseFullName)"
        } else {
            $docLines += '### System.Object'
        }
        $docLines += '## NOTES'
        $docLines += ''
        $docLines += '## RELATED LINKS'

        # Write the doc file (overwrite if exists)
        $docLines -join "`n" | Out-File -FilePath $docPath -Encoding UTF8
        Write-Verbose "  Generated docs: $docPath"
    } catch {
        Write-Warning ([string]::Format('Failed to generate docs for {0}: {1}', $cmdletClassName, $_))
    }
}

Write-Host "Generation complete. Generated $generatedCount of $($requestTypes.Count) request types. Skipped $skippedDeprecated deprecated/obsolete types. Removed $deletedCount existing generated files."

# No explicit assembly resolver cleanup required when using module import
