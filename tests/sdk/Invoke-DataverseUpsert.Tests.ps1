. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpsert Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Upsert SDK Cmdlet" {

        It "Invoke-DataverseUpsert executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UpsertRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "Upsert"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UpsertResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Create contact record for Target parameter (using contact entity in mock metadata)
            $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact | Set-DataverseRecord -Connection $script:conn -CreateOnly
            
            $target = [PSCustomObject]@{ contactid = $contactId }
            
            $response = Invoke-DataverseUpsert -Connection $script:conn -Target $target -TargetTableName "contact" -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "Upsert"
        }

    }
}
