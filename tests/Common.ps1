
$metadata = $null;
if (-not $metadata) {
    Add-Type -AssemblyName "System.Runtime.Serialization"
    $serializer = New-Object System.Runtime.Serialization.DataContractSerializer([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
    get-item $PSScriptRoot/*.xml | foreach-object {
        $stream = [IO.File]::OpenRead($_.FullName)
        $metadata += $serializer.ReadObject($stream)
        $stream.Close();
    }
}

function getMockConnection($failNextExecuteMultiple = $false, $failExecuteMultipleIndices = @(), $failExecuteMultipleTimes = 0) {
    $mockService = get-dataverseconnection -url https://fake.crm.dynamics.com/ -mock $metadata
    $innerService = $mockService.OrganizationService
    
    if ($failNextExecuteMultiple -or $failExecuteMultipleIndices.Count -gt 0 -or $failExecuteMultipleTimes -gt 0) {
        $type = [Rnwood.Dataverse.Data.PowerShell.Commands.MockOrganizationServiceWithFailures]
        $constructor = $type.GetConstructor([Microsoft.Xrm.Sdk.IOrganizationService])
        $wrapper = $constructor.Invoke(@($innerService))
        $wrapper.FailNextExecuteMultiple = $failNextExecuteMultiple
        foreach ($index in $failExecuteMultipleIndices) {
            $wrapper.FailExecuteMultipleIndices.Add($index)
        }
        $wrapper.FailExecuteMultipleTimes = $failExecuteMultipleTimes
        $service = New-Object Microsoft.PowerPlatform.Dataverse.Client.ServiceClient -ArgumentList $wrapper
    } else {
        $service = $mockService
    }
    
    return $service
}


