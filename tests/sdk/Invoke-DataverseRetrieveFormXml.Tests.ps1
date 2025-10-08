. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveFormXml Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveFormXml SDK Cmdlet" {

        It "Invoke-DataverseRetrieveFormXml executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveFormXmlRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveFormXml"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveFormXmlResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveFormXml -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveFormXml"
        }

    }
}
