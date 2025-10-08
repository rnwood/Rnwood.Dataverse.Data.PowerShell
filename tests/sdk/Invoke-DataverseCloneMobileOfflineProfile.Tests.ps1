. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCloneMobileOfflineProfile Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CloneMobileOfflineProfile SDK Cmdlet" {

        It "Invoke-DataverseCloneMobileOfflineProfile executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CloneMobileOfflineProfileRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "CloneMobileOfflineProfile"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.CloneMobileOfflineProfileResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseCloneMobileOfflineProfile -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CloneMobileOfflineProfile"
        }

    }
}
