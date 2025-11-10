# Tests for form header control functionality

. "$PSScriptRoot/Common.ps1"

Describe 'Form Control Management - Header Section' {
    BeforeAll {
        $connection = getMockConnection -Entities contact,systemform
        $script:FormId = [guid]::NewGuid()
        
        # Create a test form without header initially
        $formRecord = @{
            formid = $script:FormId
            objecttypecode = 'contact'
            formxml = @'
<form showImage="true">
    <tabs>
        <tab name="general" id="{test-tab-1}">
            <labels>
                <label description="General" languagecode="1033" />
            </labels>
            <columns>
                <column width="100%">
                    <sections>
                        <section name="section1" id="{test-section-1}" showlabel="false" showbar="false">
                            <labels>
                                <label description="Section 1" languagecode="1033" />
                            </labels>
                            <rows>
                                <row>
                                    <cell id="{cell-1}">
                                        <labels>
                                            <label description="First Name" languagecode="1033" />
                                        </labels>
                                        <control id="firstname" classid="{4273EDBD-AC1D-40d3-9FB2-095C621B552D}" datafieldname="firstname" />
                                    </cell>
                                </row>
                            </rows>
                        </section>
                    </sections>
                </column>
            </columns>
        </tab>
    </tabs>
</form>
'@
        }
        $script:InitialFormId = $formRecord | Set-DataverseRecord -Connection $connection -TableName systemform -CreateOnly -PassThru | Select-Object -ExpandProperty Id
    }

    Context 'Get-DataverseFormControl - Header Section' {
        BeforeAll {
            # Create form with header
            $formWithHeader = @{
                formid = [guid]::NewGuid()
                objecttypecode = 'contact'
                formxml = @'
<form showImage="true">
    <header id="{header-id-1}" celllabelposition="Top">
        <rows>
            <row>
                <cell id="{header-cell-1}">
                    <labels>
                        <label description="Email" languagecode="1033" />
                    </labels>
                    <control id="header_emailaddress1" classid="{ADA2203E-B4CD-49be-9DDF-234642B43B52}" datafieldname="emailaddress1" disabled="true" />
                </cell>
                <cell id="{header-cell-2}">
                    <labels>
                        <label description="Owner" languagecode="1033" />
                    </labels>
                    <control id="header_ownerid" classid="{270BD3DB-D9AF-4782-9025-509E298DEC0A}" datafieldname="ownerid" disabled="true" />
                </cell>
            </row>
        </rows>
    </header>
    <tabs>
        <tab name="general" id="{test-tab-1}">
            <labels>
                <label description="General" languagecode="1033" />
            </labels>
            <columns>
                <column width="100%">
                    <sections>
                        <section name="section1" id="{test-section-1}">
                            <labels>
                                <label description="Section 1" languagecode="1033" />
                            </labels>
                            <rows />
                        </section>
                    </sections>
                </column>
            </columns>
        </tab>
    </tabs>
</form>
'@
            }
            $script:FormWithHeaderId = $formWithHeader | Set-DataverseRecord -Connection $connection -TableName systemform -CreateOnly -PassThru | Select-Object -ExpandProperty formid
        }

        It "Returns header controls when TabName is '[Header]'" {
            $controls = Get-DataverseFormControl -Connection $connection -FormId $script:FormWithHeaderId -TabName '[Header]'
            
            $controls | Should -Not -BeNullOrEmpty
            $controls.Count | Should -BeGreaterThan 0
            $controls[0].TabName | Should -Be '[Header]'
            $controls[0].SectionName | Should -Be '[Header]'
        }

        It "Returns specific header control by DataField" {
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormWithHeaderId -TabName '[Header]' -DataField 'emailaddress1'
            
            $control | Should -Not -BeNull
            $control.DataField | Should -Be 'emailaddress1'
            $control.Id | Should -Be 'header_emailaddress1'
            $control.TabName | Should -Be '[Header]'
        }

        It "Returns specific header control by ControlId" {
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormWithHeaderId -TabName '[Header]' -ControlId 'header_ownerid'
            
            $control | Should -Not -BeNull
            $control.Id | Should -Be 'header_ownerid'
            $control.DataField | Should -Be 'ownerid'
        }

        It "Returns all controls including header when no filter specified" {
            $controls = Get-DataverseFormControl -Connection $connection -FormId $script:FormWithHeaderId
            
            $headerControls = $controls | Where-Object { $_.TabName -eq '[Header]' }
            $headerControls | Should -Not -BeNullOrEmpty
        }

        It "Returns header control with cell attributes" {
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormWithHeaderId -TabName '[Header]' -DataField 'emailaddress1'
            
            $control.CellId | Should -Not -BeNullOrEmpty
            $control.PSObject.Properties.Name | Should -Contain "CellId"
            $control.PSObject.Properties.Name | Should -Contain "ColSpan"
        }
    }

    Context 'Set-DataverseFormControl - Create Header Controls' {
        It "Creates header section when it doesn't exist" {
            # Use the form without header
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId `
                -TabName '[Header]' -DataField 'emailaddress1' `
                -ControlType 'Email' -Label 'Email Address' -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            
            # Verify header was created
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName '[Header]' -DataField 'emailaddress1'
            $control | Should -Not -BeNull
            $control.TabName | Should -Be '[Header]'
            $control.SectionName | Should -Be '[Header]'
        }

        It "Creates control in existing header" {
            # Add another control to the header
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId `
                -TabName '[Header]' -DataField 'ownerid' `
                -ControlType 'Lookup' -Label 'Owner' -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            
            # Verify both controls exist
            $controls = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName '[Header]'
            $controls.Count | Should -BeGreaterOrEqual 2
        }

        It "Creates header control with cell attributes" {
            $formId = [guid]::NewGuid()
            $newForm = @{
                formid = $formId
                objecttypecode = 'contact'
                formxml = '<form showImage="true"><tabs><tab name="general" id="{tab-1}"><labels><label description="General" languagecode="1033" /></labels><columns><column width="100%"><sections><section name="s1" id="{s-1}"><labels><label description="S1" languagecode="1033" /></labels><rows /></section></sections></column></columns></tab></tabs></form>'
            }
            $newForm | Set-DataverseRecord -Connection $connection -TableName systemform -CreateOnly | Out-Null
            
            $result = Set-DataverseFormControl -Connection $connection -FormId $formId `
                -TabName '[Header]' -DataField 'telephone1' `
                -Label 'Phone' -ColSpan 1 -CellId '{custom-header-cell}' -PassThru
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $formId -TabName '[Header]' -DataField 'telephone1'
            $control.ColSpan | Should -Be 1
            $control.CellId | Should -Be '{custom-header-cell}'
        }
    }

    Context 'Set-DataverseFormControl - Update Header Controls' {
        BeforeEach {
            # Create a form with header control
            $script:UpdateFormId = [guid]::NewGuid()
            $updateForm = @{
                formid = $script:UpdateFormId
                objecttypecode = 'contact'
                formxml = @'
<form showImage="true">
    <header id="{header-1}" celllabelposition="Top">
        <rows>
            <row>
                <cell id="{cell-1}">
                    <labels>
                        <label description="Email" languagecode="1033" />
                    </labels>
                    <control id="header_email" classid="{ADA2203E-B4CD-49be-9DDF-234642B43B52}" datafieldname="emailaddress1" />
                </cell>
            </row>
        </rows>
    </header>
    <tabs>
        <tab name="general" id="{tab-1}">
            <labels>
                <label description="General" languagecode="1033" />
            </labels>
            <columns>
                <column width="100%">
                    <sections>
                        <section name="s1" id="{s-1}">
                            <labels>
                                <label description="S1" languagecode="1033" />
                            </labels>
                            <rows />
                        </section>
                    </sections>
                </column>
            </columns>
        </tab>
    </tabs>
</form>
'@
            }
            $updateForm | Set-DataverseRecord -Connection $connection -TableName systemform -CreateOnly | Out-Null
        }

        It "Updates existing header control by DataField" {
            Set-DataverseFormControl -Connection $connection -FormId $script:UpdateFormId `
                -TabName '[Header]' -DataField 'emailaddress1' `
                -Label 'Primary Email Address' -Disabled
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:UpdateFormId -TabName '[Header]' -DataField 'emailaddress1'
            $control.Labels[0].Description | Should -Be 'Primary Email Address'
            $control.Disabled | Should -Be $true
        }

        It "Updates existing header control by ControlId" {
            Set-DataverseFormControl -Connection $connection -FormId $script:UpdateFormId `
                -TabName '[Header]' -ControlId 'header_email' -DataField 'emailaddress1' `
                -Label 'Email' -Hidden
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:UpdateFormId -TabName '[Header]' -ControlId 'header_email'
            $control.Hidden | Should -Be $true
        }
    }

    Context 'Remove-DataverseFormControl - Header Controls' {
        BeforeEach {
            # Create form with header controls
            $script:RemoveFormId = [guid]::NewGuid()
            $removeForm = @{
                formid = $script:RemoveFormId
                objecttypecode = 'contact'
                formxml = @'
<form showImage="true">
    <header id="{header-1}" celllabelposition="Top">
        <rows>
            <row>
                <cell id="{cell-1}">
                    <control id="header_email" classid="{ADA2203E-B4CD-49be-9DDF-234642B43B52}" datafieldname="emailaddress1" />
                </cell>
                <cell id="{cell-2}">
                    <control id="header_owner" classid="{270BD3DB-D9AF-4782-9025-509E298DEC0A}" datafieldname="ownerid" />
                </cell>
            </row>
        </rows>
    </header>
    <tabs>
        <tab name="general" id="{tab-1}">
            <labels>
                <label description="General" languagecode="1033" />
            </labels>
            <columns>
                <column width="100%">
                    <sections>
                        <section name="s1" id="{s-1}">
                            <labels>
                                <label description="S1" languagecode="1033" />
                            </labels>
                            <rows />
                        </section>
                    </sections>
                </column>
            </columns>
        </tab>
    </tabs>
</form>
'@
            }
            $removeForm | Set-DataverseRecord -Connection $connection -TableName systemform -CreateOnly | Out-Null
        }

        It "Removes header control by DataField" {
            Remove-DataverseFormControl -Connection $connection -FormId $script:RemoveFormId `
                -TabName '[Header]' -DataField 'emailaddress1' -Confirm:$false
            
            # Verify removal
            $controls = Get-DataverseFormControl -Connection $connection -FormId $script:RemoveFormId -TabName '[Header]'
            $controls | Where-Object { $_.DataField -eq 'emailaddress1' } | Should -BeNullOrEmpty
        }

        It "Removes header control by ControlId" {
            Remove-DataverseFormControl -Connection $connection -FormId $script:RemoveFormId `
                -ControlId 'header_owner' -Confirm:$false
            
            # Verify removal
            $controls = Get-DataverseFormControl -Connection $connection -FormId $script:RemoveFormId -TabName '[Header]'
            $controls | Where-Object { $_.Id -eq 'header_owner' } | Should -BeNullOrEmpty
        }
    }

    Context 'Integration - Complete Header Workflow' {
        It "Creates, updates, queries, and removes header controls" {
            $formId = [guid]::NewGuid()
            $form = @{
                formid = $formId
                objecttypecode = 'contact'
                formxml = '<form showImage="true"><tabs><tab name="general" id="{tab-1}"><labels><label description="General" languagecode="1033" /></labels><columns><column width="100%"><sections><section name="s1" id="{s-1}"><labels><label description="S1" languagecode="1033" /></labels><rows /></section></sections></column></columns></tab></tabs></form>'
            }
            $form | Set-DataverseRecord -Connection $connection -TableName systemform -CreateOnly | Out-Null
            
            # Create header control
            $controlId = Set-DataverseFormControl -Connection $connection -FormId $formId `
                -TabName '[Header]' -DataField 'telephone1' `
                -ControlType 'Standard' -Label 'Phone' -ColSpan 1 -PassThru
            
            $controlId | Should -Not -BeNullOrEmpty
            
            # Query header control
            $control = Get-DataverseFormControl -Connection $connection -FormId $formId -TabName '[Header]' -DataField 'telephone1'
            $control | Should -Not -BeNull
            $control.Labels[0].Description | Should -Be 'Phone'
            $control.TabName | Should -Be '[Header]'
            
            # Update header control
            Set-DataverseFormControl -Connection $connection -FormId $formId `
                -TabName '[Header]' -DataField 'telephone1' `
                -Label 'Business Phone' -ColSpan 2
            
            $updatedControl = Get-DataverseFormControl -Connection $connection -FormId $formId -TabName '[Header]' -DataField 'telephone1'
            $updatedControl.Labels[0].Description | Should -Be 'Business Phone'
            $updatedControl.ColSpan | Should -Be 2
            
            # Remove header control
            Remove-DataverseFormControl -Connection $connection -FormId $formId `
                -TabName '[Header]' -DataField 'telephone1' -Confirm:$false
            
            # Verify removal
            $controls = Get-DataverseFormControl -Connection $connection -FormId $formId -TabName '[Header]'
            $controls | Where-Object { $_.DataField -eq 'telephone1' } | Should -BeNullOrEmpty
        }
    }
}
