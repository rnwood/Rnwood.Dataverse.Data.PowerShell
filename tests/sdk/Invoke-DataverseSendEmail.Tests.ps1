. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSendEmail Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SendEmail SDK Cmdlet" {
        It "Invoke-DataverseSendEmail sends an email" {
            $emailId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SendEmailRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.SendEmailRequest"
                $request.EmailId | Should -BeOfType [System.Guid]
                $request.IssueSend | Should -BeOfType [System.Boolean]
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.SendEmailResponse
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseSendEmail -Connection $script:conn -EmailId $emailId -IssueSend $true -TrackingToken ""
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.SendEmailResponse"
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.EmailId | Should -Be $emailId
            $proxy.LastRequest.IssueSend | Should -Be $true
        }
    }
}
