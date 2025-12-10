$outputDirPath = Join-Path $PWD "examples-gallery"
if (-not (Test-Path $outputDirPath)) {
    New-Item -ItemType Directory -Path $outputDirPath | Out-Null
}
$outputDir = $outputDirPath

$docsDir = Resolve-Path "docs"
$cmdletDocsDir = Resolve-Path "Rnwood.Dataverse.Data.PowerShell/docs"

function Create-ExampleFile {
    param(
        $Title,
        $Description,
        $Code,
        $Tags,
        $SourceFile,
        $OutputDir
    )

    # Sanitize title for filename
    $filename = $Title -replace '[^a-zA-Z0-9\s-]', '' -replace '\s+', '-'
    
    # Fallback if filename is empty
    if ([string]::IsNullOrWhiteSpace($filename)) {
        $filename = "Example"
    }

    # Limit filename length
    if ($filename.Length -gt 50) { $filename = $filename.Substring(0, 50) }
    $filename = "$filename.md"
    $path = Join-Path $OutputDir $filename
    
    # Ensure unique filename
    $i = 1
    $basePath = $path
    while (Test-Path $path) {
        $path = $basePath.Replace(".md", "-$i.md")
        $i++
    }

    # Clean up code
    # 1. Remove PS C:\> prefix
    # 2. Comment out output lines
    $codeLines = $Code -split '\r?\n'
    $cleanedCodeLines = @()
    $isConsoleSession = $false
    
    # Check if it looks like a console session
    foreach ($line in $codeLines) {
        if ($line -match '^\s*PS [A-Z]:\\.*>') {
            $isConsoleSession = $true
            break
        }
    }

    if ($isConsoleSession) {
        foreach ($line in $codeLines) {
            if ($line -match '^\s*PS [A-Z]:\\.*>\s*(.*)') {
                # Command line - extract command
                $cleanedCodeLines += $matches[1]
            } elseif ($line -match '^\s+') {
                # Indented line - assume continuation
                $cleanedCodeLines += $line
            } elseif ($line -match '^\s*[)}\]]') {
                # Starts with closing bracket - assume continuation
                $cleanedCodeLines += $line
            } elseif ($line -match '^\s*$') {
                # Empty line - keep it
                $cleanedCodeLines += $line
            } else {
                # Output line - comment it out
                $cleanedCodeLines += "# $line"
            }
        }
        $Code = $cleanedCodeLines -join [Environment]::NewLine
    }

    # Create frontmatter
    $frontmatter = @"
---
title: "$Title"
tags: [$(($Tags | ForEach-Object { "'$_'" }) -join ', ')]
source: "$SourceFile"
---

$Description

``````powershell
$Code
``````
"@

    Set-Content -Path $path -Value $frontmatter -Encoding UTF8
    Write-Host "Created $path"
}

# Process Metadata-CRUD-Examples.md
$metadataFile = Join-Path $docsDir "Metadata-CRUD-Examples.md"
if (Test-Path $metadataFile) {
    $content = Get-Content $metadataFile -Raw
    # Split by headers level 3 (###)
    $sections = $content -split '(?m)^###\s+'
    
    foreach ($section in $sections) {
        if ($section -match '(?s)(.+?)\r?\n(.+?)```powershell\r?\n(.+?)```') {
            $title = $matches[1].Trim()
            $desc = $matches[2].Trim()
            $code = $matches[3].Trim()
            
            if ($title -and $code) {
                Create-ExampleFile -Title $title -Description $desc -Code $code -Tags @("Metadata", "CRUD") -SourceFile "Metadata-CRUD-Examples.md" -OutputDir $outputDir
            }
        }
    }
}

# Process Cmdlet Docs
$cmdletFiles = Get-ChildItem $cmdletDocsDir -Filter "*.md"
Write-Host "Found $($cmdletFiles.Count) cmdlet doc files in $cmdletDocsDir"
foreach ($file in $cmdletFiles) {
    # Check for override in primary docs dir
    $primaryPath = Join-Path $docsDir $file.Name
    if (Test-Path $primaryPath) {
        $fileToProcess = Get-Item $primaryPath
        Write-Host "Processing $($fileToProcess.Name) (Primary Source)"
    } else {
        $fileToProcess = $file
    }

    $content = Get-Content $fileToProcess.FullName -Raw
    
    # Look for Examples section
    if ($content -match '(?si)##\s+EXAMPLES\s+(.+?)((\r?\n##\s)|$)') {
        Write-Host "Found Examples in $($fileToProcess.Name)"
        $examplesContent = $matches[1]
        
        # Split by Example headers
        $examples = $examplesContent -split '(?m)^###\s+'
        foreach ($ex in $examples) {
            if ([string]::IsNullOrWhiteSpace($ex)) { continue }

            $title = ""
            $code = ""
            $desc = ""

            # Pattern: Title \n ```powershell \n Code \n ``` \n Description
            if ($ex -match '(?s)(.+?)\r?\n.*?```powershell\r?\n(.+?)```\r?\n(.+)') {
                $title = $matches[1].Trim()
                $code = $matches[2].Trim()
                $desc = $matches[3].Trim()
            }
            # Pattern: Title \n ```powershell \n Code \n ``` (No description or description inside code comments)
            elseif ($ex -match '(?s)(.+?)\r?\n.*?```powershell\r?\n(.+?)```') {
                $title = $matches[1].Trim()
                $code = $matches[2].Trim()
            }

            if ($title -and $code) {
                # Clean up description (remove alerts)
                if ($desc) {
                     $desc = $desc -replace '(?m)^\s*>\s*\[!(TIP|NOTE|WARNING|IMPORTANT|CAUTION)\].*(\r?\n\s*>\s*.*)*', ''
                     $desc = $desc.Trim()
                }

                # Clean up title
                $cleanTitle = $title -replace '^Example \d+:\s*', '' -replace '^Example \d+\s*', ''
                if ($cleanTitle -eq "" -or $cleanTitle -match '^Example \d+$' -or $cleanTitle -match '^\(.*\)$') {
                    # Try to use first line of description as title if title is generic
                    if ($desc) {
                        $firstLine = ($desc -split '\r?\n')[0]
                        # Remove blockquotes if present
                        $firstLine = $firstLine -replace '^>\s*', ''
                        if ($firstLine.Length -lt 80) {
                            $cleanTitle = $firstLine
                        } else {
                            $cleanTitle = "$($fileToProcess.BaseName) Example"
                        }
                    } else {
                        $cleanTitle = "$($fileToProcess.BaseName) Example"
                    }
                } else {
                    $cleanTitle = "$($fileToProcess.BaseName) - $cleanTitle"
                }

                $cmdletName = $fileToProcess.BaseName
                Create-ExampleFile -Title $cleanTitle -Description $desc -Code $code -Tags @($cmdletName) -SourceFile $fileToProcess.Name -OutputDir $outputDir
            }
        }
    }
}
