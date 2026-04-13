using System;
using System.Collections.Generic;
using Fake4Dataverse.Pipeline;
using Microsoft.Xrm.Sdk;
using InvalidPluginExecutionException = Microsoft.Xrm.Sdk.InvalidPluginExecutionException;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class PipelineTests
    {
        [Fact]
        public void PreValidation_FiresBeforeCreate()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var firedStages = new List<PipelineStage>();

            env.Pipeline.RegisterStep("Create", PipelineStage.PreValidation, ctx =>
            {
                firedStages.Add((PipelineStage)ctx.Stage);
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.Single(firedStages);
            Assert.Equal(PipelineStage.PreValidation, firedStages[0]);
        }

        [Fact]
        public void Pipeline_ExecutionOrder_PreValidation_PreOperation_PostOperation()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var order = new List<string>();

            env.Pipeline.RegisterStep("Create", PipelineStage.PreValidation, _ => order.Add("PreValidation"));
            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, _ => order.Add("PreOperation"));
            env.Pipeline.RegisterStep("Create", PipelineStage.PostOperation, _ => order.Add("PostOperation"));

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.Equal(new[] { "PreValidation", "PreOperation", "PostOperation" }, order);
        }

        [Fact]
        public void Pipeline_PreValidation_ThrowsAbortsPipeline()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            bool postFired = false;

            env.Pipeline.RegisterStep("Create", PipelineStage.PreValidation, _ =>
            {
                throw new InvalidPluginExecutionException("Blocked by plugin");
            });
            env.Pipeline.RegisterStep("Create", PipelineStage.PostOperation, _ =>
            {
                postFired = true;
            });

            var ex = Assert.Throws<InvalidPluginExecutionException>(() =>
                service.Create(new Entity("account") { ["name"] = "Test" }));
            Assert.Equal("Blocked by plugin", ex.Message);
            Assert.False(postFired);
        }

        [Fact]
        public void Pipeline_EntityScoped_OnlyFiresForMatchingEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var firedFor = new List<string>();

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, "account", ctx =>
            {
                firedFor.Add(ctx.PrimaryEntityName);
            });

            service.Create(new Entity("contact") { ["fullname"] = "John" });
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.Single(firedFor);
            Assert.Equal("account", firedFor[0]);
        }

        [Fact]
        public void Pipeline_Context_HasCorrectProperties()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            IPluginExecutionContext? capturedContext = null;

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                capturedContext = ctx;
            });

            service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.NotNull(capturedContext);
            Assert.Equal("Create", capturedContext!.MessageName);
            Assert.Equal("account", capturedContext.PrimaryEntityName);
            Assert.True(capturedContext.InputParameters.ContainsKey("Target"));
            Assert.IsType<Entity>(capturedContext.InputParameters["Target"]);
        }

        [Fact]
        public void Pipeline_Context_ExposesSDKInterfaceProperties()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var orgId = Guid.NewGuid();
            var buId = Guid.NewGuid();
            env.OrganizationId = orgId;
            service.BusinessUnitId = buId;
            env.OrganizationName = "TestOrg";
            FakePipelineContext? captured = null;
            int capturedStage = 0;

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                captured = ctx as FakePipelineContext;
                capturedStage = ctx.Stage; // capture stage value at callback time
            });

            service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.NotNull(captured);
            Assert.Equal(orgId, captured!.OrganizationId);
            Assert.Equal(buId, captured.BusinessUnitId);
            Assert.Equal("TestOrg", captured.OrganizationName);
            Assert.Equal((int)PipelineStage.PreOperation, capturedStage);
            Assert.Equal(PipelineStage.PreOperation, (PipelineStage)capturedStage);
        }

        [Fact]
        public void Pipeline_PostOperation_HasOutputParameters()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            IPluginExecutionContext? capturedContext = null;

            env.Pipeline.RegisterStep("Create", PipelineStage.PostOperation, ctx =>
            {
                capturedContext = ctx;
            });

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.NotNull(capturedContext);
            Assert.True(capturedContext!.OutputParameters.ContainsKey("id"));
            Assert.Equal(id, (Guid)capturedContext.OutputParameters["id"]);
        }

        [Fact]
        public void Pipeline_Update_FiresSteps()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var order = new List<string>();

            env.Pipeline.RegisterStep("Update", PipelineStage.PreOperation, _ => order.Add("PreOp"));
            env.Pipeline.RegisterStep("Update", PipelineStage.PostOperation, _ => order.Add("PostOp"));

            service.Update(new Entity("account", id) { ["name"] = "Fabrikam" });

            Assert.Equal(new[] { "PreOp", "PostOp" }, order);
        }

        [Fact]
        public void Pipeline_Delete_FiresSteps()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var order = new List<string>();

            env.Pipeline.RegisterStep("Delete", PipelineStage.PreValidation, _ => order.Add("PreVal"));
            env.Pipeline.RegisterStep("Delete", PipelineStage.PostOperation, _ => order.Add("PostOp"));

            service.Delete("account", id);

            Assert.Equal(new[] { "PreVal", "PostOp" }, order);
        }

        [Fact]
        public void Pipeline_PreOperation_ThrowsAborts_Update()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            env.Pipeline.RegisterStep("Update", PipelineStage.PreOperation, _ =>
            {
                throw new InvalidPluginExecutionException("Cannot update");
            });

            Assert.Throws<InvalidPluginExecutionException>(() =>
                service.Update(new Entity("account", id) { ["name"] = "Fabrikam" }));

            // Original value should be unchanged since core operation never ran
            var retrieved = service.Retrieve("account", id, new Microsoft.Xrm.Sdk.Query.ColumnSet("name"));
            Assert.Equal("Contoso", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Pipeline_StepRegistration_Dispose_Unregisters()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            int callCount = 0;

            var registration = env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, _ => callCount++);
            service.Create(new Entity("account") { ["name"] = "Test1" });
            Assert.Equal(1, callCount);

            registration.Dispose();
            service.Create(new Entity("account") { ["name"] = "Test2" });
            Assert.Equal(1, callCount); // Should not have fired again
        }

        [Fact]
        public void Pipeline_Context_UserId_MatchesCallerId()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var customCaller = Guid.NewGuid();
            service.CallerId = customCaller;
            IPluginExecutionContext? capturedContext = null;

            env.Pipeline.RegisterStep("Create", PipelineStage.PreValidation, ctx =>
            {
                capturedContext = ctx;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.NotNull(capturedContext);
            Assert.Equal(customCaller, capturedContext!.UserId);
        }

        // ── Convenience helpers ──────────────────────────────────────────────

        [Fact]
        public void Pipeline_RegisterPreOperation_Convenience_Fires()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            bool fired = false;

            env.Pipeline.RegisterPreOperation("Create", "account", _ => fired = true);
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.True(fired);
        }

        [Fact]
        public void Pipeline_RegisterPostOperation_Convenience_Fires()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            bool fired = false;

            env.Pipeline.RegisterPostOperation("Delete", _ => fired = true);
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Delete("account", id);

            Assert.True(fired);
        }

        [Fact]
        public void Pipeline_RegisterPreValidation_Convenience_Fires()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            bool fired = false;

            env.Pipeline.RegisterPreValidation("Create", _ => fired = true);
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.True(fired);
        }

        // ── IPlugin support ──────────────────────────────────────────────────

        [Fact]
        public void Pipeline_IPlugin_ExecuteIsCalled()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var plugin = new CapturingPlugin();

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, "account", plugin);
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.NotNull(plugin.CapturedContext);
            Assert.Equal("Create", plugin.CapturedContext!.MessageName);
            Assert.Equal("account", plugin.CapturedContext.PrimaryEntityName);
        }

        [Fact]
        public void Pipeline_IPlugin_ServiceProviderProvidesContext()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            IPluginExecutionContext? pluginContext = null;
            IOrganizationServiceFactory? pluginFactory = null;
            ITracingService? pluginTracing = null;

            var plugin = new LambdaPlugin(sp =>
            {
                pluginContext = (IPluginExecutionContext)sp.GetService(typeof(IPluginExecutionContext))!;
                pluginFactory = (IOrganizationServiceFactory)sp.GetService(typeof(IOrganizationServiceFactory))!;
                pluginTracing = (ITracingService)sp.GetService(typeof(ITracingService))!;
            });

            env.Pipeline.RegisterStep("Create", PipelineStage.PostOperation, plugin);
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.NotNull(pluginContext);
            Assert.NotNull(pluginFactory);
            Assert.NotNull(pluginTracing);
        }

        [Fact]
        public void Pipeline_IPlugin_CanCreateRecordsViaFactory()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var plugin = new LambdaPlugin(sp =>
            {
                var ctx = (IPluginExecutionContext)sp.GetService(typeof(IPluginExecutionContext))!;
                var factory = (IOrganizationServiceFactory)sp.GetService(typeof(IOrganizationServiceFactory))!;
                var orgService = factory.CreateOrganizationService(ctx.UserId);

                var target = (Entity)ctx.InputParameters["Target"];
                orgService.Create(new Entity("contact")
                {
                    ["parentcustomerid"] = new EntityReference("account", target.Id),
                    ["lastname"] = "Plugin Contact"
                });
            });

            env.Pipeline.RegisterStep("Create", PipelineStage.PostOperation, "account", plugin);
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            var contacts = service.RetrieveMultiple(new Microsoft.Xrm.Sdk.Query.QueryExpression("contact")
            {
                ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("lastname")
            });
            Assert.Single(contacts.Entities);
            Assert.Equal("Plugin Contact", contacts.Entities[0]["lastname"]);
        }

        [Fact]
        public void Pipeline_IPlugin_TracingServiceCapturesMessages()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var plugin = new LambdaPlugin(sp =>
            {
                var tracing = (ITracingService)sp.GetService(typeof(ITracingService))!;
                tracing.Trace("Plugin executed for {0}", "account");
                tracing.Trace("Done");
            });

            env.Pipeline.RegisterStep("Create", PipelineStage.PostOperation, plugin);
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.Equal(2, env.Pipeline.Traces.Count);
            Assert.Equal("Plugin executed for account", env.Pipeline.Traces[0]);
            Assert.Equal("Done", env.Pipeline.Traces[1]);
        }

        [Fact]
        public void Pipeline_ClearTraces_ResetsCollection()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            env.Pipeline.RegisterPostOperation("Create", ctx =>
            {
                // no-op — traces come from IPlugin steps; ClearTraces just needs to empty the list
            });
            var plugin = new LambdaPlugin(sp =>
            {
                ((ITracingService)sp.GetService(typeof(ITracingService))!).Trace("hello");
            });
            env.Pipeline.RegisterStep("Create", PipelineStage.PostOperation, plugin);

            service.Create(new Entity("account") { ["name"] = "Contoso" });
            Assert.NotEmpty(env.Pipeline.Traces);

            env.Pipeline.ClearTraces();
            Assert.Empty(env.Pipeline.Traces);
        }

        [Fact]
        public void Pipeline_IPlugin_ThrowsAbortsPipeline()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            bool postFired = false;

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation,
                new LambdaPlugin(_ => throw new InvalidPluginExecutionException("plugin abort")));
            env.Pipeline.RegisterStep("Create", PipelineStage.PostOperation, _ => postFired = true);

            Assert.Throws<InvalidPluginExecutionException>(() =>
                service.Create(new Entity("account") { ["name"] = "Test" }));
            Assert.False(postFired);
        }

        // ── Helper plugin implementations ────────────────────────────────────

        private sealed class CapturingPlugin : IPlugin
        {
            public IPluginExecutionContext? CapturedContext { get; private set; }

            public void Execute(IServiceProvider serviceProvider)
            {
                CapturedContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext))!;
            }
        }

        private sealed class LambdaPlugin : IPlugin
        {
            private readonly Action<IServiceProvider> _action;
            public LambdaPlugin(Action<IServiceProvider> action) => _action = action;
            public void Execute(IServiceProvider serviceProvider) => _action(serviceProvider);
        }

        [Fact]
        public void Pipeline_SharedVariables_PropagateAcrossStages()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            string? captured = null;

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                ctx.SharedVariables["key"] = "FromPreOp";
            });

            env.Pipeline.RegisterStep("Create", PipelineStage.PostOperation, ctx =>
            {
                captured = ctx.SharedVariables.ContainsKey("key")
                    ? (string)ctx.SharedVariables["key"]
                    : null;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.Equal("FromPreOp", captured);
        }

        [Fact]
        public void Pipeline_Context_Depth_IsOne()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            int depth = 0;

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                depth = ctx.Depth;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.Equal(1, depth);
        }

        [Fact]
        public void Pipeline_Context_Mode_IsSynchronous()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            int mode = -1;

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                mode = ctx.Mode;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.Equal(0, mode); // 0 = Synchronous
        }

        [Fact]
        public void Pipeline_Context_IsInTransaction_IsTrue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            bool inTransaction = false;

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                inTransaction = ctx.IsInTransaction;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.True(inTransaction);
        }

        [Fact]
        public void Pipeline_PreOperation_CanModifyTarget()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                var target = (Entity)ctx.InputParameters["Target"];
                target["name"] = "ModifiedByPlugin";
            });

            var id = service.Create(new Entity("account") { ["name"] = "Original" });

            var retrieved = service.Retrieve("account", id, new Microsoft.Xrm.Sdk.Query.ColumnSet("name"));
            Assert.Equal("ModifiedByPlugin", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Pipeline_MultipleStepsInSameStage_AllFire()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var firedSteps = new List<string>();

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, _ => firedSteps.Add("Step1"));
            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, _ => firedSteps.Add("Step2"));

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.Equal(2, firedSteps.Count);
            Assert.Contains("Step1", firedSteps);
            Assert.Contains("Step2", firedSteps);
        }

        // ── Plugin Images ────────────────────────────────────────────────────

        [Fact]
        public void Pipeline_PreImage_AvailableInPreOperation()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Original" });

            Entity? preImage = null;
            var step = env.Pipeline.RegisterPreOperation("Update", "account", ctx =>
            {
                if (ctx.PreEntityImages.Contains("preimage"))
                    preImage = ctx.PreEntityImages["preimage"];
            });
            step.AddPreImage("preimage", "name");

            service.Update(new Entity("account", id) { ["name"] = "Updated" });

            Assert.NotNull(preImage);
            Assert.Equal("Original", preImage!.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Pipeline_PostImage_AvailableInPostOperation()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Original" });

            Entity? postImage = null;
            var step = env.Pipeline.RegisterPostOperation("Update", "account", ctx =>
            {
                if (ctx.PostEntityImages.Contains("postimage"))
                    postImage = ctx.PostEntityImages["postimage"];
            });
            step.AddPostImage("postimage", "name");

            service.Update(new Entity("account", id) { ["name"] = "Updated" });

            Assert.NotNull(postImage);
            Assert.Equal("Updated", postImage!.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Pipeline_DeleteTarget_AccessibleInPostOperation()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "ToDelete" });

            EntityReference? deletedRef = null;
            env.Pipeline.RegisterPostOperation("Delete", "account", ctx =>
            {
                deletedRef = ctx.InputParameters["Target"] as EntityReference;
            });

            service.Delete("account", id);

            Assert.NotNull(deletedRef);
            Assert.Equal(id, deletedRef!.Id);
            Assert.Equal("account", deletedRef.LogicalName);
        }

        // ── Sync / Async Mode ────────────────────────────────────────────────

        [Fact]
        public void Pipeline_AsyncStep_HasCorrectMode()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            int? capturedMode = null;
            var step = env.Pipeline.RegisterPostOperation("Create", "account", ctx =>
            {
                capturedMode = ctx.Mode;
            });
            step.SetAsynchronous();

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.Equal(1, capturedMode);
        }

        [Fact]
        public void Pipeline_SyncStep_HasModeZero()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            int? capturedMode = null;
            env.Pipeline.RegisterPostOperation("Create", "account", ctx =>
            {
                capturedMode = ctx.Mode;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.Equal(0, capturedMode);
        }

        // ── IPluginExecutionContext v2–v7 properties ──────────────────────────

        [Fact]
        public void Pipeline_Context_V2_DefaultsToEmptyGuidsAndFalse()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            IPluginExecutionContext2? ctx2 = null;

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                ctx2 = ctx as IPluginExecutionContext2;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.NotNull(ctx2);
            Assert.Equal(Guid.Empty, ctx2!.InitiatingUserApplicationId);
            Assert.Equal(Guid.Empty, ctx2.InitiatingUserAzureActiveDirectoryObjectId);
            Assert.False(ctx2.IsPortalsClientCall);
            Assert.Equal(Guid.Empty, ctx2.PortalsContactId);
            Assert.Equal(Guid.Empty, ctx2.UserAzureActiveDirectoryObjectId);
        }

        [Fact]
        public void Pipeline_Context_V2_ServicePropertiesFlowIntoContext()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var appId = Guid.NewGuid();
            var aadObjectId = Guid.NewGuid();
            var contactId = Guid.NewGuid();
            var userAadId = Guid.NewGuid();
            service.InitiatingUserApplicationId = appId;
            service.InitiatingUserAzureActiveDirectoryObjectId = aadObjectId;
            service.IsPortalsClientCall = true;
            service.PortalsContactId = contactId;
            service.UserAzureActiveDirectoryObjectId = userAadId;

            IPluginExecutionContext2? ctx2 = null;
            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                ctx2 = ctx as IPluginExecutionContext2;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.NotNull(ctx2);
            Assert.Equal(appId, ctx2!.InitiatingUserApplicationId);
            Assert.Equal(aadObjectId, ctx2.InitiatingUserAzureActiveDirectoryObjectId);
            Assert.True(ctx2.IsPortalsClientCall);
            Assert.Equal(contactId, ctx2.PortalsContactId);
            Assert.Equal(userAadId, ctx2.UserAzureActiveDirectoryObjectId);
        }

        [Fact]
        public void Pipeline_Context_V3_AuthenticatedUserIdDefaultsToCallerId()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var customCaller = Guid.NewGuid();
            service.CallerId = customCaller;

            IPluginExecutionContext3? ctx3 = null;
            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                ctx3 = ctx as IPluginExecutionContext3;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.NotNull(ctx3);
            Assert.Equal(customCaller, ctx3!.AuthenticatedUserId);
        }

        [Fact]
        public void Pipeline_Context_V4_ImagesCollectionsWrapSingleCollections()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Original" });

            EntityImageCollection[]? preCollections = null;
            EntityImageCollection[]? postCollections = null;

            var step = env.Pipeline.RegisterPostOperation("Update", "account", ctx =>
            {
                var ctx4 = ctx as IPluginExecutionContext4;
                preCollections = ctx4?.PreEntityImagesCollection;
                postCollections = ctx4?.PostEntityImagesCollection;
            });
            step.AddPreImage("preimage", "name");
            step.AddPostImage("postimage", "name");

            service.Update(new Entity("account", id) { ["name"] = "Updated" });

            Assert.NotNull(preCollections);
            Assert.NotNull(postCollections);
            Assert.Single(preCollections!);
            Assert.Single(postCollections!);
            Assert.True(preCollections[0].Contains("preimage"));
            Assert.True(postCollections[0].Contains("postimage"));
        }

        [Fact]
        public void Pipeline_Context_V5_InitiatingUserAgentDefault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            string? userAgent = null;

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                userAgent = (ctx as IPluginExecutionContext5)?.InitiatingUserAgent;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.Equal("Fake4Dataverse", userAgent);
        }

        [Fact]
        public void Pipeline_Context_V5_InitiatingUserAgentIsConfigurable()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.InitiatingUserAgent = "MyClient/2.0";
            string? userAgent = null;

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                userAgent = (ctx as IPluginExecutionContext5)?.InitiatingUserAgent;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.Equal("MyClient/2.0", userAgent);
        }

        [Fact]
        public void Pipeline_Context_V6_EnvironmentIdAndTenantIdAreConfigurable()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.EnvironmentId = "unq12345abcde";
            env.TenantId = new Guid("aaaabbbb-0000-0000-0000-ccccdddd0001");

            string? envId = null;
            Guid tenantId = Guid.Empty;

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                var ctx6 = ctx as IPluginExecutionContext6;
                envId = ctx6?.EnvironmentId;
                tenantId = ctx6?.TenantId ?? Guid.Empty;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.Equal("unq12345abcde", envId);
            Assert.Equal(new Guid("aaaabbbb-0000-0000-0000-ccccdddd0001"), tenantId);
        }

        [Fact]
        public void Pipeline_Context_V6_EnvironmentIdDefaultsToEmpty()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            string? envId = "not-set";

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                envId = (ctx as IPluginExecutionContext6)?.EnvironmentId;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.Equal(string.Empty, envId);
        }

        [Fact]
        public void Pipeline_Context_V7_IsApplicationUserDefaultsFalse()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            bool? isAppUser = null;

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                isAppUser = (ctx as IPluginExecutionContext7)?.IsApplicationUser;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.False(isAppUser);
        }

        [Fact]
        public void Pipeline_Context_V7_IsApplicationUserIsConfigurable()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.IsApplicationUser = true;
            bool? isAppUser = null;

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                isAppUser = (ctx as IPluginExecutionContext7)?.IsApplicationUser;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.True(isAppUser);
        }

        [Fact]
        public void Pipeline_Context_ImplmentsIPluginExecutionContext7()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            bool isCtx7 = false;

            env.Pipeline.RegisterStep("Create", PipelineStage.PreOperation, ctx =>
            {
                isCtx7 = ctx is IPluginExecutionContext7;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.True(isCtx7);
        }
    }
}

