. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrievePersonalWall Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrievePersonalWall SDK Cmdlet" {

        It "Invoke-DataverseRetrievePersonalWall executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrievePersonalWallRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrievePersonalWall"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrievePersonalWallResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrievePersonalWall -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrievePersonalWall"
        }

    }
}
