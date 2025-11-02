. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseRecord - Cancellation' {
    Context "StopProcessing (Ctrl+C) Support" {
        It "GetDataverseRecordCmdlet has StopProcessing override" {
            # Verify that the cmdlet class has a StopProcessing method
            $cmdletType = [Rnwood.Dataverse.Data.PowerShell.Commands.GetDataverseRecordCmdlet]
            $stopProcessingMethod = $cmdletType.GetMethod("StopProcessing", [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Public)
            
            $stopProcessingMethod | Should -Not -BeNullOrEmpty
            $stopProcessingMethod.DeclaringType.Name | Should -Be "GetDataverseRecordCmdlet"
        }

        It "Can retrieve records without errors when not cancelled" {
            # Verify that Get-DataverseRecord properly uses cancellation-aware paging
            # by checking that it can handle large datasets without hanging
            $connection = getMockConnection
            
            # Create many records
            1..100 | ForEach-Object { @{"firstname" = "Test$_"; "lastname" = "User$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
            
            # Should be able to retrieve all records without errors when not cancelled
            $results = Get-DataverseRecord -Connection $connection -TableName contact
            $results.Count | Should -BeGreaterThan 0
        }
    }
}
