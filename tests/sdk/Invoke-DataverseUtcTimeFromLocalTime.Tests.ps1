. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUtcTimeFromLocalTime Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UtcTimeFromLocalTime SDK Cmdlet" {

        It "Invoke-DataverseUtcTimeFromLocalTime executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UtcTimeFromLocalTimeRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UtcTimeFromLocalTime"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UtcTimeFromLocalTimeResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUtcTimeFromLocalTime -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UtcTimeFromLocalTime"
        }

    }
}
