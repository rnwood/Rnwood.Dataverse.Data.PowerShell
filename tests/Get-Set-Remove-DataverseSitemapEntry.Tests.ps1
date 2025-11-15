. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseSitemapEntry' {
    Context 'Cmdlet Structure' {
        It "Get-DataverseSitemapEntry cmdlet exists and has expected parameters" {
            $command = Get-Command Get-DataverseSitemapEntry -ErrorAction SilentlyContinue
            $command | Should -Not -BeNullOrEmpty
            $command.Parameters.Keys | Should -Contain 'Sitemap'
            $command.Parameters.Keys | Should -Contain 'SitemapUniqueName'
            $command.Parameters.Keys | Should -Contain 'SitemapId'
            $command.Parameters.Keys | Should -Contain 'Area'
            $command.Parameters.Keys | Should -Contain 'Group'
            $command.Parameters.Keys | Should -Contain 'SubArea'
            $command.Parameters.Keys | Should -Contain 'EntryId'
            $command.Parameters.Keys | Should -Contain 'ParentAreaId'
            $command.Parameters.Keys | Should -Contain 'ParentGroupId'
            $command.Parameters.Keys | Should -Contain 'Connection'
        }
    }

    Context 'XML Parsing with ResourceId Properties' {
        BeforeAll {
            $connection = getMockConnection -Entities @('sitemap')
            
            # Sample sitemap XML with ResourceId properties including the typo
            $sitemapXml = @"
<SiteMap IntroducedVersion="7.0.0.0">
    <Area Id="HLP" ResourceId="Area_Help" ShowGroups="true" Icon="/_imgs/icn_help16.png" DescriptionResourceId="Help_Area_Description" IntroducedVersion="7.0.0.0">
        <Group Id="HLP_GRP" ResourceId="Group_Help" DescriptionResourceId="Help_Description" ToolTipResourseId="Help_ToolTip" IntroducedVersion="7.0.0.0">
            <SubArea Id="Help_Resource_Center" Url="https://go.microsoft.com/fwlink/p/?linkid=846388" ResourceId="Area_CustomerCenter" DescriptionResourceId="ResourceCenter_Area_Description" ToolTipResourseId="ResourceCenter_Area_ToolTip" AvailableOffline="false" IntroducedVersion="7.0.0.0" />
        </Group>
    </Area>
    <Area Id="Settings" ResourceId="Area_Settings" DescriptionResourceId="Settings_Area_Description" Icon="/_imgs/settings_24x24.gif" ShowGroups="true" IntroducedVersion="7.0.0.0">
        <Group Id="Business_Setting" ResourceId="Menu_Label_Business" DescriptionResourceId="Menu_Label_Business" ToolTipResourseId="Menu_Label_Business_ToolTip" IntroducedVersion="7.0.0.0">
            <SubArea Id="nav_businessmanagement" ResourceId="Homepage_BusinessManagement" DescriptionResourceId="BizManagement_SubArea_Description" ToolTipResourseId="BizManagement_SubArea_ToolTip" Icon="/_imgs/ico_18_busmanagement.gif" Url="/tools/business/business.aspx" AvailableOffline="false" IntroducedVersion="7.0.0.0" />
        </Group>
    </Area>
</SiteMap>
"@
            
            # Add mock sitemap to FakeXrmEasy
            $sitemapEntity = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $sitemapEntity["sitemapname"] = "TestSitemap"
            $sitemapEntity["sitemapnameunique"] = "TestSitemap"
            $sitemapEntity["sitemapxml"] = $sitemapXml
            $connection.Create($sitemapEntity) | Out-Null
        }

        It "Can retrieve all sitemap entries" {
            $entries = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSitemap"
            $entries | Should -Not -BeNullOrEmpty
            $entries.Count | Should -BeGreaterThan 0
            
            # Should have Areas, Groups, and SubAreas
            $areas = $entries | Where-Object { $_.EntryType -eq 'Area' }
            $groups = $entries | Where-Object { $_.EntryType -eq 'Group' }
            $subAreas = $entries | Where-Object { $_.EntryType -eq 'SubArea' }
            
            $areas.Count | Should -Be 2
            $groups.Count | Should -Be 2
            $subAreas.Count | Should -Be 2
        }

        It "Can filter entries by type" {
            $areas = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSitemap" -Area
            $areas | Should -Not -BeNullOrEmpty
            $areas.Count | Should -Be 2
            $areas | ForEach-Object { $_.EntryType | Should -Be 'Area' }
        }

        It "Can retrieve specific entry by ID" {
            $entry = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSitemap" -EntryId "HLP"
            $entry | Should -Not -BeNullOrEmpty
            $entry.Id | Should -Be "HLP"
            $entry.EntryType | Should -Be "Area"
        }

        It "Parses ResourceId properties correctly" {
            $helpArea = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSitemap" -EntryId "HLP"
            $helpArea.ResourceId | Should -Be "Area_Help"
            $helpArea.DescriptionResourceId | Should -Be "Help_Area_Description"
            $helpArea.Icon | Should -Be "/_imgs/icn_help16.png"
        }

        It "Parses DescriptionResourceId and ToolTipResourceId for Groups" {
            $helpGroup = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSitemap" -EntryId "HLP_GRP"
            $helpGroup.ResourceId | Should -Be "Group_Help"
            $helpGroup.DescriptionResourceId | Should -Be "Help_Description"
            $helpGroup.ToolTipResourceId | Should -Be "Help_ToolTip"
            $helpGroup.ParentAreaId | Should -Be "HLP"
        }

        It "Parses ToolTipResourceId with typo for SubAreas" {
            $helpSubArea = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSitemap" -EntryId "Help_Resource_Center"
            $helpSubArea.ResourceId | Should -Be "Area_CustomerCenter"
            $helpSubArea.DescriptionResourceId | Should -Be "ResourceCenter_Area_Description"
            $helpSubArea.ToolTipResourceId | Should -Be "ResourceCenter_Area_ToolTip"
            $helpSubArea.Url | Should -Be "https://go.microsoft.com/fwlink/p/?linkid=846388"
        }

        It "Can filter by parent Area ID" {
            $entriesInHelp = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSitemap" -Group -ParentAreaId "HLP"
            $entriesInHelp | Should -Not -BeNullOrEmpty
            $entriesInHelp | ForEach-Object { $_.ParentAreaId | Should -Be "HLP" }
        }

        It "Can filter by parent Group ID" {
            $subAreasInGroup = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSitemap" -SubArea -ParentAreaId "HLP" -ParentGroupId "HLP_GRP"
            $subAreasInGroup | Should -Not -BeNullOrEmpty
            $subAreasInGroup | ForEach-Object { 
                $_.EntryType | Should -Be "SubArea"
                $_.ParentGroupId | Should -Be "HLP_GRP"
            }
        }
    }

    Context 'Error Handling' {
        It "Throws error when sitemap not found" {
            $connection = getMockConnection -Entities @('sitemap')
            { Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "NonExistentSitemap" } | Should -Throw
        }

        It "Throws error when no sitemap identifier provided" {
            $connection = getMockConnection -Entities @('sitemap')
            { Get-DataverseSitemapEntry -Connection $connection } | Should -Throw
        }
    }
}

Describe 'Set-DataverseSitemapEntry' {
    Context 'Cmdlet Structure' {
        It "Get-DataverseSitemapEntry cmdlet exists and has expected parameters" {
            $command = Get-Command Set-DataverseSitemapEntry -ErrorAction SilentlyContinue
            $command | Should -Not -BeNullOrEmpty
            $command.Parameters.Keys | Should -Contain 'InputObject'
            $command.Parameters.Keys | Should -Contain 'Sitemap'
            $command.Parameters.Keys | Should -Contain 'SitemapUniqueName'
            $command.Parameters.Keys | Should -Contain 'SitemapId'
            $command.Parameters.Keys | Should -Contain 'Area'
            $command.Parameters.Keys | Should -Contain 'Group'
            $command.Parameters.Keys | Should -Contain 'SubArea'
            $command.Parameters.Keys | Should -Contain 'Privilege'
            $command.Parameters.Keys | Should -Contain 'EntryId'
            $command.Parameters.Keys | Should -Contain 'ResourceId'
            $command.Parameters.Keys | Should -Contain 'DescriptionResourceId'
            $command.Parameters.Keys | Should -Contain 'ToolTipResourceId'
            $command.Parameters.Keys | Should -Contain 'Titles'
            $command.Parameters.Keys | Should -Contain 'Descriptions'
            $command.Parameters.Keys | Should -Contain 'Icon'
            $command.Parameters.Keys | Should -Contain 'Entity'
            $command.Parameters.Keys | Should -Contain 'Url'
            $command.Parameters.Keys | Should -Contain 'ParentAreaId'
            $command.Parameters.Keys | Should -Contain 'ParentGroupId'
            $command.Parameters.Keys | Should -Contain 'PassThru'
            $command.Parameters.Keys | Should -Contain 'Connection'
            $command.Parameters.Keys | Should -Contain 'WhatIf'
            $command.Parameters.Keys | Should -Contain 'Confirm'
        }
    }

    Context 'Creating New Entries with ResourceId Properties' {
        BeforeAll {
            $connection = getMockConnection -Entities @('sitemap')
            
            # Create test sitemap
            $sitemapXml = "<SiteMap></SiteMap>"
            $sitemapEntity = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $sitemapEntity["sitemapname"] = "TestSitemap"
            $sitemapEntity["sitemapnameunique"] = "TestSitemap"
            $sitemapEntity["sitemapxml"] = $sitemapXml
            $connection.Create($sitemapEntity) | Out-Null
        }

        It "Can create new Area with ResourceId properties" {
            $result = Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSitemap" -Area -EntryId "TestArea" -ResourceId "Area_Test" -DescriptionResourceId "Test_Area_Description" -ToolTipResourceId "Test_Area_ToolTip" -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            $result.EntryType | Should -Be "Area"
            $result.Id | Should -Be "TestArea"
            $result.ResourceId | Should -Be "Area_Test"
            $result.DescriptionResourceId | Should -Be "Test_Area_Description"
            $result.ToolTipResourceId | Should -Be "Test_Area_ToolTip"
        }

        It "Can create new Group with ResourceId properties under Area" {
            # First ensure Area exists
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSitemap" -Area -EntryId "ParentArea" -ResourceId "Area_Parent" | Out-Null
            
            $result = Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSitemap" -Group -EntryId "TestGroup" -ParentAreaId "ParentArea" -ResourceId "Group_Test" -DescriptionResourceId "Test_Group_Description" -ToolTipResourceId "Test_Group_ToolTip" -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            $result.EntryType | Should -Be "Group"
            $result.Id | Should -Be "TestGroup"
            $result.ResourceId | Should -Be "Group_Test"
            $result.DescriptionResourceId | Should -Be "Test_Group_Description"
            $result.ToolTipResourceId | Should -Be "Test_Group_ToolTip"
            $result.ParentAreaId | Should -Be "ParentArea"
        }

        It "Can create new SubArea with ResourceId properties under Group" {
            # Ensure Area and Group exist
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSitemap" -Area -EntryId "ParentArea2" -ResourceId "Area_Parent2" | Out-Null
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSitemap" -Group -EntryId "ParentGroup" -ParentAreaId "ParentArea2" -ResourceId "Group_Parent" | Out-Null
            
            $result = Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSitemap" -SubArea -EntryId "TestSubArea" -ParentAreaId "ParentArea2" -ParentGroupId "ParentGroup" -ResourceId "SubArea_Test" -DescriptionResourceId "Test_SubArea_Description" -ToolTipResourceId "Test_SubArea_ToolTip" -Entity "contact" -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            $result.EntryType | Should -Be "SubArea"
            $result.Id | Should -Be "TestSubArea"
            $result.ResourceId | Should -Be "SubArea_Test"
            $result.DescriptionResourceId | Should -Be "Test_SubArea_Description"
            $result.ToolTipResourceId | Should -Be "Test_SubArea_ToolTip"
            $result.Entity | Should -Be "contact"
            $result.ParentAreaId | Should -Be "ParentArea2"
            $result.ParentGroupId | Should -Be "ParentGroup"
        }
    }

    Context 'Updating Existing Entries' {
        BeforeAll {
            $connection = getMockConnection -Entities @('sitemap')
            
            # Create test sitemap with existing entry
            $sitemapXml = @"
<SiteMap>
    <Area Id="ExistingArea" ResourceId="Old_Resource">
        <Group Id="ExistingGroup" ResourceId="Old_Group_Resource">
            <SubArea Id="ExistingSubArea" ResourceId="Old_SubArea_Resource" />
        </Group>
    </Area>
</SiteMap>
"@
            $sitemapEntity = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $sitemapEntity["sitemapname"] = "UpdateTestSitemap"
            $sitemapEntity["sitemapnameunique"] = "UpdateTestSitemap"
            $sitemapEntity["sitemapxml"] = $sitemapXml
            $connection.Create($sitemapEntity) | Out-Null
        }

        It "Can update existing Area with new ResourceId properties" {
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "UpdateTestSitemap" -Area -EntryId "ExistingArea" -ResourceId "Updated_Resource" -DescriptionResourceId "Updated_Description" -ToolTipResourceId "Updated_ToolTip"
            
            # Verify update
            $updatedArea = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "UpdateTestSitemap" -EntryId "ExistingArea"
            $updatedArea.ResourceId | Should -Be "Updated_Resource"
            $updatedArea.DescriptionResourceId | Should -Be "Updated_Description"
            $updatedArea.ToolTipResourceId | Should -Be "Updated_ToolTip"
        }

        It "Can update existing Group with new ResourceId properties" {
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "UpdateTestSitemap" -Group -EntryId "ExistingGroup" -ParentAreaId "ExistingArea" -ResourceId "Updated_Group_Resource" -DescriptionResourceId "Updated_Group_Description"
            
            # Verify update
            $updatedGroup = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "UpdateTestSitemap" -EntryId "ExistingGroup"
            $updatedGroup.ResourceId | Should -Be "Updated_Group_Resource"
            $updatedGroup.DescriptionResourceId | Should -Be "Updated_Group_Description"
        }

        It "Can update existing SubArea with new ResourceId properties" {
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "UpdateTestSitemap" -SubArea -EntryId "ExistingSubArea" -ParentAreaId "ExistingArea" -ParentGroupId "ExistingGroup" -ResourceId "Updated_SubArea_Resource" -DescriptionResourceId "Updated_SubArea_Description" -ToolTipResourceId "Updated_SubArea_ToolTip"
            
            # Verify update
            $updatedSubArea = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "UpdateTestSitemap" -EntryId "ExistingSubArea"
            $updatedSubArea.ResourceId | Should -Be "Updated_SubArea_Resource"
            $updatedSubArea.DescriptionResourceId | Should -Be "Updated_SubArea_Description"
            $updatedSubArea.ToolTipResourceId | Should -Be "Updated_SubArea_ToolTip"
        }
    }

    Context 'Error Handling' {
        It "Throws error when parent Area not found for Group creation" {
            $connection = getMockConnection -Entities @('sitemap')
            $sitemapXml = "<SiteMap></SiteMap>"
            $sitemapEntity = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $sitemapEntity["sitemapname"] = "ErrorTestSitemap"
            $sitemapEntity["sitemapnameunique"] = "ErrorTestSitemap"
            $sitemapEntity["sitemapxml"] = $sitemapXml
            $connection.Create($sitemapEntity) | Out-Null
            
            { Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "ErrorTestSitemap" -Group -EntryId "TestGroup" -ParentAreaId "NonExistentArea" } | Should -Throw
        }

        # Removed the missing ParentAreaId test since PowerShell now handles this with mandatory parameter validation
    }
}

Describe 'Remove-DataverseSitemapEntry' {
    Context 'Cmdlet Structure' {
        It "Remove-DataverseSitemapEntry cmdlet exists and has expected parameters" {
            $command = Get-Command Remove-DataverseSitemapEntry -ErrorAction SilentlyContinue
            $command | Should -Not -BeNullOrEmpty
            $command.Parameters.Keys | Should -Contain 'InputObject'
            $command.Parameters.Keys | Should -Contain 'Sitemap'
            $command.Parameters.Keys | Should -Contain 'SitemapUniqueName'
            $command.Parameters.Keys | Should -Contain 'SitemapId'
            $command.Parameters.Keys | Should -Contain 'Area'
            $command.Parameters.Keys | Should -Contain 'Group'
            $command.Parameters.Keys | Should -Contain 'SubArea'
            $command.Parameters.Keys | Should -Contain 'Privilege'
            $command.Parameters.Keys | Should -Contain 'EntryId'
            $command.Parameters.Keys | Should -Contain 'PrivilegeEntity'
            $command.Parameters.Keys | Should -Contain 'PrivilegeName'
            $command.Parameters.Keys | Should -Contain 'ParentSubAreaId'
            $command.Parameters.Keys | Should -Contain 'IfExists'
            $command.Parameters.Keys | Should -Contain 'Connection'
            $command.Parameters.Keys | Should -Contain 'WhatIf'
            $command.Parameters.Keys | Should -Contain 'Confirm'
        }
    }

    Context 'Removing Entries' {
        BeforeAll {
            $connection = getMockConnection -Entities @('sitemap')
            
            # Create test sitemap with entries to remove
            $sitemapXml = @"
<SiteMap>
    <Area Id="RemoveTestArea" ResourceId="Area_RemoveTest">
        <Group Id="RemoveTestGroup" ResourceId="Group_RemoveTest">
            <SubArea Id="RemoveTestSubArea" ResourceId="SubArea_RemoveTest" />
        </Group>
    </Area>
</SiteMap>
"@
            $sitemapEntity = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $sitemapEntity["sitemapname"] = "RemoveTestSitemap"
            $sitemapEntity["sitemapnameunique"] = "RemoveTestSitemap"
            $sitemapEntity["sitemapxml"] = $sitemapXml
            $connection.Create($sitemapEntity) | Out-Null
        }

        It "Can remove SubArea entry" {
            Remove-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "RemoveTestSitemap" -SubArea -EntryId "RemoveTestSubArea" -Confirm:$false
            
            # Verify removal
            $subAreas = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "RemoveTestSitemap" -SubArea
            $subAreas | Where-Object { $_.Id -eq "RemoveTestSubArea" } | Should -BeNullOrEmpty
        }

        It "Can remove Group entry" {
            Remove-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "RemoveTestSitemap" -Group -EntryId "RemoveTestGroup" -Confirm:$false
            
            # Verify removal
            $groups = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "RemoveTestSitemap" -Group
            $groups | Where-Object { $_.Id -eq "RemoveTestGroup" } | Should -BeNullOrEmpty
        }

        It "Can remove Area entry" {
            Remove-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "RemoveTestSitemap" -Area -EntryId "RemoveTestArea" -Confirm:$false
            
            # Verify removal
            $areas = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "RemoveTestSitemap" -Area
            $areas | Where-Object { $_.Id -eq "RemoveTestArea" } | Should -BeNullOrEmpty
        }

        It "Does not throw error when removing non-existent entry with IfExists flag" {
            { Remove-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "RemoveTestSitemap" -Area -EntryId "NonExistentEntry" -IfExists -Confirm:$false } | Should -Not -Throw
        }

        It "Throws error when removing non-existent entry without IfExists flag" {
            { Remove-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "RemoveTestSitemap" -Area -EntryId "NonExistentEntry" -Confirm:$false } | Should -Throw
        }
    }
}

Describe 'SitemapEntryInfo Class' {
    Context 'Properties' {
        It "SitemapEntryInfo type exists with expected properties" {
            $type = [Rnwood.Dataverse.Data.PowerShell.Commands.SitemapEntryInfo]
            $type | Should -Not -BeNullOrEmpty
            
            # Check properties exist
            $properties = $type.GetProperties().Name
            $properties | Should -Contain 'EntryType'
            $properties | Should -Contain 'Id'
            $properties | Should -Contain 'ResourceId'
            $properties | Should -Contain 'Titles'
            $properties | Should -Contain 'Descriptions'
            $properties | Should -Contain 'DescriptionResourceId'
            $properties | Should -Contain 'ToolTipResourceId'
            $properties | Should -Contain 'Icon'
            $properties | Should -Contain 'Entity'
            $properties | Should -Contain 'Url'
            $properties | Should -Contain 'IsDefault'
            $properties | Should -Contain 'ParentAreaId'
            $properties | Should -Contain 'ParentGroupId'
            $properties | Should -Contain 'ShowInAppNavigation'
            $properties | Should -Contain 'PrivilegeEntity'
            $properties | Should -Contain 'PrivilegeName'
            $properties | Should -Contain 'ParentSubAreaId'
        }

        It "Can create SitemapEntryInfo instance with ResourceId properties" {
            $entry = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.SitemapEntryInfo
            $entry.EntryType = [Rnwood.Dataverse.Data.PowerShell.Commands.SitemapEntryType]::Area
            $entry.Id = "TestArea"
            $entry.ResourceId = "Area_Test"
            $entry.DescriptionResourceId = "Test_Area_Description"
            $entry.ToolTipResourceId = "Test_Area_ToolTip"
            
            $entry.EntryType | Should -Be "Area"
            $entry.Id | Should -Be "TestArea"
            $entry.ResourceId | Should -Be "Area_Test"
            $entry.DescriptionResourceId | Should -Be "Test_Area_Description"
            $entry.ToolTipResourceId | Should -Be "Test_Area_ToolTip"
        }
    }
}

Describe 'Sitemap Entry XML Generation and Parsing' {
    Context 'XML Attribute Handling' {
        BeforeAll {
            $connection = getMockConnection -Entities @('sitemap')
        }
        
        It "Generates correct XML with ToolTipResourseId typo" {
            # Create test sitemap
            $sitemapXml = "<SiteMap></SiteMap>"
            $sitemapEntity = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $sitemapEntity["sitemapname"] = "XMLTestSitemap"
            $sitemapEntity["sitemapnameunique"] = "XMLTestSitemap"
            $sitemapEntity["sitemapxml"] = $sitemapXml
            $sitemapId = $connection.Create($sitemapEntity)
            
            # Add entry with ToolTipResourceId
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "XMLTestSitemap" -Area -EntryId "XMLTestArea" -ToolTipResourceId "Test_ToolTip"
            
            # Retrieve updated sitemap XML directly
            $updatedSitemap = $connection.Retrieve("sitemap", $sitemapId, [Microsoft.Xrm.Sdk.Query.ColumnSet]::new("sitemapxml"))
            $xmlContent = $updatedSitemap["sitemapxml"]
            
            # Verify the XML contains the typo attribute name
            $xmlContent | Should -Match 'ToolTipResourseId="Test_ToolTip"'
            $xmlContent | Should -Not -Match 'ToolTipResourceId='
        }

        It "Parses XML correctly handling ToolTipResourseId typo" {
            $sitemapXml = @"
<SiteMap>
    <Area Id="TypoTestArea" ResourceId="Area_TypoTest" DescriptionResourceId="TypoTest_Description" ToolTipResourseId="TypoTest_ToolTip" />
</SiteMap>
"@
            $sitemapEntity = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $sitemapEntity["sitemapname"] = "TypoTestSitemap"
            $sitemapEntity["sitemapnameunique"] = "TypoTestSitemap"
            $sitemapEntity["sitemapxml"] = $sitemapXml
            $connection.Create($sitemapEntity) | Out-Null
            
            $entry = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TypoTestSitemap" -EntryId "TypoTestArea"
            $entry.ToolTipResourceId | Should -Be "TypoTest_ToolTip"
        }
    }

    Context 'Privilege Parsing and Management' {
        BeforeAll {
            $connection = getMockConnection -Entities @('sitemap')
            
            # Sample sitemap XML with Privilege elements as shown in user's example
            $privilegeSitemapXml = @"
<SiteMap IntroducedVersion="7.0.0.0">
    <Area Id="Workplace" ResourceId="Area_Workplace" Icon="/_imgs/workplace_24x24.gif" DescriptionResourceId="Workplace_Area_Description" ShowGroups="true" IntroducedVersion="7.0.0.0">
        <Group Id="MyWork" ResourceId="Group_MyWork" DescriptionResourceId="MyWork_Description" IntroducedVersion="7.0.0.0">
            <SubArea Id="nav_activities" Entity="activitypointer" ResourceId="Area_Activities" DescriptionResourceId="Activities_Area_Description" Icon="/_imgs/ico_18_activitiesServices.gif" GetStartedPanePath="Activities_Web_Part_Path" IntroducedVersion="7.0.0.0">
                <Privilege Entity="activitypointer" Privilege="Read" />
                <Privilege Entity="activitypointer" Privilege="Create" />
                <Privilege Entity="email" Privilege="Read" />
                <Privilege Entity="email" Privilege="Create" />
                <Privilege Entity="phonecall" Privilege="Read" />
                <Privilege Entity="task" Privilege="Write" />
            </SubArea>
        </Group>
    </Area>
</SiteMap>
"@
            
            # Add mock sitemap to FakeXrmEasy
            $privilegeSitemapEntity = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $privilegeSitemapEntity["sitemapname"] = "PrivilegeSitemap"
            $privilegeSitemapEntity["sitemapnameunique"] = "PrivilegeSitemap"
            $privilegeSitemapEntity["sitemapxml"] = $privilegeSitemapXml
            $connection.Create($privilegeSitemapEntity) | Out-Null
        }

        It "Get-DataverseSitemapEntry returns SubArea with privilege collection" {
            $subArea = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "PrivilegeSitemap" -EntryId "nav_activities"
            $subArea | Should -Not -BeNullOrEmpty
            $subArea.EntryType | Should -Be "SubArea"
            $subArea.Id | Should -Be "nav_activities"
            $subArea.Privileges | Should -Not -BeNullOrEmpty
            $subArea.Privileges.Count | Should -Be 6
            
            # Verify specific privileges
            $activityPointerRead = $subArea.Privileges | Where-Object { $_.Entity -eq "activitypointer" -and $_.Privilege -eq "Read" }
            $activityPointerRead | Should -Not -BeNullOrEmpty
            
            $emailCreate = $subArea.Privileges | Where-Object { $_.Entity -eq "email" -and $_.Privilege -eq "Create" }
            $emailCreate | Should -Not -BeNullOrEmpty
            
            $taskWrite = $subArea.Privileges | Where-Object { $_.Entity -eq "task" -and $_.Privilege -eq "Write" }
            $taskWrite | Should -Not -BeNullOrEmpty
        }

        It "Get-DataverseSitemapEntry can return individual Privilege entries" {
            $privileges = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "PrivilegeSitemap" -Privilege -ParentSubAreaId "nav_activities"
            $privileges | Should -Not -BeNullOrEmpty
            $privileges.Count | Should -Be 6
            
            # Verify all privileges are returned as separate entries
            foreach ($privilege in $privileges) {
                $privilege.EntryType | Should -Be "Privilege"
                $privilege.PrivilegeEntity | Should -Not -BeNullOrEmpty
                $privilege.PrivilegeName | Should -Not -BeNullOrEmpty
                $privilege.ParentSubAreaId | Should -Be "nav_activities"
            }
            
            # Check specific privilege entries
            $activityPointerRead = $privileges | Where-Object { $_.PrivilegeEntity -eq "activitypointer" -and $_.PrivilegeName -eq "Read" }
            $activityPointerRead | Should -Not -BeNullOrEmpty
            $activityPointerRead.EntryType | Should -Be "Privilege"
        }

        It "Set-DataverseSitemapEntry can add new Privilege to existing SubArea" {
            # Add a new privilege to the existing SubArea
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "PrivilegeSitemap" -Privilege -PrivilegeEntity "appointment" -PrivilegeName "Read" -ParentSubAreaId "nav_activities"
            
            # Verify the privilege was added
            $subArea = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "PrivilegeSitemap" -EntryId "nav_activities"
            $newPrivilege = $subArea.Privileges | Where-Object { $_.Entity -eq "appointment" -and $_.Privilege -eq "Read" }
            $newPrivilege | Should -Not -BeNullOrEmpty
            
            # Verify total privilege count increased
            $subArea.Privileges.Count | Should -Be 7
        }

        It "Set-DataverseSitemapEntry can update existing Privilege" {
            # First, add a privilege to update
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "PrivilegeSitemap" -Privilege -PrivilegeEntity "contact" -PrivilegeName "Read" -ParentSubAreaId "nav_activities"
            
            # Update it to Write privilege
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "PrivilegeSitemap" -Privilege -PrivilegeEntity "contact" -PrivilegeName "Write" -ParentSubAreaId "nav_activities"
            
            # Verify the update
            $subArea = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "PrivilegeSitemap" -EntryId "nav_activities"
            $updatedPrivilege = $subArea.Privileges | Where-Object { $_.Entity -eq "contact" -and $_.Privilege -eq "Write" }
            $updatedPrivilege | Should -Not -BeNullOrEmpty
            
            # Verify the old privilege is gone
            $oldPrivilege = $subArea.Privileges | Where-Object { $_.Entity -eq "contact" -and $_.Privilege -eq "Read" }
            $oldPrivilege | Should -BeNullOrEmpty
        }

        It "Remove-DataverseSitemapEntry can remove specific Privilege" {
            # First add a privilege to remove
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "PrivilegeSitemap" -Privilege -PrivilegeEntity "account" -PrivilegeName "Delete" -ParentSubAreaId "nav_activities"
            
            # Verify it was added
            $subArea = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "PrivilegeSitemap" -EntryId "nav_activities"
            $addedPrivilege = $subArea.Privileges | Where-Object { $_.Entity -eq "account" -and $_.Privilege -eq "Delete" }
            $addedPrivilege | Should -Not -BeNullOrEmpty
            $initialCount = $subArea.Privileges.Count
            
            # Remove the privilege
            Remove-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "PrivilegeSitemap" -Privilege -PrivilegeEntity "account" -PrivilegeName "Delete" -ParentSubAreaId "nav_activities" -Confirm:$false
            
            # Verify it was removed
            $subAreaAfter = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "PrivilegeSitemap" -EntryId "nav_activities"
            $removedPrivilege = $subAreaAfter.Privileges | Where-Object { $_.Entity -eq "account" -and $_.Privilege -eq "Delete" }
            $removedPrivilege | Should -BeNullOrEmpty
            $subAreaAfter.Privileges.Count | Should -Be ($initialCount - 1)
        }
    }
}

Describe 'Sitemap Titles and Descriptions with LCID' {
    Context 'Parsing Titles and Descriptions Elements' {
        BeforeAll {
            $connection = getMockConnection -Entities @('sitemap')
            
            # Sitemap XML with new Titles and Descriptions format
            $sitemapXmlWithLCID = @"
<SiteMap IntroducedVersion="7.0.0.0">
    <Area Id="TestArea" ResourceId="Area_Test" ShowGroups="true" Icon="/_imgs/test.gif" IntroducedVersion="7.0.0.0">
        <Titles>
            <Title LCID="1033" Title="Test Area English" />
            <Title LCID="1036" Title="Zone de test français" />
            <Title LCID="1031" Title="Testbereich Deutsch" />
        </Titles>
        <Descriptions>
            <Description LCID="1033" Description="This is a test area in English" />
            <Description LCID="1036" Description="Ceci est une zone de test en français" />
        </Descriptions>
        <Group Id="TestGroup" ResourceId="Group_Test" IntroducedVersion="7.0.0.0">
            <Titles>
                <Title LCID="1033" Title="Test Group" />
                <Title LCID="1036" Title="Groupe de test" />
            </Titles>
            <Descriptions>
                <Description LCID="1033" Description="Test group description" />
            </Descriptions>
            <SubArea Id="TestSubArea" Icon="/_imgs/test_16.gif" Entity="contact" IntroducedVersion="7.0.0.0">
                <Titles>
                    <Title LCID="1033" Title="Contacts" />
                    <Title LCID="1036" Title="Contacts français" />
                </Titles>
            </SubArea>
        </Group>
    </Area>
</SiteMap>
"@
            
            # Add mock sitemap to FakeXrmEasy
            $sitemapEntity = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $sitemapEntity["sitemapname"] = "MultilingualSitemap"
            $sitemapEntity["sitemapnameunique"] = "MultilingualSitemap"
            $sitemapEntity["sitemapxml"] = $sitemapXmlWithLCID
            $connection.Create($sitemapEntity) | Out-Null
        }

        It "Parses Titles element with multiple LCIDs" {
            $area = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "MultilingualSitemap" -EntryId "TestArea"
            $area | Should -Not -BeNullOrEmpty
            $area.Titles | Should -Not -BeNullOrEmpty
            $area.Titles.Count | Should -Be 3
            $area.Titles[1033] | Should -Be "Test Area English"
            $area.Titles[1036] | Should -Be "Zone de test français"
            $area.Titles[1031] | Should -Be "Testbereich Deutsch"
        }

        It "Parses Descriptions element with multiple LCIDs" {
            $area = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "MultilingualSitemap" -EntryId "TestArea"
            $area.Descriptions | Should -Not -BeNullOrEmpty
            $area.Descriptions.Count | Should -Be 2
            $area.Descriptions[1033] | Should -Be "This is a test area in English"
            $area.Descriptions[1036] | Should -Be "Ceci est une zone de test en français"
        }

        It "Parses Group Titles with multiple LCIDs" {
            $group = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "MultilingualSitemap" -EntryId "TestGroup"
            $group.Titles | Should -Not -BeNullOrEmpty
            $group.Titles.Count | Should -Be 2
            $group.Titles[1033] | Should -Be "Test Group"
            $group.Titles[1036] | Should -Be "Groupe de test"
        }

        It "Parses SubArea Titles with multiple LCIDs" {
            $subArea = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "MultilingualSitemap" -EntryId "TestSubArea"
            $subArea.Titles | Should -Not -BeNullOrEmpty
            $subArea.Titles.Count | Should -Be 2
            $subArea.Titles[1033] | Should -Be "Contacts"
            $subArea.Titles[1036] | Should -Be "Contacts français"
        }
    }

    Context 'Setting Titles and Descriptions with LCID' {
        BeforeEach {
            $connection = getMockConnection -Entities @('sitemap')
            
            # Create a simple sitemap for testing
            $baseSitemapXml = @"
<SiteMap IntroducedVersion="7.0.0.0">
    <Area Id="TestSetArea" ResourceId="Area_Test" ShowGroups="true" IntroducedVersion="7.0.0.0" />
</SiteMap>
"@
            
            $sitemapEntity = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $sitemapEntity["sitemapname"] = "TestSetSitemap"
            $sitemapEntity["sitemapnameunique"] = "TestSetSitemap"
            $sitemapEntity["sitemapxml"] = $baseSitemapXml
            $connection.Create($sitemapEntity) | Out-Null
        }

        It "Creates Area with Titles dictionary" {
            $titles = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
            $titles.Add(1033, "English Title")
            $titles.Add(1036, "Titre français")
            $titles.Add(1031, "Deutscher Titel")
            
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSetSitemap" `
                -Area -EntryId "NewMultilingualArea" -Titles $titles -Confirm:$false
            
            $area = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSetSitemap" -EntryId "NewMultilingualArea"
            $area | Should -Not -BeNullOrEmpty
            $area.Titles | Should -Not -BeNullOrEmpty
            $area.Titles.Count | Should -Be 3
            $area.Titles[1033] | Should -Be "English Title"
            $area.Titles[1036] | Should -Be "Titre français"
            $area.Titles[1031] | Should -Be "Deutscher Titel"
        }

        It "Creates Area with Descriptions dictionary" {
            $descriptions = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
            $descriptions.Add(1033, "English Description")
            $descriptions.Add(1036, "Description française")
            
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSetSitemap" `
                -Area -EntryId "NewDescArea" -Descriptions $descriptions -Confirm:$false
            
            $area = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSetSitemap" -EntryId "NewDescArea"
            $area | Should -Not -BeNullOrEmpty
            $area.Descriptions | Should -Not -BeNullOrEmpty
            $area.Descriptions.Count | Should -Be 2
            $area.Descriptions[1033] | Should -Be "English Description"
            $area.Descriptions[1036] | Should -Be "Description française"
        }

        It "Updates existing Titles additively" {
            # First create with English title
            $initialTitles = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
            $initialTitles.Add(1033, "Initial English")
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSetSitemap" `
                -Area -EntryId "AdditiveArea" -Titles $initialTitles -Confirm:$false
            
            # Now add French title
            $frenchTitle = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
            $frenchTitle.Add(1036, "Titre français ajouté")
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSetSitemap" `
                -Area -EntryId "AdditiveArea" -Titles $frenchTitle -Confirm:$false
            
            # Verify both titles exist
            $area = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSetSitemap" -EntryId "AdditiveArea"
            $area.Titles.Count | Should -Be 2
            $area.Titles[1033] | Should -Be "Initial English"
            $area.Titles[1036] | Should -Be "Titre français ajouté"
        }

        It "Removes LCID when null value is provided" {
            # Create with multiple titles
            $initialTitles = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
            $initialTitles.Add(1033, "English")
            $initialTitles.Add(1036, "Français")
            $initialTitles.Add(1031, "Deutsch")
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSetSitemap" `
                -Area -EntryId "RemoveArea" -Titles $initialTitles -Confirm:$false
            
            # Remove French title
            $removeTitle = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
            $removeTitle.Add(1036, $null)
            Set-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSetSitemap" `
                -Area -EntryId "RemoveArea" -Titles $removeTitle -Confirm:$false
            
            # Verify French title is removed
            $area = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "TestSetSitemap" -EntryId "RemoveArea"
            $area.Titles.Count | Should -Be 2
            $area.Titles.ContainsKey(1033) | Should -Be $true
            $area.Titles.ContainsKey(1031) | Should -Be $true
            $area.Titles.ContainsKey(1036) | Should -Be $false
        }
    }

    Context 'Backward Compatibility with Old Format' {
        BeforeAll {
            $connection = getMockConnection -Entities @('sitemap')
            
            # Sitemap XML with old Title/Description attributes
            $oldFormatXml = @"
<SiteMap IntroducedVersion="7.0.0.0">
    <Area Id="OldFormatArea" Title="Old Style Title" Description="Old style description" ShowGroups="true" IntroducedVersion="7.0.0.0">
        <Group Id="OldFormatGroup" Title="Old Group Title" Description="Old group description" IntroducedVersion="7.0.0.0">
            <SubArea Id="OldFormatSubArea" Title="Old SubArea Title" Entity="contact" IntroducedVersion="7.0.0.0" />
        </Group>
    </Area>
</SiteMap>
"@
            
            $sitemapEntity = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $sitemapEntity["sitemapname"] = "OldFormatSitemap"
            $sitemapEntity["sitemapnameunique"] = "OldFormatSitemap"
            $sitemapEntity["sitemapxml"] = $oldFormatXml
            $connection.Create($sitemapEntity) | Out-Null
        }

        It "Parses old Title attribute as LCID 1033" {
            $area = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "OldFormatSitemap" -EntryId "OldFormatArea"
            $area.Titles | Should -Not -BeNullOrEmpty
            $area.Titles.Count | Should -Be 1
            $area.Titles[1033] | Should -Be "Old Style Title"
        }

        It "Parses old Description attribute as LCID 1033" {
            $area = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "OldFormatSitemap" -EntryId "OldFormatArea"
            $area.Descriptions | Should -Not -BeNullOrEmpty
            $area.Descriptions.Count | Should -Be 1
            $area.Descriptions[1033] | Should -Be "Old style description"
        }

        It "Parses old format for Group" {
            $group = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "OldFormatSitemap" -EntryId "OldFormatGroup"
            $group.Titles[1033] | Should -Be "Old Group Title"
            $group.Descriptions[1033] | Should -Be "Old group description"
        }

        It "Parses old format for SubArea" {
            $subArea = Get-DataverseSitemapEntry -Connection $connection -SitemapUniqueName "OldFormatSitemap" -EntryId "OldFormatSubArea"
            $subArea.Titles[1033] | Should -Be "Old SubArea Title"
        }
    }
}