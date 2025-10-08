. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveManagedProperty Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveManagedProperty SDK Cmdlet" {

        It "Invoke-DataverseRetrieveManagedProperty executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveManagedPropertyRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveManagedProperty"
                
                $responseType = "Microsoft.Xrm.Sdk.Messages.RetrieveManagedPropertyResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveManagedProperty -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveManagedProperty"
        }

    }
}
