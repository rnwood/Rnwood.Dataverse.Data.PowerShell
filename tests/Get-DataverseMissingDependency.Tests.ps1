. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseMissingDependency' {
    BeforeEach {
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveMissingDependenciesRequest') {
                $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveMissingDependenciesResponse
                $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                
                # Create a mock missing dependency entity
                $missingDependency = New-Object Microsoft.Xrm.Sdk.Entity("missingdependency")
                $missingDependency.Id = [Guid]::NewGuid()
                $missingDependency["missingcomponentid"] = [Guid]::NewGuid()
                $missingDependency["missingcomponenttype"] = 1
                
                $entityCollection.Entities.Add($missingDependency)
                $response.Results.Add("EntityCollection", $entityCollection)
                return $response
            }
        }
    }

    It "Can retrieve missing dependencies for a solution" {
        $result = Get-DataverseMissingDependency -Connection $connection -SolutionUniqueName "TestSolution"
        $result | Should -Not -BeNullOrEmpty
        $result.LogicalName | Should -Be "missingdependency"
    }
    
    It "Accepts SolutionUniqueName parameter from pipeline" {
        $inputObject = [PSCustomObject]@{ SolutionUniqueName = "TestSolution" }
        $result = $inputObject | Get-DataverseMissingDependency -Connection $connection
        $result | Should -Not -BeNullOrEmpty
    }
    
    It "Supports UniqueName alias for SolutionUniqueName parameter" {
        $result = Get-DataverseMissingDependency -Connection $connection -UniqueName "TestSolution"
        $result | Should -Not -BeNullOrEmpty
    }
    
    It "Returns empty collection when no missing dependencies exist" {
        $connection2 = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveMissingDependenciesRequest') {
                $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveMissingDependenciesResponse
                $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                $response.Results.Add("EntityCollection", $entityCollection)
                return $response
            }
        }
        
        $result = Get-DataverseMissingDependency -Connection $connection2 -SolutionUniqueName "TestSolution"
        $result | Should -BeNullOrEmpty
    }
}
