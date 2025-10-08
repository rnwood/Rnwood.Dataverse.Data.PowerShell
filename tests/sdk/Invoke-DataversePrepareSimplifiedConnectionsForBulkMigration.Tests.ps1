. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataversePrepareSimplifiedConnectionsForBulkMigration Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "PrepareSimplifiedConnectionsForBulkMigration SDK Cmdlet" {

        It "Invoke-DataversePrepareSimplifiedConnectionsForBulkMigration executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.PrepareSimplifiedConnectionsForBulkMigrationRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "PrepareSimplifiedConnectionsForBulkMigration"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.PrepareSimplifiedConnectionsForBulkMigrationResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataversePrepareSimplifiedConnectionsForBulkMigration -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "PrepareSimplifiedConnectionsForBulkMigration"
        }

    }
}
