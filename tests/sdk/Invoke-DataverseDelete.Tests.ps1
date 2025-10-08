. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDelete Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Delete SDK Cmdlet" {

        It "Invoke-DataverseDelete with Entity parameter" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.DeleteRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.DeleteRequest"
                
                # Create response
                $response = New-Object Microsoft.Xrm.Sdk.Messages.DeleteResponse
                return $response
            })
            
            # Create test entity
            $entity = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $entity.Id = [Guid]::NewGuid()
            $entity["firstname"] = "Test"
            
            # Call cmdlet
            $response = Invoke-DataverseDelete -Connection $script:conn -EntityId $entity.Id -Target $entity
            
            # Verify response
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.DeleteResponse"
            
            # Verify request via proxy
            $proxy.LastRequest.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.DeleteRequest"
        }

    }
}
