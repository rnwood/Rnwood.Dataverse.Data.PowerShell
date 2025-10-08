. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataversePublishTheme Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "PublishTheme SDK Cmdlet" {

        It "Invoke-DataversePublishTheme executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.PublishThemeRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "PublishTheme"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.PublishThemeResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataversePublishTheme -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "PublishTheme"
        }

    }
}
