using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes web resources from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseWebResource", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseWebResourceCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_ID = "Id";
        private const string PARAMSET_NAME = "Name";
        private const string PARAMSET_OBJECT = "InputObject";

        /// <summary>
        /// Gets or sets the ID of the web resource to delete.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_ID, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the web resource to delete")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the web resource to delete.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_NAME, Mandatory = true, HelpMessage = "Name of the web resource to delete")]
        [ArgumentCompleter(typeof(WebResourceNameArgumentCompleter))]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the input object containing the web resource to delete.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_OBJECT, Mandatory = true, ValueFromPipeline = true, HelpMessage = "Web resource object to delete (from pipeline)")]
        public PSObject InputObject { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to suppress errors if the web resource doesn't exist.
        /// </summary>
        [Parameter(HelpMessage = "If set, suppresses errors if the web resource doesn't exist")]
        public SwitchParameter IfExists { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Guid webResourceId = Guid.Empty;
            string webResourceName = string.Empty;

            switch (ParameterSetName)
            {
                case PARAMSET_ID:
                    webResourceId = Id;
                    webResourceName = Id.ToString();
                    break;

                case PARAMSET_NAME:
                    webResourceName = Name;
                    var foundId = FindWebResourceByName(Name);
                    if (foundId.HasValue)
                    {
                        webResourceId = foundId.Value;
                    }
                    else
                    {
                        if (IfExists)
                        {
                            WriteVerbose($"Web resource not found: {Name}");
                            return;
                        }
                        else
                        {
                            WriteError(new ErrorRecord(
                                new InvalidOperationException($"Web resource not found: {Name}"),
                                "WebResourceNotFound",
                                ErrorCategory.ObjectNotFound,
                                Name));
                            return;
                        }
                    }
                    break;

                case PARAMSET_OBJECT:
                    if (InputObject.Properties["webresourceid"] != null)
                    {
                        webResourceId = (Guid)InputObject.Properties["webresourceid"].Value;
                        webResourceName = InputObject.Properties["name"]?.Value?.ToString() ?? webResourceId.ToString();
                    }
                    else if (InputObject.Properties["Id"] != null)
                    {
                        webResourceId = (Guid)InputObject.Properties["Id"].Value;
                        webResourceName = InputObject.Properties["name"]?.Value?.ToString() ?? webResourceId.ToString();
                    }
                    else
                    {
                        WriteError(new ErrorRecord(
                            new ArgumentException("InputObject must contain 'webresourceid' or 'Id' property"),
                            "InvalidInputObject",
                            ErrorCategory.InvalidArgument,
                            InputObject));
                        return;
                    }
                    break;
            }

            if (webResourceId == Guid.Empty)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException("Could not determine web resource ID"),
                    "InvalidWebResourceId",
                    ErrorCategory.InvalidOperation,
                    webResourceName));
                return;
            }

            if (ShouldProcess(webResourceName, "Delete web resource"))
            {
                try
                {
                    Connection.Delete("webresource", webResourceId);
                    WriteVerbose($"Deleted web resource: {webResourceName} (ID: {webResourceId})");
                }
                catch (Exception ex) when (IfExists)
                {
                    // Check if it's a "does not exist" error by examining the exception type
                    // FaultException<OrganizationServiceFault> with error code -2147220969 indicates record doesn't exist
                    if (ex is System.ServiceModel.FaultException)
                    {
                        WriteVerbose($"Web resource not found: {webResourceName}");
                        return;
                    }
                    
                    // Re-throw if it's a different error
                    throw;
                }
            }
        }

        private Guid? FindWebResourceByName(string name)
        {
            var query = new QueryExpression("webresource");
            query.ColumnSet = new ColumnSet("webresourceid");
            query.Criteria.AddCondition("name", ConditionOperator.Equal, name);
            query.TopCount = 1;

            // Use RetrieveUnpublishedMultipleRequest to find unpublished web resources
            var request = new Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleRequest { Query = query };
            var response = (Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleResponse)Connection.Execute(request);
            var results = response.EntityCollection;

            return results.Entities.Count > 0 ? results.Entities[0].Id : (Guid?)null;
        }
    }
}
