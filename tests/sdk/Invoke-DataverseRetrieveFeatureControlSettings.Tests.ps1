. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveFeatureControlSettings Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveFeatureControlSettings SDK Cmdlet" {

        It "Invoke-DataverseRetrieveFeatureControlSettings executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveFeatureControlSettingsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveFeatureControlSettings"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveFeatureControlSettingsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveFeatureControlSettings -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveFeatureControlSettings"
        }

    }
}
