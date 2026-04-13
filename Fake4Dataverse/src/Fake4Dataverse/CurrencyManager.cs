using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse
{
    /// <summary>
    /// Manages exchange rates and auto-computes base currency amounts for Money fields.
    /// When an entity has a <c>transactioncurrencyid</c> lookup and Money fields,
    /// the corresponding <c>{field}_base</c> values are auto-computed.
    /// </summary>
    public sealed class CurrencyManager
    {
        private readonly Dictionary<Guid, decimal> _exchangeRates = new Dictionary<Guid, decimal>();

        /// <summary>
        /// Gets or sets the base (organization) currency ID.
        /// </summary>
        public Guid BaseCurrencyId { get; set; }

        /// <summary>
        /// Gets or sets the exchange rate for a specific currency.
        /// The rate is relative to the base currency (e.g., 1.0 for base, 0.85 for EUR if base is USD).
        /// </summary>
        /// <param name="currencyId">The transaction currency ID.</param>
        /// <param name="rate">The exchange rate relative to the base currency.</param>
        public void SetExchangeRate(Guid currencyId, decimal rate)
        {
            if (rate <= 0) throw new ArgumentOutOfRangeException(nameof(rate), "Exchange rate must be positive.");
            _exchangeRates[currencyId] = rate;
        }

        /// <summary>
        /// Gets the exchange rate for a currency. Returns 1.0 for unknown currencies.
        /// </summary>
        /// <param name="currencyId">The transaction currency ID.</param>
        /// <returns>The exchange rate.</returns>
        public decimal GetExchangeRate(Guid currencyId)
        {
            return _exchangeRates.TryGetValue(currencyId, out var rate) ? rate : 1.0m;
        }

        /// <summary>
        /// Returns true if any exchange rates have been configured.
        /// </summary>
        internal bool IsConfigured => _exchangeRates.Count > 0 || BaseCurrencyId != Guid.Empty;

        /// <summary>
        /// Auto-computes base currency fields on an entity during Create or Update.
        /// For each Money attribute, sets <c>{attributeName}_base</c> = value / exchange rate.
        /// </summary>
        internal void ComputeBaseCurrencyFields(Entity entity)
        {
            var currencyRef = entity.GetAttributeValue<EntityReference>("transactioncurrencyid");
            if (currencyRef == null)
                return;

            var rate = GetExchangeRate(currencyRef.Id);
            var moneyFields = new List<KeyValuePair<string, Money>>();

            foreach (var attr in entity.Attributes)
            {
                if (attr.Value is Money money && !attr.Key.EndsWith("_base", StringComparison.OrdinalIgnoreCase))
                {
                    moneyFields.Add(new KeyValuePair<string, Money>(attr.Key, money));
                }
            }

            foreach (var field in moneyFields)
            {
                entity[field.Key + "_base"] = new Money(field.Value.Value / rate);
            }

            if (!entity.Contains("exchangerate"))
            {
                entity["exchangerate"] = rate;
            }
        }
    }
}
