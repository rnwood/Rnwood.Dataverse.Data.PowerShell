. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetPreferredSolution Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetPreferredSolution SDK Cmdlet" {

        It "Invoke-DataverseGetPreferredSolution executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetPreferredSolutionRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetPreferredSolution"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetPreferredSolutionResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetPreferredSolution -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetPreferredSolution"
        }

    }
}
