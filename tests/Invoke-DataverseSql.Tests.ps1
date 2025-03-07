Describe 'Invoke-DataverseSql' {

    . $PSScriptRoot/Common.ps1

    It "Can make a query" {
        
        $connection = getMockConnection
            
        $in = new-object Microsoft.Xrm.Sdk.Entity "contact"
        $in["contactid"] = [Guid]::NewGuid()
        $in["firstname"] = "text"
        
        $in | Set-DataverseRecord -Connection $connection

        invoke-dataversesql -connection $connection -sql "Select * from contact"

    }

}
