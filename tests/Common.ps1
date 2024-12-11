BeforeAll {

    if ($env:TESTMODULEPATH) {
        $source = $env:TESTMODULEPATH
    }
    else {
        $source = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/"
    }

    $tempmodulefolder = "${env:TEMP}/$([Guid]::NewGuid())"
    new-item -ItemType Directory $tempmodulefolder
    copy-item -Recurse $source $tempmodulefolder/Rnwood.Dataverse.Data.PowerShell
    $env:PSModulePath = $tempmodulefolder;
    $env:ChildProcessPSModulePath = $tempmodulefolder
     
    $metadata = $null;

    function getMockConnection() {
        if (-not $metadata) {
            Import-Module Rnwood.Dataverse.Data.PowerShell
            Add-Type -AssemblyName "System.Runtime.Serialization"

            # Define the DataContractSerializer
            $serializer = New-Object System.Runtime.Serialization.DataContractSerializer([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
        
            get-item $PSScriptRoot/*.xml | foreach-object {
        
                $stream = [IO.File]::OpenRead($_.FullName)
                $metadata += $serializer.ReadObject($stream)
                $stream.Close();
            }
        }

        get-dataverseconnection -url https://fake.crm.dynamics.com/ -mock $metadata
    }

    AfterEach {
        if (Get-Module Rnwood.Dataverse.Data.PowerShell) {
            Remove-Module Rnwood.Dataverse.Data.PowerShell
        }
    }
}


