using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes a Canvas app from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseCanvasApp", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseCanvasAppCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_ID = "Id";
        private const string PARAMSET_NAME = "Name";

        /// <summary>
        /// Gets or sets the ID of the Canvas app to remove.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_ID, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the Canvas app to remove")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the Canvas app to remove.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_NAME, Mandatory = true, Position = 0, HelpMessage = "Name of the Canvas app to remove")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets whether to skip the error if the Canvas app doesn't exist.
        /// </summary>
        [Parameter(HelpMessage = "If set, no error is thrown if the Canvas app doesn't exist")]
        public SwitchParameter IfExists { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Guid canvasAppId;

            if (ParameterSetName == PARAMSET_ID)
            {
                canvasAppId = Id;
            }
            else
            {
                // Find Canvas app by name
                var foundId = FindCanvasAppByName(Name);
                if (!foundId.HasValue)
                {
                    if (IfExists.IsPresent)
                    {
                        WriteVerbose($"Canvas app '{Name}' does not exist (IfExists flag set, skipping)");
                        return;
                    }

                    ThrowTerminatingError(new ErrorRecord(
                        new Exception($"Canvas app '{Name}' not found"),
                        "CanvasAppNotFound",
                        ErrorCategory.ObjectNotFound,
                        Name));
                    return;
                }

                canvasAppId = foundId.Value;
            }

            // Verify Canvas app exists
            Entity canvasApp = null;
            try
            {
                canvasApp = Connection.Retrieve("canvasapp", canvasAppId, new ColumnSet("name", "displayname"));
            }
            catch (Exception)
            {
                if (IfExists.IsPresent)
                {
                    WriteVerbose($"Canvas app with ID '{canvasAppId}' does not exist (IfExists flag set, skipping)");
                    return;
                }
                throw;
            }

            string appName = canvasApp.GetAttributeValue<string>("name");
            string displayName = canvasApp.GetAttributeValue<string>("displayname");
            string description = $"Canvas app '{displayName ?? appName}' (ID: {canvasAppId})";

            if (!ShouldProcess(description, "Delete"))
            {
                return;
            }

            WriteVerbose($"Deleting Canvas app: {description}");

            Connection.Delete("canvasapp", canvasAppId);

            WriteVerbose("Canvas app deleted successfully");
        }

        private Guid? FindCanvasAppByName(string name)
        {
            var query = new QueryExpression("canvasapp")
            {
                ColumnSet = new ColumnSet("canvasappid"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("name", ConditionOperator.Equal, name);

            var results = Connection.RetrieveMultiple(query);
            return results.Entities.FirstOrDefault()?.Id;
        }
    }
}
