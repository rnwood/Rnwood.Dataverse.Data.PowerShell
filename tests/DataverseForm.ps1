Describe 'Get-DataverseForm' {
    Context 'Basic form retrieval' {
        It 'Should retrieve forms by entity name' {
            $connection = getMockConnection
            
            # Create test form data using CreateRequest directly
            $formId = [Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity("systemform")
            $form.Id = $formId
            $form["formid"] = $formId
            $form["name"] = "Test Form"
            $form["objecttypecode"] = "contact"
            $form["type"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(2)
            $form["description"] = "Test form description"
            $form["formactivationstate"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)
            $form["formpresentation"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(0)
            $form["isdefault"] = $false
            $form["iscustomizable"] = New-Object Microsoft.Xrm.Sdk.BooleanManagedProperty($true)
            
            $createReq = New-Object Microsoft.Xrm.Sdk.Messages.CreateRequest
            $createReq.Target = $form
            Invoke-DataverseRequest -Connection $connection -Request $createReq | Out-Null
            
            $results = Get-DataverseForm -Connection $connection -Entity 'contact'
            
            $results | Should -Not -BeNullOrEmpty
            $results[0].Entity | Should -Be 'contact'
            $results[0].Name | Should -Be 'Test Form'
            $results[0].Type | Should -Be 'Main'
        }

        It 'Should retrieve form by ID' {
            $connection = getMockConnection
            
            $formId = [Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity("systemform")
            $form.Id = $formId
            $form["formid"] = $formId
            $form["name"] = "Test Form By ID"
            $form["objecttypecode"] = "contact"
            $form["type"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(2)
            $form["formactivationstate"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)
            $form["formpresentation"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(0)
            $form["isdefault"] = $false
            $form["iscustomizable"] = New-Object Microsoft.Xrm.Sdk.BooleanManagedProperty($true)
            
            $createReq = New-Object Microsoft.Xrm.Sdk.Messages.CreateRequest
            $createReq.Target = $form
            Invoke-DataverseRequest -Connection $connection -Request $createReq | Out-Null
            
            $result = Get-DataverseForm -Connection $connection -Id $formId
            
            $result | Should -Not -BeNullOrEmpty
            $result.FormId | Should -Be $formId
            $result.Name | Should -Be "Test Form By ID"
        }

        It 'Should retrieve form by entity and name' {
            $connection = getMockConnection
            
            $formId = [Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity("systemform")
            $form.Id = $formId
            $form["formid"] = $formId
            $form["name"] = "Specific Form"
            $form["objecttypecode"] = "contact"
            $form["type"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(2)
            $form["formactivationstate"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)
            $form["formpresentation"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(0)
            $form["isdefault"] = $false
            $form["iscustomizable"] = New-Object Microsoft.Xrm.Sdk.BooleanManagedProperty($true)
            
            $createReq = New-Object Microsoft.Xrm.Sdk.Messages.CreateRequest
            $createReq.Target = $form
            Invoke-DataverseRequest -Connection $connection -Request $createReq | Out-Null
            
            $result = Get-DataverseForm -Connection $connection -Entity 'contact' -Name 'Specific Form'
            
            $result | Should -Not -BeNullOrEmpty
            $result.Name | Should -Be "Specific Form"
        }
    }

    Context 'Form type filtering' {
        It 'Should filter by Main form type' {
            $connection = getMockConnection
            
            # Create forms of different types
            $mainFormId = [Guid]::NewGuid()
            $mainForm = New-Object Microsoft.Xrm.Sdk.Entity("systemform")
            $mainForm.Id = $mainFormId
            $mainForm["formid"] = $mainFormId
            $mainForm["name"] = "Main Form"
            $mainForm["objecttypecode"] = "contact"
            $mainForm["type"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(2)
            $mainForm["formactivationstate"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)
            $mainForm["formpresentation"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(0)
            $mainForm["isdefault"] = $false
            $mainForm["iscustomizable"] = New-Object Microsoft.Xrm.Sdk.BooleanManagedProperty($true)
            
            $createReq = New-Object Microsoft.Xrm.Sdk.Messages.CreateRequest
            $createReq.Target = $mainForm
            Invoke-DataverseRequest -Connection $connection -Request $createReq | Out-Null

            $quickCreateId = [Guid]::NewGuid()
            $quickCreate = New-Object Microsoft.Xrm.Sdk.Entity("systemform")
            $quickCreate.Id = $quickCreateId
            $quickCreate["formid"] = $quickCreateId
            $quickCreate["name"] = "Quick Create Form"
            $quickCreate["objecttypecode"] = "contact"
            $quickCreate["type"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(5)
            $quickCreate["formactivationstate"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)
            $quickCreate["formpresentation"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(0)
            $quickCreate["isdefault"] = $false
            $quickCreate["iscustomizable"] = New-Object Microsoft.Xrm.Sdk.BooleanManagedProperty($true)
            
            $createReq2 = New-Object Microsoft.Xrm.Sdk.Messages.CreateRequest
            $createReq2.Target = $quickCreate
            Invoke-DataverseRequest -Connection $connection -Request $createReq2 | Out-Null
            
            $results = Get-DataverseForm -Connection $connection -Entity 'contact' -FormType 'Main'
            
            $results | Should -Not -BeNullOrEmpty
            $results | Where-Object { $_.Name -eq "Main Form" } | Should -Not -BeNullOrEmpty
            $results | Where-Object { $_.Name -eq "Quick Create Form" } | Should -BeNullOrEmpty
        }

        It 'Should filter by QuickCreate form type' {
            $connection = getMockConnection
            
            # Create forms of different types
            $mainFormId = [Guid]::NewGuid()
            $mainForm = New-Object Microsoft.Xrm.Sdk.Entity("systemform")
            $mainForm.Id = $mainFormId
            $mainForm["formid"] = $mainFormId
            $mainForm["name"] = "Main Form2"
            $mainForm["objecttypecode"] = "contact"
            $mainForm["type"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(2)
            $mainForm["formactivationstate"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)
            $mainForm["formpresentation"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(0)
            $mainForm["isdefault"] = $false
            $mainForm["iscustomizable"] = New-Object Microsoft.Xrm.Sdk.BooleanManagedProperty($true)
            
            $createReq = New-Object Microsoft.Xrm.Sdk.Messages.CreateRequest
            $createReq.Target = $mainForm
            Invoke-DataverseRequest -Connection $connection -Request $createReq | Out-Null

            $quickCreateId = [Guid]::NewGuid()
            $quickCreate = New-Object Microsoft.Xrm.Sdk.Entity("systemform")
            $quickCreate.Id = $quickCreateId
            $quickCreate["formid"] = $quickCreateId
            $quickCreate["name"] = "Quick Create Form2"
            $quickCreate["objecttypecode"] = "contact"
            $quickCreate["type"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(5)
            $quickCreate["formactivationstate"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)
            $quickCreate["formpresentation"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(0)
            $quickCreate["isdefault"] = $false
            $quickCreate["iscustomizable"] = New-Object Microsoft.Xrm.Sdk.BooleanManagedProperty($true)
            
            $createReq2 = New-Object Microsoft.Xrm.Sdk.Messages.CreateRequest
            $createReq2.Target = $quickCreate
            Invoke-DataverseRequest -Connection $connection -Request $createReq2 | Out-Null
            
            $results = Get-DataverseForm -Connection $connection -Entity 'contact' -FormType 'QuickCreate'
            
            $results | Should -Not -BeNullOrEmpty
            $results | Where-Object { $_.Name -eq "Quick Create Form2" } | Should -Not -BeNullOrEmpty
            $results | Where-Object { $_.Name -eq "Main Form2" } | Should -BeNullOrEmpty
        }
    }
}

Describe 'Set-DataverseForm' {
    Context 'Create new form' {
        It 'Should create a new form with minimal parameters' {
            $connection = getMockConnection
            
            $formId = Set-DataverseForm -Connection $connection `
                -Entity 'contact' `
                -Name 'New Test Form' `
                -FormType 'Main' `
                -PassThru
            
            $formId | Should -Not -BeNullOrEmpty
            $formId | Should -BeOfType [Guid]
            
            # Verify form was created
            $form = Get-DataverseForm -Connection $connection -Id $formId
            $form.Name | Should -Be 'New Test Form'
            $form.Entity | Should -Be 'contact'
            $form.Type | Should -Be 'Main'
        }

        It 'Should create form with description' {
            $connection = getMockConnection
            
            $formId = Set-DataverseForm -Connection $connection `
                -Entity 'contact' `
                -Name 'Form With Desc' `
                -FormType 'QuickCreate' `
                -Description 'Test description' `
                -PassThru
            
            $form = Get-DataverseForm -Connection $connection -Id $formId
            $form.Description | Should -Be 'Test description'
        }
    }

    Context 'WhatIf and Confirm support' {
        It 'Should support WhatIf for creation' {
            $connection = getMockConnection
            
            { Set-DataverseForm -Connection $connection `
                -Entity 'contact' `
                -Name 'WhatIf Test' `
                -FormType 'Main' `
                -WhatIf } | Should -Not -Throw
        }
    }
}

Describe 'Remove-DataverseForm' {
    Context 'Delete by ID' {
        It 'Should delete form by ID' {
            $connection = getMockConnection
            
            $formId = [Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity("systemform")
            $form.Id = $formId
            $form["formid"] = $formId
            $form["name"] = "Form To Delete"
            $form["objecttypecode"] = "contact"
            $form["type"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(2)
            $form["formactivationstate"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)
            $form["formpresentation"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(0)
            $form["isdefault"] = $false
            $form["iscustomizable"] = New-Object Microsoft.Xrm.Sdk.BooleanManagedProperty($true)
            
            $createReq = New-Object Microsoft.Xrm.Sdk.Messages.CreateRequest
            $createReq.Target = $form
            Invoke-DataverseRequest -Connection $connection -Request $createReq | Out-Null
            
            Remove-DataverseForm -Connection $connection -Id $formId -Confirm:$false
            
            # Verify form was deleted
            $result = Get-DataverseForm -Connection $connection -Id $formId
            $result | Should -BeNullOrEmpty
        }
    }

    Context 'Delete by name' {
        It 'Should delete form by entity and name' {
            $connection = getMockConnection
            
            $formId = [Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity("systemform")
            $form.Id = $formId
            $form["formid"] = $formId
            $form["name"] = "Named Form To Delete"
            $form["objecttypecode"] = "contact"
            $form["type"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(2)
            $form["formactivationstate"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)
            $form["formpresentation"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(0)
            $form["isdefault"] = $false
            $form["iscustomizable"] = New-Object Microsoft.Xrm.Sdk.BooleanManagedProperty($true)
            
            $createReq = New-Object Microsoft.Xrm.Sdk.Messages.CreateRequest
            $createReq.Target = $form
            Invoke-DataverseRequest -Connection $connection -Request $createReq | Out-Null
            
            Remove-DataverseForm -Connection $connection -Entity 'contact' -Name 'Named Form To Delete' -Confirm:$false
            
            # Verify form was deleted
            $result = Get-DataverseForm -Connection $connection -Entity 'contact' -Name 'Named Form To Delete'
            $result | Should -BeNullOrEmpty
        }

        It 'Should not throw error with IfExists flag when form does not exist' {
            $connection = getMockConnection
            
            { Remove-DataverseForm -Connection $connection -Entity 'contact' -Name 'NonExistent Form' -IfExists -Confirm:$false } | Should -Not -Throw
        }
    }

    Context 'WhatIf and Confirm support' {
        It 'Should support WhatIf' {
            $connection = getMockConnection
            
            $formId = [Guid]::NewGuid()
            
            { Remove-DataverseForm -Connection $connection -Id $formId -WhatIf } | Should -Not -Throw
        }
    }
}
