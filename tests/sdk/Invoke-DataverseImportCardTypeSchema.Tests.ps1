. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseImportCardTypeSchema Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ImportCardTypeSchema SDK Cmdlet" {

        It "Invoke-DataverseImportCardTypeSchema executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ImportCardTypeSchemaRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ImportCardTypeSchema"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ImportCardTypeSchemaResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseImportCardTypeSchema -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ImportCardTypeSchema"
        }

    }
}
