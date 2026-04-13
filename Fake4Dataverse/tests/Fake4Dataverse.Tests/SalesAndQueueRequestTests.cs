using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class SalesAndQueueRequestTests
    {
        [Fact]
        public void QualifyLead_CreatesAccountContactOpportunity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var leadId = service.Create(new Entity("lead")
            {
                ["firstname"] = "John",
                ["lastname"] = "Doe",
                ["companyname"] = "Contoso",
                ["subject"] = "Hot Lead"
            });

            var request = new OrganizationRequest("QualifyLead");
            request["LeadId"] = new EntityReference("lead", leadId);
            request["CreateAccount"] = true;
            request["CreateContact"] = true;
            request["CreateOpportunity"] = true;
            request["Status"] = new OptionSetValue(3);

            var response = service.Execute(request);
            var created = (EntityReferenceCollection)response["CreatedEntities"];

            Assert.Equal(3, created.Count);

            var lead = service.Retrieve("lead", leadId, new ColumnSet("statecode"));
            Assert.Equal(1, lead.GetAttributeValue<OptionSetValue>("statecode").Value);
        }

        [Fact]
        public void CloseIncident_ResolvesCase()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var incidentId = service.Create(new Entity("incident") { ["title"] = "Test Case" });

            var resolution = new Entity("incidentresolution");
            resolution["incidentid"] = new EntityReference("incident", incidentId);
            resolution["subject"] = "Resolved";

            var request = new OrganizationRequest("CloseIncident");
            request["IncidentResolution"] = resolution;
            request["Status"] = new OptionSetValue(5);

            service.Execute(request);

            var incident = service.Retrieve("incident", incidentId, new ColumnSet("statecode", "statuscode"));
            Assert.Equal(1, incident.GetAttributeValue<OptionSetValue>("statecode").Value);
            Assert.Equal(5, incident.GetAttributeValue<OptionSetValue>("statuscode").Value);
        }

        [Fact]
        public void WinOpportunity_ClosesAsWon()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var oppId = service.Create(new Entity("opportunity") { ["name"] = "Big Deal" });

            var oppClose = new Entity("opportunityclose");
            oppClose["opportunityid"] = new EntityReference("opportunity", oppId);
            oppClose["subject"] = "Won!";

            var request = new OrganizationRequest("WinOpportunity");
            request["OpportunityClose"] = oppClose;
            request["Status"] = new OptionSetValue(3);

            service.Execute(request);

            var opp = service.Retrieve("opportunity", oppId, new ColumnSet("statecode"));
            Assert.Equal(1, opp.GetAttributeValue<OptionSetValue>("statecode").Value);
        }

        [Fact]
        public void LoseOpportunity_ClosesAsLost()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var oppId = service.Create(new Entity("opportunity") { ["name"] = "Lost Deal" });

            var oppClose = new Entity("opportunityclose");
            oppClose["opportunityid"] = new EntityReference("opportunity", oppId);
            oppClose["subject"] = "Lost";

            var request = new OrganizationRequest("LoseOpportunity");
            request["OpportunityClose"] = oppClose;
            request["Status"] = new OptionSetValue(4);

            service.Execute(request);

            var opp = service.Retrieve("opportunity", oppId, new ColumnSet("statecode"));
            Assert.Equal(2, opp.GetAttributeValue<OptionSetValue>("statecode").Value);
        }

        [Fact]
        public void CloseQuote_ClosesQuote()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var quoteId = service.Create(new Entity("quote") { ["name"] = "Test Quote" });

            var quoteClose = new Entity("quoteclose");
            quoteClose["quoteid"] = new EntityReference("quote", quoteId);
            quoteClose["subject"] = "Closed";

            var request = new OrganizationRequest("CloseQuote");
            request["QuoteClose"] = quoteClose;
            request["Status"] = new OptionSetValue(5);

            service.Execute(request);

            var quote = service.Retrieve("quote", quoteId, new ColumnSet("statecode"));
            Assert.Equal(2, quote.GetAttributeValue<OptionSetValue>("statecode").Value);
        }

        [Fact]
        public void ReviseQuote_CreatesNewDraftCopy()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var quoteId = service.Create(new Entity("quote") { ["name"] = "Original Quote", ["description"] = "Test" });

            var request = new OrganizationRequest("ReviseQuote");
            request["QuoteId"] = new EntityReference("quote", quoteId);
            request["ColumnSet"] = new ColumnSet(true);

            var response = service.Execute(request);
            var revised = (Entity)response["Entity"];

            Assert.NotEqual(quoteId, revised.Id);
            Assert.Equal("Original Quote", revised.GetAttributeValue<string>("name"));
            Assert.Equal(0, revised.GetAttributeValue<OptionSetValue>("statecode").Value);
        }

        [Fact]
        public void AddToQueue_CreatesQueueItem()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var caseId = service.Create(new Entity("incident") { ["title"] = "Queue Case" });
            var queueId = service.Create(new Entity("queue") { ["name"] = "Support Queue" });

            var request = new OrganizationRequest("AddToQueue");
            request["Target"] = new EntityReference("incident", caseId);
            request["DestinationQueueId"] = queueId;

            var response = service.Execute(request);
            var queueItemId = (Guid)response["QueueItemId"];
            Assert.NotEqual(Guid.Empty, queueItemId);

            var queueItem = service.Retrieve("queueitem", queueItemId, new ColumnSet(true));
            Assert.Equal(caseId, queueItem.GetAttributeValue<EntityReference>("objectid").Id);
        }

        [Fact]
        public void RemoveFromQueue_DeletesQueueItem()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var caseId = service.Create(new Entity("incident") { ["title"] = "Queue Case" });
            var queueId = service.Create(new Entity("queue") { ["name"] = "Support Queue" });

            var addRequest = new OrganizationRequest("AddToQueue");
            addRequest["Target"] = new EntityReference("incident", caseId);
            addRequest["DestinationQueueId"] = queueId;
            var addResponse = service.Execute(addRequest);
            var queueItemId = (Guid)addResponse["QueueItemId"];

            var removeRequest = new OrganizationRequest("RemoveFromQueue");
            removeRequest["QueueItemId"] = queueItemId;
            service.Execute(removeRequest);

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Retrieve("queueitem", queueItemId, new ColumnSet(true)));
        }

        [Fact]
        public void WinQuote_ClosesQuoteAsWon()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var quoteId = service.Create(new Entity("quote") { ["name"] = "Win Quote" });

            var quoteClose = new Entity("quoteclose");
            quoteClose["quoteid"] = new EntityReference("quote", quoteId);
            quoteClose["subject"] = "Won!";

            var request = new OrganizationRequest("WinQuote");
            request["QuoteClose"] = quoteClose;
            request["Status"] = new OptionSetValue(4);

            service.Execute(request);

            var quote = service.Retrieve("quote", quoteId, new ColumnSet("statecode", "statuscode"));
            Assert.Equal(3, quote.GetAttributeValue<OptionSetValue>("statecode").Value);
            Assert.Equal(4, quote.GetAttributeValue<OptionSetValue>("statuscode").Value);
        }

        [Fact]
        public void LockInvoicePricing_SetsIsLocked()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var invoiceId = service.Create(new Entity("invoice") { ["name"] = "INV-001" });

            var request = new OrganizationRequest("LockInvoicePricing");
            request["InvoiceId"] = invoiceId;
            service.Execute(request);

            var invoice = service.Retrieve("invoice", invoiceId, new ColumnSet("ispricelocked"));
            Assert.True((bool)invoice["ispricelocked"]);
        }

        [Fact]
        public void UnlockInvoicePricing_ClearsIsLocked()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var invoiceId = service.Create(new Entity("invoice") { ["name"] = "INV-001", ["ispricelocked"] = true });

            var request = new OrganizationRequest("UnlockInvoicePricing");
            request["InvoiceId"] = invoiceId;
            service.Execute(request);

            var invoice = service.Retrieve("invoice", invoiceId, new ColumnSet("ispricelocked"));
            Assert.False((bool)invoice["ispricelocked"]);
        }

        [Fact]
        public void LockSalesOrderPricing_SetsIsLocked()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var orderId = service.Create(new Entity("salesorder") { ["name"] = "SO-001" });

            var request = new OrganizationRequest("LockSalesOrderPricing");
            request["SalesOrderId"] = orderId;
            service.Execute(request);

            var order = service.Retrieve("salesorder", orderId, new ColumnSet("ispricelocked"));
            Assert.True((bool)order["ispricelocked"]);
        }

        [Fact]
        public void UnlockSalesOrderPricing_ClearsIsLocked()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var orderId = service.Create(new Entity("salesorder") { ["name"] = "SO-001", ["ispricelocked"] = true });

            var request = new OrganizationRequest("UnlockSalesOrderPricing");
            request["SalesOrderId"] = orderId;
            service.Execute(request);

            var order = service.Retrieve("salesorder", orderId, new ColumnSet("ispricelocked"));
            Assert.False((bool)order["ispricelocked"]);
        }

        [Fact]
        public void RemoveParent_ClearsParentReference()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var parentId = service.Create(new Entity("account") { ["name"] = "Parent" });
            var childId = service.Create(new Entity("account")
            {
                ["name"] = "Child",
                ["parentid"] = new EntityReference("account", parentId)
            });

            var request = new OrganizationRequest("RemoveParent");
            request["Target"] = new EntityReference("account", childId);
            service.Execute(request);

            var child = service.Retrieve("account", childId, new ColumnSet("parentid"));
            Assert.Null(child.GetAttributeValue<EntityReference>("parentid"));
        }

        [Fact]
        public void Reschedule_UpdatesAppointmentDates()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
            var apptId = service.Create(new Entity("appointment")
            {
                ["subject"] = "Meeting",
                ["scheduledstart"] = start,
                ["scheduledend"] = start.AddHours(1)
            });

            var newStart = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc);
            var target = new Entity("appointment", apptId);
            target["scheduledstart"] = newStart;
            target["scheduledend"] = newStart.AddHours(2);

            var request = new OrganizationRequest("Reschedule");
            request["Target"] = target;
            service.Execute(request);

            var appt = service.Retrieve("appointment", apptId, new ColumnSet("scheduledstart", "scheduledend"));
            Assert.Equal(newStart, appt.GetAttributeValue<DateTime>("scheduledstart"));
        }

        [Fact]
        public void Recalculate_ExecutesWithoutError()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var oppId = service.Create(new Entity("opportunity") { ["name"] = "Test" });

            var request = new OrganizationRequest("Recalculate");
            request["Target"] = new EntityReference("opportunity", oppId);
            var response = service.Execute(request);
            Assert.NotNull(response);
        }

        [Fact]
        public void RenewContract_CreatesRenewalCopy()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var contractId = service.Create(new Entity("contract")
            {
                ["title"] = "Contract A",
                ["contractnumber"] = "C-001"
            });

            var request = new OrganizationRequest("RenewContract");
            request["ContractId"] = contractId;
            request["Status"] = 1;
            request["IncludeCanceledLines"] = false;
            var response = service.Execute(request);

            var renewed = (Entity)response["Entity"];
            Assert.NotEqual(contractId, renewed.Id);
            Assert.Equal("Contract A", renewed.GetAttributeValue<string>("title"));
            Assert.Equal(0, renewed.GetAttributeValue<OptionSetValue>("statecode").Value);
        }

        [Fact]
        public void RouteTo_MovesRecordBetweenQueues()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var caseId = service.Create(new Entity("incident") { ["title"] = "Route Case" });
            var queue1 = service.Create(new Entity("queue") { ["name"] = "Queue 1" });
            var queue2 = service.Create(new Entity("queue") { ["name"] = "Queue 2" });

            // Add to first queue
            var addRequest = new OrganizationRequest("AddToQueue");
            addRequest["Target"] = new EntityReference("incident", caseId);
            addRequest["DestinationQueueId"] = queue1;
            service.Execute(addRequest);

            // Route to second queue
            var routeRequest = new OrganizationRequest("RouteTo");
            routeRequest["Target"] = new EntityReference("incident", caseId);
            routeRequest["QueueId"] = queue2;
            var routeResponse = service.Execute(routeRequest);

            Assert.NotEqual(Guid.Empty, (Guid)routeResponse["QueueItemId"]);
            var items = service.RetrieveMultiple(new QueryExpression("queueitem") { ColumnSet = new ColumnSet(true) });
            Assert.Single(items.Entities);
            Assert.Equal(queue2, items.Entities[0].GetAttributeValue<EntityReference>("queueid").Id);
        }
    }
}
