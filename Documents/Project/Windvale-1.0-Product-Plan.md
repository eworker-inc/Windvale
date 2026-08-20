# Windvale 1.0 product plan

- Date: 2026-08-20
- Status: Active product target; implementation and qualification incomplete
- Direction: [Decision 0800](../Decisions/0800-Target-Windvale-1.0-Directly.md)
- Language contract: [Language 1.0 freeze](../Decisions/0767-Freeze-Windvale-Language-1.0-Source.md)
- Library program: [Windvale Libraries 1.0](Windvale-Libraries-1.0-Plan.md)
- Database program: [WVDB 1.0](WVDB-1.0-Specification-Plan.md)

## Product outcome

Windvale 1.0 is one coherent, supported Windows and Linux host product built
from Windvale's own language, compiler, verified execution contracts, reusable
libraries, package system, and database. It should let a person install the
toolchain, build and run bounded backend applications, store and recover useful
data through WVDB, and inspect the authority and release evidence without a
development checkout or a retired managed runtime.

The next intended product tag is `v1.0.0`. There is no planned `v0.2.0` product
release. The version is earned by the complete gate below; it is not a label for
the current repository state.

## Planning model

The roadmap uses workstreams and gates. Workstreams may advance concurrently,
and each may use small implementation slices. Those slices are engineering
units, not public product stages and not compatibility levels.

The current dependency shape is:

```text
frozen Language 1.0 contract
  -> compiler/runtime/native implementation and conformance
  -> required Libraries 1.0 contracts and providers
  -> WVDB 1.0 and ordinary backend applications

package, installer, service, security, and operations work
  -> makes every selected component installable and supportable

all required workstreams
  -> one cross-host Windvale 1.0 release gate
```

This is a dependency map, not a release sequence. A downstream specification can
advance while an upstream implementation slice is still being completed, but
qualification cannot claim behavior that the selected implementation lacks.

## Required workstreams

### Language, compiler, runtime, and toolchain

The Language 1.0 source design is frozen by Decision 0767. Complete its bounded
implementation in the normal Windvale compiler and shared verified execution
path. Compiler slice numbers describe that implementation work; they do not
reopen the frozen design.

The release gate requires exact source, diagnostics, package-data, ownership,
concurrency, resource, WIR/WVB, interpreter/native, and Windows/Linux conformance
for the selected 1.0 surface. A source-rule change requires a named defect or
contradiction and a decision that updates the freeze deliberately.

### Windvale Libraries 1.0

Freeze and implement the required Foundation, Data, and Backend profile matrix.
For the 1.0 host product this includes the bounded value and collection nucleus,
text and binary data formats, validation, filesystem and storage access, time,
entropy, cryptography and certificate handling, networking, secure streams,
HTTP services/clients, service lifecycle, diagnostics, configuration, and the
algorithms needed by WVDB and ordinary backend applications.

Each module must have exact public names, types, limits, capabilities, mutation
completion behavior, portability scope, test oracles, and package identity.
Optional System, UI, accelerator, model-provider, and specialized protocol
profiles may ship when qualified but do not become implicit core requirements.

### WVDB 1.0

Complete the accepted shared database core, strict table profile, and basic typed
relationship profile. The required product includes stable identity and catalog
rules, scalar and structured values, schemas, primary and foreign/reference keys,
constraints, ordered indexes, typed queries and results, snapshots and
transactions, storage and reclamation, durability and integrity, full backup and
restore, service sessions, authorization, limits, diagnostics, migration policy,
operations, and conformance.

Document, analytical, full-text, vector/semantic, and broader graph profiles
remain explicit extensions unless a later 1.0 scope decision admits a finite
subset. PostgreSQL, SQLite, MySQL/InnoDB, SQL Server, MongoDB, Neo4j, DuckDB, and
other systems are research comparisons only, never compatibility or parity
authorities.

### Packages, installation, services, and release

Integrate immutable packages, approvals, generations, activation, rollback,
recoverable uninstall, and signed release admission into an ordinary installed
product. Supply supported per-user toolchain installation and the explicitly
privileged system-service path needed to run WVDB on Windows and Debian/Linux.

The release must define safe install, upgrade, health, rollback, database-format
admission, data preservation, service removal, offline verification, and recovery.
An official connected source may improve delivery, but downloaded and offline
admission must select the same signed immutable objects. No transport location is
an authority or artifact identity.

### Security, operations, and evidence

Close the public threat model for every shipped parser, binary format, provider,
credential, package, service, and database boundary. Publish finite defaults and
configurable ceilings for input, memory, storage, concurrency, work, queues,
deadlines, diagnostics, backup, and recovery.

The selected release state must have deterministic builds where promised,
hostile-input coverage, interruption and crash evidence, bounded soak and
resource workloads, cross-host conformance, release provenance, signed artifacts,
and an independently usable offline verification path.

## Windvale 1.0 release gate

The `v1.0.0` tag waits until all of the following are true:

| Gate | Required evidence |
| --- | --- |
| Contract | The selected Language, Libraries, WVDB, package, service, and support contracts are normative, versioned, internally consistent, and have explicit exclusions. |
| Implementation | Every required contract has one owned implementation path; candidates and historical fixtures are reconciled or rejected. |
| Usefulness | A clean Windows and Linux installation can build and run representative backend applications and create, query, transact, back up, restore, and operate a WVDB service. |
| Safety and authority | Capabilities are exact and rights-limited; untrusted input is bounded; uncertain mutations, revocation, failure, teardown, and recovery are explicit. |
| Compatibility | The 1.0 stability, support, deprecation, file-format, package, and migration promises are written before release. |
| Qualification | The exact release commit and artifacts pass the selected Windows/Linux conformance, determinism, performance, memory, recovery, security, and release gates. |
| Distribution | Signed source, tools, packages, installers, database artifacts, documentation, provenance, and offline verification evidence are published against one immutable tag. |

Passing one row does not authorize the tag. Product progress should report each
row independently and name gaps without translating a percentage into a
compatibility claim.

## Explicitly outside the automatic 1.0 gate

- a complete general-purpose Windvale OS, desktop, or broad hardware catalog;
- wire, SQL, file, API, or behavioral compatibility with another database;
- .NET, Java, ASP.NET, E-Worker, or another framework/runtime dependency;
- distributed consensus, clustering, automatic failover, or multi-region WVDB;
- every optional library, database profile, browser application, model gateway,
  agent system, accelerator, or virtualization feature; and
- preservation of obsolete development formats without a named migration case.

These may advance independently and may ship when their own contracts qualify.
They do not silently enlarge the Windvale 1.0 promise.

## Existing versions and artifacts

The signed `v0.1.0` preview and its exact evidence remain published history.
Completed milestone records remain useful provenance but no longer organize the
forward roadmap. Checked-in `0.2.0-dev.1` installer/repository candidates keep
their exact historical names and hashes; they are implementation inputs, not a
selected `v0.2.0` release. Select any new 1.0 development artifact identity by
an explicit release or format decision rather than renaming immutable bytes.

## Immediate planning work

1. Keep the Language 1.0 implementation ledger synchronized with the frozen
   contract and record only concrete freeze defects as design issues.
2. Turn the Libraries 1.0 catalog into an accepted required/optional module and
   conformance matrix.
3. Continue WVDB 1.0 normative specifications from the accepted upper-layer
   decisions through storage, durability, service, operations, and conformance.
4. Reframe connected package, networking, and service work as reusable 1.0
   foundations; remove the external-model gateway as an automatic release gate.
5. Define the 1.0 stability/support and exact integrated qualification policies
   before selecting a release candidate.
