using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
	public ConnectionStore() : this(null)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the ConnectionStore class with a custom cache directory.
	/// </summary>
	/// <param name="cacheDirectory">Optional custom cache directory path. If null, uses the default platform-appropriate location.</param>
	public ConnectionStore(string cacheDirectory)
	{
		if (cacheDirectory != null)
		{
			_cacheDirectory = cacheDirectory;
		}
		else
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
		}
		
			// Ensure directory exists
			Directory.CreateDirectory(_cacheDirectory);
		}
		
		/// <summary>
		/// Encrypts a string using platform-specific best practices.
		/// Windows: Uses Data Protection API (DPAPI)
		/// Linux/macOS: Uses AES with a machine-specific key
		/// </summary>
		private string EncryptString(string plainText)
		{
			if (string.IsNullOrEmpty(plainText))
			{
				return plainText;
			}
			
			byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
			byte[] encryptedBytes;
			
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				// Windows: Use DPAPI
				encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
			}
			else
			{
				// Linux/macOS: Use AES with machine-specific key
				// This provides basic encryption, though not as secure as OS keyring
				byte[] key = GetMachineKey();
				using (Aes aes = Aes.Create())
				{
					aes.Key = key;
					aes.GenerateIV();
					
					using (var encryptor = aes.CreateEncryptor())
					using (var ms = new MemoryStream())
					{
						// Prepend IV to encrypted data
						ms.Write(aes.IV, 0, aes.IV.Length);
						using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
						{
							cs.Write(plainBytes, 0, plainBytes.Length);
						}
						encryptedBytes = ms.ToArray();
					}
				}
			}
			
			return Convert.ToBase64String(encryptedBytes);
		}
		
		/// <summary>
		/// Decrypts a string that was encrypted with EncryptString.
		/// </summary>
		private string DecryptString(string encryptedText)
		{
			if (string.IsNullOrEmpty(encryptedText))
			{
				return encryptedText;
			}
			
			try
			{
				byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
				byte[] plainBytes;
				
				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				{
					// Windows: Use DPAPI
					plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
				}
				else
				{
					// Linux/macOS: Use AES with machine-specific key
					byte[] key = GetMachineKey();
					using (Aes aes = Aes.Create())
					{
						aes.Key = key;
						
						// Extract IV from the beginning of encrypted data
						byte[] iv = new byte[aes.BlockSize / 8];
						Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);
						aes.IV = iv;
						
						using (var decryptor = aes.CreateDecryptor())
						using (var ms = new MemoryStream(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length))
						using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
						using (var resultMs = new MemoryStream())
						{
							cs.CopyTo(resultMs);
							plainBytes = resultMs.ToArray();
						}
					}
				}
				
				return Encoding.UTF8.GetString(plainBytes);
			}
			catch
			{
				// If decryption fails, return null to indicate corruption or tampering
				return null;
			}
		}
		
		/// <summary>
		/// Gets a machine-specific encryption key for non-Windows platforms.
		/// </summary>
		private byte[] GetMachineKey()
		{
			// Generate a deterministic key based on machine ID and a constant salt
			// This isn't as secure as OS keyring but provides basic protection
			string machineId = Environment.MachineName + Environment.UserName;
			string salt = "Rnwood.Dataverse.Data.PowerShell.Encryption.v1";
			
			using (var sha256 = SHA256.Create())
			{
				byte[] combinedBytes = Encoding.UTF8.GetBytes(machineId + salt);
				return sha256.ComputeHash(combinedBytes);
			}
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
			
			// Encrypt sensitive fields before saving
			var metadataToSave = new ConnectionMetadata
			{
				Url = metadata.Url,
				AuthMethod = metadata.AuthMethod,
				ClientId = metadata.ClientId,
				Username = metadata.Username,
				Password = EncryptString(metadata.Password),
				ManagedIdentityClientId = metadata.ManagedIdentityClientId,
				ClientSecret = EncryptString(metadata.ClientSecret),
				CertificatePath = metadata.CertificatePath,
				CertificatePassword = EncryptString(metadata.CertificatePassword),
				CertificateThumbprint = metadata.CertificateThumbprint,
				CertificateStoreLocation = metadata.CertificateStoreLocation,
				CertificateStoreName = metadata.CertificateStoreName,
				SavedAt = metadata.SavedAt
			};
			
			var connections = LoadAllMetadata();
			connections[name] = metadataToSave;
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
				// Decrypt sensitive fields after loading
				var decryptedMetadata = new ConnectionMetadata
				{
					Url = metadata.Url,
					AuthMethod = metadata.AuthMethod,
					ClientId = metadata.ClientId,
					Username = metadata.Username,
					Password = DecryptString(metadata.Password),
					ManagedIdentityClientId = metadata.ManagedIdentityClientId,
					ClientSecret = DecryptString(metadata.ClientSecret),
					CertificatePath = metadata.CertificatePath,
					CertificatePassword = DecryptString(metadata.CertificatePassword),
					CertificateThumbprint = metadata.CertificateThumbprint,
					CertificateStoreLocation = metadata.CertificateStoreLocation,
					CertificateStoreName = metadata.CertificateStoreName,
					SavedAt = metadata.SavedAt
				};
				return decryptedMetadata;
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
		/// Deletes all saved connections and clears the cache.
		/// </summary>
		public void ClearAllConnections()
		{
			// Delete metadata file
			if (File.Exists(_metadataFilePath))
			{
				File.Delete(_metadataFilePath);
			}
			
			// Delete MSAL cache file
			if (File.Exists(_cacheFilePath))
			{
				File.Delete(_cacheFilePath);
			}
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
		/// Password for username/password authentication (NOT RECOMMENDED to save, encrypted if saved).
		/// </summary>
		public string Password { get; set; }
		
		/// <summary>
		/// Client ID for user-assigned managed identity.
		/// </summary>
		public string ManagedIdentityClientId { get; set; }
		
		/// <summary>
		/// Client secret for client secret authentication (NOT RECOMMENDED to save, encrypted if saved).
		/// </summary>
		public string ClientSecret { get; set; }
		
		/// <summary>
		/// Certificate path for certificate authentication (NOT RECOMMENDED to save).
		/// </summary>
		public string CertificatePath { get; set; }
		
		/// <summary>
		/// Certificate password for certificate authentication (NOT RECOMMENDED to save, encrypted if saved).
		/// </summary>
		public string CertificatePassword { get; set; }
		
		/// <summary>
		/// Certificate thumbprint for certificate authentication.
		/// </summary>
		public string CertificateThumbprint { get; set; }
		
		/// <summary>
		/// Certificate store location for certificate authentication.
		/// </summary>
		public string CertificateStoreLocation { get; set; }
		
		/// <summary>
		/// Certificate store name for certificate authentication.
		/// </summary>
		public string CertificateStoreName { get; set; }
		
		/// <summary>
		/// When the connection was last saved.
		/// </summary>
		public DateTime SavedAt { get; set; }
	}
}
