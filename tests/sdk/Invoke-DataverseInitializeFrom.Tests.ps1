. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseInitializeFrom Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "InitializeFrom SDK Cmdlet" {

        It "Invoke-DataverseInitializeFrom initializes a new record from an existing one" {
            $entityId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.InitializeFromRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.InitializeFromRequest"
                $request.EntityMoniker | Should -Not -BeNull
                $request.EntityMoniker | Should -BeOfType [Microsoft.Xrm.Sdk.EntityReference]
                $request.TargetEntityName | Should -BeOfType [System.String]
                $request.TargetFieldType | Should -BeOfType [Microsoft.Crm.Sdk.Messages.TargetFieldType]
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.InitializeFromResponse
                $entity = New-Object Microsoft.Xrm.Sdk.Entity("opportunity")
                $entity.Id = [Guid]::NewGuid()
                $response.Results["Entity"] = $entity
                return $response
            })
            
            # Call the cmdlet
            $entityMoniker = New-Object Microsoft.Xrm.Sdk.EntityReference("lead", $entityId)
            $targetFieldType = [Microsoft.Crm.Sdk.Messages.TargetFieldType]::All
            
            $response = Invoke-DataverseInitializeFrom -Connection $script:conn -EntityMoniker $entityMoniker -TargetEntityName "opportunity" -TargetFieldType $targetFieldType
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.InitializeFromResponse"
            $response.Entity | Should -Not -BeNull
            $response.Entity | Should -BeOfType [Microsoft.Xrm.Sdk.Entity]
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.EntityMoniker.Id | Should -Be $entityId
            $proxy.LastRequest.TargetEntityName | Should -Be "opportunity"
        }
    }

    }
}
