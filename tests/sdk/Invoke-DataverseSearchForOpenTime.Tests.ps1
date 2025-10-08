. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSearchForOpenTime Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SearchForOpenTime SDK Cmdlet" {

        It "Invoke-DataverseSearchForOpenTime executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SearchForOpenTimeRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SearchForOpenTime"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SearchForOpenTimeResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSearchForOpenTime -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SearchForOpenTime"
        }

    }
}
