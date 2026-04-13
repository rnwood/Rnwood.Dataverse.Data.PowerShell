using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Azure.Identity;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Tool
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Fake4Dataverse CLI — export Dataverse table metadata to XML files for use in unit tests.");

            // ── export-metadata subcommand ────────────────────────────────────────
            var exportCommand = new Command("export-metadata", "Export entity metadata from a Dataverse environment to XML files.");

            var urlOption = new Option<string>(
                name: "--url",
                description: "The Dataverse environment URL (e.g. https://org.crm.dynamics.com/).")
            { IsRequired = true };

            var outputOption = new Option<DirectoryInfo>(
                name: "--output",
                description: "Output folder where XML files will be written.",
                getDefaultValue: () => new DirectoryInfo("metadata"))
            { IsRequired = false };

            var tablesOption = new Option<string[]>(
                name: "--tables",
                description: "Logical names of tables to export (space-separated). Omit when using --solutions.")
            { AllowMultipleArgumentsPerToken = true };

            var solutionsOption = new Option<string[]>(
                name: "--solutions",
                description: "Unique names of solutions whose table components should be exported (space-separated).")
            { AllowMultipleArgumentsPerToken = true };

            var includeAllAttributesOption = new Option<bool>(
                name: "--all-attributes",
                description: "Include all attributes in the export (default: all).",
                getDefaultValue: () => true);

            exportCommand.AddOption(urlOption);
            exportCommand.AddOption(outputOption);
            exportCommand.AddOption(tablesOption);
            exportCommand.AddOption(solutionsOption);
            exportCommand.AddOption(includeAllAttributesOption);

            exportCommand.SetHandler(async (context) =>
            {
                var url = context.ParseResult.GetValueForOption(urlOption)!;
                var output = context.ParseResult.GetValueForOption(outputOption)!;
                var tables = context.ParseResult.GetValueForOption(tablesOption) ?? Array.Empty<string>();
                var solutions = context.ParseResult.GetValueForOption(solutionsOption) ?? Array.Empty<string>();
                var ct = context.GetCancellationToken();

                context.ExitCode = await ExportMetadataAsync(url, output, tables, solutions, ct);
            });

            rootCommand.AddCommand(exportCommand);

            return await rootCommand.InvokeAsync(args);
        }

        private static async Task<int> ExportMetadataAsync(
            string url,
            DirectoryInfo outputDir,
            string[] tables,
            string[] solutions,
            CancellationToken ct)
        {
            if (tables.Length == 0 && solutions.Length == 0)
            {
                Console.Error.WriteLine("Error: specify at least one --tables or --solutions argument.");
                return 1;
            }

            Console.WriteLine($"Connecting to {url} with interactive authentication...");
            Console.WriteLine("A browser window will open for sign-in. If it doesn't open, check your default browser.");

            ServiceClient? client = null;
            try
            {
                var instanceUri = new Uri(url.TrimEnd('/') + "/");

                // Build a token credential that tries interactive browser first,
                // then falls back to device code flow (useful for CI / headless).
                var credential = new ChainedTokenCredential(
                    new InteractiveBrowserCredential(),
                    new DeviceCodeCredential());

                // ServiceClient overload that accepts a token-provider callback.
                // The resourceUrl passed in includes the WCF service path; strip to root + /.default.
                client = new ServiceClient(
                    tokenProviderFunction: async (resourceUrl) =>
                    {
                        var uri = new Uri(resourceUrl);
                        var audience = $"{uri.Scheme}://{uri.Host}/.default";
                        var tokenResult = await credential.GetTokenAsync(
                            new Azure.Core.TokenRequestContext(new[] { audience }),
                            CancellationToken.None);
                        return tokenResult.Token;
                    },
                    instanceUrl: instanceUri);

                // Force auth by executing WhoAmI — this is where the browser opens.
                var whoAmI = (Microsoft.Crm.Sdk.Messages.WhoAmIResponse)
                    await client.ExecuteAsync(new Microsoft.Crm.Sdk.Messages.WhoAmIRequest(), ct);
                Console.WriteLine($"Connected. User ID: {whoAmI.UserId}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to connect: {ex.Message}");
                var inner = ex.InnerException;
                while (inner != null)
                {
                    Console.Error.WriteLine($"  Caused by: {inner.GetType().Name}: {inner.Message}");
                    inner = inner.InnerException;
                }
                return 1;
            }

            // ── Resolve tables from solutions ─────────────────────────────────────
            var tableNames = new HashSet<string>(tables, StringComparer.OrdinalIgnoreCase);

            if (solutions.Length > 0)
            {
                Console.WriteLine($"Resolving tables from solution(s): {string.Join(", ", solutions)}");
                foreach (var solutionUniqueName in solutions)
                {
                    var resolved = await GetTableNamesFromSolutionAsync(client, solutionUniqueName, ct);
                    Console.WriteLine($"  Solution '{solutionUniqueName}': {resolved.Count} table(s) found.");
                    foreach (var t in resolved)
                        tableNames.Add(t);
                }
            }

            if (tableNames.Count == 0)
            {
                Console.Error.WriteLine("No tables resolved. Check solution names and that the solutions contain table components.");
                return 1;
            }

            Console.WriteLine($"Tables to export ({tableNames.Count}): {string.Join(", ", tableNames.OrderBy(x => x))}");

            // ── Fetch & serialize metadata ─────────────────────────────────────────
            outputDir.Create();

            int exported = 0;
            int failed = 0;

            foreach (var tableName in tableNames.OrderBy(x => x))
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    Console.Write($"  Exporting '{tableName}'... ");
                    var metadata = await RetrieveEntityMetadataAsync(client, tableName, ct);
                    var filePath = Path.Combine(outputDir.FullName, $"{tableName}.xml");
                    WriteMetadataXml(metadata, filePath);
                    Console.WriteLine($"OK → {filePath}");
                    exported++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FAILED: {ex.Message}");
                    failed++;
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Export complete. {exported} succeeded, {failed} failed.");
            Console.WriteLine($"Output folder: {outputDir.FullName}");
            return failed > 0 ? 2 : 0;
        }

        // ── Retrieve single entity metadata via SDK ──────────────────────────────

        private static async Task<EntityMetadata> RetrieveEntityMetadataAsync(
            ServiceClient client,
            string logicalName,
            CancellationToken ct)
        {
            var request = new RetrieveEntityRequest
            {
                LogicalName = logicalName,
                EntityFilters = EntityFilters.Entity | EntityFilters.Attributes | EntityFilters.Relationships,
                RetrieveAsIfPublished = true,
            };

            var response = (RetrieveEntityResponse)await client.ExecuteAsync(request, ct);
            return response.EntityMetadata;
        }

        // ── Resolve table names from a solution ───────────────────────────────────
        // Solution component type 1 = Entity.

        private static async Task<List<string>> GetTableNamesFromSolutionAsync(
            ServiceClient client,
            string solutionUniqueName,
            CancellationToken ct)
        {
            // Step 1: get the solution ID.
            var solutionQuery = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet("solutionid", "uniquename"),
                Criteria = {
                    Conditions = {
                        new ConditionExpression("uniquename", ConditionOperator.Equal, solutionUniqueName)
                    }
                },
                TopCount = 1,
            };

            var solutionResult = await client.RetrieveMultipleAsync(
                solutionQuery, ct);

            if (solutionResult.Entities.Count == 0)
            {
                Console.Error.WriteLine($"  Warning: solution '{solutionUniqueName}' not found.");
                return new List<string>();
            }

            var solutionId = solutionResult.Entities[0].Id;

            // Step 2: get the solutioncomponent records of type 1 (Entity).
            var componentQuery = new QueryExpression("solutioncomponent")
            {
                ColumnSet = new ColumnSet("objectid", "componenttype"),
                Criteria = {
                    Conditions = {
                        new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId),
                        new ConditionExpression("componenttype", ConditionOperator.Equal, 1), // Entity
                    }
                },
            };

            var componentResult = await client.RetrieveMultipleAsync(componentQuery, ct);

            if (componentResult.Entities.Count == 0)
                return new List<string>();

            // Collect object IDs (entity MetadataId values).
            var metadataIds = componentResult.Entities
                .Select(e => (Guid)e["objectid"])
                .ToList();

            // Step 3: retrieve entity metadata for each MetadataId to get the logical name.
            var tableNames = new List<string>(metadataIds.Count);
            foreach (var metadataId in metadataIds)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var req = new RetrieveEntityRequest
                    {
                        MetadataId = metadataId,
                        EntityFilters = EntityFilters.Entity,
                        RetrieveAsIfPublished = true,
                    };
                    var resp = (RetrieveEntityResponse)await client.ExecuteAsync(req, ct);
                    tableNames.Add(resp.EntityMetadata.LogicalName);
                }
                catch
                {
                    // If we can't resolve this metadata ID, skip it silently.
                }
            }

            return tableNames;
        }

        // ── Serialize EntityMetadata to XML via DataContractSerializer ────────────

        private static void WriteMetadataXml(EntityMetadata metadata, string filePath)
        {
            // Discover known attribute subtypes (same logic as EntityMetadataXmlLoader in the library).
            Type[] knownTypes;
            try
            {
                knownTypes = typeof(EntityMetadata).Assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface && typeof(AttributeMetadata).IsAssignableFrom(t))
                    .ToArray();
            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                knownTypes = (ex.Types ?? Array.Empty<Type?>())
                    .OfType<Type>()
                    .Where(t => !t.IsAbstract && !t.IsInterface && typeof(AttributeMetadata).IsAssignableFrom(t))
                    .ToArray();
            }

            var serializer = new DataContractSerializer(typeof(EntityMetadata), knownTypes);

            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            };

            using var stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = XmlWriter.Create(stream, settings);
            serializer.WriteObject(writer, metadata);
        }

    }
}
