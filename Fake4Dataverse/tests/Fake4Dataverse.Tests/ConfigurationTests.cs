using System;
using System.IO;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class ConfigurationTests
    {
        [Fact]
        public void FromJson_AllOptionsSet_AppliesCorrectly()
        {
            var json = @"{
                ""AutoSetTimestamps"": false,
                ""AutoSetOwner"": false,
                ""AutoSetVersionNumber"": false,
                ""AutoSetStateCode"": false,
                ""ValidateWithMetadata"": true,
                ""EnforceSecurityRoles"": true,
                ""EnablePipeline"": false,
                ""EnableOperationLog"": false
            }";

            var options = FakeOrganizationServiceOptions.FromJson(json);

            Assert.False(options.AutoSetTimestamps);
            Assert.False(options.AutoSetOwner);
            Assert.False(options.AutoSetVersionNumber);
            Assert.False(options.AutoSetStateCode);
            Assert.True(options.ValidateWithMetadata);
            Assert.True(options.EnforceSecurityRoles);
            Assert.False(options.EnablePipeline);
            Assert.False(options.EnableOperationLog);
        }

        [Fact]
        public void FromJson_PartialOptions_KeepsDefaults()
        {
            var json = @"{ ""AutoSetTimestamps"": false }";

            var options = FakeOrganizationServiceOptions.FromJson(json);

            Assert.False(options.AutoSetTimestamps);
            // Defaults preserved
            Assert.True(options.AutoSetOwner);
            Assert.True(options.EnablePipeline);
        }

        [Fact]
        public void FromJson_CaseInsensitive_Works()
        {
            var json = @"{ ""autosettimestamps"": false, ""ENABLEPIPELINE"": false }";

            var options = FakeOrganizationServiceOptions.FromJson(json);

            Assert.False(options.AutoSetTimestamps);
            Assert.False(options.EnablePipeline);
        }

        [Fact]
        public void FromJson_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => FakeOrganizationServiceOptions.FromJson(null!));
        }

        [Fact]
        public void FromJsonFile_ValidFile_LoadsOptions()
        {
            var tempPath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempPath, @"{ ""AutoSetTimestamps"": false, ""ValidateWithMetadata"": true }");

                var options = FakeOrganizationServiceOptions.FromJsonFile(tempPath);

                Assert.False(options.AutoSetTimestamps);
                Assert.True(options.ValidateWithMetadata);
            }
            finally
            {
                File.Delete(tempPath);
            }
        }

        [Fact]
        public void FromEnvironment_SetsOptionsFromEnvVars()
        {
            var originalTimestamps = Environment.GetEnvironmentVariable("FAKE4DATAVERSE_AUTOSETTIMESTAMPS");
            var originalPipeline = Environment.GetEnvironmentVariable("FAKE4DATAVERSE_ENABLEPIPELINE");

            try
            {
                Environment.SetEnvironmentVariable("FAKE4DATAVERSE_AUTOSETTIMESTAMPS", "false");
                Environment.SetEnvironmentVariable("FAKE4DATAVERSE_ENABLEPIPELINE", "false");

                var options = FakeOrganizationServiceOptions.FromEnvironment();

                Assert.False(options.AutoSetTimestamps);
                Assert.False(options.EnablePipeline);
                // Non-set env vars should keep defaults
                Assert.True(options.AutoSetOwner);
            }
            finally
            {
                Environment.SetEnvironmentVariable("FAKE4DATAVERSE_AUTOSETTIMESTAMPS", originalTimestamps);
                Environment.SetEnvironmentVariable("FAKE4DATAVERSE_ENABLEPIPELINE", originalPipeline);
            }
        }

        [Fact]
        public void FromEnvironment_NoEnvVars_ReturnsDefaults()
        {
            // Make sure relevant env vars are not set
            var original = Environment.GetEnvironmentVariable("FAKE4DATAVERSE_AUTOSETTIMESTAMPS");
            try
            {
                Environment.SetEnvironmentVariable("FAKE4DATAVERSE_AUTOSETTIMESTAMPS", null);

                var options = FakeOrganizationServiceOptions.FromEnvironment();

                Assert.True(options.AutoSetTimestamps);
                Assert.True(options.AutoSetOwner);
                Assert.True(options.EnablePipeline);
            }
            finally
            {
                Environment.SetEnvironmentVariable("FAKE4DATAVERSE_AUTOSETTIMESTAMPS", original);
            }
        }

        [Fact]
        public void FromEnvironment_InvalidBoolValue_IgnoredKeepsDefault()
        {
            var original = Environment.GetEnvironmentVariable("FAKE4DATAVERSE_AUTOSETTIMESTAMPS");
            try
            {
                Environment.SetEnvironmentVariable("FAKE4DATAVERSE_AUTOSETTIMESTAMPS", "notabool");

                var options = FakeOrganizationServiceOptions.FromEnvironment();

                Assert.True(options.AutoSetTimestamps); // keeps default
            }
            finally
            {
                Environment.SetEnvironmentVariable("FAKE4DATAVERSE_AUTOSETTIMESTAMPS", original);
            }
        }
    }
}
