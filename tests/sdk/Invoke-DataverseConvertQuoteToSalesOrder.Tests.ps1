. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseConvertQuoteToSalesOrder Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ConvertQuoteToSalesOrder SDK Cmdlet" {
        It "Invoke-DataverseConvertQuoteToSalesOrder converts a quote to sales order" {
            $quoteId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ConvertQuoteToSalesOrderRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.ConvertQuoteToSalesOrderRequest"
                $request.QuoteId | Should -BeOfType [System.Guid]
                $request.ColumnSet | Should -Not -BeNull
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.ConvertQuoteToSalesOrderResponse
                $salesOrder = New-Object Microsoft.Xrm.Sdk.Entity("salesorder")
                $salesOrder.Id = [Guid]::NewGuid()
                $response.Results["Entity"] = $salesOrder
                return $response
            })
            
            # Call the cmdlet
            $columnSet = New-Object Microsoft.Xrm.Sdk.Query.ColumnSet($true)
            $response = Invoke-DataverseConvertQuoteToSalesOrder -Connection $script:conn -QuoteId $quoteId -ColumnSet $columnSet
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.ConvertQuoteToSalesOrderResponse"
            $response.Entity | Should -Not -BeNull
            $response.Entity | Should -BeOfType [Microsoft.Xrm.Sdk.Entity]
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.QuoteId | Should -Be $quoteId
        }
    }
}
