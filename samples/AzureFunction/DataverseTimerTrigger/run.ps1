# Azure Functions PowerShell Timer Trigger - Dataverse Example
#
# This function demonstrates using the Rnwood.Dataverse.Data.PowerShell module
# inside an Azure Function to query Dataverse records.
#
# Prerequisites (production):
#   - Azure Function App with System-Assigned Managed Identity enabled
#   - Managed Identity must have appropriate Dataverse permissions
#   - Environment variable: DATAVERSE_URL
#
# Prerequisites (local development):
#   - Environment variables: DATAVERSE_URL, DATAVERSE_CLIENT_ID, DATAVERSE_CLIENT_SECRET
#   - Set DATAVERSE_MODULE_PATH to your locally built module directory
#   - See local.settings.json.example for configuration

param($Timer)

$ErrorActionPreference = 'Stop'

$dataverseUrl = $env:DATAVERSE_URL
if (-not $dataverseUrl) {
    throw 'DATAVERSE_URL environment variable is not set.'
}

Write-Host "Connecting to Dataverse: $dataverseUrl"

# Connect using client credentials when available (local dev), otherwise use Managed Identity (production)
if ($env:DATAVERSE_CLIENT_ID -and $env:DATAVERSE_CLIENT_SECRET) {
    Write-Host "Using client credentials (local development mode)"
    $connection = Get-DataverseConnection -Url $dataverseUrl -ClientId $env:DATAVERSE_CLIENT_ID -ClientSecret $env:DATAVERSE_CLIENT_SECRET
} else {
    Write-Host "Using Managed Identity (production mode)"
    $connection = Get-DataverseConnection -Url $dataverseUrl -ManagedIdentity
}

Write-Host "Connected to Dataverse successfully"

# Query the top 10 accounts
$accounts = Get-DataverseRecord -Connection $connection -TableName account -Top 10 -Columns name, accountnumber, telephone1

Write-Host "Retrieved $($accounts.Count) account(s):"
foreach ($account in $accounts) {
    Write-Host "  - $($account.name) (Account Number: $($account.accountnumber))"
}

Write-Host "Azure Function completed successfully"
