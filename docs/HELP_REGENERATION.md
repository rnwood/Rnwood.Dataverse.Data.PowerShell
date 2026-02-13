# Help File Regeneration Guide

## Problem: Corrupted Help Files

Help markdown files can become corrupted with duplicate parameter sections and enum values, causing:
- Massive file sizes (>100KB for a single cmdlet)
- Non-deterministic changes when `updatehelp.ps1` runs
- Merge conflicts in pull requests
- Flip-flopping content between commits

## Symptoms

- Help file is abnormally large (>100KB)
- Many duplicate parameter sections (>10x duplication)
- Enum values concatenated hundreds of times on a single line
- File grows each time `updatehelp.ps1` runs

## Validation

Run the validation script to check for corruption:

```powershell
./scripts/Validate-HelpFiles.ps1
```

This will scan all help files and report any issues.

## How to Fix Corrupted Help Files

### Step 1: Build the Module

```powershell
dotnet clean
dotnet build -c Debug
```

### Step 2: Delete the Corrupted File

```powershell
Remove-Item ./Rnwood.Dataverse.Data.PowerShell/docs/YourCmdlet.md
```

### Step 3: Regenerate with PlatyPS

```powershell
Import-Module ./Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/Rnwood.Dataverse.Data.PowerShell.psd1 -Force
New-MarkdownHelp -Command YourCmdlet -OutputFolder ./Rnwood.Dataverse.Data.PowerShell/docs -Force
```

### Step 4: Add Documentation

The generated file will have template placeholders like `{{ Fill in the Synopsis }}`. Replace these with proper documentation:

1. **SYNOPSIS**: One-line description of what the cmdlet does
2. **DESCRIPTION**: Detailed explanation of the cmdlet's purpose and behavior
3. **EXAMPLES**: Real-world usage examples with explanations

You can extract these from the original corrupted file (look at the beginning before the duplication starts) or from the cmdlet's XML comments in the C# source code.

### Step 5: Verify the File

1. Check the file size is reasonable (<50KB)
2. Count parameters: `grep -c "^### -" yourfile.md`
3. Run validation: `./scripts/Validate-HelpFiles.ps1`
4. Test help: `Get-Help YourCmdlet -Full`

### Step 6: Regenerate MAML Help

```powershell
./Rnwood.Dataverse.Data.PowerShell/buildhelp.ps1 `
    -projectdir ./Rnwood.Dataverse.Data.PowerShell `
    -outdir ./Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0
```

### Step 7: Test in PowerShell

```powershell
Import-Module ./Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/Rnwood.Dataverse.Data.PowerShell.psd1 -Force
Get-Help YourCmdlet
Get-Help YourCmdlet -Full
Get-Help YourCmdlet -Examples
```

## Prevention

- **CI Validation**: The `Validate-HelpFiles.ps1` script can be integrated into CI to catch corruption early
- **Code Review**: Watch for PRs that show large changes to help files without corresponding cmdlet changes
- **Monitor File Sizes**: Help files should typically be 5-50KB; anything larger needs investigation

## Example: Set-DataverseForm.md

This cmdlet had severe corruption:
- **Before**: 503KB, 23,624 lines, 118x parameter duplication
- **After**: 11KB, 396 lines, no duplication
- **Reduction**: 98% smaller

The corruption was caused by PlatyPS appending duplicate content repeatedly, likely due to the file already being in a corrupt state when `updatehelp.ps1` ran.
