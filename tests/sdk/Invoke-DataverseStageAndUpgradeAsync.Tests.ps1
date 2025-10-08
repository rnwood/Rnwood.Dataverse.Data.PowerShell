. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseStageAndUpgradeAsync Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "StageAndUpgradeAsync SDK Cmdlet" {

        It "Invoke-DataverseStageAndUpgradeAsync executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.StageAndUpgradeAsyncRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "StageAndUpgradeAsync"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.StageAndUpgradeAsyncResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseStageAndUpgradeAsync -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "StageAndUpgradeAsync"
        }

    }
}
