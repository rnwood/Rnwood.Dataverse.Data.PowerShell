. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseDependentComponent' {
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

    It "Can retrieve dependent components with ObjectId and ComponentType" {
        $objectId = [Guid]::NewGuid()
        $result = Get-DataverseDependentComponent -Connection $connection -ObjectId $objectId -ComponentType 1
        $result | Should -Not -BeNullOrEmpty
        $result.LogicalName | Should -Be "dependency"
    }
    
    It "Accepts ObjectId parameter from pipeline" {
        $objectId = [Guid]::NewGuid()
        $inputObject = [PSCustomObject]@{ ObjectId = $objectId }
        $result = $inputObject | Get-DataverseDependentComponent -Connection $connection -ComponentType 1
        $result | Should -Not -BeNullOrEmpty
    }
    
    It "Supports ComponentId alias for ObjectId parameter" {
        $componentId = [Guid]::NewGuid()
        $result = Get-DataverseDependentComponent -Connection $connection -ComponentId $componentId -ComponentType 1
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
        $result = Get-DataverseDependentComponent -Connection $connection2 -ObjectId $objectId -ComponentType 1
        $result | Should -BeNullOrEmpty
    }
}
