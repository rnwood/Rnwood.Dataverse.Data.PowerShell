. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetNextAutoNumberValue Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetNextAutoNumberValue SDK Cmdlet" {

        It "Invoke-DataverseGetNextAutoNumberValue executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetNextAutoNumberValueRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetNextAutoNumberValue"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetNextAutoNumberValueResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetNextAutoNumberValue -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetNextAutoNumberValue"
        }

    }
}
