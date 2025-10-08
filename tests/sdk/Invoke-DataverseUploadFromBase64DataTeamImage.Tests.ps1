. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUploadFromBase64DataTeamImage Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UploadFromBase64DataTeamImage SDK Cmdlet" {

        It "Invoke-DataverseUploadFromBase64DataTeamImage executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UploadFromBase64DataTeamImageRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UploadFromBase64DataTeamImage"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UploadFromBase64DataTeamImageResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUploadFromBase64DataTeamImage -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UploadFromBase64DataTeamImage"
        }

    }
}
