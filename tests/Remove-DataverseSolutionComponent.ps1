Describe 'Remove-DataverseSolutionComponent' {
    
    It "Cmdlet is available" {
        $command = Get-Command Remove-DataverseSolutionComponent -ErrorAction SilentlyContinue
        $command | Should -Not -BeNullOrEmpty
        $command.Name | Should -Be "Remove-DataverseSolutionComponent"
    }

    It "Has required parameters" {
        $command = Get-Command Remove-DataverseSolutionComponent
        $command.Parameters.Keys | Should -Contain "SolutionName"
        $command.Parameters.Keys | Should -Contain "ComponentId"
        $command.Parameters.Keys | Should -Contain "ComponentType"
        $command.Parameters.Keys | Should -Contain "Connection"
    }

    It "SolutionName parameter is mandatory" {
        $command = Get-Command Remove-DataverseSolutionComponent
        $command.Parameters["SolutionName"].Attributes.Where{$_ -is [Parameter]}.Mandatory | Should -Be $true
    }

    It "ComponentId parameter is mandatory" {
        $command = Get-Command Remove-DataverseSolutionComponent
        $command.Parameters["ComponentId"].Attributes.Where{$_ -is [Parameter]}.Mandatory | Should -Be $true
    }

    It "ComponentType parameter is mandatory" {
        $command = Get-Command Remove-DataverseSolutionComponent
        $command.Parameters["ComponentType"].Attributes.Where{$_ -is [Parameter]}.Mandatory | Should -Be $true
    }

    It "Supports ShouldProcess" {
        $command = Get-Command Remove-DataverseSolutionComponent
        $command.Parameters.Keys | Should -Contain "WhatIf"
        $command.Parameters.Keys | Should -Contain "Confirm"
    }

    It "Has IfExists parameter" {
        $command = Get-Command Remove-DataverseSolutionComponent
        $command.Parameters.Keys | Should -Contain "IfExists"
    }

    It "ComponentId parameter has ObjectId alias" {
        $command = Get-Command Remove-DataverseSolutionComponent
        $aliases = $command.Parameters["ComponentId"].Aliases
        $aliases | Should -Contain "ObjectId"
    }
}
