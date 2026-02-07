. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseAppModuleComponent' {
    BeforeEach {
        # Load the required entities for these tests
        $connection = getMockConnection -Entities @("appmodule", "appmodulecomponent")
    }
}