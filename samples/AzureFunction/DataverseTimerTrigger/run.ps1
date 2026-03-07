# Azure Functions PowerShell Timer Trigger - Dataverse Example
#
# This function demonstrates using the Rnwood.Dataverse.Data.PowerShell module
# inside an Azure Function to query Dataverse records.
#
# Prerequisites:
#   - Azure Function App with System-Assigned Managed Identity enabled
#   - Managed Identity must have appropriate Dataverse permissions
#   - Environment variables: DATAVERSE_URL
#
# For local testing with client credentials, set:
#   - DATAVERSE_URL, DATAVERSE_CLIENT_ID, DATAVERSE_CLIENT_SECRET

param($Timer)

$ErrorActionPreference = 'Stop'

$dataverseUrl = $env:DATAVERSE_URL
if (-not $dataverseUrl) {
    throw 'DATAVERSE_URL environment variable is not set.'
}

Write-Host "Connecting to Dataverse: $dataverseUrl"

# Connect using Managed Identity (preferred for production Azure Functions)
# For local testing, use client credentials instead:
#   $connection = Get-DataverseConnection -Url $dataverseUrl -ClientId $env:DATAVERSE_CLIENT_ID -ClientSecret $env:DATAVERSE_CLIENT_SECRET
$connection = Get-DataverseConnection -Url $dataverseUrl -ManagedIdentity

Write-Host "Connected to Dataverse successfully"

# Query the top 10 accounts
$accounts = Get-DataverseRecord -Connection $connection -TableName account -Top 10 -Columns name, accountnumber, telephone1

Write-Host "Retrieved $($accounts.Count) account(s):"
foreach ($account in $accounts) {
    Write-Host "  - $($account.name) (Account Number: $($account.accountnumber))"
}

Write-Host "Azure Function completed successfully"
