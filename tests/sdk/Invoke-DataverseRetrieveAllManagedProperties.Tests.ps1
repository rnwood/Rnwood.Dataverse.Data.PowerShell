. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveAllManagedProperties Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveAllManagedProperties SDK Cmdlet" {

        It "Invoke-DataverseRetrieveAllManagedProperties executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveAllManagedPropertiesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveAllManagedProperties"
                
                $responseType = "Microsoft.Xrm.Sdk.Messages.RetrieveAllManagedPropertiesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveAllManagedProperties -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveAllManagedProperties"
        }

    }
}
