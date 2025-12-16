. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseDependency' {
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

    It "Can retrieve dependencies for delete with ObjectId and ComponentType" {
        $objectId = [Guid]::NewGuid()
        $result = Get-DataverseDependency -Connection $connection -ObjectId $objectId -ComponentType 1
        $result | Should -Not -BeNullOrEmpty
        $result.LogicalName | Should -Be "dependency"
    }
    
    It "Accepts ObjectId parameter from pipeline" {
        $objectId = [Guid]::NewGuid()
        $inputObject = [PSCustomObject]@{ ObjectId = $objectId }
        $result = $inputObject | Get-DataverseDependency -Connection $connection -ComponentType 1
        $result | Should -Not -BeNullOrEmpty
    }
    
    It "Supports ComponentId alias for ObjectId parameter" {
        $componentId = [Guid]::NewGuid()
        $result = Get-DataverseDependency -Connection $connection -ComponentId $componentId -ComponentType 1
        $result | Should -Not -BeNullOrEmpty
    }
    
    It "Supports MetadataId alias for ObjectId parameter" {
        $metadataId = [Guid]::NewGuid()
        $result = Get-DataverseDependency -Connection $connection -MetadataId $metadataId -ComponentType 1
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
        $result = Get-DataverseDependency -Connection $connection2 -ObjectId $objectId -ComponentType 1
        $result | Should -BeNullOrEmpty
    }
}
