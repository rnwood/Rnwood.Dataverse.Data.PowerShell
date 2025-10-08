. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveFeatureControlSetting Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveFeatureControlSetting SDK Cmdlet" {

        It "Invoke-DataverseRetrieveFeatureControlSetting executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveFeatureControlSettingRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveFeatureControlSetting"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveFeatureControlSettingResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveFeatureControlSetting -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveFeatureControlSetting"
        }

    }
}
