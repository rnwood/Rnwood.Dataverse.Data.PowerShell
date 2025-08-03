using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	public abstract class CustomLogicBypassableOrganizationServiceCmdlet : OrganizationServiceCmdlet
	{
		public abstract BusinessLogicTypes[] BypassBusinessLogicExecution { get; set; }

		public abstract Guid[] BypassBusinessLogicExecutionStepIds { get; set; }

		public enum BusinessLogicTypes
		{
			CustomSync,
			CustomAsync
		}

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