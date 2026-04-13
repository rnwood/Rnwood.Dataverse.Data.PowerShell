#if NET8_0_OR_GREATER
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Abstractions.Enums;
#endif
using System;
using BenchmarkDotNet.Attributes;
#if NET462
using FakeXrmEasy;
#endif
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Benchmarks
{
    /// <summary>
    /// CRUD throughput comparison:
    ///   .NET Framework 4.6.2 — Fake4Dataverse vs FakeXrmEasy 1.x (FakeXrmEasy.9 v1.58.1, MIT)
    ///   .NET 10              — Fake4Dataverse vs FakeXrmEasy 3.x (FakeXrmEasy.Core.v9 v3.9.0, RPL-1.5)
    ///
    /// F4D = Fake4Dataverse (Lenient preset)
    /// V1  = FakeXrmEasy v1 (net462 only — depends on System.Activities)
    /// V3  = FakeXrmEasy v3 (net8+ only)
    /// </summary>
    [MemoryDiagnoser]
    public class CrudBenchmarks
    {
        private FakeOrganizationService _f4d = null!;
        private FakeDataverseEnvironment _f4dEnv = null!;
        private Guid _f4dPreseededId;

#if NET462
        private IOrganizationService _v1 = null!;
        private Guid _v1PreseededId;
#elif NET8_0_OR_GREATER
        private IOrganizationService _v3 = null!;
        private Guid _v3PreseededId;
#endif

        [GlobalSetup]
        public void Setup()
        {
            _f4dEnv = new FakeDataverseEnvironment(FakeOrganizationServiceOptions.Lenient);
            _f4d = _f4dEnv.CreateOrganizationService();
            _f4dPreseededId = _f4d.Create(new Entity("account") { ["name"] = "Preset" });

#if NET462
            var v1ctx = new XrmFakedContext();
            _v1 = v1ctx.GetOrganizationService();
            _v1PreseededId = _v1.Create(new Entity("account") { ["name"] = "Preset" });
#elif NET8_0_OR_GREATER
            var v3ctx = MiddlewareBuilder
                .New()
                .AddCrud()
                .UseCrud()
                .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                .Build();
            _v3 = v3ctx.GetOrganizationService();
            _v3PreseededId = _v3.Create(new Entity("account") { ["name"] = "Preset" });
#endif
        }

        [Benchmark(Baseline = true)]
        public Guid F4D_Create() =>
            _f4d.Create(new Entity("account") { ["name"] = "Benchmark" });

#if NET462
        [Benchmark]
        public Guid V1_Create() =>
            _v1.Create(new Entity("account") { ["name"] = "Benchmark" });
#elif NET8_0_OR_GREATER
        [Benchmark]
        public Guid V3_Create() =>
            _v3.Create(new Entity("account") { ["name"] = "Benchmark" });
#endif

        [Benchmark]
        public void F4D_CreateUpdateDelete()
        {
            var id = _f4d.Create(new Entity("account") { ["name"] = "Benchmark" });
            _f4d.Update(new Entity("account") { Id = id, ["name"] = "Updated" });
            _f4d.Delete("account", id);
        }

#if NET462
        [Benchmark]
        public void V1_CreateUpdateDelete()
        {
            var id = _v1.Create(new Entity("account") { ["name"] = "Benchmark" });
            _v1.Update(new Entity("account") { Id = id, ["name"] = "Updated" });
            _v1.Delete("account", id);
        }
#elif NET8_0_OR_GREATER
        [Benchmark]
        public void V3_CreateUpdateDelete()
        {
            var id = _v3.Create(new Entity("account") { ["name"] = "Benchmark" });
            _v3.Update(new Entity("account") { Id = id, ["name"] = "Updated" });
            _v3.Delete("account", id);
        }
#endif

        [Benchmark]
        public Entity F4D_Retrieve() =>
            _f4d.Retrieve("account", _f4dPreseededId, new ColumnSet(true));

#if NET462
        [Benchmark]
        public Entity V1_Retrieve() =>
            _v1.Retrieve("account", _v1PreseededId, new ColumnSet(true));
#elif NET8_0_OR_GREATER
        [Benchmark]
        public Entity V3_Retrieve() =>
            _v3.Retrieve("account", _v3PreseededId, new ColumnSet(true));
#endif

        [Benchmark]
        public void F4D_Update() =>
            _f4d.Update(new Entity("account") { Id = _f4dPreseededId, ["name"] = "Updated" });

#if NET462
        [Benchmark]
        public void V1_Update() =>
            _v1.Update(new Entity("account") { Id = _v1PreseededId, ["name"] = "Updated" });
#elif NET8_0_OR_GREATER
        [Benchmark]
        public void V3_Update() =>
            _v3.Update(new Entity("account") { Id = _v3PreseededId, ["name"] = "Updated" });
#endif
    }
}
