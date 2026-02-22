using System;
using System.IO;
using System.Text;
#if NET8_0_OR_GREATER
using MsAppToolkit;
#else
using ICSharpCode.SharpZipLib.Zip;
#endif

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Helper class for msapp pack/unpack operations.
    /// On .NET 8+ uses YamlFirstPackaging (MsAppToolkit) which automatically regenerates Controls/*.json from YAML.
    /// On .NET Framework 4.6.2 uses direct zip manipulation via SharpZipLib (no Controls JSON regeneration).
    /// </summary>
    internal static class MsAppPackagingHelper
    {
#if NET8_0_OR_GREATER
        /// <summary>
        /// Unpacks an msapp file to a directory using Yaml-first packaging.
        /// </summary>
        public static void UnpackMsApp(string msappPath, string outputDirectory)
        {
            MsAppToolkit.YamlFirstPackaging.UnpackToDirectory(msappPath, outputDirectory);
        }

        /// <summary>
        /// Packs a directory into an msapp file using Yaml-first packaging.
        /// This automatically generates Controls/*.json from YAML files.
        /// </summary>
        public static void PackMsApp(string sourceDirectory, string outputMsappPath, bool ignoreMissingDataSources = true)
        {
            MsAppToolkit.YamlFirstPackaging.PackFromDirectory(sourceDirectory, outputMsappPath, ignoreMissingDataSources);
        }

        /// <summary>
        /// Modifies an msapp by unpacking, modifying YAML, and repacking.
        /// Controls/*.json files are automatically regenerated from YAML.
        /// </summary>
        public static void ModifyMsApp(string msappPath, Action<string> modifyAction, bool ignoreMissingDataSources = true)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"msapp_{Guid.NewGuid():N}");
            try
            {
                // Unpack
                UnpackMsApp(msappPath, tempDir);

                // Modify
                modifyAction(tempDir);

                // Repack
                PackMsApp(tempDir, msappPath, ignoreMissingDataSources);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        /// <summary>
        /// Initializes an empty app directory with a single screen.
        /// </summary>
        public static void InitEmptyAppDirectory(string outputDirectory, string screenName = "Screen1")
        {
            MsAppToolkit.YamlFirstPackaging.InitEmptyAppDirectory(outputDirectory, screenName);
        }
#else
        /// <summary>
        /// Unpacks an msapp file to a directory using direct zip extraction.
        /// </summary>
        public static void UnpackMsApp(string msappPath, string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);

            using (var fileStream = File.OpenRead(msappPath))
            using (var zipInputStream = new ZipInputStream(fileStream))
            {
                ZipEntry entry;
                while ((entry = zipInputStream.GetNextEntry()) != null)
                {
                    if (!entry.IsFile)
                    {
                        continue;
                    }

                    string entryPath = entry.Name.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar);
                    string destPath = Path.Combine(outputDirectory, entryPath);

                    string destDir = Path.GetDirectoryName(destPath);
                    if (destDir != null && !Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    using (var outStream = File.Create(destPath))
                    {
                        byte[] buffer = new byte[4096];
                        int count;
                        while ((count = zipInputStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            outStream.Write(buffer, 0, count);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Packs a directory into an msapp file using direct zip creation.
        /// Note: Controls/*.json files are NOT automatically regenerated from YAML on .NET Framework 4.6.2.
        /// </summary>
        public static void PackMsApp(string sourceDirectory, string outputMsappPath, bool ignoreMissingDataSources = true)
        {
            using (var fileStream = File.Create(outputMsappPath))
            using (var zipOutputStream = new ZipOutputStream(fileStream))
            {
                zipOutputStream.SetLevel(6);

                string[] files = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string relativePath = file.Substring(sourceDirectory.Length).TrimStart(Path.DirectorySeparatorChar);
                    string zipEntryName = relativePath.Replace(Path.DirectorySeparatorChar, '/');

                    byte[] content = File.ReadAllBytes(file);

                    var zipEntry = new ZipEntry(zipEntryName)
                    {
                        DateTime = File.GetLastWriteTime(file),
                        Size = content.Length
                    };

                    zipOutputStream.PutNextEntry(zipEntry);
                    zipOutputStream.Write(content, 0, content.Length);
                    zipOutputStream.CloseEntry();
                }

                zipOutputStream.Finish();
            }
        }

        /// <summary>
        /// Modifies an msapp by unpacking, modifying YAML, and repacking using direct zip manipulation.
        /// Note: Controls/*.json files are NOT automatically regenerated from YAML on .NET Framework 4.6.2.
        /// </summary>
        public static void ModifyMsApp(string msappPath, Action<string> modifyAction, bool ignoreMissingDataSources = true)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"msapp_{Guid.NewGuid():N}");
            var tempMsapp = msappPath + ".tmp";
            try
            {
                UnpackMsApp(msappPath, tempDir);
                modifyAction(tempDir);
                PackMsApp(tempDir, tempMsapp);
                File.Delete(msappPath);
                File.Move(tempMsapp, msappPath);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { /* Ignore cleanup errors */ }
                }
                if (File.Exists(tempMsapp))
                {
                    try { File.Delete(tempMsapp); } catch { /* Ignore cleanup errors */ }
                }
            }
        }

        /// <summary>
        /// Initializes an empty app directory with a single screen.
        /// </summary>
        public static void InitEmptyAppDirectory(string outputDirectory, string screenName = "Screen1")
        {
            Directory.CreateDirectory(outputDirectory);

            string srcDir = Path.Combine(outputDirectory, "Src");
            Directory.CreateDirectory(srcDir);

            File.WriteAllText(
                Path.Combine(srcDir, $"{screenName}.pa.yaml"),
                $"Screens:\n  {screenName}:\n    Properties:\n      LoadingSpinnerColor: =RGBA(56, 96, 178, 1)\n",
                Encoding.UTF8);

            File.WriteAllText(
                Path.Combine(srcDir, "App.pa.yaml"),
                "App:\n  Properties:\n    Theme: =PowerAppsTheme\n",
                Encoding.UTF8);

            File.WriteAllText(
                Path.Combine(srcDir, "_EditorState.pa.yaml"),
                "{}\n",
                Encoding.UTF8);

            string refsDir = Path.Combine(outputDirectory, "References");
            Directory.CreateDirectory(refsDir);
            File.WriteAllText(Path.Combine(refsDir, "DataSources.json"), "[]", Encoding.UTF8);
            File.WriteAllText(Path.Combine(refsDir, "Themes.json"), "{\"CurrentTheme\":\"PowerAppsTheme\",\"ThemesGallery\":[]}", Encoding.UTF8);
            File.WriteAllText(Path.Combine(refsDir, "Resources.json"), "[]", Encoding.UTF8);
            File.WriteAllText(Path.Combine(refsDir, "Templates.json"), "[]", Encoding.UTF8);
        }
#endif
    }
}
