. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseRelationshipMetadata - Parameter Definition' {
    Context 'Parameter Validation' {
        It "Has IntersectEntitySchemaName parameter" {
            $command = Get-Command Set-DataverseRelationshipMetadata
            $command | Should -Not -BeNullOrEmpty
            
            $param = $command.Parameters['IntersectEntitySchemaName']
            $param | Should -Not -BeNullOrEmpty
            $param.ParameterType.Name | Should -Be 'String'
        }
        
        It "Has IntersectEntityName as alias for IntersectEntitySchemaName" {
            $command = Get-Command Set-DataverseRelationshipMetadata
            $param = $command.Parameters['IntersectEntitySchemaName']
            $param.Aliases | Should -Contain 'IntersectEntityName'
        }
    }
}
