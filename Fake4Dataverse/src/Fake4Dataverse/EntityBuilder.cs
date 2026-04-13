using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse
{
    /// <summary>
    /// Fluent builder for constructing <see cref="Entity"/> instances for test data seeding.
    /// </summary>
    public sealed class EntityBuilder
    {
        private readonly Entity _entity;

        /// <summary>
        /// Creates a new builder for the specified entity logical name.
        /// </summary>
        /// <param name="entityName">The logical name of the entity to build.</param>
        public EntityBuilder(string entityName)
        {
            _entity = new Entity(entityName);
        }

        /// <summary>
        /// Sets the entity's unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier to assign.</param>
        /// <returns>This builder for chaining.</returns>
        public EntityBuilder WithId(Guid id)
        {
            _entity.Id = id;
            return this;
        }

        /// <summary>
        /// Sets an attribute value on the entity.
        /// </summary>
        /// <param name="name">The attribute logical name.</param>
        /// <param name="value">The attribute value.</param>
        /// <returns>This builder for chaining.</returns>
        public EntityBuilder WithAttribute(string name, object value)
        {
            _entity[name] = value;
            return this;
        }

        /// <summary>
        /// Sets the "name" attribute on the entity.
        /// </summary>
        /// <param name="name">The name value.</param>
        /// <returns>This builder for chaining.</returns>
        public EntityBuilder WithName(string name)
        {
            _entity["name"] = name;
            return this;
        }

        /// <summary>
        /// Sets the statecode and statuscode on the entity.
        /// </summary>
        /// <param name="statecode">The state code value.</param>
        /// <param name="statuscode">The status code value (defaults to 1).</param>
        /// <returns>This builder for chaining.</returns>
        public EntityBuilder WithState(int statecode, int statuscode = 1)
        {
            _entity["statecode"] = new OptionSetValue(statecode);
            _entity["statuscode"] = new OptionSetValue(statuscode);
            return this;
        }

        /// <summary>
        /// Sets the ownerid on the entity.
        /// </summary>
        /// <param name="ownerId">The owner's system user ID.</param>
        /// <returns>This builder for chaining.</returns>
        public EntityBuilder WithOwner(Guid ownerId)
        {
            _entity["ownerid"] = new EntityReference("systemuser", ownerId);
            return this;
        }

        /// <summary>
        /// Builds and returns the configured entity.
        /// </summary>
        /// <returns>The constructed <see cref="Entity"/>.</returns>
        public Entity Build() => _entity;
    }
}
