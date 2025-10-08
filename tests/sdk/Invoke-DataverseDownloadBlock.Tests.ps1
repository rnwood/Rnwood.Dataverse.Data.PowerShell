. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDownloadBlock Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DownloadBlock SDK Cmdlet" {

        It "Invoke-DataverseDownloadBlock executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DownloadBlockRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "DownloadBlock"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.DownloadBlockResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseDownloadBlock -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DownloadBlock"
        }

    }
}
