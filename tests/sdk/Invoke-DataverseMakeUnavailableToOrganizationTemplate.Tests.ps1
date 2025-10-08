. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseMakeUnavailableToOrganizationTemplate Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "MakeUnavailableToOrganizationTemplate SDK Cmdlet" {

        It "Invoke-DataverseMakeUnavailableToOrganizationTemplate executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.MakeUnavailableToOrganizationTemplateRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "MakeUnavailableToOrganizationTemplate"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.MakeUnavailableToOrganizationTemplateResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseMakeUnavailableToOrganizationTemplate -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "MakeUnavailableToOrganizationTemplate"
        }

    }
}
