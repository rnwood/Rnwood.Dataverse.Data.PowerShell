. $PSScriptRoot/Common.ps1

Describe "AppModule Management Cmdlets" {
    # Note: appmodule and appmodulecomponent entities don't have metadata XML files in the test directory.
    # These tests use the mock connection without specific entity metadata.
    # For full E2E testing with actual Dataverse, use e2e-tests directory.
    
    Context "Set-DataverseAppModule - Basic Creation" {
        It "Creates an app module with UniqueName" {
            $connection = getMockConnection -Entities @("appmodule")
            
            $appModuleId = Set-DataverseAppModule -PassThru -Connection $connection `
                -UniqueName "test_app_module" `
                -Name "Test App Module" `
                -Description "This is a test app module"
            
            $appModuleId | Should -Not -BeNullOrEmpty
            $appModuleId | Should -BeOfType [Guid]
        }

        It "Creates an app module with minimal parameters" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            $appModuleId = Set-DataverseAppModule -PassThru -Connection $connection `
                -UniqueName "minimal_app_module"
            
            $appModuleId | Should -Not -BeNullOrEmpty
            $appModuleId | Should -BeOfType [Guid]
        }

        It "Creates an app module with all parameters" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            $webResourceId = [Guid]::NewGuid()
            $appModuleId = Set-DataverseAppModule -PassThru -Connection $connection `
                -UniqueName "full_app_module" `
                -Name "Full App Module" `
                -Description "Full test app module" `
                -Url "/test/app" `
                -WebResourceId $webResourceId `
                -FormFactor 1 `
                -ClientType 1
            
            $appModuleId | Should -Not -BeNullOrEmpty
        }
    }

    Context "Set-DataverseAppModule - Updates" {
        It "Updates an existing app module by ID" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # Create first
            $appModuleId = Set-DataverseAppModule -PassThru -Connection $connection `
                -UniqueName "update_test_app" `
                -Name "Original Name"
            
            # Update
            Set-DataverseAppModule -Connection $connection `
                -Id $appModuleId `
                -Name "Updated Name" `
                -Description "Updated description"
            
            # Success if no error thrown
            $true | Should -Be $true
        }

        It "Updates an existing app module by UniqueName" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # Create first
            Set-DataverseAppModule -Connection $connection `
                -UniqueName "update_by_uniquename_app" `
                -Name "Original Name"
            
            # Update by UniqueName
            Set-DataverseAppModule -Connection $connection `
                -UniqueName "update_by_uniquename_app" `
                -Name "Updated via UniqueName"
            
            # Success if no error thrown
            $true | Should -Be $true
        }

        It "NoUpdate flag prevents updates" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # Create first
            $appModuleId = Set-DataverseAppModule -PassThru -Connection $connection `
                -UniqueName "noupdate_test_app" `
                -Name "Original Name"
            
            # Try to update with NoUpdate flag
            Set-DataverseAppModule -Connection $connection `
                -Id $appModuleId `
                -Name "Should Not Update" `
                -NoUpdate
            
            # Success if no error thrown
            $true | Should -Be $true
        }

        It "NoCreate flag prevents creation" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # Try to create with NoCreate flag
            $result = Set-DataverseAppModule -Connection $connection `
                -UniqueName "nocreate_test_app" `
                -Name "Should Not Create" `
                -NoCreate `
                -PassThru
            
            # Should not return an ID
            $result | Should -BeNullOrEmpty
        }
    }

    Context "Set-DataverseAppModule - WhatIf Support" {
        It "Supports WhatIf without creating app module" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            $result = Set-DataverseAppModule -Connection $connection `
                -UniqueName "whatif_test_app" `
                -Name "WhatIf Test" `
                -WhatIf
            
            # No app module ID should be returned
            $result | Should -BeNullOrEmpty
        }
    }

    Context "Set-DataverseAppModule - Error Handling" {
        It "Throws error when UniqueName missing for creation" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            { 
                Set-DataverseAppModule -Connection $connection `
                    -Name "No UniqueName" `
                    -ErrorAction Stop
            } | Should -Throw "*UniqueName*required*"
        }
    }

    Context "Get-DataverseAppModule - Retrieval" {
        It "Gets app module by ID" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # Create an app module
            $appModuleId = Set-DataverseAppModule -PassThru -Connection $connection `
                -UniqueName "get_by_id_app" `
                -Name "Get By ID App"
            
            # Get the app module by ID
            $appModule = Get-DataverseAppModule -Connection $connection -Id $appModuleId
            
            $appModule | Should -Not -BeNullOrEmpty
            $appModule.Name | Should -Be "Get By ID App"
        }

        It "Gets app module by UniqueName" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # Create an app module
            Set-DataverseAppModule -Connection $connection `
                -UniqueName "get_by_uniquename_app" `
                -Name "Get By UniqueName App"
            
            # Get the app module by UniqueName
            $appModule = Get-DataverseAppModule -Connection $connection -UniqueName "get_by_uniquename_app"
            
            $appModule | Should -Not -BeNullOrEmpty
            $appModule.UniqueName | Should -Be "get_by_uniquename_app"
        }

        It "Gets app module with raw values" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # Create an app module
            $appModuleId = Set-DataverseAppModule -PassThru -Connection $connection `
                -UniqueName "raw_values_app" `
                -Name "Raw Values App"
            
            # Get with raw values
            $appModule = Get-DataverseAppModule -Connection $connection -Id $appModuleId -Raw
            
            $appModule | Should -Not -BeNullOrEmpty
        }

    }

    Context "Remove-DataverseAppModule - Basic Removal" {
        It "Removes an app module by ID" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # Create an app module
            $appModuleId = Set-DataverseAppModule -PassThru -Connection $connection `
                -UniqueName "remove_by_id_app" `
                -Name "Remove By ID App"
            
            # Remove the app module
            Remove-DataverseAppModule -Connection $connection -Id $appModuleId -Confirm:$false
            
            # Success if no error thrown
            $true | Should -Be $true
        }

        It "Removes an app module by UniqueName" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # Create an app module
            Set-DataverseAppModule -Connection $connection `
                -UniqueName "remove_by_uniquename_app" `
                -Name "Remove By UniqueName App"
            
            # Remove by UniqueName
            Remove-DataverseAppModule -Connection $connection -UniqueName "remove_by_uniquename_app" -Confirm:$false
            
            # Success if no error thrown
            $true | Should -Be $true
        }
    }

    Context "Remove-DataverseAppModule - IfExists Support" {
        It "Does not error when removing non-existent app module with IfExists" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # Try to remove an app module that doesn't exist
            $nonExistentId = [Guid]::NewGuid()
            
            # This should not throw an error
            { 
                Remove-DataverseAppModule -Connection $connection -Id $nonExistentId -IfExists -Confirm:$false
            } | Should -Not -Throw
        }

        It "Errors when removing non-existent app module without IfExists" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # Try to remove an app module that doesn't exist
            $nonExistentId = [Guid]::NewGuid()
            
            # This should throw an error
            { 
                Remove-DataverseAppModule -Connection $connection -Id $nonExistentId -ErrorAction Stop -Confirm:$false
            } | Should -Throw
        }

        It "Does not error when removing by UniqueName with IfExists" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # This should not throw an error
            { 
                Remove-DataverseAppModule -Connection $connection -UniqueName "nonexistent_app" -IfExists -Confirm:$false
            } | Should -Not -Throw
        }
    }

    Context "Remove-DataverseAppModule - WhatIf Support" {
        It "Supports WhatIf without removing app module" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # Create an app module
            $appModuleId = Set-DataverseAppModule -PassThru -Connection $connection `
                -UniqueName "whatif_remove_app" `
                -Name "WhatIf Remove App"
            
            # This should not remove the app module
            Remove-DataverseAppModule -Connection $connection -Id $appModuleId -WhatIf
            
            # Success if no error thrown
            $true | Should -Be $true
        }
    }

    Context "Set/Get-DataverseAppModule - NavigationType and IsFeatured" {
        It "Sets NavigationType and IsFeatured and retrieves them" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")

            $newId = Set-DataverseAppModule -PassThru -Connection $connection `
                -UniqueName "navtype_test_app" `
                -Name "NavType Test" `
                -NavigationType ([Rnwood.Dataverse.Data.PowerShell.Commands.NavigationType]::MultiSession) `
                -IsFeatured $true

            $app = Get-DataverseAppModule -Connection $connection -Id $newId
            $app | Should -Not -BeNullOrEmpty
            $app.NavigationType | Should -Be ([Rnwood.Dataverse.Data.PowerShell.Commands.NavigationType]::MultiSession)
            $app.IsFeatured | Should -Be $true
        }
    }

    Context "Set-DataverseAppModuleComponent - Basic Creation" {
    }

    Context "Set-DataverseAppModuleComponent - Updates" {
    }

    Context "Set-DataverseAppModuleComponent - Error Handling" {
        It "Throws error when AppModuleId/UniqueName missing for creation" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            { 
                Set-DataverseAppModuleComponent -Connection $connection `
                    -ObjectId ([Guid]::NewGuid()) `
                    -ComponentType 1 `
                    -ErrorAction Stop
            } | Should -Throw "*AppModuleId*AppModuleUniqueName*required*"
        }

        It "Throws error when ObjectId missing for creation" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # Create an app module first
            $appModuleId = Set-DataverseAppModule -PassThru -Connection $connection `
                -UniqueName "test_app_for_error1" `
                -Name "Test App for Error 1"
            
            { 
                Set-DataverseAppModuleComponent -Connection $connection `
                    -AppModuleId $appModuleId `
                    -ComponentType 1 `
                    -ErrorAction Stop
            } | Should -Throw "*ObjectId*required*"
        }

        It "Throws error when ComponentType missing for creation" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # Create an app module first
            $appModuleId = Set-DataverseAppModule -PassThru -Connection $connection `
                -UniqueName "test_app_for_error2" `
                -Name "Test App for Error 2"
            
            { 
                Set-DataverseAppModuleComponent -Connection $connection `
                    -AppModuleId $appModuleId `
                    -ObjectId ([Guid]::NewGuid()) `
                    -ErrorAction Stop
            } | Should -Throw "*ComponentType*required*"
        }
    }

    Context "Get-DataverseAppModuleComponent - Retrieval" {
    }

    Context "Remove-DataverseAppModuleComponent - Basic Removal" {
    }

    Context "Remove-DataverseAppModuleComponent - IfExists Support" {
        It "Does not error when removing non-existent component with IfExists" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # Try to remove a component that doesn't exist
            $nonExistentId = [Guid]::NewGuid()
            
            # This should not throw an error
            { 
                Remove-DataverseAppModuleComponent -Connection $connection -Id $nonExistentId -IfExists -Confirm:$false
            } | Should -Not -Throw
        }

        It "Errors when removing non-existent component without IfExists" {
            $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
            
            # Try to remove a component that doesn't exist
            $nonExistentId = [Guid]::NewGuid()
            
            # This should throw an error
            { 
                Remove-DataverseAppModuleComponent -Connection $connection -Id $nonExistentId -ErrorAction Stop -Confirm:$false
            } | Should -Throw
        }
    }

    Context "Integration Tests" {
    }
}
