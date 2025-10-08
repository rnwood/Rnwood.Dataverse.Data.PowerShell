. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetAutoNumberSeed1 Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetAutoNumberSeed1 SDK Cmdlet" {

        It "Invoke-DataverseGetAutoNumberSeed1 executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetAutoNumberSeed1Request", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetAutoNumberSeed1"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetAutoNumberSeed1Response" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetAutoNumberSeed1 -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetAutoNumberSeed1"
        }

    }
}
