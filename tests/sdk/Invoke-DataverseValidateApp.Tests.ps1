. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseValidateApp Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ValidateApp SDK Cmdlet" {

        It "Invoke-DataverseValidateApp executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ValidateAppRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ValidateApp"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ValidateAppResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseValidateApp -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ValidateApp"
        }

    }
}
