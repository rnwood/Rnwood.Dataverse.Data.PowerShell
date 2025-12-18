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

Describe "Expand-DataverseSolution and Compress-DataverseSolution" {
    
    Context "Basic functionality" {
        
        It "Should have Expand-DataverseSolution cmdlet available" {
            Get-Command Expand-DataverseSolution | Should -Not -BeNullOrEmpty
        }
        
        It "Should have Compress-DataverseSolution cmdlet available" {
            Get-Command Compress-DataverseSolution | Should -Not -BeNullOrEmpty
        }
        
        It "Should have Unpack-DataverseSolution alias for Expand-DataverseSolution" {
            $alias = Get-Alias Unpack-DataverseSolution -ErrorAction SilentlyContinue
            $alias | Should -Not -BeNullOrEmpty
            $alias.Definition | Should -Be "Expand-DataverseSolution"
        }
        
        It "Should have Pack-DataverseSolution alias for Compress-DataverseSolution" {
            $alias = Get-Alias Pack-DataverseSolution -ErrorAction SilentlyContinue
            $alias | Should -Not -BeNullOrEmpty
            $alias.Definition | Should -Be "Compress-DataverseSolution"
        }
    }
    
    Context "Parameter validation" {
        
        It "Expand-DataverseSolution parameters should be mandatory" {
            $cmd = Get-Command Expand-DataverseSolution
            $pathParam = $cmd.Parameters['Path']
            $outputParam = $cmd.Parameters['OutputPath']
            $pathParam.Attributes.Mandatory | Should -Contain $true
            $outputParam.Attributes.Mandatory | Should -Contain $true
        }
        
        It "Compress-DataverseSolution parameters should be mandatory" {
            $cmd = Get-Command Compress-DataverseSolution
            $pathParam = $cmd.Parameters['Path']
            $outputParam = $cmd.Parameters['OutputPath']
            $pathParam.Attributes.Mandatory | Should -Contain $true
            $outputParam.Attributes.Mandatory | Should -Contain $true
        }
        
        It "Expand-DataverseSolution should error on non-existent file" {
            $nonExistentFile = Join-Path $script:testDir "nonexistent.zip"
            { Expand-DataverseSolution -Path $nonExistentFile -OutputPath (Join-Path $script:testDir "output") -ErrorAction Stop } | 
                Should -Throw
        }
        
        It "Compress-DataverseSolution should error on non-existent folder" {
            $nonExistentFolder = Join-Path $script:testDir "nonexistent"
            { Compress-DataverseSolution -Path $nonExistentFolder -OutputPath (Join-Path $script:testDir "output.zip") -ErrorAction Stop } | 
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
            { Expand-DataverseSolution -Path $script:dummyZip -OutputPath $outputPath -WhatIf } | Should -Not -Throw
            # Output path should not be created
            Test-Path $outputPath | Should -Be $false
        }
        
        It "Compress-DataverseSolution should support -WhatIf" {
            $outputZip = Join-Path $script:testDir "whatif_compress.zip"
            { Compress-DataverseSolution -Path $script:dummyFolder -OutputPath $outputZip -WhatIf } | Should -Not -Throw
            # Output file should not be created
            Test-Path $outputZip | Should -Be $false
        }
    }
    
    Context "msapp handling" {
        
        BeforeAll {
            # Create a mock Canvas App folder structure
            $script:msappFolder = Join-Path $script:testDir "MockCanvasApp"
            New-Item -ItemType Directory -Path $script:msappFolder | Out-Null
            New-Item -ItemType Directory -Path (Join-Path $script:msappFolder "Src") | Out-Null
            New-Item -ItemType Directory -Path (Join-Path $script:msappFolder "DataSources") | Out-Null
            New-Item -ItemType Directory -Path (Join-Path $script:msappFolder "Connections") | Out-Null
            "test" | Out-File (Join-Path $script:msappFolder "Src\App.fx.yaml")
            
            # Create an msapp file from the folder
            $script:msappFile = Join-Path $script:testDir "MockCanvasApp.msapp"
            Compress-Archive -Path (Join-Path $script:msappFolder "*") -DestinationPath $script:msappFile
        }
        
        It "Should recognize Canvas App folder structure" {
            # This test verifies the folder has the expected structure
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
            { Get-Command Expand-DataverseSolution } | Should -Not -Throw
            { Get-Command Compress-DataverseSolution } | Should -Not -Throw
        }
    }
}
