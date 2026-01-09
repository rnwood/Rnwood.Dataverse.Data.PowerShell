set-strictmode -version 3.0

<#
.SYNOPSIS
Writes a list of Dataverse records to a folder of JSON files.

.DESCRIPTION
Writes a list of Dataverse records to a folder where each file represents a single record. The files are named using the Id property (or properties specified via -idproperties).

File and image columns (byte arrays) are automatically extracted to separate binary files in a '_files' subdirectory for efficient storage and change detection.
Only files that have changed content are updated, reducing unnecessary writes.

.PARAMETER OutputPath
Path to write output to.

.PARAMETER InputObject
Dataverse record(s) to write. Generally should be piped in from the pipeline.

.PARAMETER withdeletions
Output a list of deletions (records that were there last time, but are no longer present in the inputs) to 'deletions' subfolder of output.

.PARAMETER idproperties
Specifies the list of properties that will be used to generate a unique name for each file. By default this is "Id".

.EXAMPLE
Get-DataverseRecord -connection $connection -tablename contact | Set-DataverseRecordsFolder data/contacts

Writes all contacts to the folder `data/contacts`.

.EXAMPLE
Connect-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
Get-DataverseRecord -tablename contact | Set-DataverseRecordsFolder data/contacts

Writes all contacts to the folder `data/contacts` using the default connection.

.INPUTS
System.Management.Automation.PSObject

.OUTPUTS
System.Object

#>
function Set-DataverseRecordsFolder {
	[CmdletBinding()]
	param(
		[Parameter(Mandatory)][string] $OutputPath, 
		[Parameter(ValueFromPipeline=$true)] [PSObject] $InputObject,
		[switch] $withdeletions,
		[string[]]$idproperties = @("Id")
	)

	begin {
	
		if (-not (test-path $OutputPath)) {
			new-item -type directory $OutputPath | out-null
		}

		if ($withdeletions) {
			if (-not (test-path $OutputPath/deletions)) {
				new-item -type directory $OutputPath/deletions | out-null
			}
		} elseif (test-path $OutputPath/deletions) {
			remove-item -recurse $OutputPath/deletions
		}

		# Create _files directory for binary content
		$filesPath = Join-Path $OutputPath "_files"
		if (-not (test-path $filesPath)) {
			new-item -type directory $filesPath | out-null
		}

		$newfiles = @()
		$newBinaryFiles = @()
	}

	process {
		$name = ($idproperties | ForEach-Object { $InputObject.$_ }) -join "-"
		# Replace invalid filenam chars
		$name = $name.Split([IO.Path]::GetInvalidFileNameChars()) -join '_'
		$filename = "${name}.json"
		
		# Clone the object so we can modify it without affecting the original
		$outputObject = [PSCustomObject]@{}
		foreach ($prop in $InputObject.PSObject.Properties) {
			$outputObject | Add-Member -MemberType NoteProperty -Name $prop.Name -Value $prop.Value
		}
		
		# Process file and image columns (byte arrays)
		foreach ($prop in $InputObject.PSObject.Properties) {
			$value = $prop.Value
			
			# Check if property is a byte array (file or image column)
			if ($null -ne $value -and $value -is [byte[]]) {
				$propertyName = $prop.Name
				
				# Compute hash for change detection
				if ($value.Length -gt 0) {
					$hash = (Get-FileHash -InputStream ([System.IO.MemoryStream]::new($value)) -Algorithm SHA256).Hash
				} else {
					# Empty array - use a special marker hash
					$hash = "EMPTY"
				}
				
				# Create filename for binary file
				$binaryFilename = "${name}_${propertyName}.bin"
				$binaryFilePath = Join-Path $filesPath $binaryFilename
				
				# Check if file exists and has same hash
				$needsWrite = $true
				if (Test-Path $binaryFilePath) {
					if ($value.Length -gt 0) {
						$existingHash = (Get-FileHash -Path $binaryFilePath -Algorithm SHA256).Hash
					} else {
						# Empty file check
						$existingHash = if ((Get-Item $binaryFilePath).Length -eq 0) { "EMPTY" } else { "NOTEMPTY" }
					}
					if ($existingHash -eq $hash) {
						$needsWrite = $false
					}
				}
				
				# Only write if changed
				if ($needsWrite) {
					[System.IO.File]::WriteAllBytes($binaryFilePath, $value)
				}
				
				$newBinaryFiles += $binaryFilename
				
				# Replace byte array with file reference
				$outputObject.$propertyName = @{
					"__fileReference" = $binaryFilename
					"__hash" = $hash
					"__size" = $value.Length
				}
			}
		}
		
		$outputObject | convertto-json -depth 100 | out-file -encoding utf8 (join-path $OutputPath $filename)

		if ($newfiles -contains $filename) {
			throw "The properties $idproperties do not result in a unique filename. The value ''$filename' was generated more than once."
		}

		$newfiles += $filename
	}

	end {
		# Find files which we didn't just overwrite and create deletion
		get-item $OutputPath/*.json | Where-Object { $newfiles -notcontains $_.Name } | ForEach-Object {
			if ($withdeletions) {
				move-item $_ $OutputPath/deletions
			} else {
				remove-item $_
			}
		}

		# Clean up orphaned binary files in _files directory
		$filesPath = Join-Path $OutputPath "_files"
		if (Test-Path $filesPath) {
			Get-ChildItem -Path $filesPath -Filter "*.bin" | Where-Object { $newBinaryFiles -notcontains $_.Name } | ForEach-Object {
				Remove-Item $_.FullName
			}
			
			# Remove _files directory if empty
			if (-not (Get-ChildItem -Path $filesPath)) {
				Remove-Item $filesPath
			}
		}

		if ($withdeletions) {
			# Find files that have been recreated and remove the deletion
			get-item $OutputPath/deletions/*.json | Where-Object { $newfiles -contains $_.Name } | Remove-Item

			if (-not (test-path "$OutputPath/deletions/*.json")) {
				"This file ensures the directory is kept when in source control system" | out-file $OutputPath/deletions/keep.me
			} elseif (test-path $OutputPath/deletions/keep.me) {
				remove-item $OutputPath/deletions/keep.me
			}
		}

		if (-not (test-path "$OutputPath/*.json")) {
				"This file ensures the directory is kept when in source control system" | out-file $OutputPath/keep.me
			} elseif (test-path $OutputPath/keep.me) {
				remove-item $OutputPath/keep.me
			}
		}
	
}