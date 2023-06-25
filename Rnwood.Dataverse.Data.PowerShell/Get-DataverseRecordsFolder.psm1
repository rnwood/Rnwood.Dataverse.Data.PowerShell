set-strictmode -version 3.0

function Get-DataverseRecordsFolder {
	[CmdletBinding()]
	param([Parameter(Mandatory)][string] $InputPath)

	begin {
	}

	process {
		get-childitem $InputPath -filter *.json | foreach-object {
			get-content $_.fullname -encoding utf8 | convertfrom-json
		}
	}

	end {

	}
}