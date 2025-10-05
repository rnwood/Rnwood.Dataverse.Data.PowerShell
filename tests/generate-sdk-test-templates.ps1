param(
    [switch]$Force
)

<#
Generates Pester test skeleton files for SDK-generated Invoke-Dataverse* cmdlets.

Scans the Cmdlets/Commands/sdk folder for generated cmdlet classes and emits
one basic test file per cmdlet under tests/sdk-test-templates/. Files are
skipped by default to avoid overwriting existing hand-edited tests. Use
-Force to overwrite.

This generator is intentionally conservative and emits skeletons marked as
Pending to avoid introducing fragile tests automatically.
#>

$sdkCmdletsPath = Join-Path -Path $PSScriptRoot -ChildPath '..\Rnwood.Dataverse.Data.PowerShell.Cmdlets\Commands\sdk'
$outDir = Join-Path -Path $PSScriptRoot -ChildPath 'sdk-test-templates'

if (-not (Test-Path $sdkCmdletsPath)) {
    Write-Error "SDK cmdlets folder not found: $sdkCmdletsPath"
    return
}

if (-not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir | Out-Null
}

$csFiles = Get-ChildItem -Path $sdkCmdletsPath -Filter '*.cs' -Recurse

foreach ($f in $csFiles) {
    $content = Get-Content -LiteralPath $f.FullName -Raw

    # Try to find the Cmdlet attribute to determine verb-noun
    $m = [regex]::Match($content, '\[Cmdlet\("(?<verb>[^\"]+)",\s*"(?<noun>[^\"]+)"')
    if (-not $m.Success) {
        continue
    }

    $cmdlet = "$($m.Groups['verb'].Value)-$($m.Groups['noun'].Value)"
    $safeName = $cmdlet -replace '[^A-Za-z0-9\-]', '_'
    $outFile = Join-Path $outDir "$safeName.Tests.ps1"

    if ((Test-Path $outFile) -and (-not $Force)) {
        Write-Host "Skipping existing: $outFile"
        continue
    }

    $template = @"
. `$PSScriptRoot/../Common.ps1

Describe '$cmdlet (SDK-generated) - Test skeletons' {

    It 'Template: validates parameters and executes expected behaviour' -Pending {
        # TODO: implement a focused test exercising minimal parameters and expected SDK behaviour.
        # Use getMockConnection to provide a mock ServiceClient and FakeXrmEasy metadata fixtures.
    }

}
"@

    $template | Set-Content -LiteralPath $outFile -Encoding UTF8
    Write-Host "Generated: $outFile"
}

Write-Host 'Generation complete.'
