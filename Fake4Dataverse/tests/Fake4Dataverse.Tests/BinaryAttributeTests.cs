using System;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class BinaryAttributeTests
    {
        [Fact]
        public void SetAndGetBinaryAttribute()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var imageData = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            env.SetBinaryAttribute("account", id, "entityimage", imageData);

            var result = env.GetBinaryAttribute("account", id, "entityimage");

            Assert.NotNull(result);
            Assert.Equal(imageData, result);
        }

        [Fact]
        public void GetBinaryAttribute_NotSet_ReturnsNull()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var result = env.GetBinaryAttribute("account", id, "entityimage");

            Assert.Null(result);
        }

        [Fact]
        public void SetBinaryAttribute_OverwritesExisting()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var data1 = new byte[] { 1, 2, 3 };
            var data2 = new byte[] { 4, 5, 6, 7 };

            env.SetBinaryAttribute("account", id, "entityimage", data1);
            env.SetBinaryAttribute("account", id, "entityimage", data2);

            var result = env.GetBinaryAttribute("account", id, "entityimage");

            Assert.Equal(data2, result);
        }

        [Fact]
        public void SetBinaryAttribute_DoesNotShareReference()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var original = new byte[] { 1, 2, 3 };
            env.SetBinaryAttribute("account", id, "entityimage", original);

            // Modify the original array
            original[0] = 99;

            var result = env.GetBinaryAttribute("account", id, "entityimage");
            Assert.Equal(1, result![0]); // Should not be affected by modification
        }

        [Fact]
        public void GetBinaryAttribute_ReturnsCopy()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            env.SetBinaryAttribute("account", id, "entityimage", new byte[] { 1, 2, 3 });

            var result1 = env.GetBinaryAttribute("account", id, "entityimage");
            result1![0] = 99;

            var result2 = env.GetBinaryAttribute("account", id, "entityimage");
            Assert.Equal(1, result2![0]); // Should not be affected
        }

        [Fact]
        public void SetBinaryAttribute_MultipleAttributes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var imageData = new byte[] { 0x89, 0x50 };
            var fileData = new byte[] { 0x25, 0x50, 0x44, 0x46 };

            env.SetBinaryAttribute("account", id, "entityimage", imageData);
            env.SetBinaryAttribute("account", id, "document", fileData);

            Assert.Equal(imageData, env.GetBinaryAttribute("account", id, "entityimage"));
            Assert.Equal(fileData, env.GetBinaryAttribute("account", id, "document"));
        }

        [Fact]
        public void SetBinaryAttribute_MultipleEntities()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id1 = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var id2 = service.Create(new Entity("account") { ["name"] = "Fabrikam" });

            env.SetBinaryAttribute("account", id1, "entityimage", new byte[] { 1 });
            env.SetBinaryAttribute("account", id2, "entityimage", new byte[] { 2 });

            Assert.Equal(new byte[] { 1 }, env.GetBinaryAttribute("account", id1, "entityimage"));
            Assert.Equal(new byte[] { 2 }, env.GetBinaryAttribute("account", id2, "entityimage"));
        }

        [Fact]
        public void Reset_ClearsBinaryStore()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            env.SetBinaryAttribute("account", id, "entityimage", new byte[] { 1, 2, 3 });
            env.Reset();

            Assert.Null(env.GetBinaryAttribute("account", id, "entityimage"));
        }

        [Fact]
        public void SetBinaryAttribute_LargeData()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var largeData = new byte[1024 * 1024]; // 1 MB
            new Random(42).NextBytes(largeData);

            env.SetBinaryAttribute("account", id, "entityimage", largeData);

            var result = env.GetBinaryAttribute("account", id, "entityimage");
            Assert.Equal(largeData, result);
        }

        [Fact]
        public void SetBinaryAttribute_NullData_Throws()
        {
            var env = new FakeDataverseEnvironment();
            Assert.Throws<ArgumentNullException>(() =>
                env.SetBinaryAttribute("account", Guid.NewGuid(), "entityimage", null!));
        }

        [Fact]
        public void SetBinaryAttribute_EmptyEntityName_Throws()
        {
            var env = new FakeDataverseEnvironment();
            Assert.Throws<ArgumentException>(() =>
                env.SetBinaryAttribute("", Guid.NewGuid(), "entityimage", new byte[] { 1 }));
        }

        [Fact]
        public void SetBinaryAttribute_EmptyId_Throws()
        {
            var env = new FakeDataverseEnvironment();
            Assert.Throws<ArgumentException>(() =>
                env.SetBinaryAttribute("account", Guid.Empty, "entityimage", new byte[] { 1 }));
        }
    }
}
