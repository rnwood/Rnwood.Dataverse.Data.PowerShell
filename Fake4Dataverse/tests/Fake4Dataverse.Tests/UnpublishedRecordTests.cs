using System;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class UnpublishedRecordTests
    {
        // ── Create ──────────────────────────────────────────────────────────

        [Fact]
        public void Create_SolutionAwareEntity_RecordGoesToUnpublishedStore()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("webresource") { ["name"] = "test.js" });

            Assert.NotEqual(Guid.Empty, id);

            // Normal Retrieve should not find the unpublished record
            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Retrieve("webresource", id, new ColumnSet(true)));
        }

        [Fact]
        public void Create_SolutionAwareEntity_RetrieveUnpublishedFindsRecord()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("webresource") { ["name"] = "test.js" });

            var request = new OrganizationRequest("RetrieveUnpublished")
            {
                Parameters =
                {
                    ["Target"] = new EntityReference("webresource", id),
                    ["ColumnSet"] = new ColumnSet(true)
                }
            };
            var response = service.Execute(request);
            var entity = (Entity)response.Results["Entity"];

            Assert.Equal(id, entity.Id);
            Assert.Equal("test.js", entity["name"]);
        }

        [Fact]
        public void Create_SolutionAwareEntity_ComponentStateIsUnpublished()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("webresource") { ["name"] = "test.js" });

            var request = new OrganizationRequest("RetrieveUnpublished")
            {
                Parameters =
                {
                    ["Target"] = new EntityReference("webresource", id),
                    ["ColumnSet"] = new ColumnSet(true)
                }
            };
            var response = service.Execute(request);
            var entity = (Entity)response.Results["Entity"];

            var componentState = entity.GetAttributeValue<OptionSetValue>("componentstate");
            Assert.NotNull(componentState);
            Assert.Equal(1, componentState.Value); // 1 = Unpublished
        }

        [Fact]
        public void Create_NonSolutionAwareEntity_WorksNormally()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var entity = service.Retrieve("account", id, new ColumnSet(true));
            Assert.Equal("Contoso", entity["name"]);
        }

        // ── RetrieveMultiple / RetrieveUnpublishedMultiple ──────────────────

        [Fact]
        public void RetrieveMultiple_SolutionAwareEntity_DoesNotReturnUnpublished()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            service.Create(new Entity("webresource") { ["name"] = "test.js" });

            var result = service.RetrieveMultiple(new QueryExpression("webresource") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(result.Entities);
        }

        [Fact]
        public void RetrieveUnpublishedMultiple_ReturnsOnlyUnpublished()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            service.Create(new Entity("webresource") { ["name"] = "a.js" });
            service.Create(new Entity("webresource") { ["name"] = "b.js" });

            var request = new OrganizationRequest("RetrieveUnpublishedMultiple")
            {
                Parameters =
                {
                    ["Query"] = new QueryExpression("webresource") { ColumnSet = new ColumnSet(true) }
                }
            };
            var response = service.Execute(request);
            var result = (EntityCollection)response.Results["EntityCollection"];

            Assert.Equal(2, result.Entities.Count);
        }

        // ── Publish ─────────────────────────────────────────────────────────

        [Fact]
        public void PublishAllXml_MovesRecordsToPublishedStore()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("webresource") { ["name"] = "test.js" });

            // Before publish, not visible normally
            var beforePublish = service.RetrieveMultiple(new QueryExpression("webresource") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(beforePublish.Entities);

            // Publish
            service.Execute(new OrganizationRequest("PublishAllXml"));

            // After publish, visible normally
            var entity = service.Retrieve("webresource", id, new ColumnSet(true));
            Assert.Equal("test.js", entity["name"]);
            Assert.Equal(0, entity.GetAttributeValue<OptionSetValue>("componentstate").Value); // 0 = Published
        }

        [Fact]
        public void PublishAllXml_UnpublishedStoreIsEmptyAfterPublish()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            service.Create(new Entity("webresource") { ["name"] = "test.js" });
            service.Execute(new OrganizationRequest("PublishAllXml"));

            var request = new OrganizationRequest("RetrieveUnpublishedMultiple")
            {
                Parameters =
                {
                    ["Query"] = new QueryExpression("webresource") { ColumnSet = new ColumnSet(true) }
                }
            };
            var response = service.Execute(request);
            var result = (EntityCollection)response.Results["EntityCollection"];
            Assert.Empty(result.Entities);
        }

        [Fact]
        public void PublishXml_WithParameterXml_PublishesSpecificEntity()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            env.RegisterSolutionAwareEntity("savedquery");
            var service = env.CreateOrganizationService();

            var wrId = service.Create(new Entity("webresource") { ["name"] = "test.js" });
            var sqId = service.Create(new Entity("savedquery") { ["name"] = "My View" });

            // Publish only webresource
            var publishRequest = new OrganizationRequest("PublishXml")
            {
                Parameters =
                {
                    ["ParameterXml"] = "<importexportxml><entities><entity>webresource</entity></entities></importexportxml>"
                }
            };
            service.Execute(publishRequest);

            // webresource should be published
            var wr = service.Retrieve("webresource", wrId, new ColumnSet(true));
            Assert.Equal("test.js", wr["name"]);

            // savedquery should still be unpublished only
            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Retrieve("savedquery", sqId, new ColumnSet(true)));
        }

        [Fact]
        public void PublishAllXmlAsync_PublishesAllRecords()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("webresource") { ["name"] = "test.js" });
            service.Execute(new OrganizationRequest("PublishAllXmlAsync"));

            var entity = service.Retrieve("webresource", id, new ColumnSet(true));
            Assert.Equal("test.js", entity["name"]);
        }

        // ── Update ──────────────────────────────────────────────────────────

        [Fact]
        public void Update_SolutionAwareEntity_UpdateGoesToUnpublished()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            // Create and publish
            var id = service.Create(new Entity("webresource") { ["name"] = "old.js", ["content"] = "v1" });
            service.Execute(new OrganizationRequest("PublishAllXml"));

            // Update (should go to unpublished)
            service.Update(new Entity("webresource", id) { ["content"] = "v2" });

            // Published version should still be v1
            var published = service.Retrieve("webresource", id, new ColumnSet(true));
            Assert.Equal("v1", published["content"]);

            // Unpublished version should be v2
            var request = new OrganizationRequest("RetrieveUnpublished")
            {
                Parameters =
                {
                    ["Target"] = new EntityReference("webresource", id),
                    ["ColumnSet"] = new ColumnSet(true)
                }
            };
            var unpublished = (Entity)service.Execute(request).Results["Entity"];
            Assert.Equal("v2", unpublished["content"]);
        }

        [Fact]
        public void Update_ThenPublish_PublishedReflectsUpdate()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("webresource") { ["name"] = "test.js", ["content"] = "v1" });
            service.Execute(new OrganizationRequest("PublishAllXml"));

            service.Update(new Entity("webresource", id) { ["content"] = "v2" });
            service.Execute(new OrganizationRequest("PublishAllXml"));

            var entity = service.Retrieve("webresource", id, new ColumnSet(true));
            Assert.Equal("v2", entity["content"]);
        }

        [Fact]
        public void Update_UnpublishedOnly_UpdatesExistingDraft()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            // Create (goes to unpublished, never published)
            var id = service.Create(new Entity("webresource") { ["name"] = "test.js", ["content"] = "v1" });

            // Update the unpublished record
            service.Update(new Entity("webresource", id) { ["content"] = "v2" });

            var request = new OrganizationRequest("RetrieveUnpublished")
            {
                Parameters =
                {
                    ["Target"] = new EntityReference("webresource", id),
                    ["ColumnSet"] = new ColumnSet(true)
                }
            };
            var entity = (Entity)service.Execute(request).Results["Entity"];
            Assert.Equal("v2", entity["content"]);
        }

        // ── Delete ──────────────────────────────────────────────────────────

        [Fact]
        public void Delete_UnpublishedOnly_RemovesFromUnpublished()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("webresource") { ["name"] = "test.js" });
            service.Delete("webresource", id);

            // Should not be in unpublished store either
            var request = new OrganizationRequest("RetrieveUnpublished")
            {
                Parameters =
                {
                    ["Target"] = new EntityReference("webresource", id),
                    ["ColumnSet"] = new ColumnSet(true)
                }
            };
            Assert.Throws<FaultException<OrganizationServiceFault>>(() => service.Execute(request));
        }

        [Fact]
        public void Delete_PublishedAndUnpublished_RemovesFromBoth()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("webresource") { ["name"] = "test.js", ["content"] = "v1" });
            service.Execute(new OrganizationRequest("PublishAllXml"));

            // Update creates unpublished draft
            service.Update(new Entity("webresource", id) { ["content"] = "v2" });

            // Delete should remove from both
            service.Delete("webresource", id);

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Retrieve("webresource", id, new ColumnSet(true)));

            var request = new OrganizationRequest("RetrieveUnpublished")
            {
                Parameters =
                {
                    ["Target"] = new EntityReference("webresource", id),
                    ["ColumnSet"] = new ColumnSet(true)
                }
            };
            Assert.Throws<FaultException<OrganizationServiceFault>>(() => service.Execute(request));
        }

        [Fact]
        public void Delete_PublishedOnly_RemovesFromPublished()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("webresource") { ["name"] = "test.js" });
            service.Execute(new OrganizationRequest("PublishAllXml"));

            // Now only in published (unpublished was cleared by publish)
            service.Delete("webresource", id);

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Retrieve("webresource", id, new ColumnSet(true)));
        }

        [Fact]
        public void Delete_NonExistentSolutionAware_Throws()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Delete("webresource", Guid.NewGuid()));
        }

        // ── IsSolutionAwareEntity ───────────────────────────────────────────

        [Fact]
        public void IsSolutionAwareEntity_RegisteredEntity_ReturnsTrue()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            Assert.True(env.IsSolutionAwareEntity("webresource"));
        }

        [Fact]
        public void IsSolutionAwareEntity_UnregisteredEntity_ReturnsFalse()
        {
            var env = new FakeDataverseEnvironment();
            Assert.False(env.IsSolutionAwareEntity("account"));
        }

        // ── Snapshot / Scope ───────────────────────────────────────────────

        [Fact]
        public void Snapshot_PreservesUnpublishedState()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            var snapshot = env.TakeSnapshot();

            var id = service.Create(new Entity("webresource") { ["name"] = "test.js" });

            env.RestoreSnapshot(snapshot);

            // Unpublished record should be gone after restore
            var request = new OrganizationRequest("RetrieveUnpublishedMultiple")
            {
                Parameters =
                {
                    ["Query"] = new QueryExpression("webresource") { ColumnSet = new ColumnSet(true) }
                }
            };
            var response = service.Execute(request);
            var result = (EntityCollection)response.Results["EntityCollection"];
            Assert.Empty(result.Entities);
        }

        [Fact]
        public void Scope_RestoresUnpublishedState()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            Guid id;
            using (env.Scope())
            {
                id = service.Create(new Entity("webresource") { ["name"] = "test.js" });
            }

            // After scope, unpublished record should be gone
            var request = new OrganizationRequest("RetrieveUnpublishedMultiple")
            {
                Parameters =
                {
                    ["Query"] = new QueryExpression("webresource") { ColumnSet = new ColumnSet(true) }
                }
            };
            var response = service.Execute(request);
            var result = (EntityCollection)response.Results["EntityCollection"];
            Assert.Empty(result.Entities);
        }

        // ── Multi-user ─────────────────────────────────────────────────────

        [Fact]
        public void MultipleUsers_SeeSharedUnpublishedRecords()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var user1 = env.CreateOrganizationService(Guid.NewGuid());
            var user2 = env.CreateOrganizationService(Guid.NewGuid());

            var id = user1.Create(new Entity("webresource") { ["name"] = "shared.js" });

            var request = new OrganizationRequest("RetrieveUnpublished")
            {
                Parameters =
                {
                    ["Target"] = new EntityReference("webresource", id),
                    ["ColumnSet"] = new ColumnSet(true)
                }
            };
            var entity = (Entity)user2.Execute(request).Results["Entity"];
            Assert.Equal("shared.js", entity["name"]);
        }

        // ── Publish preserves other attributes ──────────────────────────────

        [Fact]
        public void Publish_PreservesAllAttributes()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("webresource")
            {
                ["name"] = "test.js",
                ["content"] = "alert('hello')",
                ["description"] = "A test resource"
            });
            service.Execute(new OrganizationRequest("PublishAllXml"));

            var entity = service.Retrieve("webresource", id, new ColumnSet(true));
            Assert.Equal("test.js", entity["name"]);
            Assert.Equal("alert('hello')", entity["content"]);
            Assert.Equal("A test resource", entity["description"]);
        }

        // ── Update merges with published on publish ─────────────────────────

        [Fact]
        public void Publish_MergesUpdateIntoPublished()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("webresource")
            {
                ["name"] = "test.js",
                ["content"] = "v1",
                ["description"] = "original"
            });
            service.Execute(new OrganizationRequest("PublishAllXml"));

            // Update only content, leaving description unchanged
            service.Update(new Entity("webresource", id) { ["content"] = "v2" });
            service.Execute(new OrganizationRequest("PublishAllXml"));

            var entity = service.Retrieve("webresource", id, new ColumnSet(true));
            Assert.Equal("v2", entity["content"]);
            Assert.Equal(0, entity.GetAttributeValue<OptionSetValue>("componentstate").Value);
        }

        // ── RetrieveUnpublishedMultiple with FetchXml ───────────────────────

        [Fact]
        public void RetrieveUnpublishedMultiple_WithFetchXml_Works()
        {
            var env = new FakeDataverseEnvironment();
            env.RegisterSolutionAwareEntity("webresource");
            var service = env.CreateOrganizationService();

            service.Create(new Entity("webresource") { ["name"] = "a.js" });
            service.Create(new Entity("webresource") { ["name"] = "b.css" });

            var fetchXml = @"<fetch><entity name='webresource'><attribute name='name' /></entity></fetch>";
            var request = new OrganizationRequest("RetrieveUnpublishedMultiple")
            {
                Parameters =
                {
                    ["Query"] = new FetchExpression(fetchXml)
                }
            };
            var response = service.Execute(request);
            var result = (EntityCollection)response.Results["EntityCollection"];
            Assert.Equal(2, result.Entities.Count);
        }
    }
}
