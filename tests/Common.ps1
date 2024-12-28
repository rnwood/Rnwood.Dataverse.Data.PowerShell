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

    function getMockConnection() {
        if (-not $metadata) {
            if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)){
                Import-Module Rnwood.Dataverse.Data.PowerShell
            }
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

    function newPwsh([scriptblock] $scriptblock) {
        if ([System.Environment]::OSVersion.Platform -eq "Unix") {
            pwsh -noninteractive -noprofile -command $scriptblock
        } else {
            cmd /c pwsh -noninteractive -noprofile -command $scriptblock
        }
    }

    AfterEach {
        if (Get-Module Rnwood.Dataverse.Data.PowerShell) {
            Remove-Module Rnwood.Dataverse.Data.PowerShell
        }
    }
}


