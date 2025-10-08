. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseMakeAvailableToOrganizationTemplate Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "MakeAvailableToOrganizationTemplate SDK Cmdlet" {

        It "Invoke-DataverseMakeAvailableToOrganizationTemplate executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.MakeAvailableToOrganizationTemplateRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "MakeAvailableToOrganizationTemplate"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.MakeAvailableToOrganizationTemplateResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseMakeAvailableToOrganizationTemplate -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "MakeAvailableToOrganizationTemplate"
        }

    }
}
