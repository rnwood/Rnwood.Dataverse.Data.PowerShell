. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpsertMultiple Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UpsertMultiple SDK Cmdlet" {

        It "Invoke-DataverseUpsertMultiple executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UpsertMultipleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UpsertMultiple"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UpsertMultipleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUpsertMultiple -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UpsertMultiple"
        }

    }
}
