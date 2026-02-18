using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Compares two Copilot Studio bot components and shows their differences.
    /// </summary>
    [Cmdlet(VerbsData.Compare, "DataverseBotComponent")]
    [OutputType(typeof(PSObject))]
    public class CompareDataverseBotComponentCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the first bot component ID to compare.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "First bot component ID (GUID) to compare.")]
        public Guid ComponentId1 { get; set; }

        /// <summary>
        /// Gets or sets the second bot component ID to compare.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Second bot component ID (GUID) to compare.")]
        public Guid ComponentId2 { get; set; }

        /// <summary>
        /// Gets or sets which attributes to compare. If not specified, compares key attributes.
        /// </summary>
        [Parameter(HelpMessage = "Specific attributes to compare. If not specified, compares: name, description, data, content, category, componenttype.")]
        public string[] Attributes { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Retrieve both components
            Entity component1 = Connection.Retrieve("botcomponent", ComponentId1, new ColumnSet(true));
            Entity component2 = Connection.Retrieve("botcomponent", ComponentId2, new ColumnSet(true));

            string name1 = component1.GetAttributeValue<string>("name");
            string name2 = component2.GetAttributeValue<string>("name");

            WriteVerbose($"Comparing '{name1}' with '{name2}'");

            // Determine which attributes to compare
            string[] attributesToCompare = Attributes ?? new[]
            {
                "name",
                "description",
                "data",
                "content",
                "category",
                "componenttype",
                "language",
                "schemaname",
                "accentcolor",
                "helplink",
                "iconurl"
            };

            var differences = new List<PSObject>();

            foreach (string attr in attributesToCompare)
            {
                object value1 = component1.Contains(attr) ? component1[attr] : null;
                object value2 = component2.Contains(attr) ? component2[attr] : null;

                // Convert EntityReference to string for comparison
                string stringValue1 = ConvertValueToString(value1);
                string stringValue2 = ConvertValueToString(value2);

                bool isDifferent = !string.Equals(stringValue1, stringValue2, StringComparison.Ordinal);

                var diffObject = new PSObject();
                diffObject.Properties.Add(new PSNoteProperty("Attribute", attr));
                diffObject.Properties.Add(new PSNoteProperty("Component1Value", stringValue1));
                diffObject.Properties.Add(new PSNoteProperty("Component2Value", stringValue2));
                diffObject.Properties.Add(new PSNoteProperty("IsDifferent", isDifferent));

                differences.Add(diffObject);
            }

            // Output summary
            int differentCount = differences.Count(d => (bool)d.Properties["IsDifferent"].Value);
            WriteVerbose($"Found {differentCount} differences out of {differences.Count} attributes compared");

            // Write all differences
            foreach (var diff in differences)
            {
                WriteObject(diff);
            }
        }

        private string ConvertValueToString(object value)
        {
            if (value == null)
                return string.Empty;

            if (value is EntityReference entityRef)
                return $"{entityRef.LogicalName}:{entityRef.Id}";

            if (value is OptionSetValue optionSet)
                return optionSet.Value.ToString();

            if (value is Money money)
                return money.Value.ToString();

            return value.ToString();
        }
    }
}
