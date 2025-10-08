. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseConvertKitToProduct Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ConvertKitToProduct SDK Cmdlet" {

        It "Invoke-DataverseConvertKitToProduct executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ConvertKitToProductRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "ConvertKitToProductRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.ConvertKitToProductResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseConvertKitToProduct -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ConvertKitToProductRequest"
        }

    }
}
