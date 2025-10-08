. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpdateSolutionComponent Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UpdateSolutionComponent SDK Cmdlet" {

        It "Invoke-DataverseUpdateSolutionComponent executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UpdateSolutionComponentRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UpdateSolutionComponent"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UpdateSolutionComponentResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUpdateSolutionComponent -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UpdateSolutionComponent"
        }

    }
}
