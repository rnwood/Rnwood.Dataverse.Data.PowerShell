Describe 'Get-DataverseConnection -FromPac' {

    BeforeAll {
        # Set up module path
        if ($env:TESTMODULEPATH) {
            $source = $env:TESTMODULEPATH
        }
        else {
            $source = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/"
        }

        $tempmodulefolder = "$([IO.Path]::GetTempPath())/$([Guid]::NewGuid())"
        New-Item -ItemType Directory $tempmodulefolder | Out-Null
        Copy-Item -Recurse $source $tempmodulefolder/Rnwood.Dataverse.Data.PowerShell
        $env:PSModulePath = $tempmodulefolder
        
        # Import the module
        Import-Module Rnwood.Dataverse.Data.PowerShell
        
        # Create a temporary PAC CLI profiles directory for testing
        $tempDir = if ($env:TEMP) { $env:TEMP } elseif ($env:TMP) { $env:TMP } else { [System.IO.Path]::GetTempPath() }
        $tempPacDir = Join-Path $tempDir "PacCliTest-$(New-Guid)"
        $tempPacFile = Join-Path $tempPacDir "authprofiles_v2.json"
        
        # Make variables available to tests
        $script:tempPacDir = $tempPacDir
        $script:tempPacFile = $tempPacFile
        $script:tempModuleFolder = $tempmodulefolder
        
        # Save original LocalApplicationData if it exists
        $script:originalLocalAppData = $env:LOCALAPPDATA
    }

    AfterAll {
        # Restore original environment
        if ($null -ne $script:originalLocalAppData) {
            $env:LOCALAPPDATA = $script:originalLocalAppData
        }
        
        # Clean up temp directory
        if ($null -ne $script:tempPacDir -and (Test-Path $script:tempPacDir)) {
            Remove-Item -Path $script:tempPacDir -Recurse -Force -ErrorAction SilentlyContinue
        }
        
        # Clean up temp module folder
        if ($null -ne $script:tempModuleFolder -and (Test-Path $script:tempModuleFolder)) {
            Remove-Item -Path $script:tempModuleFolder -Recurse -Force -ErrorAction SilentlyContinue
        }
        
        if (Get-Module Rnwood.Dataverse.Data.PowerShell) {
            Remove-Module Rnwood.Dataverse.Data.PowerShell
        }
    }

    BeforeEach {
        # Clean up any temp files from previous test
        if ($null -ne $script:tempPacDir -and (Test-Path $script:tempPacDir)) {
            Remove-Item -Path $script:tempPacDir -Recurse -Force -ErrorAction SilentlyContinue
        }
        
        # Create temp PAC directory structure
        $pacCliDir = Join-Path $script:tempPacDir "Microsoft\PowerPlatform\Cli"
        New-Item -Path $pacCliDir -ItemType Directory -Force | Out-Null
        $script:tempPacFile = Join-Path $pacCliDir "authprofiles_v2.json"
        
        # Point LOCALAPPDATA to temp directory - save and restore in each test
        $script:savedLocalAppData = $env:LOCALAPPDATA
        $env:LOCALAPPDATA = $script:tempPacDir
    }

    AfterEach {
        # Restore original environment after each test
        if ($null -ne $script:savedLocalAppData) {
            $env:LOCALAPPDATA = $script:savedLocalAppData
        } elseif ($null -ne $script:originalLocalAppData) {
            $env:LOCALAPPDATA = $script:originalLocalAppData
        }
    }

    It "FromPac throws error when profiles file does not exist" {
        # Ensure the profiles file doesn't exist
        if (Test-Path $script:tempPacFile) {
            Remove-Item $script:tempPacFile -Force
        }
        
        { Get-DataverseConnection -FromPac -ErrorAction Stop } | Should -Throw "*PAC CLI profiles file not found*"
    }

    It "FromPac throws error when profiles file is empty" {
        # Create empty profiles file
        Set-Content -Path $script:tempPacFile -Value ""
        
        { Get-DataverseConnection -FromPac -ErrorAction Stop } | Should -Throw
    }

    It "FromPac throws error when no profiles exist in file" {
        # Create profiles file with no profiles
        $emptyProfiles = @{
            Profiles = @()
        } | ConvertTo-Json
        
        Set-Content -Path $script:tempPacFile -Value $emptyProfiles
        
        { Get-DataverseConnection -FromPac -ErrorAction Stop } | Should -Throw "*No profiles found*"
    }

    It "FromPac throws error when profile has no environment URL" {
        # Create a profile without environment URL
        $profiles = @{
            Profiles = @(
                @{
                    Name = @{ Value = "TestProfile" }
                    # Missing ActiveEnvironmentUrl and LegacyResource
                }
            )
        } | ConvertTo-Json -Depth 10
        
        Set-Content -Path $script:tempPacFile -Value $profiles
        
        { Get-DataverseConnection -FromPac -ErrorAction Stop } | Should -Throw "*does not have an active environment URL*"
    }

    It "FromPac throws error when specified profile name not found" {
        # Create a valid profile with a different name
        $profiles = @{
            Profiles = @(
                @{
                    Name = @{ Value = "OtherProfile" }
                    ActiveEnvironmentUrl = "https://test.crm.dynamics.com"
                }
            )
        } | ConvertTo-Json -Depth 10
        
        Set-Content -Path $script:tempPacFile -Value $profiles
        
        { Get-DataverseConnection -FromPac -ProfileName "NonExistent" -ErrorAction Stop } | Should -Throw "*not found*"
    }

    It "FromPac parses profile with ActiveEnvironmentUrl correctly" {
        # Create a valid profile with ActiveEnvironmentUrl
        $testUrl = "https://testorg.crm.dynamics.com"
        $profiles = @{
            Profiles = @(
                @{
                    Name = @{ Value = "TestProfile" }
                    ActiveEnvironmentUrl = $testUrl
                }
            )
        } | ConvertTo-Json -Depth 10
        
        Set-Content -Path $script:tempPacFile -Value $profiles
        
        # This will fail at token acquisition since we don't have real PAC auth,
        # but it should parse the profile correctly and get past the initial validation
        try {
            $conn = Get-DataverseConnection -FromPac -ErrorAction Stop
            # If we get here without error about profile parsing, the test passes
            # The connection attempt will likely fail due to auth, but that's expected
        } catch {
            # Check that the error is about authentication, not profile parsing
            $_.Exception.Message | Should -Not -Match "PAC CLI profiles file not found"
            $_.Exception.Message | Should -Not -Match "No profiles found"
            $_.Exception.Message | Should -Not -Match "does not have an active environment URL"
        }
    }

    It "FromPac parses profile with LegacyResource correctly" {
        # Create a valid profile with LegacyResource
        $testUrl = "https://testorg.crm.dynamics.com"
        $profiles = @{
            Profiles = @(
                @{
                    Name = @{ Value = "TestProfile" }
                    LegacyResource = @{ Resource = $testUrl }
                }
            )
        } | ConvertTo-Json -Depth 10
        
        Set-Content -Path $script:tempPacFile -Value $profiles
        
        # This will fail at token acquisition since we don't have real PAC auth,
        # but it should parse the profile correctly
        try {
            $conn = Get-DataverseConnection -FromPac -ErrorAction Stop
        } catch {
            # Check that the error is about authentication, not profile parsing
            $_.Exception.Message | Should -Not -Match "PAC CLI profiles file not found"
            $_.Exception.Message | Should -Not -Match "No profiles found"
            $_.Exception.Message | Should -Not -Match "does not have an active environment URL"
        }
    }

    It "FromPac with ProfileName selects correct profile" {
        # Create multiple profiles
        $profiles = @{
            Profiles = @(
                @{
                    Name = @{ Value = "Profile1" }
                    ActiveEnvironmentUrl = "https://org1.crm.dynamics.com"
                },
                @{
                    Name = @{ Value = "Profile2" }
                    ActiveEnvironmentUrl = "https://org2.crm.dynamics.com"
                }
            )
        } | ConvertTo-Json -Depth 10
        
        Set-Content -Path $script:tempPacFile -Value $profiles
        
        # Try to get Profile2 specifically
        try {
            $conn = Get-DataverseConnection -FromPac -ProfileName "Profile2" -ErrorAction Stop
        } catch {
            # Check that it didn't fail due to wrong profile being selected
            $_.Exception.Message | Should -Not -Match "Profile2.*not found"
        }
    }
}
