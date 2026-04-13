using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class OptionsTests
    {
        [Fact]
        public void DefaultOptions_MatchCurrentBehavior()
        {
            var options = new FakeOrganizationServiceOptions();

            Assert.True(options.AutoSetTimestamps);
            Assert.True(options.AutoSetOwner);
            Assert.True(options.AutoSetVersionNumber);
            Assert.True(options.AutoSetStateCode);
            Assert.False(options.ValidateWithMetadata);
            Assert.False(options.EnforceSecurityRoles);
            Assert.True(options.EnablePipeline);
            Assert.True(options.EnableOperationLog);
        }

        [Fact]
        public void StrictPreset_EnablesValidationAndSecurity()
        {
            var options = FakeOrganizationServiceOptions.Strict;

            Assert.True(options.ValidateWithMetadata);
            Assert.True(options.EnforceSecurityRoles);
            Assert.True(options.AutoSetTimestamps);
            Assert.True(options.AutoSetOwner);
            Assert.True(options.AutoSetVersionNumber);
            Assert.True(options.AutoSetStateCode);
            Assert.True(options.EnablePipeline);
            Assert.True(options.EnableOperationLog);
        }

        [Fact]
        public void LenientPreset_DisablesAllAutoBehaviors()
        {
            var options = FakeOrganizationServiceOptions.Lenient;

            Assert.False(options.AutoSetTimestamps);
            Assert.False(options.AutoSetOwner);
            Assert.False(options.AutoSetVersionNumber);
            Assert.False(options.AutoSetStateCode);
            Assert.False(options.ValidateWithMetadata);
            Assert.False(options.EnforceSecurityRoles);
            Assert.False(options.EnablePipeline);
            Assert.False(options.EnableOperationLog);
        }

        [Fact]
        public void OptionsConstructor_SetsValidateWithMetadata()
        {
            var options = FakeOrganizationServiceOptions.Strict;
            var env = new FakeDataverseEnvironment(options);

            Assert.True(env.Options.ValidateWithMetadata);
            Assert.Same(options, env.Options);
        }

        [Fact]
        public void OptionsConstructor_SetsEnforceSecurityRoles()
        {
            var options = new FakeOrganizationServiceOptions { EnforceSecurityRoles = true };
            var env = new FakeDataverseEnvironment(options);

            Assert.True(env.Security.EnforceSecurityRoles);
        }

        [Fact]
        public void DefaultConstructor_UsesDefaultOptions()
        {
            var env = new FakeDataverseEnvironment();

            Assert.NotNull(env.Options);
            Assert.True(env.Options.AutoSetTimestamps);
            Assert.True(env.Options.EnableOperationLog);
        }

        [Fact]
        public void Lenient_Create_DoesNotAutoSetTimestamps()
        {
            var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Lenient);
            var service = env.CreateOrganizationService();
            var entity = new Entity("account") { ["name"] = "Test" };

            var id = service.Create(entity);
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));

            Assert.False(retrieved.Contains("createdon"));
            Assert.False(retrieved.Contains("modifiedon"));
        }

        [Fact]
        public void Lenient_Create_DoesNotAutoSetOwner()
        {
            var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Lenient);
            var service = env.CreateOrganizationService();
            var entity = new Entity("account") { ["name"] = "Test" };

            var id = service.Create(entity);
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));

            Assert.False(retrieved.Contains("ownerid"));
            Assert.False(retrieved.Contains("createdby"));
            Assert.False(retrieved.Contains("modifiedby"));
        }

        [Fact]
        public void Lenient_Create_DoesNotAutoSetStateCode()
        {
            var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Lenient);
            var service = env.CreateOrganizationService();
            var entity = new Entity("account") { ["name"] = "Test" };

            var id = service.Create(entity);
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));

            Assert.False(retrieved.Contains("statecode"));
            Assert.False(retrieved.Contains("statuscode"));
        }

        [Fact]
        public void Lenient_Create_DoesNotAutoSetVersionNumber()
        {
            var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Lenient);
            var service = env.CreateOrganizationService();
            var entity = new Entity("account") { ["name"] = "Test" };

            var id = service.Create(entity);
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));

            Assert.False(retrieved.Contains("versionnumber"));
        }

        [Fact]
        public void Lenient_Update_DoesNotAutoSetFields()
        {
            var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Lenient);
            var service = env.CreateOrganizationService();
            var entity = new Entity("account") { ["name"] = "Test" };
            var id = service.Create(entity);

            service.Update(new Entity("account") { Id = id, ["name"] = "Updated" });
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));

            Assert.Equal("Updated", retrieved["name"]);
            Assert.False(retrieved.Contains("modifiedon"));
            Assert.False(retrieved.Contains("modifiedby"));
            Assert.False(retrieved.Contains("versionnumber"));
        }

        [Fact]
        public void Lenient_DisablesOperationLog()
        {
            var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Lenient);
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.Empty(service.OperationLog.Records);
        }

        [Fact]
        public void EnableOperationLog_True_RecordsOperations()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.Single(service.OperationLog.Records);
        }

        [Fact]
        public void Default_Create_StillAutoSetsAllFields()
        {
            // Ensure default behavior is unchanged
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var entity = new Entity("account") { ["name"] = "Test" };
            var id = service.Create(entity);

            var retrieved = service.Retrieve("account", id, new ColumnSet(true));

            Assert.True(retrieved.Contains("createdon"));
            Assert.True(retrieved.Contains("modifiedon"));
            Assert.True(retrieved.Contains("ownerid"));
            Assert.True(retrieved.Contains("createdby"));
            Assert.True(retrieved.Contains("modifiedby"));
            Assert.True(retrieved.Contains("statecode"));
            Assert.True(retrieved.Contains("statuscode"));
            Assert.True(retrieved.Contains("versionnumber"));
        }

        [Fact]
        public void IndividualOption_AutoSetTimestamps_CanBeToggledAlone()
        {
            var options = new FakeOrganizationServiceOptions { AutoSetTimestamps = false };
            var env = new FakeDataverseEnvironment(options);
            var service = env.CreateOrganizationService();
            var entity = new Entity("account") { ["name"] = "Test" };
            var id = service.Create(entity);

            var retrieved = service.Retrieve("account", id, new ColumnSet(true));

            // Timestamps disabled, but owner/state/version still set
            Assert.False(retrieved.Contains("createdon"));
            Assert.True(retrieved.Contains("ownerid"));
            Assert.True(retrieved.Contains("statecode"));
            Assert.True(retrieved.Contains("versionnumber"));
        }

        [Fact]
        public void Lenient_Pipeline_NotTriggered()
        {
            var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Lenient);
            var service = env.CreateOrganizationService();
            bool pipelineFired = false;
            env.Pipeline.RegisterStep("Create", Pipeline.PipelineStage.PreOperation, ctx =>
            {
                pipelineFired = true;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.False(pipelineFired);
        }

        [Fact]
        public void Default_Pipeline_StillTriggered()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            bool pipelineFired = false;
            env.Pipeline.RegisterStep("Create", Pipeline.PipelineStage.PreOperation, ctx =>
            {
                pipelineFired = true;
            });

            service.Create(new Entity("account") { ["name"] = "Test" });

            Assert.True(pipelineFired);
        }

        [Fact]
        public void Strict_EnablesValidation()
        {
            var env = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Strict);

            Assert.True(env.Options.ValidateWithMetadata);
            Assert.True(env.Security.EnforceSecurityRoles);
        }
    }
}
