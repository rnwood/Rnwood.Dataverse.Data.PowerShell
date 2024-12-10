using Microsoft.Xrm.Sdk;
using System.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	public abstract class CustomLogicBypassableOrganizationServiceCmdlet : OrganizationServiceCmdlet
	{
		public abstract BusinessLogicTypes[] BypassBusinessLogicExecution { get; set; }

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
		}
	}
}