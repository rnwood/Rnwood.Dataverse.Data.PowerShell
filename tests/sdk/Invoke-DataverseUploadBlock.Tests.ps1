. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUploadBlock Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UploadBlock SDK Cmdlet" {

        It "Invoke-DataverseUploadBlock executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UploadBlockRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UploadBlock"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UploadBlockResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUploadBlock -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UploadBlock"
        }

    }
}
