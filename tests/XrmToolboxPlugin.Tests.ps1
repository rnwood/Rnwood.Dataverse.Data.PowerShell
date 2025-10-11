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
}
