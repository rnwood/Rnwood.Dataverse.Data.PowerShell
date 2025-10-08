. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseLocalTimeFromUtcTime Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "LocalTimeFromUtcTime SDK Cmdlet" {

        It "Invoke-DataverseLocalTimeFromUtcTime executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.LocalTimeFromUtcTimeRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "LocalTimeFromUtcTime"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.LocalTimeFromUtcTimeResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseLocalTimeFromUtcTime -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "LocalTimeFromUtcTime"
        }

    }
}
