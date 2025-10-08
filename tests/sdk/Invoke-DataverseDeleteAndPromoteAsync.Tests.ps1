. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeleteAndPromoteAsync Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeleteAndPromoteAsync SDK Cmdlet" {

        It "Invoke-DataverseDeleteAndPromoteAsync executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeleteAndPromoteAsyncRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "DeleteAndPromoteAsync"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.DeleteAndPromoteAsyncResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseDeleteAndPromoteAsync -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeleteAndPromoteAsync"
        }

    }
}
