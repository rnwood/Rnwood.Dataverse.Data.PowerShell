. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCreateActivitiesList Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CreateActivitiesList SDK Cmdlet" {

        It "Invoke-DataverseCreateActivitiesList executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CreateActivitiesListRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CreateActivitiesListRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CreateActivitiesListResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCreateActivitiesList -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CreateActivitiesListRequest"
        }

    }
}
