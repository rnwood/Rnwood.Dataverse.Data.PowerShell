Describe "XrmToolbox Plugin" {
    BeforeAll {
        $projectPath = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin/Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.csproj"
    }

    It "Plugin project file exists" {
        Test-Path $projectPath | Should -Be $true
    }

    It "Plugin project builds successfully in Release mode" {
        $result = dotnet build $projectPath -c Release 2>&1
        $LASTEXITCODE | Should -Be 0
    }

    It "Plugin DLL is created after build" {
        $dllPath = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin/bin/Release/net48/Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.dll"
        Test-Path $dllPath | Should -Be $true
    }

    It "Plugin has required dependencies" {
        $binPath = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin/bin/Release/net48/"
        
        # Check for XrmToolbox dependencies
        Test-Path "$binPath/XrmToolBox.Extensibility.dll" -ErrorAction SilentlyContinue | Should -Be $true -Because "XrmToolbox extensibility is required"
    }

    It "Plugin README exists" {
        Test-Path "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin/README.md" | Should -Be $true
    }

    It "Plugin TESTING documentation exists" {
        Test-Path "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin/TESTING.md" | Should -Be $true
    }
    
    It "PowerShellCompletionService class is created" {
        $servicePath = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin/PowerShellCompletionService.cs"
        Test-Path $servicePath | Should -Be $true
    }
}

Describe "PowerShell Completion Service" {
    It "PowerShell 5.1+ is available for completion service" {
        # The service uses powershell.exe which should be available on Windows
        # On Linux/Mac, this test will be skipped as the service is Windows-only
        if ($IsWindows -or $null -eq $IsWindows) {
            $psExe = Get-Command powershell.exe -ErrorAction SilentlyContinue
            $psExe | Should -Not -BeNullOrEmpty -Because "PowerShellCompletionService requires powershell.exe"
        }
        else {
            Set-ItResult -Skipped -Because "PowerShell completion service is Windows-only"
        }
    }
    
    It "TabExpansion2 API is available in PowerShell" {
        Get-Command TabExpansion2 | Should -Not -BeNullOrEmpty -Because "Completion service uses TabExpansion2"
    }
    
    It "TabExpansion2 returns completions for simple commands" {
        $result = TabExpansion2 'Get-D' 5
        $result | Should -Not -BeNullOrEmpty
        $result.CompletionMatches | Should -Not -BeNullOrEmpty
        $result.CompletionMatches[0].CompletionText | Should -Match 'Get-'
    }
    
    It "TabExpansion2 returns parameter completions" {
        $result = TabExpansion2 'Get-Date -' 10
        $result | Should -Not -BeNullOrEmpty
        $result.CompletionMatches | Should -Not -BeNullOrEmpty
        # Should have parameters like -Year, -Month, etc.
        ($result.CompletionMatches | Where-Object { $_.CompletionText -like '-*' }).Count | Should -BeGreaterThan 0
    }
}
