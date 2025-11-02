. $PSScriptRoot/Common.ps1

Describe 'Performance Investigation - Mock Connection' {
    It "Measures time breakdown for getMockConnection" {
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        
        Write-Host "Starting performance test at $($sw.Elapsed.TotalSeconds)s"
        
        # Measure just the getMockConnection call
        $swMock = [System.Diagnostics.Stopwatch]::StartNew()
        $connection = getMockConnection
        $swMock.Stop()
        Write-Host "getMockConnection completed in $($swMock.Elapsed.TotalSeconds)s"
        
        # Measure a simple operation
        $swOp = [System.Diagnostics.Stopwatch]::StartNew()
        $in = New-Object Microsoft.Xrm.Sdk.Entity "contact"
        $in["contactid"] = [Guid]::NewGuid()
        $in["firstname"] = "TestPerf"
        $swOp.Stop()
        Write-Host "Creating test entity completed in $($swOp.Elapsed.TotalSeconds)s"
        
        # Measure Set-DataverseRecord
        $swSet = [System.Diagnostics.Stopwatch]::StartNew()
        $in | Set-DataverseRecord -Connection $connection
        $swSet.Stop()
        Write-Host "Set-DataverseRecord completed in $($swSet.Elapsed.TotalSeconds)s"
        
        # Measure Get-DataverseRecord
        $swGet = [System.Diagnostics.Stopwatch]::StartNew()
        $result = Get-DataverseRecord -Connection $connection -TableName contact
        $swGet.Stop()
        Write-Host "Get-DataverseRecord completed in $($swGet.Elapsed.TotalSeconds)s"
        
        $sw.Stop()
        Write-Host "Total test time: $($sw.Elapsed.TotalSeconds)s"
        Write-Host "Breakdown:"
        Write-Host "  getMockConnection: $($swMock.Elapsed.TotalSeconds)s ($([math]::Round($swMock.Elapsed.TotalSeconds/$sw.Elapsed.TotalSeconds*100, 1))%)"
        Write-Host "  Create entity:     $($swOp.Elapsed.TotalSeconds)s ($([math]::Round($swOp.Elapsed.TotalSeconds/$sw.Elapsed.TotalSeconds*100, 1))%)"
        Write-Host "  Set-DataverseRecord: $($swSet.Elapsed.TotalSeconds)s ($([math]::Round($swSet.Elapsed.TotalSeconds/$sw.Elapsed.TotalSeconds*100, 1))%)"
        Write-Host "  Get-DataverseRecord: $($swGet.Elapsed.TotalSeconds)s ($([math]::Round($swGet.Elapsed.TotalSeconds/$sw.Elapsed.TotalSeconds*100, 1))%)"
        
        $result | Should -Not -BeNullOrEmpty
        $result.firstname | Should -Be "TestPerf"
    }
    
    It "Measures time for second call to getMockConnection (should be fast)" {
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $connection = getMockConnection
        $sw.Stop()
        Write-Host "Second getMockConnection call completed in $($sw.Elapsed.TotalSeconds)s"
        
        $connection | Should -Not -BeNullOrEmpty
    }
}
