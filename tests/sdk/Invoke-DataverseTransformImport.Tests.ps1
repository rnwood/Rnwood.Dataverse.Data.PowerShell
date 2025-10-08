. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseTransformImport Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "TransformImport SDK Cmdlet" {

        It "Invoke-DataverseTransformImport executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.TransformImportRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "TransformImport"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.TransformImportResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseTransformImport -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "TransformImport"
        }

    }
}
