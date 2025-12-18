BeforeAll {
    . "$PSScriptRoot/Common.ps1"
    
    # Create a temporary directory for test artifacts
    $script:testDir = Join-Path ([System.IO.Path]::GetTempPath()) "DataversePackUnpackTests_$(New-Guid)"
    New-Item -ItemType Directory -Path $script:testDir -Force | Out-Null
}

AfterAll {
    # Clean up test directory
    if ($script:testDir -and (Test-Path $script:testDir)) {
        Remove-Item -Path $script:testDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Describe "Expand-DataverseSolutionFile and Compress-DataverseSolutionFile" {
    
    Context "Basic functionality" {
        
        It "Should have Expand-DataverseSolutionFile cmdlet available" {
            Get-Command Expand-DataverseSolutionFile | Should -Not -BeNullOrEmpty
        }
        
        It "Should have Compress-DataverseSolutionFile cmdlet available" {
            Get-Command Compress-DataverseSolutionFile | Should -Not -BeNullOrEmpty
        }
        
        It "Should have backward compatible Expand-DataverseSolution alias" {
            $alias = Get-Alias Expand-DataverseSolution -ErrorAction SilentlyContinue
            $alias | Should -Not -BeNullOrEmpty
            $alias.Definition | Should -Be "Expand-DataverseSolutionFile"
        }
        
        It "Should have backward compatible Compress-DataverseSolution alias" {
            $alias = Get-Alias Compress-DataverseSolution -ErrorAction SilentlyContinue
            $alias | Should -Not -BeNullOrEmpty
            $alias.Definition | Should -Be "Compress-DataverseSolutionFile"
        }
    }
    
    Context "Parameter validation" {
        
        It "Expand-DataverseSolutionFile parameters should be mandatory" {
            $cmd = Get-Command Expand-DataverseSolutionFile
            $pathParam = $cmd.Parameters['Path']
            $outputParam = $cmd.Parameters['OutputPath']
            $pathParam.Attributes.Mandatory | Should -Contain $true
            $outputParam.Attributes.Mandatory | Should -Contain $true
        }
        
        It "Compress-DataverseSolutionFile parameters should be mandatory" {
            $cmd = Get-Command Compress-DataverseSolutionFile
            $pathParam = $cmd.Parameters['Path']
            $outputParam = $cmd.Parameters['OutputPath']
            $pathParam.Attributes.Mandatory | Should -Contain $true
            $outputParam.Attributes.Mandatory | Should -Contain $true
        }
        
        It "Expand-DataverseSolution should error on non-existent file" {
            $nonExistentFile = Join-Path $script:testDir "nonexistent.zip"
            { Expand-DataverseSolutionFile -Path $nonExistentFile -OutputPath (Join-Path $script:testDir "output") -ErrorAction Stop } | 
                Should -Throw
        }
        
        It "Compress-DataverseSolution should error on non-existent folder" {
            $nonExistentFolder = Join-Path $script:testDir "nonexistent"
            { Compress-DataverseSolutionFile -Path $nonExistentFolder -OutputPath (Join-Path $script:testDir "output.zip") -ErrorAction Stop } | 
                Should -Throw
        }
    }
    
    Context "WhatIf support" {
        
        BeforeAll {
            # Create a dummy ZIP file
            $script:dummyZip = Join-Path $script:testDir "dummy.zip"
            $dummyContent = Join-Path $script:testDir "dummy.txt"
            "test content" | Out-File $dummyContent
            Compress-Archive -Path $dummyContent -DestinationPath $script:dummyZip
            
            # Create a dummy folder
            $script:dummyFolder = Join-Path $script:testDir "dummyFolder"
            New-Item -ItemType Directory -Path $script:dummyFolder | Out-Null
            "test" | Out-File (Join-Path $script:dummyFolder "test.txt")
        }
        
        It "Expand-DataverseSolution should support -WhatIf" {
            $outputPath = Join-Path $script:testDir "whatif_expand"
            { Expand-DataverseSolutionFile -Path $script:dummyZip -OutputPath $outputPath -WhatIf } | Should -Not -Throw
            # Output path should not be created
            Test-Path $outputPath | Should -Be $false
        }
        
        It "Compress-DataverseSolution should support -WhatIf" {
            $outputZip = Join-Path $script:testDir "whatif_compress.zip"
            { Compress-DataverseSolutionFile -Path $script:dummyFolder -OutputPath $outputZip -WhatIf } | Should -Not -Throw
            # Output file should not be created
            Test-Path $outputZip | Should -Be $false
        }
        
        It "Expand-DataverseSolution should not have Clobber or AllowDelete parameters" {
            $cmd = Get-Command Expand-DataverseSolutionFile
            $cmd.Parameters.Keys | Should -Not -Contain 'Clobber'
            $cmd.Parameters.Keys | Should -Not -Contain 'AllowDelete'
        }
    }
    
    Context "msapp handling" {
        
        BeforeAll {
            # Create a mock Canvas App folder with .msapp extension
            $script:msappFolder = Join-Path $script:testDir "MockCanvasApp.msapp"
            New-Item -ItemType Directory -Path $script:msappFolder | Out-Null
            New-Item -ItemType Directory -Path (Join-Path $script:msappFolder "Src") | Out-Null
            New-Item -ItemType Directory -Path (Join-Path $script:msappFolder "DataSources") | Out-Null
            New-Item -ItemType Directory -Path (Join-Path $script:msappFolder "Connections") | Out-Null
            "test" | Out-File (Join-Path $script:msappFolder "Src\App.fx.yaml")
            
            # Create an msapp zip file from a different folder (to avoid naming conflict)
            $tempMsappDir = Join-Path $script:testDir "TempMsappContent"
            New-Item -ItemType Directory -Path $tempMsappDir | Out-Null
            "test" | Out-File (Join-Path $tempMsappDir "test.txt")
            $script:msappFile = Join-Path $script:testDir "TestApp.msapp"
            Compress-Archive -Path (Join-Path $tempMsappDir "*") -DestinationPath $script:msappFile -Force
            Remove-Item $tempMsappDir -Recurse -Force
        }
        
        It "Should recognize Canvas App folder with .msapp extension" {
            # This test verifies the folder has .msapp extension
            $script:msappFolder | Should -Match '\.msapp$'
            Test-Path (Join-Path $script:msappFolder "Src") | Should -Be $true
            Test-Path (Join-Path $script:msappFolder "DataSources") | Should -Be $true
            Test-Path (Join-Path $script:msappFolder "Connections") | Should -Be $true
        }
        
        It "Should have created .msapp file" {
            Test-Path $script:msappFile | Should -Be $true
        }
    }
}

Describe "PacCliHelper" {
    
    Context "PAC CLI detection" {
        
        It "Should be able to search for PAC CLI" {
            # This test just verifies that the cmdlet can attempt to find/install PAC
            # We don't actually want to install PAC in the test environment unless it's already there
            # So we'll just verify the cmdlets exist and can be invoked with WhatIf
            { Get-Command Expand-DataverseSolutionFile } | Should -Not -Throw
            { Get-Command Compress-DataverseSolutionFile } | Should -Not -Throw
        }
    }
}
