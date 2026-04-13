using System;
using System.IO;
using Fake4Dataverse.DataProviders;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class DataProviderExtensionsTests
    {
        [Fact]
        public void SeedFromJsonFile_LoadsEntitiesFromDisk()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var tempPath = Path.GetTempFileName();

            try
            {
                File.WriteAllText(tempPath,
                    "[{\"logicalname\":\"account\",\"name\":\"Contoso\",\"numberofemployees\":42}]");

                env.SeedFromJsonFile(tempPath);

                var accounts = service.RetrieveMultiple(new QueryExpression("account")
                {
                    ColumnSet = new ColumnSet(true)
                });

                Assert.Single(accounts.Entities);
                Assert.Equal("Contoso", accounts.Entities[0].GetAttributeValue<string>("name"));
                Assert.Equal(42, accounts.Entities[0].GetAttributeValue<int>("numberofemployees"));
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        [Fact]
        public void SeedFromCsvFile_LoadsEntitiesFromDisk()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var tempPath = Path.GetTempFileName();

            try
            {
                File.WriteAllText(tempPath,
                    "logicalname,name,revenue" + Environment.NewLine +
                    "account,Contoso,1000" + Environment.NewLine +
                    "account,Fabrikam,2000");

                env.SeedFromCsvFile(tempPath);

                var accounts = service.RetrieveMultiple(new QueryExpression("account")
                {
                    ColumnSet = new ColumnSet(true)
                });

                Assert.Equal(2, accounts.Entities.Count);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
