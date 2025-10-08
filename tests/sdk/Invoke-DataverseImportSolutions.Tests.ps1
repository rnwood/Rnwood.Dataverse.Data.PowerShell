. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseImportSolutions Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ImportSolutions SDK Cmdlet" {

        It "Invoke-DataverseImportSolutions executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ImportSolutionsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ImportSolutions"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ImportSolutionsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseImportSolutions -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ImportSolutions"
        }

    }
}
