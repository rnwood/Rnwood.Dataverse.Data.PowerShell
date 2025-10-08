. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseInsertOptionValue Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "InsertOptionValue SDK Cmdlet" {

        It "Invoke-DataverseInsertOptionValue executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.InsertOptionValueRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "InsertOptionValue"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.InsertOptionValueResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseInsertOptionValue -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "InsertOptionValue"
        }

    }
}
