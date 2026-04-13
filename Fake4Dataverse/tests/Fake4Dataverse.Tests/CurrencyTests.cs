using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class CurrencyTests
    {
        private static readonly Guid UsdCurrencyId = new Guid("00000000-0000-0000-0000-000000000USD".Replace("USD", "001"));
        private static readonly Guid EurCurrencyId = new Guid("00000000-0000-0000-0000-000000000002");

        [Fact]
        public void Create_WithCurrency_ComputesBaseField()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Currency.BaseCurrencyId = UsdCurrencyId;
            env.Currency.SetExchangeRate(UsdCurrencyId, 1.0m);
            env.Currency.SetExchangeRate(EurCurrencyId, 0.85m);

            var id = service.Create(new Entity("opportunity")
            {
                ["transactioncurrencyid"] = new EntityReference("transactioncurrency", EurCurrencyId),
                ["estimatedvalue"] = new Money(1000m)
            });

            var result = service.Retrieve("opportunity", id, new ColumnSet(true));

            var baseValue = result.GetAttributeValue<Money>("estimatedvalue_base");
            Assert.NotNull(baseValue);
            // 1000 / 0.85 ≈ 1176.47
            Assert.Equal(Math.Round(1000m / 0.85m, 4), Math.Round(baseValue.Value, 4));
        }

        [Fact]
        public void Create_BaseCurrency_BaseEqualsOriginal()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Currency.BaseCurrencyId = UsdCurrencyId;
            env.Currency.SetExchangeRate(UsdCurrencyId, 1.0m);

            var id = service.Create(new Entity("opportunity")
            {
                ["transactioncurrencyid"] = new EntityReference("transactioncurrency", UsdCurrencyId),
                ["estimatedvalue"] = new Money(500m)
            });

            var result = service.Retrieve("opportunity", id, new ColumnSet(true));

            var baseValue = result.GetAttributeValue<Money>("estimatedvalue_base");
            Assert.NotNull(baseValue);
            Assert.Equal(500m, baseValue.Value);
        }

        [Fact]
        public void Update_WithCurrency_RecomputesBaseField()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Currency.BaseCurrencyId = UsdCurrencyId;
            env.Currency.SetExchangeRate(UsdCurrencyId, 1.0m);
            env.Currency.SetExchangeRate(EurCurrencyId, 2.0m);

            var id = service.Create(new Entity("opportunity")
            {
                ["transactioncurrencyid"] = new EntityReference("transactioncurrency", EurCurrencyId),
                ["estimatedvalue"] = new Money(1000m)
            });

            service.Update(new Entity("opportunity", id)
            {
                ["transactioncurrencyid"] = new EntityReference("transactioncurrency", EurCurrencyId),
                ["estimatedvalue"] = new Money(2000m)
            });

            var result = service.Retrieve("opportunity", id, new ColumnSet(true));
            var baseValue = result.GetAttributeValue<Money>("estimatedvalue_base");
            Assert.NotNull(baseValue);
            Assert.Equal(1000m, baseValue.Value); // 2000 / 2.0
        }

        [Fact]
        public void Create_SetsExchangeRateOnEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Currency.BaseCurrencyId = UsdCurrencyId;
            env.Currency.SetExchangeRate(EurCurrencyId, 0.85m);

            var id = service.Create(new Entity("opportunity")
            {
                ["transactioncurrencyid"] = new EntityReference("transactioncurrency", EurCurrencyId),
                ["estimatedvalue"] = new Money(100m)
            });

            var result = service.Retrieve("opportunity", id, new ColumnSet(true));

            Assert.Equal(0.85m, result.GetAttributeValue<decimal>("exchangerate"));
        }

        [Fact]
        public void Create_MultipleMoneyFields_AllGetBaseComputed()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Currency.BaseCurrencyId = UsdCurrencyId;
            env.Currency.SetExchangeRate(EurCurrencyId, 2.0m);

            var id = service.Create(new Entity("opportunity")
            {
                ["transactioncurrencyid"] = new EntityReference("transactioncurrency", EurCurrencyId),
                ["estimatedvalue"] = new Money(1000m),
                ["actualvalue"] = new Money(800m)
            });

            var result = service.Retrieve("opportunity", id, new ColumnSet(true));

            Assert.Equal(500m, result.GetAttributeValue<Money>("estimatedvalue_base")?.Value);
            Assert.Equal(400m, result.GetAttributeValue<Money>("actualvalue_base")?.Value);
        }

        [Fact]
        public void Create_NoCurrency_NoBaseFieldCreated()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Currency.BaseCurrencyId = UsdCurrencyId;
            env.Currency.SetExchangeRate(UsdCurrencyId, 1.0m);

            var id = service.Create(new Entity("opportunity")
            {
                ["estimatedvalue"] = new Money(1000m)
            });

            var result = service.Retrieve("opportunity", id, new ColumnSet(true));

            Assert.False(result.Contains("estimatedvalue_base"));
        }

        [Fact]
        public void GetExchangeRate_UnknownCurrency_ReturnsOne()
        {
            var env = new FakeDataverseEnvironment();
            var rate = env.Currency.GetExchangeRate(Guid.NewGuid());
            Assert.Equal(1.0m, rate);
        }

        [Fact]
        public void SetExchangeRate_InvalidRate_Throws()
        {
            var env = new FakeDataverseEnvironment();
            Assert.Throws<ArgumentOutOfRangeException>(() => env.Currency.SetExchangeRate(Guid.NewGuid(), 0m));
            Assert.Throws<ArgumentOutOfRangeException>(() => env.Currency.SetExchangeRate(Guid.NewGuid(), -1m));
        }

        [Fact]
        public void RetrieveExchangeRate_ReturnsConfiguredRate()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Currency.SetExchangeRate(EurCurrencyId, 0.85m);

            var response = (RetrieveExchangeRateResponse)service.Execute(new RetrieveExchangeRateRequest
            {
                TransactionCurrencyId = EurCurrencyId
            });

            Assert.Equal(0.85m, response.ExchangeRate);
        }

        [Fact]
        public void RetrieveExchangeRate_UnknownCurrency_ReturnsOne()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (RetrieveExchangeRateResponse)service.Execute(new RetrieveExchangeRateRequest
            {
                TransactionCurrencyId = Guid.NewGuid()
            });

            Assert.Equal(1.0m, response.ExchangeRate);
        }
    }
}
