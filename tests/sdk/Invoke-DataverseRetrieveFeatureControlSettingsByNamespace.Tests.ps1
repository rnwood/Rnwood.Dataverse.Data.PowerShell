. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveFeatureControlSettingsByNamespace Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveFeatureControlSettingsByNamespace SDK Cmdlet" {

        It "Invoke-DataverseRetrieveFeatureControlSettingsByNamespace executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveFeatureControlSettingsByNamespaceRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveFeatureControlSettingsByNamespace"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveFeatureControlSettingsByNamespaceResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveFeatureControlSettingsByNamespace -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveFeatureControlSettingsByNamespace"
        }

    }
}
