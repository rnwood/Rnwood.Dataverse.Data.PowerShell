. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCalculatePrice Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CalculatePrice SDK Cmdlet" {

        It "Invoke-DataverseCalculatePrice executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CalculatePriceRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CalculatePriceRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CalculatePriceResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Create a test target using contact (CalculatePrice normally uses opportunity/quote/order/invoice)
            $target = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", [Guid]::NewGuid())
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCalculatePrice -Connection $script:conn -Target $target -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CalculatePriceRequest"
        }

    }
}
