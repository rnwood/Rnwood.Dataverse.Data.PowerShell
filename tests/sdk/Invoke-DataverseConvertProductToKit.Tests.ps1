. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseConvertProductToKit Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ConvertProductToKit SDK Cmdlet" {

        It "Invoke-DataverseConvertProductToKit executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ConvertProductToKitRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "ConvertProductToKitRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.ConvertProductToKitResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseConvertProductToKit -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ConvertProductToKitRequest"
        }

    }
}
