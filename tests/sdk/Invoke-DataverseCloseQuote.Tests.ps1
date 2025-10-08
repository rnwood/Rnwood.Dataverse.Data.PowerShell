. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCloseQuote Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CloseQuote SDK Cmdlet" {
        It "Invoke-DataverseCloseQuote closes a quote" {
            $quoteId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CloseQuoteRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.CloseQuoteRequest"
                $request.QuoteClose | Should -Not -BeNull
                $request.QuoteClose | Should -BeOfType [Microsoft.Xrm.Sdk.Entity]
                $request.Status | Should -BeOfType [Microsoft.Xrm.Sdk.OptionSetValue]
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.CloseQuoteResponse
                return $response
            })
            
            # Call the cmdlet
            $quoteClose = New-Object Microsoft.Xrm.Sdk.Entity("quoteclose")
            $quoteClose["quoteid"] = New-Object Microsoft.Xrm.Sdk.EntityReference("quote", $quoteId)
            $status = New-Object Microsoft.Xrm.Sdk.OptionSetValue(3)
            
            $response = Invoke-DataverseCloseQuote -Connection $script:conn -QuoteClose $quoteClose -Status $status
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.CloseQuoteResponse"
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.Status.Value | Should -Be 3
        }
    }
}
