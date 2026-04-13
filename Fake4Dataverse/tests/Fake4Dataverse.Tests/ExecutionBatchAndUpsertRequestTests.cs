using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class ExecutionBatchAndUpsertRequestTests
    {
        [Fact]
        public void ExecuteMultipleRequest_ProcessesAllRequests()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var requests = new OrganizationRequestCollection
            {
                new CreateRequest { Target = new Entity("account") { ["name"] = "A" } },
                new CreateRequest { Target = new Entity("account") { ["name"] = "B" } },
                new CreateRequest { Target = new Entity("account") { ["name"] = "C" } }
            };

            var response = (ExecuteMultipleResponse)service.Execute(new ExecuteMultipleRequest
            {
                Requests = requests,
                Settings = new ExecuteMultipleSettings { ContinueOnError = false, ReturnResponses = true }
            });

            Assert.Equal(3, response.Responses.Count);
            Assert.All(response.Responses, item => Assert.Null(item.Fault));
        }

        [Fact]
        public void ExecuteMultipleRequest_ContinueOnError_False_StopsOnFirstError()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var existingId = Guid.NewGuid();
            service.Create(new Entity("account", existingId) { ["name"] = "Existing" });

            var requests = new OrganizationRequestCollection
            {
                new CreateRequest { Target = new Entity("account") { ["name"] = "A" } },
                new CreateRequest { Target = new Entity("account", existingId) { ["name"] = "Dup" } },
                new CreateRequest { Target = new Entity("account") { ["name"] = "C" } }
            };

            var response = (ExecuteMultipleResponse)service.Execute(new ExecuteMultipleRequest
            {
                Requests = requests,
                Settings = new ExecuteMultipleSettings { ContinueOnError = false, ReturnResponses = true }
            });

            Assert.Equal(2, response.Responses.Count);
            Assert.Null(response.Responses[0].Fault);
            Assert.NotNull(response.Responses[1].Fault);
        }

        [Fact]
        public void ExecuteMultipleRequest_ContinueOnError_True_CollectsErrors()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var existingId = Guid.NewGuid();
            service.Create(new Entity("account", existingId) { ["name"] = "Existing" });

            var requests = new OrganizationRequestCollection
            {
                new CreateRequest { Target = new Entity("account") { ["name"] = "A" } },
                new CreateRequest { Target = new Entity("account", existingId) { ["name"] = "Dup" } },
                new CreateRequest { Target = new Entity("account") { ["name"] = "C" } }
            };

            var response = (ExecuteMultipleResponse)service.Execute(new ExecuteMultipleRequest
            {
                Requests = requests,
                Settings = new ExecuteMultipleSettings { ContinueOnError = true, ReturnResponses = true }
            });

            Assert.Equal(3, response.Responses.Count);
            Assert.Null(response.Responses[0].Fault);
            Assert.NotNull(response.Responses[1].Fault);
            Assert.Null(response.Responses[2].Fault);
        }

        [Fact]
        public void ExecuteTransactionRequest_ExecutesAll()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var requests = new OrganizationRequestCollection
            {
                new CreateRequest { Target = new Entity("account") { ["name"] = "A" } },
                new CreateRequest { Target = new Entity("account") { ["name"] = "B" } }
            };

            var response = (ExecuteTransactionResponse)service.Execute(new ExecuteTransactionRequest
            {
                Requests = requests
            });

            Assert.Equal(2, response.Responses.Count);
        }

        [Fact]
        public void ExecuteTransactionRequest_FailsOnError()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var existingId = Guid.NewGuid();
            service.Create(new Entity("account", existingId) { ["name"] = "Existing" });

            var requests = new OrganizationRequestCollection
            {
                new CreateRequest { Target = new Entity("account") { ["name"] = "A" } },
                new CreateRequest { Target = new Entity("account", existingId) { ["name"] = "Dup" } }
            };

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new ExecuteTransactionRequest
                {
                    Requests = requests
                }));
        }

        [Fact]
        public void UpsertRequest_CreatesWhenNew()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (UpsertResponse)service.Execute(new UpsertRequest
            {
                Target = new Entity("account") { ["name"] = "NewCo" }
            });

            Assert.True((bool)response.Results["RecordCreated"]);
            var targetRef = (EntityReference)response.Results["Target"];
            Assert.NotEqual(Guid.Empty, targetRef.Id);

            var retrieved = service.Retrieve("account", targetRef.Id, new ColumnSet("name"));
            Assert.Equal("NewCo", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void UpsertRequest_UpdatesWhenExisting()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Original" });

            var response = (UpsertResponse)service.Execute(new UpsertRequest
            {
                Target = new Entity("account", id) { ["name"] = "Updated" }
            });

            Assert.False((bool)response.Results["RecordCreated"]);
            var retrieved = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Updated", retrieved.GetAttributeValue<string>("name"));
        }
    }
}
