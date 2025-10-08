. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSetFeatureStatus Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SetFeatureStatus SDK Cmdlet" {

        It "Invoke-DataverseSetFeatureStatus executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SetFeatureStatusRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SetFeatureStatus"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SetFeatureStatusResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSetFeatureStatus -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SetFeatureStatus"
        }

    }
}
