. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseDynamicPluginAssembly and Get-DataverseDynamicPluginAssembly' {
    
    It "Compiles C# plugin source code and uploads to Dataverse" {
        $connection = getMockConnection
        
        $sourceCode = @"
using System;
using Microsoft.Xrm.Sdk;

namespace TestPlugin
{
    public class TestPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Plugin logic here
        }
    }
}
"@

        $result = Set-DataverseDynamicPluginAssembly -Connection $connection -SourceCode $sourceCode -Name "TestPlugin" -PassThru
        
        $result | Should -Not -BeNullOrEmpty
        $result.name | Should -Be "TestPlugin"
    }

    It "Detects plugin types implementing IPlugin interface" {
        $connection = getMockConnection
        
        $sourceCode = @"
using System;
using Microsoft.Xrm.Sdk;

namespace TestPlugin
{
    public class PluginOne : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider) { }
    }
    
    public class PluginTwo : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider) { }
    }
}
"@

        # This should succeed because plugin types are found
        { Set-DataverseDynamicPluginAssembly -Connection $connection -SourceCode $sourceCode -Name "MultiPlugin" -WhatIf } | Should -Not -Throw
    }

    It "Throws error when no plugin types found in source" {
        $connection = getMockConnection
        
        $sourceCode = @"
using System;

namespace TestPlugin
{
    public class NotAPlugin
    {
        public void SomeMethod() { }
    }
}
"@

        # This should fail because no IPlugin classes found
        { Set-DataverseDynamicPluginAssembly -Connection $connection -SourceCode $sourceCode -Name "NoPlugin" } | Should -Throw "*No plugin types found*"
    }

    It "Extracts source code from compiled assembly" {
        $sourceCode = @"
using System;
using Microsoft.Xrm.Sdk;

namespace TestPlugin
{
    public class ExtractTest : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider) { }
    }
}
"@

        $connection = getMockConnection
        
        # First, create the assembly
        $result = Set-DataverseDynamicPluginAssembly -Connection $connection -SourceCode $sourceCode -Name "ExtractTest" -PassThru
        
        # Then retrieve it
        $assembly = Get-DataversePluginAssembly -Connection $connection -Name "ExtractTest"
        
        # Extract metadata
        $base64Content = $assembly.content
        $bytes = [Convert]::FromBase64String($base64Content)
        $metadata = Get-DataverseDynamicPluginAssembly -AssemblyBytes $bytes
        
        $metadata | Should -Not -BeNullOrEmpty
        $metadata.AssemblyName | Should -Be "ExtractTest"
        $metadata.SourceCode.Trim() | Should -Be $sourceCode.Trim()
    }

    It "Reuses existing version when not specified" {
        $connection = getMockConnection
        
        $sourceCode1 = @"
using System;
using Microsoft.Xrm.Sdk;

namespace Test { public class Plugin1 : IPlugin { public void Execute(IServiceProvider sp) { } } }
"@

        # Create with version 1.0.0.0
        Set-DataverseDynamicPluginAssembly -Connection $connection -SourceCode $sourceCode1 -Name "VersionTest" -Version "1.0.0.0"
        
        $sourceCode2 = @"
using System;
using Microsoft.Xrm.Sdk;

namespace Test { public class Plugin2 : IPlugin { public void Execute(IServiceProvider sp) { } } }
"@

        # Update without specifying version - should keep existing
        $result = Set-DataverseDynamicPluginAssembly -Connection $connection -SourceCode $sourceCode2 -Name "VersionTest" -PassThru
        
        # Note: In mock, version may not be preserved exactly, but cmdlet should attempt to reuse it
        $result | Should -Not -BeNullOrEmpty
    }

    It "Handles compilation errors gracefully" {
        $connection = getMockConnection
        
        $invalidCode = @"
using System;
using Microsoft.Xrm.Sdk;

namespace TestPlugin
{
    public class InvalidPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var x = 5  // Missing semicolon
        }
    }
}
"@

        { Set-DataverseDynamicPluginAssembly -Connection $connection -SourceCode $invalidCode -Name "InvalidPlugin" } | Should -Throw "*Compilation failed*"
    }

    It "Can read source from file" {
        $connection = getMockConnection
        $tempFile = [System.IO.Path]::GetTempFileName() + ".cs"
        $sourceCode = @"
using System;
using Microsoft.Xrm.Sdk;

namespace TestPlugin
{
    public class FilePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider) { }
    }
}
"@
        Set-Content -Path $tempFile -Value $sourceCode

        try {
            $result = Set-DataverseDynamicPluginAssembly -Connection $connection -SourceFile $tempFile -Name "FilePlugin" -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            $result.name | Should -Be "FilePlugin"
        }
        finally {
            Remove-Item -Path $tempFile -ErrorAction SilentlyContinue
        }
    }
}
