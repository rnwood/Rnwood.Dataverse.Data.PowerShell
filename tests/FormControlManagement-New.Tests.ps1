# Tests for new form control functionality (cell attributes, new control types, update capability)

. "$PSScriptRoot/Common.ps1"

Describe 'Form Control Management - New Features' {
    BeforeAll {
        $connection = getMockConnection -Entities @("contact", "systemform")
        $script:FormId = [guid]::NewGuid()
        
        # Create a test form
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
        $formRecord | Set-DataverseRecord -Connection $connection -TableName systemform -CreateOnly | Out-Null
    }

    Context 'Get-DataverseFormControl - Cell Attributes' {
        It "Returns cell attributes with controls" {
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "firstname"
            
            $control | Should -Not -BeNull
            $control.CellId | Should -Be "{cell-1}"
            $control.PSObject.Properties.Name | Should -Contain "CellId"
            $control.PSObject.Properties.Name | Should -Contain "ColSpan"
            $control.PSObject.Properties.Name | Should -Contain "RowSpan"
            $control.PSObject.Properties.Name | Should -Contain "Auto"
            $control.PSObject.Properties.Name | Should -Contain "LockLevel"
        }

        It "Returns cell labels when present" {
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "firstname"
            
            $control.CellLabels | Should -Not -BeNullOrEmpty
            $control.CellLabels[0].Description | Should -Be "First Name"
        }
    }

    Context 'Set-DataverseFormControl - New Control Types' {
        It "Creates Email control type" {
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "emailaddress1" -ControlType "Email" -Label "Primary Email" -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "emailaddress1"
            $control.ClassId.ToUpper() | Should -Be "{ADA2203E-B4CD-49BE-9DDF-234642B43B52}"
        }

        It "Creates Memo control type" {
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "description" -ControlType "Memo" -Label "Description" -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "description"
            $control.ClassId.ToUpper() | Should -Be "{E0DECE4B-6FC8-4A8F-A065-082708572369}"
        }

        It "Creates Money control type" {
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "creditlimit" -ControlType "Money" -Label "Credit Limit" -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "creditlimit"
            $control.ClassId.ToUpper() | Should -Be "{533B9E00-756B-4312-95A0-DC888637AC78}"
        }

        It "Creates Data control type (hidden data)" {
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "hidden_field" -ControlType "Data" -Label "Hidden Field" -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "hidden_field"
            $control.ClassId.ToUpper() | Should -Be "{5546E6CD-394C-4BEE-94A8-4425E17EF6C6}"
        }
    }

    Context 'Set-DataverseFormControl - Cell Attributes' {
        It "Creates control with cell attributes" {
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "lastname" -Label "Last Name" `
                -ColSpan 2 -RowSpan 1 -Auto -CellId "{custom-cell-id}" -LockLevel 0 -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "lastname"
            $control.ColSpan | Should -Be 2
            $control.RowSpan | Should -Be 1
            $control.Auto | Should -Be $true
            $control.CellId | Should -Be "{custom-cell-id}"
            $control.LockLevel | Should -Be "0"
        }

        It "Updates cell attributes on existing control" {
            # First create the control
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "middlename" -Label "Middle Name"
            
            # Then update cell attributes
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "middlename" -ColSpan 3 -RowSpan 2
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "middlename"
            $control.ColSpan | Should -Be 3
            $control.RowSpan | Should -Be 2
        }
    }

    Context 'Set-DataverseFormControl - Update Existing Controls' {
        BeforeEach {
            # Create a control to update
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "telephone1" -Label "Phone" -ControlType "Standard"
        }

        It "Updates control label" {
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "telephone1" -Label "Business Phone"
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "telephone1"
            $control.Labels[0].Description | Should -Be "Business Phone"
        }

        It "Updates control to required" {
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "telephone1" -IsRequired
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "telephone1"
            $control.IsRequired | Should -Be $true
        }

        It "Updates control visibility" {
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "telephone1" -Hidden
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "telephone1"
            $control.Hidden | Should -Be $true
        }

        It "Updates control disabled state" {
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "telephone1" -Disabled
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "telephone1"
            $control.Disabled | Should -Be $true
        }

        It "Updates control type" {
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "emailaddress1" -ControlType "Email"
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "emailaddress1"
            $control.ClassId.ToUpper() | Should -Be "{ADA2203E-B4CD-49BE-9DDF-234642B43B52}"
        }
    }

    Context 'Set-DataverseFormControl - Hidden Default Value Fix' {
        It "Creates visible control by default (Hidden not specified)" {
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "jobtitle" -Label "Job Title"
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "jobtitle"
            $control.Hidden | Should -Be $false
        }

        It "Creates hidden control when -Hidden specified" {
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "assistantname" -Label "Assistant" -Hidden
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "assistantname"
            $control.Hidden | Should -Be $true
        }
    }

    Context 'Integration - Complete Workflow' {
        It "Creates, updates, queries, and removes control" {
            # Create
            $controlId = Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "fax" -Label "Fax Number" -ControlType "Standard" -ColSpan 1 -PassThru
            
            $controlId | Should -Not -BeNullOrEmpty
            
            # Query
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "fax"
            $control | Should -Not -BeNull
            $control.Labels[0].Description | Should -Be "Fax Number"
            $control.ColSpan | Should -Be 1
            
            # Update
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "fax" -Label "Fax" -ColSpan 2 -IsRequired
            
            $updatedControl = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "fax"
            $updatedControl.Labels[0].Description | Should -Be "Fax"
            $updatedControl.ColSpan | Should -Be 2
            $updatedControl.IsRequired | Should -Be $true
            
            # Remove
            Remove-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "fax"
            
            # Verify removal
            { Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "fax" } | Should -Throw
        }
    }
}
