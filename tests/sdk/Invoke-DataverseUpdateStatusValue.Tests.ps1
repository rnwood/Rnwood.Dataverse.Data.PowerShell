. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpdateStatusValue Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UpdateStatusValue SDK Cmdlet" {

        It "Invoke-DataverseUpdateStatusValue executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UpdateStatusValueRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UpdateStatusValue"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UpdateStatusValueResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUpdateStatusValue -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UpdateStatusValue"
        }

    }
}
