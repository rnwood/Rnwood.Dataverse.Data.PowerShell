. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveUnpublishedMultiple Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveUnpublishedMultiple SDK Cmdlet" {

        It "Invoke-DataverseRetrieveUnpublishedMultiple executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveUnpublishedMultiple"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveUnpublishedMultiple -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveUnpublishedMultiple"
        }

    }
}
