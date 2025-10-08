. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseInstantiateTemplate Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "InstantiateTemplate SDK Cmdlet" {

        It "Invoke-DataverseInstantiateTemplate executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.InstantiateTemplateRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "InstantiateTemplate"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.InstantiateTemplateResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseInstantiateTemplate -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "InstantiateTemplate"
        }

    }
}
