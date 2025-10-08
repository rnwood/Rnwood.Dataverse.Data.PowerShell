. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetDecryptionKey Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetDecryptionKey SDK Cmdlet" {

        It "Invoke-DataverseGetDecryptionKey executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetDecryptionKeyRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetDecryptionKey"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetDecryptionKeyResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetDecryptionKey -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetDecryptionKey"
        }

    }
}
