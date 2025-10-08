. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRemoveSolutionComponent Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RemoveSolutionComponent SDK Cmdlet" {
        It "Invoke-DataverseRemoveSolutionComponent removes a component from a solution" {
            $componentId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RemoveSolutionComponentRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.RemoveSolutionComponentRequest"
                $request.ComponentId | Should -BeOfType [System.Guid]
                $request.ComponentType | Should -BeOfType [System.Int32]
                $request.SolutionUniqueName | Should -BeOfType [System.String]
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.RemoveSolutionComponentResponse
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseRemoveSolutionComponent -Connection $script:conn -ComponentId $componentId -ComponentType 1 -SolutionUniqueName "TestSolution"
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.RemoveSolutionComponentResponse"
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.ComponentId | Should -Be $componentId
            $proxy.LastRequest.SolutionUniqueName | Should -Be "TestSolution"
        }
    }
}
