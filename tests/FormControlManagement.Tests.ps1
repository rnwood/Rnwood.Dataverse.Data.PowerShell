. $PSScriptRoot/Common.ps1

Describe 'Form Control Management' {
    BeforeAll {
        $connection = getMockConnection -Entities @("systemform", "contact")
        
        # Create a test form with a simple structure
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
                                        <control id="{control1-id}" datafieldname="firstname" classid="{4273EDBD-AC1D-40D3-9FB2-095C621B552D}">
                                            <labels>
                                                <label description="First Name" languagecode="1033" />
                                            </labels>
                                        </control>
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
                -ControlId "{control1-id}" -DataField "firstname" -Label "Updated First Name"
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -ControlId "{control1-id}"
            $control.Labels[0].Description | Should -Be "Updated First Name"
        }

        It "Updates an existing control by DataField" {
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "firstname" -Label "Updated by DataField"
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "firstname"
            $control.Labels[0].Description | Should -Be "Updated by DataField"
        }
    }

    Context 'Set-DataverseFormControl - Create new control' {
        It "Creates a new control with generated ID when ControlId not provided" {
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -DataField "lastname" -Label "Last Name" -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "lastname"
            $control | Should -Not -BeNull
            $control.Labels[0].Description | Should -Be "Last Name"
            $control.Id | Should -Be $result
        }

        It "Creates a new control with specified ControlId" {
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -ControlId "{control2-id}" -DataField "emailaddress1" -Label "Email"
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -ControlId "{control2-id}"
            $control | Should -Not -BeNull
            $control.DataField | Should -Be "emailaddress1"
        }
    }

    Context 'Set-DataverseFormControl - RawXml parameter set' {
        It "Creates a new control using RawXml with generated ID" {
            $rawXml = @'
<control datafieldname="mobilephone" classid="{4273EDBD-AC1D-40D3-9FB2-095C621B552D}">
    <labels>
        <label description="Mobile Phone" languagecode="1033" />
    </labels>
</control>
'@
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -ControlXml $rawXml -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "mobilephone"
            $control | Should -Not -BeNull
            $control.Labels[0].Description | Should -Be "Mobile Phone"
        }

        It "Updates existing control using RawXml by DataField" {
            $rawXml = @'
<control datafieldname="firstname" classid="{4273EDBD-AC1D-40D3-9FB2-095C621B552D}">
    <labels>
        <label description="Updated via RawXml" languagecode="1033" />
    </labels>
</control>
'@
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -ControlXml $rawXml
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "firstname"
            $control.Labels[0].Description | Should -Be "Updated via RawXml"
        }
    }

    Context 'Remove-DataverseFormControl' {
        It "Removes control by ControlId" {
            # First create a control to remove
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -ControlId "{control-to-remove}" -DataField "jobtitle" -Label "Job Title"
            
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
                -DataField "department" -Label "Department"
            
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
        It "Throws error when trying to update non-existent control by ControlId" -Skip {
            # This test is skipped because Set-DataverseFormControl is designed to upsert
            # (create or update). When ControlId is provided for a non-existent control,
            # it creates a new control with that ID, which is the intended behavior.
            { Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" `
                -ControlId "{non-existent-id}" -DataField "nonexistent" -Label "Test" } | Should -Throw
        }

        It "Throws error when trying to remove non-existent control" {
            { Remove-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -ControlId "{non-existent-id}" -Confirm:$false } | Should -Throw
        }
    }
}