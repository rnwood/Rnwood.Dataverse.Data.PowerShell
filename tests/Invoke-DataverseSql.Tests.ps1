Describe 'Invoke-DataverseSql' {

    . $PSScriptRoot/Common.ps1

    It "Can make a query" {
        
        $connection = getMockConnection
            
        $in = new-object Microsoft.Xrm.Sdk.Entity "contact"
        $in["contactid"] = [Guid]::NewGuid()
        $in["firstname"] = "text"
        $in["birthdate"] = [datetime]::Today
        $in["accountrolecode"] = [Microsoft.Xrm.Sdk.OptionSetValue] (new-object Microsoft.Xrm.Sdk.OptionSetValue 2)
        $in["parentcontactid"] = [Microsoft.Xrm.Sdk.EntityReference] (new-object Microsoft.Xrm.Sdk.EntityReference "contact", ([Guid]::NewGuid()))

        $in | Set-DataverseRecord -Connection $connection

        invoke-dataversesql -connection $connection -sql "Select * from contact"

    }

}
