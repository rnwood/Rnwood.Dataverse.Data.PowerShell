. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpdate Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Update SDK Cmdlet" {

        It "Invoke-DataverseUpdate with Entity parameter" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.UpdateRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.UpdateRequest"
                
                # Create response
                $response = New-Object Microsoft.Xrm.Sdk.Messages.UpdateResponse
                return $response
            })
            
            # Create test entity
            $entity = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $entity.Id = [Guid]::NewGuid()
            $entity["firstname"] = "Test"
            
            # Call cmdlet
            $response = Invoke-DataverseUpdate -Connection $script:conn -EntityId $entity.Id -Target $entity
            
            # Verify response
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.UpdateResponse"
            
            # Verify request via proxy
            $proxy.LastRequest.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.UpdateRequest"
        }

    }
}
