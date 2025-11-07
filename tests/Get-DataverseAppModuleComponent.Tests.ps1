. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseAppModuleComponent' {
    It "Can be called with AppModuleUniqueName parameter" {
        $connection = getMockConnection
        
        # This test validates that the cmdlet accepts the AppModuleUniqueName parameter
        # and doesn't throw errors during basic invocation
        { Get-DataverseAppModuleComponent -Connection $connection -AppModuleUniqueName "TestApp" } | Should -Not -Throw
    }
    
    It "Can be called with AppModuleId parameter" {
        $connection = getMockConnection
        
        # Test the existing AppModuleId parameter still works
        $testGuid = [Guid]::NewGuid()
        { Get-DataverseAppModuleComponent -Connection $connection -AppModuleId $testGuid } | Should -Not -Throw
    }
    
    It "Can be called with both AppModuleUniqueName and ComponentType parameters" {
        $connection = getMockConnection
        
        # Test combination of new parameter with existing ones
        { Get-DataverseAppModuleComponent -Connection $connection -AppModuleUniqueName "TestApp" -ComponentType Entity } | Should -Not -Throw
    }
    
    It "Returns empty results when no components match AppModuleUniqueName" {
        $connection = getMockConnection
        
        # Test that querying by a non-existent app module unique name returns empty results
        $result = Get-DataverseAppModuleComponent -Connection $connection -AppModuleUniqueName "NonExistentApp"
        $result | Should -BeNullOrEmpty
    }
}