using System;
using System.Threading;
using Microsoft.Xrm.Sdk;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Fake4Dataverse.Moq
{
    /// <summary>
    /// Extension methods for bridging <see cref="FakeOrganizationService"/> with Moq.
    /// </summary>
    public static class MoqServiceExtensions
    {
        /// <summary>
        /// Creates a <see cref="Mock{IOrganizationService}"/> that delegates all calls
        /// to the specified <see cref="FakeOrganizationService"/>.
        /// This enables scenarios where production code accepts <c>Mock&lt;IOrganizationService&gt;</c>
        /// and you want to back it with the full in-memory fake.
        /// </summary>
        public static Mock<IOrganizationService> AsMock(this FakeOrganizationService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            var mock = new Mock<IOrganizationService>();
            IOrganizationService svc = service;

            mock.Setup(m => m.Create(It.IsAny<Entity>()))
                .Returns((Entity e) => svc.Create(e));

            mock.Setup(m => m.Retrieve(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Microsoft.Xrm.Sdk.Query.ColumnSet>()))
                .Returns((string name, Guid id, Microsoft.Xrm.Sdk.Query.ColumnSet cs) => svc.Retrieve(name, id, cs));

            mock.Setup(m => m.RetrieveMultiple(It.IsAny<Microsoft.Xrm.Sdk.Query.QueryBase>()))
                .Returns((Microsoft.Xrm.Sdk.Query.QueryBase q) => svc.RetrieveMultiple(q));

            mock.Setup(m => m.Update(It.IsAny<Entity>()))
                .Callback((Entity e) => svc.Update(e));

            mock.Setup(m => m.Delete(It.IsAny<string>(), It.IsAny<Guid>()))
                .Callback((string name, Guid id) => svc.Delete(name, id));

            mock.Setup(m => m.Execute(It.IsAny<OrganizationRequest>()))
                .Returns((OrganizationRequest r) => svc.Execute(r));

            mock.Setup(m => m.Associate(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Relationship>(), It.IsAny<EntityReferenceCollection>()))
                .Callback((string name, Guid id, Relationship rel, EntityReferenceCollection refs) => svc.Associate(name, id, rel, refs));

            mock.Setup(m => m.Disassociate(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Relationship>(), It.IsAny<EntityReferenceCollection>()))
                .Callback((string name, Guid id, Relationship rel, EntityReferenceCollection refs) => svc.Disassociate(name, id, rel, refs));

            return mock;
        }

        /// <summary>
        /// Creates a <see cref="Mock{IOrganizationServiceAsync2}"/> that delegates all async calls
        /// to the specified <see cref="FakeOrganizationService"/>.
        /// This enables scenarios where production code accepts <c>IOrganizationServiceAsync2</c>
        /// and you want Moq verification against the async interface.
        /// </summary>
        public static Mock<IOrganizationServiceAsync2> AsMockAsync(this FakeOrganizationService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            var mock = new Mock<IOrganizationServiceAsync2>();
            IOrganizationServiceAsync asyncService = service;
            IOrganizationServiceAsync2 asyncService2 = service;

            // IOrganizationServiceAsync (no cancellation token)
            mock.Setup(m => m.CreateAsync(It.IsAny<Entity>()))
                .Returns((Entity e) => asyncService.CreateAsync(e));

            mock.Setup(m => m.RetrieveAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<ColumnSet>()))
                .Returns((string name, Guid id, ColumnSet cs) => asyncService.RetrieveAsync(name, id, cs));

            mock.Setup(m => m.RetrieveMultipleAsync(It.IsAny<QueryBase>()))
                .Returns((QueryBase q) => asyncService.RetrieveMultipleAsync(q));

            mock.Setup(m => m.UpdateAsync(It.IsAny<Entity>()))
                .Returns((Entity e) => asyncService.UpdateAsync(e));

            mock.Setup(m => m.DeleteAsync(It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns((string name, Guid id) => asyncService.DeleteAsync(name, id));

            mock.Setup(m => m.ExecuteAsync(It.IsAny<OrganizationRequest>()))
                .Returns((OrganizationRequest r) => asyncService.ExecuteAsync(r));

            mock.Setup(m => m.AssociateAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Relationship>(), It.IsAny<EntityReferenceCollection>()))
                .Returns((string name, Guid id, Relationship rel, EntityReferenceCollection refs) => asyncService.AssociateAsync(name, id, rel, refs));

            mock.Setup(m => m.DisassociateAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Relationship>(), It.IsAny<EntityReferenceCollection>()))
                .Returns((string name, Guid id, Relationship rel, EntityReferenceCollection refs) => asyncService.DisassociateAsync(name, id, rel, refs));

            // IOrganizationServiceAsync2 (with cancellation token)
            mock.Setup(m => m.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
                .Returns((Entity e, CancellationToken ct) => asyncService2.CreateAsync(e, ct));

            mock.Setup(m => m.CreateAndReturnAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
                .Returns((Entity e, CancellationToken ct) => asyncService2.CreateAndReturnAsync(e, ct));

            mock.Setup(m => m.RetrieveAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<ColumnSet>(), It.IsAny<CancellationToken>()))
                .Returns((string name, Guid id, ColumnSet cs, CancellationToken ct) => asyncService2.RetrieveAsync(name, id, cs, ct));

            mock.Setup(m => m.RetrieveMultipleAsync(It.IsAny<QueryBase>(), It.IsAny<CancellationToken>()))
                .Returns((QueryBase q, CancellationToken ct) => asyncService2.RetrieveMultipleAsync(q, ct));

            mock.Setup(m => m.UpdateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
                .Returns((Entity e, CancellationToken ct) => asyncService2.UpdateAsync(e, ct));

            mock.Setup(m => m.DeleteAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns((string name, Guid id, CancellationToken ct) => asyncService2.DeleteAsync(name, id, ct));

            mock.Setup(m => m.ExecuteAsync(It.IsAny<OrganizationRequest>(), It.IsAny<CancellationToken>()))
                .Returns((OrganizationRequest r, CancellationToken ct) => asyncService2.ExecuteAsync(r, ct));

            mock.Setup(m => m.AssociateAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Relationship>(), It.IsAny<EntityReferenceCollection>(), It.IsAny<CancellationToken>()))
                .Returns((string name, Guid id, Relationship rel, EntityReferenceCollection refs, CancellationToken ct) => asyncService2.AssociateAsync(name, id, rel, refs, ct));

            mock.Setup(m => m.DisassociateAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Relationship>(), It.IsAny<EntityReferenceCollection>(), It.IsAny<CancellationToken>()))
                .Returns((string name, Guid id, Relationship rel, EntityReferenceCollection refs, CancellationToken ct) => asyncService2.DisassociateAsync(name, id, rel, refs, ct));

            return mock;
        }

        /// <summary>
        /// Creates a <see cref="Mock{IOrganizationServiceFactory}"/> that returns
        /// a mock organization service backed by the specified <see cref="FakeOrganizationService"/>.
        /// Useful for plugin testing where <c>IOrganizationServiceFactory</c> is injected.
        /// </summary>
        public static Mock<IOrganizationServiceFactory> AsMockFactory(this FakeOrganizationService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            var mockFactory = new Mock<IOrganizationServiceFactory>();
            mockFactory.Setup(f => f.CreateOrganizationService(It.IsAny<Guid?>()))
                .Returns(service);

            return mockFactory;
        }
    }
}
