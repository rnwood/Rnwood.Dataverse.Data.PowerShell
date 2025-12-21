using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Text.Json;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model
{
    /// <summary>
    /// Extracts solution components from a Dataverse environment.
    /// </summary>
    public class EnvironmentComponentExtractor : IComponentExtractor
    {
        private readonly ServiceClient _connection;
        private readonly PSCmdlet _cmdlet;
        private readonly Guid _solutionId;
        private bool? _isManagedSolution;

        /// <summary>
        /// Initializes a new instance of the EnvironmentComponentExtractor class.
        /// </summary>
        public EnvironmentComponentExtractor(ServiceClient connection, PSCmdlet cmdlet, Guid solutionId)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _cmdlet = cmdlet ?? throw new ArgumentNullException(nameof(cmdlet));
            _solutionId = solutionId;
        }

        /// <summary>
        /// Gets a value indicating whether the solution is managed.
        /// </summary>
        public bool? IsManagedSolution
        {
            get
            {
                if (!_isManagedSolution.HasValue)
                {
                    QuerySolutionManagedStatus();
                }
                return _isManagedSolution;
            }
        }

        /// <summary>
        /// Queries and caches the managed status of the solution.
        /// </summary>
        private void QuerySolutionManagedStatus()
        {
            var query = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet("ismanaged"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("solutionid", ConditionOperator.Equal, _solutionId)
                    }
                },
                TopCount = 1
            };

            var results = _connection.RetrieveMultiple(query);
            if (results.Entities.Count > 0)
            {
                var solution = results.Entities[0];
                _isManagedSolution = solution.GetAttributeValue<bool>("ismanaged");
            }
        }

        /// <summary>
        /// Gets the components from the Dataverse environment.
        /// </summary>
        public List<SolutionComponent> GetComponents(bool includeImpliedSubcomponents)
        {
            var components = ExtractRootComponents();

                components = ExpandWithSubcomponents(components, includeImpliedSubcomponents);
            

            return components;
        }

        /// <summary>
        /// Extracts root components from the environment.
        /// </summary>
        private List<SolutionComponent> ExtractRootComponents()
        {
            var components = new List<SolutionComponent>();

            // Use REST API to query msdyn_solutioncomponentsummary (virtual table)
            var filter = $"msdyn_solutionid eq {_solutionId:D}";
            if (IsManagedSolution.HasValue && IsManagedSolution.Value)
            {
                filter += " and msdyn_ismanaged eq 'True'";
            }
            var select = "msdyn_objectid,msdyn_componenttype,msdyn_uniquename,msdyn_primaryentityname,msdyn_componenttypename,msdyn_name,msdyn_schemaname,msdyn_isdefault,msdyn_iscustom,msdyn_hasactivecustomization,msdyn_ismanaged";
            var queryString = $"$filter={Uri.EscapeDataString(filter)}&$select={select}";

            string nextLink = null;
            do
            {
                var uri = nextLink ?? $"msdyn_solutioncomponentsummaries?{queryString}";
                var response = _connection.ExecuteWebRequest(HttpMethod.Get, uri, null, null);

                response.EnsureSuccessStatusCode();

                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                using (var doc = JsonDocument.Parse(jsonResponse))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("value", out var valueArray))
                    {
                        foreach (var item in valueArray.EnumerateArray())
                        {
                            var objectId = Guid.Parse(item.GetProperty("msdyn_objectid").GetString());
                            var componentType = item.GetProperty("msdyn_componenttype").GetInt32();
                            var schemaName = item.TryGetProperty("msdyn_schemaname", out var sn) ? sn.GetString() : null;
                            var uniqueName = item.TryGetProperty("msdyn_uniquename", out var un) ? un.GetString() : null;
                            var name = item.TryGetProperty("msdyn_name", out var n) ? n.GetString() : null;
                            var componentTypeName = item.TryGetProperty("msdyn_componenttypename", out var ctn) ? ctn.GetString() : null;
                            var isDefault = item.TryGetProperty("msdyn_isdefault", out var d) && d.ValueKind != JsonValueKind.Null ? d.GetBoolean() : (bool?)null;
                            var isCustom = item.TryGetProperty("msdyn_iscustom", out var c) && (c.ValueKind == JsonValueKind.True || c.ValueKind == JsonValueKind.False) ? c.GetBoolean() : (bool?)null;
                            var isCustomized = item.TryGetProperty("msdyn_hasactivecustomization", out var cz) && (cz.ValueKind == JsonValueKind.True || cz.ValueKind == JsonValueKind.False) ? cz.GetBoolean() : (bool?)null;

                            string logicalName = schemaName ?? uniqueName ?? name;

                            var component = new SolutionComponent
                            {
                                UniqueName = logicalName?.ToLower(),
                                ObjectId = objectId,
                                ComponentType = componentType,
                                ComponentTypeName = componentTypeName,
                                RootComponentBehavior = 0, // Will be updated from solutioncomponent query
                                IsDefault = isDefault,
                                IsCustom = isCustom,
                                IsCustomized = isCustomized
                            };
                            components.Add(component);
                        }
                    }

                    nextLink = root.TryGetProperty("@odata.nextLink", out var nl) ? nl.GetString() : null;
                    if (nextLink != null)
                    {
                        // Extract the query part after the entity set
                        var uriObj = new Uri(nextLink);
                        var pathAndQuery = uriObj.PathAndQuery;
                        var index = pathAndQuery.IndexOf("msdyn_solutioncomponentsummaries");
                        if (index >= 0)
                        {
                            nextLink = pathAndQuery.Substring(index);
                        }
                        else
                        {
                            nextLink = null; // Fallback
                        }
                    }
                }
            } while (nextLink != null);

            // Query solutioncomponent table to get rootcomponentbehavior for each component
            if (components.Count > 0)
            {
                var solutionComponentQuery = new QueryExpression("solutioncomponent")
                {
                    ColumnSet = new ColumnSet("objectid", "rootcomponentbehavior"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("solutionid", ConditionOperator.Equal, _solutionId)
                        }
                    }
                };

                var allSolutionComponents = new List<Entity>();
                EntityCollection result;
                do
                {
                    result = _connection.RetrieveMultiple(solutionComponentQuery);
                    allSolutionComponents.AddRange(result.Entities);
                    if (result.MoreRecords)
                    {
                        solutionComponentQuery.PageInfo.PageNumber++;
                        solutionComponentQuery.PageInfo.PagingCookie = result.PagingCookie;
                    }
                } while (result.MoreRecords);

                // Create a lookup dictionary for quick matching by objectid
                var behaviorLookup = allSolutionComponents
                    .Where(e => e.Contains("rootcomponentbehavior"))
                    .ToDictionary(
                        e => e.GetAttributeValue<Guid>("objectid"),
                        e => e.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value ?? 0
                    );

                // Update components with behavior information
                foreach (var component in components)
                {
                    if (component.ObjectId.HasValue && behaviorLookup.TryGetValue(component.ObjectId.Value, out var behavior))
                    {
                        component.RootComponentBehavior = behavior;
                    }
                }
            }

            return components;
        }

        /// <summary>
        /// Expands components with their subcomponents.
        /// </summary>
        private List<SolutionComponent> ExpandWithSubcomponents(List<SolutionComponent> components, bool includeImpliedSubcomponents)
        {
            var expandedComponents = new List<SolutionComponent>();

            foreach (var component in components)
            {
                // Always add the root component itself
                expandedComponents.Add(new SolutionComponent
                {
                    UniqueName = component.UniqueName,
                    ObjectId = component.ObjectId,
                    ComponentType = component.ComponentType,
                    ComponentTypeName = component.ComponentTypeName,
                    RootComponentBehavior = component.RootComponentBehavior
                });

                if (component.ComponentType == 1 && (includeImpliedSubcomponents || component.RootComponentBehavior != (int)RootComponentBehavior.IncludeSubcomponents))
                {
                    var subcomponents = GetSubcomponents(component);
                    expandedComponents.AddRange(subcomponents);
                }
            }

            return expandedComponents;
        }

        /// <summary>
        /// Gets the subcomponents for a specific parent component.
        /// </summary>
        public List<SolutionComponent> GetSubcomponents(SolutionComponent parentComponent)
        {
            return GetSubcomponentsFromEnvironment(parentComponent);
        }

        /// <summary>
        /// Retrieves subcomponents from the Dataverse environment.
        /// </summary>
        private List<SolutionComponent> GetSubcomponentsFromEnvironment(SolutionComponent parentComponent)
        {
            var subcomponents = new List<SolutionComponent>();

            var subcomponentTypes = new[] { 2, 10, 14, 22, 26, 59, 60, 29 };

            foreach (var componentTypeFilter in subcomponentTypes)
            {
                // Use REST API to query msdyn_solutioncomponentsummary for child components
                var filter = $"msdyn_primaryentityname eq '{parentComponent.UniqueName}' and msdyn_componenttype eq {componentTypeFilter}";
                if (IsManagedSolution.HasValue && IsManagedSolution.Value)
                {
                    filter += " and msdyn_ismanaged eq 'True'";
                }
                if (_solutionId != Guid.Empty)
                {
                    filter += $" and msdyn_solutionid eq {_solutionId:B}";
                }

                var select = "msdyn_objectid,msdyn_componenttype,msdyn_uniquename,msdyn_primaryentityname,msdyn_componenttypename,msdyn_schemaname,msdyn_isdefault,msdyn_iscustom,msdyn_hasactivecustomization,msdyn_ismanaged";
                var queryString = $"$filter={Uri.EscapeDataString(filter)}&$select={select}";

                string nextLink = null;
                do
                {
                    var uri = nextLink ?? $"msdyn_solutioncomponentsummaries?{queryString}";
                    HttpResponseMessage response;

                    try
                    {
                        response = _connection.ExecuteWebRequest(HttpMethod.Get, uri, null, null);
                    } catch (Exception ex)
                    {
                        throw new Exception($"Error retrieving subcomponents for parent component {parentComponent.UniqueName} (Type {parentComponent.ComponentType}): {ex.Message}\n{uri}", ex);
                    }

                    response.EnsureSuccessStatusCode();

                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    using (var doc = JsonDocument.Parse(jsonResponse))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("value", out var valueArray))
                        {
                            foreach (var item in valueArray.EnumerateArray())
                            {
                                ExtractSubcomponentDetails(parentComponent, subcomponents, item);
                            }
                        }

                        nextLink = root.TryGetProperty("@odata.nextLink", out var nl) ? nl.GetString() : null;
                        if (nextLink != null)
                        {
                            // Extract the query part after the entity set
                            var uriObj = new Uri(nextLink);
                            var pathAndQuery = uriObj.PathAndQuery;
                            var index = pathAndQuery.IndexOf("msdyn_solutioncomponentsummaries");
                            if (index >= 0)
                            {
                                nextLink = pathAndQuery.Substring(index);
                            }
                            else
                            {
                                nextLink = null; // Fallback
                            }
                        }
                    }
                } while (nextLink != null);
            }
            LoadRootComponentBehaviors(subcomponents);

            RetrieveDisplayStrings(subcomponents);
            _cmdlet.WriteVerbose($"Found {subcomponents.Count} subcomponents from msdyn_solutioncomponentsummary");


            return subcomponents;
        }

        private static void ExtractSubcomponentDetails(SolutionComponent parentComponent, List<SolutionComponent> subcomponents, JsonElement item)
        {
            var objectId = Guid.Parse(item.GetProperty("msdyn_objectid").GetString());
            var componentType = item.GetProperty("msdyn_componenttype").GetInt32();

            var schemaName = item.TryGetProperty("msdyn_schemaname", out var sn) ? sn.GetString() : null;
            var name = item.TryGetProperty("msdyn_name", out var n) ? n.GetString() : null;
            var uniqueName = item.TryGetProperty("msdyn_uniquename", out var un) ? un.GetString() : null;
            var componentTypeName = item.TryGetProperty("msdyn_componenttypename", out var ctn) ? ctn.GetString() : null;
            var isDefault = item.TryGetProperty("msdyn_isdefault", out var d) && d.ValueKind != JsonValueKind.Null ? d.GetBoolean() : (bool?)null;
            var isCustom = item.TryGetProperty("msdyn_iscustom", out var c) && (c.ValueKind == JsonValueKind.True || c.ValueKind == JsonValueKind.False) ? c.GetBoolean() : (bool?)null;
            var isCustomized = item.TryGetProperty("msdyn_hasactivecustomization", out var cz) && (cz.ValueKind == JsonValueKind.True || cz.ValueKind == JsonValueKind.False) ? cz.GetBoolean() : (bool?)null;
            var isManaged = item.TryGetProperty("msdyn_ismanaged", out var m) && (m.ValueKind == JsonValueKind.True || m.ValueKind == JsonValueKind.False) ? m.GetBoolean() : (bool?)null;

            // Use msdyn_uniquename if available, otherwise use msdyn_schemaname
            string logicalName = schemaName ?? uniqueName ?? name;

            subcomponents.Add(new SolutionComponent
            {
                UniqueName = logicalName,
                ObjectId = objectId,
                ComponentType = componentType,
                ComponentTypeName = componentTypeName,
                RootComponentBehavior = 0, // Will be updated from solutioncomponent query
                IsSubcomponent = true,
                ParentComponentType = parentComponent.ComponentType,
                ParentTableName = parentComponent.UniqueName,
                ParentIsCustom = parentComponent.IsCustom,
                ParentIsCustomized = parentComponent.IsCustomized,
                ParentIsDefault = parentComponent.IsDefault,
                ParentIsManaged = parentComponent.IsManaged,
                IsDefault = isDefault,
                IsCustom = isCustom,
                IsCustomized = isCustomized,
                IsManaged = isManaged
            });
        }

        private void LoadRootComponentBehaviors(List<SolutionComponent> subcomponents)
        {

            // Query solutioncomponent table to get rootcomponentbehavior for each subcomponent
            if (subcomponents.Count > 0 && _solutionId != Guid.Empty)
            {
                var solutionComponentQuery = new QueryExpression("solutioncomponent")
                {
                    ColumnSet = new ColumnSet("objectid", "rootcomponentbehavior"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("solutionid", ConditionOperator.Equal, _solutionId)
                        }
                    }
                };

                var allSolutionComponents = new List<Entity>();
                EntityCollection result;
                do
                {
                    result = _connection.RetrieveMultiple(solutionComponentQuery);
                    allSolutionComponents.AddRange(result.Entities);
                    if (result.MoreRecords)
                    {
                        solutionComponentQuery.PageInfo.PageNumber++;
                        solutionComponentQuery.PageInfo.PagingCookie = result.PagingCookie;
                    }
                } while (result.MoreRecords);

                // Create a lookup dictionary for quick matching by objectid
                var behaviorLookup = allSolutionComponents
                    .Where(e => e.Contains("rootcomponentbehavior"))
                    .ToDictionary(
                        e => e.GetAttributeValue<Guid>("objectid"),
                        e => e.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value ?? 0
                    );

                // Update subcomponents with behavior information
                foreach (var subcomponent in subcomponents)
                {
                    if (subcomponent.ObjectId.HasValue && behaviorLookup.TryGetValue(subcomponent.ObjectId.Value, out var behavior))
                    {
                        subcomponent.RootComponentBehavior = behavior;
                    }
                }
            }
        }

        private void RetrieveDisplayStrings(List<SolutionComponent> subcomponents)
        {

            // Retrieve displaystringkey for component type 22 (Display String)
            var displayStringIds = subcomponents.Where(sc => sc.ComponentType == 22).Select(sc => sc.ObjectId.Value).ToList();
            if (displayStringIds.Any())
            {
                foreach (var displayStringIdsChunk in ChunkList(displayStringIds, 20))
                {

                    var idsFilter = string.Join(" or ", displayStringIdsChunk.Select(id => $"displaystringid eq {id:B}"));
                    var queryString = $"$filter={Uri.EscapeDataString(idsFilter)}&$select=displaystringid,displaystringkey";

                    string nextLink = null;
                    do
                    {
                        var uri = nextLink ?? $"displaystrings?{queryString}";
                        var response = _connection.ExecuteWebRequest(HttpMethod.Get, uri, null, null);

                        response.EnsureSuccessStatusCode();

                        var jsonResponse = response.Content.ReadAsStringAsync().Result;
                        using (var doc = JsonDocument.Parse(jsonResponse))
                        {
                            var root = doc.RootElement;
                            if (root.TryGetProperty("value", out var valueArray))
                            {
                                foreach (var item in valueArray.EnumerateArray())
                                {
                                    var displaystringid = Guid.Parse(item.GetProperty("displaystringid").GetString());
                                    var displaystringkey = item.GetProperty("displaystringkey").GetString();

                                    var subcomponent = subcomponents.FirstOrDefault(sc => sc.ObjectId == displaystringid && sc.ComponentType == 22);
                                    if (subcomponent != null)
                                    {
                                        subcomponent.UniqueName = displaystringkey;
                                    }
                                }
                            }

                            nextLink = root.TryGetProperty("@odata.nextLink", out var nl) ? nl.GetString() : null;
                            if (nextLink != null)
                            {
                                // Extract the query part after the entity set
                                var uriObj = new Uri(nextLink);
                                var pathAndQuery = uriObj.PathAndQuery;
                                var index = pathAndQuery.IndexOf("displaystrings");
                                if (index >= 0)
                                {
                                    nextLink = pathAndQuery.Substring(index);
                                }
                                else
                                {
                                    nextLink = null; // Fallback
                                }
                            }
                        }
                    } while (nextLink != null);
                }
            }
        }

        // Add this helper method to the EnvironmentComponentExtractor class
        private static IEnumerable<List<T>> ChunkList<T>(List<T> source, int chunkSize)
        {
            for (int i = 0; i < source.Count; i += chunkSize)
            {
                yield return source.GetRange(i, Math.Min(chunkSize, source.Count - i));
            }
        }
    }
}
