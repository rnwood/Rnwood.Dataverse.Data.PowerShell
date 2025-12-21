$ErrorActionPreference = "Stop"

Describe "Canvas App E2E Tests" {

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

    It "Can create, update Canvas app and manage screens/components" {
            
        $ErrorActionPreference = "Stop"
        $ConfirmPreference = 'None'  # Suppress all confirmation prompts in non-interactive mode
        $VerbosePreference = 'Continue'  # Enable verbose output
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            $connection.EnableAffinityCookie = $true    

            # Generate unique test identifiers
            $timestamp = [DateTime]::UtcNow.ToString("yyyyMMddHHmmss")
            $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
            $appName = "new_e2ecanvas_${timestamp}_${testRunId}"
            
            Write-Host "Test Canvas app: $appName"
            
            # Create a temporary .msapp file
            $tempMsappPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "test_${testRunId}.msapp")
            
            Write-Host "Step 1: Creating temporary .msapp file..."
            
            # Create a minimal .msapp file (it's a zip file)
            Add-Type -AssemblyName System.IO.Compression.FileSystem
            $zipArchive = [System.IO.Compression.ZipFile]::Open($tempMsappPath, [System.IO.Compression.ZipArchiveMode]::Create)
            
            # Add minimal required files
            $headerEntry = $zipArchive.CreateEntry("Header.json")
            $headerWriter = [System.IO.StreamWriter]::new($headerEntry.Open())
            $headerWriter.Write('{"DocVersion":"1.33","MinVersionToLoad":"1.33","MSAppStructureVersion":"2.0"}')
            $headerWriter.Close()
            
            $propsEntry = $zipArchive.CreateEntry("Properties.json")
            $propsWriter = [System.IO.StreamWriter]::new($propsEntry.Open())
            $propsWriter.Write('{"InstrumentationKey":"","AppCreationSource":"AppFromScratch","AppDescription":"E2E Test","AppPreviewFlagsKey":""}')
            $propsWriter.Close()
            
            $appYamlEntry = $zipArchive.CreateEntry("Src/App.pa.yaml")
            $appYamlWriter = [System.IO.StreamWriter]::new($appYamlEntry.Open())
            $appYamlWriter.Write("App:`n  Properties:`n    Theme: =PowerAppsTheme`n")
            $appYamlWriter.Close()
            
            $screenYamlEntry = $zipArchive.CreateEntry("Src/Screen1.pa.yaml")
            $screenYamlWriter = [System.IO.StreamWriter]::new($screenYamlEntry.Open())
            $screenYamlWriter.Write("Screens:`n  Screen1:`n    Properties:`n      LoadingSpinnerColor: =RGBA(56, 96, 178, 1)`n")
            $screenYamlWriter.Close()
            
            $dsEntry = $zipArchive.CreateEntry("References/DataSources.json")
            $dsWriter = [System.IO.StreamWriter]::new($dsEntry.Open())
            $dsWriter.Write("[]")
            $dsWriter.Close()
            
            $resEntry = $zipArchive.CreateEntry("References/Resources.json")
            $resWriter = [System.IO.StreamWriter]::new($resEntry.Open())
            $resWriter.Write("[]")
            $resWriter.Close()
            
            $themesEntry = $zipArchive.CreateEntry("References/Themes.json")
            $themesWriter = [System.IO.StreamWriter]::new($themesEntry.Open())
            $themesWriter.Write("{}")
            $themesWriter.Close()
            
            $zipArchive.Dispose()
            
            Write-Host "✓ Temporary .msapp file created: $tempMsappPath"
            
            Write-Host "Step 2: Creating Canvas app..."
            $appId = Set-DataverseCanvasApp -Connection $connection `
                -Name $appName `
                -DisplayName "E2E Test Canvas App" `
                -Description "E2E test for Canvas app cmdlets" `
                -MsAppPath $tempMsappPath `
                -PassThru `
                -Confirm:$false
            
            $appId | Should -Not -BeNullOrEmpty
            Write-Host "✓ Canvas app created with ID: $appId"
            
            Write-Host "Step 3: Verifying Canvas app exists..."
            $retrievedApp = Get-DataverseCanvasApp -Connection $connection -Id $appId
            $retrievedApp | Should -Not -BeNullOrEmpty
            $retrievedApp.Name | Should -Be $appName
            Write-Host "✓ Canvas app verified"
            
            Write-Host "Step 4: Testing screen management..."
            # Get screens from the .msapp file
            $screens = Get-DataverseMsAppScreen -MsAppPath $tempMsappPath
            $screens | Should -Not -BeNullOrEmpty
            $screens.Count | Should -BeGreaterThan 0
            Write-Host "✓ Retrieved $($screens.Count) screen(s) from .msapp file"
            
            # Add a new screen to the .msapp file
            $newScreenYaml = @"
Screens:
  TestScreen2:
    Properties:
      LoadingSpinnerColor: =RGBA(255, 0, 0, 1)
"@
            Set-DataverseMsAppScreen -MsAppPath $tempMsappPath -ScreenName "TestScreen2" -YamlContent $newScreenYaml -Confirm:$false
            
            # Verify new screen was added
            $screensAfter = Get-DataverseMsAppScreen -MsAppPath $tempMsappPath
            $screensAfter.Count | Should -Be ($screens.Count + 1)
            Write-Host "✓ Added new screen to .msapp file"
            
            Write-Host "Step 5: Updating Canvas app with modified .msapp..."
            Set-DataverseCanvasApp -Connection $connection `
                -Id $appId `
                -MsAppPath $tempMsappPath `
                -Confirm:$false
            
            Write-Host "✓ Canvas app updated with new screen"
            
            Write-Host "Step 6: Testing component management..."
            # Add a component to the .msapp file
            $componentYaml = @"
Component:
  TestButton:
    Properties:
      Width: 100
      Height: 50
"@
            Set-DataverseMsAppComponent -MsAppPath $tempMsappPath -ComponentName "TestButton" -YamlContent $componentYaml -Confirm:$false
            
            # Verify component was added
            $components = Get-DataverseMsAppComponent -MsAppPath $tempMsappPath
            $components | Should -Not -BeNullOrEmpty
            $components.Count | Should -BeGreaterThan 0
            Write-Host "✓ Added component to .msapp file"
            
            Write-Host "Step 7: Removing test screen..."
            Remove-DataverseMsAppScreen -MsAppPath $tempMsappPath -ScreenName "TestScreen2" -Confirm:$false
            
            $screensAfterRemove = Get-DataverseMsAppScreen -MsAppPath $tempMsappPath
            $screensAfterRemove.Count | Should -Be $screens.Count
            Write-Host "✓ Removed screen from .msapp file"
            
            Write-Host "Step 8: Removing test component..."
            Remove-DataverseMsAppComponent -MsAppPath $tempMsappPath -ComponentName "TestButton" -Confirm:$false
            
            $componentsAfterRemove = Get-DataverseMsAppComponent -MsAppPath $tempMsappPath
            $componentsAfterRemove.Count | Should -Be 0
            Write-Host "✓ Removed component from .msapp file"
            
            Write-Host "Step 9: Testing upsert with Name..."
            # Update using Name (should update existing app)
            Set-DataverseCanvasApp -Connection $connection `
                -Name $appName `
                -DisplayName "Updated E2E Test Canvas App" `
                -MsAppPath $tempMsappPath `
                -Confirm:$false
            
            $updatedApp = Get-DataverseCanvasApp -Connection $connection -Id $appId
            $updatedApp.DisplayName | Should -Be "Updated E2E Test Canvas App"
            Write-Host "✓ Upsert by Name worked correctly"
            
            Write-Host "Step 10: Cleaning up - Removing Canvas app..."
            Remove-DataverseCanvasApp -Connection $connection -Id $appId -Confirm:$false
            Write-Host "✓ Canvas app removed"
            
            # Cleanup temp file
            if (Test-Path $tempMsappPath) {
                Remove-Item $tempMsappPath -Force
            }
            
            Write-Host "=== All Canvas App E2E tests passed! ==="
        }
        catch {
            Write-Error "E2E test failed: $_"
            throw
        }
    }
}
