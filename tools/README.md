# GenerateRequestCmdlets.ps1

This script automatically generates PowerShell cmdlets for all Microsoft Dataverse SDK request types.

## Usage

```powershell
# Generate all cmdlets and tests (slow - generates 300+ cmdlets and tests)
./tools/GenerateRequestCmdlets.ps1

# Generate specific cmdlets only
./tools/GenerateRequestCmdlets.ps1 -OnlyRequests "ImportSolutionRequest","ExportSolutionRequest"

# Specify custom output directories
./tools/GenerateRequestCmdlets.ps1 -OutputDirectory "./Custom/Path" -TestOutputDirectory "./Custom/Tests"
```

## Prerequisites

1. Build the solution first:
   ```powershell
   dotnet build -c Release
   ```

2. The script requires:
   - PowerShell 7+ (pwsh)
   - Built assemblies in `Rnwood.Dataverse.Data.PowerShell.Cmdlets/bin/Release/net462/`

## Features

### Automatic Test Generation

The generator automatically creates Pester tests for each SDK cmdlet in `tests/sdk/`:

**Test Structure:**
- Each cmdlet gets its own `Invoke-Dataverse[Name].Tests.ps1` file
- Tests use the same `Common.ps1` setup as other module tests
- Tests use FakeXrmEasy mock connection for safe, isolated testing
- Automatically generates appropriate test parameters based on request property types

**Generated Test Features:**
- Basic invocation test with mock connection
- Automatic parameter value generation (Guid, String, Int, Entity, EntityReference)
- Entity parameters automatically include `-TableName` parameter
- WhatIf test for cmdlets without mandatory parameters
- Follows existing Pester test patterns

**Example Generated Test:**
```powershell
. $PSScriptRoot/../Common.ps1

Describe 'Invoke-DataverseWhoAmI' {
    BeforeAll {
        $script:conn = getMockConnection
    }

    It 'Can invoke the cmdlet with a mock connection' {
        {
            $result = Invoke-DataverseWhoAmI -Connection $script:conn
            $result | Should -Not -BeNull
        } | Should -Not -Throw
    }

    It 'Supports -WhatIf parameter' {
        {
            $result = Invoke-DataverseWhoAmI -Connection $script:conn -WhatIf
        } | Should -Not -Throw
    }
}
```

**Running Generated Tests:**
```powershell
# Set module path for tests
$env:TESTMODULEPATH = (Resolve-Path "Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0")

# Run all SDK tests
Invoke-Pester -Path tests/sdk -Output Detailed

# Run specific test
Invoke-Pester -Path tests/sdk/Invoke-DataverseWhoAmI.Tests.ps1 -Output Detailed
```

### Automatic InFile/OutFile Parameter Generation

The generator automatically adds file I/O support for cmdlets with byte[] parameters:

**For Input Cmdlets** (byte[] parameters like `CustomizationFile`, `TranslationFile`, `BlockData`):
- Adds `-InFile <path>` parameter in a separate "FromFile" parameter set
- Automatically reads file contents when `-InFile` is used
- Validates file existence and throws `FileNotFoundException` if missing

**Example Generated Code:**
```csharp
[Parameter(ParameterSetName = "Default")]
public System.Byte[] CustomizationFile { get; set; }

[Parameter(ParameterSetName = "FromFile", Mandatory = true)]
[ValidateNotNullOrEmpty]
public System.String InFile { get; set; }
```

**ProcessRecord Implementation:**
```csharp
// Load file if InFile parameter is specified
byte[] customizationFileData = CustomizationFile;
if (!string.IsNullOrEmpty(InFile))
{
    if (!File.Exists(InFile))
    {
        throw new FileNotFoundException($"The specified file does not exist: {InFile}");
    }
    customizationFileData = File.ReadAllBytes(InFile);
}
request.CustomizationFile = customizationFileData;
```

### Parameter Sets

Cmdlets with byte[] parameters automatically get two parameter sets:
- **Default**: Use byte[] parameter directly
- **FromFile**: Use `-InFile` parameter instead

This allows both usage patterns:
```powershell
# Direct byte array
$bytes = [System.IO.File]::ReadAllBytes("solution.zip")
Invoke-DataverseImportSolution -CustomizationFile $bytes ...

# File path (new!)
Invoke-DataverseImportSolution -InFile "solution.zip" ...
```

## Implementation Details

The generator:
1. Detects byte[] properties on request types
2. Adds `using System.IO;` to cmdlets with byte[] properties
3. Adds `DefaultParameterSetName = "Default"` to cmdlet attribute
4. Generates InFile parameter with "FromFile" parameter set
5. Generates file loading logic in ProcessRecord method
6. Updates help documentation with both parameter options

## Regenerating All Cmdlets and Tests

To regenerate all SDK cmdlets and their tests with the latest generator enhancements:

```powershell
# 1. Build the solution
dotnet build -c Release

# 2. Run the generator (takes 5-10 minutes)
pwsh ./tools/GenerateRequestCmdlets.ps1

# 3. Rebuild the solution with new cmdlets
dotnet build -c Release

# 4. Run tests to verify
$env:TESTMODULEPATH = (Resolve-Path "Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0")
Invoke-Pester -Path tests/sdk -Output Detailed
```

**Note:** The generator removes all existing files in:
- `Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands/sdk/` (cmdlet .cs files)
- `tests/sdk/` (test .ps1 files)

Any manual changes to generated files will be lost.

## Future Enhancements

Planned features:
- **OutFile support** for export cmdlets (responses with byte[] properties)
- Automatic detection of response types to add `-OutFile` parameter
- Progress reporting during generation
- Enhanced test coverage for complex parameter types
- Response validation in tests

## Troubleshooting

**Issue:** Generator hangs or takes too long
- **Solution:** Use `-OnlyRequests` to generate specific cmdlets only

**Issue:** Assembly not found error  
- **Solution:** Build the solution first with `dotnet build -c Release`

**Issue:** Module import fails
- **Solution:** Ensure the module is built in `Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/`
