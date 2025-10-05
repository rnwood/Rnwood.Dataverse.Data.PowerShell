. $PSScriptRoot/Common.ps1

Describe "Get-DataverseConnection examples" {

    It "Can create a mock connection for testing" {
    $conn = getMockConnection -Entities 'contact'
        $conn | Should -Not -BeNull
    }

    It "Connection example pattern is valid" {
    $conn = getMockConnection -Entities 'contact'
        $conn | Should -Not -BeNull
        $conn.GetType().Name | Should -Be "ServiceClient"
    }
}
