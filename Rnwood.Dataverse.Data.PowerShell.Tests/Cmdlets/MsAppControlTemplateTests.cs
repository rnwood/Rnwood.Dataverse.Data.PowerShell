using MsAppToolkit;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    public class MsAppControlTemplateTests
    {
        [Fact]
        public void ListControlTemplates_WithAugmentedMsapp_ReturnsLatestVersionForDuplicateTemplate()
        {
            // Arrange
            var embeddedTemplates = YamlFirstPackaging.ListEmbeddedControlTemplates();
            Assert.NotEmpty(embeddedTemplates);

            var templateName = embeddedTemplates[0].Name;
            var augmentedVersion = "999.0.0";
            var templateId = embeddedTemplates[0].TemplateId;
            var augmentedXml = BuildTemplateXml(templateId, "AugmentedProperty");

            var msappPath = CreateMsappWithTemplate(templateName, augmentedVersion, templateId, augmentedXml);

            try
            {
                // Act
                var templates = YamlFirstPackaging.ListControlTemplates(msappPath);

                // Assert
                var selected = Assert.Single(templates, t => string.Equals(t.Name, templateName, StringComparison.OrdinalIgnoreCase));
                Assert.Equal(augmentedVersion, selected.Version);
            }
            finally
            {
                if (File.Exists(msappPath))
                {
                    File.Delete(msappPath);
                }
            }
        }

        [Fact]
        public void DescribeTemplate_WithAugmentedMsappWithoutExplicitVersion_ReturnsLatestTemplateDetails()
        {
            // Arrange
            var embeddedTemplates = YamlFirstPackaging.ListEmbeddedControlTemplates();
            Assert.NotEmpty(embeddedTemplates);

            var templateName = embeddedTemplates[0].Name;
            var augmentedVersion = "999.1.0";
            var templateId = embeddedTemplates[0].TemplateId;
            var augmentedPropertyName = "AugmentedProperty";
            var augmentedXml = BuildTemplateXml(templateId, augmentedPropertyName);

            var msappPath = CreateMsappWithTemplate(templateName, augmentedVersion, templateId, augmentedXml);

            try
            {
                // Act
                var details = YamlFirstPackaging.DescribeTemplate(templateName, null, msappPath);

                // Assert
                Assert.Equal(templateName, details.Name, ignoreCase: true);
                Assert.Equal(augmentedVersion, details.Version);
                Assert.Contains(details.Properties, p => string.Equals(p.PropertyName, augmentedPropertyName, StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                if (File.Exists(msappPath))
                {
                    File.Delete(msappPath);
                }
            }
        }

        private static string CreateMsappWithTemplate(string templateName, string version, string templateId, string templateXml)
        {
            var msappPath = Path.Combine(Path.GetTempPath(), $"msapp-template-test-{Guid.NewGuid():N}.msapp");

            using (var archive = ZipFile.Open(msappPath, ZipArchiveMode.Create))
            {
                var templatesJson = $"{{\n  \"UsedTemplates\": [\n    {{\n      \"Name\": \"{EscapeJson(templateName)}\",\n      \"Id\": \"{EscapeJson(templateId)}\",\n      \"Version\": \"{EscapeJson(version)}\",\n      \"Template\": {ToJsonString(templateXml)}\n    }}\n  ]\n}}";

                var entry = archive.CreateEntry("References/Templates.json");
                using (var stream = entry.Open())
                {
                    var bytes = Encoding.UTF8.GetBytes(templatesJson);
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            return msappPath;
        }

        private static string BuildTemplateXml(string templateId, string propertyName)
        {
            return $"<control id=\"{templateId}\"><property name=\"{propertyName}\" defaultValue=\"1\" datatype=\"Number\" /></control>";
        }

        private static string EscapeJson(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string ToJsonString(string value)
        {
            return "\"" + EscapeJson(value).Replace("\r", "\\r").Replace("\n", "\\n") + "\"";
        }
    }
}
