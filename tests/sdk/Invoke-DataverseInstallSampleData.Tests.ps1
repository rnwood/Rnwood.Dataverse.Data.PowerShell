. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseInstallSampleData Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "InstallSampleData SDK Cmdlet" {

        It "Invoke-DataverseInstallSampleData executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.InstallSampleDataRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "InstallSampleData"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.InstallSampleDataResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseInstallSampleData -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "InstallSampleData"
        }

    }
}
