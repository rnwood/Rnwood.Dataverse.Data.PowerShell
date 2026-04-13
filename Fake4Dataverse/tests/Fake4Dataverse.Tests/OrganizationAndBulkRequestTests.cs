using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class OrganizationAndBulkRequestTests
    {
        [Fact]
        public void Merge_CopiesAttributesAndDeactivatesSubordinate()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var targetId = service.Create(new Entity("account") { ["name"] = "Target", ["city"] = "Seattle" });
            var subordinateId = service.Create(new Entity("account") { ["name"] = "Subordinate", ["revenue"] = new Money(500m) });

            var request = new OrganizationRequest("Merge");
            request["Target"] = new EntityReference("account", targetId);
            request["SubordinateId"] = subordinateId;
            request["UpdateContent"] = new Entity("account") { ["revenue"] = new Money(500m) };
            request["PerformParentingChecks"] = false;

            service.Execute(request);

            var target = service.Retrieve("account", targetId, new ColumnSet("revenue"));
            Assert.Equal(500m, target.GetAttributeValue<Money>("revenue").Value);

            var subordinate = service.Retrieve("account", subordinateId, new ColumnSet("statecode"));
            Assert.Equal(1, subordinate.GetAttributeValue<OptionSetValue>("statecode").Value);
        }

        [Fact]
        public void UpsertMultiple_CreatesAndUpdatesRecords()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var existingId = service.Create(new Entity("account") { ["name"] = "Existing" });

            var targets = new EntityCollection();
            targets.Entities.Add(new Entity("account", existingId) { ["name"] = "Updated" });
            targets.Entities.Add(new Entity("account") { ["name"] = "New" });

            var request = new OrganizationRequest("UpsertMultiple");
            request["Targets"] = targets;

            service.Execute(request);

            var existing = service.Retrieve("account", existingId, new ColumnSet("name"));
            Assert.Equal("Updated", existing.GetAttributeValue<string>("name"));

            var all = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Equal(2, all.Entities.Count);
        }

        [Fact]
        public void BulkDelete_DeletesMatchingRecords()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Delete1" });
            service.Create(new Entity("account") { ["name"] = "Delete2" });
            service.Create(new Entity("account") { ["name"] = "Keep" });

            var query = new QueryExpression("account");
            query.Criteria.AddCondition("name", ConditionOperator.BeginsWith, "Delete");

            var request = new OrganizationRequest("BulkDelete");
            request["QuerySet"] = new[] { query };
            request["JobName"] = "TestBulkDelete";
            request["StartDateTime"] = DateTime.UtcNow;
            request["RecurrencePattern"] = string.Empty;
            request["SendEmailNotification"] = false;
            request["ToRecipients"] = Array.Empty<Guid>();
            request["CCRecipients"] = Array.Empty<Guid>();

            var response = service.Execute(request);
            Assert.NotEqual(Guid.Empty, (Guid)response["JobId"]);

            var remaining = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Single(remaining.Entities);
            Assert.Equal("Keep", remaining.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void DeleteMultiple_DeletesAllTargets()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var account1 = service.Create(new Entity("account") { ["name"] = "Delete 1" });
            var account2 = service.Create(new Entity("account") { ["name"] = "Delete 2" });
            service.Create(new Entity("account") { ["name"] = "Keep" });

            service.Execute(new DeleteMultipleRequest
            {
                Targets = new EntityReferenceCollection
                {
                    new EntityReference("account", account1),
                    new EntityReference("account", account2)
                }
            });

            var remaining = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet("name") });
            Assert.Single(remaining.Entities);
            Assert.Equal("Keep", remaining.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public void RetrieveCurrentOrganization_ReturnsOrgDetails()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = service.Execute(new OrganizationRequest("RetrieveCurrentOrganization"));
            var detail = (Entity)response["Detail"];

            Assert.NotNull(detail);
            Assert.Equal(env.OrganizationId, detail.GetAttributeValue<Guid>("organizationid"));
        }

        [Fact]
        public void RetrieveOptionSet_ReturnsResponse()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var request = new RetrieveOptionSetRequest
            {
                Name = "account_category",
                MetadataId = Guid.Empty
            };

            var response = service.Execute(request);
            Assert.NotNull(response);
        }

        [Fact]
        public void InsertOptionValue_ReturnsNewValue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var request = new InsertOptionValueRequest
            {
                OptionSetName = "account_category",
                Label = new Label("Test Option", 1033),
                Value = 999
            };

            var response = (InsertOptionValueResponse)service.Execute(request);
            Assert.Equal(999, response.NewOptionValue);
        }

        [Fact]
        public void InsertStatusValue_ReturnsNewValue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var request = new InsertStatusValueRequest
            {
                EntityLogicalName = "incident",
                AttributeLogicalName = "statuscode",
                Label = new Label("Custom Status", 1033),
                Value = 100000,
                StateCode = 0
            };

            var response = (InsertStatusValueResponse)service.Execute(request);
            Assert.Equal(100000, response.NewOptionValue);
        }

        [Fact]
        public void PublishXml_ExecutesWithoutError()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var request = new OrganizationRequest("PublishXml");
            request["ParameterXml"] = "<importexportxml></importexportxml>";

            var response = service.Execute(request);
            Assert.NotNull(response);
        }

        [Fact]
        public void ExportPdfDocument_ReturnsPdfBytes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var request = new OrganizationRequest("ExportPdfDocument");
            request["EntityTypeCode"] = 1;
            request["SelectedRecords"] = "[]";

            var response = service.Execute(request);
            Assert.NotNull(response["PdfFile"]);
        }

        [Fact]
        public void RetrieveDeploymentLicenseType_ReturnsLicenseType()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = service.Execute(new OrganizationRequest("RetrieveDeploymentLicenseType"));
            Assert.Equal("OnPremise", (string)response["licenseType"]);
        }

        [Fact]
        public void RetrieveLicenseInfo_ReturnsDefaults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = service.Execute(new OrganizationRequest("RetrieveLicenseInfo"));
            Assert.Equal(100, (int)response["AvailableCount"]);
            Assert.Equal(100, (int)response["GrantedLicenseCount"]);
        }

        [Fact]
        public void RetrieveUserQueues_ReturnsOwnedQueues()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var userId = Guid.NewGuid();
            service.Create(new Entity("queue") { ["name"] = "My Queue", ["ownerid"] = new EntityReference("systemuser", userId) });
            service.Create(new Entity("queue") { ["name"] = "Other Queue", ["ownerid"] = new EntityReference("systemuser", Guid.NewGuid()) });

            var request = new OrganizationRequest("RetrieveUserQueues");
            request["UserId"] = userId;
            request["IncludePublic"] = false;
            var response = service.Execute(request);
            var collection = (EntityCollection)response["EntityCollection"];

            Assert.Single(collection.Entities);
        }
    }
}
