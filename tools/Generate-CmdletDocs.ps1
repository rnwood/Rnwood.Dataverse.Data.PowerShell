# Generate Markdown Documentation for Cmdlets
# This creates basic markdown docs for all cmdlets that don't have documentation yet

param(
    [Parameter(Mandatory=$false)]
    [switch]$All
)

$ErrorActionPreference = "Stop"

$docsDir = "./Rnwood.Dataverse.Data.PowerShell/docs"
$cmdletsDir = "./Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands"

# Get all cmdlet files
$cmdletFiles = Get-ChildItem -Path $cmdletsDir -Filter "*Cmdlet.cs"

$generated = 0
$skipped = 0

foreach ($file in $cmdletFiles) {
    # Extract cmdlet name from class name
    $className = $file.BaseName
    $content = Get-Content $file.FullName -Raw
    
    # Extract Cmdlet attribute
    if ($content -match '\[Cmdlet\(([^,]+),\s*"([^"]+)"') {
        $verb = $matches[1] -replace 'Verbs\w+\.', ''
        $noun = $matches[2]
        $cmdletName = "$verb-$noun"
        
        $docFile = Join-Path $docsDir "$cmdletName.md"
        
        # Skip if doc already exists and -All not specified
        if ((Test-Path $docFile) -and -not $All) {
            $skipped++
            continue
        }
        
        # Extract summary from XML comment
        $summary = if ($content -match '///<summary>([^<]+)</summary>') {
            $matches[1].Trim()
        } else {
            "Executes the $cmdletName operation."
        }
        
        # Extract request type
        $requestType = if ($content -match 'new (\w+Request)\(\)') {
            $matches[1]
        } else {
            "OrganizationRequest"
        }
        
        # Extract response type
        $responseType = if ($content -match '\[OutputType\(typeof\((\w+)\)\)\]') {
            $matches[1]
        } else {
            "OrganizationResponse"
        }
        
        # Extract parameters
        $parameters = @()
        $paramMatches = [regex]::Matches($content, '\[Parameter\([^\]]+\)\]\s+public\s+(\w+(?:<[\w,\s]+>)?)\s+(\w+)\s+\{')
        foreach ($match in $paramMatches) {
            $paramType = $match.Groups[1].Value
            $paramName = $match.Groups[2].Value
            
            if ($paramName -ne "Connection") {
                $parameters += @{
                    Name = $paramName
                    Type = $paramType
                }
            }
        }
        
        # Determine if supports ShouldProcess
        $supportsShouldProcess = $content -match 'SupportsShouldProcess\s*=\s*true'
        
        # Generate markdown
        $markdown = @"
# $cmdletName

## SYNOPSIS
$summary

## SYNTAX

``````
$cmdletName -Connection <ServiceClient>
"@
        
        foreach ($param in $parameters) {
            $markdown += " [-$($param.Name) <$($param.Type)>]"
        }
        
        if ($supportsShouldProcess) {
            $markdown += " [-WhatIf] [-Confirm]"
        }
        
        $markdown += @"
 [<CommonParameters>]
``````

## DESCRIPTION

This cmdlet wraps the ``$requestType`` SDK message. It executes the operation through the Dataverse Organization Service.

$summary

### Type Conversion

This cmdlet follows the standard type conversion patterns:

- **EntityReference parameters**: Accept EntityReference objects, PSObjects with Id/TableName properties, or Guid values (with corresponding TableName parameter). Conversion handled by DataverseTypeConverter.ToEntityReference().

- **Entity parameters**: Accept PSObjects representing records. Properties map to attribute logical names. Lookup fields accept Guid/EntityReference/PSObject. Choice fields accept numeric values or string labels. Conversion handled by DataverseEntityConverter.

- **OptionSetValue parameters**: Accept numeric option codes or string labels. Conversion handled by DataverseTypeConverter.ToOptionSetValue().

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.

``````yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
``````

"@
        
        foreach ($param in $parameters) {
            $pipelineInput = if ($content -match "ValueFromPipelineByPropertyName.*\s+public\s+\w+\s+$($param.Name)") { "True (ByPropertyName)" } else { "False" }
            
            # Generate type-specific parameter descriptions based on the conversion patterns
            $paramDescription = "Parameter for the $requestType operation"
            
            if ($param.Type -eq "EntityReference" -or $param.Type -eq "Object") {
                $paramDescription = @"
Reference to a Dataverse record. Can be:
- **EntityReference** object from the SDK
- **PSObject** with Id and TableName properties (e.g., from Get-DataverseRecord)
- **Guid** value (requires corresponding TableName parameter)

The cmdlet uses DataverseTypeConverter to handle the conversion automatically.
"@
            }
            elseif ($param.Type -eq "EntityReference[]" -or $param.Type -eq "Object[]") {
                $paramDescription = @"
Array of references to Dataverse records. Each element can be:
- **EntityReference** object from the SDK
- **PSObject** with Id and TableName properties (e.g., from Get-DataverseRecord)
- **Guid** value (requires corresponding TableName parameter)

The cmdlet uses DataverseTypeConverter to handle the conversion automatically.
"@
            }
            elseif ($param.Type -eq "Entity") {
                $paramDescription = @"
PSObject representing a Dataverse Entity record. Properties should match the logical names of columns in the target table.

The cmdlet converts the PSObject to an Entity object using DataverseEntityConverter, following these rules:
- Property names map to attribute logical names
- Values are converted to appropriate SDK types (Money, EntityReference, OptionSetValue, etc.)
- For lookup fields, accepts Guid, EntityReference, or PSObject with Id and TableName
- For choice fields (picklists), accepts numeric value or string label
"@
            }
            elseif ($param.Type -eq "OptionSetValue") {
                $paramDescription = @"
OptionSet (picklist) value. Can be:
- **Numeric value** (option set integer code)
- **String label** (display name of the option)

The cmdlet uses DataverseTypeConverter to handle the conversion automatically.
"@
            }
            elseif ($param.Name -match "TableName|EntityName|EntityLogicalName") {
                $paramDescription = "Logical name of the Dataverse table (entity). Required when providing Guid values for record references instead of EntityReference or PSObject."
            }
            
            $markdown += @"
### -$($param.Name)
$paramDescription

``````yaml
Type: $($param.Type)
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: $pipelineInput
Accept wildcard characters: False
``````

"@
        }
        
        if ($supportsShouldProcess) {
            $markdown += @"
### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run.

``````yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
``````

### -Confirm
Prompts you for confirmation before running the cmdlet.

``````yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
``````

"@
        }
        
        $markdown += @"
### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### $responseType

Returns the response from the ``$requestType`` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
"@
        
        # Write to file
        Set-Content -Path $docFile -Value $markdown -Encoding UTF8
        Write-Host "Generated: $cmdletName.md"
        $generated++
    }
}

Write-Host "`nSummary:"
Write-Host "  Generated: $generated"
Write-Host "  Skipped: $skipped"
Write-Host "  Total cmdlets: $($cmdletFiles.Count)"
