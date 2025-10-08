. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetValidReferencedEntities Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetValidReferencedEntities SDK Cmdlet" {

        It "Invoke-DataverseGetValidReferencedEntities executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetValidReferencedEntitiesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetValidReferencedEntities"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetValidReferencedEntitiesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetValidReferencedEntities -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetValidReferencedEntities"
        }

    }
}
