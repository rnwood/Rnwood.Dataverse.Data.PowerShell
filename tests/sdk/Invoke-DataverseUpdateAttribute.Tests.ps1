. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpdateAttribute Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UpdateAttribute SDK Cmdlet" {

        It "Invoke-DataverseUpdateAttribute executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UpdateAttributeRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UpdateAttribute"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UpdateAttributeResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUpdateAttribute -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UpdateAttribute"
        }

    }
}
