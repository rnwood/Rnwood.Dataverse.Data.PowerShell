. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetAllTimeZonesWithDisplayName Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetAllTimeZonesWithDisplayName SDK Cmdlet" {

        It "Invoke-DataverseGetAllTimeZonesWithDisplayName executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetAllTimeZonesWithDisplayNameRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetAllTimeZonesWithDisplayName"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetAllTimeZonesWithDisplayNameResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetAllTimeZonesWithDisplayName -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetAllTimeZonesWithDisplayName"
        }

    }
}
