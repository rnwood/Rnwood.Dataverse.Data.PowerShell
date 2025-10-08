. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseConvertSalesOrderToInvoice Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ConvertSalesOrderToInvoice SDK Cmdlet" {

        It "Invoke-DataverseConvertSalesOrderToInvoice executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ConvertSalesOrderToInvoiceRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "ConvertSalesOrderToInvoiceRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.ConvertSalesOrderToInvoiceResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseConvertSalesOrderToInvoice -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ConvertSalesOrderToInvoiceRequest"
        }

    }
}
