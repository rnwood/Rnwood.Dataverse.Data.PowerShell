using MsAppToolkit;

if (args.Length < 1)
{
    PrintUsage();
    return 1;
}

try
{
    var command = args[0].ToLowerInvariant();
    switch (command)
    {
        case "unpack":
            if (args.Length != 3)
            {
                PrintUsage();
                return 2;
            }

            YamlFirstPackaging.UnpackToDirectory(args[1], args[2]);
            Console.WriteLine($"Unpacked '{args[1]}' -> '{args[2]}'");
            Console.WriteLine("Edit YAML in Src/*.pa.yaml (including Src/Components/*.pa.yaml), then run pack.");
            return 0;

        case "pack":
            if (args.Length < 3 || args.Length > 4)
            {
                PrintUsage();
                return 2;
            }

            var ignoreDs = args.Length == 4 && args[3].Equals("--ignore-missing-datasources", StringComparison.OrdinalIgnoreCase);
            YamlFirstPackaging.PackFromDirectory(args[1], args[2], ignoreDs);
            Console.WriteLine($"Packed '{args[1]}' -> '{args[2]}' (derived files regenerated from YAML)");
            return 0;

        case "init-empty-app":
            if (args.Length < 2 || args.Length > 3)
            {
                PrintUsage();
                return 2;
            }

            YamlFirstPackaging.InitEmptyAppDirectory(args[1], args.Length == 3 ? args[2] : "Screen1");
            Console.WriteLine($"Initialized empty app folder at '{args[1]}' with start screen '{(args.Length == 3 ? args[2] : "Screen1")}'");
            Console.WriteLine("Edit YAML in Src/*.pa.yaml, then run pack.");
            return 0;

        case "gen-collection-datasource":
            if (args.Length != 4)
            {
                PrintUsage();
                return 2;
            }

            YamlFirstPackaging.GenerateCollectionDataSourceFromJson(args[1], args[2], args[3]);
            Console.WriteLine($"Generated/updated data source '{args[2]}' in '{args[1]}' from '{args[3]}'");
            return 0;

        case "upsert-collection-datasource":
            if (args.Length != 4)
            {
                PrintUsage();
                return 2;
            }

            YamlFirstPackaging.GenerateCollectionDataSourceFromJson(args[1], args[2], args[3]);
            Console.WriteLine($"Upserted collection data source '{args[2]}' in '{args[1]}' from '{args[3]}'");
            return 0;

        case "remove-datasource":
            if (args.Length != 3)
            {
                PrintUsage();
                return 2;
            }

            var removed = YamlFirstPackaging.RemoveDataSource(args[1], args[2]);
            Console.WriteLine(removed
                ? $"Removed data source '{args[2]}' from '{args[1]}'"
                : $"Data source '{args[2]}' not found in '{args[1]}'");
            return 0;

        case "upsert-dataverse-datasource":
            if (args.Length < 4 || args.Length > 6)
            {
                PrintUsage();
                return 2;
            }

            await YamlFirstPackaging.UpsertDataverseTableDataSourceFromEnvironmentAsync(
                args[1],
                args[2],
                args[3],
                args.Length >= 5 ? args[4] : null,
                args.Length == 6 ? args[5] : null);
            Console.WriteLine($"Fetched Dataverse metadata for table '{args[3]}' from '{args[2]}' and upserted datasource artifacts in '{args[1]}'");
            return 0;

        case "list-control-templates":
            if (args.Length != 1)
            {
                PrintUsage();
                return 2;
            }

            var templates = YamlFirstPackaging.ListEmbeddedControlTemplates();
            Console.WriteLine($"Embedded templates: {templates.Count}");
            foreach (var t in templates)
            {
                var variantTag = t.RequiresVariantKeyword
                    ? $" [Variant required: {(t.AvailableVariants.Count > 0 ? string.Join(", ", t.AvailableVariants) : "see template-properties") }]"
                    : string.Empty;
                var flagTag = t.AppFlagRequirements.Count > 0
                    ? " [Flag: " + string.Join(", ", t.AppFlagRequirements.Select(r => $"{r.FlagName}={r.RequiredValue}")) + "]"
                    : string.Empty;
                Console.WriteLine($"- YAML:{t.YamlControlName}@{t.Version} | Template:{t.Name}@{t.Version} | Id={t.TemplateId}{variantTag}{flagTag}");
            }

            return 0;

        case "template-properties":
            if (args.Length < 2 || args.Length > 3)
            {
                PrintUsage();
                return 2;
            }

            var templateName = args[1];
            var templateVersion = args.Length == 3 ? args[2] : null;
            var details = YamlFirstPackaging.DescribeEmbeddedTemplate(templateName, templateVersion);
            Console.WriteLine($"Template: {details.Name}@{details.Version}");
            Console.WriteLine($"YAML Control: {details.YamlControlName}@{details.Version}");
            Console.WriteLine($"Id: {details.TemplateId}");

            if (details.RequiresVariantKeyword)
            {
                Console.WriteLine();
                Console.WriteLine("Required YAML keywords:");
                Console.WriteLine("  Variant  (must be non-empty; pack will fail without it)");
                if (details.AvailableVariants.Count > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Available variants ({details.AvailableVariants.Count}):");
                    foreach (var v in details.AvailableVariants)
                    {
                        Console.WriteLine($"  - {v}");
                    }
                }
                else
                {
                    Console.WriteLine("  (no variants found in embedded all-controls catalog)");
                }
            }

            if (details.AppFlagRequirements.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"App flag requirements ({details.AppFlagRequirements.Count}):");
                foreach (var req in details.AppFlagRequirements)
                {
                    Console.WriteLine($"  * AppPreviewFlagsMap.{req.FlagName}={req.RequiredValue}");
                    Console.WriteLine($"    {req.Reason}");
                }
            }
        
            Console.WriteLine();
            Console.WriteLine($"Properties ({details.Properties.Count}):");
            foreach (var p in details.Properties)
            {
                Console.WriteLine($"- {p.PropertyName}" + (string.IsNullOrWhiteSpace(p.DefaultValue) ? string.Empty : $" (default={p.DefaultValue})"));
            }

            if (details.ContextualDynamicProperties.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"Dynamic container-child properties ({details.ContextualDynamicProperties.Count}):");
                Console.WriteLine("  Available on any control placed inside an auto-layout (horizontal or vertical)");
                Console.WriteLine("  GroupContainer. Injected by the parent at runtime; not part of the template.");
                foreach (var p in details.ContextualDynamicProperties)
                {
                    Console.WriteLine($"- {p.PropertyName}" + (string.IsNullOrWhiteSpace(p.DefaultValue) ? string.Empty : $" (default={p.DefaultValue})"));
                }
            }

            return 0;

        default:
            PrintUsage();
            return 2;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    return 1;
}

static void PrintUsage()
{
    Console.WriteLine("MsAppYamlCli - YAML-first pack/unpack for Power Apps .msapp");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  MsAppYamlCli unpack <input.msapp> <output-directory>");
    Console.WriteLine("  MsAppYamlCli pack   <input-directory> <output.msapp> [--ignore-missing-datasources]");
    Console.WriteLine("  MsAppYamlCli init-empty-app <output-directory> [screen-name]");
    Console.WriteLine("  MsAppYamlCli gen-collection-datasource <input-directory> <collection-name> <json-example-file>");
    Console.WriteLine("  MsAppYamlCli upsert-collection-datasource <input-directory> <collection-name> <json-example-file>");
    Console.WriteLine("  MsAppYamlCli remove-datasource <input-directory> <datasource-name>");
    Console.WriteLine("  MsAppYamlCli upsert-dataverse-datasource <input-directory> <environment-url> <table-logical-name> [datasource-name] [dataset-name]");
    Console.WriteLine("  MsAppYamlCli list-control-templates");
    Console.WriteLine("  MsAppYamlCli template-properties <template-name> [template-version]");
}