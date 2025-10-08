. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRescheduleRequest Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RescheduleRequest SDK Cmdlet" {

        It "Invoke-DataverseRescheduleRequest executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RescheduleRequestRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RescheduleRequest"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RescheduleRequestResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRescheduleRequest -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RescheduleRequest"
        }

    }
}
