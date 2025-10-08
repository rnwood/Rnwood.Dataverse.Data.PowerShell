. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataversePreferredSolutionUsedBy Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "PreferredSolutionUsedBy SDK Cmdlet" {

        It "Invoke-DataversePreferredSolutionUsedBy executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.PreferredSolutionUsedByRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "PreferredSolutionUsedBy"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.PreferredSolutionUsedByResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataversePreferredSolutionUsedBy -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "PreferredSolutionUsedBy"
        }

    }
}
