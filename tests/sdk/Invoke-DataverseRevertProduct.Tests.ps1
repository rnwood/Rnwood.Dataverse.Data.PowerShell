. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRevertProduct Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RevertProduct SDK Cmdlet" {

        It "Invoke-DataverseRevertProduct executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RevertProductRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RevertProduct"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RevertProductResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRevertProduct -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RevertProduct"
        }

    }
}
