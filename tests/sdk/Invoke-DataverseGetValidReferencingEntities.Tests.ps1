. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetValidReferencingEntities Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetValidReferencingEntities SDK Cmdlet" {

        It "Invoke-DataverseGetValidReferencingEntities executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetValidReferencingEntitiesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetValidReferencingEntities"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetValidReferencingEntitiesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetValidReferencingEntities -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetValidReferencingEntities"
        }

    }
}
