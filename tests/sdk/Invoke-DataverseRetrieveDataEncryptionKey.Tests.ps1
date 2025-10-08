. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveDataEncryptionKey Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveDataEncryptionKey SDK Cmdlet" {

        It "Invoke-DataverseRetrieveDataEncryptionKey executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveDataEncryptionKeyRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveDataEncryptionKey"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveDataEncryptionKeyResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveDataEncryptionKey -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveDataEncryptionKey"
        }

    }
}
