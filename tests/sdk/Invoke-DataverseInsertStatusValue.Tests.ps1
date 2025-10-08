. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseInsertStatusValue Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "InsertStatusValue SDK Cmdlet" {

        It "Invoke-DataverseInsertStatusValue executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.InsertStatusValueRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "InsertStatusValue"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.InsertStatusValueResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseInsertStatusValue -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "InsertStatusValue"
        }

    }
}
