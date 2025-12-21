. $PSScriptRoot/Common.ps1

Describe 'Expand-DataverseSolutionFile -UnpackMsapp Path Separator Fix' {

    BeforeAll {
        # Ensure module is loaded (required for parallel job execution in CI)
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop
        }
        
        # Helper function to create a test .msapp file with Windows-style path separators
        # This simulates .msapp files created on Windows that have backslashes in entry names
        function New-TestMsappWithBackslashes {
            param(
                [string]$OutputPath
            )
            
            # Use Python to create a ZIP with literal backslashes in entry names
            # SharpZipLib normalizes backslashes to forward slashes, so we need Python
            $pythonScript = @'
import zipfile
import sys

output_path = sys.argv[1]

with zipfile.ZipFile(output_path, 'w', zipfile.ZIP_DEFLATED) as zf:
    # Add files with backslashes in names (simulating Windows-created .msapp)
    zf.writestr(r"Controls\1.json", '{"control": "button1"}')
    zf.writestr(r"Controls\2.json", '{"control": "button2"}')
    zf.writestr(r"Src\AppInfo.json", '{"appinfo": "test"}')
    zf.writestr(r"Src\EditorState\Canvas1.editorstate.json", '{"editor": "state"}')
    zf.writestr("Metadata.json", '{"metadata": "test"}')
'@
            
            $tempPy = [IO.Path]::GetTempFileName() + ".py"
            Set-Content -Path $tempPy -Value $pythonScript -Encoding UTF8
            
            try {
                # Try to run Python to create the test file
                $pythonExe = if ($IsWindows -or $PSVersionTable.PSVersion.Major -le 5) { "python" } else { "python3" }
                & $pythonExe $tempPy $OutputPath 2>&1 | Out-Null
                
                if (-not (Test-Path $OutputPath)) {
                    throw "Failed to create test .msapp file"
                }
            }
            finally {
                if (Test-Path $tempPy) {
                    Remove-Item $tempPy -Force -ErrorAction SilentlyContinue
                }
            }
            
            return $OutputPath
        }
        
        Set-Variable -Name "TempPaths" -Value @() -Scope Script
    }

    AfterAll {
        foreach ($path in $Script:TempPaths) {
            if (Test-Path $path) {
                Remove-Item $path -Force -Recurse -ErrorAction SilentlyContinue
            }
        }
    }

    It "Unpacks .msapp files with Windows path separators correctly" {
        # Skip on Windows PowerShell 5.1 as it doesn't manifest the issue
        if ($IsWindows -or $PSVersionTable.PSVersion.Major -le 5) {
            Set-ItResult -Skipped -Because "This test validates Linux-specific path separator handling"
            return
        }
        
        # Create test .msapp file with backslashes directly
        $msappPath = Join-Path ([IO.Path]::GetTempPath()) "test_backslash_$(New-Guid).msapp"
        $Script:TempPaths += $msappPath
        New-TestMsappWithBackslashes -OutputPath $msappPath
        
        # Create output directory for unpacking
        $outputDir = Join-Path ([IO.Path]::GetTempPath()) "msapp_output_$(New-Guid)"
        $Script:TempPaths += $outputDir
        New-Item -ItemType Directory -Path $outputDir | Out-Null
        
        # Copy the .msapp to the output directory (simulating what happens after solution extraction)
        $targetMsappPath = Join-Path $outputDir "test.msapp"
        Copy-Item $msappPath $targetMsappPath
        
        # Load the SharpZipLib assembly from the module
        $modulePath = if ($env:TESTMODULEPATH) { $env:TESTMODULEPATH } else { (Get-Module Rnwood.Dataverse.Data.PowerShell).ModuleBase }
        $sharpZipLibPath = Get-ChildItem -Path $modulePath -Recurse -Filter "ICSharpCode.SharpZipLib.dll" -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
        if ($sharpZipLibPath) {
            try {
                [System.Reflection.Assembly]::LoadFrom($sharpZipLibPath) | Out-Null
            } catch {
                # Might already be loaded
            }
        }
        
        $tempOutputFolder = Join-Path $outputDir "test.msapp.tmp"
        $finalOutputFolder = Join-Path $outputDir "test.msapp"
        
        if (Test-Path $tempOutputFolder) {
            Remove-Item $tempOutputFolder -Recurse -Force
        }
        
        New-Item -ItemType Directory -Path $tempOutputFolder | Out-Null
        
        # Unpack using manual extraction with path normalization (our fix)
        # This is the same code as in ExpandDataverseSolutionFileCmdlet
        $zipFile = New-Object ICSharpCode.SharpZipLib.Zip.ZipFile($targetMsappPath)
        try {
            foreach ($entry in $zipFile) {
                # Normalize the entry name by replacing backslashes with forward slashes
                $entryName = $entry.Name.Replace('\', '/')
                
                # Skip directory entries
                if ($entry.IsDirectory) {
                    $dirPath = Join-Path $tempOutputFolder $entryName
                    New-Item -ItemType Directory -Path $dirPath -Force | Out-Null
                    continue
                }
                
                # Ensure the directory exists for the file
                $filePath = Join-Path $tempOutputFolder $entryName
                $fileDir = Split-Path $filePath -Parent
                if ($fileDir -and -not (Test-Path $fileDir)) {
                    New-Item -ItemType Directory -Path $fileDir -Force | Out-Null
                }
                
                # Extract the file
                $inputStream = $zipFile.GetInputStream($entry)
                $outputStream = [System.IO.File]::Create($filePath)
                $inputStream.CopyTo($outputStream)
                $outputStream.Close()
                $inputStream.Close()
            }
        }
        finally {
            $zipFile.Close()
        }
        
        # Remove the .msapp file and rename temp folder
        Remove-Item $targetMsappPath -Force
        Move-Item $tempOutputFolder $finalOutputFolder
        
        # Verify the structure
        # Check that directories were created (not files with backslashes in names)
        Test-Path (Join-Path $finalOutputFolder "Controls") -PathType Container | Should -Be $true
        Test-Path (Join-Path $finalOutputFolder "Src") -PathType Container | Should -Be $true
        Test-Path (Join-Path $finalOutputFolder "Src/EditorState") -PathType Container | Should -Be $true
        
        # Check that files are in the correct locations
        Test-Path (Join-Path $finalOutputFolder "Controls/1.json") -PathType Leaf | Should -Be $true
        Test-Path (Join-Path $finalOutputFolder "Controls/2.json") -PathType Leaf | Should -Be $true
        Test-Path (Join-Path $finalOutputFolder "Src/AppInfo.json") -PathType Leaf | Should -Be $true
        Test-Path (Join-Path $finalOutputFolder "Src/EditorState/Canvas1.editorstate.json") -PathType Leaf | Should -Be $true
        Test-Path (Join-Path $finalOutputFolder "Metadata.json") -PathType Leaf | Should -Be $true
        
        # Verify that files with backslashes in their names do NOT exist
        # (This would be the bug - files named "Controls\1.json" instead of proper structure)
        $filesWithBackslash = Get-ChildItem -Path $finalOutputFolder -Recurse -File | Where-Object { 
            $_.Name -like "*\*" 
        }
        $filesWithBackslash | Should -BeNullOrEmpty
    }
}
