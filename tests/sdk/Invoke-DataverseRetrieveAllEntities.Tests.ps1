. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveAllEntities Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveAllEntitiesRequest SDK Cmdlet" {

        It "Invoke-DataverseRetrieveAllEntities executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveAllEntitiesRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "RetrieveAllEntitiesRequest"
                
                # Create response
                $responseType = "Microsoft.Xrm.Sdk.Messages.RetrieveAllEntitiesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseRetrieveAllEntities -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveAllEntitiesRequest"
        }

    }
}
