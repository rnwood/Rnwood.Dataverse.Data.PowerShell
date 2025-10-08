. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseQualifyLead Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "QualifyLead SDK Cmdlet" {
        It "Invoke-DataverseQualifyLead qualifies a lead" {
            $leadId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.QualifyLeadRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.QualifyLeadRequest"
                $request.LeadId | Should -Not -BeNull
                $request.LeadId | Should -BeOfType [Microsoft.Xrm.Sdk.EntityReference]
                $request.CreateAccount | Should -BeOfType [System.Boolean]
                $request.CreateContact | Should -BeOfType [System.Boolean]
                $request.CreateOpportunity | Should -BeOfType [System.Boolean]
                $request.Status | Should -BeOfType [Microsoft.Xrm.Sdk.OptionSetValue]
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.QualifyLeadResponse
                $createdEntities = New-Object Microsoft.Xrm.Sdk.EntityReferenceCollection
                $createdEntities.Add((New-Object Microsoft.Xrm.Sdk.EntityReference("opportunity", [Guid]::NewGuid())))
                $response.Results["CreatedEntities"] = $createdEntities
                return $response
            })
            
            # Call the cmdlet
            $leadRef = New-Object Microsoft.Xrm.Sdk.EntityReference("lead", $leadId)
            $status = New-Object Microsoft.Xrm.Sdk.OptionSetValue(3)
            
            $response = Invoke-DataverseQualifyLead -Connection $script:conn -LeadId $leadRef -CreateAccount $false -CreateContact $false -CreateOpportunity $true -Status $status
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.QualifyLeadResponse"
            $response.CreatedEntities | Should -Not -BeNull
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.LeadId.Id | Should -Be $leadId
            $proxy.LastRequest.CreateOpportunity | Should -Be $true
        }
    }
}
