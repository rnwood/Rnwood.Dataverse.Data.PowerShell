. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseComponentDependency' {
    Context 'RequiredBy parameter set' {
        BeforeEach {
            $connection = getMockConnection -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveDependenciesForDeleteRequest') {
                    $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveDependenciesForDeleteResponse
                    $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    
                    # Create a mock dependency entity
                    $dependency = New-Object Microsoft.Xrm.Sdk.Entity("dependency")
                    $dependency.Id = [Guid]::NewGuid()
                    $dependency["dependentcomponentobjectid"] = [Guid]::NewGuid()
                    $dependency["dependentcomponenttype"] = 1
                    $dependency["requiredcomponentobjectid"] = $request.ObjectId
                    $dependency["requiredcomponenttype"] = $request.ComponentType
                    
                    $entityCollection.Entities.Add($dependency)
                    $response.Results.Add("EntityCollection", $entityCollection)
                    return $response
                }
            }
        }

        It "Can retrieve dependencies for delete with RequiredBy switch" {
            $objectId = [Guid]::NewGuid()
            $result = Get-DataverseComponentDependency -Connection $connection -ObjectId $objectId -ComponentType 1 -RequiredBy
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "dependency"
        }
        
        It "Accepts ObjectId parameter from pipeline" {
            $objectId = [Guid]::NewGuid()
            $inputObject = [PSCustomObject]@{ ObjectId = $objectId }
            $result = $inputObject | Get-DataverseComponentDependency -Connection $connection -ComponentType 1 -RequiredBy
            $result | Should -Not -BeNullOrEmpty
        }
        
        It "Supports ComponentId alias for ObjectId parameter" {
            $componentId = [Guid]::NewGuid()
            $result = Get-DataverseComponentDependency -Connection $connection -ComponentId $componentId -ComponentType 1 -RequiredBy
            $result | Should -Not -BeNullOrEmpty
        }
        
        It "Supports MetadataId alias for ObjectId parameter" {
            $metadataId = [Guid]::NewGuid()
            $result = Get-DataverseComponentDependency -Connection $connection -MetadataId $metadataId -ComponentType 1 -RequiredBy
            $result | Should -Not -BeNullOrEmpty
        }
        
        It "Returns empty collection when no dependencies exist" {
            $connection2 = getMockConnection -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveDependenciesForDeleteRequest') {
                    $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveDependenciesForDeleteResponse
                    $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    $response.Results.Add("EntityCollection", $entityCollection)
                    return $response
                }
            }
            
            $objectId = [Guid]::NewGuid()
            $result = Get-DataverseComponentDependency -Connection $connection2 -ObjectId $objectId -ComponentType 1 -RequiredBy
            $result | Should -BeNullOrEmpty
        }
    }
    
    Context 'Dependent parameter set' {
        BeforeEach {
            $connection = getMockConnection -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveDependentComponentsRequest') {
                    $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveDependentComponentsResponse
                    $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    
                    # Create a mock dependency entity
                    $dependency = New-Object Microsoft.Xrm.Sdk.Entity("dependency")
                    $dependency.Id = [Guid]::NewGuid()
                    $dependency["requiredcomponentobjectid"] = [Guid]::NewGuid()
                    $dependency["requiredcomponenttype"] = 1
                    $dependency["dependentcomponentobjectid"] = $request.ObjectId
                    $dependency["dependentcomponenttype"] = $request.ComponentType
                    
                    $entityCollection.Entities.Add($dependency)
                    $response.Results.Add("EntityCollection", $entityCollection)
                    return $response
                }
            }
        }

        It "Can retrieve dependent components with Dependent switch" {
            $objectId = [Guid]::NewGuid()
            $result = Get-DataverseComponentDependency -Connection $connection -ObjectId $objectId -ComponentType 1 -Dependent
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "dependency"
        }
        
        It "Accepts ObjectId parameter from pipeline" {
            $objectId = [Guid]::NewGuid()
            $inputObject = [PSCustomObject]@{ ObjectId = $objectId }
            $result = $inputObject | Get-DataverseComponentDependency -Connection $connection -ComponentType 1 -Dependent
            $result | Should -Not -BeNullOrEmpty
        }
        
        It "Supports ComponentId alias for ObjectId parameter" {
            $componentId = [Guid]::NewGuid()
            $result = Get-DataverseComponentDependency -Connection $connection -ComponentId $componentId -ComponentType 1 -Dependent
            $result | Should -Not -BeNullOrEmpty
        }
        
        It "Returns empty collection when no dependent components exist" {
            $connection2 = getMockConnection -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveDependentComponentsRequest') {
                    $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveDependentComponentsResponse
                    $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    $response.Results.Add("EntityCollection", $entityCollection)
                    return $response
                }
            }
            
            $objectId = [Guid]::NewGuid()
            $result = Get-DataverseComponentDependency -Connection $connection2 -ObjectId $objectId -ComponentType 1 -Dependent
            $result | Should -BeNullOrEmpty
        }
    }
}
