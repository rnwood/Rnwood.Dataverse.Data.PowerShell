using Xunit;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using FluentAssertions;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

/// <summary>
/// Tests for MsAppPackagingHelper to verify msapp pack/unpack operations work on .NET Framework 4.6.2.
/// The net8.0 implementation uses MsAppToolkit which has its own tests.
/// </summary>
#if !NET8_0_OR_GREATER
public class MsAppPackagingHelperTests : TestBase
{
    private static byte[] CreateTestMsApp(string screenYaml)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddZipEntry(archive, "Src/Screen1.pa.yaml", screenYaml);
            AddZipEntry(archive, "Src/App.pa.yaml", "App:\n  Properties:\n    Theme: =PowerAppsTheme\n");
            AddZipEntry(archive, "Src/_EditorState.pa.yaml", "{}\n");
            AddZipEntry(archive, "References/DataSources.json", "[]");
        }
        return memoryStream.ToArray();
    }

    private static void AddZipEntry(ZipArchive archive, string name, string content)
    {
        var entry = archive.CreateEntry(name);
        using var stream = entry.Open();
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static string ReadZipEntry(byte[] zipBytes, string entryName)
    {
        using var memoryStream = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
        var entry = archive.GetEntry(entryName);
        if (entry == null) return null;
        using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
        return reader.ReadToEnd();
    }

    [Fact]
    public void ModifyMsApp_Should_ApplyModificationToYamlFile()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.msapp");
        try
        {
            byte[] original = CreateTestMsApp("Screens:\n  Screen1:\n    Properties:\n      Fill: =Color.Blue\n");
            File.WriteAllBytes(tempFile, original);

            string newYaml = "Screens:\n  Screen1:\n    Properties:\n      Fill: =Color.Red\n";

            // Act
            MsAppPackagingHelper.ModifyMsApp(tempFile, unpackDir =>
            {
                string yamlPath = Path.Combine(unpackDir, "Src", "Screen1.pa.yaml");
                File.WriteAllText(yamlPath, newYaml, Encoding.UTF8);
            });

            // Assert
            byte[] modified = File.ReadAllBytes(tempFile);
            string content = ReadZipEntry(modified, "Src/Screen1.pa.yaml");
            content.Should().Be(newYaml);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void ModifyMsApp_Should_PreserveOtherFiles()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.msapp");
        try
        {
            string appYaml = "App:\n  Properties:\n    Theme: =PowerAppsTheme\n";
            byte[] original = CreateTestMsApp("Screens:\n  Screen1:\n    Properties: {}\n");
            File.WriteAllBytes(tempFile, original);

            // Act - modify only Screen1.pa.yaml
            MsAppPackagingHelper.ModifyMsApp(tempFile, unpackDir =>
            {
                string yamlPath = Path.Combine(unpackDir, "Src", "Screen1.pa.yaml");
                File.WriteAllText(yamlPath, "Screens:\n  Screen1:\n    Properties:\n      Fill: =Color.Green\n", Encoding.UTF8);
            });

            // Assert - App.pa.yaml should still be there unchanged
            byte[] modified = File.ReadAllBytes(tempFile);
            string appContent = ReadZipEntry(modified, "Src/App.pa.yaml");
            appContent.Should().Be(appYaml);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void UnpackMsApp_Should_ExtractFilesToDirectory()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.msapp");
        var outputDir = Path.Combine(Path.GetTempPath(), $"msapp_unpack_{Guid.NewGuid():N}");
        try
        {
            byte[] msapp = CreateTestMsApp("Screens:\n  Screen1:\n    Properties: {}\n");
            File.WriteAllBytes(tempFile, msapp);

            // Act
            MsAppPackagingHelper.UnpackMsApp(tempFile, outputDir);

            // Assert
            File.Exists(Path.Combine(outputDir, "Src", "Screen1.pa.yaml")).Should().BeTrue();
            File.Exists(Path.Combine(outputDir, "Src", "App.pa.yaml")).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
            if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
        }
    }

    [Fact]
    public void PackMsApp_Should_CreateZipFromDirectory()
    {
        // Arrange
        var sourceDir = Path.Combine(Path.GetTempPath(), $"msapp_source_{Guid.NewGuid():N}");
        var outputFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.msapp");
        try
        {
            Directory.CreateDirectory(Path.Combine(sourceDir, "Src"));
            string screenContent = "Screens:\n  Screen1:\n    Properties: {}\n";
            File.WriteAllText(Path.Combine(sourceDir, "Src", "Screen1.pa.yaml"), screenContent, Encoding.UTF8);

            // Act
            MsAppPackagingHelper.PackMsApp(sourceDir, outputFile);

            // Assert
            File.Exists(outputFile).Should().BeTrue();
            string content = ReadZipEntry(File.ReadAllBytes(outputFile), "Src/Screen1.pa.yaml");
            content.Should().Be(screenContent);
        }
        finally
        {
            if (Directory.Exists(sourceDir)) Directory.Delete(sourceDir, true);
            if (File.Exists(outputFile)) File.Delete(outputFile);
        }
    }

    [Fact]
    public void InitEmptyAppDirectory_Should_CreateRequiredFiles()
    {
        // Arrange
        var outputDir = Path.Combine(Path.GetTempPath(), $"msapp_empty_{Guid.NewGuid():N}");
        try
        {
            // Act
            MsAppPackagingHelper.InitEmptyAppDirectory(outputDir, "MyScreen");

            // Assert
            File.Exists(Path.Combine(outputDir, "Src", "MyScreen.pa.yaml")).Should().BeTrue();
            File.Exists(Path.Combine(outputDir, "Src", "App.pa.yaml")).Should().BeTrue();
            File.Exists(Path.Combine(outputDir, "Src", "_EditorState.pa.yaml")).Should().BeTrue();
            File.Exists(Path.Combine(outputDir, "References", "DataSources.json")).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
        }
    }
}
#endif
