using System;
using System.IO;
using System.Text.Json;

namespace Fake4Dataverse
{
    /// <summary>
    /// Configuration options for <see cref="FakeOrganizationService"/> controlling
    /// which automatic behaviors are enabled during Create, Update, and other operations.
    /// </summary>
    public sealed class FakeOrganizationServiceOptions
    {
        /// <summary>
        /// Gets or sets whether <c>createdon</c> and <c>modifiedon</c> are automatically set.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool AutoSetTimestamps { get; set; } = true;

        /// <summary>
        /// Gets or sets whether <c>ownerid</c>, <c>createdby</c>, and <c>modifiedby</c> are automatically set.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool AutoSetOwner { get; set; } = true;

        /// <summary>
        /// Gets or sets whether <c>versionnumber</c> is automatically set and incremented.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool AutoSetVersionNumber { get; set; } = true;

        /// <summary>
        /// Gets or sets whether <c>statecode</c> and <c>statuscode</c> are automatically set on Create.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool AutoSetStateCode { get; set; } = true;

        /// <summary>
        /// Gets or sets whether metadata-based validation is applied on Create and Update.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool ValidateWithMetadata { get; set; }

        /// <summary>
        /// Gets or sets whether security role checks are enforced.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool EnforceSecurityRoles { get; set; }

        /// <summary>
        /// Gets or sets whether the plugin-like pipeline is enabled.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool EnablePipeline { get; set; } = true;

        /// <summary>
        /// Gets or sets whether operations are recorded in the <see cref="OperationLog"/>.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool EnableOperationLog { get; set; } = true;

        /// <summary>
        /// Creates a <see cref="FakeOrganizationServiceOptions"/> preset that validates like real Dataverse,
        /// with metadata validation and security role enforcement enabled.
        /// All auto-set behaviors remain on.
        /// </summary>
        public static FakeOrganizationServiceOptions Strict => new FakeOrganizationServiceOptions
        {
            ValidateWithMetadata = true,
            EnforceSecurityRoles = true,
        };

        /// <summary>
        /// Creates a <see cref="FakeOrganizationServiceOptions"/> preset with all automatic behaviors disabled.
        /// Useful for tests that need full control over entity state.
        /// </summary>
        public static FakeOrganizationServiceOptions Lenient => new FakeOrganizationServiceOptions
        {
            AutoSetTimestamps = false,
            AutoSetOwner = false,
            AutoSetVersionNumber = false,
            AutoSetStateCode = false,
            ValidateWithMetadata = false,
            EnforceSecurityRoles = false,
            EnablePipeline = false,
            EnableOperationLog = false,
        };

        /// <summary>
        /// Loads options from a JSON string. Property names match the option property names (case-insensitive).
        /// Unspecified properties keep their default values.
        /// </summary>
        /// <param name="json">JSON object with option values, e.g. <c>{"AutoSetTimestamps": false}</c>.</param>
        /// <returns>A configured <see cref="FakeOrganizationServiceOptions"/> instance.</returns>
        public static FakeOrganizationServiceOptions FromJson(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            var options = new FakeOrganizationServiceOptions();
            using var doc = JsonDocument.Parse(json);
            ApplyJsonElement(options, doc.RootElement);
            return options;
        }

        /// <summary>
        /// Loads options from a JSON file. Property names match the option property names (case-insensitive).
        /// Unspecified properties keep their default values.
        /// </summary>
        /// <param name="path">Path to a JSON configuration file.</param>
        /// <returns>A configured <see cref="FakeOrganizationServiceOptions"/> instance.</returns>
        public static FakeOrganizationServiceOptions FromJsonFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            var json = File.ReadAllText(path);
            return FromJson(json);
        }

        /// <summary>
        /// Creates options from environment variables. Each option maps to an environment variable
        /// prefixed with <c>FAKE4DATAVERSE_</c> (e.g. <c>FAKE4DATAVERSE_AUTOSETTIMESTAMPS=false</c>).
        /// Only variables that are set will override the default values.
        /// </summary>
        /// <returns>A configured <see cref="FakeOrganizationServiceOptions"/> instance.</returns>
        public static FakeOrganizationServiceOptions FromEnvironment()
        {
            var options = new FakeOrganizationServiceOptions();

            ApplyEnvBool("FAKE4DATAVERSE_AUTOSETTIMESTAMPS", v => options.AutoSetTimestamps = v);
            ApplyEnvBool("FAKE4DATAVERSE_AUTOSETOWNER", v => options.AutoSetOwner = v);
            ApplyEnvBool("FAKE4DATAVERSE_AUTOSETVERSIONNUMBER", v => options.AutoSetVersionNumber = v);
            ApplyEnvBool("FAKE4DATAVERSE_AUTOSETSTATECODE", v => options.AutoSetStateCode = v);
            ApplyEnvBool("FAKE4DATAVERSE_VALIDATEWITHMETADATA", v => options.ValidateWithMetadata = v);
            ApplyEnvBool("FAKE4DATAVERSE_ENFORCESECURITYROLES", v => options.EnforceSecurityRoles = v);
            ApplyEnvBool("FAKE4DATAVERSE_ENABLEPIPELINE", v => options.EnablePipeline = v);
            ApplyEnvBool("FAKE4DATAVERSE_ENABLEOPERATIONLOG", v => options.EnableOperationLog = v);

            return options;
        }

        private static void ApplyEnvBool(string variable, Action<bool> setter)
        {
            var value = Environment.GetEnvironmentVariable(variable);
            if (value != null && bool.TryParse(value, out var parsed))
                setter(parsed);
        }

        private static void ApplyJsonElement(FakeOrganizationServiceOptions options, JsonElement root)
        {
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Value.ValueKind != JsonValueKind.True && prop.Value.ValueKind != JsonValueKind.False)
                    continue;

                var val = prop.Value.GetBoolean();
                if (string.Equals(prop.Name, nameof(AutoSetTimestamps), StringComparison.OrdinalIgnoreCase))
                    options.AutoSetTimestamps = val;
                else if (string.Equals(prop.Name, nameof(AutoSetOwner), StringComparison.OrdinalIgnoreCase))
                    options.AutoSetOwner = val;
                else if (string.Equals(prop.Name, nameof(AutoSetVersionNumber), StringComparison.OrdinalIgnoreCase))
                    options.AutoSetVersionNumber = val;
                else if (string.Equals(prop.Name, nameof(AutoSetStateCode), StringComparison.OrdinalIgnoreCase))
                    options.AutoSetStateCode = val;
                else if (string.Equals(prop.Name, nameof(ValidateWithMetadata), StringComparison.OrdinalIgnoreCase))
                    options.ValidateWithMetadata = val;
                else if (string.Equals(prop.Name, nameof(EnforceSecurityRoles), StringComparison.OrdinalIgnoreCase))
                    options.EnforceSecurityRoles = val;
                else if (string.Equals(prop.Name, nameof(EnablePipeline), StringComparison.OrdinalIgnoreCase))
                    options.EnablePipeline = val;
                else if (string.Equals(prop.Name, nameof(EnableOperationLog), StringComparison.OrdinalIgnoreCase))
                    options.EnableOperationLog = val;
            }
        }
    }
}
