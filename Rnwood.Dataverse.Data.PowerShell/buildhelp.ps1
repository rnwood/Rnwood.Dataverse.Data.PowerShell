param([Parameter(Mandatory)][string]$projectdir, [Parameter(Mandatory)][string]$outdir)
$ErrorActionPreference="Stop"
$projectdir=$projectdir.Trim('""')
$outdir=$outdir.Trim('""')
if (-not(get-installedmodule Platyps -MinimumVersion 0.14.1 -ErrorAction silentlycontinue)) {
	try {
		install-module -scope CurrentUser PlatyPs -force -ErrorAction Stop
	} catch {
		write-host "Could not install PlatyPS module: $_"
		write-host "Skipping help build (PlatyPS not available)"
		exit 0
	}
}

if (test-path $outdir\en-GB\) {
	remove-item -verbose -recurse -force $outdir\en-GB\
}


New-ExternalHelp -verbose -path $projectdir/docs -OutputPath $outdir\en-GB\