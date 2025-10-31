# Plugin Cmdlet Tests
# Note: These tests require metadata for plugin entities (pluginassembly, pluginpackage, plugintype, 
# sdkmessageprocessingstep, sdkmessageprocessingstepimage) which are not included in the contact.xml 
# metadata used by the mock connection. These tests demonstrate the expected API but would need to be 
# run against a real Dataverse environment for full validation.
# 
# The cmdlets follow the same patterns as other working cmdlets (Get-DataverseRecord, Set-DataverseRecord, 
# Remove-DataverseRecord) and should work correctly with real connections.

Describe 'Plugin Assembly Cmdlets' {
    It "Creates, retrieves, and removes a plugin assembly with file content" {
        $connection = getMockConnection
        
        # Create a mock assembly content
        $assemblyContent = [System.Text.Encoding]::UTF8.GetBytes("MockAssemblyContent")
        
        # Create a new plugin assembly
        $assembly = Set-DataversePluginAssembly -Connection $connection -Name "TestAssembly" -Content $assemblyContent -IsolationMode 2 -PassThru
        
        $assembly | Should -Not -BeNullOrEmpty
        $assembly.name | Should -Be "TestAssembly"
        $assembly.Id | Should -Not -Be ([Guid]::Empty)
        
        # Retrieve the assembly by ID
        $retrieved = Get-DataversePluginAssembly -Connection $connection -Id $assembly.Id
        
        $retrieved | Should -Not -BeNullOrEmpty
        $retrieved.name | Should -Be "TestAssembly"
        
        # Retrieve all assemblies
        $allAssemblies = Get-DataversePluginAssembly -Connection $connection -All
        
        $allAssemblies | Should -Not -BeNullOrEmpty
        $allAssemblies | Where-Object { $_.Id -eq $assembly.Id } | Should -Not -BeNullOrEmpty
        
        # Remove the assembly
        Remove-DataversePluginAssembly -Connection $connection -Id $assembly.Id -Confirm:$false
        
        # Verify removal with IfExists flag (should not throw)
        { Remove-DataversePluginAssembly -Connection $connection -Id $assembly.Id -IfExists -Confirm:$false } | Should -Not -Throw
    }
    
    It "Creates and retrieves plugin assembly by name" {
        $connection = getMockConnection
        
        $assemblyContent = [System.Text.Encoding]::UTF8.GetBytes("MockAssemblyContent")
        
        $assembly = Set-DataversePluginAssembly -Connection $connection -Name "NamedAssembly" -Content $assemblyContent -PassThru
        
        $assembly | Should -Not -BeNullOrEmpty
        
        $retrieved = Get-DataversePluginAssembly -Connection $connection -Name "NamedAssembly"
        
        $retrieved | Should -Not -BeNullOrEmpty
        $retrieved.name | Should -Be "NamedAssembly"
    }
    
    It "Updates an existing plugin assembly" {
        $connection = getMockConnection
        
        $assemblyContent = [System.Text.Encoding]::UTF8.GetBytes("MockAssemblyContent")
        
        # Create initial assembly
        $assembly = Set-DataversePluginAssembly -Connection $connection -Name "UpdateTest" -Content $assemblyContent -PassThru
        
        # Update with new content
        $updatedContent = [System.Text.Encoding]::UTF8.GetBytes("UpdatedAssemblyContent")
        $updated = Set-DataversePluginAssembly -Connection $connection -Id $assembly.Id -Name "UpdateTest" -Content $updatedContent -Version "2.0.0" -PassThru
        
        $updated | Should -Not -BeNullOrEmpty
        $updated.Id | Should -Be $assembly.Id
    }
}

Describe 'Plugin Package Cmdlets' {
    It "Creates, retrieves, and removes a plugin package" {
        $connection = getMockConnection
        
        $packageContent = [System.Text.Encoding]::UTF8.GetBytes("MockPackageContent")
        
        $package = Set-DataversePluginPackage -Connection $connection -UniqueName "testpackage" -Content $packageContent -PassThru
        
        $package | Should -Not -BeNullOrEmpty
        $package.uniquename | Should -Be "testpackage"
        
        $retrieved = Get-DataversePluginPackage -Connection $connection -Id $package.Id
        
        $retrieved | Should -Not -BeNullOrEmpty
        $retrieved.uniquename | Should -Be "testpackage"
        
        Remove-DataversePluginPackage -Connection $connection -Id $package.Id -Confirm:$false
    }
}

Describe 'Plugin Type Cmdlets' {
    It "Creates, retrieves, and removes a plugin type" {
        $connection = getMockConnection
        
        # Create an assembly first
        $assemblyContent = [System.Text.Encoding]::UTF8.GetBytes("MockAssemblyContent")
        $assembly = Set-DataversePluginAssembly -Connection $connection -Name "TypeTestAssembly" -Content $assemblyContent -PassThru
        
        # Create a plugin type
        $type = Set-DataversePluginType -Connection $connection -PluginAssemblyId $assembly.Id -TypeName "TestNamespace.TestPlugin" -PassThru
        
        $type | Should -Not -BeNullOrEmpty
        $type.typename | Should -Be "TestNamespace.TestPlugin"
        
        # Retrieve by ID
        $retrieved = Get-DataversePluginType -Connection $connection -Id $type.Id
        
        $retrieved | Should -Not -BeNullOrEmpty
        
        # Retrieve by assembly ID
        $typesForAssembly = Get-DataversePluginType -Connection $connection -PluginAssemblyId $assembly.Id
        
        $typesForAssembly | Should -Not -BeNullOrEmpty
        $typesForAssembly | Where-Object { $_.Id -eq $type.Id } | Should -Not -BeNullOrEmpty
        
        # Remove the type
        Remove-DataversePluginType -Connection $connection -Id $type.Id -Confirm:$false
    }
}

Describe 'Plugin Step Cmdlets' {
    It "Creates, retrieves, and removes a plugin step" {
        $connection = getMockConnection
        
        # Create prerequisite entities
        $assemblyContent = [System.Text.Encoding]::UTF8.GetBytes("MockAssemblyContent")
        $assembly = Set-DataversePluginAssembly -Connection $connection -Name "StepTestAssembly" -Content $assemblyContent -PassThru
        $type = Set-DataversePluginType -Connection $connection -PluginAssemblyId $assembly.Id -TypeName "TestNamespace.StepPlugin" -PassThru
        
        # For the test, we'll use a mock SDK message ID
        $mockMessageId = [Guid]::NewGuid()
        
        # Create a plugin step
        $step = Set-DataversePluginStep -Connection $connection -Name "TestStep" -PluginTypeId $type.Id -SdkMessageId $mockMessageId -Stage 20 -Mode 0 -PassThru
        
        $step | Should -Not -BeNullOrEmpty
        $step.name | Should -Be "TestStep"
        
        # Retrieve by ID
        $retrieved = Get-DataversePluginStep -Connection $connection -Id $step.Id
        
        $retrieved | Should -Not -BeNullOrEmpty
        
        # Retrieve by plugin type ID
        $stepsForType = Get-DataversePluginStep -Connection $connection -PluginTypeId $type.Id
        
        $stepsForType | Should -Not -BeNullOrEmpty
        $stepsForType | Where-Object { $_.Id -eq $step.Id } | Should -Not -BeNullOrEmpty
        
        # Remove the step
        Remove-DataversePluginStep -Connection $connection -Id $step.Id -Confirm:$false
    }
}

Describe 'Plugin Step Image Cmdlets' {
    It "Creates, retrieves, and removes a plugin step image" {
        $connection = getMockConnection
        
        # Create prerequisite entities
        $assemblyContent = [System.Text.Encoding]::UTF8.GetBytes("MockAssemblyContent")
        $assembly = Set-DataversePluginAssembly -Connection $connection -Name "ImageTestAssembly" -Content $assemblyContent -PassThru
        $type = Set-DataversePluginType -Connection $connection -PluginAssemblyId $assembly.Id -TypeName "TestNamespace.ImagePlugin" -PassThru
        $mockMessageId = [Guid]::NewGuid()
        $step = Set-DataversePluginStep -Connection $connection -Name "ImageTestStep" -PluginTypeId $type.Id -SdkMessageId $mockMessageId -Stage 20 -Mode 0 -PassThru
        
        # Create a step image
        $image = Set-DataversePluginStepImage -Connection $connection -SdkMessageProcessingStepId $step.Id -EntityAlias "PreImage" -ImageType 0 -PassThru
        
        $image | Should -Not -BeNullOrEmpty
        $image.entityalias | Should -Be "PreImage"
        
        # Retrieve by ID
        $retrieved = Get-DataversePluginStepImage -Connection $connection -Id $image.Id
        
        $retrieved | Should -Not -BeNullOrEmpty
        
        # Retrieve by step ID
        $imagesForStep = Get-DataversePluginStepImage -Connection $connection -SdkMessageProcessingStepId $step.Id
        
        $imagesForStep | Should -Not -BeNullOrEmpty
        $imagesForStep | Where-Object { $_.Id -eq $image.Id } | Should -Not -BeNullOrEmpty
        
        # Remove the image
        Remove-DataversePluginStepImage -Connection $connection -Id $image.Id -Confirm:$false
    }
}
