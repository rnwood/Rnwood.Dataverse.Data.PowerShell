# Azure Functions profile.ps1
#
# This profile.ps1 will get executed every 'cold start' of your Azure Functions app.
# 'cold start' occurs when:
#
# * A Function App instance is first started
# * A Function App instance is started after being de-allocated
#
# To define shared variables or functions that can be used in your Azure Functions,
# write them in this profile.ps1. You can then reference the function or variable in
# your function scripts.
#
# You can define functions specific to your deployment environment in this profile.
#
# Authenticate with Azure PowerShell using MSI.
# Remove this if you are not planning on using MSI or Azure PowerShell.
if ($env:MSI_SECRET) {
    Disable-AzContextAutosave -Scope Process | Out-Null
    Connect-AzAccount -Identity
}
