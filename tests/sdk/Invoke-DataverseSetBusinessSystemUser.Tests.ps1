. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSetBusinessSystemUser Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SetBusinessSystemUser SDK Cmdlet" {

        It "Invoke-DataverseSetBusinessSystemUser executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SetBusinessSystemUserRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SetBusinessSystemUser"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SetBusinessSystemUserResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSetBusinessSystemUser -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SetBusinessSystemUser"
        }

    }
}
