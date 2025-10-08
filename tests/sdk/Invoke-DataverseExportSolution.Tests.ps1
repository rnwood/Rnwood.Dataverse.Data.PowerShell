. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseExportSolution Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ExportSolution SDK Cmdlet" {
        It "Invoke-DataverseExportSolution exports a solution" {
            $solutionName = "TestSolution"
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ExportSolutionRequest", {
                param($request)
                
                # Validate request parameters were properly converted
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.ExportSolutionRequest"
                $request.SolutionName | Should -BeOfType [System.String]
                $request.SolutionName | Should -Be "TestSolution"
                $request.Managed | Should -BeOfType [System.Boolean]
                $request.Managed | Should -Be $false
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.ExportSolutionResponse
                # Create a fake solution file (just some bytes)
                $response.Results["ExportSolutionFile"] = [System.Text.Encoding]::UTF8.GetBytes("fake solution content")
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseExportSolution -Connection $script:conn -SolutionName $solutionName -Managed $false
            
            # Verify response type as documented
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.ExportSolutionResponse"
            $response.GetType().Name | Should -Be "ExportSolutionResponse"
            
            # Verify response contains expected data
            $response.ExportSolutionFile | Should -Not -BeNull
            $response.ExportSolutionFile | Should -BeOfType [System.Byte[]]
            $response.ExportSolutionFile.Length | Should -BeGreaterThan 0
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.ExportSolutionRequest"
            $proxy.LastRequest.SolutionName | Should -Be $solutionName
            $proxy.LastRequest.Managed | Should -Be $false
        }
    }
}
