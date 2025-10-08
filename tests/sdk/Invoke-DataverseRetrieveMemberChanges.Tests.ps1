. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveMemberChanges Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveMemberChanges SDK Cmdlet" {

        It "Invoke-DataverseRetrieveMemberChanges executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveMemberChangesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveMemberChanges"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveMemberChangesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveMemberChanges -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveMemberChanges"
        }

    }
}
