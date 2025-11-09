. $PSScriptRoot/Common.ps1

Describe 'FormTab Layout Management' {
    BeforeAll {
        $connection = getMockConnection -Entities @("systemform")
        
        # Create a test form with a simple tab structure
        $formXml = @'
<form>
    <tabs>
        <tab name="general" id="{9748ec98-3746-40cc-83bf-d15c7363166f}">
            <labels>
                <label description="General" languagecode="1033" />
            </labels>
            <columns>
                <column width="100%">
                    <sections>
                        <section name="section1">
                            <labels>
                                <label description="Section 1" languagecode="1033" />
                            </labels>
                        </section>
                        <section name="section2">
                            <labels>
                                <label description="Section 2" languagecode="1033" />
                            </labels>
                        </section>
                        <section name="section3">
                            <labels>
                                <label description="Section 3" languagecode="1033" />
                            </labels>
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

    Context 'Get-DataverseFormTab Layout Information' {
        It "Returns layout information for existing tab" {
            $tab = Get-DataverseFormTab -Connection $connection -FormId $script:FormId -TabName "general"
            
            $tab | Should -Not -BeNullOrEmpty
            $tab.Layout | Should -Be "OneColumn"
            $tab.Column1Width | Should -Be "100%"
            $tab | Should -Not -HaveProperty "Column2Width"
            $tab | Should -Not -HaveProperty "Column3Width"
        }
    }

    Context 'Set-DataverseFormTab Layout Operations' {
        It "Updates tab to two-column layout" {
            Set-DataverseFormTab -Connection $connection -FormId $script:FormId -TabId "{9748ec98-3746-40cc-83bf-d15c7363166f}" `
                -Layout TwoColumns -Column1Width 60 -Column2Width 40
            
            $tab = Get-DataverseFormTab -Connection $connection -FormId $script:FormId -TabName "general"
            $tab.Layout | Should -Be "TwoColumns"
            $tab.Column1Width | Should -Be 60
            $tab.Column2Width | Should -Be 40
        }

        It "Updates tab to three-column layout" {
            Set-DataverseFormTab -Connection $connection -FormId $script:FormId -TabId "{9748ec98-3746-40cc-83bf-d15c7363166f}" `
                -Layout ThreeColumns -Column1Width 30 -Column2Width 35 -Column3Width 35
            
            $tab = Get-DataverseFormTab -Connection $connection -FormId $script:FormId -TabName "general"
            $tab.Layout | Should -Be "ThreeColumns"
            $tab.Column1Width | Should -Be 30
            $tab.Column2Width | Should -Be 35
            $tab.Column3Width | Should -Be 35
        }

        It "Updates tab back to one-column layout" {
            Set-DataverseFormTab -Connection $connection -FormId $script:FormId -TabId "{9748ec98-3746-40cc-83bf-d15c7363166f}" `
                -Layout OneColumn -Column1Width 100
            
            $tab = Get-DataverseFormTab -Connection $connection -FormId $script:FormId -TabName "general"
            $tab.Layout | Should -Be "OneColumn"
            $tab.Column1Width | Should -Be 100
        }

        It "Uses default widths when not specified" {
            Set-DataverseFormTab -Connection $connection -FormId $script:FormId -TabId "{9748ec98-3746-40cc-83bf-d15c7363166f}" -Layout TwoColumns
            
            $tab = Get-DataverseFormTab -Connection $connection -FormId $script:FormId -TabName "general"
            $tab.Layout | Should -Be "TwoColumns"
            $tab.Column1Width | Should -Be 50
            $tab.Column2Width | Should -Be 50
        }

        It "Infers layout from column width parameters" {
            # If Column2Width is provided, should infer TwoColumns layout
            Set-DataverseFormTab -Connection $connection -FormId $script:FormId -TabId "{9748ec98-3746-40cc-83bf-d15c7363166f}" `
                -Column1Width 70 -Column2Width 30
            
            $tab = Get-DataverseFormTab -Connection $connection -FormId $script:FormId -TabName "general"
            $tab.Layout | Should -Be "TwoColumns"
            $tab.Column1Width | Should -Be 70
            $tab.Column2Width | Should -Be 30
        }

        It "Infers three-column layout from Column3Width parameter" {
            Set-DataverseFormTab -Connection $connection -FormId $script:FormId -TabId "{9748ec98-3746-40cc-83bf-d15c7363166f}" `
                -Column1Width 25 -Column2Width 25 -Column3Width 50
            
            $tab = Get-DataverseFormTab -Connection $connection -FormId $script:FormId -TabName "general"
            $tab.Layout | Should -Be "ThreeColumns"
            $tab.Column1Width | Should -Be 25
            $tab.Column2Width | Should -Be 25
            $tab.Column3Width | Should -Be 50
        }
    }

    Context 'Section Redistribution' {
        It "Preserves sections when changing layouts" {
            # Verify original sections exist
            $originalSections = Get-DataverseFormSection -Connection $connection -FormId $script:FormId -TabName "general"
            $originalSections | Should -HaveCount 3
            
            # Change to two-column layout
            Set-DataverseFormTab -Connection $connection -FormId $script:FormId -TabId "{9748ec98-3746-40cc-83bf-d15c7363166f}" -Layout TwoColumns
            
            # Verify sections are still present
            $newSections = Get-DataverseFormSection -Connection $connection -FormId $script:FormId -TabName "general"
            $newSections | Should -HaveCount 3
            
            # Should have sections distributed across columns
            $sectionNames = $newSections | ForEach-Object { $_.Name }
            $sectionNames | Should -Contain "section1"
            $sectionNames | Should -Contain "section2"
            $sectionNames | Should -Contain "section3"
        }
    }

    Context 'New Tab Creation with Layout' {
        It "Creates new tab with specified layout" {
            $newTabId = Set-DataverseFormTab -Connection $connection -FormId $script:FormId -TabId "{abcd1234-1234-1234-1234-123456789012}" -Name "newlayout" `
                -Label "New Layout Tab" -Layout ThreeColumns `
                -Column1Width 40 -Column2Width 30 -Column3Width 30 -PassThru
            
            $newTabId | Should -Not -BeNullOrEmpty
            
            $tab = Get-DataverseFormTab -Connection $connection -FormId $script:FormId -TabName "newlayout"
            $tab.Layout | Should -Be "ThreeColumns"
            $tab.Column1Width | Should -Be 40
            $tab.Column2Width | Should -Be 30
            $tab.Column3Width | Should -Be 30
        }
    }

    Context 'Error Handling' {
        It "Validates column width format" {
            { Set-DataverseFormTab -Connection $connection -FormId $script:FormId -TabId "{9748ec98-3746-40cc-83bf-d15c7363166f}" `
                -Layout TwoColumns -Column1Width "invalid" -ErrorAction Stop } |
                Should -Throw
        }

        It "Validates layout parameter" {
            { Set-DataverseFormTab -Connection $connection -FormId $script:FormId -TabId "{9748ec98-3746-40cc-83bf-d15c7363166f}" `
                -Layout "InvalidLayout" -ErrorAction Stop } |
                Should -Throw
        }
    }
}

Describe 'Backward Compatibility' {
    BeforeAll {
        $connection = getMockConnection -Entities @("systemform")
        
        # Create a form with multi-column layout
        $formXml = @'
<form>
    <tabs>
        <tab name="multicolumn" id="{12345678-1234-1234-1234-123456789012}">
            <labels>
                <label description="Multi Column" languagecode="1033" />
            </labels>
            <columns>
                <column width="50%">
                    <sections>
                        <section name="left">
                        </section>
                    </sections>
                </column>
                <column width="50%">
                    <sections>
                        <section name="right">
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

    Context 'Existing Tab Parsing' {
        It "Correctly identifies existing two-column layout" {
            $tab = Get-DataverseFormTab -Connection $connection -FormId $script:FormId -TabName "multicolumn"
            
            $tab.Layout | Should -Be "TwoColumns"
            $tab.Column1Width | Should -Be 50
            $tab.Column2Width | Should -Be 50
        }

        It "Maintains backward compatibility with Sections property" {
            $tab = Get-DataverseFormTab -Connection $connection -FormId $script:FormId -TabName "multicolumn"
            
            # Sections property should still exist for backward compatibility
            $tab.Sections | Should -Not -BeNullOrEmpty
            $tab.Sections | Should -HaveCount 2
        }
    }
}