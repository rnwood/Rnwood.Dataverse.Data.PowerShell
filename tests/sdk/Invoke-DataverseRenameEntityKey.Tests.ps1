. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRenameEntityKey Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RenameEntityKey SDK Cmdlet" {

        It "Invoke-DataverseRenameEntityKey executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RenameEntityKeyRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RenameEntityKey"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RenameEntityKeyResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRenameEntityKey -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RenameEntityKey"
        }

    }
}
