using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.Json;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.PacProfileParsing
{
    /// <summary>
    /// Handles parsing of PAC CLI authentication profiles.
    /// </summary>
    internal static class PacProfileParser
    {
        /// <summary>
        /// Gets the environment URL from PAC CLI profiles.
        /// </summary>
        /// <param name="profile">Optional name of the profile to use. If null, uses the first profile.</param>
        /// <returns>The environment URL string.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the PAC profiles file is not found.</exception>
        /// <exception cref="InvalidDataException">Thrown when the profiles file cannot be parsed or no valid profile is found.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the selected profile does not have an environment URL.</exception>
        public static string GetEnvironmentUrl(string profile = null)
        {
            // Read PAC CLI profiles
            var profilesPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft",
                "PowerAppsCLI",
                "authprofiles_v2.json");

            if (!File.Exists(profilesPath))
            {
                throw new FileNotFoundException($"PAC CLI profiles file not found at: {profilesPath}. Please run 'pac auth create' first to authenticate with PAC CLI.", profilesPath);
            }

            // Read and parse the profiles JSON
            string json = File.ReadAllText(profilesPath);
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            PacProfilesData pacProfiles;
            try
            {
                pacProfiles = JsonSerializer.Deserialize<PacProfilesData>(json, jsonOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Failed to parse PAC CLI profiles file: {ex.Message}", ex);
            }

            if (pacProfiles == null || pacProfiles.Profiles == null || pacProfiles.Profiles.Count == 0)
            {
                throw new InvalidOperationException("No profiles found in PAC CLI. Please run 'pac auth create' first to authenticate with PAC CLI.");
            }

            // Find the profile to use
            PacProfileData profileData = null;
            if (!string.IsNullOrEmpty(profile))
            {
                // First try match by name
                profileData = pacProfiles.Profiles.FirstOrDefault(p =>
                 !string.IsNullOrEmpty(p.Name) &&
                 string.Equals(p.Name, profile, StringComparison.OrdinalIgnoreCase));

                if (profileData == null)
                {
                    // Try match by index
                    if (int.TryParse(profile, out int index) && index >= 0 && index < pacProfiles.Profiles.Count)
                    {
                        profileData = pacProfiles.Profiles[index];
                    }
                    else
                    {
                        var availableProfiles = string.Join(", ", pacProfiles.Profiles.Select((p, idx) => $"[{idx}] {p.Name ?? "Unnamed"} ({(!string.IsNullOrEmpty(p.Resource) && p.Resource != "https://service.powerapps.com/" ? p.Resource  : "no environment selected")})"));
                        throw new InvalidOperationException($"PAC CLI profile '{profile}' not found. Available profiles: {availableProfiles}");
                    }
                }
            }
            else
            {

                if (!pacProfiles.Current.TryGetValue("UNIVERSAL", out profileData))
                {
                    throw new InvalidOperationException("No current PAC CLI profile found.");
                }
            }

            string environmentUrlString = profileData.Resource;

            if (string.IsNullOrEmpty(environmentUrlString) || environmentUrlString == "https://service.powerapps.com/")
            {
                throw new InvalidOperationException($"The selected PAC CLI profile does not have an active environment URL. Please select an environment with 'pac env select'.");
            }

            return environmentUrlString;
        }
    }

    // Simple classes for parsing PAC CLI profile JSON without depending on bolt.authentication types
    internal class PacProfileData
    {
        // PAC CLI formats vary over versions. Name may be a simple string. Older formats wrapped it in an object.
        public string Name { get; set; }


        // Some formats expose Resource directly on the profile
        public string Resource { get; set; }

        public string FriendlyName { get; set; }
        public string OrganizationUniqueName { get; set; }
    }

    internal class PacProfilesData
    {
        public List<PacProfileData> Profiles { get; set; }
        public IDictionary<string, PacProfileData> Current { get; set; }
    }
}