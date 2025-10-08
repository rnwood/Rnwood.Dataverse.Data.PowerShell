. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseLoseOpportunity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "LoseOpportunity SDK Cmdlet" {

        It "Invoke-DataverseLoseOpportunity marks opportunity as lost" {
            $opportunityId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.LoseOpportunityRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.LoseOpportunityRequest"
                $request.OpportunityClose | Should -Not -BeNull
                $request.OpportunityClose | Should -BeOfType [Microsoft.Xrm.Sdk.Entity]
                $request.Status | Should -BeOfType [Microsoft.Xrm.Sdk.OptionSetValue]
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.LoseOpportunityResponse
                return $response
            })
            
            # Call the cmdlet
            $opportunityClose = New-Object Microsoft.Xrm.Sdk.Entity("opportunityclose")
            $opportunityClose["opportunityid"] = New-Object Microsoft.Xrm.Sdk.EntityReference("opportunity", $opportunityId)
            $status = New-Object Microsoft.Xrm.Sdk.OptionSetValue(4)
            
            $response = Invoke-DataverseLoseOpportunity -Connection $script:conn -OpportunityClose $opportunityClose -Status $status
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.LoseOpportunityResponse"
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.Status.Value | Should -Be 4
        }
    }

    }
}
