# Azure Functions profile.ps1
#
# This profile.ps1 will get executed every 'cold start' of your Azure Functions app.
# 'cold start' occurs when:
#
# * A Function App instance is first started
# * A Function App instance is started after being de-allocated

# For local development: if DATAVERSE_MODULE_PATH is set, import from that path.
# In production, the module is installed via requirements.psd1 (managed dependencies).
if ($env:DATAVERSE_MODULE_PATH) {
    Write-Host "Importing Rnwood.Dataverse.Data.PowerShell from local path: $env:DATAVERSE_MODULE_PATH"
    Import-Module "$env:DATAVERSE_MODULE_PATH/Rnwood.Dataverse.Data.PowerShell.psd1" -Force -ErrorAction Stop
} else {
    Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop
}

# Authenticate with Azure PowerShell using MSI.
# Remove this if you are not planning on using MSI or Azure PowerShell.
if ($env:MSI_SECRET) {
    Disable-AzContextAutosave -Scope Process | Out-Null
    Connect-AzAccount -Identity
}
