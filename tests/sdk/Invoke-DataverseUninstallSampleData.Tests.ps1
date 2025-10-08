. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUninstallSampleData Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UninstallSampleData SDK Cmdlet" {

        It "Invoke-DataverseUninstallSampleData executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UninstallSampleDataRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UninstallSampleData"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UninstallSampleDataResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUninstallSampleData -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UninstallSampleData"
        }

    }
}
