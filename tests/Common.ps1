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

    function New-MinimalEntityMetadata {
        param(
            [string]$LogicalName,
            [string]$PrimaryIdAttribute,
            [string]$PrimaryNameAttribute = "name",
            [hashtable]$Attributes = @{}
        )
        
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)){
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }

        $entityMetadata = New-Object Microsoft.Xrm.Sdk.Metadata.EntityMetadata
        
        # Use reflection to set read-only properties
        $entityMetadata.GetType().GetProperty("LogicalName").SetValue($entityMetadata, $LogicalName)
        $entityMetadata.GetType().GetProperty("SchemaName").SetValue($entityMetadata, (Get-Culture).TextInfo.ToTitleCase($LogicalName))
        
        if ($PrimaryIdAttribute) {
            $entityMetadata.GetType().GetProperty("PrimaryIdAttribute").SetValue($entityMetadata, $PrimaryIdAttribute)
        } else {
            $entityMetadata.GetType().GetProperty("PrimaryIdAttribute").SetValue($entityMetadata, "${LogicalName}id")
        }
        
        $entityMetadata.GetType().GetProperty("PrimaryNameAttribute").SetValue($entityMetadata, $PrimaryNameAttribute)
        
        # Create minimal attribute collection
        $attributeList = New-Object 'System.Collections.Generic.List[Microsoft.Xrm.Sdk.Metadata.AttributeMetadata]'
        
        # Add primary ID attribute
        $idAttr = New-Object Microsoft.Xrm.Sdk.Metadata.AttributeMetadata
        $idAttr.GetType().GetProperty("LogicalName").SetValue($idAttr, $entityMetadata.PrimaryIdAttribute)
        $idAttr.GetType().GetProperty("SchemaName").SetValue($idAttr, (Get-Culture).TextInfo.ToTitleCase($entityMetadata.PrimaryIdAttribute))
        $attributeList.Add($idAttr)
        
        # Add primary name attribute
        $nameAttr = New-Object Microsoft.Xrm.Sdk.Metadata.StringAttributeMetadata
        $nameAttr.GetType().GetProperty("LogicalName").SetValue($nameAttr, $PrimaryNameAttribute)
        $nameAttr.GetType().GetProperty("SchemaName").SetValue($nameAttr, (Get-Culture).TextInfo.ToTitleCase($PrimaryNameAttribute))
        $attributeList.Add($nameAttr)
        
        # Add custom attributes
        foreach ($attrName in $Attributes.Keys) {
            $attr = New-Object Microsoft.Xrm.Sdk.Metadata.StringAttributeMetadata
            $attr.GetType().GetProperty("LogicalName").SetValue($attr, $attrName)
            $attr.GetType().GetProperty("SchemaName").SetValue($attr, (Get-Culture).TextInfo.ToTitleCase($attrName))
            $attributeList.Add($attr)
        }
        
        # Use reflection to set the private _attributes field
        $attributesField = $entityMetadata.GetType().GetField("_attributes", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
        $attributesField.SetValue($entityMetadata, $attributeList.ToArray())
        
        return $entityMetadata
    }

    function getMockConnection {
        param(
            [string[]]$AdditionalEntities = @()
        )
        
        if (-not $metadata) {
            if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)){
                Import-Module Rnwood.Dataverse.Data.PowerShell
            }
            Add-Type -AssemblyName "System.Runtime.Serialization"

            # Define the DataContractSerializer
            $serializer = New-Object System.Runtime.Serialization.DataContractSerializer([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
        
            # Load all existing XML metadata files
            get-item $PSScriptRoot/*.xml -ErrorAction SilentlyContinue | foreach-object {
                $stream = [IO.File]::OpenRead($_.FullName)
                $metadata += $serializer.ReadObject($stream)
                $stream.Close();
            }
        }

        # Create a copy of metadata to avoid modifying the cached version
        $connectionMetadata = @()
        if ($metadata) {
            $connectionMetadata += $metadata
        }
        
        # Add minimal metadata for additional entities if requested
        foreach ($entityName in $AdditionalEntities) {
            # Check if this entity already exists in loaded metadata
            $exists = $connectionMetadata | Where-Object { $_.LogicalName -eq $entityName }
            if (-not $exists) {
                $minimalMetadata = New-MinimalEntityMetadata -LogicalName $entityName -PrimaryIdAttribute "${entityName}id"
                $connectionMetadata += $minimalMetadata
            }
        }

        get-dataverseconnection -url https://fake.crm.dynamics.com/ -mock $connectionMetadata
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


