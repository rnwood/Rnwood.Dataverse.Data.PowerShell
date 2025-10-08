. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCloseIncident Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CloseIncident SDK Cmdlet" {
        It "Invoke-DataverseCloseIncident closes an incident (case)" {
            $incidentId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CloseIncidentRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.CloseIncidentResponse
                return $response
            })
            
            # Create an incident resolution entity
            $resolution = New-Object Microsoft.Xrm.Sdk.Entity("incidentresolution")
            $resolution["subject"] = "Resolved"
            $resolution["incidentid"] = New-Object Microsoft.Xrm.Sdk.EntityReference("incident", $incidentId)
            
            # Call the cmdlet
            { Invoke-DataverseCloseIncident -Connection $script:conn -IncidentResolution $resolution -Status -1 } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "CloseIncidentRequest"
            $proxy.LastRequest.Status | Should -Be -1
        }
    }
}
