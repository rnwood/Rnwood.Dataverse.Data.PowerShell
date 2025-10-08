. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetTimeZoneCodeByLocalizedName Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetTimeZoneCodeByLocalizedName SDK Cmdlet" {

        It "Invoke-DataverseGetTimeZoneCodeByLocalizedName executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetTimeZoneCodeByLocalizedNameRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetTimeZoneCodeByLocalizedName"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetTimeZoneCodeByLocalizedNameResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetTimeZoneCodeByLocalizedName -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetTimeZoneCodeByLocalizedName"
        }

    }
}
