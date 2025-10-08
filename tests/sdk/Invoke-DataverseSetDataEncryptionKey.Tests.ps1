. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSetDataEncryptionKey Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SetDataEncryptionKey SDK Cmdlet" {

        It "Invoke-DataverseSetDataEncryptionKey executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SetDataEncryptionKeyRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SetDataEncryptionKey"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SetDataEncryptionKeyResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSetDataEncryptionKey -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SetDataEncryptionKey"
        }

    }
}
