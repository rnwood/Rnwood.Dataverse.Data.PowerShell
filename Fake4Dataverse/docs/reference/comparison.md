# Feature Comparison

Three major frameworks exist for unit testing Dataverse / Dynamics 365 applications without requiring a live environment: **Fake4Dataverse**, **FakeXrmEasy**, and **XrmMockup**. Each takes a different approach to simulating Dataverse service interfaces (`IOrganizationService` / `IOrganizationServiceAsync2`), with trade-offs in fidelity, licensing, and developer experience.

This guide provides a fair, comprehensive comparison to help you choose the right tool for your project.

---

## Overview

|                  | Fake4Dataverse                          | FakeXrmEasy (v2+ / v3)                       | XrmMockup                                      |
| ---------------- | --------------------------------------- | -------------------------------------------- | ----------------------------------------------- |
| **License**      | MIT                                     | RPL-1.5 / Commercial                        | MIT                                             |
| **Approach**     | In-memory fake, handler registry        | In-memory fake, middleware pipeline          | In-memory fake, reads real metadata from CRM    |
| **.NET Targets** | `net462` + `net10.0`                    | v2: `net462` + `netcoreapp3.1`; v3: `netcoreapp3.1`+ | `net462`+                                |
| **GitHub**       | [nicknow/Fake4Dataverse][gh-f4d]        | [DynamicsValue/fake-xrm-easy][gh-fxe]       | [delegateas/XrmMockup][gh-xm]                   |
| **NuGet**        | `Fake4Dataverse`                        | `FakeXrmEasy.*` (split packages)             | `XrmMockup365`                                  |
| **Min Setup**    | `new FakeDataverseEnvironment()` + `env.CreateOrganizationService()` | `XrmFakedContext` + middleware builder        | Metadata XML files + `XrmMockup365` instance    |

[gh-f4d]: https://github.com/nicknow/Fake4Dataverse
[gh-fxe]: https://github.com/DynamicsValue/fake-xrm-easy
[gh-xm]: https://github.com/delegateas/XrmMockup

---

## Feature Matrix

✅ = full support &nbsp; ⚠️ = partial / limited &nbsp; ❌ = not supported

### Core Operations

| Feature                        | Fake4Dataverse       | FakeXrmEasy          | XrmMockup            |
| ------------------------------ | :------------------: | :------------------: | :------------------: |
| **Basic CRUD**                 | ✅                   | ✅                   | ✅                   |
| **Upsert**                     | ✅                   | ✅                   | ✅                   |
| **Assign**                     | ✅                   | ✅                   | ✅                   |
| **SetState**                   | ✅                   | ✅                   | ✅                   |
| **ExecuteMultiple**            | ✅                   | ✅                   | ✅                   |
| **ExecuteTransaction**         | ✅                   | ❌                   | ✅                   |
| **100+ Request Handlers**      | ✅                   | ✅                   | ✅                   |
| **Custom APIs**                | ✅ `RegisterCustomApi` | ✅ (v2+)           | ✅                   |
| **WhoAmI**                     | ✅                   | ✅                   | ✅                   |

### Querying

| Feature                        | Fake4Dataverse       | FakeXrmEasy          | XrmMockup            |
| ------------------------------ | :------------------: | :------------------: | :------------------: |
| **QueryExpression**            | ✅ 40+ operators     | ✅                   | ✅                   |
| **FetchXml**                   | ✅ Full + aggregates | ✅ Full              | ✅ Full              |
| **FetchXml Aggregates**        | ✅ Count/Sum/Avg/Min/Max | ✅               | ✅                   |
| **LinkEntity Joins**           | ✅ Multi-level + semi-joins | ✅             | ✅                   |
| **Paging**                     | ✅ Cookie-based      | ✅                   | ✅                   |
| **Alternate Keys**             | ✅ Full              | ⚠️ Limited           | ✅                   |
| **Attribute Indexing**         | ✅ Equality indexes  | ❌                   | ⚠️ Implicit          |

### Pipeline & Plugins

| Feature                        | Fake4Dataverse       | FakeXrmEasy          | XrmMockup            |
| ------------------------------ | :------------------: | :------------------: | :------------------: |
| **Plugin Pipeline**            | ✅ Pre-val / Pre-op / Post-op | ✅ Plugin simulation | ✅ Auto-executes plugins |
| **Real IPlugin Testing**       | ✅ Full `IServiceProvider` | ✅              | ✅                   |
| **Workflow / Code Activities** | ❌                   | ✅                   | ✅                   |
| **SPKL Integration**           | ✅ Auto-register from attributes | ❌        | ❌                   |
| **Impersonation**              | ⚠️ `CallerId` / `InitiatingUserId` | ❌    | ✅ In plugins        |

### Security

| Feature                        | Fake4Dataverse       | FakeXrmEasy          | XrmMockup            |
| ------------------------------ | :------------------: | :------------------: | :------------------: |
| **Security Roles**             | ✅ Full role + depth model | ⚠️ Limited     | ✅ Full              |
| **Record Sharing**             | ✅ Grant / Modify / Revoke | ⚠️ Limited     | ✅                   |
| **Team Security**              | ✅                   | ⚠️ Limited           | ✅                   |
| **State Transitions**          | ✅ Validation        | ⚠️                   | ✅                   |

### Metadata & Field Logic

| Feature                        | Fake4Dataverse       | FakeXrmEasy          | XrmMockup            |
| ------------------------------ | :------------------: | :------------------: | :------------------: |
| **Metadata Validation**        | ✅ Optional (Strict mode) | ⚠️ Optional     | ✅ From real metadata |
| **Calculated Fields**          | ✅ Lambda formulas   | ✅                   | ✅                   |
| **Rollup Fields**              | ✅ 5 aggregate types | ✅                   | ✅                   |
| **PowerFx Fields**             | ❌                   | ❌                   | ✅ Experimental      |
| **Currency / Exchange Rates**  | ✅ Auto base-currency | ✅                  | ✅                   |
| **Formatted Values**           | ✅ Auto-populated    | ✅                   | ✅                   |
| **Cascade Operations**         | ✅ Delete + Assign   | ✅                   | ✅                   |
| **Early-Bound Support**        | ✅ Assembly scanning | ✅ CrmSvcUtil / PAC  | ✅ DLC + generator   |
| **Metadata Generator Tool**    | ❌                   | ❌                   | ✅ .NET tool         |

### Binary & File Operations

| Feature                        | Fake4Dataverse       | FakeXrmEasy          | XrmMockup            |
| ------------------------------ | :------------------: | :------------------: | :------------------: |
| **Binary / File Columns**      | ✅ Block-based upload / download | ✅       | ✅                   |

### Test Infrastructure

| Feature                        | Fake4Dataverse       | FakeXrmEasy          | XrmMockup            |
| ------------------------------ | :------------------: | :------------------: | :------------------: |
| **Thread Safety**              | ✅ Full (thread-safe)     | ⚠️ Limited      | ✅                   |
| **Deep Cloning**               | ✅ Always            | ✅                   | ✅                   |
| **Snapshot / Restore**         | ✅ `TakeSnapshot` / `RestoreSnapshot` | ❌  | ✅ File-based        |
| **Scope (Auto-Rollback)**      | ✅ `env.Scope()` | ❌                  | ❌                   |
| **Time Control**               | ✅ `FakeClock` + `AdvanceTime` | ⚠️ Optional | ✅                  |
| **Operation Log**              | ✅ Built-in          | ❌                   | ❌                   |
| **Fluent Assertions**          | ✅ Built-in + 3 adapters | ❌              | ❌                   |
| **Mocking Adapters**           | ✅ Moq + FakeItEasy  | ⚠️ DI patterns      | ❌                   |
| **Seeding (JSON / CSV)**       | ✅ Built-in          | ✅                   | ⚠️ Live data         |
| **Live Data Loading**          | ❌                   | ❌                   | ✅                   |

### Architecture & Extensibility

| Feature                        | Fake4Dataverse       | FakeXrmEasy          | XrmMockup            |
| ------------------------------ | :------------------: | :------------------: | :------------------: |
| **Async `IOrganizationService`** | ✅ `IOrganizationServiceAsync2` | ✅ `IOrganizationServiceAsync2` | ✅          |
| **DI / Middleware**            | ❌                   | ✅ ASP.NET Core style | ❌                  |

---

## Licensing

|                              | Fake4Dataverse | FakeXrmEasy                                                     | XrmMockup |
| ---------------------------- | :------------: | :-------------------------------------------------------------: | :-------: |
| **Open Source**              | ✅ MIT         | ⚠️ v1 MIT; v2+ RPL-1.5                                         | ✅ MIT    |
| **Free for Commercial Use**  | ✅             | ⚠️ v2+ requires commercial license for ISVs / SaaS             | ✅        |
| **Commercial Support**       | ❌             | ✅                                                               | ❌        |

FakeXrmEasy v1 remains MIT-licensed but targets .NET Framework only and is no longer maintained. Starting with v2, the license changed to [RPL-1.5](https://opensource.org/licenses/RPL-1.5), which requires a paid commercial license for proprietary / SaaS use.

---

## When to Choose Each

### Choose Fake4Dataverse when:

- You want a **modern, MIT-licensed** solution with no commercial restrictions.
- **Test isolation** matters — `Scope()` for automatic rollback and `TakeSnapshot()` / `RestoreSnapshot()` for save-points.
- You need **rich assertion support** via built-in extensions plus adapters for AwesomeAssertions, FluentAssertions, and Shouldly.
- You value an **environment + session model**: `new FakeDataverseEnvironment()` + `env.CreateOrganizationService()` for multi-user testing out of the box.
- You want **deterministic time** with `FakeClock` and `AdvanceTime`.
- You need an **operation log** for verifying what service calls were made.
- You need **sync + async service interface parity** (`IOrganizationService` and `IOrganizationServiceAsync2`).
- **Thread safety** is required for parallel test execution.

### Choose FakeXrmEasy when:

- You need **workflow / code activity** testing.
- Your team already uses FakeXrmEasy v1 and wants an **upgrade path**.
- You need **commercial support** with SLAs.
- You want **middleware-style extensibility** (ASP.NET Core–inspired pipeline).
- You prefer a **DI-oriented** architecture.

### Choose XrmMockup when:

- You want the **most production-realistic** testing — it reads actual metadata exported from your environment.
- You need **PowerFx formula field** evaluation (experimental).
- You want a **metadata generator tool** to keep test metadata in sync with your org.
- You need **live data loading** for reproducing production scenarios locally.
- Plugin **impersonation** in the pipeline is critical to your tests.

---

## Migration

If you're migrating from FakeXrmEasy to Fake4Dataverse, see the [Migration Guide](../guides/migration-from-fakexrmeasy.md) for a step-by-step walkthrough including API mapping and pattern equivalents.
