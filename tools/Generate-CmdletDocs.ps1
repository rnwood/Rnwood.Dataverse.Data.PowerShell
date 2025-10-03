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
            
            $markdown += @"
### -$($param.Name)
Parameter for the $requestType operation.

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
