param(
    [Parameter(Mandatory=$true, HelpMessage="ServiceClient connection to Dataverse")]
    [Microsoft.PowerPlatform.Dataverse.Client.ServiceClient]$Connection
)

$ErrorActionPreference = "Stop"

Write-Host "================================" -ForegroundColor Cyan
Write-Host " Entity Metadata Generator" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Entities required for comprehensive test coverage
# Default set of entities to collect metadata for. Add or remove entries as needed for your environment/tests.
$entities = @(
    "account",
    "contact",
    "lead",
    "opportunity",
    "incident",
    "task",
    "email",
    "phonecall",
    "appointment",
    "campaign",
    "list",
    "solution",
    "systemuser",
    "team",
    "workflow",
    "asyncoperation",
    "organization",
    "duplicaterule",
    "processstage",
    "savedquery",
    "userquery",
    "annotation",
    "activitypointer",
    "activityparty",
    "systemuserroles"
)

Write-Host "Will generate metadata for $($entities.Count) entities:" -ForegroundColor Yellow
$entities | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }
Write-Host ""

Add-Type -AssemblyName "System.Runtime.Serialization"
$serializer = New-Object System.Runtime.Serialization.DataContractSerializer([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])

$successCount = 0
$failCount = 0
$totalSize = 0

foreach($entityname in $entities) {
    Write-Host "[$($entities.IndexOf($entityname) + 1)/$($entities.Count)] Retrieving $entityname..." -NoNewline -ForegroundColor Cyan
    
    try {
        # Retrieve complete entity metadata
        $em = (Invoke-DataverseRequest -Connection $Connection -RequestName "RetrieveEntity" -Parameters @{
            "LogicalName" = $entityname
            "EntityFilters" = [Microsoft.Xrm.Sdk.Metadata.EntityFilters]::All
            "MetadataId" = [Guid]::Empty
            "RetrieveAsIfPublished" = $false
        }).Results["EntityMetadata"]
        
        # Serialize to XML
        $outputPath = Join-Path $PSScriptRoot/metadata "${entityname}.xml"
        $outputStream = [IO.File]::OpenWrite($outputPath)
        $serializer.WriteObject($outputStream, $em)
        $outputStream.Close()
        
        # Report success
        $fileSize = (Get-Item $outputPath).Length
        $totalSize += $fileSize
        $sizeKB = [Math]::Round($fileSize / 1KB, 2)
        $attrCount = $em.Attributes.Count
        
        Write-Host " ✓" -ForegroundColor Green
        Write-Host "    Size: $sizeKB KB | Attributes: $attrCount | Path: $outputPath" -ForegroundColor Gray
        
        $successCount++
    }
    catch {
        Write-Host " ✗" -ForegroundColor Red
        Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
        $failCount++
    }
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host " Summary" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host "  Success: $successCount" -ForegroundColor Green
Write-Host "  Failed:  $failCount" -ForegroundColor $(if ($failCount -gt 0) { "Red" } else { "Gray" })
Write-Host "  Total size: $([Math]::Round($totalSize / 1MB, 2)) MB" -ForegroundColor Cyan
Write-Host ""

if ($successCount -gt 0) {
    Write-Host "Metadata files created in: $PSScriptRoot/metadata" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Run tests: Invoke-Pester -Path tests/Examples.Tests.ps1 -Output Detailed" -ForegroundColor Gray
    Write-Host "  2. Commit files: git add tests/metadata/*.xml && git commit -m 'Add entity metadata'" -ForegroundColor Gray
    Write-Host ""
}

if ($failCount -gt 0) {
    Write-Host "Some entities failed to retrieve metadata. This may be normal if they don't exist in your environment." -ForegroundColor Yellow
    Write-Host "IMPORTANT: Tests that require metadata files for those entities will FAIL until you add the corresponding XML files under tests/metadata/." -ForegroundColor Yellow
    Write-Host "To include them, re-run this script against the missing entities or supply their logical names to the script." -ForegroundColor Yellow
}
