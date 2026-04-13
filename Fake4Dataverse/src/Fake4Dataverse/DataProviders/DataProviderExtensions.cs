using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.DataProviders
{
    /// <summary>
    /// Provides methods to seed a <see cref="FakeDataverseEnvironment"/> from external data sources.
    /// </summary>
    public static class DataProviderExtensions
    {
        /// <summary>
        /// Seeds the environment from a JSON file containing an array of entity objects.
        /// Each object must have a <c>logicalname</c> property and optionally an <c>id</c> property.
        /// </summary>
        public static void SeedFromJsonFile(this FakeDataverseEnvironment environment, string filePath)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            var json = File.ReadAllText(filePath);
            SeedFromJsonString(environment, json);
        }

        /// <summary>
        /// Seeds the environment from a JSON string containing an array of entity objects.
        /// </summary>
        public static void SeedFromJsonString(this FakeDataverseEnvironment environment, string json)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (json == null) throw new ArgumentNullException(nameof(json));

            using (var doc = JsonDocument.Parse(json))
            {
                var entities = new List<Entity>();
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    var logicalName = element.GetProperty("logicalname").GetString()
                        ?? throw new InvalidOperationException("Entity must have a 'logicalname' property.");

                    var entity = new Entity(logicalName);

                    if (element.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                    {
                        entity.Id = Guid.Parse(idProp.GetString()!);
                    }

                    foreach (var prop in element.EnumerateObject())
                    {
                        if (string.Equals(prop.Name, "logicalname", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(prop.Name, "id", StringComparison.OrdinalIgnoreCase))
                            continue;

                        entity[prop.Name] = ConvertJsonValue(prop.Value);
                    }

                    entities.Add(entity);
                }

                environment.Seed(entities.ToArray());
            }
        }

        /// <summary>
        /// Seeds the environment from a CSV file. The first row must be headers.
        /// The first column must be <c>logicalname</c>.
        /// </summary>
        public static void SeedFromCsvFile(this FakeDataverseEnvironment environment, string filePath)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            var csv = File.ReadAllText(filePath);
            environment.SeedFromCsv(csv);
        }

        private static object? ConvertJsonValue(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    if (Guid.TryParse(element.GetString(), out var guid))
                        return guid;
                    if (DateTime.TryParse(element.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        return dt;
                    return element.GetString();

                case JsonValueKind.Number:
                    if (element.TryGetInt32(out var intVal))
                        return intVal;
                    if (element.TryGetInt64(out var longVal))
                        return longVal;
                    return element.GetDecimal();

                case JsonValueKind.True:
                    return true;

                case JsonValueKind.False:
                    return false;

                case JsonValueKind.Null:
                    return null;

                default:
                    return element.GetRawText();
            }
        }
    }
}
