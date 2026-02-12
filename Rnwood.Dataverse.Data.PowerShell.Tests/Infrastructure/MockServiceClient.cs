using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure
{
    /// <summary>
    /// Factory for creating mock ServiceClient instances.
    /// The created ServiceClient instances can be wrapped in MockDataverseConnection
    /// to support cloning for parallel operations.
    /// </summary>
    internal static class MockServiceClientFactory
    {
        private static ConstructorInfo? _constructor;

        /// <summary>
        /// Creates a new ServiceClient using the internal constructor.
        /// The service parameter should be wrapped with ThreadSafeOrganizationService for parallel operations.
        /// </summary>
        public static ServiceClient Create(
            IOrganizationService service,
            HttpClient httpClient,
            string instanceUri,
            Version sdkVersion,
            ILogger logger)
        {
            if (_constructor == null)
            {
                _constructor = typeof(ServiceClient).GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(IOrganizationService), typeof(HttpClient), typeof(string), typeof(Version), typeof(ILogger) },
                    null);

                if (_constructor == null)
                {
                    throw new InvalidOperationException("Could not find ServiceClient internal constructor");
                }
            }

            var client = (ServiceClient)_constructor.Invoke(new object[] { service, httpClient, instanceUri, sdkVersion, logger });
            
            // Try to set additional properties that SQL4Cds and other components may need
            // Use reflection to set all possible fields that might store the URI
            var uri = new Uri(instanceUri);
            
            // Try properties first
            SetPrivateProperty(client, "ConnectedOrgUniqueName", "fakeorg");
            SetPrivateProperty(client, "ConnectedOrgFriendlyName", "Fake Organization");
            SetPrivateProperty(client, "ConnectedOrgVersion", sdkVersion);
            
            // Try to set URI-related fields directly since the property is read-only
            TrySetUriField(client, uri);
            
            return client;
        }
        
        private static void TrySetUriField(ServiceClient client, Uri uri)
        {
            // Try common field name patterns for storing the connected org URI
            var fieldNames = new[]
            {
                "_connectedOrgUriActual",
                "_connectedOrgUri",
                "<ConnectedOrgUriActual>k__BackingField",
                "connectedOrgUriActual",
                "_organizationUri",
                "_orgUri",
                "_actualUri"
            };
            
            var clientType = client.GetType();
            foreach (var fieldName in fieldNames)
            {
                var field = clientType.GetField(fieldName, 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null && field.FieldType == typeof(Uri))
                {
                    try
                    {
                        field.SetValue(client, uri);
                        // Verify it worked
                        if (client.ConnectedOrgUriActual != null && 
                            client.ConnectedOrgUriActual.Host == uri.Host)
                        {
                            return; // Success!
                        }
                    }
                    catch
                    {
                        // Continue trying other fields
                    }
                }
            }
            
            // If predefined field names didn't work, try ALL Uri fields
            var allFields = clientType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in allFields.Where(f => f.FieldType == typeof(Uri)))
            {
                try
                {
                    field.SetValue(client, uri);
                    // Verify it worked
                    if (client.ConnectedOrgUriActual != null && 
                        client.ConnectedOrgUriActual.Host == uri.Host)
                    {
                        return; // Success!
                    }
                }
                catch
                {
                    // Continue trying other fields
                }
            }
            
            // If we STILL haven't succeeded, the internal constructor may have set it already
            // or the field pattern has changed in a newer version of ServiceClient
        }

        private static void SetPrivateProperty(object obj, string propertyName, object value)
        {
            var property = obj.GetType().GetProperty(propertyName, 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value);
            }
            else
            {
                // Try to set via backing field (auto-properties have backing fields like <PropertyName>k__BackingField)
                var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (backingField != null)
                {
                    backingField.SetValue(obj, value);
                }
                // If we can't set the property, silently ignore (e.g., ConnectedOrgUriActual is read-only)
            }
        }
    }
}
