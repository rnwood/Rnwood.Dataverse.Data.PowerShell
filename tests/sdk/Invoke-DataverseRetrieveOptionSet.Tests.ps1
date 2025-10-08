. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveOptionSet Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveOptionSet SDK Cmdlet" {

        It "Invoke-DataverseRetrieveOptionSet retrieves option set metadata" {
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveOptionSetRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.RetrieveOptionSetRequest"
                $request.Name | Should -BeOfType [System.String]
                $request.Name | Should -Be "testoptionset"
                
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveOptionSetResponse
                $optionSetMetadata = New-Object Microsoft.Xrm.Sdk.Metadata.OptionSetMetadata
                $optionSetMetadata.GetType().GetProperty("Name").SetValue($optionSetMetadata, "testoptionset")
                $response.Results["OptionSetMetadata"] = $optionSetMetadata
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseRetrieveOptionSet -Connection $script:conn -Name "testoptionset"
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.RetrieveOptionSetResponse"
            $response.OptionSetMetadata | Should -Not -BeNull
            $response.OptionSetMetadata.Name | Should -Be "testoptionset"
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.Name | Should -Be "testoptionset"
        }
    }

    }
}
