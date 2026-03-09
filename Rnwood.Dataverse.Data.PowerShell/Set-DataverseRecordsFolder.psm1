set-strictmode -version 3.0

<#
.SYNOPSIS
Writes a list of Dataverse records to a folder of JSON files.

.DESCRIPTION
Writes a list of Dataverse records to a folder where each file represents a single record. The files are named using the Id property (or properties specified via -idproperties).
If records contain DataverseFileReference properties, the files are downloaded to a 'files' subfolder.

.PARAMETER OutputPath
Path to write output to.

.PARAMETER InputObject
Dataverse record(s) to write. Generally should be piped in from the pipeline.

.PARAMETER Connection
Dataverse connection to use for downloading files. Required if records contain DataverseFileReference properties.

.PARAMETER withdeletions
Output a list of deletions (records that were there last time, but are no longer present in the inputs) to 'deletions' subfolder of output.

.PARAMETER idproperties
Specifies the list of properties that will be used to generate a unique name for each file. By default this is "Id".

.EXAMPLE
Get-DataverseRecord -connection $connection -tablename contact | Set-DataverseRecordsFolder -OutputPath data/contacts -Connection $connection

Writes all contacts to the folder `data/contacts`, downloading any file attachments.

.EXAMPLE
Connect-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
Get-DataverseRecord -tablename contact | Set-DataverseRecordsFolder -OutputPath data/contacts

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
		[Parameter(Mandatory=$false)] $Connection,
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

		# Track files that are referenced by records
		$script:referencedFileIds = @()
		
		$newfiles = @()
	}

	process {
		$name = ($idproperties | ForEach-Object { $InputObject.$_ }) -join "-"
		# Replace invalid filename chars
		$name = $name.Split([IO.Path]::GetInvalidFileNameChars()) -join '_'
		$filename = "${name}.json"
		
		# Convert Byte[] properties to base64 strings before serialization
		$recordToSerialize = [PSCustomObject]@{}
		foreach ($property in $InputObject.PSObject.Properties) {
			$value = $property.Value
			
			# Check if the property value is a byte array
			if ($value -is [byte[]]) {
				# Convert byte array to base64 string
				$recordToSerialize | Add-Member -NotePropertyName $property.Name -NotePropertyValue ([Convert]::ToBase64String($value))
			}
			else {
				# Copy the property as-is
				$recordToSerialize | Add-Member -NotePropertyName $property.Name -NotePropertyValue $value
			}
		}
		
		$recordToSerialize | convertto-json -depth 100 | out-file -encoding utf8 (join-path $OutputPath $filename)

		if ($newfiles -contains $filename) {
			throw "The properties $idproperties do not result in a unique filename. The value '$filename' was generated more than once."
		}

		$newfiles += $filename

		# Process DataverseFileReference properties
		foreach ($property in $InputObject.PSObject.Properties) {
			$value = $property.Value
			
			# Check if the property value is a DataverseFileReference
			if ($value -ne $null -and $value.PSObject.TypeNames -contains 'Rnwood.Dataverse.Data.PowerShell.Commands.DataverseFileReference') {
				$fileId = $value.Id
				
				if ($fileId -eq [Guid]::Empty) {
					continue
				}

				# Track this file ID
				if ($script:referencedFileIds -notcontains $fileId) {
					$script:referencedFileIds += $fileId
				}

				# Create files subfolder if it doesn't exist
				$filesPath = Join-Path $OutputPath "files"
				if (-not (Test-Path $filesPath)) {
					New-Item -Type Directory $filesPath | Out-Null
				}

				# Check if file already exists (skip if unchanged)
				$folderFullPath = Join-Path $filesPath $fileId
				
				if (-not (Test-Path $folderFullPath/*)) {
	
					Write-Verbose "Downloading file $fileId for property '$($property.Name)' to $folderfullPath"
					
					# We need to get TableName, Id, and ColumnName from the record
					# The property name is the column name
					$tableName = $InputObject.TableName
					$recordId = $InputObject.Id
					$columnName = $property.Name
						
					Get-DataverseFileData -Connection:$Connection -TableName $tableName -Id $recordId -ColumnName $columnName -FolderPath $folderfullPath | Out-Null

				}
				else {
					Write-Verbose "File $fileId already exists, skipping download"
				}
			}
		}
	}

	end {
		# Find files which we didn't just overwrite and create deletion
		get-item $OutputPath/*.json -ErrorAction SilentlyContinue | Where-Object { $newfiles -notcontains $_.Name } | ForEach-Object {
			if ($withdeletions) {
				move-item $_ $OutputPath/deletions
			} else {
				remove-item $_
			}
		}

		if ($withdeletions) {
			# Find files that have been recreated and remove the deletion
			get-item $OutputPath/deletions/*.json -ErrorAction SilentlyContinue | Where-Object { $newfiles -contains $_.Name } | Remove-Item

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

		# Clean up unreferenced files in the files subfolder
		$filesPath = Join-Path $OutputPath "files"
		if (Test-Path $filesPath) {
			if (Test-Path "$filesPath/keep.me") {
				Remove-Item "$filesPath/keep.me"
			}
			Get-ChildItem $filesPath -ErrorAction SilentlyContinue | ForEach-Object {
				# Extract the file ID from the filename
				$fileIdString = $_.BaseName
				$fileId = [Guid]$fileIdString
					
				# If this file ID was not referenced by any record, delete it
				if ($script:referencedFileIds -notcontains $fileId) {
					Write-Verbose "Removing unreferenced file: $($_.Name)"
					Remove-Item -recurse $_.FullName
				}
			}

			# Add keep.me file if files folder is empty
			if (-not (Test-Path "$filesPath/*")) {
				"This file ensures the directory is kept when in source control system" | Out-File "$filesPath/keep.me"
			}
		}
	}
}