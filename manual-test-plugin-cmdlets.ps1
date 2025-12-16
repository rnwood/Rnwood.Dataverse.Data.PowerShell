#!/usr/bin/env pwsh
# Manual test script for Set-DataverseDynamicPluginAssembly and Get-DataverseDynamicPluginAssembly

$ErrorActionPreference = "Stop"

# Import the module
Import-Module ./Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/Rnwood.Dataverse.Data.PowerShell.psd1 -Force

Write-Host "=== Plugin Assembly Cmdlets Manual Test ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check cmdlets are available
Write-Host "Test 1: Verify cmdlets are loaded..." -ForegroundColor Yellow
$cmdlets = @(
    'Set-DataverseDynamicPluginAssembly',
    'Get-DataverseDynamicPluginAssembly'
)

foreach ($cmdlet in $cmdlets) {
    $cmd = Get-Command $cmdlet -ErrorAction SilentlyContinue
    if ($cmd) {
        Write-Host "  ✓ $cmdlet is available" -ForegroundColor Green
    } else {
        Write-Host "  ✗ $cmdlet NOT FOUND" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""

# Test 2: Compile a simple plugin assembly (without Dataverse connection)
Write-Host "Test 2: Compile C# source code to assembly bytes..." -ForegroundColor Yellow

$sourceCode = @"
using System;
using Microsoft.Xrm.Sdk;

namespace TestPlugin
{
    public class SimplePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            trace.Trace("SimplePlugin executing on entity: " + context.PrimaryEntityName);
        }
    }
}
"@

Write-Host "  Source code:" -ForegroundColor Gray
Write-Host $sourceCode -ForegroundColor DarkGray

Write-Host ""
Write-Host "  Note: Full Dataverse upload requires a connection." -ForegroundColor Yellow
Write-Host "  This test would call: Set-DataverseDynamicPluginAssembly -Connection <conn> -SourceCode <code> -Name 'TestPlugin'" -ForegroundColor Gray
Write-Host ""

# Test 3: Show Get-DataverseDynamicPluginAssembly cmdlet help
Write-Host "Test 3: Display cmdlet help..." -ForegroundColor Yellow
Write-Host ""
Get-Help Get-DataverseDynamicPluginAssembly -Detailed

Write-Host ""
Write-Host "=== Manual Test Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "To test with a real Dataverse connection:" -ForegroundColor Yellow
Write-Host '  $conn = Get-DataverseConnection -Url "https://your-org.crm.dynamics.com" -Interactive' -ForegroundColor Gray
Write-Host '  Set-DataverseDynamicPluginAssembly -Connection $conn -SourceCode $sourceCode -Name "TestPlugin" -PassThru' -ForegroundColor Gray
Write-Host ""
