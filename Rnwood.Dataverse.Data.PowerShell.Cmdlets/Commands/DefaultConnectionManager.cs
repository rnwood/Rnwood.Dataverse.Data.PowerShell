using Microsoft.PowerPlatform.Dataverse.Client;
using System.Threading;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Manages the default Dataverse connection for cmdlets.
	/// </summary>
	internal static class DefaultConnectionManager
	{
		private static ServiceClient _defaultConnection;
		private static readonly object _lock = new object();

		/// <summary>
		/// Gets or sets the default connection.
		/// </summary>
		public static ServiceClient DefaultConnection
		{
			get
			{
				lock (_lock)
				{
					return _defaultConnection;
				}
			}
			set
			{
				lock (_lock)
				{
					_defaultConnection = value;
				}
			}
		}

		/// <summary>
		/// Clears the default connection.
		/// </summary>
		public static void ClearDefaultConnection()
		{
			lock (_lock)
			{
				_defaultConnection = null;
			}
		}
	}
}
