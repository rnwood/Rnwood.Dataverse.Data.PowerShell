. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCreate Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Create SDK Cmdlet" {

        It "Invoke-DataverseCreate with Entity parameter" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.CreateRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.CreateRequest"
                
                # Create response
                $response = New-Object Microsoft.Xrm.Sdk.Messages.CreateResponse
                return $response
            })
            
            # Create test entity
            $entity = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $entity.Id = [Guid]::NewGuid()
            $entity["firstname"] = "Test"
            
            # Call cmdlet
            $response = Invoke-DataverseCreate -Connection $script:conn  -Target $entity
            
            # Verify response
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.CreateResponse"
            
            # Verify request via proxy
            $proxy.LastRequest.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.CreateRequest"
        }

    }
}
