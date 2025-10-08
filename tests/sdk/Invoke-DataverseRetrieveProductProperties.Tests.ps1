. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveProductProperties Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveProductProperties SDK Cmdlet" {

        It "Invoke-DataverseRetrieveProductProperties executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveProductPropertiesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveProductProperties"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveProductPropertiesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveProductProperties -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveProductProperties"
        }

    }
}
