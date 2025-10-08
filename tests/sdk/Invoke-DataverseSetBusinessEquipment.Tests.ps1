. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSetBusinessEquipment Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SetBusinessEquipment SDK Cmdlet" {

        It "Invoke-DataverseSetBusinessEquipment executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SetBusinessEquipmentRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SetBusinessEquipment"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SetBusinessEquipmentResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSetBusinessEquipment -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SetBusinessEquipment"
        }

    }
}
