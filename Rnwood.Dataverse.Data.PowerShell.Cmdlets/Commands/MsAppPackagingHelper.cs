using System;
using System.Collections.Generic;
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
        public static void PackMsApp(
            string sourceDirectory,
            string outputMsappPath,
            bool ignoreMissingDataSources = true,
            IReadOnlyList<string>? templateSourceMsappPaths = null,
            bool suppressDefaultTemplates = false)
        {
            MsAppToolkit.YamlFirstPackaging.PackFromDirectory(
                sourceDirectory,
                outputMsappPath,
                ignoreMissingDataSources,
                templateSourceMsappPaths,
                suppressDefaultTemplates);
        }

        /// <summary>
        /// Modifies an msapp by unpacking, modifying YAML, and repacking.
        /// Controls/*.json files are automatically regenerated from YAML.
        /// The original msapp is used as a template source during repacking to preserve
        /// control metadata (ControlPropertyState format, template defaults, etc.).
        /// </summary>
        public static void ModifyMsApp(string msappPath, Action<string> modifyAction, bool ignoreMissingDataSources = true, IReadOnlyList<string>? additionalTemplateSourcePaths = null)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"msapp_{Guid.NewGuid():N}");
            // Save a copy of the original msapp to use as template source during repack.
            // This ensures CPS entries retain their complex object format and template
            // metadata is resolved correctly.
            var templateCopy = Path.Combine(Path.GetTempPath(), $"msapp_template_{Guid.NewGuid():N}.msapp");
            try
            {
                File.Copy(msappPath, templateCopy, overwrite: true);

                // Unpack
                UnpackMsApp(msappPath, tempDir);

                // Modify
                modifyAction(tempDir);

                // Repack with original msapp as template source (plus any additional sources)
                var allTemplateSources = new List<string> { templateCopy };
                if (additionalTemplateSourcePaths is not null)
                {
                    allTemplateSources.AddRange(additionalTemplateSourcePaths);
                }

                PackMsApp(tempDir, msappPath, ignoreMissingDataSources,
                    templateSourceMsappPaths: allTemplateSources);
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

                if (File.Exists(templateCopy))
                {
                    try
                    {
                        File.Delete(templateCopy);
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
