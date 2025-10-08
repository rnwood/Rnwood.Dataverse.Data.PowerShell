. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveFilteredForms Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveFilteredForms SDK Cmdlet" {

        It "Invoke-DataverseRetrieveFilteredForms executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveFilteredFormsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveFilteredForms"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveFilteredFormsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveFilteredForms -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveFilteredForms"
        }

    }
}
