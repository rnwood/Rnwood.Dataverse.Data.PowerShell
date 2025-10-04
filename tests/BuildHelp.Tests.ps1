Describe 'BuildHelp target (incremental)' {
	. $PSScriptRoot/Common.ps1

	It 'skips running buildhelp.ps1 when inputs are unchanged, and runs when an input changes' {
		# Arrange
		$projDir = Resolve-Path (Join-Path $PSScriptRoot '..\Rnwood.Dataverse.Data.PowerShell')
		$proj = Join-Path $projDir 'Rnwood.Dataverse.Data.PowerShell.csproj'
		$outDir = Join-Path $env:TEMP ("Rnwood.HelpTest.{0}" -f ([guid]::NewGuid().ToString()))
		New-Item -ItemType Directory -Path $outDir | Out-Null

		# Ensure docs folder exists (one of the inputs)
		$docsDir = Join-Path $projDir 'docs'
		New-Item -ItemType Directory -Path $docsDir -Force | Out-Null

		# Clean any previous stamp in the chosen outdir
		$stamp = Join-Path $outDir 'help.stamp'
		if (Test-Path $stamp) { Remove-Item $stamp -Force }

		# Act - first build should run and create the stamp
		& dotnet msbuild $proj -t:BuildHelp -nologo -p:Configuration=Release -p:OutDir="$outDir\" | Out-Null
		$LASTEXITCODE | Should -Be 0
		Test-Path $stamp | Should -BeTrue
		$t1 = (Get-Item $stamp).LastWriteTimeUtc

		# Act - second build without changes should be skipped (stamp unchanged)
		Start-Sleep -Milliseconds 500
		& dotnet msbuild $proj -t:BuildHelp -nologo -p:Configuration=Release -p:OutDir="$outDir\" | Out-Null
		$LASTEXITCODE | Should -Be 0
		$t2 = (Get-Item $stamp).LastWriteTimeUtc
		$t2 | Should -Be $t1

		# Touch an input file (create a new md file in docs) to force rebuild
		$newMd = Join-Path $docsDir ("test-trigger-{0}.md" -f ([guid]::NewGuid().ToString()))
		"trigger" | Out-File -FilePath $newMd -Encoding utf8
		# Ensure the filesystem has a different timestamp
		Start-Sleep -Seconds 1

		# Act - third build should run and update the stamp
		& dotnet msbuild $proj -t:BuildHelp -nologo -p:Configuration=Release -p:OutDir="$outDir\" | Out-Null
		$LASTEXITCODE | Should -Be 0
		$t3 = (Get-Item $stamp).LastWriteTimeUtc
		$t3 | Should -BeGreaterThan $t1

		# Cleanup
		Remove-Item $outDir -Recurse -Force -ErrorAction SilentlyContinue
		Remove-Item $newMd -Force -ErrorAction SilentlyContinue
	}
}
