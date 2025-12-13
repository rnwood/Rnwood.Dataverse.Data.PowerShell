using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Applies a staged solution upgrade by deleting the original solution and promoting the holding solution.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseSolutionUpgrade", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class InvokeDataverseSolutionUpgradeCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the unique name of the solution to upgrade.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The unique name of the solution to upgrade (e.g., 'MySolution'). The holding solution 'MySolution_Upgrade' must exist.")]
        [ValidateNotNullOrEmpty]
        public string SolutionName { get; set; }

        /// <summary>
        /// Gets or sets whether to check if the holding solution exists before attempting to apply the upgrade.
        /// </summary>
        [Parameter(HelpMessage = "Check if the holding solution (SolutionName_Upgrade) exists before attempting to apply the upgrade. If it doesn't exist, skip the operation.")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string holdingSolutionName = $"{SolutionName}_Upgrade";

            // Check if holding solution exists when IfExists is specified
            if (IfExists.IsPresent)
            {
                WriteVerbose($"Checking if holding solution '{holdingSolutionName}' exists...");
                
                if (!DoesHoldingSolutionExist(holdingSolutionName))
                {
                    WriteWarning($"Holding solution '{holdingSolutionName}' does not exist. Skipping upgrade operation.");
                    return;
                }

                WriteVerbose($"Holding solution '{holdingSolutionName}' exists.");
            }

            if (!ShouldProcess($"Solution '{SolutionName}'", "Apply upgrade"))
            {
                return;
            }

            WriteVerbose($"Applying upgrade for solution '{SolutionName}' using holding solution '{holdingSolutionName}'...");

            // Create and execute DeleteAndPromoteRequest
            var request = new DeleteAndPromoteRequest
            {
                UniqueName = SolutionName
            };

            Connection.Execute(request);

            WriteVerbose($"Successfully applied upgrade for solution '{SolutionName}'.");

            // Output result
            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("SolutionName", SolutionName));
            result.Properties.Add(new PSNoteProperty("HoldingSolutionName", holdingSolutionName));
            result.Properties.Add(new PSNoteProperty("Status", "Success"));
            WriteObject(result);
        }

        private bool DoesHoldingSolutionExist(string holdingSolutionName)
        {
            var query = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet("uniquename"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("uniquename", ConditionOperator.Equal, holdingSolutionName)
                    }
                },
                TopCount = 1
            };

            var solutions = Connection.RetrieveMultiple(query);
            return solutions.Entities.Count > 0;
        }
    }
}
