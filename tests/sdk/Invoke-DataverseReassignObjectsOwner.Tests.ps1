. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseReassignObjectsOwner Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ReassignObjectsOwner SDK Cmdlet" {

        It "Invoke-DataverseReassignObjectsOwner executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ReassignObjectsOwnerRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ReassignObjectsOwner"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ReassignObjectsOwnerResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseReassignObjectsOwner -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ReassignObjectsOwner"
        }

    }
}
