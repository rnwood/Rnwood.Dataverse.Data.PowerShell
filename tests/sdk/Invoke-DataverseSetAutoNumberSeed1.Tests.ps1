. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSetAutoNumberSeed1 Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SetAutoNumberSeed1 SDK Cmdlet" {

        It "Invoke-DataverseSetAutoNumberSeed1 executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SetAutoNumberSeed1Request", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SetAutoNumberSeed1"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SetAutoNumberSeed1Response" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSetAutoNumberSeed1 -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SetAutoNumberSeed1"
        }

    }
}
