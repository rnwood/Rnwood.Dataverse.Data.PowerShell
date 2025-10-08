. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDisassociate Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Disassociate SDK Cmdlet" {
        It "Invoke-DataverseDisassociate removes a many-to-many relationship" {
            $contact1Id = [Guid]::NewGuid()
            $contact2Id = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.DisassociateRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.DisassociateRequest"
                $request.Target | Should -Not -BeNull
                $request.Target | Should -BeOfType [Microsoft.Xrm.Sdk.EntityReference]
                $request.Relationship | Should -Not -BeNull
                
                $response = New-Object Microsoft.Xrm.Sdk.Messages.DisassociateResponse
                return $response
            })
            
            # Call the cmdlet
            $target = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", $contact1Id)
            $relationship = New-Object Microsoft.Xrm.Sdk.Relationship("contact_contact")
            $relatedEntities = New-Object Microsoft.Xrm.Sdk.EntityReferenceCollection
            $relatedEntities.Add((New-Object Microsoft.Xrm.Sdk.EntityReference("contact", $contact2Id)))
            
            $response = Invoke-DataverseDisassociate -Connection $script:conn -Target $target -Relationship $relationship -RelatedEntities $relatedEntities
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.DisassociateResponse"
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.Target.Id | Should -Be $contact1Id
        }
    }
}
