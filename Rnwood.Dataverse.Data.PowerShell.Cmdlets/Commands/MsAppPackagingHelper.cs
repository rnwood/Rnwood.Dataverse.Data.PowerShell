using System;
using System.IO;
using MsAppToolkit;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Helper class for msapp pack/unpack operations using YamlFirstPackaging (MsAppToolkit).
    /// Automatically regenerates Controls/*.json from YAML.
    /// </summary>
    internal static class MsAppPackagingHelper
    {
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
    }
}
