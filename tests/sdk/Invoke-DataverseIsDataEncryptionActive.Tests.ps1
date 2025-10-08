. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseIsDataEncryptionActive Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "IsDataEncryptionActive SDK Cmdlet" {

        It "Invoke-DataverseIsDataEncryptionActive executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.IsDataEncryptionActiveRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "IsDataEncryptionActive"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.IsDataEncryptionActiveResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseIsDataEncryptionActive -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "IsDataEncryptionActive"
        }

    }
}
