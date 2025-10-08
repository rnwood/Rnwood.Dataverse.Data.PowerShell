. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGenerateSocialProfile Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GenerateSocialProfile SDK Cmdlet" {

        It "Invoke-DataverseGenerateSocialProfile executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GenerateSocialProfileRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GenerateSocialProfile"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GenerateSocialProfileResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGenerateSocialProfile -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GenerateSocialProfile"
        }

    }
}
