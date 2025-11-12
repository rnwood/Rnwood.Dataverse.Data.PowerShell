# Tests for Set-DataverseFormControl automatic control type determination and metadata validation

. "$PSScriptRoot/Common.ps1"

Describe 'Set-DataverseFormControl - Automatic Control Type Determination' {
    BeforeAll {
        $connection = getMockConnection -Entities @("contact", "systemform")
        $script:FormId = [guid]::NewGuid()
        
        # Create test form for contact
        $contactForm = @{
            formid = $script:FormId
            objecttypecode = 2  # contact entity type code
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
        
        $contactForm | Set-DataverseRecord -Connection $connection -TableName systemform -CreateOnly | Out-Null
    }

    Context 'Automatic Control Type Determination - Basic Fields' {
        It 'Should auto-determine Standard control for regular string field' {
            # firstname is a regular string field in contact entity
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "firstname" -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            
            # Verify the control was created with Standard type (verify via form XML)
            $form = Get-DataverseRecord -Connection $connection -TableName systemform -Id $script:FormId
            $form.formxml | Should -Match 'datafieldname="firstname"'
            $form.formxml | Should -Match '4273EDBD-AC1D-40D3-9FB2-095C621B552D'  # Standard control class ID
        }

        It 'Should auto-determine Email control for email format string field' {
            # emailaddress1 is an email format string field in contact entity
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "emailaddress1" -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            
            # Verify the control was created with Email type
            $form = Get-DataverseRecord -Connection $connection -TableName systemform -Id $script:FormId
            $form.formxml | Should -Match 'datafieldname="emailaddress1"'
            $form.formxml | Should -Match 'ADA2203E-B4CD-49BE-9DDF-234642B43B52'  # Email control class ID
        }

        It 'Should auto-determine Boolean control for boolean field' {
            # donotemail is a boolean field in contact entity
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "donotemail" -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            
            # Verify the control was created with Boolean type
            $form = Get-DataverseRecord -Connection $connection -TableName systemform -Id $script:FormId
            $form.formxml | Should -Match 'datafieldname="donotemail"'
            $form.formxml | Should -Match '67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED'  # Boolean control class ID
        }
    }

    Context 'Control Type Override' {
        It 'Should respect explicitly specified ControlType' {
            # Override auto-determination with explicit type
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "lastname" -ControlType "Standard" -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            
            # Verify the control was created with explicitly specified type
            $form = Get-DataverseRecord -Connection $connection -TableName systemform -Id $script:FormId
            $form.formxml | Should -Match 'datafieldname="lastname"'
            $form.formxml | Should -Match '4273EDBD-AC1D-40D3-9FB2-095C621B552D'  # Standard control class ID
        }
    }

    Context 'Field Existence Validation' {
        It 'Should throw error when attribute does not exist' {
            # Try to create control for non-existent field
            { Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "nonexistentfield" -PassThru } | Should -Throw "*not found*"
        }

        It 'Should handle relationship navigation fields gracefully' {
            # Try relationship navigation field (should default to Standard)
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId -TabName "general" -SectionName "section1" -DataField "parentcustomerid.name" -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            
            # Verify Standard control type was used
            $form = Get-DataverseRecord -Connection $connection -TableName systemform -Id $script:FormId
            $form.formxml | Should -Match 'datafieldname="parentcustomerid.name"'
            $form.formxml | Should -Match '4273EDBD-AC1D-40D3-9FB2-095C621B552D'  # Standard control class ID
        }
    }
}