$ErrorActionPreference = "Stop"

Describe "View Management E2E Tests" {

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

        Import-Module Rnwood.Dataverse.Data.PowerShell
    }

    It "Can create and update views without creating duplicates" {
        $ErrorActionPreference = "Stop"
        $ConfirmPreference = 'None'
        $VerbosePreference = 'Continue'
        
        # Helper function to clean up test views
        function Remove-TestViews {
            param(
                [Parameter(Mandatory)]
                $Connection,
                [Parameter(Mandatory)]
                [string]$ViewNamePattern
            )
            
            try {
                # Clean up system views
                Get-DataverseView -Connection $Connection -Name $ViewNamePattern -ViewType "System" -ErrorAction SilentlyContinue | ForEach-Object {
                    Write-Host "Cleaning up system view: $($_.Name) (ID: $($_.Id))"
                    Remove-DataverseView -Connection $Connection -Id $_.Id -ViewType "System" -Confirm:$false -ErrorAction SilentlyContinue
                }
                
                # Clean up personal views
                Get-DataverseView -Connection $Connection -Name $ViewNamePattern -ViewType "Personal" -ErrorAction SilentlyContinue | ForEach-Object {
                    Write-Host "Cleaning up personal view: $($_.Name) (ID: $($_.Id))"
                    Remove-DataverseView -Connection $Connection -Id $_.Id -ViewType "Personal" -Confirm:$false -ErrorAction SilentlyContinue
                }
            } catch {
                Write-Host "Warning during cleanup: $_"
            }
        }
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            
            # Generate unique test identifier
            $testId = [Guid]::NewGuid().ToString("N").Substring(0, 8)
            $viewName = "E2ETest View $testId"
            Write-Host "Test ID: $testId"
            Write-Host "View Name: $viewName"
            
            # Pre-cleanup: Remove any existing test views with the same pattern
            Write-Host "`nPre-cleanup: Removing existing test views..."
            Remove-TestViews -Connection $connection -ViewNamePattern "E2ETest View*"
            
            $viewId = $null
            
            try {
                # ==========================================
                # Step 1: Create initial view
                # ==========================================
                Write-Host "`nStep 1: Creating initial system view..."
                
                $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
    <attribute name="emailaddress1" />
    <filter type="and">
      <condition attribute="statecode" operator="eq" value="0" />
    </filter>
  </entity>
</fetch>
"@
                
                $viewId = Set-DataverseView -Connection $connection `
                    -Name $viewName `
                    -TableName "contact" `
                    -ViewType "System" `
                    -FetchXml $fetchXml `
                    -Description "E2E test view - initial version" `
                    -QueryType MainApplicationView `
                    -PassThru
                
                Write-Host "✓ Created view with ID: $viewId"
                
                # Verify view was created
                $retrievedView = Get-DataverseView -Connection $connection -Id $viewId
                $retrievedView | Should -Not -BeNullOrEmpty
                $retrievedView.Name | Should -Be $viewName
                Write-Host "✓ Verified view creation"
                
                # ==========================================
                # Step 2: Update view using same Name and TableName (should NOT create duplicate)
                # ==========================================
                Write-Host "`nStep 2: Updating view using Name and TableName (upsert pattern)..."
                
                $fetchXml2 = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
    <attribute name="emailaddress1" />
    <attribute name="telephone1" />
    <filter type="and">
      <condition attribute="statecode" operator="eq" value="0" />
    </filter>
  </entity>
</fetch>
"@
                
                $viewId2 = Set-DataverseView -Connection $connection `
                    -Name $viewName `
                    -TableName "contact" `
                    -ViewType "System" `
                    -FetchXml $fetchXml2 `
                    -Description "E2E test view - updated version" `
                    -QueryType MainApplicationView `
                    -PassThru
                
                Write-Host "✓ Updated view, returned ID: $viewId2"
                
                # ==========================================
                # Step 3: Verify NO duplicate was created
                # ==========================================
                Write-Host "`nStep 3: Verifying no duplicate was created..."
                
                # IDs should be the same
                $viewId2 | Should -Be $viewId
                Write-Host "✓ View ID matches original (no new view created)"
                
                # Query for views with this name
                $allViewsWithName = Get-DataverseView -Connection $connection -Name $viewName -TableName "contact" -ViewType "System"
                $allViewsWithName.Count | Should -Be 1
                Write-Host "✓ Only one view exists with this name"
                
                $allViewsWithName[0].Id | Should -Be $viewId
                Write-Host "✓ The single view has the correct ID"
                
                # Verify the update was applied
                $updatedView = Get-DataverseView -Connection $connection -Id $viewId
                $updatedView.Description | Should -Be "E2E test view - updated version"
                Write-Host "✓ View description was updated"
                
                # ==========================================
                # Step 4: Test multiple consecutive updates
                # ==========================================
                Write-Host "`nStep 4: Testing multiple consecutive updates..."
                
                for ($i = 1; $i -le 3; $i++) {
                    $fetchXmlUpdate = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
    <filter type="and">
      <condition attribute="statecode" operator="eq" value="0" />
    </filter>
  </entity>
</fetch>
"@
                    
                    $updatedId = Set-DataverseView -Connection $connection `
                        -Name $viewName `
                        -TableName "contact" `
                        -ViewType "System" `
                        -FetchXml $fetchXmlUpdate `
                        -Description "E2E test view - update iteration $i" `
                        -QueryType MainApplicationView `
                        -PassThru
                    
                    $updatedId | Should -Be $viewId
                    Write-Host "✓ Update iteration ${i}: ID still matches original"
                }
                
                # Verify still only one view
                $allViewsAfterUpdates = Get-DataverseView -Connection $connection -Name $viewName -TableName "contact" -ViewType "System"
                $allViewsAfterUpdates.Count | Should -Be 1
                Write-Host "✓ Still only one view exists after multiple updates"
                
                # ==========================================
                # Step 5: Test personal view (different ViewType)
                # ==========================================
                Write-Host "`nStep 5: Testing personal view creation and update..."
                
                $personalViewName = "E2ETest Personal View $testId"
                
                $personalViewId = Set-DataverseView -Connection $connection `
                    -Name $personalViewName `
                    -TableName "contact" `
                    -ViewType "Personal" `
                    -FetchXml $fetchXml `
                    -PassThru
                
                Write-Host "✓ Created personal view with ID: $personalViewId"
                
                # Update the personal view
                $personalViewId2 = Set-DataverseView -Connection $connection `
                    -Name $personalViewName `
                    -TableName "contact" `
                    -ViewType "Personal" `
                    -FetchXml $fetchXml2 `
                    -PassThru
                
                $personalViewId2 | Should -Be $personalViewId
                Write-Host "✓ Personal view updated without creating duplicate"
                
                # Verify only one personal view with this name
                $personalViews = Get-DataverseView -Connection $connection -Name $personalViewName -TableName "contact" -ViewType "Personal"
                $personalViews.Count | Should -Be 1
                Write-Host "✓ Only one personal view exists with this name"
                
                Write-Host "`n✓ All view management tests passed successfully!"
                
            } finally {
                # ==========================================
                # Cleanup
                # ==========================================
                Write-Host "`nCleanup: Removing test views..."
                Remove-TestViews -Connection $connection -ViewNamePattern "E2ETest*View*$testId"
                Write-Host "✓ Cleanup complete"
            }
            
        } catch {
            # Format error with full details
            $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
            Write-Host $errorDetails
            throw $errorDetails
        }
    }
}
