. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseBulkDelete Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "BulkDelete SDK Cmdlet" {
        It "Invoke-DataverseBulkDelete accepts query and parameters" {
            # Create a query expression
            $query = New-Object Microsoft.Xrm.Sdk.Query.QueryExpression("contact")
            $query.Criteria.AddCondition("statecode", [Microsoft.Xrm.Sdk.Query.ConditionOperator]::Equal, 1)
            
            # Call the cmdlet
            $jobName = "Test Bulk Delete"
            $sendNotification = $false
            $toRecipients = @()
            $ccRecipients = @()
            $recurrencePattern = ""
            $startDateTime = [DateTime]::Now
            
            { Invoke-DataverseBulkDelete -Connection $script:conn -QuerySet @($query) -JobName $jobName -SendEmailNotification $sendNotification -ToRecipients $toRecipients -CCRecipients $ccRecipients -RecurrencePattern $recurrencePattern -StartDateTime $startDateTime } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "BulkDeleteRequest"
            $proxy.LastRequest.JobName | Should -Be $jobName
            $proxy.LastRequest.QuerySet.Count | Should -Be 1
            $proxy.LastRequest.QuerySet[0].EntityName | Should -Be "contact"
        }
    }
}
