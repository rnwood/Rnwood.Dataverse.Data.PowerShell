using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a Canvas app in a Dataverse environment (upsert operation).
    /// Automatically determines whether to create or update based on ID or Name.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseCanvasApp", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Guid))]
    public class SetDataverseCanvasAppCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the Canvas app ID. If provided and exists, updates the app. If not exists, creates with this ID.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the Canvas app (used as unique key for upsert)")]
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the name for the Canvas app. Used as unique key if ID not provided.
        /// </summary>
        [Parameter(Position = 1, HelpMessage = "Name for the Canvas app (logical name, used as unique key if ID not provided)")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display name for the Canvas app.
        /// </summary>
        [Parameter(HelpMessage = "Display name for the Canvas app")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description for the Canvas app.
        /// </summary>
        [Parameter(HelpMessage = "Description for the Canvas app")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the path to an .msapp file to use as the document content.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Path to an .msapp file to use as the document content")]
        [ValidateNotNullOrEmpty]
        public string MsAppPath { get; set; }

        /// <summary>
        /// Gets or sets the publisher prefix for new Canvas apps.
        /// </summary>
        [Parameter(HelpMessage = "Publisher prefix for new Canvas apps (e.g., 'new'). Required for creation if Name doesn't contain prefix.")]
        public string PublisherPrefix { get; set; }

        /// <summary>
        /// Gets or sets whether to return the Canvas app ID.
        /// </summary>
        [Parameter(HelpMessage = "If set, returns the Canvas app ID")]
        public SwitchParameter PassThru { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Validate MsAppPath
            var msappFilePath = GetUnresolvedProviderPathFromPSPath(MsAppPath);
            if (!File.Exists(msappFilePath))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new FileNotFoundException($"MsApp file not found: {msappFilePath}"),
                    "FileNotFound",
                    ErrorCategory.ObjectNotFound,
                    msappFilePath));
                return;
            }

            // Read .msapp file
            byte[] msappBytes = File.ReadAllBytes(msappFilePath);
            string base64Document = Convert.ToBase64String(msappBytes);

            // Determine if we're creating or updating
            Guid canvasAppId;
            bool isUpdate = false;
            Entity existingApp = null;

            if (Id.HasValue && Id.Value != Guid.Empty)
            {
                // Try to find by ID
                try
                {
                    existingApp = Connection.Retrieve("canvasapp", Id.Value, new ColumnSet("canvasappid", "name"));
                    canvasAppId = Id.Value;
                    isUpdate = true;
                    WriteVerbose($"Found existing Canvas app by ID: {canvasAppId}");
                }
                catch
                {
                    // Doesn't exist, will create with this ID
                    canvasAppId = Id.Value;
                    isUpdate = false;
                    WriteVerbose($"Canvas app with ID {canvasAppId} not found, will create new");
                }
            }
            else if (!string.IsNullOrEmpty(Name))
            {
                // Try to find by name
                var foundId = FindCanvasAppByName(Name);
                if (foundId.HasValue)
                {
                    canvasAppId = foundId.Value;
                    existingApp = Connection.Retrieve("canvasapp", canvasAppId, new ColumnSet("canvasappid", "name"));
                    isUpdate = true;
                    WriteVerbose($"Found existing Canvas app by name '{Name}': {canvasAppId}");
                }
                else
                {
                    canvasAppId = Guid.NewGuid();
                    isUpdate = false;
                    WriteVerbose($"Canvas app '{Name}' not found, will create new with ID {canvasAppId}");
                }
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("Either Id or Name must be provided"),
                    "MissingIdentifier",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            // Perform upsert
            if (isUpdate)
            {
                PerformUpdate(canvasAppId, base64Document);
            }
            else
            {
                PerformCreate(canvasAppId, base64Document);
            }

            if (PassThru)
            {
                WriteObject(canvasAppId);
            }
        }

        private void PerformCreate(Guid canvasAppId, string base64Document)
        {
            // Validate we have required fields for creation
            if (string.IsNullOrEmpty(Name))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("Name is required for creating a new Canvas app"),
                    "MissingName",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            // Determine display name
            string displayName = DisplayName ?? Name;

            string action = $"Create Canvas app '{Name}'";
            if (!ShouldProcess(action, action, "Create Canvas App"))
            {
                return;
            }

            WriteVerbose($"Creating new Canvas app: {Name} (ID: {canvasAppId})");

            var entity = new Entity("canvasapp")
            {
                Id = canvasAppId,
                ["canvasappid"] = canvasAppId,
                ["name"] = Name,
                ["displayname"] = displayName,
                ["document"] = base64Document
            };

            if (!string.IsNullOrEmpty(Description))
            {
                entity["description"] = Description;
            }

            Connection.Create(entity);

            WriteVerbose($"Canvas app created successfully with ID: {canvasAppId}");
        }

        private void PerformUpdate(Guid canvasAppId, string base64Document)
        {
            string action = $"Update Canvas app '{canvasAppId}'";
            if (!ShouldProcess(action, action, "Update Canvas App"))
            {
                return;
            }

            WriteVerbose($"Updating Canvas app: {canvasAppId}");

            var entity = new Entity("canvasapp")
            {
                Id = canvasAppId,
                ["canvasappid"] = canvasAppId,
                ["document"] = base64Document
            }; 

            if (!string.IsNullOrEmpty(DisplayName))
            {
                entity["displayname"] = DisplayName;
            }

            if (!string.IsNullOrEmpty(Description))
            {
                entity["description"] = Description;
            }

            Connection.Update(entity);

            WriteVerbose($"Canvas app updated successfully: {canvasAppId}");
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
