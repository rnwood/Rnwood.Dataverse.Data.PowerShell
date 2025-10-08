. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGenerateSalesOrderFromOpportunity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GenerateSalesOrderFromOpportunity SDK Cmdlet" {

        It "Invoke-DataverseGenerateSalesOrderFromOpportunity executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GenerateSalesOrderFromOpportunityRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GenerateSalesOrderFromOpportunity"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GenerateSalesOrderFromOpportunityResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGenerateSalesOrderFromOpportunity -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GenerateSalesOrderFromOpportunity"
        }

    }
}
