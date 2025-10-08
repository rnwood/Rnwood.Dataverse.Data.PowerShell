. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpdateOptionSet Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UpdateOptionSet SDK Cmdlet" {

        It "Invoke-DataverseUpdateOptionSet executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UpdateOptionSetRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UpdateOptionSet"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UpdateOptionSetResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUpdateOptionSet -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UpdateOptionSet"
        }

    }
}
