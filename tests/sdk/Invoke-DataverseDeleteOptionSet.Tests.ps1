. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeleteOptionSet Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeleteOptionSet SDK Cmdlet" {

        It "Invoke-DataverseDeleteOptionSet executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeleteOptionSetRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "DeleteOptionSet"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.DeleteOptionSetResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseDeleteOptionSet -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeleteOptionSet"
        }

    }
}
