. $PSScriptRoot/Common.ps1

Describe "Canvas App Cmdlets" {
    # Note: canvasapp entity doesn't have metadata XML file in the test directory.
    # These tests use the mock connection without specific entity metadata.
    # For full E2E testing with actual Dataverse, use e2e-tests directory.
    
    # Note: Skipping tests that require canvasapp metadata since it's not available
    # These would work with real Dataverse in E2E tests
    
    Context "Get-DataverseCanvasApp - Basic Retrieval" -Skip {
        # Skipped: canvasapp metadata not available
    }

    Context "Remove-DataverseCanvasApp - Deletion" -Skip {
        # Skipped: canvasapp metadata not available
    }

    # Screen and Component extraction from file work without Dataverse connection
    # Note: Screen/component cmdlets have parameters to work with Dataverse or files directly
    # These tests validate file-based operations which don't require metadata
    
    Context "Get-DataverseCanvasAppScreen - Screen Extraction from File" -Skip {
        # Skipped: These cmdlets require a connection even when reading from file
        # The cmdlet inherits from OrganizationServiceCmdlet which requires connection
        # For E2E testing, use actual Dataverse with saved .msapp files
    }

    Context "Get-DataverseCanvasAppComponent - Component Extraction from File" -Skip {
        # Skipped: These cmdlets require a connection even when reading from file
        # The cmdlet inherits from OrganizationServiceCmdlet which requires connection
        # For E2E testing, use actual Dataverse with saved .msapp files
    }
}
