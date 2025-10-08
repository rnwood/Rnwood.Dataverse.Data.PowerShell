. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseAddSolutionComponent Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "AddSolutionComponent SDK Cmdlet" {
        It "Invoke-DataverseAddSolutionComponent adds a component to a solution" {
            $solutionId = [Guid]::NewGuid()
            $componentId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.AddSolutionComponentRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.AddSolutionComponentRequest"
                $request.ComponentId | Should -BeOfType [System.Guid]
                $request.ComponentType | Should -BeOfType [System.Int32]
                $request.SolutionUniqueName | Should -BeOfType [System.String]
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.AddSolutionComponentResponse
                $response.Results["id"] = [Guid]::NewGuid()
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseAddSolutionComponent -Connection $script:conn -ComponentId $componentId -ComponentType 1 -SolutionUniqueName "TestSolution"
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.AddSolutionComponentResponse"
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.ComponentId | Should -Be $componentId
            $proxy.LastRequest.SolutionUniqueName | Should -Be "TestSolution"
        }
    }
}
