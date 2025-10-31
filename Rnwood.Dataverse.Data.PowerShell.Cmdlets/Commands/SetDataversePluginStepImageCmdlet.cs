using Microsoft.Xrm.Sdk;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates an SDK message processing step image (plugin step image) in a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataversePluginStepImage", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class SetDataversePluginStepImageCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin step image to update.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin step image to update. If not specified, a new image is created.")]
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the plugin step ID this image belongs to.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Plugin step ID this image belongs to")]
        public Guid SdkMessageProcessingStepId { get; set; }

        /// <summary>
        /// Gets or sets the entity alias for the image.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Entity alias for the image (used to reference the image in plugin code)")]
        public string EntityAlias { get; set; }

        /// <summary>
        /// Gets or sets the image type. 0=PreImage, 1=PostImage, 2=Both
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Image type: 0=PreImage, 1=PostImage, 2=Both")]
        public int ImageType { get; set; }

        /// <summary>
        /// Gets or sets the message property name.
        /// </summary>
        [Parameter(HelpMessage = "Message property name. Default is 'Target' for most messages.")]
        public string MessagePropertyName { get; set; } = "Target";

        /// <summary>
        /// Gets or sets the attributes (comma-separated list of attribute logical names).
        /// </summary>
        [Parameter(HelpMessage = "Attributes to include in the image (comma-separated list of attribute logical names). Leave empty for all attributes.")]
        public string Attributes { get; set; }

        /// <summary>
        /// Gets or sets the name of the image.
        /// </summary>
        [Parameter(HelpMessage = "Name of the image")]
        public string Name { get; set; }

        /// <summary>
        /// If specified, the created/updated image is written to the pipeline as a PSObject.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the created/updated image is written to the pipeline as a PSObject")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Entity image = new Entity("sdkmessageprocessingstepimage");
            if (Id.HasValue)
            {
                image.Id = Id.Value;
            }

            image["sdkmessageprocessingstepid"] = new EntityReference("sdkmessageprocessingstep", SdkMessageProcessingStepId);
            image["entityalias"] = EntityAlias;
            image["imagetype"] = new OptionSetValue(ImageType);
            image["messagepropertyname"] = MessagePropertyName;

            if (!string.IsNullOrEmpty(Attributes))
            {
                image["attributes"] = Attributes;
            }

            if (!string.IsNullOrEmpty(Name))
            {
                image["name"] = Name;
            }

            if (ShouldProcess($"Plugin Step Image: {EntityAlias}", Id.HasValue ? "Update" : "Create"))
            {
                Guid imageId;
                if (Id.HasValue)
                {
                    Connection.Update(image);
                    imageId = Id.Value;
                    WriteVerbose($"Updated plugin step image: {EntityAlias} (ID: {imageId})");
                }
                else
                {
                    imageId = Connection.Create(image);
                    WriteVerbose($"Created plugin step image: {EntityAlias} (ID: {imageId})");
                }

                if (PassThru)
                {
                    Entity retrieved = Connection.Retrieve("sdkmessageprocessingstepimage", imageId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                    EntityMetadataFactory entityMetadataFactory = new EntityMetadataFactory(Connection);
                    DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                    PSObject psObject = converter.ConvertToPSObject(retrieved, new Microsoft.Xrm.Sdk.Query.ColumnSet(true), _ => ValueType.Display);
                    WriteObject(psObject);
                }
            }
        }
    }
}
