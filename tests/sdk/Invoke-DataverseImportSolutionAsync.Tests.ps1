. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseImportSolutionAsync Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ImportSolutionAsync SDK Cmdlet" {

        It "Invoke-DataverseImportSolutionAsync executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ImportSolutionAsyncRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ImportSolutionAsync"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ImportSolutionAsyncResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseImportSolutionAsync -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ImportSolutionAsync"
        }

    }
}
