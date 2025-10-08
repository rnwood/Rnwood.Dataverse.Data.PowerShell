. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSetParentSystemUser Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SetParentSystemUser SDK Cmdlet" {

        It "Invoke-DataverseSetParentSystemUser sets parent system user" {
            $userId = [Guid]::NewGuid()
            $parentId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SetParentSystemUserRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.SetParentSystemUserRequest"
                $request.UserId | Should -BeOfType [System.Guid]
                $request.ParentId | Should -BeOfType [System.Guid]
                $request.KeepChildUsers | Should -BeOfType [System.Boolean]
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.SetParentSystemUserResponse
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseSetParentSystemUser -Connection $script:conn -UserId $userId -ParentId $parentId -KeepChildUsers $true
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.SetParentSystemUserResponse"
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.UserId | Should -Be $userId
            $proxy.LastRequest.ParentId | Should -Be $parentId
            $proxy.LastRequest.KeepChildUsers | Should -Be $true
        }
    }

    }
}
