. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseQuerySchedule Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "QuerySchedule SDK Cmdlet" {

        It "Invoke-DataverseQuerySchedule executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.QueryScheduleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "QuerySchedule"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.QueryScheduleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseQuerySchedule -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "QuerySchedule"
        }

    }
}
