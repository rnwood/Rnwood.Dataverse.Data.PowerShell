. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseApplyRoutingRule Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ApplyRoutingRule SDK Cmdlet" {

        It "Invoke-DataverseApplyRoutingRule executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ApplyRoutingRuleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ApplyRoutingRule"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ApplyRoutingRuleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Create contact record for Target parameter (using contact entity in mock metadata)
            $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact | Set-DataverseRecord -Connection $script:conn -CreateOnly
            
            $target = [PSCustomObject]@{ contactid = $contactId }
            
            $response = Invoke-DataverseApplyRoutingRule -Connection $script:conn -Target $target -TargetTableName "contact" -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ApplyRoutingRule"
        }

    }
}
