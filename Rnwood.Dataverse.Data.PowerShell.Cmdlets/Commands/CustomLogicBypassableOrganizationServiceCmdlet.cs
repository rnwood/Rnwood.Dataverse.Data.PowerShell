using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Base class for cmdlets that support bypassing custom business logic execution.
	/// </summary>
	public abstract class CustomLogicBypassableOrganizationServiceCmdlet : OrganizationServiceCmdlet
	{
		/// <summary>
		/// Gets or sets the types of business logic to bypass during execution.
		/// </summary>
		public abstract BusinessLogicTypes[] BypassBusinessLogicExecution { get; set; }

		/// <summary>
		/// Gets or sets the GUIDs of specific business logic execution steps to bypass.
		/// </summary>
		public abstract Guid[] BypassBusinessLogicExecutionStepIds { get; set; }

		/// <summary>
		/// Types of business logic that can be bypassed.
		/// </summary>
		public enum BusinessLogicTypes
		{
			/// <summary>
			/// Synchronous custom logic.
			/// </summary>
			CustomSync,
			/// <summary>
			/// Asynchronous custom logic.
			/// </summary>
			CustomAsync
		}

		/// <summary>
		/// Applies business logic bypass parameters to the request.
		/// </summary>
		/// <param name="request">The organization request to apply bypass parameters to.</param>
		protected void ApplyBypassBusinessLogicExecution(OrganizationRequest request)
		{
			if (BypassBusinessLogicExecution?.Length > 0)
			{
				request.Parameters["BypassBusinessLogicExecution"] = string.Join(",", BypassBusinessLogicExecution.Select(o => o.ToString()));


			}
			else
			{
				request.Parameters.Remove("BypassBusinessLogicExecution");
			}

			if (BypassBusinessLogicExecutionStepIds?.Length > 0)
			{
				request.Parameters["BypassBusinessLogicExecutionStepIds"] = string.Join(",", BypassBusinessLogicExecutionStepIds.Select(id => id.ToString()));
			}
			else
			{
				request.Parameters.Remove("BypassBusinessLogicExecutionStepIds");
			}
		}
	}
}