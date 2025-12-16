. $PSScriptRoot/Common.ps1

Describe 'New-DataversePluginAssembly and Get-DataversePluginAssemblySource' {
    
    It "Compiles simple C# source code into an assembly" {
        $sourceCode = @"
using System;

namespace TestPlugin
{
    public class TestClass
    {
        public void TestMethod()
        {
            Console.WriteLine("Hello from plugin!");
        }
    }
}
"@

        $assembly = New-DataversePluginAssembly -SourceCode $sourceCode -AssemblyName "TestPlugin" -Version "1.0.0.0"
        
        $assembly | Should -Not -BeNullOrEmpty
        $assembly | Should -BeOfType [byte[]]
        $assembly.Length | Should -BeGreaterThan 0
    }

    It "Embeds metadata in compiled assembly that can be extracted" {
        $sourceCode = @"
using System;

namespace TestPlugin
{
    public class EmbedTest
    {
        public string GetMessage() { return "Test"; }
    }
}
"@

        $assembly = New-DataversePluginAssembly -SourceCode $sourceCode -AssemblyName "EmbedTest" -Version "2.0.0.0"
        
        $metadata = Get-DataversePluginAssemblySource -AssemblyBytes $assembly
        
        $metadata | Should -Not -BeNullOrEmpty
        $metadata.AssemblyName | Should -Be "EmbedTest"
        $metadata.Version | Should -Be "2.0.0.0"
        $metadata.SourceCode | Should -Be $sourceCode
    }

    It "Includes framework references in metadata" {
        $sourceCode = @"
using System;
using System.Linq;

namespace TestPlugin
{
    public class LinqTest
    {
        public void UseLinq()
        {
            var list = new[] { 1, 2, 3 };
            var result = list.Where(x => x > 1).ToArray();
        }
    }
}
"@

        $frameworkRefs = @("System.Linq")
        $assembly = New-DataversePluginAssembly -SourceCode $sourceCode -AssemblyName "LinqTest" -FrameworkReferences $frameworkRefs
        
        $metadata = Get-DataversePluginAssemblySource -AssemblyBytes $assembly
        
        $metadata.FrameworkReferences | Should -Contain "System.Linq"
    }

    It "Handles compilation errors gracefully" {
        $invalidCode = @"
using System;

namespace TestPlugin
{
    public class InvalidClass
    {
        // Missing semicolon
        public void Method()
        {
            var x = 5
        }
    }
}
"@

        { New-DataversePluginAssembly -SourceCode $invalidCode -AssemblyName "InvalidPlugin" } | Should -Throw
    }

    It "Can read source from file" {
        $tempFile = [System.IO.Path]::GetTempFileName() + ".cs"
        $sourceCode = @"
using System;

namespace TestPlugin
{
    public class FileTest
    {
        public void Method() { }
    }
}
"@
        Set-Content -Path $tempFile -Value $sourceCode

        try {
            $assembly = New-DataversePluginAssembly -SourceFile $tempFile -AssemblyName "FileTest"
            
            $assembly | Should -Not -BeNullOrEmpty
            
            $metadata = Get-DataversePluginAssemblySource -AssemblyBytes $assembly
            $metadata.SourceCode.Trim() | Should -Be $sourceCode.Trim()
        }
        finally {
            Remove-Item -Path $tempFile -ErrorAction SilentlyContinue
        }
    }

    It "Can write assembly to output file" {
        $sourceCode = @"
using System;

namespace TestPlugin
{
    public class OutputTest
    {
        public void Method() { }
    }
}
"@
        $outputPath = [System.IO.Path]::GetTempFileName() + ".dll"

        try {
            $assembly = New-DataversePluginAssembly -SourceCode $sourceCode -AssemblyName "OutputTest" -OutputPath $outputPath
            
            Test-Path $outputPath | Should -Be $true
            (Get-Item $outputPath).Length | Should -BeGreaterThan 0
            
            # Verify we can read it back
            $metadata = Get-DataversePluginAssemblySource -FilePath $outputPath
            $metadata.AssemblyName | Should -Be "OutputTest"
        }
        finally {
            Remove-Item -Path $outputPath -ErrorAction SilentlyContinue
        }
    }

    It "Can extract source to output file" {
        $sourceCode = @"
using System;

namespace TestPlugin
{
    public class ExtractTest
    {
        public void Method() { }
    }
}
"@
        $assembly = New-DataversePluginAssembly -SourceCode $sourceCode -AssemblyName "ExtractTest"
        $outputFile = [System.IO.Path]::GetTempFileName() + ".cs"

        try {
            $metadata = Get-DataversePluginAssemblySource -AssemblyBytes $assembly -OutputSourceFile $outputFile
            
            Test-Path $outputFile | Should -Be $true
            $extractedSource = Get-Content -Path $outputFile -Raw
            $extractedSource.Trim() | Should -Be $sourceCode.Trim()
        }
        finally {
            Remove-Item -Path $outputFile -ErrorAction SilentlyContinue
        }
    }

    It "Returns null metadata for assembly without embedded data" {
        # Create a minimal valid assembly without our metadata
        $simpleCode = @"
using System;
public class Simple { }
"@
        # Use .NET's built-in compilation (this won't have our metadata marker)
        $assembly = New-DataversePluginAssembly -SourceCode $simpleCode -AssemblyName "NoMetadata"
        
        # Remove the metadata footer to simulate an assembly without our marker
        $markerSize = 8 # 4 bytes for length + 4 bytes for "DPLM"
        $assemblyWithoutMetadata = $assembly[0..($assembly.Length - 1 - $markerSize - 100)]
        
        # This should warn about missing metadata
        $result = Get-DataversePluginAssemblySource -AssemblyBytes $assemblyWithoutMetadata -WarningAction SilentlyContinue
        
        $result | Should -BeNullOrEmpty
    }

    It "Preserves culture setting in metadata" {
        $sourceCode = "using System; public class Test { }"
        $assembly = New-DataversePluginAssembly -SourceCode $sourceCode -AssemblyName "CultureTest" -Culture "en-US"
        
        $metadata = Get-DataversePluginAssemblySource -AssemblyBytes $assembly
        
        $metadata.Culture | Should -Be "en-US"
    }

    It "Includes package references in metadata" {
        $sourceCode = "using System; public class Test { }"
        $packageRefs = @("Microsoft.Xrm.Sdk@9.0.2", "Newtonsoft.Json@13.0.1")
        
        $assembly = New-DataversePluginAssembly -SourceCode $sourceCode -AssemblyName "PackageTest" -PackageReferences $packageRefs
        
        $metadata = Get-DataversePluginAssemblySource -AssemblyBytes $assembly
        
        $metadata.PackageReferences | Should -Contain "Microsoft.Xrm.Sdk@9.0.2"
        $metadata.PackageReferences | Should -Contain "Newtonsoft.Json@13.0.1"
    }
}
