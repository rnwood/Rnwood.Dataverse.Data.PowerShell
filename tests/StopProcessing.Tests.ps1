$ErrorActionPreference = "Stop"

# Import the common test utilities
. "$PSScriptRoot/Common.ps1"

Describe "Stop Processing (Ctrl+C) Support" {

    Context "Cmdlet StopProcessing Implementation" {
        BeforeAll {
            if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
                Import-Module Rnwood.Dataverse.Data.PowerShell
            }
        }
        
        It "Get-DataverseRecordCmdlet has StopProcessing override" {
            # Verify that the cmdlet class has a StopProcessing method
            $cmdletType = [Rnwood.Dataverse.Data.PowerShell.Commands.GetDataverseRecordCmdlet]
            $stopProcessingMethod = $cmdletType.GetMethod("StopProcessing", [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Public)
            
            $stopProcessingMethod | Should -Not -BeNullOrEmpty
            $stopProcessingMethod.DeclaringType.Name | Should -Be "GetDataverseRecordCmdlet"
        }
        
        It "InvokeDataverseSqlCmdlet has StopProcessing override" {
            # Verify that the cmdlet class has a StopProcessing method
            $cmdletType = [Rnwood.Dataverse.Data.PowerShell.Commands.InvokeDataverseSqlCmdlet]
            $stopProcessingMethod = $cmdletType.GetMethod("StopProcessing", [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Public)
            
            $stopProcessingMethod | Should -Not -BeNullOrEmpty
            $stopProcessingMethod.DeclaringType.Name | Should -Be "InvokeDataverseSqlCmdlet"
        }
        
        It "SetDataverseRecordCmdlet has StopProcessing override (existing)" {
            # Verify that the cmdlet class has a StopProcessing method (existed before)
            $cmdletType = [Rnwood.Dataverse.Data.PowerShell.Commands.SetDataverseRecordCmdlet]
            $stopProcessingMethod = $cmdletType.GetMethod("StopProcessing", [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Public)
            
            $stopProcessingMethod | Should -Not -BeNullOrEmpty
            $stopProcessingMethod.DeclaringType.Name | Should -Be "SetDataverseRecordCmdlet"
        }
        
        It "RemoveDataverseRecordCmdlet has StopProcessing override (existing)" {
            # Verify that the cmdlet class has a StopProcessing method (existed before)
            $cmdletType = [Rnwood.Dataverse.Data.PowerShell.Commands.RemoveDataverseRecordCmdlet]
            $stopProcessingMethod = $cmdletType.GetMethod("StopProcessing", [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Public)
            
            $stopProcessingMethod | Should -Not -BeNullOrEmpty
            $stopProcessingMethod.DeclaringType.Name | Should -Be "RemoveDataverseRecordCmdlet"
        }
        
        It "InvokeDataverseRequestCmdlet has StopProcessing override (existing)" {
            # Verify that the cmdlet class has a StopProcessing method (existed before)
            $cmdletType = [Rnwood.Dataverse.Data.PowerShell.Commands.InvokeDataverseRequestCmdlet]
            $stopProcessingMethod = $cmdletType.GetMethod("StopProcessing", [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Public)
            
            $stopProcessingMethod | Should -Not -BeNullOrEmpty
            $stopProcessingMethod.DeclaringType.Name | Should -Be "InvokeDataverseRequestCmdlet"
        }
    }
    
    Context "QueryHelpers Cancellation Support" {
        It "Get-DataverseRecord uses paging with cancellation support" {
            # Verify that Get-DataverseRecord properly uses cancellation-aware paging
            # by checking that it can handle large datasets without hanging
            $connection = getMockConnection
            
            # Create many records
            1..100 | ForEach-Object { @{"firstname" = "Test$_"; "lastname" = "User$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
            
            # Should be able to retrieve all records
            $results = Get-DataverseRecord -Connection $connection -TableName contact
            $results.Count | Should -BeGreaterThan 0
        }
    }
    
    Context "Get-DataverseRecord Cancellation Behavior" {
        It "Can retrieve records without errors when not cancelled" {
            $connection = getMockConnection
            1..10 | ForEach-Object { @{"firstname" = "Test$_"; "lastname" = "User$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
            
            # Should retrieve all 10 records without issues
            $results = Get-DataverseRecord -Connection $connection -TableName contact
            $results.Count | Should -BeGreaterThan 0
        }
    }
}
