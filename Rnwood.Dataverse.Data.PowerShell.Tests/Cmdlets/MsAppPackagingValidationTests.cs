using MsAppToolkit;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

public class MsAppPackagingValidationTests
{
    [Fact]
    public void BuildExpandedEntityMetadataRelativePath_IncludesPowerAppsCriticalExpands()
    {
        var method = typeof(YamlFirstPackaging).GetMethod("BuildExpandedEntityMetadataRelativePath", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var path = method!.Invoke(null, new object[] { "contact" }) as string;
        Assert.False(string.IsNullOrWhiteSpace(path));
        Assert.Contains("EntityDefinitions(LogicalName='contact')", path!, StringComparison.Ordinal);
        Assert.Contains("$expand=Attributes", path!, StringComparison.Ordinal);
        Assert.Contains("ManyToOneRelationships", path!, StringComparison.Ordinal);
        Assert.Contains("OneToManyRelationships", path!, StringComparison.Ordinal);
        Assert.Contains("ManyToManyRelationships", path!, StringComparison.Ordinal);
        Assert.Contains("Privileges", path!, StringComparison.Ordinal);
    }

    [Fact]
    public void HasExpandedEntityMetadata_ReturnsTrueWhenAttributesPresent()
    {
        var method = typeof(YamlFirstPackaging).GetMethod("HasExpandedEntityMetadata", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var withAttributes = new JsonObject
        {
            ["Attributes"] = new JsonArray
            {
                new JsonObject { ["LogicalName"] = "fullname" },
            },
        };

        var withoutAttributes = new JsonObject
        {
            ["DisplayName"] = new JsonObject(),
        };

        var withResult = method!.Invoke(null, new object[] { withAttributes }) as bool?;
        var withoutResult = method.Invoke(null, new object[] { withoutAttributes }) as bool?;

        Assert.True(withResult.GetValueOrDefault());
        Assert.False(withoutResult.GetValueOrDefault());
    }

    [Fact]
    public void BuildDataverseBoundActionWadlXml_IncludesOnlyBoundActionsForTargetEntity()
    {
        var method = typeof(YamlFirstPackaging).GetMethod("BuildDataverseBoundActionWadlXml", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        const string metadataXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx""><edmx:DataServices>
<Schema Namespace=""Microsoft.Dynamics.CRM"" xmlns=""http://docs.oasis-open.org/odata/ns/edm""><Action Name=""SendCode"" IsBound=""true""><Parameter Name=""entity"" Type=""Microsoft.Dynamics.CRM.contact"" Nullable=""false"" /><Parameter Name=""Code"" Type=""Edm.String"" /><Parameter Name=""RetryCount"" Type=""Edm.Int32"" /></Action><Action Name=""AccountOnlyAction"" IsBound=""true""><Parameter Name=""entity"" Type=""Microsoft.Dynamics.CRM.account"" Nullable=""false"" /><Parameter Name=""AccountId"" Type=""Edm.Guid"" /></Action><Action Name=""UnboundAction""><Parameter Name=""Value"" Type=""Edm.String"" /></Action></Schema>
</edmx:DataServices></edmx:Edmx>";

        var wadlXml = method!.Invoke(
            null,
            new object[]
            {
                metadataXml,
                "https://contoso.crm.dynamics.com/api/data/v9.0",
                "contact",
                "contacts",
                "contactid",
            }) as string;

        Assert.False(string.IsNullOrWhiteSpace(wadlXml));
        Assert.Contains("/contacts({contactid})/Microsoft.Dynamics.CRM.SendCode", wadlXml!, StringComparison.Ordinal);
        Assert.Contains("<method siena:requiresAuthentication=\"true\" name=\"POST\" id=\"SendCode\"", wadlXml!, StringComparison.Ordinal);
        Assert.Contains("<param style=\"plain\" name=\"Code\" path=\"/Code\" type=\"xs:string\"", wadlXml!, StringComparison.Ordinal);
        Assert.Contains("<param style=\"plain\" name=\"RetryCount\" path=\"/RetryCount\" type=\"xs:int\"", wadlXml!, StringComparison.Ordinal);
        Assert.True(
            wadlXml!.Contains("<authenticationProviders", StringComparison.Ordinal)
            || wadlXml.Contains("<siena:authenticationProviders", StringComparison.Ordinal));
        Assert.True(
            wadlXml.Contains("<connectionProvider id=\"PowerAppAuth\" siena:connectionProviderId=\"contact\"", StringComparison.Ordinal)
            || wadlXml.Contains("<siena:connectionProvider id=\"PowerAppAuth\" siena:connectionProviderId=\"contact\"", StringComparison.Ordinal));
        Assert.True(
            wadlXml.Contains("<template", StringComparison.Ordinal)
            || wadlXml.Contains("<siena:template", StringComparison.Ordinal));
        Assert.DoesNotContain("AccountOnlyAction", wadlXml!, StringComparison.Ordinal);
        Assert.DoesNotContain("UnboundAction", wadlXml!, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateUniqueControlNames_AllowsDuplicateControlNamesAcrossDifferentComponents()
    {
        var topControlFiles = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
        var topComponentFiles = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase)
        {
            ["Components/100.json"] = Wrap(CreateTopParent("ComponentA", "Component", "TextInput1")),
            ["Components/200.json"] = Wrap(CreateTopParent("ComponentB", "Component", "TextInput1")),
        };

        var ex = Record.Exception(() => InvokeValidateUniqueControlNames(topControlFiles, topComponentFiles));
        Assert.Null(ex);
    }

    [Fact]
    public void ValidateUniqueControlNames_RejectsDuplicateNamesAcrossScreensIncludingScreenNames()
    {
        var topControlFiles = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase)
        {
            ["Controls/10.json"] = Wrap(CreateTopParent("ScreenA", "screen", "Label1")),
            ["Controls/20.json"] = Wrap(CreateTopParent("ScreenB", "screen", "ScreenA")),
        };

        var topComponentFiles = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);

        var ex = Assert.Throws<InvalidDataException>(() => InvokeValidateUniqueControlNames(topControlFiles, topComponentFiles));
        Assert.Contains("across screens", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ScreenA", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateUniqueControlNames_RejectsDuplicateControlNamesWithinSingleComponent()
    {
        var topControlFiles = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
        var topComponentFiles = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase)
        {
            ["Components/100.json"] = Wrap(CreateTopParent("ComponentA", "Component", "Label1", "Label1")),
        };

        var ex = Assert.Throws<InvalidDataException>(() => InvokeValidateUniqueControlNames(topControlFiles, topComponentFiles));
        Assert.Contains("within component", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Label1", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PackFromDirectory_InvalidScreenYaml_ThrowsInsteadOfSilentlySkipping()
    {
        var workDir = Path.Combine(Path.GetTempPath(), $"msapp-pack-validation-{Guid.NewGuid():N}");
        var outputPath = Path.Combine(Path.GetTempPath(), $"msapp-pack-validation-{Guid.NewGuid():N}.msapp");

        try
        {
            YamlFirstPackaging.InitEmptyAppDirectory(workDir, "Screen1");
            var badYamlPath = Path.Combine(workDir, "Src", "BrokenScreen.pa.yaml");
            File.WriteAllText(badYamlPath, "Screens:\n  - this is invalid for expected schema");

            var ex = Assert.Throws<InvalidDataException>(() => YamlFirstPackaging.PackFromDirectory(workDir, outputPath));
            Assert.Contains("BrokenScreen.pa.yaml", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(workDir))
            {
                Directory.Delete(workDir, true);
            }

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    [Fact]
    public void PackFromDirectory_ComponentYamlWithoutComponentDefinitions_Throws()
    {
        var workDir = Path.Combine(Path.GetTempPath(), $"msapp-pack-validation-{Guid.NewGuid():N}");
        var outputPath = Path.Combine(Path.GetTempPath(), $"msapp-pack-validation-{Guid.NewGuid():N}.msapp");

        try
        {
            YamlFirstPackaging.InitEmptyAppDirectory(workDir, "Screen1");

            var componentsDir = Path.Combine(workDir, "Src", "Components");
            Directory.CreateDirectory(componentsDir);
            var badYamlPath = Path.Combine(componentsDir, "BrokenComponent.pa.yaml");
            File.WriteAllText(badYamlPath, "Screens:\n  ScreenX:\n    Children: []\n");

            var ex = Assert.Throws<InvalidDataException>(() => YamlFirstPackaging.PackFromDirectory(workDir, outputPath));
            Assert.Contains("ComponentDefinitions", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("BrokenComponent.pa.yaml", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(workDir))
            {
                Directory.Delete(workDir, true);
            }

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    [Fact]
    public void PackFromDirectory_InlineControlProperties_ThrowsHelpfulError()
    {
        var workDir = Path.Combine(Path.GetTempPath(), $"msapp-pack-inlineprops-{Guid.NewGuid():N}");
        var outputPath = Path.Combine(Path.GetTempPath(), $"msapp-pack-inlineprops-{Guid.NewGuid():N}.msapp");

        try
        {
            YamlFirstPackaging.InitEmptyAppDirectory(workDir, "Screen1");

            var screen2Path = Path.Combine(workDir, "Src", "Screen2.pa.yaml");
            File.WriteAllText(screen2Path,
@"Screens:
  Screen2:
    Properties:
      LoadingSpinnerColor: =RGBA(56, 96, 178, 1)
    Children:
      - Label2:
          Control: Label@2.5.1
          Properties:
          Text: =""Screen 2""
          Fill: =RGBA(1, 2, 3, 1)
");

            var ex = Assert.Throws<InvalidDataException>(() => YamlFirstPackaging.PackFromDirectory(workDir, outputPath));
            Assert.Contains("Unrecognized control-level keys", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Text", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Properties", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(workDir))
            {
                Directory.Delete(workDir, true);
            }

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    [Fact]
    public void PackFromDirectory_LabelWithoutExplicitFill_DoesNotMaterializeStyleDefault()
    {
        var workDir = Path.Combine(Path.GetTempPath(), $"msapp-pack-label-fill-{Guid.NewGuid():N}");
        var outputPath = Path.Combine(Path.GetTempPath(), $"msapp-pack-label-fill-{Guid.NewGuid():N}.msapp");
        var unpackPath = Path.Combine(Path.GetTempPath(), $"msapp-pack-label-fill-unpack-{Guid.NewGuid():N}");

        try
        {
            YamlFirstPackaging.InitEmptyAppDirectory(workDir, "Screen1");

            var screen2Path = Path.Combine(workDir, "Src", "Screen2.pa.yaml");
            File.WriteAllText(screen2Path,
@"Screens:
  Screen2:
    Properties:
      LoadingSpinnerColor: =RGBA(56, 96, 178, 1)
    Children:
      - Label2:
          Control: Label@2.5.1
          Properties:
            BorderColor: =RGBA(0, 0, 0, 0)
            BorderStyle: =BorderStyle.None
            BorderThickness: =2
            Color: =RGBA(50, 49, 48, 1)
            DisabledBorderColor: =RGBA(0, 0, 0, 0)
            DisabledColor: =RGBA(161, 159, 157, 1)
            FocusedBorderThickness: =4
            Font: =Font.'Segoe UI'
            Text: =""Screen 1""
            X: =327
            Y: =223
");

            YamlFirstPackaging.PackFromDirectory(workDir, outputPath);

            ZipFile.ExtractToDirectory(outputPath, unpackPath);
            var screen2 = FindTopParentByName(unpackPath, "Screen2");
            var label2 = ((screen2["Children"] as JsonArray) ?? new JsonArray())
                .OfType<JsonObject>()
                .FirstOrDefault(c => string.Equals(c["Name"]?.GetValue<string>() ?? string.Empty, "Label2", StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(label2);

            // Studio only materializes rules for properties explicitly in YAML.
            // Fill is not in the YAML, so it should not appear in Rules or CPS.
            var fillRule = ((label2!["Rules"] as JsonArray) ?? new JsonArray())
                .OfType<JsonObject>()
                .FirstOrDefault(r => string.Equals(r["Property"]?.GetValue<string>() ?? string.Empty, "Fill", StringComparison.OrdinalIgnoreCase));

            Assert.Null(fillRule);

            var hasFillState = ((label2["ControlPropertyState"] as JsonArray) ?? new JsonArray())
                .Any(s => string.Equals((s as JsonValue)?.GetValue<string>() ?? string.Empty, "Fill", StringComparison.OrdinalIgnoreCase));

            Assert.False(hasFillState);
        }
        finally
        {
            if (Directory.Exists(workDir))
            {
                Directory.Delete(workDir, true);
            }

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            if (Directory.Exists(unpackPath))
            {
                Directory.Delete(unpackPath, true);
            }
        }
    }

    [Fact]
    public void PackFromDirectory_RefreshesLocalDatabaseReferencesFromDataSources()
    {
        var workDir = Path.Combine(Path.GetTempPath(), $"msapp-pack-localdbrefs-{Guid.NewGuid():N}");
        var outputPath = Path.Combine(Path.GetTempPath(), $"msapp-pack-localdbrefs-{Guid.NewGuid():N}.msapp");
        var unpackPath = Path.Combine(Path.GetTempPath(), $"msapp-pack-localdbrefs-unpack-{Guid.NewGuid():N}");

        try
        {
            YamlFirstPackaging.InitEmptyAppDirectory(workDir, "Screen1");

            var dataSourcesPath = Path.Combine(workDir, "References", "DataSources.json");
            var dataSourcesRoot = new JsonObject
            {
                ["DataSources"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["Name"] = "Accounts",
                        ["Type"] = "NativeCDSDataSourceInfo",
                        ["EntitySetName"] = "accounts",
                        ["LogicalName"] = "account",
                        ["TableDefinition"] = JsonValue.Create("{\"Views\":\"https://contoso.crm.dynamics.com/api/data/v9.2/savedqueries\"}"),
                    },
                    new JsonObject
                    {
                        ["Name"] = "Contacts",
                        ["Type"] = "NativeCDSDataSourceInfo",
                        ["EntitySetName"] = "contacts",
                        ["LogicalName"] = "contact",
                        ["TableDefinition"] = JsonValue.Create("{\"Views\":\"https://contoso.crm.dynamics.com/api/data/v9.2/savedqueries\"}"),
                    },
                },
            };
            File.WriteAllText(dataSourcesPath, dataSourcesRoot.ToJsonString());

            var propertiesPath = Path.Combine(workDir, "Properties.json");
            var properties = JsonNode.Parse(File.ReadAllText(propertiesPath)) as JsonObject;
            Assert.NotNull(properties);

            var staleLocalDbRefs = new JsonObject
            {
                ["default.cds"] = new JsonObject
                {
                    ["state"] = "Configured",
                    ["instanceUrl"] = "https://contoso.crm.dynamics.com/",
                    ["webApiVersion"] = "v9.0",
                    ["environmentVariableName"] = string.Empty,
                    ["dataSources"] = new JsonObject
                    {
                        ["Accounts"] = new JsonObject
                        {
                            ["entitySetName"] = "accounts",
                            ["logicalName"] = "account",
                        },
                    },
                },
            };

            properties!["LocalDatabaseReferences"] = staleLocalDbRefs.ToJsonString();
            File.WriteAllText(propertiesPath, properties.ToJsonString());

            YamlFirstPackaging.PackFromDirectory(workDir, outputPath);

            ZipFile.ExtractToDirectory(outputPath, unpackPath);
            var packedPropertiesPath = Path.Combine(unpackPath, "Properties.json");
            var packedProperties = JsonNode.Parse(File.ReadAllText(packedPropertiesPath)) as JsonObject;
            Assert.NotNull(packedProperties);

            var packedLocalDbRefsText = packedProperties!["LocalDatabaseReferences"]?.GetValue<string>() ?? string.Empty;
            var packedLocalDbRefs = JsonNode.Parse(packedLocalDbRefsText) as JsonObject;
            Assert.NotNull(packedLocalDbRefs);

            var packedDataSources = packedLocalDbRefs!["default.cds"]?["dataSources"] as JsonObject;
            Assert.NotNull(packedDataSources);
            Assert.NotNull(packedDataSources!["Accounts"]);
            Assert.NotNull(packedDataSources["Contacts"]);
        }
        finally
        {
            if (Directory.Exists(workDir))
            {
                Directory.Delete(workDir, true);
            }

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            if (Directory.Exists(unpackPath))
            {
                Directory.Delete(unpackPath, true);
            }
        }
    }

    [Fact]
    public async Task UpsertDataverseTableDataSourceAsync_DoesNotRestoreCachedEntryVerbatim()
    {
        var workDir = Path.Combine(Path.GetTempPath(), $"msapp-pack-restore-cache-{Guid.NewGuid():N}");

        try
        {
            YamlFirstPackaging.InitEmptyAppDirectory(workDir, "Screen1");

            var dataSourcesPath = Path.Combine(workDir, "References", "DataSources.json");
            var dataSourcesRoot = new JsonObject
            {
                ["DataSources"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["Name"] = "Accounts",
                        ["Type"] = "NativeCDSDataSourceInfo",
                        ["DatasetName"] = "default.cds",
                        ["EntitySetName"] = "accounts",
                        ["LogicalName"] = "account",
                        ["TableDefinition"] = JsonValue.Create("{}"),
                    },
                },
            };

            File.WriteAllText(dataSourcesPath, dataSourcesRoot.ToJsonString());

            var removed = YamlFirstPackaging.RemoveDataSource(workDir, "Accounts");
            Assert.True(removed);

            await Assert.ThrowsAnyAsync<Exception>(() =>
                YamlFirstPackaging.UpsertDataverseTableDataSourceAsync(
                    workDir,
                    "https://127.0.0.1:1",
                    "account",
                    "dummy-token"));

            var updatedRoot = JsonNode.Parse(File.ReadAllText(dataSourcesPath)) as JsonObject;
            var updatedSources = updatedRoot?["DataSources"] as JsonArray;
            Assert.NotNull(updatedSources);
            Assert.Empty(updatedSources!);
        }
        finally
        {
            if (Directory.Exists(workDir))
            {
                Directory.Delete(workDir, true);
            }
        }
    }

    [Fact]
    public async Task ResolvePowerAppsApiAccessTokenAsync_UsesProviderWithPublicCloudResource()
    {
        var method = typeof(YamlFirstPackaging).GetMethod("ResolvePowerAppsApiAccessTokenAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        string requestedResource = string.Empty;
        Func<string, Task<string>> provider = resource =>
        {
            requestedResource = resource;
            return Task.FromResult("powerapps-token");
        };

        var task = method!.Invoke(null, new object[] { "dataverse-token", provider }) as Task<string>;
        Assert.NotNull(task);

        var result = await task!;
        Assert.Equal("powerapps-token", result);
        Assert.Equal("https://service.powerapps.com/", requestedResource);
    }

    [Fact]
    public async Task ResolvePowerAppsApiAccessTokenAsync_FallsBackToDataverseTokenWhenProviderFails()
    {
        var method = typeof(YamlFirstPackaging).GetMethod("ResolvePowerAppsApiAccessTokenAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        Func<string, Task<string>> provider = _ => throw new InvalidOperationException("Token provider failed");

        var task = method!.Invoke(null, new object[] { "dataverse-token", provider }) as Task<string>;
        Assert.NotNull(task);

        var result = await task!;
        Assert.Equal("dataverse-token", result);
    }

    [Fact]
    public void PackFromDirectory_ClassicTextInput_AcceptsClassicPropertySet()
    {
        // Classic/TextInput@2.3.2 maps to the "text" template (http://microsoft.com/appmagic/text)
        // which has a different (larger) property set than the modern PCF TextInputCanvas.
        // Properties like Color, HoverColor, HoverFill, PressedFill, DisabledColor, DisabledFill,
        // FocusedBorderThickness and Size are valid for the classic text template but were
        // previously rejected because NormalizeControlType incorrectly mapped Classic/TextInput
        // to PowerApps_CoreControls_TextInputCanvas.
        var workDir = Path.Combine(Path.GetTempPath(), $"msapp-pack-classic-textinput-{Guid.NewGuid():N}");
        var outputPath = Path.Combine(Path.GetTempPath(), $"msapp-pack-classic-textinput-{Guid.NewGuid():N}.msapp");

        try
        {
            YamlFirstPackaging.InitEmptyAppDirectory(workDir, "Screen1");

            var screenPath = Path.Combine(workDir, "Src", "Screen2.pa.yaml");
            File.WriteAllText(screenPath,
@"Screens:
  Screen2:
    Properties:
      Fill: =RGBA(255, 255, 255, 1)
    Children:
      - SearchInput1:
          Control: Classic/TextInput@2.3.2
          Properties:
            BorderColor: =RGBA(0, 0, 0, 1)
            BorderStyle: =BorderStyle.None
            BorderThickness: =1
            Color: =RGBA(70, 68, 64, 1)
            Default: """"
            DisabledBorderColor: =ColorFade(Self.BorderColor, 40%)
            DisabledColor: =ColorFade(Self.Color, 40%)
            DisabledFill: =Self.Fill
            Fill: =RGBA(0, 0, 0, 0)
            FocusedBorderThickness: =3
            Font: =Font.'Open Sans'
            Height: =44
            HintText: =""Search""
            HoverColor: =Self.Color
            HoverFill: =RGBA(0, 0, 0, 0)
            PressedFill: =RGBA(0, 0, 0, 0)
            Size: =14
");

            // Should not throw — all listed properties are valid for Classic/TextInput
            YamlFirstPackaging.PackFromDirectory(workDir, outputPath);
        }
        finally
        {
            if (Directory.Exists(workDir))
            {
                Directory.Delete(workDir, true);
            }

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    [Fact]
    public void Roundtrip_ReferenceMsApp_PreservesTemplatesAndControlRulesInDetail()
    {
        var referencePath = GetFixedReferenceMsAppPath();

        var unpackDir = Path.Combine(Path.GetTempPath(), $"msapp-roundtrip-unpack-{Guid.NewGuid():N}");
        var repackedMsappPath = Path.Combine(Path.GetTempPath(), $"msapp-roundtrip-repacked-{Guid.NewGuid():N}.msapp");
        var repackedUnpackDir = Path.Combine(Path.GetTempPath(), $"msapp-roundtrip-repacked-unpack-{Guid.NewGuid():N}");

        try
        {
            YamlFirstPackaging.UnpackToDirectory(referencePath, unpackDir);
            YamlFirstPackaging.PackFromDirectory(unpackDir, repackedMsappPath);
            YamlFirstPackaging.UnpackToDirectory(repackedMsappPath, repackedUnpackDir);

            var templateDiff = CompareUsedTemplatesInDetail(unpackDir, repackedUnpackDir);
            Assert.True(string.IsNullOrWhiteSpace(templateDiff),
                $"Roundtrip changed References/Templates.json UsedTemplates:{Environment.NewLine}{templateDiff}");

            var controlDiff = CompareControlsAndRulesInDetail(unpackDir, repackedUnpackDir);
            Assert.True(string.IsNullOrWhiteSpace(controlDiff),
                $"Roundtrip changed control templates/rules/control-state:{Environment.NewLine}{controlDiff}");

            var dataSourceDiff = CompareDataSourceArtifactsInDetail(unpackDir, repackedUnpackDir);
            Assert.True(string.IsNullOrWhiteSpace(dataSourceDiff),
                $"Roundtrip changed data source-related artifacts:{Environment.NewLine}{dataSourceDiff}");

            var fullArtifactDiff = CompareAllArtifactsInDetail(unpackDir, repackedUnpackDir);
            Assert.True(string.IsNullOrWhiteSpace(fullArtifactDiff),
                $"Roundtrip changed control/other unpacked files:{Environment.NewLine}{fullArtifactDiff}");
        }
        finally
        {
            if (Directory.Exists(unpackDir))
            {
                Directory.Delete(unpackDir, true);
            }

            if (File.Exists(repackedMsappPath))
            {
                File.Delete(repackedMsappPath);
            }

            if (Directory.Exists(repackedUnpackDir))
            {
                Directory.Delete(repackedUnpackDir, true);
            }
        }
    }

    [Fact]
    public void BuildNewMsApp_FromReferenceScreenYaml_PreservesTemplatesAndControlRulesInDetail()
    {
        var referencePath = GetFixedReferenceMsAppPath();

        var referenceUnpackDir = Path.Combine(Path.GetTempPath(), $"msapp-reference-unpack-{Guid.NewGuid():N}");
        var yamlBuildDir = Path.Combine(Path.GetTempPath(), $"msapp-yaml-build-{Guid.NewGuid():N}");
        var rebuiltMsappPath = Path.Combine(Path.GetTempPath(), $"msapp-yaml-rebuilt-{Guid.NewGuid():N}.msapp");
        var rebuiltUnpackDir = Path.Combine(Path.GetTempPath(), $"msapp-yaml-rebuilt-unpack-{Guid.NewGuid():N}");

        try
        {
            YamlFirstPackaging.UnpackToDirectory(referencePath, referenceUnpackDir);
            CopyDirectory(referenceUnpackDir, yamlBuildDir);

            // Re-export screen YAML using our code so the round-trip tests our own
            // export fidelity (screen variant, icon variant, data-card variant names, etc.)
            // rather than relying on Studio-generated YAML which uses different
            // canonicalized variant names and omits certain fields.
            var doc = MsAppDocument.Load(referencePath);
            foreach (var screen in doc.GetScreens())
            {
                var yaml = doc.ExportScreenYaml(screen.Name);
                File.WriteAllText(
                    Path.Combine(yamlBuildDir, "Src", $"{screen.Name}.pa.yaml"),
                    yaml);
            }

            var controlsDir = Path.Combine(yamlBuildDir, "Controls");
            var removedScreens = 0;
            foreach (var controlFile in Directory.EnumerateFiles(controlsDir, "*.json", SearchOption.TopDirectoryOnly))
            {
                var root = JsonNode.Parse(File.ReadAllText(controlFile)) as JsonObject;
                var topName = root?["TopParent"]?["Name"]?.GetValue<string>() ?? string.Empty;
                if (!string.Equals(topName, "App", StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(controlFile);
                    removedScreens++;
                }
            }

            Assert.True(removedScreens > 0, "Expected at least one screen control JSON to be removed before YAML rebuild.");

            YamlFirstPackaging.PackFromDirectory(
                yamlBuildDir,
                rebuiltMsappPath,
                templateSourceMsappPaths: new[] { referencePath });
            YamlFirstPackaging.UnpackToDirectory(rebuiltMsappPath, rebuiltUnpackDir);

            var templateDiff = CompareUsedTemplatesInDetail(referenceUnpackDir, rebuiltUnpackDir);
            Assert.True(string.IsNullOrWhiteSpace(templateDiff),
                $"YAML rebuild changed References/Templates.json UsedTemplates:{Environment.NewLine}{templateDiff}");

            // When rebuilding from YAML with the reference .msapp as a template source,
            // CompareControlsAndRulesInDetail performs a full logical comparison of every
            // control from top-level screen down to the deepest nested child, validating:
            //  - Template name/version/ID, VariantName, Layout, MetaDataIDKey
            //  - IsFromScreenLayout, IsAutoGenerated, Parent, IsLocked
            //  - Rules (category, provider, script), CPS membership
            //  - Child order (ZIndex-stable), DynamicProperties, HasDynamicProperties
            // This covers Form/DataCard metadata, nested DataCard sub-controls
            // (DataCardKey, DataCardValue, ErrorMessage, StarVisible, etc.), gallery
            // templates, and all other auto-generated controls.
            //
            // The remaining exclusions represent genuine YAML-rebuild limitations:
            //  - StyleName: YAML does not include style names; resolution is histogram-based
            //  - RawSignatures: CoreSignature includes StyleName which differs; see above
            var controlDiff = CompareControlsAndRulesInDetail(
                referenceUnpackDir,
                rebuiltUnpackDir,
                compareStyleName: false,
                compareRawSignatures: false);
            Assert.True(string.IsNullOrWhiteSpace(controlDiff),
                $"YAML rebuild changed control templates/rules/control-state:{Environment.NewLine}{controlDiff}");

            var dataSourceDiff = CompareDataSourceArtifactsInDetail(referenceUnpackDir, rebuiltUnpackDir);
            Assert.True(string.IsNullOrWhiteSpace(dataSourceDiff),
                $"YAML rebuild changed data source-related artifacts:{Environment.NewLine}{dataSourceDiff}");

            // Controls/*.json raw bytes differ due to rule/state differences above.
            // Src/ YAML files are the source of truth (not compared here).
            // References/Templates.json entry ordering may change.
            var fullArtifactDiff = CompareAllArtifactsInDetail(referenceUnpackDir, rebuiltUnpackDir,
                excludePatterns: new[] { "Controls/", "Src/", "References/Templates.json" });
            Assert.True(string.IsNullOrWhiteSpace(fullArtifactDiff),
                $"YAML rebuild changed control/other unpacked files:{Environment.NewLine}{fullArtifactDiff}");
        }
        finally
        {
            if (Directory.Exists(referenceUnpackDir))
            {
                Directory.Delete(referenceUnpackDir, true);
            }

            if (Directory.Exists(yamlBuildDir))
            {
                Directory.Delete(yamlBuildDir, true);
            }

            if (File.Exists(rebuiltMsappPath))
            {
                File.Delete(rebuiltMsappPath);
            }

            if (Directory.Exists(rebuiltUnpackDir))
            {
                Directory.Delete(rebuiltUnpackDir, true);
            }
        }
    }

    /// <summary>
    /// Simulates the test.ps1 BuildNewMsApp flow using the cmdlet-level code paths:
    /// 1. Copy reference msapp
    /// 2. Export screen YAML via MsAppDocument.ExportScreenYaml (same as Get-DataverseMsAppScreen)
    /// 3. In a single ModifyMsApp call: remove all screen Controls JSON and YAML, then write back all
    ///    screen YAMLs. This simulates the cmdlet-level rebuild where screens are replaced from YAML.
    ///
    /// This validates that the cmdlet flow (ModifyMsApp with automatic template sourcing)
    /// produces the same fidelity as the unit test flow (PackFromDirectory with explicit
    /// templateSourceMsappPaths). Catches issues like MetaDataIDKey loss, CPS format
    /// degradation, and missing IsFromScreenLayout/IsAutoGenerated flags.
    /// </summary>
    [Fact]
    public void BuildNewMsApp_ViaCmdletModifyMsApp_PreservesControlDetailsLikeDirectPack()
    {
        var referencePath = GetFixedReferenceMsAppPath();

        var referenceUnpackDir = Path.Combine(Path.GetTempPath(), $"msapp-ref-unpack-cmdlet-{Guid.NewGuid():N}");
        var cmdletMsappPath = Path.Combine(Path.GetTempPath(), $"msapp-cmdlet-rebuilt-{Guid.NewGuid():N}.msapp");
        var cmdletUnpackDir = Path.Combine(Path.GetTempPath(), $"msapp-cmdlet-rebuilt-unpack-{Guid.NewGuid():N}");

        try
        {
            // Step 1: Copy reference msapp
            File.Copy(referencePath, cmdletMsappPath, overwrite: true);

            // Step 2: Export screen YAML using MsAppDocument (same as Get-DataverseMsAppScreen now does)
            var doc = MsAppDocument.Load(referencePath);
            var screenYamls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var screen in doc.GetScreens())
            {
                // ExportScreenYaml includes MetadataKey, IsLocked, Layout etc.
                screenYamls[screen.Name] = doc.ExportScreenYaml(screen.Name, includeHeader: true);
            }

            // Step 3: Single ModifyMsApp call that removes all screen Controls JSON + YAML
            // and writes back all screen YAMLs. The repack uses the pre-modification msapp
            // as a template source (automatic in ModifyMsApp).
            MsAppPackagingHelper.ModifyMsApp(cmdletMsappPath, unpackDir =>
            {
                // Remove all screen Controls/*.json (but keep App)
                var controlsDir = Path.Combine(unpackDir, "Controls");
                if (Directory.Exists(controlsDir))
                {
                    foreach (var controlFile in Directory.EnumerateFiles(controlsDir, "*.json"))
                    {
                        var json = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(controlFile)) as System.Text.Json.Nodes.JsonObject;
                        var topName = json?["TopParent"]?["Name"]?.GetValue<string>() ?? string.Empty;
                        var templateName = json?["TopParent"]?["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
                        if (string.Equals(templateName, "screen", StringComparison.OrdinalIgnoreCase))
                        {
                            File.Delete(controlFile);
                        }
                    }
                }

                // Remove all screen YAML files (but keep App.pa.yaml and _EditorState.pa.yaml)
                var srcDir = Path.Combine(unpackDir, "Src");
                if (Directory.Exists(srcDir))
                {
                    foreach (var yamlFile in Directory.EnumerateFiles(srcDir, "*.pa.yaml"))
                    {
                        var fileName = Path.GetFileName(yamlFile);
                        if (!string.Equals(fileName, "App.pa.yaml", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(fileName, "_EditorState.pa.yaml", StringComparison.OrdinalIgnoreCase))
                        {
                            File.Delete(yamlFile);
                        }
                    }
                }

                // Write back all screen YAMLs
                Directory.CreateDirectory(Path.Combine(unpackDir, "Src"));
                foreach (var kvp in screenYamls)
                {
                    var screenName = kvp.Key;
                    var yaml = kvp.Value;
                    var yamlPath = Path.Combine(unpackDir, "Src", $"{screenName}.pa.yaml");
                    File.WriteAllText(yamlPath, yaml, new System.Text.UTF8Encoding(false));
                }
            });

            // Unpack both for comparison
            YamlFirstPackaging.UnpackToDirectory(referencePath, referenceUnpackDir);
            YamlFirstPackaging.UnpackToDirectory(cmdletMsappPath, cmdletUnpackDir);

            var templateDiff = CompareUsedTemplatesInDetail(referenceUnpackDir, cmdletUnpackDir);
            Assert.True(string.IsNullOrWhiteSpace(templateDiff),
                $"Cmdlet-flow rebuild changed References/Templates.json UsedTemplates:{Environment.NewLine}{templateDiff}");

            // Use the same comparison parameters as the direct-pack test.
            // StyleName and RawSignatures are excluded for the same reasons.
            var controlDiff = CompareControlsAndRulesInDetail(
                referenceUnpackDir,
                cmdletUnpackDir,
                compareStyleName: false,
                compareRawSignatures: false);
            Assert.True(string.IsNullOrWhiteSpace(controlDiff),
                $"Cmdlet-flow rebuild changed control templates/rules/control-state:{Environment.NewLine}{controlDiff}");
        }
        finally
        {
            if (Directory.Exists(referenceUnpackDir))
            {
                Directory.Delete(referenceUnpackDir, true);
            }

            if (File.Exists(cmdletMsappPath))
            {
                File.Delete(cmdletMsappPath);
            }

            if (Directory.Exists(cmdletUnpackDir))
            {
                Directory.Delete(cmdletUnpackDir, true);
            }
        }
    }

    private static string GetFixedReferenceMsAppPath()
    {
        var repoRoot = FindRepositoryRoot();
        var referencePath = Path.Combine(repoRoot, "Rnwood.Dataverse.Data.PowerShell", "TestData", "AccountAppReference.msapp");
        Assert.True(File.Exists(referencePath),
            $"Reference .msapp file not found at fixed project path: {referencePath}");
        return referencePath;
    }

    private static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Rnwood.Dataverse.Data.PowerShell.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing Rnwood.Dataverse.Data.PowerShell.sln.");
    }

    /// <summary>Polyfill for Path.GetRelativePath which is unavailable on net462.</summary>
    private static string GetRelativePath(string basePath, string fullPath)
    {
        var baseUri = new Uri(basePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar);
        var fullUri = new Uri(fullPath);
        return Uri.UnescapeDataString(baseUri.MakeRelativeUri(fullUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relative = GetRelativePath(sourceDir, file);
            var destinationFile = Path.Combine(destinationDir, relative);
            var destinationParent = Path.GetDirectoryName(destinationFile);
            if (!string.IsNullOrWhiteSpace(destinationParent))
            {
                Directory.CreateDirectory(destinationParent);
            }

            File.Copy(file, destinationFile, true);
        }
    }

    private static string CompareUsedTemplatesInDetail(string leftUnpackedDir, string rightUnpackedDir)
    {
        var leftPath = Path.Combine(leftUnpackedDir, "References", "Templates.json");
        var rightPath = Path.Combine(rightUnpackedDir, "References", "Templates.json");

        var left = JsonNode.Parse(File.ReadAllText(leftPath)) as JsonObject;
        var right = JsonNode.Parse(File.ReadAllText(rightPath)) as JsonObject;

        var leftSet = ((left?["UsedTemplates"] as JsonArray) ?? new JsonArray())
            .OfType<JsonObject>()
            .Select(t => new
            {
                Name = t["Name"]?.GetValue<string>() ?? string.Empty,
                Version = t["Version"]?.GetValue<string>() ?? string.Empty,
                Id = t["Id"]?.GetValue<string>() ?? string.Empty,
            })
            .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(t => t.Version, StringComparer.OrdinalIgnoreCase)
            .ThenBy(t => t.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var rightSet = ((right?["UsedTemplates"] as JsonArray) ?? new JsonArray())
            .OfType<JsonObject>()
            .Select(t => new
            {
                Name = t["Name"]?.GetValue<string>() ?? string.Empty,
                Version = t["Version"]?.GetValue<string>() ?? string.Empty,
                Id = t["Id"]?.GetValue<string>() ?? string.Empty,
            })
            .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(t => t.Version, StringComparer.OrdinalIgnoreCase)
            .ThenBy(t => t.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var leftKeys = new HashSet<string>(
            leftSet.Select(t => $"{t.Name}|{t.Version}|{t.Id}"),
            StringComparer.OrdinalIgnoreCase);
        var rightKeys = new HashSet<string>(
            rightSet.Select(t => $"{t.Name}|{t.Version}|{t.Id}"),
            StringComparer.OrdinalIgnoreCase);

        var sb = new StringBuilder();
        foreach (var missing in leftKeys.Except(rightKeys, StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            sb.AppendLine($"- Missing in roundtrip: {missing}");
        }

        foreach (var extra in rightKeys.Except(leftKeys, StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            sb.AppendLine($"- Added in roundtrip: {extra}");
        }

        return sb.ToString();
    }

    private static string CompareControlsAndRulesInDetail(
        string leftUnpackedDir,
        string rightUnpackedDir,
        bool compareStyleName = true,
        bool compareRawSignatures = true,
        bool reportUnmatchedRules = true,
        bool compareControlPropertyState = true,
        bool compareVariantName = true,
        bool compareLayout = true,
        bool compareMetadataKey = true,
        bool compareChildOrder = true,
        bool compareIsFromScreenLayout = true,
        bool compareIsAutoGenerated = true,
        bool compareParent = true,
        bool compareIsLocked = true,
        IEnumerable<string>? ignoredDynamicPropertyNames = null)
    {
        var leftRoots = LoadTopParentRootsByName(leftUnpackedDir, "Controls")
            .Concat(LoadTopParentRootsByName(leftUnpackedDir, "Components"))
            .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
        var rightRoots = LoadTopParentRootsByName(rightUnpackedDir, "Controls")
            .Concat(LoadTopParentRootsByName(rightUnpackedDir, "Components"))
            .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

        var sb = new StringBuilder();
        var leftRootNames = leftRoots.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
        var rightRootNames = rightRoots.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();

        foreach (var missingRoot in leftRootNames.Except(rightRootNames, StringComparer.OrdinalIgnoreCase))
        {
            sb.AppendLine($"- Missing control root in roundtrip: {missingRoot}");
        }

        foreach (var extraRoot in rightRootNames.Except(leftRootNames, StringComparer.OrdinalIgnoreCase))
        {
            sb.AppendLine($"- Added control root in roundtrip: {extraRoot}");
        }

        foreach (var rootName in leftRootNames.Intersect(rightRootNames, StringComparer.OrdinalIgnoreCase))
        {
            var leftByPath = new Dictionary<string, ControlSnapshot>(StringComparer.OrdinalIgnoreCase);
            var rightByPath = new Dictionary<string, ControlSnapshot>(StringComparer.OrdinalIgnoreCase);

            CollectControlSnapshots(leftRoots[rootName], string.Empty, leftByPath);
            CollectControlSnapshots(rightRoots[rootName], string.Empty, rightByPath);

            var leftPaths = leftByPath.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
            var rightPaths = rightByPath.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();

            foreach (var missingPath in leftPaths.Except(rightPaths, StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine($"- [{rootName}] Missing control path in roundtrip: {missingPath}");
            }

            foreach (var extraPath in rightPaths.Except(leftPaths, StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine($"- [{rootName}] Added control path in roundtrip: {extraPath}");
            }

            foreach (var path in leftPaths.Intersect(rightPaths, StringComparer.OrdinalIgnoreCase))
            {
                var l = leftByPath[path];
                var r = rightByPath[path];

                if (!string.Equals(l.TemplateName, r.TemplateName, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(l.TemplateVersion, r.TemplateVersion, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(l.TemplateId, r.TemplateId, StringComparison.OrdinalIgnoreCase)
                    || (compareVariantName && !string.Equals(l.VariantName, r.VariantName, StringComparison.OrdinalIgnoreCase))
                    || (compareLayout && !string.Equals(l.Layout, r.Layout, StringComparison.OrdinalIgnoreCase))
                    || (compareMetadataKey && !string.Equals(l.MetadataKey, r.MetadataKey, StringComparison.OrdinalIgnoreCase))
                    || (compareStyleName && !string.Equals(l.StyleName, r.StyleName, StringComparison.OrdinalIgnoreCase))
                    || (compareChildOrder && !string.Equals(l.ChildOrderSignature, r.ChildOrderSignature, StringComparison.Ordinal))
                    || l.HasDynamicProperties != r.HasDynamicProperties)
                {
                    sb.AppendLine($"- [{rootName}] Template mismatch at '{path}': "
                        + $"left={l.TemplateName}@{l.TemplateVersion} id={l.TemplateId} variant={l.VariantName} layout={l.Layout} metadataKey={l.MetadataKey} style={l.StyleName} hasDynamic={l.HasDynamicProperties} childOrder={l.ChildOrderSignature}; "
                        + $"right={r.TemplateName}@{r.TemplateVersion} id={r.TemplateId} variant={r.VariantName} layout={r.Layout} metadataKey={r.MetadataKey} style={r.StyleName} hasDynamic={r.HasDynamicProperties} childOrder={r.ChildOrderSignature}");
                }

                if (compareIsFromScreenLayout && l.IsFromScreenLayout != r.IsFromScreenLayout)
                {
                    sb.AppendLine($"- [{rootName}] IsFromScreenLayout mismatch at '{path}': left={l.IsFromScreenLayout} right={r.IsFromScreenLayout}");
                }

                if (compareIsAutoGenerated && l.IsAutoGenerated != r.IsAutoGenerated)
                {
                    sb.AppendLine($"- [{rootName}] IsAutoGenerated mismatch at '{path}': left={l.IsAutoGenerated} right={r.IsAutoGenerated}");
                }

                if (compareParent && !string.Equals(l.Parent, r.Parent, StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine($"- [{rootName}] Parent mismatch at '{path}': left='{l.Parent}' right='{r.Parent}'");
                }

                if (compareIsLocked && l.IsLocked != r.IsLocked)
                {
                    sb.AppendLine($"- [{rootName}] IsLocked mismatch at '{path}': left={l.IsLocked} right={r.IsLocked}");
                }

                if (compareRawSignatures && !string.Equals(l.CoreSignature, r.CoreSignature, StringComparison.Ordinal))
                {
                    sb.AppendLine($"- [{rootName}] Control core JSON changed at '{path}': leftHash={ComputeStableHash(l.CoreSignature)} rightHash={ComputeStableHash(r.CoreSignature)}");
                }

                if (compareRawSignatures && !string.Equals(l.RulesSignature, r.RulesSignature, StringComparison.Ordinal))
                {
                    sb.AppendLine($"- [{rootName}] Rules JSON changed at '{path}': leftHash={ComputeStableHash(l.RulesSignature)} rightHash={ComputeStableHash(r.RulesSignature)}");
                }

                if (compareControlPropertyState && compareRawSignatures && !string.Equals(l.ControlPropertyStateSignature, r.ControlPropertyStateSignature, StringComparison.Ordinal))
                {
                    sb.AppendLine($"- [{rootName}] ControlPropertyState JSON changed at '{path}': leftHash={ComputeStableHash(l.ControlPropertyStateSignature)} rightHash={ComputeStableHash(r.ControlPropertyStateSignature)}");
                }

                if (compareRawSignatures && !string.Equals(l.DynamicPropertiesSignature, r.DynamicPropertiesSignature, StringComparison.Ordinal))
                {
                    sb.AppendLine($"- [{rootName}] DynamicProperties JSON changed at '{path}': leftHash={ComputeStableHash(l.DynamicPropertiesSignature)} rightHash={ComputeStableHash(r.DynamicPropertiesSignature)}");
                }

                AppendMapDiffs(sb, rootName, path, "Rule", l.Rules, r.Rules, skipUnmatched: !reportUnmatchedRules);
                if (compareControlPropertyState)
                {
                    // Compare CPS membership (added/missing properties) but skip
                    // position-only differences.  CPS position ordering is a
                    // non-functional presentation detail driven by complex template
                    // XML, auto-rule binding, and style interactions that cannot be
                    // fully replicated from YAML alone.
                    AppendMapDiffs(sb, rootName, path, "ControlPropertyState", l.ControlPropertyState, r.ControlPropertyState, skipValueDifferences: true);
                }

                var effectiveDynLeft = ignoredDynamicPropertyNames is null
                    ? l.DynamicProperties
                    : l.DynamicProperties
                        .Where(kv => !ignoredDynamicPropertyNames.Contains(kv.Key))
                        .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
                var effectiveDynRight = ignoredDynamicPropertyNames is null
                    ? r.DynamicProperties
                    : r.DynamicProperties
                        .Where(kv => !ignoredDynamicPropertyNames.Contains(kv.Key))
                        .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
                AppendMapDiffs(sb, rootName, path, "DynamicProperty", effectiveDynLeft, effectiveDynRight, skipUnmatched: !reportUnmatchedRules);
            }
        }

        return sb.ToString();
    }

    private static string CompareDataSourceArtifactsInDetail(string leftUnpackedDir, string rightUnpackedDir)
    {
        var sb = new StringBuilder();

        var leftDataSourcesPath = Path.Combine(leftUnpackedDir, "References", "DataSources.json");
        var rightDataSourcesPath = Path.Combine(rightUnpackedDir, "References", "DataSources.json");

        var leftDataSourcesText = File.Exists(leftDataSourcesPath) ? File.ReadAllText(leftDataSourcesPath) : string.Empty;
        var rightDataSourcesText = File.Exists(rightDataSourcesPath) ? File.ReadAllText(rightDataSourcesPath) : string.Empty;
        if (!string.Equals(leftDataSourcesText, rightDataSourcesText, StringComparison.Ordinal))
        {
            sb.AppendLine("- DataSources.json raw content changed");
        }

        var leftDataSourcesRoot = JsonNode.Parse(leftDataSourcesText) as JsonObject;
        var rightDataSourcesRoot = JsonNode.Parse(rightDataSourcesText) as JsonObject;

        var leftDataSources = ((leftDataSourcesRoot?["DataSources"] as JsonArray) ?? new JsonArray())
            .OfType<JsonObject>()
            .ToList();
        var rightDataSources = ((rightDataSourcesRoot?["DataSources"] as JsonArray) ?? new JsonArray())
            .OfType<JsonObject>()
            .ToList();

        var leftByKey = leftDataSources
            .ToDictionary(BuildDataSourceKey, ds => ds.ToJsonString(), StringComparer.OrdinalIgnoreCase);
        var rightByKey = rightDataSources
            .ToDictionary(BuildDataSourceKey, ds => ds.ToJsonString(), StringComparer.OrdinalIgnoreCase);

        foreach (var missing in leftByKey.Keys.Except(rightByKey.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            sb.AppendLine($"- DataSource missing in roundtrip: {missing}");
        }

        foreach (var extra in rightByKey.Keys.Except(leftByKey.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            sb.AppendLine($"- DataSource added in roundtrip: {extra}");
        }

        foreach (var key in leftByKey.Keys.Intersect(rightByKey.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            if (!string.Equals(leftByKey[key], rightByKey[key], StringComparison.Ordinal))
            {
                sb.AppendLine($"- DataSource changed: {key}");
            }
        }

        var leftOrdered = leftDataSources
            .Select(ds => new
            {
                Key = BuildDataSourceKey(ds),
                Canonical = CanonicalizeJson(ds),
            })
            .ToList();
        var rightOrdered = rightDataSources
            .Select(ds => new
            {
                Key = BuildDataSourceKey(ds),
                Canonical = CanonicalizeJson(ds),
            })
            .ToList();

        if (leftOrdered.Count != rightOrdered.Count)
        {
            sb.AppendLine($"- DataSource count changed: left={leftOrdered.Count} right={rightOrdered.Count}");
        }

        var compared = Math.Min(leftOrdered.Count, rightOrdered.Count);
        const int maxOrderDiffs = 50;
        var orderDiffs = 0;
        for (var i = 0; i < compared; i++)
        {
            if (!string.Equals(leftOrdered[i].Canonical, rightOrdered[i].Canonical, StringComparison.Ordinal))
            {
                sb.AppendLine($"- DataSource order/content changed at index {i}: left={leftOrdered[i].Key} right={rightOrdered[i].Key}");
                orderDiffs++;
                if (orderDiffs >= maxOrderDiffs)
                {
                    sb.AppendLine($"- Additional DataSource order/content differences truncated after {maxOrderDiffs} entries");
                    break;
                }
            }
        }

        var leftQualifiedValuesPath = Path.Combine(leftUnpackedDir, "References", "QualifiedValues.json");
        var rightQualifiedValuesPath = Path.Combine(rightUnpackedDir, "References", "QualifiedValues.json");
        var leftQualifiedValues = File.Exists(leftQualifiedValuesPath) ? File.ReadAllText(leftQualifiedValuesPath) : string.Empty;
        var rightQualifiedValues = File.Exists(rightQualifiedValuesPath) ? File.ReadAllText(rightQualifiedValuesPath) : string.Empty;
        if (!string.Equals(leftQualifiedValues, rightQualifiedValues, StringComparison.Ordinal))
        {
            sb.AppendLine("- QualifiedValues.json changed");
        }

        var leftPropertiesPath = Path.Combine(leftUnpackedDir, "Properties.json");
        var rightPropertiesPath = Path.Combine(rightUnpackedDir, "Properties.json");
        var leftProperties = JsonNode.Parse(File.ReadAllText(leftPropertiesPath)) as JsonObject;
        var rightProperties = JsonNode.Parse(File.ReadAllText(rightPropertiesPath)) as JsonObject;

        var leftLocalRefs = leftProperties?["LocalDatabaseReferences"]?.GetValue<string>() ?? string.Empty;
        var rightLocalRefs = rightProperties?["LocalDatabaseReferences"]?.GetValue<string>() ?? string.Empty;
        if (!string.Equals(leftLocalRefs, rightLocalRefs, StringComparison.Ordinal))
        {
            sb.AppendLine("- Properties.json LocalDatabaseReferences changed");
        }

        var leftFlags = (leftProperties?["AppPreviewFlagsMap"] as JsonObject)?.ToJsonString() ?? string.Empty;
        var rightFlags = (rightProperties?["AppPreviewFlagsMap"] as JsonObject)?.ToJsonString() ?? string.Empty;
        if (!string.Equals(leftFlags, rightFlags, StringComparison.Ordinal))
        {
            sb.AppendLine("- Properties.json AppPreviewFlagsMap changed");
        }

        return sb.ToString();
    }

    private static string CompareAllArtifactsInDetail(string leftUnpackedDir, string rightUnpackedDir, string[]? excludePatterns = null)
    {
        var sb = new StringBuilder();

        bool IsExcluded(string relativePath)
        {
            if (excludePatterns is null) return false;
            return excludePatterns.Any(pattern =>
                relativePath.StartsWith(pattern, StringComparison.OrdinalIgnoreCase)
                || string.Equals(relativePath, pattern, StringComparison.OrdinalIgnoreCase));
        }

        var leftFiles = Directory.EnumerateFiles(leftUnpackedDir, "*", SearchOption.AllDirectories)
            .Select(path => new
            {
                Absolute = path,
                Relative = GetRelativePath(leftUnpackedDir, path).Replace('\\', '/'),
            })
            .Where(x => !IsExcluded(x.Relative))
            .OrderBy(x => x.Relative, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var rightFiles = Directory.EnumerateFiles(rightUnpackedDir, "*", SearchOption.AllDirectories)
            .Select(path => new
            {
                Absolute = path,
                Relative = GetRelativePath(rightUnpackedDir, path).Replace('\\', '/'),
            })
            .Where(x => !IsExcluded(x.Relative))
            .OrderBy(x => x.Relative, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var leftByRelative = leftFiles.ToDictionary(x => x.Relative, x => x.Absolute, StringComparer.OrdinalIgnoreCase);
        var rightByRelative = rightFiles.ToDictionary(x => x.Relative, x => x.Absolute, StringComparer.OrdinalIgnoreCase);

        foreach (var missing in leftByRelative.Keys.Except(rightByRelative.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            sb.AppendLine($"- Missing file in roundtrip: {missing}");
        }

        foreach (var extra in rightByRelative.Keys.Except(leftByRelative.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            sb.AppendLine($"- Added file in roundtrip: {extra}");
        }

        const int maxFileDiffs = 100;
        var fileDiffs = 0;
        foreach (var rel in leftByRelative.Keys.Intersect(rightByRelative.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            var leftBytes = File.ReadAllBytes(leftByRelative[rel]);
            var rightBytes = File.ReadAllBytes(rightByRelative[rel]);
            if (leftBytes.SequenceEqual(rightBytes))
            {
                continue;
            }

            sb.AppendLine($"- Changed file content: {rel}");
            fileDiffs++;
            if (fileDiffs >= maxFileDiffs)
            {
                sb.AppendLine($"- Additional changed files truncated after {maxFileDiffs} entries");
                break;
            }
        }

        return sb.ToString();
    }

    private static string BuildDataSourceKey(JsonObject dataSource)
    {
        var name = dataSource["Name"]?.GetValue<string>() ?? string.Empty;
        var type = dataSource["Type"]?.GetValue<string>() ?? string.Empty;
        var datasetName = dataSource["DatasetName"]?.GetValue<string>() ?? string.Empty;
        var entitySetName = dataSource["EntitySetName"]?.GetValue<string>() ?? string.Empty;
        var logicalName = dataSource["LogicalName"]?.GetValue<string>() ?? string.Empty;
        return $"{name}|{type}|{datasetName}|{entitySetName}|{logicalName}";
    }

    private static Dictionary<string, JsonObject> LoadTopParentRootsByName(string unpackedDir, string folderName)
    {
        var controlsDir = Path.Combine(unpackedDir, folderName);
        var result = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(controlsDir))
        {
            return result;
        }

        foreach (var file in Directory.EnumerateFiles(controlsDir, "*.json", SearchOption.TopDirectoryOnly))
        {
            var doc = JsonNode.Parse(File.ReadAllText(file)) as JsonObject;
            var topParent = doc?["TopParent"] as JsonObject;
            if (topParent is null)
            {
                continue;
            }

            var name = topParent["Name"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            result[$"{folderName}/{name}"] = topParent;
        }

        return result;
    }

    private static void CollectControlSnapshots(JsonObject node, string parentPath, Dictionary<string, ControlSnapshot> sink)
    {
        var name = node["Name"]?.GetValue<string>() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var path = string.IsNullOrWhiteSpace(parentPath) ? name : $"{parentPath}/{name}";
        var template = node["Template"] as JsonObject;
        var templateName = template?["Name"]?.GetValue<string>() ?? string.Empty;
        var templateVersion = template?["Version"]?.GetValue<string>() ?? string.Empty;
        var templateId = template?["Id"]?.GetValue<string>() ?? string.Empty;
        var variantName = node["VariantName"]?.GetValue<string>() ?? string.Empty;
        var layout = node["LayoutName"]?.GetValue<string>() ?? string.Empty;
        var metadataKey = node["MetaDataIDKey"]?.GetValue<string>() ?? string.Empty;
        var styleName = node["StyleName"]?.GetValue<string>() ?? string.Empty;
        var hasDynamicProperties = (node["HasDynamicProperties"] as JsonValue)?.GetValue<bool>() ?? false;
        var isFromScreenLayout = (node["IsFromScreenLayout"] as JsonValue)?.GetValue<bool>() ?? false;
        var isAutoGenerated = (node["IsAutoGenerated"] as JsonValue)?.GetValue<bool>() ?? false;
        var parent = node["Parent"]?.GetValue<string>() ?? string.Empty;
        var isLocked = (node["IsLocked"] as JsonValue)?.GetValue<bool>() ?? false;

        // Sort children by their ZIndex value so the signature captures relative stacking order,
        // not absolute ZIndex values (which may differ between builds).
        var childOrderSignature = string.Join(",",
            ((node["Children"] as JsonArray) ?? new JsonArray())
                .OfType<JsonObject>()
                .OrderBy(c => GetZIndexValue(c))
                .ThenBy(c => c["Name"]?.GetValue<string>() ?? string.Empty)
                .Select(c => c["Name"]?.GetValue<string>() ?? string.Empty));

        var rules = ((node["Rules"] as JsonArray) ?? new JsonArray())
            .OfType<JsonObject>()
            .Where(r => !string.IsNullOrWhiteSpace(r["Property"]?.GetValue<string>() ?? string.Empty))
            .ToDictionary(
                r => r["Property"]!.GetValue<string>(),
                r => $"{(r["Category"]?.GetValue<string>() ?? string.Empty)}|{(r["RuleProviderType"]?.GetValue<string>() ?? string.Empty)}|{(r["InvariantScript"]?.GetValue<string>() ?? string.Empty)}",
                StringComparer.OrdinalIgnoreCase);

        var cps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var cpsArray = ((node["ControlPropertyState"] as JsonArray) ?? new JsonArray());
        for (var i = 0; i < cpsArray.Count; i++)
        {
            var stateName = GetControlPropertyStateName(cpsArray[i]);
            if (string.IsNullOrWhiteSpace(stateName) || cps.ContainsKey(stateName))
            {
                continue;
            }

            cps[stateName] = i.ToString();
        }

        var dynamicProperties = ((node["DynamicProperties"] as JsonArray) ?? new JsonArray())
            .OfType<JsonObject>()
            .Where(d => !string.IsNullOrWhiteSpace(d["PropertyName"]?.GetValue<string>() ?? string.Empty))
            .ToDictionary(
                d => d["PropertyName"]!.GetValue<string>(),
                d =>
                {
                    var rule = d["Rule"] as JsonObject;
                    return $"{(d["Type"]?.GetValue<string>() ?? string.Empty)}|{(d["PropertyType"]?.GetValue<string>() ?? string.Empty)}|{(rule?["InvariantScript"]?.GetValue<string>() ?? string.Empty)}";
                },
                StringComparer.OrdinalIgnoreCase);

        var coreSignature = CanonicalizeJson(BuildCoreControlNode(node));
        var rulesSignature = CanonicalizeJson(node["Rules"]);
        var controlPropertyStateSignature = CanonicalizeJson(node["ControlPropertyState"]);
        var dynamicPropertiesSignature = CanonicalizeJson(node["DynamicProperties"]);

        sink[path] = new ControlSnapshot(
            templateName,
            templateVersion,
            templateId,
            variantName,
            layout,
            metadataKey,
            styleName,
            hasDynamicProperties,
            isFromScreenLayout,
            isAutoGenerated,
            parent,
            isLocked,
            childOrderSignature,
            rules,
            cps,
            dynamicProperties,
            coreSignature,
            rulesSignature,
            controlPropertyStateSignature,
            dynamicPropertiesSignature);

        foreach (var child in ((node["Children"] as JsonArray) ?? new JsonArray()).OfType<JsonObject>())
        {
            CollectControlSnapshots(child, path, sink);
        }
    }

    private static void AppendMapDiffs(
        StringBuilder sb,
        string rootName,
        string path,
        string label,
        Dictionary<string, string> left,
        Dictionary<string, string> right,
        bool skipUnmatched = false,
        bool skipValueDifferences = false)
    {
        // ZIndex ordering is validated via ZIndex-sorted ChildOrderSignature; skip exact value comparison.
        var effectiveLeft = string.Equals(label, "Rule", StringComparison.OrdinalIgnoreCase)
            ? left.Where(kv => !string.Equals(kv.Key, "ZIndex", StringComparison.OrdinalIgnoreCase))
                  .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase)
            : left;
        var effectiveRight = string.Equals(label, "Rule", StringComparison.OrdinalIgnoreCase)
            ? right.Where(kv => !string.Equals(kv.Key, "ZIndex", StringComparison.OrdinalIgnoreCase))
                   .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase)
            : right;

        if (!skipUnmatched)
        {
            foreach (var missing in effectiveLeft.Keys.Except(effectiveRight.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine($"- [{rootName}] {label} missing at '{path}': {missing}='{effectiveLeft[missing]}'");
            }

            foreach (var extra in effectiveRight.Keys.Except(effectiveLeft.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine($"- [{rootName}] {label} added at '{path}': {extra}='{effectiveRight[extra]}'");
            }
        }

        foreach (var key in effectiveLeft.Keys.Intersect(effectiveRight.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            if (skipValueDifferences)
            {
                continue;
            }

            if (!string.Equals(effectiveLeft[key], effectiveRight[key], StringComparison.Ordinal))
            {
                sb.AppendLine($"- [{rootName}] {label} changed at '{path}': {key} left='{effectiveLeft[key]}' right='{effectiveRight[key]}'");
            }
        }
    }

    private static string GetControlPropertyStateName(JsonNode? stateEntry)
    {
        if (stateEntry is null)
        {
            return string.Empty;
        }

        if (stateEntry is JsonValue v)
        {
            if (v.TryGetValue<string>(out var stringValue) && !string.IsNullOrWhiteSpace(stringValue))
            {
                return stringValue;
            }

            var raw = v.ToJsonString().Trim();
            if (string.IsNullOrWhiteSpace(raw) || string.Equals(raw, "null", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            if (raw.Length >= 2 && raw[0] == '"' && raw[raw.Length - 1] == '"')
            {
                return raw.Substring(1, raw.Length - 2);
            }

            return raw;
        }

        if (stateEntry is JsonObject o)
        {
            return o["InvariantPropertyName"]?.GetValue<string>()
                   ?? o["Property"]?.GetValue<string>()
                   ?? string.Empty;
        }

        return string.Empty;
    }

    private static int GetZIndexValue(JsonObject control)
    {
        var zIndexRule = ((control["Rules"] as JsonArray) ?? new JsonArray())
            .OfType<JsonObject>()
            .FirstOrDefault(r => string.Equals(r["Property"]?.GetValue<string>(), "ZIndex", StringComparison.OrdinalIgnoreCase));
        if (zIndexRule is null)
        {
            return 0;
        }

        return int.TryParse(zIndexRule["InvariantScript"]?.GetValue<string>(), out var val) ? val : 0;
    }

    private sealed record ControlSnapshot(
        string TemplateName,
        string TemplateVersion,
        string TemplateId,
        string VariantName,
        string Layout,
        string MetadataKey,
        string StyleName,
        bool HasDynamicProperties,
        bool IsFromScreenLayout,
        bool IsAutoGenerated,
        string Parent,
        bool IsLocked,
        string ChildOrderSignature,
        Dictionary<string, string> Rules,
        Dictionary<string, string> ControlPropertyState,
        Dictionary<string, string> DynamicProperties,
        string CoreSignature,
        string RulesSignature,
        string ControlPropertyStateSignature,
        string DynamicPropertiesSignature);

    private static JsonObject BuildCoreControlNode(JsonObject node)
    {
        var result = new JsonObject();
        foreach (var kv in node)
        {
            if (string.Equals(kv.Key, "Children", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            result[kv.Key] = kv.Value?.DeepClone();
        }

        return result;
    }

    private static string CanonicalizeJson(JsonNode? node)
    {
        if (node is null)
        {
            return "null";
        }

        if (node is JsonObject obj)
        {
            var sorted = new JsonObject();
            foreach (var kv in obj.OrderBy(k => k.Key, StringComparer.Ordinal))
            {
                sorted[kv.Key] = kv.Value is null ? null : JsonNode.Parse(CanonicalizeJson(kv.Value));
            }

            return sorted.ToJsonString();
        }

        if (node is JsonArray arr)
        {
            var cloned = new JsonArray();
            foreach (var item in arr)
            {
                cloned.Add(item is null ? null : JsonNode.Parse(CanonicalizeJson(item)));
            }

            return cloned.ToJsonString();
        }

        return node.ToJsonString();
    }

    private static string ComputeStableHash(string value)
    {
        if (value is null)
        {
            return "00000000";
        }

        unchecked
        {
            var hash = 17;
            for (var i = 0; i < value.Length; i++)
            {
                hash = (hash * 31) + value[i];
            }

            return hash.ToString("x8");
        }
    }

    private static JsonObject? FindControlByPath(JsonObject node, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return node;
        }

        var sep = relativePath.IndexOf('/');
        var segment = sep < 0 ? relativePath : relativePath.Substring(0, sep);
        var rest = sep < 0 ? string.Empty : relativePath.Substring(sep + 1);

        var child = ((node["Children"] as JsonArray) ?? new JsonArray())
            .OfType<JsonObject>()
            .FirstOrDefault(c => string.Equals(
                c["Name"]?.GetValue<string>() ?? string.Empty,
                segment,
                StringComparison.OrdinalIgnoreCase));

        return child is null ? null : FindControlByPath(child, rest);
    }

    private static JsonObject FindTopParentByName(string unpackPath, string topParentName)
    {
        var controlsDir = Path.Combine(unpackPath, "Controls");
        foreach (var file in Directory.EnumerateFiles(controlsDir, "*.json", SearchOption.TopDirectoryOnly))
        {
            var doc = JsonNode.Parse(File.ReadAllText(file)) as JsonObject;
            var topParent = doc?["TopParent"] as JsonObject;
            var name = topParent?["Name"]?.GetValue<string>() ?? string.Empty;
            if (string.Equals(name, topParentName, StringComparison.OrdinalIgnoreCase))
            {
                return topParent!;
            }
        }

        throw new InvalidOperationException($"TopParent '{topParentName}' not found in unpacked controls.");
    }

    private static JsonObject Wrap(JsonObject topParent) => new() { ["TopParent"] = topParent };

    private static JsonObject CreateTopParent(string name, string templateName, params string[] childNames)
    {
        var children = new JsonArray();
        foreach (var childName in childNames)
        {
            children.Add(new JsonObject
            {
                ["Name"] = childName,
                ["Children"] = new JsonArray(),
            });
        }

        return new JsonObject
        {
            ["Name"] = name,
            ["Template"] = new JsonObject { ["Name"] = templateName },
            ["Children"] = children,
        };
    }

    private static void InvokeValidateUniqueControlNames(
        Dictionary<string, JsonObject> topControlFiles,
        Dictionary<string, JsonObject> topComponentFiles)
    {
        var method = typeof(YamlFirstPackaging).GetMethod("ValidateUniqueControlNames", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        try
        {
            method!.Invoke(null, new object[] { topControlFiles, topComponentFiles });
        }
        catch (TargetInvocationException tie) when (tie.InnerException is not null)
        {
            throw tie.InnerException;
        }
    }
}
