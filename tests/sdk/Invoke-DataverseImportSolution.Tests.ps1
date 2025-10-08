. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseImportSolution Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ImportSolution SDK Cmdlet" {
        It "Invoke-DataverseImportSolution imports a solution" {
            $solutionBytes = [System.Text.Encoding]::UTF8.GetBytes("fake solution content")
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ImportSolutionRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.ImportSolutionResponse
                return $response
            })
            
            # Call the cmdlet
            { Invoke-DataverseImportSolution -Connection $script:conn -CustomizationFile $solutionBytes } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "ImportSolutionRequest"
            $proxy.LastRequest.CustomizationFile | Should -Be $solutionBytes
        }
    }
}
