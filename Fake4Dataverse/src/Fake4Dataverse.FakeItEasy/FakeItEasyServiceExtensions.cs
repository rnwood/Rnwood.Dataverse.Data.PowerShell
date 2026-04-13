using System;
using System.Threading;
using FakeItEasy;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.FakeItEasy
{
    /// <summary>
    /// Extension methods for bridging <see cref="FakeOrganizationService"/> with FakeItEasy.
    /// </summary>
    public static class FakeItEasyServiceExtensions
    {
        /// <summary>
        /// Creates an <see cref="IOrganizationService"/> fake that delegates all calls
        /// to the specified <see cref="FakeOrganizationService"/>.
        /// This enables scenarios where production code accepts <see cref="IOrganizationService"/>
        /// and you still want FakeItEasy call assertions on the wrapper.
        /// </summary>
        public static IOrganizationService AsFake(this FakeOrganizationService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            IOrganizationService wrappedService = service;
            var fake = A.Fake<IOrganizationService>();

            A.CallTo(() => fake.Create(A<Entity>.Ignored))
                .ReturnsLazily((Entity entity) => wrappedService.Create(entity));

            A.CallTo(() => fake.Retrieve(A<string>.Ignored, A<Guid>.Ignored, A<ColumnSet>.Ignored))
                .ReturnsLazily((string entityName, Guid id, ColumnSet columnSet) => wrappedService.Retrieve(entityName, id, columnSet));

            A.CallTo(() => fake.RetrieveMultiple(A<QueryBase>.Ignored))
                .ReturnsLazily((QueryBase query) => wrappedService.RetrieveMultiple(query));

            A.CallTo(() => fake.Update(A<Entity>.Ignored))
                .Invokes((Entity entity) => wrappedService.Update(entity));

            A.CallTo(() => fake.Delete(A<string>.Ignored, A<Guid>.Ignored))
                .Invokes((string entityName, Guid id) => wrappedService.Delete(entityName, id));

            A.CallTo(() => fake.Execute(A<OrganizationRequest>.Ignored))
                .ReturnsLazily((OrganizationRequest request) => wrappedService.Execute(request));

            A.CallTo(() => fake.Associate(A<string>.Ignored, A<Guid>.Ignored, A<Relationship>.Ignored, A<EntityReferenceCollection>.Ignored))
                .Invokes((string entityName, Guid id, Relationship relationship, EntityReferenceCollection relatedEntities) => wrappedService.Associate(entityName, id, relationship, relatedEntities));

            A.CallTo(() => fake.Disassociate(A<string>.Ignored, A<Guid>.Ignored, A<Relationship>.Ignored, A<EntityReferenceCollection>.Ignored))
                .Invokes((string entityName, Guid id, Relationship relationship, EntityReferenceCollection relatedEntities) => wrappedService.Disassociate(entityName, id, relationship, relatedEntities));

            return fake;
        }

        /// <summary>
        /// Creates an <see cref="IOrganizationServiceAsync2"/> fake that delegates all async calls
        /// to the specified <see cref="FakeOrganizationService"/>.
        /// This enables scenarios where production code accepts <see cref="IOrganizationServiceAsync2"/>
        /// and you still want FakeItEasy call assertions on the wrapper.
        /// </summary>
        public static IOrganizationServiceAsync2 AsFakeAsync(this FakeOrganizationService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            IOrganizationServiceAsync wrappedAsyncService = service;
            IOrganizationServiceAsync2 wrappedAsyncService2 = service;
            var fake = A.Fake<IOrganizationServiceAsync2>();

            // IOrganizationServiceAsync (no cancellation token)
            A.CallTo(() => fake.CreateAsync(A<Entity>.Ignored))
                .ReturnsLazily((Entity entity) => wrappedAsyncService.CreateAsync(entity));

            A.CallTo(() => fake.RetrieveAsync(A<string>.Ignored, A<Guid>.Ignored, A<ColumnSet>.Ignored))
                .ReturnsLazily((string entityName, Guid id, ColumnSet columnSet) => wrappedAsyncService.RetrieveAsync(entityName, id, columnSet));

            A.CallTo(() => fake.RetrieveMultipleAsync(A<QueryBase>.Ignored))
                .ReturnsLazily((QueryBase query) => wrappedAsyncService.RetrieveMultipleAsync(query));

            A.CallTo(() => fake.UpdateAsync(A<Entity>.Ignored))
                .ReturnsLazily((Entity entity) => wrappedAsyncService.UpdateAsync(entity));

            A.CallTo(() => fake.DeleteAsync(A<string>.Ignored, A<Guid>.Ignored))
                .ReturnsLazily((string entityName, Guid id) => wrappedAsyncService.DeleteAsync(entityName, id));

            A.CallTo(() => fake.ExecuteAsync(A<OrganizationRequest>.Ignored))
                .ReturnsLazily((OrganizationRequest request) => wrappedAsyncService.ExecuteAsync(request));

            A.CallTo(() => fake.AssociateAsync(A<string>.Ignored, A<Guid>.Ignored, A<Relationship>.Ignored, A<EntityReferenceCollection>.Ignored))
                .ReturnsLazily((string entityName, Guid id, Relationship relationship, EntityReferenceCollection relatedEntities) => wrappedAsyncService.AssociateAsync(entityName, id, relationship, relatedEntities));

            A.CallTo(() => fake.DisassociateAsync(A<string>.Ignored, A<Guid>.Ignored, A<Relationship>.Ignored, A<EntityReferenceCollection>.Ignored))
                .ReturnsLazily((string entityName, Guid id, Relationship relationship, EntityReferenceCollection relatedEntities) => wrappedAsyncService.DisassociateAsync(entityName, id, relationship, relatedEntities));

            // IOrganizationServiceAsync2 (with cancellation token)
            A.CallTo(() => fake.CreateAsync(A<Entity>.Ignored, A<CancellationToken>.Ignored))
                .ReturnsLazily((Entity entity, CancellationToken ct) => wrappedAsyncService2.CreateAsync(entity, ct));

            A.CallTo(() => fake.CreateAndReturnAsync(A<Entity>.Ignored, A<CancellationToken>.Ignored))
                .ReturnsLazily((Entity entity, CancellationToken ct) => wrappedAsyncService2.CreateAndReturnAsync(entity, ct));

            A.CallTo(() => fake.RetrieveAsync(A<string>.Ignored, A<Guid>.Ignored, A<ColumnSet>.Ignored, A<CancellationToken>.Ignored))
                .ReturnsLazily((string entityName, Guid id, ColumnSet columnSet, CancellationToken ct) => wrappedAsyncService2.RetrieveAsync(entityName, id, columnSet, ct));

            A.CallTo(() => fake.RetrieveMultipleAsync(A<QueryBase>.Ignored, A<CancellationToken>.Ignored))
                .ReturnsLazily((QueryBase query, CancellationToken ct) => wrappedAsyncService2.RetrieveMultipleAsync(query, ct));

            A.CallTo(() => fake.UpdateAsync(A<Entity>.Ignored, A<CancellationToken>.Ignored))
                .ReturnsLazily((Entity entity, CancellationToken ct) => wrappedAsyncService2.UpdateAsync(entity, ct));

            A.CallTo(() => fake.DeleteAsync(A<string>.Ignored, A<Guid>.Ignored, A<CancellationToken>.Ignored))
                .ReturnsLazily((string entityName, Guid id, CancellationToken ct) => wrappedAsyncService2.DeleteAsync(entityName, id, ct));

            A.CallTo(() => fake.ExecuteAsync(A<OrganizationRequest>.Ignored, A<CancellationToken>.Ignored))
                .ReturnsLazily((OrganizationRequest request, CancellationToken ct) => wrappedAsyncService2.ExecuteAsync(request, ct));

            A.CallTo(() => fake.AssociateAsync(A<string>.Ignored, A<Guid>.Ignored, A<Relationship>.Ignored, A<EntityReferenceCollection>.Ignored, A<CancellationToken>.Ignored))
                .ReturnsLazily((string entityName, Guid id, Relationship relationship, EntityReferenceCollection relatedEntities, CancellationToken ct) => wrappedAsyncService2.AssociateAsync(entityName, id, relationship, relatedEntities, ct));

            A.CallTo(() => fake.DisassociateAsync(A<string>.Ignored, A<Guid>.Ignored, A<Relationship>.Ignored, A<EntityReferenceCollection>.Ignored, A<CancellationToken>.Ignored))
                .ReturnsLazily((string entityName, Guid id, Relationship relationship, EntityReferenceCollection relatedEntities, CancellationToken ct) => wrappedAsyncService2.DisassociateAsync(entityName, id, relationship, relatedEntities, ct));

            return fake;
        }

        /// <summary>
        /// Creates an <see cref="IOrganizationServiceFactory"/> fake that returns
        /// the specified <see cref="FakeOrganizationService"/> for any caller ID.
        /// Useful for plugin testing where <see cref="IOrganizationServiceFactory"/> is injected.
        /// </summary>
        public static IOrganizationServiceFactory AsFakeFactory(this FakeOrganizationService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            var fakeFactory = A.Fake<IOrganizationServiceFactory>();
            A.CallTo(() => fakeFactory.CreateOrganizationService(A<Guid?>.Ignored))
                .Returns(service);

            return fakeFactory;
        }
    }
}
