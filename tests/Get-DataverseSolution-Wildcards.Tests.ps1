. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseSolution - Wildcard Support' {
    
    BeforeAll {
        # Ensure module is loaded
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop
        }
    }

    It 'Should use Equal operator for exact name (no wildcards)' {
        # Track query operator used
        $global:capturedOperator = $null
        $global:capturedPattern = $null
        
        $mockConnection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                $query = $request.Query
                
                # Capture query conditions for verification
                $condition = $query.Criteria.Conditions | Where-Object { $_.AttributeName -eq 'uniquename' }
                $global:capturedOperator = $condition.Operator
                $global:capturedPattern = $condition.Values[0]
                
                # Return empty collection (we only care about query construction)
                $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                $response.Results.Add('EntityCollection', $entityCollection)
                return $response
            }
        } -Entities @('solution')
        
        # Execute query
        $result = Get-DataverseSolution -Connection $mockConnection -UniqueName 'TestSolution'
        
        # Verify correct operator was used
        $global:capturedOperator | Should -Be 'Equal'
        $global:capturedPattern | Should -Be 'TestSolution'
    }
    It 'Should use Like operator with % for wildcard pattern with asterisk' {
        # Track query operator used
        $global:capturedOperator = $null
        $global:capturedPattern = $null
        
        $mockConnection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                $query = $request.Query
                
                # Capture query conditions for verification
                $condition = $query.Criteria.Conditions | Where-Object { $_.AttributeName -eq 'uniquename' }
                $global:capturedOperator = $condition.Operator
                $global:capturedPattern = $condition.Values[0]
                
                # Return empty collection (we only care about query construction)
                $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                $response.Results.Add('EntityCollection', $entityCollection)
                return $response
            }
        } -Entities @('solution')
        
        # Execute query
        $result = Get-DataverseSolution -Connection $mockConnection -UniqueName 'Contoso*'
        
        # Verify correct operator and pattern conversion (* -> %)
        $global:capturedOperator | Should -Be 'Like'
        $global:capturedPattern | Should -Be 'Contoso%'
    }

    It 'Should use Like operator with _ for wildcard pattern with question mark' {
        # Track query operator used
        $global:capturedOperator = $null
        $global:capturedPattern = $null
        
        $mockConnection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                $query = $request.Query
                
                # Capture query conditions for verification
                $condition = $query.Criteria.Conditions | Where-Object { $_.AttributeName -eq 'uniquename' }
                $global:capturedOperator = $condition.Operator
                $global:capturedPattern = $condition.Values[0]
                
                # Return empty collection (we only care about query construction)
                $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                $response.Results.Add('EntityCollection', $entityCollection)
                return $response
            }
        } -Entities @('solution')
        
        # Execute query
        $result = Get-DataverseSolution -Connection $mockConnection -UniqueName 'Test?Solution'
        
        # Verify correct operator and pattern conversion (? -> _)
        $global:capturedOperator | Should -Be 'Like'
        $global:capturedPattern | Should -Be 'Test_Solution'
    }

    It 'Should use Like operator with both % and _ for combined wildcards' {
        # Track query operator used
        $global:capturedOperator = $null
        $global:capturedPattern = $null
        
        $mockConnection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                $query = $request.Query
                
                # Capture query conditions for verification
                $condition = $query.Criteria.Conditions | Where-Object { $_.AttributeName -eq 'uniquename' }
                $global:capturedOperator = $condition.Operator
                $global:capturedPattern = $condition.Values[0]
                
                # Return empty collection (we only care about query construction)
                $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                $response.Results.Add('EntityCollection', $entityCollection)
                return $response
            }
        } -Entities @('solution')
        
        # Execute query
        $result = Get-DataverseSolution -Connection $mockConnection -UniqueName '*Custom?'
        
        # Verify correct operator and pattern conversion (* -> %, ? -> _)
        $global:capturedOperator | Should -Be 'Like'
        $global:capturedPattern | Should -Be '%Custom_'
    }

    It 'Should combine wildcard filter with managed filter' {
        # Track query conditions used
        $global:capturedNameOperator = $null
        $global:capturedNamePattern = $null
        $global:capturedManagedOperator = $null
        $global:capturedManagedValue = $null
        
        $mockConnection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                $query = $request.Query
                
                # Capture query conditions for verification
                $nameCondition = $query.Criteria.Conditions | Where-Object { $_.AttributeName -eq 'uniquename' }
                $global:capturedNameOperator = $nameCondition.Operator
                $global:capturedNamePattern = $nameCondition.Values[0]
                
                $managedCondition = $query.Criteria.Conditions | Where-Object { $_.AttributeName -eq 'ismanaged' }
                $global:capturedManagedOperator = $managedCondition.Operator
                $global:capturedManagedValue = $managedCondition.Values[0]
                
                # Return empty collection (we only care about query construction)
                $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                $response.Results.Add('EntityCollection', $entityCollection)
                return $response
            }
        } -Entities @('solution')
        
        # Execute query
        $result = Get-DataverseSolution -Connection $mockConnection -UniqueName 'Partner*' -Managed
        
        # Verify correct operators and values
        $global:capturedNameOperator | Should -Be 'Like'
        $global:capturedNamePattern | Should -Be 'Partner%'
        $global:capturedManagedOperator | Should -Be 'Equal'
        $global:capturedManagedValue | Should -Be $true
    }
}

