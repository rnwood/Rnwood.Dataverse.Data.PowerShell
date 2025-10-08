. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetNextAutoNumberValue1 Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetNextAutoNumberValue1 SDK Cmdlet" {

        It "Invoke-DataverseGetNextAutoNumberValue1 executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetNextAutoNumberValue1Request", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetNextAutoNumberValue1"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetNextAutoNumberValue1Response" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetNextAutoNumberValue1 -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetNextAutoNumberValue1"
        }

    }
}
