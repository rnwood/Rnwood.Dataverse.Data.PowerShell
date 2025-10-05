. $PSScriptRoot/../Common.ps1

Describe "Specialized Cmdlets (documentation)" {

    It "Specialized WhoAmI cmdlet exists and works" {
    $connection = getMockConnection -Entities 'systemuser'
        $cmdlet = Get-Command Invoke-DataverseWhoAmI -ErrorAction SilentlyContinue
        $cmdlet | Should -Not -BeNull
        $response = Invoke-DataverseWhoAmI -Connection $connection
        $response | Should -Not -BeNull
    }

    It "Solution management specialized cmdlets exist" {
        Get-Command Invoke-DataverseExportSolution -ErrorAction SilentlyContinue | Should -Not -BeNull
        Get-Command Invoke-DataverseImportSolution -ErrorAction SilentlyContinue | Should -Not -BeNull
    }

    It "User/Team specialized cmdlets exist" {
        Get-Command Invoke-DataverseAddMembersTeam -ErrorAction SilentlyContinue | Should -Not -BeNull
    }

    It "Marketing list specialized cmdlets exist" {
        Get-Command Invoke-DataverseAddMemberList -ErrorAction SilentlyContinue | Should -Not -BeNull
        Get-Command Invoke-DataverseRemoveMemberList -ErrorAction SilentlyContinue | Should -Not -BeNull
    }

    It "Duplicate detection specialized cmdlets exist" {
        Get-Command Invoke-DataversePublishDuplicateRule -ErrorAction SilentlyContinue | Should -Not -BeNull
        Get-Command Invoke-DataverseUnpublishDuplicateRule -ErrorAction SilentlyContinue | Should -Not -BeNull
        Get-Command Invoke-DataverseBulkDetectDuplicates -ErrorAction SilentlyContinue | Should -Not -BeNull
    }

    It "Ribbon specialized cmdlets exist" {
        Get-Command Invoke-DataverseRetrieveApplicationRibbon -ErrorAction SilentlyContinue | Should -Not -BeNull
        Get-Command Invoke-DataverseRetrieveEntityRibbon -ErrorAction SilentlyContinue | Should -Not -BeNull
    }

    It "CloseIncident specialized cmdlet exists" {
        Get-Command Invoke-DataverseCloseIncident -ErrorAction SilentlyContinue | Should -Not -BeNull
    }
}
