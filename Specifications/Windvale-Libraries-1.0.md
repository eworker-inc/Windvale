# Windvale Libraries 1.0

## Status and scope

- Status: Draft proposal for owner review
- Product identity: **Windvale Libraries 1.0**
- Language dependency: [Windvale Language 1.0](Windvale-Language-1.0.md)
- Foundation dependency: [Windvale Language 1.0 Foundation](Windvale-Language-1.0-Foundation.md)
- Backend profile: [Windvale Backend Libraries 1.0](Windvale-Backend-Libraries-1.0.md)
- Binary data profile: [Windvale binary data profile 1.0](Windvale-Binary-Data-1.0.md)
- Delivery plan: [Windvale Libraries 1.0 plan](../Documents/Project/Windvale-Libraries-1.0-Plan.md)

This document defines the proposed public organization and common contracts for
the official Windvale 1.0 library suite. It does not claim that every listed
module or operation is implemented. Existing checked-in modules retain their
current specifications until they are deliberately migrated, replaced, or
qualified against this contract.

Windvale Libraries 1.0 is not a compatibility layer for .NET, Java, POSIX,
browser JavaScript, or another framework. Those ecosystems provide useful
product comparisons and workload examples, but Windvale owns its module names,
types, bounds, capability model, failure behavior, and versioning.

## Product relationship

The three first-version products have distinct ownership:

| Product | Owns |
| --- | --- |
| Windvale Language 1.0 | Source syntax, typing, ownership, effects, profiles, WIR/WVB semantics, and execution contracts. |
| Windvale Libraries 1.0 | Reusable values, algorithms, codecs, domain APIs, capability-facing adapters, and application frameworks. |
| WVDB 1.0 | Database schema, records, relationships, indexes, queries, transactions, storage, recovery, backup, and database conformance. |

A library may expose WVDB to an application, but it does not redefine WVDB.
The database may use Foundation and storage libraries, but it does not make
those libraries database-specific. Language syntax does not absorb an API
merely because the API is common.

## Goals

Windvale Libraries 1.0 should let applications build useful software without
reimplementing routine infrastructure. The intended workloads include:

- command-line programs and repository tools;
- bounded text and binary data parsers, encoders, and transformations;
- local and service-backed WVDB applications;
- JSON and CBOR APIs and HTTPS services for browser and native clients;
- single-page-application and mobile backends;
- file, blob, package, cache, and configuration management;
- authenticated client and service connections;
- scheduled and concurrent hosted work with explicit cancellation;
- structured diagnostics, metrics, and test providers; and
- later Windvale OS applications through the same semantic contracts.

The suite should provide the productivity normally associated with a mature
standard framework without requiring classes, inheritance, reflection-based
controllers, ambient authority, or server-side page generation.

## Non-goals

Version 1 does not attempt to provide:

- source, binary, wire, or behavioral compatibility with another framework;
- every protocol or data format in common use;
- an ambient global prelude or wildcard-imported utility namespace;
- unrestricted native paths, sockets, environment variables, trust stores, or
  private-key bytes;
- reflection-based dependency injection, controller discovery, or object-
  relational mapping;
- server-side HTML page frameworks;
- automatic replay of an uncertain mutation;
- a custom cryptographic protocol or weakened substitute for a standard one;
- unbounded collections, bodies, diagnostics, queues, retries, recursion, or
  retained application state; or
- a compatibility promise for obsolete experimental library APIs.

## Library roles

The suite retains four cross-cutting ownership roles:

| Role | Responsibility | Authority rule |
| --- | --- | --- |
| `Foundation/` | Values, memory accounting, collections, text, bytes, formatting, bounded algorithms, local resources, and hosted task contracts required across domains. | Capability-free except the separately declared Hosted task operations and System unsafe boundary. |
| Focused domain libraries | Portable policy and data models such as JSON, URI, HTTP framing, certificates, algorithms, packages, and WVDB clients. | Capability-free unless the module name and contract explicitly describe a bound operation. |
| `Platform/` | Application-facing operations over filesystem, storage, streams, clocks, entropy, networking, key stores, processes, diagnostics, or UI providers. | Every required capability, bound instance, generation, limit, and failure is explicit. |
| `System/` | Reusable privileged kernel, driver, machine, DMA, or FFI contracts. | Available only to an admitted System profile and never implied by an ordinary hosted dependency. |

`Protocol/` remains the owner for a reusable bounded service or provider wire
format. A focused domain directory such as `Database/`, `Network/`, `Data/`, or
`Security/` may exist when it has an implemented owner; directory placement is
not itself an authority grant.

## Suite profiles

Windvale Libraries 1.0 is one suite with selectable profiles rather than one
monolithic binary:

| Profile | Required purpose | Representative families |
| --- | --- | --- |
| Foundation 1.0 | Portable language-level application nucleus. | Option, Result, numeric, ordering, memory, collections, bytes, text, resources. |
| Data 1.0 | Deterministic data parsing and transformation. | JSON, CBOR, optional CDDL, URI, validation, encodings, algorithms, digests, compression profiles. |
| Backend 1.0 | Hosted data services and web-application backends. | Operation context, filesystem, blob/storage, network streams, TLS, HTTP client/service/router, configuration, diagnostics, testing, WVDB integration. |
| System 1.0 | Explicit privileged or foreign boundaries. | Unsafe memory witnesses, ABI values, devices, drivers, and kernel-facing interfaces. |

Foundation is the base dependency. Data depends on Foundation. Backend depends
on selected Foundation and Data modules, not on every optional module. System
may reuse lower profiles but cannot make an unsafe or privileged API visible to
Core or Hosted code.

Suite profiles and execution classes are different dimensions. Foundation,
Data, Backend, and System describe the library product selected by a package;
Core, Hosted, and System describe where a module may execute and what effects it
may require. A Core codec may belong to the optional Data product profile, and
a Hosted adapter may belong to Backend. Neither label grants a capability.

## Proposed module families

The initial suite catalog is:

| Family | Execution class and status | Version 1 direction |
| --- | --- | --- |
| `Foundationˉoption` | Core | Required; exact identity recognized by language operations that need optional presence. |
| `Foundationˉresult` | Core | Required; exact identity recognized by typed failure propagation. |
| `Foundationˉnumeric` | Core | Required checked conversions, parsing, decimal support, and strict floating operations. |
| `Foundationˉordering` | Core | Required equality and deterministic total ordering protocols. |
| `Foundationˉmemory` | Core | Required budgets, allocation leases, limits, and capacity failures. |
| `Foundationˉcollections` | Core | Required bounded arrays, vectors, sequences, slices, maps, sets, iterators, and arenas. |
| `Foundationˉbytes` | Core | Required immutable bytes, byte ranges, endian codecs, and bounded builders. |
| `Foundationˉtext` | Core | Required strict UTF-8 text, Unicode scalar iteration, formatting, and bounded builders. |
| `Foundationˉresource` | Core | Required local-release and completion-outcome contracts. |
| `Foundationˉtask` | Hosted | Required for the complete Backend profile; structured tasks, deadlines, cancellation, join, and teardown. |
| `Foundationˉtime` | Core | Proposed pure date, time-of-day, UTC instant, duration, and checked calendar operations. It does not read a clock. |
| `Foundationˉidentity` | Core | Proposed `Id128`, parsing, display, ordering, and byte encoding. It does not generate random identities. |
| `Dataˉjson` | Core | Strict bounded JSON document model, parser, deterministic encoder, optional [RFC 8785 JCS](https://www.rfc-editor.org/rfc/rfc8785.html) profile, and typed observations. |
| `Dataˉcbor` | Core | RFC 8949 bounded binary documents, validate-once views, writer, and Windvale deterministic profile. |
| `Dataˉcddl` | Core optional tooling | Optional RFC 8610 schema compilation and validation for CBOR and JSON; no runtime reflection requirement. |
| `Dataˉcborˉsequence` | Core optional | RFC 8742 incremental sequences of independently admitted CBOR items. |
| `Dataˉcborˉartifact` | Core optional | Deterministic validation receipts, observed-limit summaries, and bounded structural indexes for authenticated immutable CBOR artifacts. |
| `Dataˉuri` | Core | Strict URI authority, origin target, path segment, and query parsing without network authority. |
| `Dataˉvalidation` | Core | Bounded field and object validation with deterministic failure order. |
| `Dataˉencoding` | Core | Hex, Base64, and Base64url with exact alphabets and limits. |
| `Algorithmsˉsequence` | Core | Stable sorting, selection, binary search, bounds, and deterministic comparison accounting. |
| `Algorithmsˉgraph` | Core | Optional bounded traversal, topological ordering, components, and shortest-path profiles. |
| `Securityˉdigest` | Core | SHA-256 and explicitly selected digest/checksum profiles with ordinary and streaming APIs. |
| `Securityˉcose` | Core and Hosted optional | Bounded RFC 9052 COSE Sign1 structures and streaming signing/verification composition; no custom cryptography. |
| `Dataˉcompression` | Core | Optional bounded compression/decompression profiles; every decoder requires an output maximum. |
| `Filesystemˉpath` | Core | Semantic relative paths and segments without native-path authority. |
| `Platformˉfile`, `Platformˉdirectory`, `Platformˉpublication` | Hosted | Rights-limited file and directory instances, positioned I/O, explicit durability, and atomic publication profiles. |
| `Platformˉrandomˉaccessˉstorage` | Hosted | Pre-bound mutable byte storage with explicit generations and mutation outcomes. |
| `Storageˉblob` | Hosted | Bounded semantic object storage over a bound namespace. |
| `Networkˉaddress` | Core | Addresses, prefixes, endpoints, service names, and authority reduction. |
| `Platformˉnetworkˉstream`, `Platformˉnetworkˉdatagram` | Hosted | Resolver, reliable stream, datagram, and listener operations over bound grants. |
| `Platformˉclock`, `Platformˉentropy` | Hosted | Explicit monotonic/civil time, timers, secure entropy, and structurally separate deterministic test entropy. |
| `Securityˉcertificate` | Core | Bounded public certificate parsing and evidence; no private-key custody. |
| `Platformˉtrust` | Hosted | Immutable trust snapshots and verification policy. |
| `Platformˉartifactˉtrust` | Hosted optional | Immutable artifact acquisition, COSE or signed-package authentication, authorized validation attestations, and opaque prevalidated evidence. |
| `Platformˉkeyˉstore` | Hosted | Non-exportable private-key operations, rotation, revocation, and destruction. |
| `Networkˉtls` | Hosted | Secure client and service streams with typed peer evidence. |
| `Networkˉhttp` | Core and Hosted | Portable HTTP values/framing plus bound clients, services, routing, bodies, and backend policies. |
| `Backendˉidentity` | Hosted | Authentication and authorization evidence over explicitly bound providers. |
| `Backendˉconfiguration` | Core and Hosted | Typed configuration from explicit sources; secrets remain protected provider values. |
| `Backendˉcache` | Core and Hosted | Bounded cache policy over explicit memory, WVDB, or provider-backed stores. |
| `Diagnosticsˉlog` | Hosted | Structured bounded events over an explicit diagnostic sink. |
| `Diagnosticsˉmetrics` | Hosted | Bounded counters, gauges, histograms, and export snapshots. |
| `Testingˉbackend` | Hosted test profile | Virtual clock, deterministic entropy, in-process HTTP, fake trust/key providers, and fault injection that cannot satisfy production capabilities. |
| `Databaseˉwvdb` | Core and Hosted | Typed application facade over WVDB 1.0; WVDB remains the semantic owner. |

This table is a proposed 1.0 catalog, not an assertion that every family must be
implemented before any useful application ships. Conformance is declared by
named profile and exact module set.

## Explicit imports and call-site naming

There is no ambient prelude. Applications import exact module identities and
choose concise local aliases:

~~~text
import Networkˉhttpˉservice as Http;
import Networkˉhttpˉrouter as Router;
import Dataˉjson as Json;

let Routes = Router.Construct(Routeˉlimits);
let Service = Http.Create(Serviceˉbinding, Routes, Httpˉlimits);
let Outcome = await Http.Run(Service, Context);
~~~

The module identity supplies the broad domain. Exported operation names should
therefore prefer a clear type or verb such as `Parse`, `Readˉat`, `Mapˉget`, or
`Finishˉdurable` rather than repeat a long global namespace. Existing exported
names are migrated only through an explicit refactoring decision and fixture
update; they are not silently aliased forever.

## Common contract rules

### Bounds

Every operation that can allocate, retain, iterate, recurse, queue, parse,
format, decompress, wait, transfer, retry, or emit diagnostics states a maximum
or consumes a previously admitted bounded owner. Rejection occurs before
expensive work or mutation whenever the contract permits it.

Convenience constructors may supply a named conservative profile, but the
selected values remain observable. There is no process-wide hidden unlimited
default.

### Failures

Recoverable library failures use `Result<T, E>` or an operation-specific
variant. They do not throw a general catchable exception. Provider defects,
violated proven preconditions, unsafe ABI corruption, and exhausted terminal
runtime invariants retain their separately specified containment behavior.

Failure precedence is deterministic. Parsers report the first defined failure
position. Validation may collect several failures only under an explicit count
and diagnostic-byte maximum.

### Mutation progress

Mutating operations distinguish:

- rejection with known zero progress;
- accepted exact partial progress;
- completed exact progress; and
- indeterminate completion after dispatch.

An indeterminate mutation is never automatically replayed. HTTP helpers,
filesystem publication, WVDB transactions, cache writers, and key operations
must preserve this rule even when a higher-level call appears idempotent.

### Ownership and release

Handles, streams, request bodies, transactions, builders, key-operation leases,
and provider sessions are ordinary owned or borrowed values. Local release is
deterministic and does not invent remote completion, durability, rollback, or
peer receipt.

### Authority

A library dependency states a requirement; it does not grant authority. The
application approves the exact transitive capability set, and a launcher or
service manager binds rights-limited instances separately. Source code cannot
discover a broader filesystem root, listener, network origin, trust store,
private key, environment, clock, entropy source, database, or diagnostic sink.

### Determinism

Capability-free modules produce identical results for identical admitted
inputs, module versions, and options. Maps and sets have canonical iteration
order. Encoders define exact bytes when they claim canonical output. Locale,
civil time, entropy, provider scheduling, and host diagnostics never enter a
portable result implicitly.

## Versioning and compatibility

Each public module has:

- one canonical module identity;
- one major contract version;
- one exact public signature-set identity;
- a minimum source/profile requirement;
- declared required and optional capabilities;
- deterministic limits and failure behavior; and
- an implementation and target-support matrix.

Foundation modules used directly by language typing or syntax advance as one
coordinated Foundation major contract. Other library families may version
independently. A package lock identifies exact selected parts and signature
sets. Minor evolution may add separately named operations or compatible data
only where the module contract permits it; it cannot weaken validation, widen
authority, change canonical bytes, or reinterpret an existing outcome.

Windvale 1.0 does not promise preservation of pre-1.0 experimental library
surfaces. Early implementations may be moved, split, renamed, or replaced when
the new owner is clearer and all maintained consumers, fixtures, and documents
move together.

## Conformance claims

An implementation may claim only the profiles and module sets it actually
qualifies. Suggested claim forms are:

- **Foundation 1.0 conforming** — every required Foundation module and its
  ordinary reference behavior passes the exact suite;
- **Data 1.0 JSON profile** — the named Data modules and limits pass without
  implying compression or graph algorithms;
- **Data 1.0 CBOR profile** — RFC 8949 admission plus the named Windvale
  document and deterministic profiles pass without implying artifact trust;
- **Backend 1.0 prevalidated-artifact profile** — the exact CBOR, receipt, COSE,
  immutable-provider, and trust-policy set passes, including stale and tampered
  evidence rejection;
- **Backend 1.0 HTTP service profile** — the exact hosted operation, stream,
  TLS, HTTP service, routing, diagnostics, and test-provider set passes; or
- **Windvale Libraries 1.0 complete profile** — every module marked required
  for the complete suite passes on the claimed targets.

Source parsing, WVB verification, interpreter execution, native execution,
Windows provider execution, Linux provider execution, browser execution, and
Windvale OS execution are separate evidence rows. Passing one is not reported
as all of them.

## First production-shaped qualification workload

The recommended first Backend 1.0 vertical slice is a bounded HTTPS JSON and
CBOR CRUD service over WVDB:

1. the launcher binds one TLS service, one trust/key identity, one WVDB
   database, one diagnostic sink, and exact limits;
2. the router accepts `GET`, `POST`, `PATCH`, and `DELETE` for one resource;
3. the request owner parses route/query data and a bounded JSON or CBOR body;
4. the handler validates a typed command and executes one explicit WVDB
   transaction;
5. the response owner emits deterministic JSON, deterministic CBOR, or a bounded
   problem response according to explicit content negotiation;
6. structured logs omit credentials, private data, and raw provider errors;
7. deterministic in-process tests cover all routes, limits, cancellation,
   provider restart, malformed JSON and CBOR, equivalence of the schema-approved
   typed command across both formats, authorization denial, transaction
   conflicts, and indeterminate output; and
8. isolated TLS tests exercise the same public service contract on Windows and
   Linux.

This workload is useful to browser, mobile, and native clients without adding
server-side pages or a broad web application framework.

## Review boundary

The umbrella organization, module names, and exact Backend signatures remain
under review until a named decision accepts them. The review should resolve the
items recorded in the [delivery plan](../Documents/Project/Windvale-Libraries-1.0-Plan.md),
then assign conflict-free decision numbers immediately before publication.
