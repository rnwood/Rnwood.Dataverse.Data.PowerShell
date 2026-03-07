# Azure Functions PowerShell - Dataverse Example

This sample demonstrates how to use the `Rnwood.Dataverse.Data.PowerShell` module inside an Azure Function with PowerShell.

## Prerequisites

- [Azure Functions Core Tools v4](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [PowerShell 7.4+](https://aka.ms/powershell-release?tag=stable)
- .NET 8.0 SDK
- Azure CLI (optional, for deployment)

## Local Development Setup

### 1. Configure local settings

Create a `local.settings.json` file in this directory (it is gitignored):

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "powershell",
    "FUNCTIONS_WORKER_RUNTIME_VERSION": "7.4",
    "DATAVERSE_URL": "https://yourorg.crm.dynamics.com",
    "DATAVERSE_CLIENT_ID": "your-client-id",
    "DATAVERSE_CLIENT_SECRET": "your-client-secret"
  }
}
```

### 2. Update the function to use client credentials for local testing

Edit `DataverseTimerTrigger/run.ps1` and replace the `Get-DataverseConnection` call:

```powershell
# For local testing with client credentials:
$connection = Get-DataverseConnection `
    -Url $env:DATAVERSE_URL `
    -ClientId $env:DATAVERSE_CLIENT_ID `
    -ClientSecret $env:DATAVERSE_CLIENT_SECRET
```

### 3. Run the function locally

```bash
# Install dependencies (downloads Rnwood.Dataverse.Data.PowerShell from PowerShell Gallery)
func extensions install

# Start the function host
func start
```

The timer trigger runs every 15 minutes. To trigger it manually via HTTP (for testing):
```bash
curl -X POST http://localhost:7071/admin/functions/DataverseTimerTrigger -H "Content-Type: application/json" -d "{}"
```

## Authentication Options

### Managed Identity (Production - Azure only)
```powershell
$connection = Get-DataverseConnection -Url $env:DATAVERSE_URL -ManagedIdentity
```

### Client Secret (Local development / Service Principal)
```powershell
$connection = Get-DataverseConnection `
    -Url $env:DATAVERSE_URL `
    -ClientId $env:DATAVERSE_CLIENT_ID `
    -ClientSecret $env:DATAVERSE_CLIENT_SECRET
```

### DefaultAzureCredential (Local dev with Azure CLI login)
```powershell
$connection = Get-DataverseConnection -Url $env:DATAVERSE_URL -DefaultAzureCredential
```

## Notes

This sample uses the **requirements.psd1** approach to install the module automatically:

```powershell
@{
    'Rnwood.Dataverse.Data.PowerShell' = '2.*'
}
```

Azure Functions automatically downloads and caches the module on first run when using this approach.

## Troubleshooting

### Assembly load errors (issue #168)

If you encounter `Could not load file or assembly 'Microsoft.Extensions.Logging.Abstractions'` errors, ensure you are using version **2.0.0+** of `Rnwood.Dataverse.Data.PowerShell` which includes the ALC (Assembly Load Context) fix.

This fix ensures the module's dependencies are loaded in an isolated assembly context, preventing conflicts with the Azure Functions worker's pre-loaded assemblies.
