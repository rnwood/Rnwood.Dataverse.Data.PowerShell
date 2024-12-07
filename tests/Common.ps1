BeforeAll {

    if ($env:TESTMODULEPATH) {
        import-module "${env:TESTMODULEPATH}/Rnwood.Dataverse.Data.PowerShell.psd1"
    } else {
        import-module "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/Rnwood.Dataverse.Data.PowerShell.psd1"
    }
     
    Add-Type -AssemblyName "System.Runtime.Serialization"
    Add-Type -AssemblyName "Microsoft.Xrm.Sdk"

    # Define the DataContractSerializer
    $serializer = New-Object System.Runtime.Serialization.DataContractSerializer([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])

    get-item $PSScriptRoot/*.xml | foreach-object {

        $stream = [IO.File]::OpenRead($_.FullName)
        $metadata += $serializer.ReadObject($stream)
        $stream.Close();
    }


    function getMockConnection() {
        get-dataverseconnection -url https://fake.crm.dynamics.com/ -mock $metadata
    }
}
