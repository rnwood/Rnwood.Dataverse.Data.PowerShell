. $PSScriptRoot/Common.ps1

Describe "Set-DataverseView - Empty LayoutXml Bug Fix" {
    Context "Bug: Null reference when updating view with empty layoutxml" {
        It "Updates view with empty layoutxml without null reference error" {
            # This test reproduces the issue from GitHub issue: Set-DataverseView with empty layoutxml
            # throws "Value cannot be null. (Parameter 'value')" when updating columns
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact") -RequestInterceptor {
                param($request)
                
                # Intercept Retrieve request to return a view with empty layoutxml and fetchxml
                if ($request.GetType().Name -eq 'RetrieveRequest') {
                    $viewEntity = New-Object Microsoft.Xrm.Sdk.Entity("savedquery")
                    $viewEntity.Id = $request.Target.Id
                    $viewEntity["name"] = "Test View"
                    $viewEntity["returnedtypecode"] = "contact"
                    $viewEntity["fetchxml"] = ""
                    $viewEntity["layoutxml"] = ""
                    $viewEntity["querytype"] = 0
                    
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveResponse
                    $response.Results.Add("Entity", $viewEntity)
                    return $response
                }
                
                # Let other requests go through
                return $null
            }
            
            # Create a view with FetchXML first
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
  </entity>
</fetch>
"@
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test View" `
                -TableName contact `
                -ViewType "System" `
                -FetchXml $fetchXml
            
            # Now update with Columns parameter - this should NOT throw "Value cannot be null"
            # The bug was that when layoutxml is empty, BuildDefaultLayoutXml was called
            # but TableName property wasn't set, causing null reference in XAttribute
            { 
                Set-DataverseView -Connection $connection `
                    -Id $viewId `
                    -ViewType "System" `
                    -Columns @(@{ name = "firstname"; width = 200 }, @{ name = "lastname"; width = 150 }) `
                    -Confirm:$false
            } | Should -Not -Throw
        }
        
        It "Updates view with empty layoutxml and no TableName parameter" {
            # This test ensures tableName is extracted from the view entity's returnedtypecode
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact") -RequestInterceptor {
                param($request)
                
                # Intercept Retrieve request to return a view with empty layoutxml but valid returnedtypecode
                if ($request.GetType().Name -eq 'RetrieveRequest') {
                    $viewEntity = New-Object Microsoft.Xrm.Sdk.Entity("savedquery")
                    $viewEntity.Id = $request.Target.Id
                    $viewEntity["name"] = "Test View"
                    $viewEntity["returnedtypecode"] = "contact"
                    $viewEntity["fetchxml"] = "<fetch><entity name='contact'><attribute name='contactid'/></entity></fetch>"
                    $viewEntity["layoutxml"] = ""
                    $viewEntity["querytype"] = 0
                    
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveResponse
                    $response.Results.Add("Entity", $viewEntity)
                    return $response
                }
                
                return $null
            }
            
            # Create a view
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
  </entity>
</fetch>
"@
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test View" `
                -TableName contact `
                -ViewType "System" `
                -FetchXml $fetchXml
            
            # Update without TableName parameter - should extract from returnedtypecode
            { 
                Set-DataverseView -Connection $connection `
                    -Id $viewId `
                    -ViewType "System" `
                    -Columns @("firstname", "lastname") `
                    -Confirm:$false
            } | Should -Not -Throw
        }
        
        It "Updates view with empty layoutxml and returnedtypecode by extracting from fetchxml" {
            # This test ensures tableName is extracted from fetchxml when returnedtypecode is also empty
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact") -RequestInterceptor {
                param($request)
                
                # Intercept Retrieve to return view with empty returnedtypecode but valid fetchxml
                if ($request.GetType().Name -eq 'RetrieveRequest') {
                    $viewEntity = New-Object Microsoft.Xrm.Sdk.Entity("savedquery")
                    $viewEntity.Id = $request.Target.Id
                    $viewEntity["name"] = "Test View"
                    $viewEntity["returnedtypecode"] = ""  # Empty!
                    $viewEntity["fetchxml"] = "<fetch><entity name='contact'><attribute name='firstname'/></entity></fetch>"
                    $viewEntity["layoutxml"] = ""
                    $viewEntity["querytype"] = 0
                    
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveResponse
                    $response.Results.Add("Entity", $viewEntity)
                    return $response
                }
                
                return $null
            }
            
            # Create a view
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
  </entity>
</fetch>
"@
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test View" `
                -TableName contact `
                -ViewType "System" `
                -FetchXml $fetchXml
            
            # Update - should extract tableName from fetchxml
            { 
                Set-DataverseView -Connection $connection `
                    -Id $viewId `
                    -ViewType "System" `
                    -AddColumns @("emailaddress1") `
                    -Confirm:$false
            } | Should -Not -Throw
        }
    }
}
