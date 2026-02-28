using MsAppToolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
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
    public void PackFromDirectory_LabelWithoutExplicitFill_UsesStyleDefaultFillRule()
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

            var fillRule = ((label2!["Rules"] as JsonArray) ?? new JsonArray())
                .OfType<JsonObject>()
                .FirstOrDefault(r => string.Equals(r["Property"]?.GetValue<string>() ?? string.Empty, "Fill", StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(fillRule);
            Assert.Equal("RGBA(0, 0, 0, 0)", fillRule!["InvariantScript"]?.GetValue<string>() ?? string.Empty);

            var hasFillState = ((label2["ControlPropertyState"] as JsonArray) ?? new JsonArray())
                .Any(s => string.Equals((s as JsonValue)?.GetValue<string>() ?? string.Empty, "Fill", StringComparison.OrdinalIgnoreCase));

            Assert.True(hasFillState);
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
