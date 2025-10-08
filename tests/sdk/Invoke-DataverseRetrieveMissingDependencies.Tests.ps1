. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveMissingDependencies Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveMissingDependencies SDK Cmdlet" {

        It "Invoke-DataverseRetrieveMissingDependencies executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveMissingDependenciesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveMissingDependencies"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveMissingDependenciesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveMissingDependencies -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveMissingDependencies"
        }

    }
}
