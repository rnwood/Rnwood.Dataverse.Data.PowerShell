. $PSScriptRoot/Common.ps1

Describe 'Form Control Management' {
    BeforeAll {
        $connection = getMockConnection -Entities @("systemform", "contact")
        
        # Create a test form with a simple structure
        # Note: Labels are in the cell, before the control (per schema)
        $formXml = @'
<form>
    <tabs>
        <tab name="general" id="{9748ec98-3746-40cc-83bf-d15c7363166f}">
            <columns>
                <column width="100%">
                    <sections>
                        <section name="section1" id="{section1-id}">
                            <rows>
                                <row>
                                    <cell id="{cell1-id}">
                                        <labels>
                                            <label description="First Name" languagecode="1033" />
                                        </labels>
                                        <control id="{control1-id}" datafieldname="firstname" classid="{4273EDBD-AC1D-40D3-9FB2-095C621B552D}" />
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

        $form = @{
            formxml = $formXml
            objecttypecode = "contact"
        } | Set-DataverseRecord -Connection $connection -TableName systemform -CreateOnly -PassThru
        
        $script:FormId = $form.Id
    }

    Context 'Set-DataverseFormControl - Update existing control' {
        It "Updates an existing control by ControlId" {
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -ControlId "{control1-id}" -DataField "firstname" -Labels @{1033 = "Updated First Name"}
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -ControlId "{control1-id}"
            $control.Labels[0].Description | Should -Be "Updated First Name"
        }

        It "Updates an existing control by DataField" {
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "firstname" -Labels @{1033 = "Updated by DataField"}
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "firstname"
            $control.Labels[0].Description | Should -Be "Updated by DataField"
        }
    }

    Context 'Set-DataverseFormControl - Create new control' {
        It "Creates a new control with generated ID when ControlId not provided" {
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "lastname" -Labels @{1033 = "Last Name"} -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "lastname"
            $control | Should -Not -BeNull
            $control.Labels[0].Description | Should -Be "Last Name"
            $control.Id | Should -Be $result
        }

        It "Creates a new control with specified ControlId" {
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -ControlId "{control2-id}" -DataField "emailaddress1" -Labels @{1033 = "Email"}
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -ControlId "{control2-id}"
            $control | Should -Not -BeNull
            $control.DataField | Should -Be "emailaddress1"
        }

        It "Creates a new control with multiple language labels" {
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -ControlId "{control-multilang}" -DataField "fax" -Labels @{1033 = "Fax Number"; 1031 = "Faxnummer"}
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -ControlId "{control-multilang}"
            $control | Should -Not -BeNull
            $control.Labels.Count | Should -BeGreaterOrEqual 2
            ($control.Labels | Where-Object { $_.LanguageCode -eq "1033" }).Description | Should -Be "Fax Number"
            ($control.Labels | Where-Object { $_.LanguageCode -eq "1031" }).Description | Should -Be "Faxnummer"
        }
    }

    Context 'Set-DataverseFormControl - RawXml parameter set' {
        It "Creates a new control using RawXml with generated ID" {
            # Note: RawXml may contain labels inside the control (legacy support)
            # but Get-DataverseFormControl reads labels from the cell
            $rawXml = @'
<control datafieldname="mobilephone" classid="{4273EDBD-AC1D-40D3-9FB2-095C621B552D}" />
'@
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -ControlXml $rawXml -Labels @{1033 = "Mobile Phone"} -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "mobilephone"
            $control | Should -Not -BeNull
            $control.Labels[0].Description | Should -Be "Mobile Phone"
        }

        It "Updates existing control using RawXml by DataField" {
            $rawXml = @'
<control datafieldname="firstname" classid="{4273EDBD-AC1D-40D3-9FB2-095C621B552D}" />
'@
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -ControlXml $rawXml -Labels @{1033 = "Updated via RawXml"}
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "firstname"
            $control.Labels[0].Description | Should -Be "Updated via RawXml"
        }
    }

    Context 'Remove-DataverseFormControl' {
        It "Removes control by ControlId" {
            # First create a control to remove
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -ControlId "{control-to-remove}" -DataField "jobtitle" -Labels @{1033 = "Job Title"}
            
            # Verify it exists
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -ControlId "{control-to-remove}"
            $control | Should -Not -BeNull
            
            # Remove it
            Remove-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -ControlId "{control-to-remove}" -Confirm:$false
            
            # Verify it's gone
            { Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -ControlId "{control-to-remove}" } | Should -Throw
        }

        It "Removes control by DataField" {
            # First create a control to remove
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "department" -Labels @{1033 = "Department"}
            
            # Verify it exists
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "department"
            $control | Should -Not -BeNull
            
            # Remove it
            Remove-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -DataField "department" -SectionName "section1" -Confirm:$false
            
            # Verify it's gone
            { Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "department" } | Should -Throw
        }
    }

    Context 'Error Handling' {
        It "Throws error when trying to remove non-existent control" {
            { Remove-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -ControlId "{non-existent-id}" -Confirm:$false } | Should -Throw
        }
    }
}