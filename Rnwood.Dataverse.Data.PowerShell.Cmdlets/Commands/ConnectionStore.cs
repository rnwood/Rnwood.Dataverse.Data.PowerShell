using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Manages persistent storage of named Dataverse connections.
	/// Uses MSAL token cache for secure token storage and JSON for connection metadata.
	/// </summary>
	internal class ConnectionStore
	{
		private const string CacheFileName = "dataverse_connection_cache.bin";
		private const string MetadataFileName = "dataverse_connections.json";
		private const string CacheSchemaVersion = "1.0.0.0";
		
		private readonly string _cacheDirectory;
		private readonly string _cacheFilePath;
		private readonly string _metadataFilePath;
		
		/// <summary>
		/// Initializes a new instance of the ConnectionStore class.
		/// </summary>
		public ConnectionStore()
		{
			// Use platform-appropriate directory for cache storage
			string appDataPath;
			
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				// Windows: use LocalApplicationData
				appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			}
			else
			{
				// Linux/macOS: use user home directory
				appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			}
			
			_cacheDirectory = Path.Combine(appDataPath, "Rnwood.Dataverse.Data.PowerShell");
			_cacheFilePath = Path.Combine(_cacheDirectory, CacheFileName);
			_metadataFilePath = Path.Combine(_cacheDirectory, MetadataFileName);
			
			// Ensure directory exists
			Directory.CreateDirectory(_cacheDirectory);
		}
		
		/// <summary>
		/// Gets the MSAL storage creation properties for cross-platform token caching.
		/// </summary>
		private StorageCreationProperties GetStorageProperties()
		{
			var storageProperties = new StorageCreationPropertiesBuilder(
				CacheFileName,
				_cacheDirectory)
				.WithMacKeyChain(
					serviceName: "Rnwood.Dataverse.Data.PowerShell",
					accountName: "DataverseConnections")
				.WithLinuxKeyring(
					schemaName: "dataverse.connection.cache",
					collection: "default",
					secretLabel: "Dataverse Connection Cache",
					attribute1: new KeyValuePair<string, string>("Version", CacheSchemaVersion),
					attribute2: new KeyValuePair<string, string>("ProductName", "Rnwood.Dataverse.Data.PowerShell"))
				.Build();
			
			return storageProperties;
		}
		
		/// <summary>
		/// Registers MSAL cache helper with a public client application.
		/// </summary>
		public MsalCacheHelper RegisterCache(IPublicClientApplication app)
		{
			var storageProperties = GetStorageProperties();
			var cacheHelper = MsalCacheHelper.CreateAsync(storageProperties).GetAwaiter().GetResult();
			cacheHelper.RegisterCache(app.UserTokenCache);
			return cacheHelper;
		}
		
		/// <summary>
		/// Registers MSAL cache helper with a confidential client application.
		/// </summary>
		public MsalCacheHelper RegisterCache(IConfidentialClientApplication app)
		{
			var storageProperties = GetStorageProperties();
			var cacheHelper = MsalCacheHelper.CreateAsync(storageProperties).GetAwaiter().GetResult();
			cacheHelper.RegisterCache(app.AppTokenCache);
			return cacheHelper;
		}
		
		/// <summary>
		/// Saves connection metadata.
		/// </summary>
		public void SaveConnection(string name, ConnectionMetadata metadata)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Connection name cannot be empty.", nameof(name));
			
			var connections = LoadAllMetadata();
			connections[name] = metadata;
			SaveAllMetadata(connections);
		}
		
		/// <summary>
		/// Loads connection metadata by name.
		/// </summary>
		public ConnectionMetadata LoadConnection(string name)
		{
			var connections = LoadAllMetadata();
			if (connections.TryGetValue(name, out var metadata))
			{
				return metadata;
			}
			return null;
		}
		
		/// <summary>
		/// Lists all saved connection names.
		/// </summary>
		public List<string> ListConnections()
		{
			var connections = LoadAllMetadata();
			return connections.Keys.OrderBy(k => k).ToList();
		}
		
		/// <summary>
		/// Deletes a named connection.
		/// </summary>
		public bool DeleteConnection(string name)
		{
			var connections = LoadAllMetadata();
			bool removed = connections.Remove(name);
			if (removed)
			{
				SaveAllMetadata(connections);
			}
			return removed;
		}
		
		/// <summary>
		/// Checks if a named connection exists.
		/// </summary>
		public bool ConnectionExists(string name)
		{
			var connections = LoadAllMetadata();
			return connections.ContainsKey(name);
		}
		
		private Dictionary<string, ConnectionMetadata> LoadAllMetadata()
		{
			if (!File.Exists(_metadataFilePath))
			{
				return new Dictionary<string, ConnectionMetadata>(StringComparer.OrdinalIgnoreCase);
			}
			
			try
			{
				var json = File.ReadAllText(_metadataFilePath);
				var connections = JsonSerializer.Deserialize<Dictionary<string, ConnectionMetadata>>(json);
				return connections ?? new Dictionary<string, ConnectionMetadata>(StringComparer.OrdinalIgnoreCase);
			}
			catch
			{
				// If file is corrupted, return empty dictionary
				return new Dictionary<string, ConnectionMetadata>(StringComparer.OrdinalIgnoreCase);
			}
		}
		
		private void SaveAllMetadata(Dictionary<string, ConnectionMetadata> connections)
		{
			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
			};
			
			var json = JsonSerializer.Serialize(connections, options);
			File.WriteAllText(_metadataFilePath, json);
		}
	}
	
	/// <summary>
	/// Metadata for a saved connection.
	/// </summary>
	internal class ConnectionMetadata
	{
		/// <summary>
		/// The Dataverse environment URL.
		/// </summary>
		public string Url { get; set; }
		
		/// <summary>
		/// The authentication method used.
		/// </summary>
		public string AuthMethod { get; set; }
		
		/// <summary>
		/// Client ID for OAuth authentication.
		/// </summary>
		public string ClientId { get; set; }
		
		/// <summary>
		/// Username for username/password authentication.
		/// </summary>
		public string Username { get; set; }
		
		/// <summary>
		/// Client ID for user-assigned managed identity.
		/// </summary>
		public string ManagedIdentityClientId { get; set; }
		
		/// <summary>
		/// When the connection was last saved.
		/// </summary>
		public DateTime SavedAt { get; set; }
	}
}
