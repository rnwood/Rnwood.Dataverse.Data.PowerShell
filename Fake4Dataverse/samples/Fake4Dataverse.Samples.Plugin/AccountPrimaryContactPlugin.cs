using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Samples.Plugin
{
    /// <summary>
    /// Sample Dataverse plugin that creates a related primary contact when an account is created.
    /// </summary>
    public sealed class AccountPrimaryContactPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var context = (IPluginExecutionContext?)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory?)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var tracing = (ITracingService?)serviceProvider.GetService(typeof(ITracingService));

            if (context == null || factory == null)
            {
                return;
            }

            if (!context.InputParameters.Contains("Target") || context.InputParameters["Target"] is not Entity target)
            {
                return;
            }

            var accountName = target.GetAttributeValue<string>("name");
            if (string.IsNullOrWhiteSpace(accountName))
            {
                return;
            }

            var accountId = target.Id;
            if (accountId == Guid.Empty && context.OutputParameters.Contains("id"))
            {
                accountId = (Guid)context.OutputParameters["id"];
            }

            if (accountId == Guid.Empty)
            {
                return;
            }

            var orgService = factory.CreateOrganizationService(context.UserId);

            orgService.Create(new Entity("contact")
            {
                ["parentcustomerid"] = new EntityReference("account", accountId),
                ["lastname"] = accountName + " Primary Contact"
            });

            tracing?.Trace("Created primary contact for account {0}", accountId);
        }
    }
}
