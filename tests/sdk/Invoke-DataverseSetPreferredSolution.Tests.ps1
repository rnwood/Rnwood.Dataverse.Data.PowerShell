. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSetPreferredSolution Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SetPreferredSolution SDK Cmdlet" {

        It "Invoke-DataverseSetPreferredSolution executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SetPreferredSolutionRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SetPreferredSolution"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SetPreferredSolutionResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSetPreferredSolution -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SetPreferredSolution"
        }

    }
}
