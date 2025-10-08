. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseBackgroundSendEmail Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "BackgroundSendEmail SDK Cmdlet" {
        It "Invoke-DataverseBackgroundSendEmail queues email for background sending" {
            $emailId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.BackgroundSendEmailRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.BackgroundSendEmailRequest"
                $request.EntityId | Should -BeOfType [System.Guid]
                $request.EntityId | Should -Be $emailId
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.BackgroundSendEmailResponse
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseBackgroundSendEmail -Connection $script:conn -EntityId $emailId
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.BackgroundSendEmailResponse"
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.EntityId | Should -Be $emailId
        }
    }
}
