Describe 'updatehelp.ps1 (incremental guard)' {
	. $PSScriptRoot\Common.ps1

	It 'creates a stamp on first run, is skipped when inputs unchanged, and re-runs when an input is touched' {
		$projDir = Resolve-Path (Join-Path $PSScriptRoot '..\Rnwood.Dataverse.Data.PowerShell')
		$outDir = Join-Path $env:TEMP ("Rnwood.UpdateHelpTest.{0}" -f ([guid]::NewGuid().ToString()))
		New-Item -ItemType Directory -Path $outDir | Out-Null

		$stamp = Join-Path $outDir 'updatehelp.stamp'
		if (Test-Path $stamp) { Remove-Item $stamp -Force }

		# First run - should create stamp
		& pwsh -NoProfile -NonInteractive -ExecutionPolicy Bypass -File (Join-Path $projDir 'updatehelp.ps1') -ProjectDir $projDir -OutDir $outDir
		Test-Path $stamp | Should -BeTrue
		$t1 = (Get-Item $stamp).LastWriteTimeUtc

		# Second run without changes - should be skipped and stamp unchanged
		Start-Sleep -Milliseconds 500
		& pwsh -NoProfile -NonInteractive -ExecutionPolicy Bypass -File (Join-Path $projDir 'updatehelp.ps1') -ProjectDir $projDir -OutDir $outDir
		$t2 = (Get-Item $stamp).LastWriteTimeUtc
		$t2 | Should -Be $t1

		# Touch an input (create a new doc file) to force re-run
		$docsDir = Join-Path $projDir 'docs'
		New-Item -ItemType Directory -Path $docsDir -Force | Out-Null
		$newMd = Join-Path $docsDir ("trigger-{0}.md" -f ([guid]::NewGuid().ToString()))
		"trigger" | Out-File -FilePath $newMd -Encoding utf8
		Start-Sleep -Seconds 1

		# Third run - stamp should be updated
		& pwsh -NoProfile -NonInteractive -ExecutionPolicy Bypass -File (Join-Path $projDir 'updatehelp.ps1') -ProjectDir $projDir -OutDir $outDir
		$t3 = (Get-Item $stamp).LastWriteTimeUtc
		$t3 | Should -BeGreaterThan $t1

		# Cleanup
		Remove-Item $outDir -Recurse -Force -ErrorAction SilentlyContinue
		Remove-Item $newMd -Force -ErrorAction SilentlyContinue
	}
}
