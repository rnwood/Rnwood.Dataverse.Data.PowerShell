$ErrorActionPreference = "Stop"

Add-Type -AssemblyName "System.Runtime.Serialization"

# Define the DataContractSerializer
$serializer = New-Object System.Runtime.Serialization.DataContractSerializer([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])

foreach($entityname in "contact") {
$em  = (invoke-dataverserequest -connection $c RetrieveEntity @{"LogicalName"=$entityname; "EntityFilters"=[Microsoft.Xrm.Sdk.Metadata.EntityFilters]::All; "MetadataId"=[Guid]::Empty; "RetrieveAsIfPublished"=$false}).Results["EntityMetadata"]

$outputStream = [IO.File]::OpenWrite("$(get-location)/${entityname}.xml")
$serializer.WriteObject($outputStream, $em)
$outputStream.Close()
}
