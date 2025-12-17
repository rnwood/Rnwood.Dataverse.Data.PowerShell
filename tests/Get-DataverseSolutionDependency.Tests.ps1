. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseSolutionDependency' {
    Context 'Missing parameter set' {
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

        It "Can retrieve missing dependencies with Missing switch" {
            $result = Get-DataverseSolutionDependency -Connection $connection -SolutionUniqueName "TestSolution" -Missing
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "missingdependency"
        }
        
        It "Accepts SolutionUniqueName parameter from pipeline" {
            $inputObject = [PSCustomObject]@{ SolutionUniqueName = "TestSolution" }
            $result = $inputObject | Get-DataverseSolutionDependency -Connection $connection -Missing
            $result | Should -Not -BeNullOrEmpty
        }
        
        It "Supports UniqueName alias for SolutionUniqueName parameter" {
            $result = Get-DataverseSolutionDependency -Connection $connection -UniqueName "TestSolution" -Missing
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
            
            $result = Get-DataverseSolutionDependency -Connection $connection2 -SolutionUniqueName "TestSolution" -Missing
            $result | Should -BeNullOrEmpty
        }
    }
    
    Context 'Uninstall parameter set' {
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

        It "Can retrieve uninstall dependencies with Uninstall switch" {
            $result = Get-DataverseSolutionDependency -Connection $connection -SolutionUniqueName "TestSolution" -Uninstall
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "dependency"
        }
        
        It "Accepts SolutionUniqueName parameter from pipeline" {
            $inputObject = [PSCustomObject]@{ SolutionUniqueName = "TestSolution" }
            $result = $inputObject | Get-DataverseSolutionDependency -Connection $connection -Uninstall
            $result | Should -Not -BeNullOrEmpty
        }
        
        It "Supports UniqueName alias for SolutionUniqueName parameter" {
            $result = Get-DataverseSolutionDependency -Connection $connection -UniqueName "TestSolution" -Uninstall
            $result | Should -Not -BeNullOrEmpty
        }
        
        It "Returns empty collection when no uninstall dependencies exist" {
            $connection2 = getMockConnection -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveDependenciesForUninstallRequest') {
                    $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveDependenciesForUninstallResponse
                    $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    $response.Results.Add("EntityCollection", $entityCollection)
                    return $response
                }
            }
            
            $result = Get-DataverseSolutionDependency -Connection $connection2 -SolutionUniqueName "TestSolution" -Uninstall
            $result | Should -BeNullOrEmpty
        }
    }
}
