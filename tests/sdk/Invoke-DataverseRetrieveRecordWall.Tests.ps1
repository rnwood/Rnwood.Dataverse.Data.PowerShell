. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveRecordWall Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveRecordWall SDK Cmdlet" {

        It "Invoke-DataverseRetrieveRecordWall executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveRecordWallRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveRecordWall"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveRecordWallResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveRecordWall -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveRecordWall"
        }

    }
}
