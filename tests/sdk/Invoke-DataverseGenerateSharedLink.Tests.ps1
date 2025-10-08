. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGenerateSharedLink Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GenerateSharedLink SDK Cmdlet" {

        It "Invoke-DataverseGenerateSharedLink executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GenerateSharedLinkRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GenerateSharedLink"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GenerateSharedLinkResponse" -as [Type]
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
            
            $response = Invoke-DataverseGenerateSharedLink -Connection $script:conn -Target $target -TargetTableName "contact" -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GenerateSharedLink"
        }

    }
}
