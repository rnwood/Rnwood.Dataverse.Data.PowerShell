. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataversePublishDuplicateRule Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "PublishDuplicateRule SDK Cmdlet" {
        It "Invoke-DataversePublishDuplicateRule publishes a rule" {
            $ruleId = [Guid]::NewGuid()
            
            # Stub the response since FakeXrmEasy OSS doesn't support this
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.PublishDuplicateRuleRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.PublishDuplicateRuleResponse
                $response.Results["JobId"] = [Guid]::NewGuid()
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataversePublishDuplicateRule -Connection $script:conn -DuplicateRuleId $ruleId
            
            # Verify response
            $response | Should -Not -BeNull
            $response.GetType().Name | Should -Be "PublishDuplicateRuleResponse"
            $response.JobId | Should -Not -BeNullOrEmpty
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "PublishDuplicateRuleRequest"
            $proxy.LastRequest.DuplicateRuleId | Should -Be $ruleId
        }
    }
}
