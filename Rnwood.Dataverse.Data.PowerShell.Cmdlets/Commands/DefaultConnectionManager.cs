using Microsoft.PowerPlatform.Dataverse.Client;
using System;
using System.Threading;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Manages the default Dataverse connection for cmdlets.
	/// Supports both process-wide and thread-local connections for tests.
	/// </summary>
	internal static class DefaultConnectionManager
	{
		private static ServiceClient _defaultConnection;
		private static readonly object _lock = new object();

		public static bool UseThreadLocalConnection = false;

		private static AsyncLocal<ServiceClient> _threadLocalConnection = new AsyncLocal<ServiceClient>();

		
		/// <summary>
		/// Gets or sets the default connection.
		/// Sets both the process-wide and thread-local connection.
		/// </summary>
		public static ServiceClient DefaultConnection
		{
			get
			{
				// Thread-local connection takes precedence
				if (UseThreadLocalConnection)
				{
					return _threadLocalConnection.Value;
				}

				lock (_lock)
				{
					return _defaultConnection;
				}
			}
			set
			{
				if (UseThreadLocalConnection)
				{
					_threadLocalConnection.Value = value;
				} else {
					lock (_lock)
					{
						_defaultConnection = value;
					}
				}
			}
		}


		/// <summary>
		/// Clears the default connection (both process-wide and thread-local).
		/// </summary>
		public static void ClearDefaultConnection()
		{
			_threadLocalConnection.Value = null;

			lock (_lock)
			{
				_defaultConnection = null;
			}
		}
	}
}
