. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseValidate Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Validate SDK Cmdlet" {

        It "Invoke-DataverseValidate executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ValidateRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "Validate"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ValidateResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseValidate -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "Validate"
        }

    }
}
