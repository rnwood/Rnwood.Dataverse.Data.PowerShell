using Microsoft.PowerPlatform.Dataverse.Client;
using System;
using System.Threading;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Manages the default Dataverse connection for cmdlets.
	/// Supports both process-wide and thread-local connections.
	/// Thread-local connections take precedence when set.
	/// </summary>
	internal static class DefaultConnectionManager
	{
		private static ServiceClient _defaultConnection;
		private static readonly object _lock = new object();

		[ThreadStatic]
		private static ServiceClient _threadLocalConnection;

		/// <summary>
		/// Gets or sets the default connection.
		/// Sets both the process-wide and thread-local connection.
		/// </summary>
		public static ServiceClient DefaultConnection
		{
			get
			{
				// Thread-local connection takes precedence
				if (_threadLocalConnection != null)
				{
					return _threadLocalConnection;
				}

				lock (_lock)
				{
					return _defaultConnection;
				}
			}
			set
			{
				// Set thread-local connection
				_threadLocalConnection = value;

				// Also set process-wide connection
				lock (_lock)
				{
					_defaultConnection = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the thread-local connection without affecting the process-wide connection.
		/// This is useful for parallel processing scenarios where each thread needs its own connection.
		/// </summary>
		public static ServiceClient ThreadLocalConnection
		{
			get
			{
				return _threadLocalConnection;
			}
			set
			{
				_threadLocalConnection = value;
			}
		}

		/// <summary>
		/// Clears the default connection (both process-wide and thread-local).
		/// </summary>
		public static void ClearDefaultConnection()
		{
			_threadLocalConnection = null;

			lock (_lock)
			{
				_defaultConnection = null;
			}
		}

		/// <summary>
		/// Clears only the thread-local connection.
		/// </summary>
		public static void ClearThreadLocalConnection()
		{
			_threadLocalConnection = null;
		}
	}
}
