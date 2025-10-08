. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCopyMembersList Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CopyMembersList SDK Cmdlet" {

        It "Invoke-DataverseCopyMembersList executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CopyMembersListRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CopyMembersListRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CopyMembersListResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCopyMembersList -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CopyMembersListRequest"
        }

    }
}
