# Tests for Get-DataverseDynamicPluginAssembly -OutputProjectPath parameter set

. "$PSScriptRoot/Common.ps1"

Describe 'Get-DataverseDynamicPluginAssembly - VS Project Export' {
    
    It "Exports a complete Visual Studio project from dynamic plugin assembly bytes" {
        # Create test metadata manually (simulating what Set-DataverseDynamicPluginAssembly would embed)
        $pluginSource = @"
using System;
using Microsoft.Xrm.Sdk;

namespace TestPluginProject
{
    public class TestPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            
            trace.Trace("Test plugin executing");
            
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                var target = (Entity)context.InputParameters["Target"];
                target["description"] = "Modified by plugin";
            }
        }
    }
}
"@
        
        # Create a valid base64-encoded mock key (160 bytes for testing)
        $mockKeyBytes = [byte[]](1..160)
        $mockKeyBase64 = [Convert]::ToBase64String($mockKeyBytes)
        
        # Create metadata object as JSON
        $metadata = @{
            AssemblyName = "TestVSProjectPlugin"
            Version = "1.0.0.0"
            Culture = "neutral"
            PublicKeyToken = "abcd1234"
            SourceCode = $pluginSource
            FrameworkReferences = @("System.Runtime.Serialization.dll")
            PackageReferences = @()
            StrongNameKey = $mockKeyBase64
        } | ConvertTo-Json
        
        # Create a mock assembly with embedded metadata
        $fakeAssemblyBytes = [System.Text.Encoding]::UTF8.GetBytes("FakeAssemblyContent")
        $metadataBytes = [System.Text.Encoding]::UTF8.GetBytes($metadata)
        $marker = [System.Text.Encoding]::ASCII.GetBytes("DPLM")
        $lengthBytes = [System.BitConverter]::GetBytes($metadataBytes.Length)
        
        # Combine: assembly + metadata + length + marker
        $assemblyWithMetadata = New-Object byte[] ($fakeAssemblyBytes.Length + $metadataBytes.Length + 8)
        [Array]::Copy($fakeAssemblyBytes, 0, $assemblyWithMetadata, 0, $fakeAssemblyBytes.Length)
        [Array]::Copy($metadataBytes, 0, $assemblyWithMetadata, $fakeAssemblyBytes.Length, $metadataBytes.Length)
        [Array]::Copy($lengthBytes, 0, $assemblyWithMetadata, $fakeAssemblyBytes.Length + $metadataBytes.Length, 4)
        [Array]::Copy($marker, 0, $assemblyWithMetadata, $fakeAssemblyBytes.Length + $metadataBytes.Length + 4, 4)
        
        # Export to VS project
        $outputPath = Join-Path $TestDrive "VSProject"
        Get-DataverseDynamicPluginAssembly -AssemblyBytes $assemblyWithMetadata -OutputProjectPath $outputPath
        
        # Verify project files were created
        $projectPath = Join-Path $outputPath "TestVSProjectPlugin.csproj"
        $sourcePath = Join-Path $outputPath "TestVSProjectPlugin.cs"
        $keyPath = Join-Path $outputPath "TestVSProjectPlugin.snk"
        
        Test-Path $projectPath | Should -Be $true
        Test-Path $sourcePath | Should -Be $true
        Test-Path $keyPath | Should -Be $true
        
        # Verify source code content
        $sourceContent = Get-Content $sourcePath -Raw
        $sourceContent | Should -Match "TestPluginProject"
        $sourceContent | Should -Match "TestPlugin"
        $sourceContent | Should -Match "IPlugin"
        
        # Verify project file content
        $projectContent = Get-Content $projectPath -Raw
        $projectContent | Should -Match "<TargetFramework>net462</TargetFramework>"
        $projectContent | Should -Match "<AssemblyName>TestVSProjectPlugin</AssemblyName>"
        $projectContent | Should -Match "<AssemblyVersion>1.0.0.0</AssemblyVersion>"
        $projectContent | Should -Match "<SignAssembly>true</SignAssembly>"
        $projectContent | Should -Match "Microsoft.CrmSdk.CoreAssemblies"
        
        # Verify key file exists and has content
        $keyFileInfo = Get-Item $keyPath
        $keyFileInfo.Length | Should -BeGreaterThan 0
    }
    
    It "Exports VS project with custom package references" {
        $pluginSource = @"
using System;
using Microsoft.Xrm.Sdk;

namespace CustomPackagePlugin
{
    public class CustomPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Custom logic
        }
    }
}
"@
        
        # Create a valid base64-encoded mock key
        $mockKeyBytes = [byte[]](1..160)
        $mockKeyBase64 = [Convert]::ToBase64String($mockKeyBytes)
        
        $metadata = @{
            AssemblyName = "CustomPackagePlugin"
            Version = "2.0.0.0"
            Culture = "neutral"
            PublicKeyToken = "xyz789"
            SourceCode = $pluginSource
            FrameworkReferences = @()
            PackageReferences = @("Newtonsoft.Json@13.0.1", "Microsoft.CrmSdk.CoreAssemblies@9.0.0")
            StrongNameKey = $mockKeyBase64
        } | ConvertTo-Json
        
        $fakeAssemblyBytes = [System.Text.Encoding]::UTF8.GetBytes("FakeAssemblyContent2")
        $metadataBytes = [System.Text.Encoding]::UTF8.GetBytes($metadata)
        $marker = [System.Text.Encoding]::ASCII.GetBytes("DPLM")
        $lengthBytes = [System.BitConverter]::GetBytes($metadataBytes.Length)
        
        $assemblyWithMetadata = New-Object byte[] ($fakeAssemblyBytes.Length + $metadataBytes.Length + 8)
        [Array]::Copy($fakeAssemblyBytes, 0, $assemblyWithMetadata, 0, $fakeAssemblyBytes.Length)
        [Array]::Copy($metadataBytes, 0, $assemblyWithMetadata, $fakeAssemblyBytes.Length, $metadataBytes.Length)
        [Array]::Copy($lengthBytes, 0, $assemblyWithMetadata, $fakeAssemblyBytes.Length + $metadataBytes.Length, 4)
        [Array]::Copy($marker, 0, $assemblyWithMetadata, $fakeAssemblyBytes.Length + $metadataBytes.Length + 4, 4)
        
        $outputPath = Join-Path $TestDrive "VSProjectCustom"
        Get-DataverseDynamicPluginAssembly -AssemblyBytes $assemblyWithMetadata -OutputProjectPath $outputPath
        
        $projectPath = Join-Path $outputPath "CustomPackagePlugin.csproj"
        Test-Path $projectPath | Should -Be $true
        
        $projectContent = Get-Content $projectPath -Raw
        $projectContent | Should -Match "Newtonsoft.Json"
        $projectContent | Should -Match "13.0.1"
    }
    
    It "Exports VS project from file path parameter set" {
        $pluginSource = @"
using System;
using Microsoft.Xrm.Sdk;

namespace FilePathPlugin
{
    public class FilePathTestPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Test logic
        }
    }
}
"@
        
        # Create a valid base64-encoded mock key
        $mockKeyBytes = [byte[]](1..160)
        $mockKeyBase64 = [Convert]::ToBase64String($mockKeyBytes)
        
        $metadata = @{
            AssemblyName = "FilePathTestPlugin"
            Version = "3.0.0.0"
            Culture = "neutral"
            PublicKeyToken = "test123"
            SourceCode = $pluginSource
            FrameworkReferences = @()
            PackageReferences = @()
            StrongNameKey = $mockKeyBase64
        } | ConvertTo-Json
        
        $fakeAssemblyBytes = [System.Text.Encoding]::UTF8.GetBytes("FakeAssemblyContent3")
        $metadataBytes = [System.Text.Encoding]::UTF8.GetBytes($metadata)
        $marker = [System.Text.Encoding]::ASCII.GetBytes("DPLM")
        $lengthBytes = [System.BitConverter]::GetBytes($metadataBytes.Length)
        
        $assemblyWithMetadata = New-Object byte[] ($fakeAssemblyBytes.Length + $metadataBytes.Length + 8)
        [Array]::Copy($fakeAssemblyBytes, 0, $assemblyWithMetadata, 0, $fakeAssemblyBytes.Length)
        [Array]::Copy($metadataBytes, 0, $assemblyWithMetadata, $fakeAssemblyBytes.Length, $metadataBytes.Length)
        [Array]::Copy($lengthBytes, 0, $assemblyWithMetadata, $fakeAssemblyBytes.Length + $metadataBytes.Length, 4)
        [Array]::Copy($marker, 0, $assemblyWithMetadata, $fakeAssemblyBytes.Length + $metadataBytes.Length + 4, 4)
        
        # Save assembly to temp file
        $assemblyFilePath = Join-Path $TestDrive "FilePathTestPlugin.dll"
        [System.IO.File]::WriteAllBytes($assemblyFilePath, $assemblyWithMetadata)
        
        # Export from file path
        $outputPath = Join-Path $TestDrive "VSProjectFromFile"
        Get-DataverseDynamicPluginAssembly -FilePath $assemblyFilePath -OutputProjectPath $outputPath
        
        # Verify files were created
        $projectPath = Join-Path $outputPath "FilePathTestPlugin.csproj"
        $sourcePath = Join-Path $outputPath "FilePathTestPlugin.cs"
        
        Test-Path $projectPath | Should -Be $true
        Test-Path $sourcePath | Should -Be $true
        
        $projectContent = Get-Content $projectPath -Raw
        $projectContent | Should -Match "<AssemblyVersion>3.0.0.0</AssemblyVersion>"
    }
}
