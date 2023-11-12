param([Parameter(Mandatory)][string]$projectdir, [Parameter(Mandatory)][string]$outdir)
$ErrorActionPreference="Stop"
$projectdir=$projectdir.Trim('""')
$outdir=$outdir.Trim('""')


if (-not(get-installedmodule Platyps -MinimumVersion 0.14.1 -ErrorAction silentlycontinue)) {
	install-module -scope CurrentUser PlatyPs -force
}

$modulepath = "$outdir/Rnwood.Dataverse.Data.PowerShell.psd1"
write-host "Using module: $modulepath"

import-module $modulepath

update-markdownhelpmodule -path $projectdir/docs -UpdateInputOutput
