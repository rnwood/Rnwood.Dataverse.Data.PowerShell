function getMockConnection($failNextExecuteMultiple = $false, $failExecuteMultipleIndices = @()) {
    $context = New-XrmFakedContext
    $context.InitializeFromFile("$PSScriptRoot\contact.xml")
    $innerService = $context.GetOrganizationService()
    
    if ($failNextExecuteMultiple -or $failExecuteMultipleIndices.Count -gt 0) {
        $wrapper = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.MockOrganizationServiceWithFailures -ArgumentList $innerService
        $wrapper.FailNextExecuteMultiple = $failNextExecuteMultiple
        foreach ($index in $failExecuteMultipleIndices) {
            $wrapper.FailExecuteMultipleIndices.Add($index)
        }
        $service = New-Object Microsoft.PowerPlatform.Dataverse.Client.ServiceClient -ArgumentList $wrapper
    } else {
        $service = New-Object Microsoft.PowerPlatform.Dataverse.Client.ServiceClient -ArgumentList $innerService
    }
    
    return $service
}


