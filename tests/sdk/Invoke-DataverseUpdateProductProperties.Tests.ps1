. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpdateProductProperties Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UpdateProductProperties SDK Cmdlet" {

        It "Invoke-DataverseUpdateProductProperties executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UpdateProductPropertiesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UpdateProductProperties"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UpdateProductPropertiesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUpdateProductProperties -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UpdateProductProperties"
        }

    }
}
