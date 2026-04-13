using System;
using Fake4Dataverse.Spkl;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class SpklPluginRegistrationExtensionsTests
    {
        [Fact]
        public void RegisterSpklPluginsFromAssembly_WithAttributedPlugins_RegistersAndExecutes()
        {
            AssemblyDiscoveryPlugin.Reset();
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            using var result = env.RegisterSpklPluginsFromAssembly(typeof(SpklPluginRegistrationExtensionsTests).Assembly);

            var id = service.Create(new Entity("spkl_assemblyaccount") { ["name"] = "Original" });
            var created = service.Retrieve("spkl_assemblyaccount", id, new ColumnSet("name"));

            Assert.True(AssemblyDiscoveryPlugin.ExecuteCount > 0);
            Assert.Equal("FromAssembly", created.GetAttributeValue<string>("name"));
            Assert.NotEmpty(result.Registrations);
        }

        [Fact]
        public void RegisterSpklPlugins_MapsStageAndModeFromAttribute()
        {
            StageModePlugin.Reset();
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            using var result = env.RegisterSpklPlugins(typeof(StageModePlugin));

            service.Create(new Entity("spkl_stageaccount") { ["name"] = "Contoso" });

            Assert.Single(result.Registrations);
            Assert.Equal(10, StageModePlugin.CapturedStage);
            Assert.Equal(1, StageModePlugin.CapturedMode);
        }

        [Fact]
        public void RegisterSpklPlugins_MapsImages_PrePostAndBoth()
        {
            ImageCapturePlugin.Reset();
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("spkl_imageaccount")
            {
                ["name"] = "Before",
                ["accountnumber"] = "A-001",
                ["description"] = "Desc"
            });

            using var result = env.RegisterSpklPlugins(typeof(ImageCapturePlugin));

            service.Update(new Entity("spkl_imageaccount", id)
            {
                ["name"] = "After"
            });

            Assert.Single(result.Registrations);
            Assert.True(ImageCapturePlugin.PreContainsImage1);
            Assert.True(ImageCapturePlugin.PreContainsImage2);
            Assert.True(ImageCapturePlugin.PostContainsImage2);
            Assert.False(ImageCapturePlugin.PostContainsImage1);
            Assert.Equal("Before", ImageCapturePlugin.PreImage1Name);
            Assert.Equal("A-001", ImageCapturePlugin.PreImage2AccountNumber);
            Assert.Equal("After", ImageCapturePlugin.PostImage2Name);
        }

        [Fact]
        public void RegisterSpklPlugins_NormalizesFilteringAttributes_AndExecutesOnlyForMatches()
        {
            FilteringPlugin.Reset();
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("spkl_filteraccount")
            {
                ["name"] = "Contoso",
                ["accountnumber"] = "N-1"
            });

            using var result = env.RegisterSpklPlugins(typeof(FilteringPlugin));

            // Does not match filtering attributes (name/accountnumber)
            service.Update(new Entity("spkl_filteraccount", id)
            {
                ["description"] = "no-op"
            });

            // Matches filtering attributes
            service.Update(new Entity("spkl_filteraccount", id)
            {
                ["name"] = "Fabrikam"
            });

            Assert.Single(result.Registrations);
            Assert.Equal(1, FilteringPlugin.ExecuteCount);
        }

        [Fact]
        public void RegisterSpklPlugins_UnsupportedForms_AreSkippedPredictably()
        {
            UnsupportedFormsPlugin.Reset();
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            using var result = env.RegisterSpklPlugins(typeof(UnsupportedFormsPlugin));

            service.Create(new Entity("spkl_unsupported") { ["name"] = "Ignored" });

            Assert.Empty(result.Registrations);
            Assert.Equal(2, result.SkippedRegistrations.Count);
            Assert.Contains(result.SkippedRegistrations, s => s.Reason.IndexOf("Custom API", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.Contains(result.SkippedRegistrations, s => s.Reason.IndexOf("Workflow", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.Equal(0, UnsupportedFormsPlugin.ExecuteCount);
        }

        [CrmPluginRegistration(
            "Create",
            "spkl_assemblyaccount",
            StageEnum.PreOperation,
            ExecutionModeEnum.Synchronous,
            "",
            "AssemblyDiscovery",
            1,
            IsolationModeEnum.Sandbox)]
        private sealed class AssemblyDiscoveryPlugin : IPlugin
        {
            public static int ExecuteCount { get; private set; }

            public static void Reset()
            {
                ExecuteCount = 0;
            }

            public void Execute(IServiceProvider serviceProvider)
            {
                ExecuteCount++;
                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext))!;
                var target = (Entity)context.InputParameters["Target"];
                target["name"] = "FromAssembly";
            }
        }

        [CrmPluginRegistration(
            "Create",
            "spkl_stageaccount",
            StageEnum.PreValidation,
            ExecutionModeEnum.Asynchronous,
            "",
            "StageMode",
            1,
            IsolationModeEnum.Sandbox)]
        private sealed class StageModePlugin : IPlugin
        {
            public static int CapturedStage { get; private set; }
            public static int CapturedMode { get; private set; }

            public static void Reset()
            {
                CapturedStage = -1;
                CapturedMode = -1;
            }

            public void Execute(IServiceProvider serviceProvider)
            {
                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext))!;
                CapturedStage = context.Stage;
                CapturedMode = context.Mode;
            }
        }

        [CrmPluginRegistration(
            "Update",
            "spkl_imageaccount",
            StageEnum.PostOperation,
            ExecutionModeEnum.Synchronous,
            " name, accountnumber ",
            "ImageCapture",
            1,
            IsolationModeEnum.Sandbox,
            Image1Name = "image1",
            Image1Type = ImageTypeEnum.PreImage,
            Image1Attributes = "name",
            Image2Name = "image2",
            Image2Type = ImageTypeEnum.Both,
            Image2Attributes = " name , accountnumber ")]
        private sealed class ImageCapturePlugin : IPlugin
        {
            public static bool PreContainsImage1 { get; private set; }
            public static bool PreContainsImage2 { get; private set; }
            public static bool PostContainsImage1 { get; private set; }
            public static bool PostContainsImage2 { get; private set; }
            public static string? PreImage1Name { get; private set; }
            public static string? PreImage2AccountNumber { get; private set; }
            public static string? PostImage2Name { get; private set; }

            public static void Reset()
            {
                PreContainsImage1 = false;
                PreContainsImage2 = false;
                PostContainsImage1 = false;
                PostContainsImage2 = false;
                PreImage1Name = null;
                PreImage2AccountNumber = null;
                PostImage2Name = null;
            }

            public void Execute(IServiceProvider serviceProvider)
            {
                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext))!;

                PreContainsImage1 = context.PreEntityImages.Contains("image1");
                PreContainsImage2 = context.PreEntityImages.Contains("image2");
                PostContainsImage1 = context.PostEntityImages.Contains("image1");
                PostContainsImage2 = context.PostEntityImages.Contains("image2");

                if (PreContainsImage1)
                {
                    PreImage1Name = context.PreEntityImages["image1"].GetAttributeValue<string>("name");
                }

                if (PreContainsImage2)
                {
                    PreImage2AccountNumber = context.PreEntityImages["image2"].GetAttributeValue<string>("accountnumber");
                }

                if (PostContainsImage2)
                {
                    PostImage2Name = context.PostEntityImages["image2"].GetAttributeValue<string>("name");
                }
            }
        }

        [CrmPluginRegistration(
            "Update",
            "spkl_filteraccount",
            StageEnum.PostOperation,
            ExecutionModeEnum.Synchronous,
            " name , accountnumber ",
            "Filtering",
            1,
            IsolationModeEnum.Sandbox)]
        private sealed class FilteringPlugin : IPlugin
        {
            public static int ExecuteCount { get; private set; }

            public static void Reset()
            {
                ExecuteCount = 0;
            }

            public void Execute(IServiceProvider serviceProvider)
            {
                ExecuteCount++;
            }
        }

        [CrmPluginRegistration("spkl_CustomApi")]
        [CrmPluginRegistration("WorkflowActivity", "Friendly", "Description", "Group", IsolationModeEnum.Sandbox)]
        private sealed class UnsupportedFormsPlugin : IPlugin
        {
            public static int ExecuteCount { get; private set; }

            public static void Reset()
            {
                ExecuteCount = 0;
            }

            public void Execute(IServiceProvider serviceProvider)
            {
                ExecuteCount++;
            }
        }
    }
}