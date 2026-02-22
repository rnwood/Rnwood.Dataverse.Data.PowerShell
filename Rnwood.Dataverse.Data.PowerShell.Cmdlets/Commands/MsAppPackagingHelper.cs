using System;
using System.IO;
using System.Management.Automation;
#if NET8_0_OR_GREATER
using MsAppToolkit;
#endif

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Helper class for msapp pack/unpack operations using YamlFirstPackaging.
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
        // For net462, we don't have MsAppToolkit available
        // The cmdlets will need to handle this appropriately
        public static void UnpackMsApp(string msappPath, string outputDirectory)
        {
            throw new NotSupportedException(
                "YAML-first msapp packaging with automatic Controls JSON generation is only supported on PowerShell 7+ (.NET 8). " +
                "For PowerShell 5.1 (.NET Framework 4.6.2), please use PowerShell 7+.");
        }

        public static void PackMsApp(string sourceDirectory, string outputMsappPath, bool ignoreMissingDataSources = true)
        {
            throw new NotSupportedException(
                "YAML-first msapp packaging with automatic Controls JSON generation is only supported on PowerShell 7+ (.NET 8). " +
                "For PowerShell 5.1 (.NET Framework 4.6.2), please use PowerShell 7+.");
        }

        public static void ModifyMsApp(string msappPath, Action<string> modifyAction, bool ignoreMissingDataSources = true)
        {
            throw new NotSupportedException(
                "YAML-first msapp packaging with automatic Controls JSON generation is only supported on PowerShell 7+ (.NET 8). " +
                "For PowerShell 5.1 (.NET Framework 4.6.2), please use PowerShell 7+.");
        }

        public static void InitEmptyAppDirectory(string outputDirectory, string screenName = "Screen1")
        {
            throw new NotSupportedException(
                "YAML-first msapp packaging with automatic Controls JSON generation is only supported on PowerShell 7+ (.NET 8). " +
                "For PowerShell 5.1 (.NET Framework 4.6.2), please use PowerShell 7+.");
        }
#endif
    }
}
