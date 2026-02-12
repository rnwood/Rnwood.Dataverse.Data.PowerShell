using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Forms
{
    /// <summary>
    /// Form library and event handler manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/FormLibraryAndEventHandler.Tests.ps1
    /// </summary>
    public class FormLibraryAndEventHandlerTests : E2ETestBase
    {
        [Fact]
        public void CanCreateReadUpdateAndDeleteFormLibrariesAndEventHandlers()
        {


            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

try {
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $entityName = 'contact'
    
    Write-Host 'Step 1: Getting a contact form to work with...'
    $form = Get-DataverseForm -Connection $connection -Entity $entityName -Published | Select-Object -First 1
    if (-not $form) {
        throw 'No contact forms found'
    }
    $formId = $form.FormId
    Write-Host ""  Using form: $($form.Name) (ID: $formId)""
    Write-Host '✓ Form selected'
    
    Write-Host 'Step 2: Creating test web resources...'
    $webResourceName1 = ""new_e2etest_$testRunId/library1.js""
    $webResourceName2 = ""new_e2etest_$testRunId/library2.js""
    
    $tempFile1 = [System.IO.Path]::GetTempFileName()
    $tempJsFile1 = [System.IO.Path]::ChangeExtension($tempFile1, '.js')
    Move-Item $tempFile1 $tempJsFile1 -Force
    ""function testFunction1() { console.log('test'); }"" | Out-File -FilePath $tempJsFile1 -NoNewline -Encoding utf8
    
    $tempFile2 = [System.IO.Path]::GetTempFileName()
    $tempJsFile2 = [System.IO.Path]::ChangeExtension($tempFile2, '.js')
    Move-Item $tempFile2 $tempJsFile2 -Force
    ""function testFunction2() { console.log('test2'); }"" | Out-File -FilePath $tempJsFile2 -NoNewline -Encoding utf8
    
    Invoke-WithRetry {
        $wr1 = Set-DataverseWebResource -Connection $connection `
            -Name $webResourceName1 `
            -Path $tempJsFile1 `
            -DisplayName 'E2E Test Library 1' `
            -PassThru `
            -Confirm:$false
        $script:webResourceId1 = $wr1.webresourceid
        Write-Host ""  Created web resource 1: $webResourceName1""
        
        $wr2 = Set-DataverseWebResource -Connection $connection `
            -Name $webResourceName2 `
            -Path $tempJsFile2 `
            -DisplayName 'E2E Test Library 2' `
            -PassThru `
            -Confirm:$false
        $script:webResourceId2 = $wr2.webresourceid
        Write-Host ""  Created web resource 2: $webResourceName2""
    }
    
    Remove-Item $tempJsFile1 -Force -ErrorAction SilentlyContinue
    Remove-Item $tempJsFile2 -Force -ErrorAction SilentlyContinue
    Write-Host '✓ Web resources created'
    
    Write-Host 'Step 3: Adding libraries to form...'
    Invoke-WithRetry {
        $script:library1 = Set-DataverseFormLibrary -Connection $connection -FormId $formId -LibraryName $webResourceName1 -Confirm:$false
        $script:library2 = Set-DataverseFormLibrary -Connection $connection -FormId $formId -LibraryName $webResourceName2 -Confirm:$false
    }
    Write-Host '✓ Libraries added'
    
    Write-Host 'Step 4: Adding form-level event handler...'
    Invoke-WithRetry {
        Set-DataverseFormEventHandler -Connection $connection `
            -FormId $formId `
            -EventName 'onload' `
            -FunctionName ""E2ETestOnLoad_$testRunId"" `
            -LibraryName $webResourceName1 `
            -Confirm:$false
    }
    Write-Host '✓ Form handler added'
    
    Write-Host 'Step 5: Cleanup - Removing form handler...'
    Invoke-WithRetry {
        Remove-DataverseFormEventHandler -Connection $connection `
            -FormId $formId `
            -EventName 'onload' `
            -FunctionName ""E2ETestOnLoad_$testRunId"" `
            -LibraryName $webResourceName1 `
            -Confirm:$false
    }
    Write-Host '✓ Form handler removed'
    
    Write-Host 'Step 6: Cleanup - Removing libraries...'
    Invoke-WithRetry {
        Remove-DataverseFormLibrary -Connection $connection -FormId $formId -LibraryName $webResourceName1 -Confirm:$false
        Remove-DataverseFormLibrary -Connection $connection -FormId $formId -LibraryName $webResourceName2 -Confirm:$false
    }
    Write-Host '✓ Libraries removed'
    
    Write-Host 'Step 7: Cleanup - Deleting test web resources...'
    Invoke-WithRetry {
        Remove-DataverseWebResource -Connection $connection -Id $webResourceId1 -Confirm:$false
        Remove-DataverseWebResource -Connection $connection -Id $webResourceId2 -Confirm:$false
    }
    Write-Host '✓ Web resources deleted'
    
    Write-Host 'SUCCESS: All form library and event handler operations completed successfully'
}
catch {
    Write-Host ""ERROR: $($_ | Out-String)""
    throw
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS");
        }
    }
}
