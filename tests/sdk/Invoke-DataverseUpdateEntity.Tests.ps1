. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpdateEntity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UpdateEntity SDK Cmdlet" {

        It "Invoke-DataverseUpdateEntity executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UpdateEntityRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UpdateEntity"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UpdateEntityResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUpdateEntity -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UpdateEntity"
        }

    }
}
