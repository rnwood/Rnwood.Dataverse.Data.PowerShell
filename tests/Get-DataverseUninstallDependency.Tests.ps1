. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseUninstallDependency' {
    BeforeEach {
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveDependenciesForUninstallRequest') {
                $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveDependenciesForUninstallResponse
                $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                
                # Create a mock dependency entity
                $dependency = New-Object Microsoft.Xrm.Sdk.Entity("dependency")
                $dependency.Id = [Guid]::NewGuid()
                $dependency["dependentcomponentobjectid"] = [Guid]::NewGuid()
                $dependency["dependentcomponenttype"] = 1
                $dependency["requiredcomponentobjectid"] = [Guid]::NewGuid()
                $dependency["requiredcomponenttype"] = 1
                
                $entityCollection.Entities.Add($dependency)
                $response.Results.Add("EntityCollection", $entityCollection)
                return $response
            }
        }
    }

    It "Can retrieve dependencies that prevent solution uninstall" {
        $result = Get-DataverseUninstallDependency -Connection $connection -SolutionUniqueName "TestSolution"
        $result | Should -Not -BeNullOrEmpty
        $result.LogicalName | Should -Be "dependency"
    }
    
    It "Accepts SolutionUniqueName parameter from pipeline" {
        $inputObject = [PSCustomObject]@{ SolutionUniqueName = "TestSolution" }
        $result = $inputObject | Get-DataverseUninstallDependency -Connection $connection
        $result | Should -Not -BeNullOrEmpty
    }
    
    It "Supports UniqueName alias for SolutionUniqueName parameter" {
        $result = Get-DataverseUninstallDependency -Connection $connection -UniqueName "TestSolution"
        $result | Should -Not -BeNullOrEmpty
    }
    
    It "Returns empty collection when no dependencies prevent uninstall" {
        $connection2 = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveDependenciesForUninstallRequest') {
                $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveDependenciesForUninstallResponse
                $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                $response.Results.Add("EntityCollection", $entityCollection)
                return $response
            }
        }
        
        $result = Get-DataverseUninstallDependency -Connection $connection2 -SolutionUniqueName "TestSolution"
        $result | Should -BeNullOrEmpty
    }
}
