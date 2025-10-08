. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseBook Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Book SDK Cmdlet" {

        It "Invoke-DataverseBook executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.BookRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "BookRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.BookResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Create a contact record first (exists in mock metadata)
            $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact | Set-DataverseRecord -Connection $script:conn -CreateOnly
            
            # Create a test target PSObject (Book would normally use appointment, but contact works for testing)
            $target = [PSCustomObject]@{
                contactid = $contactId
            }
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseBook -Connection $script:conn -Target $target -TargetTableName "contact" -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "BookRequest"
        }

    }
}
