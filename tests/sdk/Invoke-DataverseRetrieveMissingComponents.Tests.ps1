. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveMissingComponents Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveMissingComponents SDK Cmdlet" {

        It "Invoke-DataverseRetrieveMissingComponents executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveMissingComponentsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveMissingComponents"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveMissingComponentsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveMissingComponents -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveMissingComponents"
        }

    }
}
