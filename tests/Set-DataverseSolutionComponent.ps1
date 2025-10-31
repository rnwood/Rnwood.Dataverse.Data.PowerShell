Describe 'Set-DataverseSolutionComponent' {
    
    It "Cmdlet is available" {
        $command = Get-Command Set-DataverseSolutionComponent -ErrorAction SilentlyContinue
        $command | Should -Not -BeNullOrEmpty
        $command.Name | Should -Be "Set-DataverseSolutionComponent"
    }

    It "Has required parameters" {
        $command = Get-Command Set-DataverseSolutionComponent
        $command.Parameters.Keys | Should -Contain "SolutionName"
        $command.Parameters.Keys | Should -Contain "ComponentId"
        $command.Parameters.Keys | Should -Contain "ComponentType"
        $command.Parameters.Keys | Should -Contain "Behavior"
        $command.Parameters.Keys | Should -Contain "Connection"
    }

    It "SolutionName parameter is mandatory" {
        $command = Get-Command Set-DataverseSolutionComponent
        $command.Parameters["SolutionName"].Attributes.Where{$_ -is [Parameter]}.Mandatory | Should -Be $true
    }

    It "ComponentId parameter is mandatory" {
        $command = Get-Command Set-DataverseSolutionComponent
        $command.Parameters["ComponentId"].Attributes.Where{$_ -is [Parameter]}.Mandatory | Should -Be $true
    }

    It "ComponentType parameter is mandatory" {
        $command = Get-Command Set-DataverseSolutionComponent
        $command.Parameters["ComponentType"].Attributes.Where{$_ -is [Parameter]}.Mandatory | Should -Be $true
    }

    It "Behavior parameter has default value of 0" {
        $command = Get-Command Set-DataverseSolutionComponent
        $command.Parameters["Behavior"].Attributes.Where{$_ -is [Parameter]}.Mandatory | Should -Be $false
    }

    It "Supports ShouldProcess" {
        $command = Get-Command Set-DataverseSolutionComponent
        $command.Parameters.Keys | Should -Contain "WhatIf"
        $command.Parameters.Keys | Should -Contain "Confirm"
    }

    It "Has PassThru parameter" {
        $command = Get-Command Set-DataverseSolutionComponent
        $command.Parameters.Keys | Should -Contain "PassThru"
    }

    It "Has AddRequiredComponents parameter" {
        $command = Get-Command Set-DataverseSolutionComponent
        $command.Parameters.Keys | Should -Contain "AddRequiredComponents"
    }

    It "Has DoNotIncludeSubcomponents parameter" {
        $command = Get-Command Set-DataverseSolutionComponent
        $command.Parameters.Keys | Should -Contain "DoNotIncludeSubcomponents"
    }

    It "ComponentId parameter has ObjectId alias" {
        $command = Get-Command Set-DataverseSolutionComponent
        $aliases = $command.Parameters["ComponentId"].Aliases
        $aliases | Should -Contain "ObjectId"
    }

    It "Behavior parameter validates range 0-2" {
        $command = Get-Command Set-DataverseSolutionComponent
        $validateRange = $command.Parameters["Behavior"].Attributes.Where{$_ -is [ValidateRange]}
        $validateRange | Should -Not -BeNullOrEmpty
        $validateRange[0].MinRange | Should -Be 0
        $validateRange[0].MaxRange | Should -Be 2
    }
}
