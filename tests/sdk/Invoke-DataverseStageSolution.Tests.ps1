. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseStageSolution Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "StageSolution SDK Cmdlet" {

        It "Invoke-DataverseStageSolution executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.StageSolutionRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "StageSolution"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.StageSolutionResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseStageSolution -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "StageSolution"
        }

    }
}
