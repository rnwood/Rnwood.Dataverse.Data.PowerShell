. "$PSScriptRoot\Common.ps1"

Describe 'Dataverse Form Cmdlets' {
    BeforeAll {
        $connection = getMockConnection -Entities @('systemform')
        $global:testFormXml = Get-Content "$PSScriptRoot\test.formxml" -Raw
    }

    Context 'FormXmlHelper Parsing' {
        # These tests are for ParseFormStructure which doesn't exist yet
        # The actual parsing is done by individual Parse* methods like ParseTab, ParseSection, etc.
        
        It "Parses complete form structure from test.formxml" -Skip {
            # Test parsing with direct form element (no SystemForm wrapper)
            $doc = [System.Xml.Linq.XDocument]::Parse($testFormXml)
            
            { $parsed = [Rnwood.Dataverse.Data.PowerShell.Model.FormXmlHelper]::ParseFormStructure($doc) } | Should -Not -Throw
            
            $parsed | Should -Not -BeNullOrEmpty
            $parsed.Tabs | Should -Not -BeNullOrEmpty
            $parsed.Tabs.Count | Should -BeGreaterThan 0
        }

        It "Parses form root attributes correctly" -Skip {
            $doc = [System.Xml.Linq.XDocument]::Parse($testFormXml)
            $parsed = [Rnwood.Dataverse.Data.PowerShell.Model.FormXmlHelper]::ParseFormStructure($doc)
            
            $parsed.FormAttributes.showImage | Should -Be "true"
        }

        It "Parses hidden controls section" {
            $doc = [System.Xml.Linq.XDocument]::Parse($testFormXml)
            $hiddenControls = $doc.Root.Element("hiddencontrols")
            
            $hiddenControls | Should -Not -BeNullOrEmpty
            $hiddenControls.Elements("data") | Should -HaveCount 3
            
            $firstHidden = $hiddenControls.Elements("data") | Select-Object -First 1
            $firstHidden.Attribute("id").Value | Should -Be "fullname"
            $firstHidden.Attribute("datafieldname").Value | Should -Be "fullname"
            $firstHidden.Attribute("classid").Value | Should -Be "{5546E6CD-394C-4bee-94A8-4425E17EF6C6}"
        }

        It "Parses tabs with all attributes" -Skip {
            # Test parsing with direct form element
            $doc = [System.Xml.Linq.XDocument]::Parse($testFormXml)
            $parsed = [Rnwood.Dataverse.Data.PowerShell.Model.FormXmlHelper]::ParseFormStructure($doc)
            
            $parsed.Tabs | Should -HaveCount 4
            
            $generalTab = $parsed.Tabs | Where-Object { $_.Name -eq "general" } 
            $generalTab | Should -Not -BeNullOrEmpty
            $generalTab.Id | Should -Be "{9748ec98-3746-40cc-83bf-d15c7363166f}"
            $generalTab.Hidden | Should -Be $false
            $generalTab.Labels | Should -HaveCount 1
            $generalTab.Labels[0].Description | Should -Be "General"
            $generalTab.Labels[0].LanguageCode | Should -Be "1033"
        }

        It "Parses sections with all attributes" -Skip {
            $doc = [System.Xml.Linq.XDocument]::Parse($testFormXml)
            $parsed = [Rnwood.Dataverse.Data.PowerShell.Model.FormXmlHelper]::ParseFormStructure($doc)
            
            $generalTab = $parsed.Tabs | Where-Object { $_.Name -eq "general" }
            $generalTab.Sections | Should -HaveCount 4
            
            $nameSection = $generalTab.Sections | Where-Object { $_.Name -eq "name" }
            $nameSection | Should -Not -BeNullOrEmpty
            $nameSection.Id | Should -Be "{90b0776f-2977-4695-9cd4-73bb6c820f88}"
            $nameSection.ShowLabel | Should -Be $false
            $nameSection.Labels[0].Description | Should -Be "Name"
        }

        It "Parses controls with complex attributes and events" -Skip {
            $doc = [System.Xml.Linq.XDocument]::Parse($testFormXml)
            $parsed = [Rnwood.Dataverse.Data.PowerShell.Model.FormXmlHelper]::ParseFormStructure($doc)
            
            $generalTab = $parsed.Tabs | Where-Object { $_.Name -eq "general" }
            $nameSection = $generalTab.Sections | Where-Object { $_.Name -eq "name" }
            $nameSection.Controls.Count | Should -BeGreaterThan 5
            
            # Find the parentcustomerid control which has events
            $parentCustomerControl = $nameSection.Controls | Where-Object { $_.DataField -eq "parentcustomerid" }
            $parentCustomerControl | Should -Not -BeNullOrEmpty
            $parentCustomerControl.Id | Should -Be "parentcustomerid"
            $parentCustomerControl.ClassId | Should -Be "{270BD3DB-D9AF-4782-9025-509E298DEC0A}"
            $parentCustomerControl.Disabled | Should -Be $false
        }

        It "Parses header section correctly" {
            $doc = [System.Xml.Linq.XDocument]::Parse($testFormXml)
            $header = $doc.Root.Element("header")
            
            $header | Should -Not -BeNullOrEmpty
            $header.Attribute("id").Value | Should -Be "{59314f22-396d-45d8-baef-75b09c8dd512}"
            $header.Attribute("celllabelposition").Value | Should -Be "Top"
            
            $headerRows = $header.Element("rows")
            $headerRows | Should -Not -BeNullOrEmpty
            $headerRows.Elements("row") | Should -HaveCount 1
            
            $firstRow = $headerRows.Elements("row") | Select-Object -First 1
            $firstRow.Elements("cell") | Should -HaveCount 3
        }

        It "Parses client resources section" {
            $doc = [System.Xml.Linq.XDocument]::Parse($testFormXml)
            $clientResources = $doc.Root.Element("clientresources")
            
            $clientResources | Should -Not -BeNullOrEmpty
            $internalResources = $clientResources.Element("internalresources")
            $internalResources | Should -Not -BeNullOrEmpty
            
            $clientIncludes = $internalResources.Element("clientincludes")
            $clientIncludes | Should -Not -BeNullOrEmpty
            
            $jsFile = $clientIncludes.Element("internaljscriptfile")
            $jsFile | Should -Not -BeNullOrEmpty
            $jsFile.Attribute("src").Value | Should -Be "`$webresource:AppCommon/Contact/Contact_main_system_library.js"
        }

        It "Parses navigation section" {
            $doc = [System.Xml.Linq.XDocument]::Parse($testFormXml)
            $navigation = $doc.Root.Element("Navigation")
            
            $navigation | Should -Not -BeNullOrEmpty
            $navBar = $navigation.Element("NavBar")
            $navBar | Should -Not -BeNullOrEmpty
            
            $navItems = $navBar.Elements("NavBarByRelationshipItem")
            $navItems | Should -HaveCount 5
            
            $firstNavItem = $navItems | Select-Object -First 1
            $firstNavItem.Attribute("RelationshipName").Value | Should -Be "Contact_CustomerAddress"
            $firstNavItem.Attribute("Id").Value | Should -Be "navAddresses"
        }

        It "Handles cell attributes including colspan, rowspan, auto" {
            $doc = [System.Xml.Linq.XDocument]::Parse($testFormXml)
            $rows = $doc.Root.Descendants("row")
            
            # Find a cell with colspan=2 (from the description section)
            $cellsWithColspan = $doc.Root.Descendants("cell") | Where-Object { $_.Attribute("colspan") -ne $null }
            $cellsWithColspan | Should -Not -BeNullOrEmpty
            
            # Check the actual colspan value from description section
            $descriptionCell = $cellsWithColspan | Where-Object { $_.Attribute("colspan").Value -eq "2" } | Select-Object -First 1
            $descriptionCell | Should -Not -BeNullOrEmpty
            $descriptionCell.Attribute("colspan").Value | Should -Be "2"
            
            # Find a cell with auto attribute
            $cellWithAuto = $doc.Root.Descendants("cell") | Where-Object { $_.Attribute("auto") -ne $null } | Select-Object -First 1
            $cellWithAuto | Should -Not -BeNullOrEmpty
        }
    }

    Context 'Get-DataverseForm' {

        It "Retrieves forms by entity" {
            $connection = getMockConnection -Entities @("systemform")
            
            # Create a test form
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = [System.Guid]::NewGuid()
            $form["name"] = "Contact Information"
            $form["uniquename"] = "Contact_Information"
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $form["description"] = "Main form for contact"
            $form["formactivationstate"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(1)
            $form["formpresentation"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(0)
            $form["isdefault"] = $true
            $form["formxml"] = [string]$global:testFormXml
            $connection.Create($form)
            
            $result = Get-DataverseForm -Connection $connection -Entity 'contact' -published
            
            $result | Should -Not -BeNullOrEmpty
            $result.Entity | Should -Be 'contact'
            $result.Name | Should -Be 'Contact Information'
            $result.Type | Should -Be 'Main'
        }

        It "Includes FormXml when IncludeFormXml switch is used" {
            $connection = getMockConnection -Entities @("systemform")
            
            # Create a test form
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = [System.Guid]::NewGuid()
            $form["name"] = "Contact Information"
            $form["uniquename"] = "Contact_Information"
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $form["description"] = "Main form for contact"
            $form["formactivationstate"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(1)
            $form["formpresentation"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(0)
            $form["isdefault"] = $true
            $form["formxml"] = [string]$global:testFormXml
            $connection.Create($form)
            
            $result = Get-DataverseForm -Connection $connection -Entity 'contact' -IncludeFormXml -Published
            
            $result | Should -Not -BeNullOrEmpty
            $result.FormXml | Should -Not -BeNullOrEmpty
            $result.FormXml | Should -BeLike '*<form*'
        }



        It "Filters by FormType when specified" {
            $connection = getMockConnection -Entities @("systemform")
            
            # Create a main form
            $mainForm = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $mainForm["formid"] = $mainForm.Id = [System.Guid]::NewGuid()
            $mainForm["name"] = "Main Form"
            $mainForm["objecttypecode"] = "contact"
            $mainForm["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2) # Main form
            $mainForm["formxml"] = [string]$global:testFormXml
            $connection.Create($mainForm)
            
            # Create a quick create form (should be filtered out)
            $quickForm = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $quickForm["formid"] = $quickForm.Id = [System.Guid]::NewGuid()
            $quickForm["name"] = "Quick Create Form"
            $quickForm["objecttypecode"] = "contact"
            $quickForm["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(7) # Quick Create form
            $quickForm["formxml"] = [string]$global:testFormXml
            $connection.Create($quickForm)
            
            $result = Get-DataverseForm -Connection $connection -Entity 'contact' -FormType Main -Published
            
            $result | Should -Not -BeNullOrEmpty
            @($result).Count | Should -Be 1
            $result.Type | Should -Be 'Main'
        }

        It "Retrieves by form ID" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["name"] = "Test Form"
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $form["formxml"] = [string]$global:testFormXml
            $connection.Create($form)

            $result = Get-DataverseForm -Connection $connection -Id $formId -Published
            
            $result | Should -Not -BeNullOrEmpty
            $result.FormId | Should -Be $formId
        }
    }

    Context 'Get-DataverseFormTab' {
        It "Retrieves all tabs from a form" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            $result = Get-DataverseFormTab -Connection $connection -FormId $formId
            
            $result | Should -Not -BeNullOrEmpty
            $result.Count | Should -Be 4
            $result[0].Name | Should -Be 'general'
            $result[1].Name | Should -Be 'details'
        }

        It "Retrieves specific tab by name" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            $result = Get-DataverseFormTab -Connection $connection -FormId $formId -TabName 'general'
            
            $result | Should -Not -BeNullOrEmpty
            $result.Name | Should -Be 'general'
            $result.Id | Should -Be '{9748ec98-3746-40cc-83bf-d15c7363166f}'
        }

        It "Includes sections in tab output" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            $result = Get-DataverseFormTab -Connection $connection -FormId $formId -TabName 'general'
            
            $result | Should -Not -BeNullOrEmpty
            $result.Sections | Should -Not -BeNullOrEmpty
            $result.Sections.Count | Should -BeGreaterThan 0
        }
    }

    Context 'Get-DataverseFormSection' {
        It "Retrieves all sections from a form" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            $result = Get-DataverseFormSection -Connection $connection -FormId $formId
            
            $result | Should -Not -BeNullOrEmpty
            $result.Count | Should -BeGreaterThan 4
        }

        It "Retrieves sections by tab name" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            $result = Get-DataverseFormSection -Connection $connection -FormId $formId -TabName 'general'
            
            $result | Should -Not -BeNullOrEmpty
            $result.Count | Should -Be 4
            $result[0].Name | Should -Be 'name'
        }

        It "Retrieves specific section by name and tab" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            $result = Get-DataverseFormSection -Connection $connection -FormId $formId -TabName 'general' -SectionName 'name'
            
            $result | Should -Not -BeNullOrEmpty
            $result.Name | Should -Be 'name'
            $result.Id | Should -Be '{90b0776f-2977-4695-9cd4-73bb6c820f88}'
        }

        It "Includes controls in section output" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            $result = Get-DataverseFormSection -Connection $connection -FormId $formId -TabName 'general' -SectionName 'name'
            
            $result | Should -Not -BeNullOrEmpty
            $result.Controls | Should -Not -BeNullOrEmpty
            $result.Controls.Count | Should -BeGreaterThan 5
        }

        It "Parses cell label alignment and position properties" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            $result = Get-DataverseFormSection -Connection $connection -FormId $formId -TabName 'general' -SectionName 'name'
            
            $result | Should -Not -BeNullOrEmpty
            $result.PSObject.Properties.Name | Should -Contain 'CellLabelAlignment'
            $result.PSObject.Properties.Name | Should -Contain 'CellLabelPosition'
        }
    }

    Context 'Get-DataverseFormControl' {
        It "Retrieves all controls from a form" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            $result = Get-DataverseFormControl -Connection $connection -FormId $formId
            
            $result | Should -Not -BeNullOrEmpty
            $result.Count | Should -BeGreaterThan 20
        }

        It "Retrieves controls by section" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            $result = Get-DataverseFormControl -Connection $connection -FormId $formId -TabName 'general' -SectionName 'name'
            
            $result | Should -Not -BeNullOrEmpty
            $result.Count | Should -BeGreaterThan 5
            $result | Where-Object { $_.DataField -eq 'firstname' } | Should -Not -BeNullOrEmpty
        }

        It "Retrieves specific control by ID" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            $result = Get-DataverseFormControl -Connection $connection -FormId $formId -ControlId 'firstname'
            
            $result | Should -Not -BeNullOrEmpty
            $result.Id | Should -Be 'firstname'
            $result.DataField | Should -Be 'firstname'
        }

        It "Retrieves specific control by data field name" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            $result = Get-DataverseFormControl -Connection $connection -FormId $formId -DataField 'lastname'
            
            $result | Should -Not -BeNullOrEmpty
            $result.DataField | Should -Be 'lastname'
            $result.Id | Should -Be 'lastname'
        }
    }

    Context 'Set-DataverseFormControl' {
        It "Creates new control in section" {
            $connection = getMockConnection -Entities @("systemform", "contact")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Set-DataverseFormControl -Connection $connection -FormId $formId -TabName 'general' -SectionName 'name' -DataField 'mobilephone' -Labels @{1033 = 'New Field'} -WhatIf } | Should -Not -Throw
        }

        It "Updates existing control" {
            $connection = getMockConnection -Entities @("systemform", "contact")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Set-DataverseFormControl -Connection $connection -FormId $formId -TabName 'general' -SectionName 'name' -ControlId 'firstname' -DataField 'firstname' -Labels @{1033 = 'Updated First Name'} -WhatIf } | Should -Not -Throw
        }

        It "Creates control with raw XML" {
            $connection = getMockConnection -Entities @("systemform", "contact")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            $controlXml = '<control id="testfield" classid="{4273EDBD-AC1D-40d3-9FB2-095C621B552D}" datafieldname="testfield" />'
            { Set-DataverseFormControl -Connection $connection -FormId $formId -TabName 'general' -SectionName 'name' -ControlXml $controlXml -WhatIf } | Should -Not -Throw
        }

        It "Sets control properties correctly" {
            $connection = getMockConnection -Entities @("systemform", "contact")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Set-DataverseFormControl -Connection $connection -FormId $formId -TabName 'general' -SectionName 'name' -DataField 'jobtitle' -Labels @{1033 = 'Test Label'} -Disabled -Hidden:$false -IsRequired -WhatIf } | Should -Not -Throw
        }

        It "Handles positioning with Index" -Skip {
            # Skip: Index parameter doesn't exist - use Row and Column instead
            $connection = getMockConnection -Entities @("systemform", "contact")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Set-DataverseFormControl -Connection $connection -FormId $formId -TabName 'general' -SectionName 'name' -DataField 'telephone1' -Row 2 -WhatIf } | Should -Not -Throw
        }

        It "Handles positioning with InsertBefore" -Skip {
            # Skip: InsertBefore parameter doesn't exist - use Row and Column instead
            $connection = getMockConnection -Entities @("systemform", "contact")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Set-DataverseFormControl -Connection $connection -FormId $formId -TabName 'general' -SectionName 'name' -DataField 'telephone1' -Row 0 -WhatIf } | Should -Not -Throw
        }

        It "Handles positioning with InsertAfter" -Skip {
            # Skip: InsertAfter parameter doesn't exist - use Row and Column instead
            $connection = getMockConnection -Entities @("systemform", "contact")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Set-DataverseFormControl -Connection $connection -FormId $formId -TabName 'general' -SectionName 'name' -DataField 'telephone1' -Row 3 -WhatIf } | Should -Not -Throw
        }
    }

    Context 'Set-DataverseFormSection' {
        It "Creates new section in tab" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Set-DataverseFormSection -Connection $connection -FormId $formId -TabName 'general' -Name 'newsection' -Label 'New Section' -WhatIf } | Should -Not -Throw
        }

        It "Updates existing section" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Set-DataverseFormSection -Connection $connection -FormId $formId -TabName 'general' -Name 'name' -Label 'Updated Name Section' -WhatIf } | Should -Not -Throw
        }

        It "Sets section visibility and properties" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Set-DataverseFormSection -Connection $connection -FormId $formId -TabName 'general' -Name 'testsection' -Label 'Test Section' -Hidden:$true -ShowLabel:$false -WhatIf } | Should -Not -Throw
        }

        It "Sets cell label alignment and position" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Set-DataverseFormSection -Connection $connection -FormId $formId -TabName 'general' -Name 'testsection' -CellLabelAlignment 'Center' -CellLabelPosition 'Top' -WhatIf } | Should -Not -Throw
        }
    }

    Context 'Set-DataverseFormTab' {
        It "Creates new tab" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Set-DataverseFormTab -Connection $connection -FormId $formId -Name 'newtab' -Label 'New Tab' -WhatIf } | Should -Not -Throw
        }

        It "Updates existing tab" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Set-DataverseFormTab -Connection $connection -FormId $formId -Name 'general' -Label 'Updated General Tab' -WhatIf } | Should -Not -Throw
        }

        It "Sets tab visibility and expansion" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Set-DataverseFormTab -Connection $connection -FormId $formId -Name 'testtab' -Label 'Test Tab' -Hidden:$true -Expanded:$false -WhatIf } | Should -Not -Throw
        }
    }

    Context 'Remove-DataverseFormControl' {
        It "Removes control by ID" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Remove-DataverseFormControl -Connection $connection -FormId $formId -ControlId 'firstname' -Confirm:$false -WhatIf } | Should -Not -Throw
        }

        It "Removes control by data field" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Remove-DataverseFormControl -Connection $connection -FormId $formId -TabName 'general' -DataField 'lastname' -Confirm:$false -WhatIf } | Should -Not -Throw
        }

        It "Removes control from specific section" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Remove-DataverseFormControl -Connection $connection -FormId $formId -TabName 'general' -SectionName 'name' -DataField 'middlename' -Confirm:$false -WhatIf } | Should -Not -Throw
        }
    }

    Context 'Remove-DataverseFormSection' {
        It "Removes section by name and tab" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Remove-DataverseFormSection -Connection $connection -FormId $formId -TabName 'general' -SectionName 'description' -Confirm:$false -WhatIf } | Should -Not -Throw
        }

        It "Removes section by ID" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Remove-DataverseFormSection -Connection $connection -FormId $formId -SectionId '{e165b345-b7ce-4c73-a508-0f42cbf6dd53}' -Confirm:$false -WhatIf } | Should -Not -Throw
        }
    }

    Context 'Remove-DataverseFormTab' {
        It "Removes tab by name" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Remove-DataverseFormTab -Connection $connection -FormId $formId -TabName 'administration' -Confirm:$false -WhatIf } | Should -Not -Throw
        }

        It "Removes tab by ID" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Remove-DataverseFormTab -Connection $connection -FormId $formId -TabId '{0dd7233e-247b-4eef-a71b-0185da6d16ad}' -Confirm:$false -WhatIf } | Should -Not -Throw
        }
    }

    Context 'Set-DataverseForm' {
        It "Updates form with new XML" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["name"] = "Contact Information"
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Set-DataverseForm -Connection $connection -FormId $formId -FormXml $global:testFormXml -Confirm:$false -WhatIf } | Should -Not -Throw
        }

        It "Updates form name and description" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["name"] = "Contact Information"
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Set-DataverseForm -Connection $connection -FormId $formId -Name 'Updated Contact Form' -Description 'Updated description' -Confirm:$false -WhatIf } | Should -Not -Throw
        }
    }

    Context 'Remove-DataverseForm' {
        It "Removes form by ID" {
            $connection = getMockConnection -Entities @("systemform")
            
            $formId = [System.Guid]::NewGuid()
            $form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $form["formid"] = $form.Id = $formId
            $form["name"] = "Contact Information"
            $form["formxml"] = [string]$global:testFormXml
            $form["objecttypecode"] = "contact"
            $form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $connection.Create($form)
            
            { Remove-DataverseForm -Connection $connection -FormId $formId -Confirm:$false -WhatIf } | Should -Not -Throw
        }
    }

    Context 'FormXmlHelper Edge Cases and Error Handling' {
        It "Handles FindTab with null parameters" {
            $doc = [System.Xml.Linq.XDocument]::Parse($testFormXml)
            $systemForm = $doc.Root
            
            { [Rnwood.Dataverse.Data.PowerShell.Model.FormXmlHelper]::FindTab($systemForm, $null, $null) } | Should -Throw
        }

        It "Handles FindControl with null parameters" {
            $doc = [System.Xml.Linq.XDocument]::Parse($testFormXml)
            $systemForm = $doc.Root
            
            { [Rnwood.Dataverse.Data.PowerShell.Model.FormXmlHelper]::FindControl($systemForm, $null, $null) } | Should -Throw
        }

        It "Returns null for non-existent tab" {
            $doc = [System.Xml.Linq.XDocument]::Parse($testFormXml)
            $systemForm = $doc.Root
            
            $result = [Rnwood.Dataverse.Data.PowerShell.Model.FormXmlHelper]::FindTab($systemForm, "nonexistent")
            $result | Should -BeNullOrEmpty
        }

        It "Returns null for non-existent control" {
            $doc = [System.Xml.Linq.XDocument]::Parse($testFormXml)
            $systemForm = $doc.Root
            
            $result = [Rnwood.Dataverse.Data.PowerShell.Model.FormXmlHelper]::FindControl($systemForm, "nonexistent")
            $result.Control | Should -BeNullOrEmpty
            $result.ParentRow | Should -BeNullOrEmpty
        }
    }
}


