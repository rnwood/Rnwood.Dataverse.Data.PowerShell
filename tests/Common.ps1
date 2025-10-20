
BeforeAll {

    if ($env:TESTMODULEPATH) {
        $source = $env:TESTMODULEPATH
    }
    else {
        $source = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/"
    }

    $tempmodulefolder = "$([IO.Path]::GetTempPath())/$([Guid]::NewGuid())"
    new-item -ItemType Directory $tempmodulefolder
    copy-item -Recurse $source $tempmodulefolder/Rnwood.Dataverse.Data.PowerShell
    $env:PSModulePath = $tempmodulefolder;
    $env:ChildProcessPSModulePath = $tempmodulefolder


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
        }
        else {
            $service = $mockService
        }
    
        return $service
    }


    function newPwsh([scriptblock] $scriptblock) {
        if ([System.Environment]::OSVersion.Platform -eq "Unix") {
            pwsh -noninteractive -noprofile -command $scriptblock
        }
        else {
            cmd /c pwsh -noninteractive -noprofile -command $scriptblock
        }
    }

    AfterEach {
        if (Get-Module Rnwood.Dataverse.Data.PowerShell) {
            Remove-Module Rnwood.Dataverse.Data.PowerShell
        }
    }

}

