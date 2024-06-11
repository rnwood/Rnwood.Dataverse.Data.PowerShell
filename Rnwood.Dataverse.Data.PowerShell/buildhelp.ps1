param([Parameter(Mandatory)][string]$projectdir, [Parameter(Mandatory)][string]$outdir)
$ErrorActionPreference="Stop"
$projectdir=$projectdir.Trim('""')
$outdir=$outdir.Trim('""')
if (-not(get-installedmodule Platyps -MinimumVersion 0.14.1 -ErrorAction silentlycontinue)) {
	install-module -scope CurrentUser PlatyPs -force
}

if (test-path $outdir\en-GB\) {
	remove-item -recurse -force $outdir\en-GB\
}

New-ExternalHelp $projectdir/docs -OutputPath $outdir\en-GB\ -force
