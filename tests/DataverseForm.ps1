Describe 'Get-DataverseForm' {
    BeforeAll {
        $connection = getMockConnection
    }

    Context 'Basic form retrieval' {
        It 'Should retrieve forms by entity name' {
            # Create test form data using Set-DataverseRecord
            $formId = [Guid]::NewGuid()
            @{
                formid = $formId
                name = "Test Form"
                objecttypecode = "contact"
                type = 2  # Main form
                description = "Test form description"
                formactivationstate = 1  # Active
                formpresentation = 0  # ClassicForm
                isdefault = $false
            } | Set-DataverseRecord -Connection $connection -TableName systemform -Id $formId
            
            $results = Get-DataverseForm -Connection $connection -Entity 'contact'
            
            $results | Should -Not -BeNullOrEmpty
            $results[0].Entity | Should -Be 'contact'
            $results[0].Name | Should -Be 'Test Form'
            $results[0].Type | Should -Be 'Main'
        }

        It 'Should retrieve form by ID' {
            $formId = [Guid]::NewGuid()
            @{
                formid = $formId
                name = "Test Form By ID"
                objecttypecode = "contact"
                type = 2
                formactivationstate = 1
                formpresentation = 0
                isdefault = $false
            } | Set-DataverseRecord -Connection $connection -TableName systemform -Id $formId
            
            $result = Get-DataverseForm -Connection $connection -Id $formId
            
            $result | Should -Not -BeNullOrEmpty
            $result.FormId | Should -Be $formId
            $result.Name | Should -Be "Test Form By ID"
        }

        It 'Should retrieve form by entity and name' {
            $formId = [Guid]::NewGuid()
            @{
                formid = $formId
                name = "Specific Form"
                objecttypecode = "contact"
                type = 2
                formactivationstate = 1
                formpresentation = 0
                isdefault = $false
            } | Set-DataverseRecord -Connection $connection -TableName systemform -Id $formId
            
            $result = Get-DataverseForm -Connection $connection -Entity 'contact' -Name 'Specific Form'
            
            $result | Should -Not -BeNullOrEmpty
            $result.Name | Should -Be "Specific Form"
        }
    }

    Context 'Form type filtering' {
        BeforeEach {
            # Create forms of different types
            $mainFormId = [Guid]::NewGuid()
            @{
                formid = $mainFormId
                name = "Main Form"
                objecttypecode = "contact"
                type = 2  # Main
                formactivationstate = 1
                formpresentation = 0
                isdefault = $false
            } | Set-DataverseRecord -Connection $connection -TableName systemform -Id $mainFormId

            $quickCreateId = [Guid]::NewGuid()
            @{
                formid = $quickCreateId
                name = "Quick Create Form"
                objecttypecode = "contact"
                type = 5  # QuickCreate
                formactivationstate = 1
                formpresentation = 0
                isdefault = $false
            } | Set-DataverseRecord -Connection $connection -TableName systemform -Id $quickCreateId
        }

        It 'Should filter by Main form type' {
            $results = Get-DataverseForm -Connection $connection -Entity 'contact' -FormType 'Main'
            
            $results | Should -Not -BeNullOrEmpty
            $results | Where-Object { $_.Name -eq "Main Form" } | Should -Not -BeNullOrEmpty
            $results | Where-Object { $_.Name -eq "Quick Create Form" } | Should -BeNullOrEmpty
        }

        It 'Should filter by QuickCreate form type' {
            $results = Get-DataverseForm -Connection $connection -Entity 'contact' -FormType 'QuickCreate'
            
            $results | Should -Not -BeNullOrEmpty
            $results | Where-Object { $_.Name -eq "Quick Create Form" } | Should -Not -BeNullOrEmpty
            $results | Where-Object { $_.Name -eq "Main Form" } | Should -BeNullOrEmpty
        }
    }

    Context 'FormXml handling' {
        BeforeAll {
            $formId = [Guid]::NewGuid()
            $formXml = @"
<forms type="main">
  <SystemForm>
    <formid>{$($formId.ToString('B').ToUpper())}</formid>
    <FormPresentation>0</FormPresentation>
    <tabs>
      <tab name="general" id="{$([Guid]::NewGuid().ToString('B'))}" expanded="true" showlabel="false" verticallayout="true">
        <labels>
          <label description="General" languagecode="1033" />
        </labels>
        <columns>
          <column width="100%">
            <sections>
              <section name="section_1" showlabel="false" showbar="false" id="{$([Guid]::NewGuid().ToString('B'))}" columns="1">
                <labels>
                  <label description="Section" languagecode="1033" />
                </labels>
                <rows>
                  <row>
                    <cell>
                      <control id="firstname" datafieldname="firstname" classid="{4273EDBD-AC1D-40D3-9FB2-095C621B552D}" />
                    </cell>
                  </row>
                </rows>
              </section>
            </sections>
          </column>
        </columns>
      </tab>
    </tabs>
    <Navigation />
    <footer />
    <events />
  </SystemForm>
</forms>
"@
            $form = New-Object Microsoft.Xrm.Sdk.Entity("systemform")
            $form.Id = $formId
            $form["formid"] = $formId
            $form["name"] = "Form With XML"
            $form["objecttypecode"] = "contact"
            $form["type"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(2)
            $form["formxml"] = $formXml
            $form["formactivationstate"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)
            $form["formpresentation"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(0)
            $form["isdefault"] = $false
            $form["iscustomizable"] = New-Object Microsoft.Xrm.Sdk.BooleanManagedProperty($true)
            
            $connection.GetOrganizationService().Create($form) | Out-Null
        }

        It 'Should not include FormXml by default' {
            $result = Get-DataverseForm -Connection $connection -Entity 'contact' -Name 'Form With XML'
            
            $result.PSObject.Properties.Name | Should -Not -Contain 'FormXml'
        }

        It 'Should include FormXml when requested' {
            $result = Get-DataverseForm -Connection $connection -Entity 'contact' -Name 'Form With XML' -IncludeFormXml
            
            $result.FormXml | Should -Not -BeNullOrEmpty
            $result.FormXml | Should -Match '<SystemForm>'
        }

        It 'Should parse FormXml when requested' {
            $result = Get-DataverseForm -Connection $connection -Entity 'contact' -Name 'Form With XML' -ParseFormXml
            
            $result.ParsedForm | Should -Not -BeNullOrEmpty
            $result.ParsedForm.Tabs | Should -Not -BeNullOrEmpty
            $result.ParsedForm.Tabs.Count | Should -BeGreaterThan 0
        }
    }
}

Describe 'Set-DataverseForm' {
    BeforeAll {
        $connection = getMockConnection
    }

    Context 'Create new form' {
        It 'Should create a new form with minimal parameters' {
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
            $formId = Set-DataverseForm -Connection $connection `
                -Entity 'contact' `
                -Name 'Form With Desc' `
                -FormType 'QuickCreate' `
                -Description 'Test description' `
                -PassThru
            
            $form = Get-DataverseForm -Connection $connection -Id $formId
            $form.Description | Should -Be 'Test description'
        }

        It 'Should create form with custom FormXml' {
            $customXml = @"
<forms type="main">
  <SystemForm>
    <formid>{PLACEHOLDER}</formid>
    <FormPresentation>1</FormPresentation>
    <tabs>
      <tab name="custom_tab" id="{$([Guid]::NewGuid().ToString('B'))}" expanded="true">
        <labels>
          <label description="Custom Tab" languagecode="1033" />
        </labels>
        <columns>
          <column width="100%">
            <sections />
          </column>
        </columns>
      </tab>
    </tabs>
    <Navigation />
    <footer />
    <events />
  </SystemForm>
</forms>
"@
            $formId = Set-DataverseForm -Connection $connection `
                -Entity 'contact' `
                -Name 'Form With Custom XML' `
                -FormType 'Main' `
                -FormXmlContent $customXml `
                -PassThru
            
            $form = Get-DataverseForm -Connection $connection -Id $formId -IncludeFormXml
            $form.FormXml | Should -Match 'custom_tab'
            $form.FormXml | Should -Match $formId.ToString('B').ToUpper()
        }
    }

    Context 'Update existing form' {
        BeforeAll {
            $existingFormId = [Guid]::NewGuid()
            $existingForm = New-Object Microsoft.Xrm.Sdk.Entity("systemform")
            $existingForm.Id = $existingFormId
            $existingForm["formid"] = $existingFormId
            $existingForm["name"] = "Existing Form"
            $existingForm["objecttypecode"] = "contact"
            $existingForm["type"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(2)
            $existingForm["description"] = "Original description"
            $existingForm["formactivationstate"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)
            $existingForm["formpresentation"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(0)
            $existingForm["isdefault"] = $false
            $existingForm["iscustomizable"] = New-Object Microsoft.Xrm.Sdk.BooleanManagedProperty($true)
            
            $connection.GetOrganizationService().Create($existingForm) | Out-Null
        }

        It 'Should update form name' {
            Set-DataverseForm -Connection $connection `
                -Id $existingFormId `
                -Name 'Updated Form Name'
            
            $form = Get-DataverseForm -Connection $connection -Id $existingFormId
            $form.Name | Should -Be 'Updated Form Name'
        }

        It 'Should update form description' {
            Set-DataverseForm -Connection $connection `
                -Id $existingFormId `
                -Description 'Updated description'
            
            $form = Get-DataverseForm -Connection $connection -Id $existingFormId
            $form.Description | Should -Be 'Updated description'
        }
    }

    Context 'WhatIf and Confirm support' {
        It 'Should support WhatIf for creation' {
            { Set-DataverseForm -Connection $connection `
                -Entity 'contact' `
                -Name 'WhatIf Test' `
                -FormType 'Main' `
                -WhatIf } | Should -Not -Throw
        }

        It 'Should support WhatIf for update' {
            $formId = [Guid]::NewGuid()
            
            { Set-DataverseForm -Connection $connection `
                -Id $formId `
                -Name 'WhatIf Update' `
                -WhatIf } | Should -Not -Throw
        }
    }
}

Describe 'Remove-DataverseForm' {
    BeforeAll {
        $connection = getMockConnection
    }

    Context 'Delete by ID' {
        It 'Should delete form by ID' {
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
            
            $connection.GetOrganizationService().Create($form) | Out-Null
            
            Remove-DataverseForm -Connection $connection -Id $formId -Confirm:$false
            
            # Verify form was deleted
            { Get-DataverseForm -Connection $connection -Id $formId -ErrorAction Stop } | Should -Throw
        }
    }

    Context 'Delete by name' {
        It 'Should delete form by entity and name' {
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
            
            $connection.GetOrganizationService().Create($form) | Out-Null
            
            Remove-DataverseForm -Connection $connection -Entity 'contact' -Name 'Named Form To Delete' -Confirm:$false
            
            # Verify form was deleted
            $result = Get-DataverseForm -Connection $connection -Entity 'contact' -Name 'Named Form To Delete'
            $result | Should -BeNullOrEmpty
        }

        It 'Should throw error if form not found' {
            { Remove-DataverseForm -Connection $connection -Entity 'contact' -Name 'NonExistent Form' -Confirm:$false -ErrorAction Stop } | Should -Throw
        }

        It 'Should not throw error with IfExists flag' {
            { Remove-DataverseForm -Connection $connection -Entity 'contact' -Name 'NonExistent Form' -IfExists -Confirm:$false } | Should -Not -Throw
        }
    }

    Context 'WhatIf and Confirm support' {
        It 'Should support WhatIf' {
            $formId = [Guid]::NewGuid()
            
            { Remove-DataverseForm -Connection $connection -Id $formId -WhatIf } | Should -Not -Throw
        }
    }
}
