. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseReassignObjectsSystemUser Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ReassignObjectsSystemUser SDK Cmdlet" {

        It "Invoke-DataverseReassignObjectsSystemUser executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ReassignObjectsSystemUserRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ReassignObjectsSystemUser"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ReassignObjectsSystemUserResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseReassignObjectsSystemUser -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ReassignObjectsSystemUser"
        }

    }
}
