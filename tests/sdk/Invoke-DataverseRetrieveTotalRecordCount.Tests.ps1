. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveTotalRecordCount Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveTotalRecordCount SDK Cmdlet" {

        It "Invoke-DataverseRetrieveTotalRecordCount executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveTotalRecordCountRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveTotalRecordCount"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveTotalRecordCountResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveTotalRecordCount -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveTotalRecordCount"
        }

    }
}
