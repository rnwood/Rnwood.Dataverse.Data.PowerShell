. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseReleaseToQueue Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ReleaseToQueue SDK Cmdlet" {

        It "Invoke-DataverseReleaseToQueue executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ReleaseToQueueRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ReleaseToQueue"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ReleaseToQueueResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseReleaseToQueue -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ReleaseToQueue"
        }

    }
}
