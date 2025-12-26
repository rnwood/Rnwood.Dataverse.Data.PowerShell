. $PSScriptRoot/Common.ps1

Describe "MsApp Controls JSON Generation" {
    Context "Set-DataverseMsAppProperties - Generates Controls/1.json" {
        It "Generates Controls/1.json with properties from App.pa.yaml" {
            # Arrange
            $testMsAppPath = Join-Path $TestDrive "test.msapp"
            $exampleMsApp = Join-Path $PSScriptRoot ".." "ExampleCanvasApp_1_0_0_1.zip"
            
            # Extract the original msapp for testing
            if (Test-Path $exampleMsApp) {
                # Extract the example solution and get the msapp
                $tempExtractPath = Join-Path $TestDrive "extracted"
                New-Item -ItemType Directory -Path $tempExtractPath -Force | Out-Null
                
                Add-Type -AssemblyName System.IO.Compression.FileSystem
                [System.IO.Compression.ZipFile]::ExtractToDirectory($exampleMsApp, $tempExtractPath)
                
                # Find the .msapp file
                $msappFile = Get-ChildItem -Path $tempExtractPath -Recurse -Filter "*.msapp" | Select-Object -First 1
                
                if ($msappFile) {
                    Copy-Item $msappFile.FullName -Destination $testMsAppPath
                    
                    # Act - Update App properties
                    $yamlContent = @"
Properties:
  Theme: =PowerAppsTheme
  BackEnabled: =true
"@
                    
                    Set-DataverseMsAppProperties -MsAppPath $testMsAppPath -YamlContent $yamlContent
                    
                    # Assert - Check that Controls/1.json was generated
                    $tempVerifyPath = Join-Path $TestDrive "verify"
                    if (Test-Path $tempVerifyPath) {
                        Remove-Item -Path $tempVerifyPath -Recurse -Force
                    }
                    New-Item -ItemType Directory -Path $tempVerifyPath -Force | Out-Null
                    [System.IO.Compression.ZipFile]::ExtractToDirectory($testMsAppPath, $tempVerifyPath)
                    
                    $controlFile = Join-Path $tempVerifyPath "Controls" "1.json"
                    $controlFile | Should -Exist
                    
                    # Verify the JSON content
                    $controlJson = Get-Content $controlFile -Raw | ConvertFrom-Json
                    $controlJson.TopParent.Name | Should -Be "App"
                    $controlJson.TopParent.ControlUniqueId | Should -Be "1"
                    
                    # Verify properties are in ControlPropertyState
                    $controlJson.TopParent.ControlPropertyState | Should -Contain "Theme"
                    $controlJson.TopParent.ControlPropertyState | Should -Contain "BackEnabled"
                    
                    # Verify Rules contain the properties
                    $themeRule = $controlJson.TopParent.Rules | Where-Object { $_.Property -eq "Theme" }
                    $themeRule | Should -Not -BeNullOrEmpty
                    $themeRule.InvariantScript | Should -Be "PowerAppsTheme"
                    
                    $backRule = $controlJson.TopParent.Rules | Where-Object { $_.Property -eq "BackEnabled" }
                    $backRule | Should -Not -BeNullOrEmpty
                    $backRule.InvariantScript | Should -Be "true"
                }
            }
        }
    }
    
    Context "Set-DataverseMsAppScreen - Regenerates all Controls JSON files" {
        It "Regenerates Controls JSON for App and all screens" {
            # Arrange
            $testMsAppPath = Join-Path $TestDrive "test2.msapp"
            $exampleMsApp = Join-Path $PSScriptRoot ".." "ExampleCanvasApp_1_0_0_1.zip"
            
            if (Test-Path $exampleMsApp) {
                # Extract the example solution and get the msapp
                $tempExtractPath = Join-Path $TestDrive "extracted2"
                New-Item -ItemType Directory -Path $tempExtractPath -Force | Out-Null
                
                Add-Type -AssemblyName System.IO.Compression.FileSystem
                [System.IO.Compression.ZipFile]::ExtractToDirectory($exampleMsApp, $tempExtractPath)
                
                # Find the .msapp file
                $msappFile = Get-ChildItem -Path $tempExtractPath -Recurse -Filter "*.msapp" | Select-Object -First 1
                
                if ($msappFile) {
                    Copy-Item $msappFile.FullName -Destination $testMsAppPath
                    
                    # Act - Update a screen
                    $yamlContent = @"
Properties:
  LoadingSpinnerColor: =RGBA(56, 96, 178, 1)
  Fill: =Color.White
"@
                    
                    Set-DataverseMsAppScreen -MsAppPath $testMsAppPath -ScreenName "Screen1" -YamlContent $yamlContent
                    
                    # Assert - Check that Controls/*.json were regenerated
                    $tempVerifyPath = Join-Path $TestDrive "verify2"
                    if (Test-Path $tempVerifyPath) {
                        Remove-Item -Path $tempVerifyPath -Recurse -Force
                    }
                    New-Item -ItemType Directory -Path $tempVerifyPath -Force | Out-Null
                    [System.IO.Compression.ZipFile]::ExtractToDirectory($testMsAppPath, $tempVerifyPath)
                    
                    # Check Controls/1.json (App)
                    $appControlFile = Join-Path $tempVerifyPath "Controls" "1.json"
                    $appControlFile | Should -Exist
                    
                    # Check Controls/4.json (Screen1 - should be ID 4)
                    $screenControlFile = Join-Path $tempVerifyPath "Controls" "4.json"
                    $screenControlFile | Should -Exist
                    
                    # Verify Screen1 content
                    $screenJson = Get-Content $screenControlFile -Raw | ConvertFrom-Json
                    $screenJson.TopParent.Name | Should -Be "Screen1"
                    $screenJson.TopParent.ControlUniqueId | Should -Be "4"
                    
                    # Verify properties are in ControlPropertyState
                    $screenJson.TopParent.ControlPropertyState | Should -Contain "LoadingSpinnerColor"
                    $screenJson.TopParent.ControlPropertyState | Should -Contain "Fill"
                    
                    # Verify Rules contain the properties
                    $colorRule = $screenJson.TopParent.Rules | Where-Object { $_.Property -eq "LoadingSpinnerColor" }
                    $colorRule | Should -Not -BeNullOrEmpty
                    $colorRule.InvariantScript | Should -Be "RGBA(56, 96, 178, 1)"
                }
            }
        }
    }
}
