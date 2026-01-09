# Tests for Subgrid control creation with automatic parameter population

. "$PSScriptRoot/Common.ps1"

Describe 'Set-DataverseFormControl - Subgrid Default Parameters' {
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

    Context 'Subgrid Control without DataField' {
        It 'Should create subgrid with minimal parameters and add defaults' {
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId `
                -TabName "general" -SectionName "section1" `
                -ControlType "Subgrid" -ControlId "test_subgrid" `
                -Labels @{1033 = 'Related Records'} -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            $result | Should -Be "test_subgrid"
            
            # Verify the control was created
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId `
                -TabName "general" -SectionName "section1" -ControlId "test_subgrid"
            
            $control | Should -Not -BeNull
            $control.ClassId.ToUpper() | Should -Be "{E7A81278-8635-4D9E-8D4D-59480B391C5B}"
            
            # Verify DataField is null (not set for subgrids)
            $control.DataField | Should -BeNullOrEmpty
            
            # Verify default parameters are set
            $control.Parameters.RecordsPerPage | Should -Be "4"
            $control.Parameters.AutoExpand | Should -Be "Fixed"
            $control.Parameters.EnableQuickFind | Should -Be "false"
            $control.Parameters.EnableViewPicker | Should -Be "false"
            $control.Parameters.EnableChartPicker | Should -Be "false"
        }

        It 'Should not add datafieldname attribute to subgrid XML' {
            # Create a subgrid
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId `
                -TabName "general" -SectionName "section1" `
                -ControlType "Subgrid" -ControlId "test_subgrid_2" `
                -Labels @{1033 = 'Related Data'}
            
            # Get the raw XML
            $form = Get-DataverseRecord -Connection $connection -TableName systemform -Id $script:FormId -Columns formxml
            
            # Verify indicationOfSubgrid attribute is present
            $form.formxml | Should -Match 'id="test_subgrid_2"[^>]*indicationOfSubgrid="true"'
            
            # Verify NO datafieldname attribute for this control
            $form.formxml | Should -Not -Match '<control[^>]*id="test_subgrid_2"[^>]*datafieldname'
        }
    }

    Context 'Subgrid Control with Custom Parameters' {
        It 'Should merge custom parameters with defaults' {
            $customParams = @{
                'RecordsPerPage' = '10'
                'EnableQuickFind' = 'true'
                'RelationshipName' = 'contact_customer_accounts'
                'TargetEntityType' = 'account'
            }
            
            $result = Set-DataverseFormControl -Connection $connection -FormId $script:FormId `
                -TabName "general" -SectionName "section1" `
                -ControlType "Subgrid" -ControlId "custom_subgrid" `
                -Labels @{1033 = 'Accounts'} -Parameters $customParams -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            
            # Verify the control
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId `
                -TabName "general" -SectionName "section1" -ControlId "custom_subgrid"
            
            # Custom parameters should be preserved
            $control.Parameters.RecordsPerPage | Should -Be "10"
            $control.Parameters.EnableQuickFind | Should -Be "true"
            $control.Parameters.RelationshipName | Should -Be "contact_customer_accounts"
            $control.Parameters.TargetEntityType | Should -Be "account"
            
            # Default parameters should still be added
            $control.Parameters.AutoExpand | Should -Be "Fixed"
            $control.Parameters.EnableViewPicker | Should -Be "false"
            $control.Parameters.EnableChartPicker | Should -Be "false"
        }

        It 'Should preserve user-provided ViewId' {
            $customViewId = "{12345678-1234-1234-1234-123456789012}"
            $customParams = @{
                'ViewId' = $customViewId
                'TargetEntityType' = 'opportunity'
                'RelationshipName' = 'contact_opportunities'
            }
            
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId `
                -TabName "general" -SectionName "section1" `
                -ControlType "Subgrid" -ControlId "subgrid_with_view" `
                -Labels @{1033 = 'Opportunities'} -Parameters $customParams
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId `
                -TabName "general" -SectionName "section1" -ControlId "subgrid_with_view"
            
            # User's ViewId should be preserved
            $control.Parameters.ViewId | Should -Be $customViewId
            # ViewIds should match ViewId
            $control.Parameters.ViewIds | Should -Be $customViewId
            # Other parameters should have been set
            $control.Parameters.TargetEntityType | Should -Be 'opportunity'
            $control.Parameters.RelationshipName | Should -Be 'contact_opportunities'
        }
    }

    Context 'Subgrid with Relationship Detection' {
        It 'Should use DataField as RelationshipName when provided' {
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId `
                -TabName "general" -SectionName "section1" `
                -ControlType "Subgrid" -ControlId "relationship_subgrid" `
                -DataField "contact_customer_accounts" `
                -Labels @{1033 = 'Related Accounts'}
            
            $control = Get-DataverseFormControl -Connection $connection -FormId $script:FormId `
                -TabName "general" -SectionName "section1" -ControlId "relationship_subgrid"
            
            # RelationshipName should be set from DataField
            $control.Parameters.RelationshipName | Should -Be "contact_customer_accounts"
            
            # But datafieldname should NOT be set in the control XML
            $control.DataField | Should -BeNullOrEmpty
        }
    }

    Context 'Validation' {
        It 'Should create valid XML structure' {
            Set-DataverseFormControl -Connection $connection -FormId $script:FormId `
                -TabName "general" -SectionName "section1" `
                -ControlType "Subgrid" -ControlId "validation_test" `
                -Labels @{1033 = 'Test Grid'}
            
            $form = Get-DataverseRecord -Connection $connection -TableName systemform -Id $script:FormId -Columns formxml
            
            # Should be valid XML
            { [xml]$form.formxml } | Should -Not -Throw
            
            # Parse the XML and find the control
            $xml = [xml]$form.formxml
            $control = $xml.form.tabs.tab.columns.column.sections.section.rows.row.cell.control | 
                Where-Object { $_.id -eq "validation_test" }
            
            $control | Should -Not -BeNull
            $control.classid | Should -Be "{E7A81278-8635-4D9E-8D4D-59480B391C5B}"
            $control.indicationOfSubgrid | Should -Be "true"
            $control.datafieldname | Should -BeNullOrEmpty
            
            # Verify parameters exist
            $control.parameters | Should -Not -BeNull
            $control.parameters.RecordsPerPage | Should -Be "4"
            $control.parameters.AutoExpand | Should -Be "Fixed"
        }
    }
}
