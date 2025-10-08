. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUninstallSolutionAsync Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UninstallSolutionAsync SDK Cmdlet" {

        It "Invoke-DataverseUninstallSolutionAsync executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UninstallSolutionAsyncRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UninstallSolutionAsync"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UninstallSolutionAsyncResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUninstallSolutionAsync -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UninstallSolutionAsync"
        }

    }
}
