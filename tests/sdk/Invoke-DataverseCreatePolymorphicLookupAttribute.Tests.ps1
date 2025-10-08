. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCreatePolymorphicLookupAttribute Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CreatePolymorphicLookupAttribute SDK Cmdlet" {

        It "Invoke-DataverseCreatePolymorphicLookupAttribute executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CreatePolymorphicLookupAttributeRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CreatePolymorphicLookupAttributeRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CreatePolymorphicLookupAttributeResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCreatePolymorphicLookupAttribute -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CreatePolymorphicLookupAttributeRequest"
        }

    }
}
