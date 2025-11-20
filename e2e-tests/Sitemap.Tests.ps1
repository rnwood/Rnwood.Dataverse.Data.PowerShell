$ErrorActionPreference = "Stop"

Describe "Sitemap Manipulation" {

    BeforeAll {

        if ($env:TESTMODULEPATH) {
            $source = $env:TESTMODULEPATH
        }
        else {
            $source = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/"
        }

        $tempmodulefolder = "$([IO.Path]::GetTempPath())/$([Guid]::NewGuid())"
        new-item -ItemType Directory $tempmodulefolder
        copy-item -Recurse $source $tempmodulefolder/Rnwood.Dataverse.Data.PowerShell
        $env:PSModulePath = $tempmodulefolder;
        $env:ChildProcessPSModulePath = $tempmodulefolder
    }

    It "Can perform full lifecycle of sitemap manipulation including entries" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            # Retry helper function with exponential backoff
            function Invoke-WithRetry {
                param(
                    [Parameter(Mandatory = $true)]
                    [scriptblock]$ScriptBlock,
                    [int]$MaxRetries = 5,
                    [int]$InitialDelaySeconds = 10
                )

                $attempt = 0
                $delay = $InitialDelaySeconds

                while ($attempt -lt $MaxRetries) {
                    try {
                        $attempt++
                        Write-Verbose "Attempt $attempt of $MaxRetries"
                        & $ScriptBlock
                        return  # Success
                    }
                    catch {
                        # Check if this is an EntityCustomization operation error
                        if ($_.Exception.Message -like "*Cannot start the requested operation*EntityCustomization*") {
                            Write-Warning "EntityCustomization operation conflict detected. Waiting 2 minutes before retry without incrementing attempt count..."
                            $attempt--  # Don't count this as a retry attempt
                            Start-Sleep -Seconds 120
                            continue
                        }
                        
                        if ($attempt -eq $MaxRetries) {
                            Write-Error "All $MaxRetries attempts failed. Last error: $_"
                            throw
                        }

                        Write-Warning "Attempt $attempt failed: $_. Retrying in $delay seconds..."
                        Start-Sleep -Seconds $delay
                        $delay = $delay * 2
                    }
                }
            }

            try {
                # Connect to environment
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Generate unique test identifiers to avoid conflicts with parallel CI runs
                $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
                $sitemapUniqueName = "test_sitemap_$testRunId"
                $sitemapName = "Test Sitemap $testRunId"
                
                Write-Host "Using test sitemap unique name: $sitemapUniqueName"
                
                # --- CLEANUP: Remove any previously failed test sitemaps ---
                Write-Host "Cleaning up any existing test sitemaps from previous failed runs..."
                $existingSitemaps = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemap -Connection $connection | Where-Object {
                        $_.UniqueName -like "test_sitemap_*"
                    }
                }
                
                foreach ($existingSitemap in $existingSitemaps) {
                    Write-Host "  Removing old sitemap: $($existingSitemap.UniqueName) (ID: $($existingSitemap.Id))"
                    try {
                        Invoke-WithRetry {
                            Wait-DataversePublish -Connection $connection -Verbose
                            Remove-DataverseSitemap -Connection $connection -Id $existingSitemap.Id -IfExists -Confirm:$false
                        }
                    } catch {
                        Write-Warning "Failed to remove old sitemap $($existingSitemap.UniqueName): $_"
                    }
                }
                Write-Host "Cleanup complete."
                
                # --- TEST 1: Create a new sitemap ---
                Write-Host "`nTest 1: Creating new sitemap..."
                $sitemapXml = @"
<SiteMap IntroducedVersion="7.0.0.0">
  <Area Id="area_test_$testRunId" ResourceId="Area.Test" ShowGroups="true" IntroducedVersion="7.0.0.0">
    <Titles>
      <Title LCID="1033" Title="Test Area $testRunId" />
    </Titles>
    <Group Id="group_test_$testRunId" ResourceId="Group.Test" IntroducedVersion="7.0.0.0" IsProfile="false">
      <Titles>
        <Title LCID="1033" Title="Test Group $testRunId" />
      </Titles>
      <SubArea Id="subarea_test_$testRunId" Icon="/_imgs/imagestrips/transparent_spacer.gif" Entity="contact" Client="All,Outlook,OutlookLaptopClient,OutlookWorkstationClient,Web" AvailableOffline="true" PassParams="false" Sku="All,OnPremise,Live,SPLA">
        <Titles>
          <Title LCID="1033" Title="Test SubArea $testRunId" />
        </Titles>
      </SubArea>
    </Group>
  </Area>
</SiteMap>
"@
                
                $sitemapId = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Set-DataverseSitemap -Connection $connection -Name $sitemapName -UniqueName $sitemapUniqueName -SitemapXml $sitemapXml -PassThru -Confirm:$false
                }
                
                if (-not $sitemapId) {
                    throw "Failed to create sitemap - no ID returned"
                }
                Write-Host "Created sitemap with ID: $sitemapId"
                
                # --- TEST 2: Retrieve the created sitemap ---
                Write-Host "`nTest 2: Retrieving sitemap by UniqueName..."
                $retrievedSitemap = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemap -Connection $connection -UniqueName $sitemapUniqueName
                }
                
                if (-not $retrievedSitemap) {
                    throw "Failed to retrieve sitemap by UniqueName"
                }
                if ($retrievedSitemap.Id -ne $sitemapId) {
                    throw "Retrieved sitemap ID mismatch. Expected: $sitemapId, Got: $($retrievedSitemap.Id)"
                }
                if ($retrievedSitemap.Name -ne $sitemapName) {
                    throw "Retrieved sitemap Name mismatch. Expected: $sitemapName, Got: $($retrievedSitemap.Name)"
                }
                Write-Host "Successfully retrieved sitemap: $($retrievedSitemap.Name)"
                
                # --- TEST 3: Retrieve sitemap by ID ---
                Write-Host "`nTest 3: Retrieving sitemap by ID..."
                $retrievedById = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemap -Connection $connection -Id $sitemapId
                }
                
                if (-not $retrievedById) {
                    throw "Failed to retrieve sitemap by ID"
                }
                if ($retrievedById.UniqueName -ne $sitemapUniqueName) {
                    throw "Retrieved sitemap UniqueName mismatch"
                }
                Write-Host "Successfully retrieved sitemap by ID"
                
                # --- TEST 4: Update the sitemap ---
                Write-Host "`nTest 4: Updating sitemap..."
                $updatedName = "Updated $sitemapName"
                
                # Get current sitemap to retrieve XML
                $currentSitemap = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemap -Connection $connection -Id $sitemapId
                }
                
                # Update name along with XML to ensure proper Dataverse handling
                Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Set-DataverseSitemap -Connection $connection -Id $sitemapId -Name $updatedName -SitemapXml $currentSitemap.SitemapXml -Confirm:$false
                }
                
                $updatedSitemap = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemap -Connection $connection -Id $sitemapId
                }
                if ($updatedSitemap.Name -ne $updatedName) {
                    throw "Sitemap name update failed. Expected: $updatedName, Got: $($updatedSitemap.Name)"
                }
                Write-Host "Successfully updated sitemap name to: $updatedName"
                
                # --- TEST 5: Retrieve sitemap entries ---
                Write-Host "`nTest 5: Retrieving sitemap entries..."
                
                # Get all entries
                $allEntries = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId
                }
                if ($allEntries.Count -eq 0) {
                    throw "No sitemap entries found"
                }
                Write-Host "Found $($allEntries.Count) sitemap entries"
                
                # Get Area entries
                $areaEntries = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -Area
                }
                if ($areaEntries.Count -eq 0) {
                    throw "No Area entries found"
                }
                Write-Host "Found $($areaEntries.Count) Area entries"
                
                # Get Group entries
                $groupEntries = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -Group
                }
                if ($groupEntries.Count -eq 0) {
                    throw "No Group entries found"
                }
                Write-Host "Found $($groupEntries.Count) Group entries"
                
                # Get SubArea entries
                $subAreaEntries = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -SubArea
                }
                if ($subAreaEntries.Count -eq 0) {
                    throw "No SubArea entries found"
                }
                Write-Host "Found $($subAreaEntries.Count) SubArea entries"
                
                # --- TEST 6: Add a new Area entry ---
                Write-Host "`nTest 6: Adding new Area entry..."
                $newAreaId = "area_new_$testRunId"
                $newAreaEntry = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Set-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -Area `
                    -EntryId $newAreaId `
                    -Titles @{ 1033 = "New Test Area $testRunId" } `
                    -Icon "/_imgs/area_icon.png" `
                    -PassThru -Confirm:$false
                }
                
                if (-not $newAreaEntry) {
                    throw "Failed to create new Area entry"
                }
                Write-Host "Created new Area entry: $newAreaId"
                
                # Verify new area exists
                $verifyArea = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -Area -EntryId $newAreaId
                }
                if (-not $verifyArea) {
                    throw "Failed to retrieve newly created Area entry"
                }
                Write-Host "Verified new Area entry exists"
                
                # --- TEST 7: Add a Group to the new Area ---
                Write-Host "`nTest 7: Adding Group to new Area..."
                $newGroupId = "group_new_$testRunId"
                $newGroupEntry = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Set-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -Group `
                    -EntryId $newGroupId `
                    -ParentAreaId $newAreaId `
                    -Titles @{ 1033 = "New Test Group $testRunId" } `
                    -PassThru -Confirm:$false
                }
                
                if (-not $newGroupEntry) {
                    throw "Failed to create new Group entry"
                }
                Write-Host "Created new Group entry: $newGroupId"
                
                # --- TEST 8: Add a SubArea to the new Group ---
                Write-Host "`nTest 8: Adding SubArea to new Group..."
                $newSubAreaId = "subarea_new_$testRunId"
                $newSubAreaEntry = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Set-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -SubArea `
                    -EntryId $newSubAreaId `
                    -ParentAreaId $newAreaId `
                    -ParentGroupId $newGroupId `
                    -Entity "account" `
                    -Titles @{ 1033 = "New Test SubArea $testRunId" } `
                    -Icon "/_imgs/subarea_icon.png" `
                    -PassThru -Confirm:$false
                }
                
                if (-not $newSubAreaEntry) {
                    throw "Failed to create new SubArea entry"
                }
                Write-Host "Created new SubArea entry: $newSubAreaId"
                
                # --- TEST 9: Update an existing entry ---
                Write-Host "`nTest 9: Updating SubArea entry..."
                $updatedSubAreaEntry = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Set-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -SubArea `
                    -EntryId $newSubAreaId `
                    -Titles @{ 1033 = "Updated SubArea $testRunId" } `
                    -PassThru -Confirm:$false
                }
                
                if (-not $updatedSubAreaEntry) {
                    throw "Failed to update SubArea entry"
                }
                
                # Verify update
                $verifyUpdatedSubArea = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -SubArea -EntryId $newSubAreaId
                }
                $updatedTitle = $verifyUpdatedSubArea.Titles[1033]
                if ($updatedTitle -ne "Updated SubArea $testRunId") {
                    throw "SubArea title update failed. Expected: 'Updated SubArea $testRunId', Got: '$updatedTitle'"
                }
                Write-Host "Successfully updated SubArea entry"
                
                # --- TEST 10: Publish sitemap ---
                Write-Host "`nTest 10: Publishing sitemap..."
                Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Set-DataverseSitemap -Connection $connection -Id $sitemapId -Name $updatedName -Publish -Confirm:$false
                }
                Write-Host "Successfully published sitemap"
                
                # Verify published sitemap is retrievable
                $publishedSitemap = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemap -Connection $connection -Id $sitemapId -Published
                }
                if (-not $publishedSitemap) {
                    throw "Failed to retrieve published sitemap"
                }
                Write-Host "Verified published sitemap is retrievable"
                
                # --- TEST 11: Remove SubArea entry ---
                Write-Host "`nTest 11: Removing SubArea entry..."
                Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Remove-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -SubArea -EntryId $newSubAreaId -Confirm:$false
                }
                
                # Verify deletion
                $deletedSubArea = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -SubArea -EntryId $newSubAreaId
                }
                if ($deletedSubArea) {
                    throw "SubArea entry still exists after deletion"
                }
                Write-Host "Successfully removed SubArea entry"
                
                # --- CLEANUP: Remove the test sitemap ---
                Write-Host "`nCleaning up: Removing test sitemap..."
                Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Remove-DataverseSitemap -Connection $connection -Id $sitemapId -Confirm:$false
                }
                Write-Host "Successfully removed test sitemap"
                
                # Verify deletion
                $deletedSitemap = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemap -Connection $connection -Id $sitemapId
                }
                if ($deletedSitemap) {
                    throw "Sitemap still exists after deletion"
                }
                Write-Host "Verified sitemap was deleted"
                
                # --- TEST 12: Test IfExists flag on non-existent sitemap ---
                Write-Host "`nTest 12: Testing IfExists flag on non-existent sitemap..."
                Remove-DataverseSitemap -Connection $connection -Id $sitemapId -IfExists -Confirm:$false
                Write-Host "IfExists flag worked correctly - no error on non-existent sitemap"
                
                Write-Host "`n=== All sitemap tests passed successfully ==="
                
            } catch {
                # Attempt cleanup on failure
                Write-Host "ERROR: Test failed with exception: $_"
                Write-Host "Stack trace: $($_.ScriptStackTrace)"
                
                # Try to clean up test sitemap if it exists
                try {
                    if ($sitemapId) {
                        Write-Host "Attempting cleanup of test sitemap..."
                        $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                        Remove-DataverseSitemap -Connection $connection -Id $sitemapId -IfExists -Confirm:$false
                        Write-Host "Cleanup successful"
                    }
                } catch {
                    Write-Warning "Failed to cleanup test sitemap: $_"
                }
                
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Can set sitemap group titles with hashtable keys (integer and string)" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            # Retry helper function with exponential backoff
            function Invoke-WithRetry {
                param(
                    [Parameter(Mandatory = $true)]
                    [scriptblock]$ScriptBlock,
                    [int]$MaxRetries = 5,
                    [int]$InitialDelaySeconds = 10
                )

                $attempt = 0
                $delay = $InitialDelaySeconds

                while ($attempt -lt $MaxRetries) {
                    try {
                        $attempt++
                        Write-Verbose "Attempt $attempt of $MaxRetries"
                        & $ScriptBlock
                        return  # Success
                    }
                    catch {
                        # Check if this is an EntityCustomization operation error
                        if ($_.Exception.Message -like "*Cannot start the requested operation*EntityCustomization*") {
                            Write-Warning "EntityCustomization operation conflict detected. Waiting 2 minutes before retry without incrementing attempt count..."
                            $attempt--  # Don't count this as a retry attempt
                            Start-Sleep -Seconds 120
                            continue
                        }
                        
                        if ($attempt -eq $MaxRetries) {
                            Write-Error "All $MaxRetries attempts failed. Last error: $_"
                            throw
                        }

                        Write-Warning "Attempt $attempt failed: $_. Retrying in $delay seconds..."
                        Start-Sleep -Seconds $delay
                        $delay = $delay * 2
                    }
                }
            }

            try {
                # Connect to environment
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Generate unique test identifiers
                $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
                $sitemapUniqueName = "test_titles_$testRunId"
                $sitemapName = "Test Titles Sitemap $testRunId"
                
                Write-Host "Using test sitemap unique name: $sitemapUniqueName"
                
                # --- TEST 1: Create sitemap ---
                Write-Host "`nTest 1: Creating new sitemap for title testing..."
                $sitemap = Invoke-WithRetry {
                    Set-DataverseSitemap -Connection $connection -Name $sitemapName -UniqueName $sitemapUniqueName -PassThru -Confirm:$false
                }
                $sitemapId = $sitemap.Id
                Write-Host "Created sitemap with ID: $sitemapId"
                
                # --- TEST 2: Add Area with integer keys ---
                Write-Host "`nTest 2: Adding Area with integer keys for titles..."
                $areaId = "area_intkeys_$testRunId"
                $areaEntry = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Set-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -Area `
                        -EntryId $areaId `
                        -Titles @{ 1033 = "English Area"; 1036 = "Zone française" } `
                        -Icon "/_imgs/area_icon.png" `
                        -PassThru -Confirm:$false
                }
                
                if (-not $areaEntry) {
                    throw "Failed to create Area with integer key titles"
                }
                Write-Host "Created Area with integer key titles"
                
                # Verify titles were set correctly
                $verifyArea = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -Area -EntryId $areaId
                }
                if ($verifyArea.Titles[1033] -ne "English Area") {
                    throw "Area title for LCID 1033 not set correctly"
                }
                if ($verifyArea.Titles[1036] -ne "Zone française") {
                    throw "Area title for LCID 1036 not set correctly"
                }
                Write-Host "Verified Area titles with integer keys"
                
                # --- TEST 3: Add Group with string keys for titles ---
                Write-Host "`nTest 3: Adding Group with string keys for titles..."
                $groupId = "group_strkeys_$testRunId"
                $groupEntry = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Set-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -Group `
                        -EntryId $groupId `
                        -ParentAreaId $areaId `
                        -Titles @{ "1033" = "English Group"; "1036" = "Groupe français" } `
                        -PassThru -Confirm:$false
                }
                
                if (-not $groupEntry) {
                    throw "Failed to create Group with string key titles"
                }
                Write-Host "Created Group with string key titles"
                
                # Verify titles were set correctly
                $verifyGroup = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -Group -EntryId $groupId
                }
                if ($verifyGroup.Titles[1033] -ne "English Group") {
                    throw "Group title for LCID 1033 not set correctly"
                }
                if ($verifyGroup.Titles[1036] -ne "Groupe français") {
                    throw "Group title for LCID 1036 not set correctly"
                }
                Write-Host "Verified Group titles with string keys"
                
                # --- TEST 4: Add SubArea with mixed integer and string keys, and entity validation ---
                Write-Host "`nTest 4: Adding SubArea with mixed keys and entity validation..."
                $subAreaId = "subarea_mixed_$testRunId"
                $subAreaEntry = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Set-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -SubArea `
                        -EntryId $subAreaId `
                        -ParentAreaId $areaId `
                        -ParentGroupId $groupId `
                        -Entity "account" `
                        -Titles @{ 1033 = "English SubArea"; "1036" = "Sous-zone française" } `
                        -Descriptions @{ "1033" = "English Description"; 1036 = "Description française" } `
                        -Icon "/_imgs/subarea_icon.png" `
                        -PassThru -Confirm:$false
                }
                
                if (-not $subAreaEntry) {
                    throw "Failed to create SubArea with mixed keys"
                }
                Write-Host "Created SubArea with mixed keys and entity validation passed"
                
                # Verify titles and descriptions
                $verifySubArea = Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -SubArea -EntryId $subAreaId
                }
                if ($verifySubArea.Titles[1033] -ne "English SubArea") {
                    throw "SubArea title for LCID 1033 not set correctly"
                }
                if ($verifySubArea.Titles[1036] -ne "Sous-zone française") {
                    throw "SubArea title for LCID 1036 not set correctly"
                }
                if ($verifySubArea.Descriptions[1033] -ne "English Description") {
                    throw "SubArea description for LCID 1033 not set correctly"
                }
                if ($verifySubArea.Descriptions[1036] -ne "Description française") {
                    throw "SubArea description for LCID 1036 not set correctly"
                }
                if ($verifySubArea.Entity -ne "account") {
                    throw "SubArea entity not set correctly"
                }
                Write-Host "Verified SubArea titles, descriptions with mixed keys and entity"
                
                # --- TEST 5: Test entity validation with invalid entity ---
                Write-Host "`nTest 5: Testing entity validation with invalid entity..."
                $invalidSubAreaId = "subarea_invalid_$testRunId"
                $validationFailed = $false
                try {
                    Invoke-WithRetry {
                        Wait-DataversePublish -Connection $connection -Verbose
                        Set-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -SubArea `
                            -EntryId $invalidSubAreaId `
                            -ParentAreaId $areaId `
                            -ParentGroupId $groupId `
                            -Entity "nonexistent_entity_xyz" `
                            -Titles @{ 1033 = "Should Fail" } `
                            -Confirm:$false
                    }
                }
                catch {
                    if ($_.Exception.Message -like "*does not exist*") {
                        $validationFailed = $true
                        Write-Host "Entity validation correctly rejected invalid entity"
                    }
                    else {
                        throw "Unexpected error during entity validation test: $_"
                    }
                }
                
                if (-not $validationFailed) {
                    throw "Entity validation did not fail for invalid entity"
                }
                
                Write-Host "`nAll title and entity validation tests passed successfully!"
                
            }
            finally {
                # Cleanup
                Write-Host "`nCleaning up test sitemap..."
                try {
                    if ($sitemapId) {
                        Invoke-WithRetry {
                            Wait-DataversePublish -Connection $connection -Verbose
                            Remove-DataverseSitemap -Connection $connection -Id $sitemapId -Confirm:$false
                        }
                        Write-Host "Test sitemap removed successfully"
                    }
                }
                catch {
                    Write-Warning "Failed to clean up test sitemap: $_"
                }
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
}
