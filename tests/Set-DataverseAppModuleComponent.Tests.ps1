. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseAppModuleComponent' {
    BeforeEach {
        $connection = getMockConnection
    }

    Context "Parameter validation" {
        It "Should throw error when AppModuleIdValue is missing for creation" {
            $objectId = [Guid]::NewGuid()

            # Test validation for required AppModuleIdValue
            { Set-DataverseAppModuleComponent -Connection $connection -ObjectId $objectId -ComponentType ([Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::Entity) } | Should -Throw "*AppModuleIdValue is required*"
        }

        It "Should throw error when ObjectId is missing for creation" {
            $appModuleId = [Guid]::NewGuid()

            # Test validation for required ObjectId
            { Set-DataverseAppModuleComponent -Connection $connection -AppModuleIdValue $appModuleId -ComponentType ([Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::Entity) } | Should -Throw "*ObjectId is required*"
        }

        It "Should throw error when ComponentType is missing for creation" {
            $appModuleId = [Guid]::NewGuid()
            $objectId = [Guid]::NewGuid()

            # Test validation for required ComponentType
            { Set-DataverseAppModuleComponent -Connection $connection -AppModuleIdValue $appModuleId -ObjectId $objectId } | Should -Throw "*ComponentType is required*"
        }

        It "Should skip creation when NoCreate flag is specified and component doesn't exist" {
            $appModuleId = [Guid]::NewGuid()
            $objectId = [Guid]::NewGuid()

            # Test NoCreate flag - this should exit gracefully without throwing
            { Set-DataverseAppModuleComponent -Connection $connection -AppModuleIdValue $appModuleId -ObjectId $objectId -ComponentType ([Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::Entity) -NoCreate } | Should -Not -Throw
        }
    }

    Context "Updating existing app module components" {
        It "Should skip update when NoUpdate flag is specified" {
            $componentId = [Guid]::NewGuid()

            # Test NoUpdate flag - component won't be found in mock but should handle gracefully
            { Set-DataverseAppModuleComponent -Connection $connection -Id $componentId -RootComponentBehavior ([Rnwood.Dataverse.Data.PowerShell.Commands.Model.RootComponentBehavior]::IncludeAsShell) -NoUpdate } | Should -Not -Throw
        }

        It "Should handle non-existent component ID gracefully when updating" {
            $componentId = [Guid]::NewGuid()

            # Test updating non-existent component - should handle gracefully in mock environment
            { Set-DataverseAppModuleComponent -Connection $connection -Id $componentId -RootComponentBehavior ([Rnwood.Dataverse.Data.PowerShell.Commands.Model.RootComponentBehavior]::DoNotIncludeSubcomponents) -IsDefault $true -IsMetadata $false } | Should -Not -Throw
        }
    }

    Context "Component type validation" {
        It "Should accept all valid AppModuleComponentType enum values for table name mapping" {
            # Test that GetTableNameForComponentType method works for all enum values
            # We can't actually execute the creation due to mock limitations, but we can test parameter binding
            $appModuleId = [Guid]::NewGuid()
            $objectId = [Guid]::NewGuid()

            $componentTypes = @(
                [Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::Entity,
                [Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::View,
                [Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::BusinessProcessFlow,
                [Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::RibbonCommand,
                [Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::Chart,
                [Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::Form,
                [Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::SiteMap
            )
            
            # Each should at least not fail parameter binding
            $componentTypes | ForEach-Object {
                $componentType = $_
                try {
                    Set-DataverseAppModuleComponent -Connection $connection -AppModuleIdValue $appModuleId -ObjectId $objectId -ComponentType $componentType -ErrorAction Stop
                } catch {
                    # We expect AddAppComponentsRequest to fail in mock environment, but parameter binding should work
                    $_.Exception.Message | Should -Not -Match "Cannot bind parameter.*ComponentType"
                }
            }
        }
    }

    Context "Root component behavior validation" {
        It "Should accept all valid RootComponentBehavior enum values" {
            $appModuleId = [Guid]::NewGuid()
            $objectId = [Guid]::NewGuid()

            $behaviors = @(
                [Rnwood.Dataverse.Data.PowerShell.Commands.Model.RootComponentBehavior]::IncludeSubcomponents,
                [Rnwood.Dataverse.Data.PowerShell.Commands.Model.RootComponentBehavior]::DoNotIncludeSubcomponents,
                [Rnwood.Dataverse.Data.PowerShell.Commands.Model.RootComponentBehavior]::IncludeAsShell
            )
            
            # Test that parameter binding works for all enum values
            $behaviors | ForEach-Object {
                $behavior = $_
                try {
                    Set-DataverseAppModuleComponent -Connection $connection -AppModuleIdValue $appModuleId -ObjectId $objectId -ComponentType ([Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::Entity) -RootComponentBehavior $behavior -ErrorAction Stop
                } catch {
                    # We expect AddAppComponentsRequest to fail in mock environment, but parameter binding should work
                    $_.Exception.Message | Should -Not -Match "Cannot bind parameter.*RootComponentBehavior"
                }
            }
        }
    }

    Context "WhatIf and Confirm support" {
        It "Should support WhatIf parameter" {
            $appModuleId = [Guid]::NewGuid()
            $objectId = [Guid]::NewGuid()

            # Test WhatIf support - should not actually execute
            { Set-DataverseAppModuleComponent -Connection $connection -AppModuleIdValue $appModuleId -ObjectId $objectId -ComponentType ([Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::Entity) -WhatIf } | Should -Not -Throw
        }

        It "Should support Confirm parameter" {
            $appModuleId = [Guid]::NewGuid()
            $objectId = [Guid]::NewGuid()

            # Test Confirm support with confirmation disabled
            try {
                Set-DataverseAppModuleComponent -Connection $connection -AppModuleIdValue $appModuleId -ObjectId $objectId -ComponentType ([Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::Entity) -Confirm:$false
            } catch {
                # Expected to fail due to mock limitations, but should not be a parameter binding issue
                $_.Exception.Message | Should -Not -Match "Cannot bind parameter.*Confirm"
            }
        }
    }

    Context "Pipeline support" {
        It "Should accept pipeline input with proper property binding" {
            # Create test objects with pipeline properties
            $testObjects = @(
                [PSCustomObject]@{
                    AppModuleIdValue = [Guid]::NewGuid()
                    ObjectId = [Guid]::NewGuid()
                    ComponentType = [Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::Entity
                    RootComponentBehavior = [Rnwood.Dataverse.Data.PowerShell.Commands.Model.RootComponentBehavior]::IncludeSubcomponents
                    IsDefault = $true
                }
                [PSCustomObject]@{
                    Id = [Guid]::NewGuid()
                    RootComponentBehavior = [Rnwood.Dataverse.Data.PowerShell.Commands.Model.RootComponentBehavior]::DoNotIncludeSubcomponents
                    IsMetadata = $false
                }
            )

            # Test pipeline input - should not fail due to parameter binding issues
            try {
                $testObjects | Set-DataverseAppModuleComponent -Connection $connection
            } catch {
                # Expected failures due to mock limitations, but not parameter binding
                foreach ($exception in $Error) {
                    $exception.Exception.Message | Should -Not -Match "Cannot bind parameter"
                }
            }
        }
    }

    Context "Cmdlet attributes and metadata" {
        It "Should have proper cmdlet attributes defined" {
            $cmdlet = Get-Command Set-DataverseAppModuleComponent
            $cmdlet | Should -Not -BeNullOrEmpty
            $cmdlet.Verb | Should -Be "Set"
            $cmdlet.Noun | Should -Be "DataverseAppModuleComponent"
        }

        It "Should support ShouldProcess for WhatIf/Confirm" {
            $cmdlet = Get-Command Set-DataverseAppModuleComponent
            $cmdlet.Parameters.Keys | Should -Contain "WhatIf"
            $cmdlet.Parameters.Keys | Should -Contain "Confirm"
        }

        It "Should have proper parameter sets defined" {
            $cmdlet = Get-Command Set-DataverseAppModuleComponent
            $cmdlet.Parameters.Keys | Should -Contain "Id"
            $cmdlet.Parameters.Keys | Should -Contain "AppModuleIdValue" 
            $cmdlet.Parameters.Keys | Should -Contain "AppModuleUniqueName"
            $cmdlet.Parameters.Keys | Should -Contain "ObjectId"
            $cmdlet.Parameters.Keys | Should -Contain "ComponentType"
            $cmdlet.Parameters.Keys | Should -Contain "RootComponentBehavior"
            $cmdlet.Parameters.Keys | Should -Contain "IsDefault"
            $cmdlet.Parameters.Keys | Should -Contain "IsMetadata"
            $cmdlet.Parameters.Keys | Should -Contain "NoUpdate"
            $cmdlet.Parameters.Keys | Should -Contain "NoCreate"
            $cmdlet.Parameters.Keys | Should -Contain "PassThru"
        }
    }

    Context "Alias support" {
        It "Should support AppModuleId as an alias for AppModuleIdValue" {
            $appModuleId = [Guid]::NewGuid()
            $objectId = [Guid]::NewGuid()

            # Test using the alias instead of the main parameter name
            try {
                Set-DataverseAppModuleComponent -Connection $connection -AppModuleId $appModuleId -ObjectId $objectId -ComponentType ([Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::Entity) -ErrorAction Stop
            } catch {
                # We expect AddAppComponentsRequest to fail in mock environment, but parameter binding should work
                $_.Exception.Message | Should -Not -Match "Cannot bind parameter.*AppModuleId"
            }
        }
    }

    Context "AppModuleUniqueName support" {
        It "Should support AppModuleUniqueName parameter for app module identification" {
            $objectId = [Guid]::NewGuid()

            # Test that parameter binding works for AppModuleUniqueName
            try {
                Set-DataverseAppModuleComponent -Connection $connection -AppModuleUniqueName "TestApp" -ObjectId $objectId -ComponentType ([Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::Entity) -ErrorAction Stop
            } catch {
                # We expect the query for appmodule to fail in mock environment, but parameter binding should work
                $_.Exception.Message | Should -Not -Match "Cannot bind parameter.*AppModuleUniqueName"
            }
        }
    }
}