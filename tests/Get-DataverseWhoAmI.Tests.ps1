. $PSScriptRoot/Common.ps1

Describe "Get-DataverseWhoAmI examples" {

    It "Can execute WhoAmI request" {
    $connection = getMockConnection -Entities 'systemuser'
        $whoami = Get-DataverseWhoAmI -Connection $connection
        $whoami | Should -Not -BeNull
        $whoami.UserId | Should -Not -BeNullOrEmpty
    }

    It "Can invoke WhoAmI to get current user (user/team examples)" {
    $connection = getMockConnection -Entities 'systemuser'
        $whoami = Get-DataverseWhoAmI -Connection $connection
        $whoami | Should -Not -BeNull
        $whoami.BusinessUnitId | Should -Not -BeNullOrEmpty
        $whoami.OrganizationId | Should -Not -BeNullOrEmpty
    }
}
