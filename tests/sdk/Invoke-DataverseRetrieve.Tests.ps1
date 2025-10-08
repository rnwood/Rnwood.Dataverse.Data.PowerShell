. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieve Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Retrieve SDK Cmdlet" {

        It "Invoke-DataverseRetrieve with Entity parameter" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.RetrieveRequest"
                
                # Create response
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveResponse
                return $response
            })
            
            # Create test entity
            $entity = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $entity.Id = [Guid]::NewGuid()
            $entity["firstname"] = "Test"
            
            # Call cmdlet
            $response = Invoke-DataverseRetrieve -Connection $script:conn -EntityId $entity.Id 
            
            # Verify response
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.RetrieveResponse"
            
            # Verify request via proxy
            $proxy.LastRequest.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.RetrieveRequest"
        }

    }
}
