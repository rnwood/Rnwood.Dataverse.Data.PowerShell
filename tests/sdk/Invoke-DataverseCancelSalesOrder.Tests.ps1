. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCancelSalesOrder Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CancelSalesOrder SDK Cmdlet" {

        It "Invoke-DataverseCancelSalesOrder executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CancelSalesOrderRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CancelSalesOrderRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CancelSalesOrderResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCancelSalesOrder -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CancelSalesOrderRequest"
        }

    }
}
