# Windvale Libraries 1.0 plan

## Status

- Status: Draft for owner review
- Product target: [Windvale 1.0](Windvale-1.0-Product-Plan.md)
- Umbrella specification: [Windvale Libraries 1.0](../../Specifications/Windvale-Libraries-1.0.md)
- Backend API catalog:
  [Windvale Backend Libraries 1.0](../../Specifications/Windvale-Backend-Libraries-1.0.md)
- Binary data profile:
  [Windvale binary data profile 1.0](../../Specifications/Windvale-Binary-Data-1.0.md)
- Language dependency: [Windvale Language 1.0](../../Specifications/Windvale-Language-1.0.md)
- Database dependency: [WVDB 1.0 specification plan](WVDB-1.0-Specification-Plan.md)

This plan turns the existing collection of Foundation, database, platform,
network, hosted HTTP, and browser libraries into a deliberate Windvale 1.0
library product. It is not a request to preserve every experimental module or
name. Early refactoring is permitted when it produces one clearer public owner
and moves every maintained consumer, specification, fixture, and verifier
together.

No numbered decision is assigned in this draft. The exact signature catalog is
still under review, and other active work may consume the next repository
decision number before publication.

## Proposed product names

The recommended public names are:

| Name | Meaning |
| --- | --- |
| **Windvale Libraries 1.0** | Complete official reusable library suite. |
| **Windvale Foundation 1.0** | Small portable nucleus aligned with the Language 1.0 Foundation registry. |
| **Windvale Data 1.0** | Portable JSON, CBOR, optional CDDL, URI, validation, encoding, algorithms, digest, and selected compression modules. |
| **Windvale Backend 1.0** | Hosted data-service and web-backend profile over Foundation and Data. |
| **WVDB 1.0** | Database product with its own specifications and conformance. |

“Standard library” remains an ordinary descriptive phrase. The official suite
name is plural because applications select explicit modules rather than receive
one implicit runtime library.

## What exists today

The repository already proves important pieces, but the developer-facing
surface is uneven:

| Area | Existing state | Library 1.0 gap |
| --- | --- | --- |
| Option and Result | Generic implementations exist under `Libraries/Foundation/Values/`. | Align exact public signatures and package identities with the Foundation 1.0 registry. |
| Collections and text | Detailed normative-candidate contracts exist in the Foundation specification. | Implement the ordinary reference modules and qualify generic ownership, bounds, and failures. |
| SHA-256 | Portable ordinary and streaming implementations exist under `Foundation/`. | Move or facade them under the selected `Securityˉdigest` public owner and preserve simple oracles. |
| JSON | Strict JSON admission and protocol code exist under `Libraries/Database/`. | Extract general JSON semantics into `Dataˉjson`; leave database envelopes under WVDB. |
| Binary structured data | No general binary document codec is implemented. | Adopt RFC 8949 CBOR under `Dataˉcbor`, with a deterministic profile, optional CDDL tooling, and optional CBOR Sequences. |
| Authenticated artifacts | The [release envelope](../../Specifications/Windvale-Release-Envelope.md) already verifies signed manifests plus every artifact's SHA-256 and length, while explicitly refusing to treat a signature as safety or authority evidence. | Preserve that release format and oracle; add provider-neutral COSE Sign1 and CBOR validation-receipt profiles without retroactively changing existing releases. |
| Network values | Address, prefix, endpoint, service name, grant, and stream state-machine code exists. | Consolidate overlapping authority models and add final Language 1.0 package identities. |
| Filesystem | Portable semantic validators, a read-only directory facade, paper file-copy contracts, and host/OS provider work exist. | Produce one coherent file/directory/publication API with explicit capability families. |
| Random-access storage | A typed capability facade with exact generation and mutation behavior exists. | Generalize endpoint binding and add the higher-level blob/object profile. |
| TLS and HTTPS | Hosted bootstrap resolver/TCP, TLS 1.3, and bounded HTTPS implementations exist with isolated evidence. | Move public semantics out of bootstrap JavaScript ownership and bind them through Windvale Language 1.0 modules/providers. |
| HTTP service | A detailed Language 1.0 paper handler and service contract exists. | Generalize it into reusable HTTP core, body, router, pipeline, and service modules. |
| Browser framework | Reusable TypeScript components and state/lifecycle code exist under `Libraries/Web/`. | Keep browser UI ownership separate while sharing HTTP/data contracts through explicit browser adapters. |
| WVDB | Extensive portable and hosted storage modules exist, and WVDB 1.0 product specifications are now being defined. | Define the application facade jointly with WVDB without inventing an ORM or second transaction model. |
| Diagnostics and testing | Individual repository tools and deterministic fixtures exist. | Create reusable structured log, metrics, virtual time, fake provider, and fault-plan contracts. |

The immediate problem is therefore organization and public contract selection,
not an empty repository.

## Target ownership

Create source directories only with the first accepted implementation. The
intended ownership, once real code exists, is:

~~~text
Libraries/
  Foundation/       portable nucleus and hosted task contracts
  Algorithms/       sequence and graph algorithms
  Data/             JSON, CBOR, CDDL, URI, validation, encoding, compression
  Security/         public certificates, digests, and COSE signed-object policy
  Network/          address, framing, TLS and HTTP semantic modules
  Database/         WVDB-owned portable policy and application facade
  Backend/          configuration and cache policy
  Diagnostics/      structured log and metrics values/facades
  Testing/          deterministic non-production providers and fault plans
  Platform/         rights-limited host/OS and artifact-trust adapters
  Web/              browser-native TypeScript framework and components
~~~

This is an ownership map, not a request to add empty directories. Package and
module identities remain explicit and are not inferred from filesystem paths.

## Existing-library refactoring map

### Foundation values and algorithms

| Existing owner | Proposed disposition |
| --- | --- |
| `Libraries/Foundation/Values/Option.wv` | Keep as the implementation seed for canonical `Foundationˉoption`; adjust names/signatures only with Language 1.0 compiler and registry evidence. |
| `Libraries/Foundation/Values/Result.wv` | Keep as the implementation seed for canonical `Foundationˉresult`; align `try` identity and failure ownership. |
| `Libraries/Foundation/Operations/Bounded-Operation-Core.wv` | Retain its deterministic state-machine oracle; migrate public hosted types into canonical Foundation operation/task modules. |
| `Foundation/Byte-Construction.wv` and `Foundation/Byte-Ordering.wv` | Migrate capability-free behavior into `Foundationˉbytes`; keep focused reference fixtures. |
| `Foundation/Sha256*.wv` | Move the public algorithm family to `Securityˉdigest`; retain compression and streaming implementations as separately testable internal modules. |

There should be no permanent parallel “Seed Foundation” and “Language 1.0
Foundation” public APIs after migration. The older modules may remain as build
or qualification oracles only while their replacement path is explicit.

### JSON and data

| Existing owner | Proposed disposition |
| --- | --- |
| `Libraries/Database/Json-Value.wv` | Extract strict UTF-8, syntax, depth, value count, duplicate-name, string, and number admission into `Dataˉjson`. |
| `Libraries/Database/Json-Protocol.wv` | Keep the database request/response envelope under WVDB and make it consume the shared JSON module. |
| `Libraries/Package/Canonical-Package-Text.wv` | Keep package canonical text policy under Package; reuse Foundation text/bytes rather than making it a general text library. |

The extraction should not keep two independently evolving JSON parsers. A
simple strict parser remains the oracle even if a faster parser is introduced.

The adjacent binary document owner is `Dataˉcbor`, specified in the
[binary data profile](../../Specifications/Windvale-Binary-Data-1.0.md). It uses
standard CBOR rather than a Windvale-only wire format. Admission validates one
owned buffer and may retain a bounded structural index for repeated views;
encoding, deterministic encoding, CDDL validation, and JSON conversion remain
explicit operations. WVDB may consume the module, but the module does not
select database pages, keys, logs, or backup formats.

### Network and HTTP

| Existing owner | Proposed disposition |
| --- | --- |
| `Libraries/Network/Address-Authority.wv` | Primary portable seed for `Networkˉaddress`. |
| `Libraries/Platform/Networking/Network-Authority.wv` | Reconcile its useful resource-limit checks into the primary model, then remove overlapping public types. |
| `Libraries/Network/Connect-Stream-Core.wv` | Retain as the portable transition oracle beneath `Platformˉnetworkˉstream`. |
| `Runtime/Hosted/Network/` providers | Retain as supervised bootstrap providers and evidence until Windvale-owned native provider leaves replace them. They do not own the final source API. |
| `Runtime/Hosted/Http/Bounded-Http1.mjs` | Retain strict framing tests and behavior as a bootstrap/reference oracle; move portable framing semantics to `Networkˉhttpˉcore`. |
| `Runtime/Hosted/Http/Bounded-Https-Client.mjs` | Retain isolated-provider evidence; replace its JavaScript-facing client class with a Windvale Language 1.0 hosted facade. |
| Language 1.0 HTTP paper workload | Use as the first server API and malformed-input corpus, then generalize only the parts needed by at least two real consumers. |

The final service API should not expose Node, Windows, Linux, TLS-library,
socket, or native certificate objects.

### Filesystem and storage

| Existing owner | Proposed disposition |
| --- | --- |
| `Libraries/Platform/Filesystem/Filesystem-Semantics.wv` | Preserve its operation/status/completion validators as the semantic oracle for the new file facade. |
| `Libraries/Platform/Filesystem/Read-Only-Directory.wv` | Keep the rights-limited instance pattern; migrate from the narrow 3 KiB bootstrap envelope to the accepted directory profile. |
| Language 1.0 bounded file-copy contract | Adopt its snapshot, exclusive creation, positioned I/O, durable finish, generation, and local-release semantics. |
| `Libraries/Platform/Storage/Random-Access-Storage.wv` | Preserve exact generation, borrowed response, partial/indeterminate progress, and flush classes; add an explicit endpoint parameter when Language 1.0 instance-bearing capabilities land. |
| `Libraries/Platform/Database/` | Keep database-specific page/tree/transaction compositions under WVDB rather than treating them as general storage APIs. |

The filesystem should not become one broad ambient `System.IO` equivalent.
File read, file creation, mutation, directory management, and atomic publication
have different authority and failure contracts.

### Browser libraries

`Libraries/Web/` remains browser-native code. It may consume generated or hand-
written adapters for shared JSON, CBOR, HTTP, identity, and WVDB service
protocols, but it does not make TypeScript the semantic owner of Windvale
Backend 1.0.
Browser credentials, service URLs, and cached state remain untrusted scoped
inputs. Native paths, database files, trust-store administration, and private
keys must not enter browser bundles.

## Recommended decisions and alternatives

### 1. One suite or unrelated libraries

**Recommendation:** one Windvale Libraries 1.0 suite with Foundation, Data,
Backend, and System profiles.

| Choice | Advantages | Costs |
| --- | --- | --- |
| One monolithic standard library | Simple marketing and installation. | Couples unrelated capabilities, increases every application, and encourages ambient APIs. |
| Unrelated independently named libraries | Maximum implementation independence. | No coherent developer expectation, conformance vocabulary, or common safety rules. |
| One suite with explicit profiles | Coherent product and rules while applications select focused parts. | Requires an exact catalog, profile matrix, and package metadata. |

### 2. Small Foundation or broad mandatory framework

**Recommendation:** small mandatory Foundation; Data and Backend are official
selectable profiles.

A broad mandatory framework offers immediate name availability but makes the
language runtime, HTTP, filesystem, database, and security evolve in lockstep.
A small Foundation keeps portable applications and the OS usable while the
official optional profiles still provide a batteries-included development
experience.

### 3. Export naming

**Recommendation:** concise module-qualified exports such as `Json.Parse`,
`Http.Send`, and `File.Readˉat`.

Keeping existing fully prefixed exports minimizes early edits but produces call
sites such as `Json.Databaseˉjsonˉprotocolˉrequestˉdecode`. Concise exports are
easier to learn and document because imports already establish the namespace.
The cost is one deliberate early refactor of source, fixtures, specs, and
consumers.

### 4. Compatibility with current experimental APIs

**Recommendation:** clean migration without permanent aliases.

Compatibility shims reduce immediate churn but create two names and sometimes
two failure models for the same operation. Windvale is still early enough to
move maintained consumers together. A named release may later create a real
compatibility promise; this draft should not invent one retroactively.

### 5. HTTP protocol baseline

**Recommendation:** strict HTTP/1.1 client and service semantics for the first
complete Backend profile; design semantic request/response values so HTTP/2 and
HTTP/3 can be later transports.

HTTP/2 improves multiplexing and is important for high-load services, but it
adds stream-level flow control, header compression, concurrent cancellation,
connection-wide failure, and substantially more malformed-input work. HTTP/1.1
is sufficient for the first JSON/WVDB service and supplies a simpler oracle.

### 6. TLS implementation

**Recommendation:** use constrained qualified Windows/Linux providers first,
with a portable semantic contract and isolated deterministic fixtures. Do not
write new production cryptography merely to make the library Windvale-owned.

A portable TLS implementation increases Windvale independence but should come
only after entropy, key custody, certificate parsing, trust, civil time,
constant-time primitives, algorithm profiles, and side-channel testing exist.
The provider-first approach gives useful HTTPS sooner without making provider
objects part of the public API.

### 7. Certificate and trust profile

**Recommendation:** support two explicit policies in stages:

1. pinned public-key or directly pinned end-entity certificate identity for
   controlled Windvale services; then
2. public-PKI server validation with exact DNS identity, bounded chain depth,
   immutable trust snapshot, and civil-time evidence.

Pinned identity is smaller and avoids ambient public trust but requires managed
provisioning and rotation. Public PKI works with ordinary websites but requires
broader parsing, time, trust updates, and revocation policy. Neither should be
silently substituted for the other.

### 8. Filesystem facade shape

**Recommendation:** separate file read/create/mutate, directory management, and
atomic publication capabilities while sharing values and error vocabulary.

One large facade is familiar to users of existing frameworks, but a reference
to it tends to grant deletion, traversal, enumeration, replacement, and
metadata operations together. Focused endpoints preserve least authority and
make unsupported durability or atomicity fail clearly.

### 9. JSON ownership

**Recommendation:** one general `Dataˉjson` module; WVDB protocols consume it.

Leaving JSON under Database avoids a move but makes HTTP/configuration depend on
WVDB and encourages independent parsers elsewhere. Extracting it creates one
strict malformed-input and performance owner.

The ordinary deterministic encoder preserves admitted numeric spelling, emits
UTF-8 without insignificant whitespace, sorts object members by decoded UTF-8
name bytes, and uses one exact string-escape spelling. It must not be called
semantically canonical. Applications requiring standard hashable JSON may
explicitly select RFC 8785 JCS and accept its I-JSON and IEEE 754 binary64
restrictions. Deterministic CBOR remains the preferred signed artifact
representation for Windvale-owned binary data.

### 10. Binary structured data

**Recommendation:** adopt RFC 8949 CBOR as `Dataˉcbor`, use the narrowed
Windvale document profile for ordinary applications, and require the Windvale
deterministic profile wherever bytes are hashed, signed, compared, or retained
as reproducible artifacts. Add RFC 8742 CBOR Sequences and RFC 8610 CDDL as
optional companion modules.

CBOR is self-describing and keeps the JSON-shaped use case while directly
representing bytes and integers. It reduces delimiter, escape, decimal-number,
and Base64 work and permits validate-once views over an immutable buffer. It
still requires bounded validation and application-schema checks. MessagePack is
a reasonable interoperability codec but has a smaller standards family;
Protocol Buffers, FlatBuffers, and Cap'n Proto require schemas and solve the
separate typed-layout problem. BSON and Ion carry database-oriented or richer
data-model machinery that the first general codec does not need.

### 11. Authenticated and prevalidated artifacts

**Recommendation:** never mark a path or mutable file “trusted.” Authenticate an
immutable byte snapshot with an existing signed-package identity or bounded
COSE Sign1 profile, then accept a prevalidated fast path only when an authorized
validator attestation binds the exact content digest, CBOR profile, schema,
observed resource use, validator contract, and optional index.

A publisher signature proves byte selection, not structural validity, schema,
or authorization. The first uncached signed load is normally slower because it
adds hashing and signature verification; hashing and admission should share one
sequential read. Repeated loads can be faster when an immutable provider reuses
valid receipt/index evidence. Cache identity includes content, schema, profile,
validator, trust generation, and index—not a path or modification time.

Use RFC 9052 COSE Sign1 with EdDSA/Ed25519 for new general signed-object APIs,
but keep the signature input small. Large content is authenticated by signing a
deterministic manifest or validation receipt containing its SHA-256 and exact
length, then streaming the content hash. Do not replace or rename the already
specified Windvale release-envelope signature format merely for uniformity.
Distinct formats may share digest, immutable-snapshot, signer-authorization,
and validation-evidence abstractions without claiming wire compatibility or
reusing key identities automatically.

### 12. Compression

**Recommendation:** gzip and zlib-wrapped deflate as optional Data profiles;
never transparent by default in HTTP.

Compression is common and valuable, but decompression magnifies untrusted input
and complicates length, CPU, memory, and digest accounting. Mandatory output and
work limits keep it reusable. Brotli and Zstandard should follow measured
consumer need.

### 13. Browser-backend helpers

**Recommendation:** include CORS, strict cookie parsing/encoding, and immutable
static-resource serving in the first useful Backend library. Keep server-sent
events and WebSocket optional until their lifecycle suites pass.

This supports conventional browser applications without making long-lived
connection complexity part of the initial qualification gate.

### 14. WVDB ergonomics

**Recommendation:** typed explicit transactions, commands, and query cursors;
no reflection-based ORM in 1.0.

An ORM can reduce repetitive application code but would prematurely freeze
record mapping, identity generation, lazy loading, change tracking, migrations,
and query translation. Windvale can later supply typed code generation over a
frozen WVDB schema contract without adding runtime reflection or hidden writes.

### 15. Versioning

**Recommendation:** one coordinated Foundation major contract; independently
versioned Data, Backend, Security, Network, and WVDB modules selected by an
exact package lock.

Lockstep versioning simplifies a release number but forces unrelated changes to
advance together. Fully independent versioning makes compiler-recognized Option
and Result identities difficult to reason about. The hybrid keeps the language
nucleus coherent and domain evolution focused.

## Delivery phases

The phases are ordered by dependency and production usefulness. A phase may
deliver several small slices; it does not authorize empty scaffolding or one
large merge.

### Phase 1: Foundation implementation closure

Implement and qualify the required Language 1.0 Foundation modules:

- Option and Result exact identities;
- numeric conversions, Decimal128, ordering, and parsing;
- memory budgets and ownership-return failures;
- vectors, sequences, slices, maps, sets, iterators, and arenas;
- bytes, UTF-8 text, builders, formatting, and local resource release; and
- hosted operation context and structured tasks required by Backend.

Exit gate: ordinary reference implementations and every selected intrinsic pass
the Foundation registry, malformed/boundary cases, deterministic artifacts, and
claimed target matrix.

### Phase 2: Data profile

Implement:

- general strict JSON by extracting the WVDB parser oracle;
- RFC 8949 CBOR admission, views, writer, and deterministic profile;
- optional RFC 8610 CDDL validation and RFC 8742 CBOR Sequences;
- deterministic CBOR validation receipts and a bounded structural-index format,
  without enabling the prevalidated fast path before artifact trust exists;
- URI origin/target/query parsing;
- validation reports;
- hex, Base64, and Base64url;
- stable sorting, binary search, and bounds;
- the SHA-256 digest facade; and
- optional gzip/zlib compression after resource limits are measured.

Exit gate: the package parser, HTTP paper workload, and WVDB protocols use the
same JSON/CBOR modules; JSON and CBOR fixtures produce the same schema-approved
typed values where the selected application schema supports both formats; and
no second general parser remains.

### Phase 3: Filesystem and storage profile

Implement:

- semantic path segments and relative paths;
- snapshot read, exclusive creation, positioned mutation, and durable finish;
- bounded directory enumeration and identity-gated management;
- atomic single-file publication;
- instance-bearing random-access storage; and
- a blob interface with one file-backed and one in-memory test provider.

Exit gate: a bounded file-copy application, WVDB storage consumer, package
publication workload, cancellation suite, restart suite, and indeterminate-
mutation suite use the shared interfaces on Windows and Linux.

### Phase 4: Network, time, and secure-provider profile

Implement:

- monotonic clock, timer, civil-time evidence, and secure entropy providers;
- the consolidated address/authority model;
- resolver/connect, reliable streams, service accept, and datagrams;
- public certificate parsing and immutable trust snapshots;
- bounded COSE Sign1 EdDSA/Ed25519 parsing and exact small signature-input
  construction;
- immutable artifact authentication, authorized validation attestations, and
  stale/tampered receipt and index rejection;
- non-exportable key operations; and
- TLS 1.3 client/service streams with typed peer evidence.

Exit gate: deterministic fixtures plus isolated Windows/Linux client and service
peers cover limits, certificate failures, trust generations, cancellation,
provider restart, partial writes, and teardown. Production listener claims
remain disabled without qualified key custody.

### Phase 5: HTTP Backend profile

Implement:

- strict portable HTTP/1.1 request/response framing;
- bounded request and response bodies;
- origin-bound HTTPS client;
- route construction and matching;
- typed request-limit, identity, authorization, CORS, logging, metrics, and
  error-mapping filters;
- HTTPS service lifecycle, stop-accepting, and drain;
- JSON and CBOR request/response helpers with bounded content negotiation;
- immutable resource/blob serving; and
- deterministic in-process HTTP test provider.

Exit gate: the first production-shaped JSON/CBOR CRUD service over WVDB passes
all semantic, schema-approved typed-command equivalence, malformed-input,
denial, cancellation, restart, and isolated TLS cases.

### Phase 6: Application integration profile

Implement only after the vertical service exposes actual reuse pressure:

- typed configuration acquisition and layering;
- bounded cache providers;
- reusable authentication profiles;
- WVDB typed facade and schema-generated adapters;
- server-sent events or WebSocket if a selected application requires them;
- additional compression or schema-bound serialization profiles; and
- service packaging, installation, recovery, and upgrade integration.

## First vertical service

The recommended reference application is an inventory or knowledge-item
service used by a browser application. It should be deliberately ordinary:

| Route | Behavior |
| --- | --- |
| `GET /v1/items/{id}` | Read one typed WVDB record and return JSON, CBOR, or not found. |
| `GET /v1/items?after=...&limit=...` | Execute one bounded ordered query and return a page plus continuation evidence. |
| `POST /v1/items` | Validate a bounded JSON or CBOR command, create an explicit primary identity, insert in one transaction, and return created evidence. |
| `PATCH /v1/items/{id}` | Apply an explicit change set with expected record version. |
| `DELETE /v1/items/{id}` | Delete with expected version and distinguish absent/conflict/commit outcomes. |
| `GET /health` | Report bounded process/provider readiness without revealing secrets or internal topology. |

The service uses HTTPS, CORS for an exact browser origin, typed identity and
authorization, structured logs, metrics, configuration, and deterministic test
providers. It serves no server-side pages. A browser app may be hosted
separately or consume immutable static resources from a bound resource store.

## Quantitative limits to freeze with the workload

The first workload must select measured values for:

- maximum connections, concurrent requests, queued operations, and shutdown
  time;
- request-line, header, header-count, body, response, and total wire bytes;
- route count, route segments, route parameter bytes, and query pairs;
- JSON depth, values, members, decoded text, and output bytes;
- CBOR input, depth, items, map pairs, text/byte strings, tags, structural index,
  validation work, sequence items, and output bytes;
- signed-artifact bytes, receipt/index bytes, signature/hash work, authenticated
  cache entries/bytes/lifetime, trust generations, and evidence teardown;
- WVDB transaction operations, query results, cursor batch, and retained bytes;
- log fields, field bytes, event bytes, event rate, metric label cardinality,
  and retained series;
- certificate bytes, chain depth, service identities, trust anchors, and
  handshake bytes/work;
- file/blob transfer chunk, object length, operations, and publication time;
- task children, completion records, cancellation events, and teardown time;
  and
- test fault-plan entries, virtual-time events, and diagnostic saturation.

The specification should not guess large production maxima without measurement.
Named conservative defaults and hard format/provider ceilings remain distinct.

## Verification model

Every library slice needs:

1. ordinary valid and boundary behavior;
2. invalid, truncated, oversized, inconsistent, and malicious input where it
   parses or decodes untrusted data;
3. exact ownership on success and failure;
4. deterministic failure precedence and output bytes;
5. resource/time/work accounting at its admitted limits;
6. provider denial, revocation, stale generation, peer loss, restart,
   cancellation, and teardown where applicable;
7. exact mutation progress including indeterminate outcomes;
8. one simple reference oracle for optimized code;
9. differential results showing ordinary admission and an accepted prevalidated
   path expose the same document and reject altered/stale evidence;
10. source/WVB/interpreter/native/host target rows rather than one blanket claim;
   and
11. Windows and Linux evidence before a cross-host hosted claim.

Performance evidence records workload, input size, host/profile, elapsed time,
and peak or working-set memory when practical. Stable workloads should gain
enforceable regression thresholds only after normal variance is measured.

## Documentation set

This initial set intentionally contains:

- one umbrella suite contract;
- one detailed Backend API catalog;
- one focused binary data profile; and
- this product, migration, decision, and delivery plan.

After owner review, split accepted families into focused normative
specifications and one exact signature registry. Suggested future owners are:

- `Specifications/Windvale-Data-1.0.md`;
- `Specifications/Windvale-Filesystem-1.0.md`;
- `Specifications/Windvale-Network-1.0.md`;
- `Specifications/Windvale-Security-And-Trust-1.0.md`;
- `Specifications/Windvale-Http-1.0.md`;
- `Specifications/Windvale-Backend-Diagnostics-1.0.md`;
- `Specifications/Windvale-WVDB-Client-1.0.md`; and
- `Specifications/Windvale-Libraries-1.0-Registry.md`.

Do not create those files merely to reproduce headings from the catalog. Create
each when its exact types, functions, limits, failures, and test ownership are
ready for focused review.

## Review checklist

Before assigning decision numbers or calling the surface accepted, review:

- whether the suite/profile names are clear to a public reader;
- every proposed module and capability identity for overlap;
- every operation name for module-qualified readability;
- every hidden authority, allocation, retry, clock, locale, encoding, or
  provider assumption;
- mutation progress and ownership on every path;
- whether each convenience operation can be implemented without changing the
  underlying guarantee;
- the initial TLS/certificate/trust algorithms and policy profiles;
- COSE protected-header and domain policy, publisher/validator role separation,
  immutable-content evidence, receipt/index format, cache invalidation, and
  first-load cost;
- HTTP method, framing, redirect, cookie, CORS, compression, and shutdown scope;
- filesystem identity, replacement, deletion, durability, and atomicity scope;
- the boundary between general blob storage and WVDB large objects;
- the boundary between WVDB typed access and an ORM;
- test-provider structural separation from production capabilities;
- existing consumers that must migrate or should be retired; and
- the smallest module set that makes the vertical service genuinely useful.

Once these are resolved, record the high-level suite decision, focused security
and protocol decisions, exact signature identities, reconsideration triggers,
and the implementation order. Publication should continue to distinguish the
accepted contract from implemented and qualified subsets.
