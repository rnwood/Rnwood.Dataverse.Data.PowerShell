. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpdateAutoNumberSeed1 Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UpdateAutoNumberSeed1 SDK Cmdlet" {

        It "Invoke-DataverseUpdateAutoNumberSeed1 executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UpdateAutoNumberSeed1Request", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UpdateAutoNumberSeed1"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UpdateAutoNumberSeed1Response" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUpdateAutoNumberSeed1 -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UpdateAutoNumberSeed1"
        }

    }
}
