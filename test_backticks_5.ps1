$v = "code"
$backticks = "```"
$s = @"
${backticks}powershell
$v
${backticks}
"@
Write-Host $s
