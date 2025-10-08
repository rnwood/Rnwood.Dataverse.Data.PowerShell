. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetTrackingTokenEmail Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetTrackingTokenEmail SDK Cmdlet" {

        It "Invoke-DataverseGetTrackingTokenEmail executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetTrackingTokenEmailRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetTrackingTokenEmail"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetTrackingTokenEmailResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetTrackingTokenEmail -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetTrackingTokenEmail"
        }

    }
}
