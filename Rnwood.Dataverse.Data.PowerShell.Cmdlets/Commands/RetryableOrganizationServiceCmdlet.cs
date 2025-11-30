using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Management.Automation;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Base class for cmdlets that support retry logic with exponential backoff.
	/// </summary>
	public abstract class RetryableOrganizationServiceCmdlet : OrganizationServiceCmdlet
	{
		/// <summary>
		/// Number of times to retry on failure. Default is 0 (no retries).
		/// </summary>
		[Parameter(HelpMessage = "Number of times to retry on failure. Default is 0 (no retries).")]
		public int Retries { get; set; } = 0;

		/// <summary>
		/// Initial delay in seconds before first retry. Subsequent retries use exponential backoff. Default is 5s.
		/// </summary>
		[Parameter(HelpMessage = "Initial delay in seconds before first retry. Subsequent retries use exponential backoff. Default is 5s.")]
		public int InitialRetryDelay { get; set; } = 5;

		/// <summary>
		/// Executes an organization request with retry logic.
		/// Throttling exceptions from service protection limits are automatically retried.
		/// </summary>
		/// <param name="request">The request to execute.</param>
		/// <returns>The organization response.</returns>
		protected OrganizationResponse ExecuteWithRetry(OrganizationRequest request)
		{
			int retriesRemaining = Retries;
			int attemptNumber = 0;

			while (true)
			{
				attemptNumber++;

				try
				{
					if (attemptNumber > 1)
					{
						WriteVerbose($"Retry attempt {attemptNumber} of {Retries + 1}");
					}

					return Connection.Execute(request);
				}
				catch (FaultException<OrganizationServiceFault> ex) when (QueryHelpers.IsThrottlingException(ex, out TimeSpan retryDelay))
				{
					// Throttling exceptions are always retried, regardless of Retries setting
					WriteVerbose($"Throttled by service protection. Waiting {retryDelay.TotalSeconds:F1}s before retry...");
					Thread.Sleep(retryDelay);
					// Don't decrement retriesRemaining for throttling - it's a separate retry mechanism
				}
				catch (Exception ex) when (retriesRemaining > 0)
				{
					// Calculate exponential backoff delay
					int delayS = InitialRetryDelay * (int)Math.Pow(2, attemptNumber - 1);
					retriesRemaining--;

					WriteVerbose($"Request failed (attempt {attemptNumber} of {Retries + 1}), will retry in {delayS}s. Error: {ex.Message}");

					if (retriesRemaining > 0)
					{
						Thread.Sleep(delayS*1000);
					}
					else
					{
						// No more retries - rethrow the exception
						throw;
					}
				}
			}
		}

		/// <summary>
		/// Appends fault details to a string builder.
		/// </summary>
		protected void AppendFaultDetails(OrganizationServiceFault fault, StringBuilder output)
		{
			output.AppendLine("OrganizationServiceFault " + fault.ErrorCode + ": " + fault.Message);
			output.AppendLine(fault.TraceText);

			if (fault.InnerFault != null)
			{
				output.AppendLine("---");
				AppendFaultDetails(fault.InnerFault, output);
			}
		}
	}
}
