. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveMultiple Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveMultiple SDK Cmdlet" {

        It "Invoke-DataverseRetrieveMultiple with Entity parameter" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest"
                
                # Create response
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                return $response
            })
            
            # Create test entity
            $entity = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $entity.Id = [Guid]::NewGuid()
            $entity["firstname"] = "Test"
            
            # Call cmdlet
            $response = Invoke-DataverseRetrieveMultiple -Connection $script:conn -EntityId $entity.Id 
            
            # Verify response
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse"
            
            # Verify request via proxy
            $proxy.LastRequest.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest"
        }

    }
}
