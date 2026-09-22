# Compiler, tools, and Libraries 1.0 completion matrix

> Status: Current delivery inventory; candidate API choices remain unaccepted
> Authority: Informative; linked contracts and decisions own requirements
> Last reviewed: 2026-09-22

This is the finite tracking surface for the compiler, toolchain, and required
Libraries 1.0 goal. The frozen Language 1.0 compiler and selected immutable
Option/Result consumer are delivered checkpoints, not proof that the complete
library suite or installed product is finished. No complete library profile is
marked qualified here.

The [completion plan](Compiler-Tools-And-Libraries-Completion-Plan.md) owns work
order. The [product plan](Windvale-1.0-Product-Plan.md) owns required host-product
outcomes, and the [library plan](Windvale-Libraries-1.0-Plan.md) owns proposed
library organization. This inventory does not silently accept that proposal.

## How a row closes

For each row, record these six independent facts before marking it complete:

1. **Contract:** accepted exact public declarations, ownership/effects, limits,
   failure order, platform scope, and required/optional capabilities.
2. **Implementation:** one maintained source owner for every declaration and
   finite signature-family expansion; identify compiler-supplied operations.
3. **Package:** canonical module/major/signature identity, distributable package,
   dependency lock, capability closure, and chosen implementation identity.
4. **Consumer:** a maintained application or tool exercising normal build,
   publication, execution, resource failure, and teardown through that API.
5. **Targets:** distinct source, WVB verification, interpreter, native lowering,
   Windows provider, and Debian provider results where the contract requires
   them. A Windows-constructed Linux executable is not independent Linux build
   evidence. Browser and Windvale OS support are not implied.
6. **Evidence:** exact inputs/tools, deterministic bytes where promised,
   hostile-input and ownership/authority failures, bounded time/memory,
   clean-install integration, and required qualification of the chosen state.

An existing test owner is a place to extend coverage, not a certificate for the
whole row. Missing evidence stays open even when code exists. Immutable earlier
qualification remains valid for its recorded identities only. Implementation
checkpoints must not be written into frozen source-design files merely to
refresh their status; current plans and unfrozen execution specifications own
that reporting.

All required host rows target Windows and Debian/Linux. Core rows must retain
portable semantics; Hosted rows need separately bound rights-limited providers.
System support is limited to the accepted toolchain/Foreign boundary, not an
undeclared requirement to finish the OS.

## A. Accepted Foundation declaration inventory

The [Foundation registry](../../Specifications/Windvale-Language-1.0-Foundation-Registry.md)
contains exactly twelve major-1 module blocks. Every declaration and each
finite `family` expansion in those blocks is in scope for its selected profile;
the operation descriptions below are navigation, not substitute signatures.
[The source freeze](../Decisions/0767-Freeze-Windvale-Language-1.0-Source.md)
and its [current amendment](../Decisions/0942-Advance-The-Frozen-Source-Identity-For-Foreign-And-Payload-Borrowing.md)
govern acceptance despite historical candidate wording inside frozen files.
Canonical module/signature identities already exist; a complete selected 1.0
distribution lock and implementation/target mapping remain to be reconciled.

| ID / major-1 module | Required API boundary | Current source/evidence boundary | Consumer and remaining closure |
| --- | --- | --- | --- |
| F01 `Foundationˉoption` | Presence, immutable/exclusive borrow, Take, consuming Map. | [Option source](../../Libraries/Foundation/Values/Option.wv); selected immutable borrowing has [paired evidence](../Evidence/2026-09-22-Borrowed-Vector-Payloads.json). | Package parser delivered; add maintained owned-resource use, then complete exclusive borrowing, Take, Map and all accepted payload classes. |
| F02 `Foundationˉresult` | Valid/failure observations, both immutable/exclusive projections, both consuming maps. | [Result source](../../Libraries/Foundation/Values/Result.wv); same selected borrow evidence, not full payload composition. | Package lock and owned-resource consumer; both branches, callback ownership, refusal and release. No invented Result Take operation. |
| F03 `Foundationˉnumeric` | All registered integer/float conversion and reinterpretation families, strict floating operations and whole-u64 decimal parsing. | No complete canonical module/binding identified; compiler primitive support and Seed decimal parsing are different APIs. | Numeric/parser consumers; every declaration needs implementation and boundary/rounding/overflow evidence. Decimal128 requires the separate contract reconciliation below. |
| F04 `Foundationˉordering` | Equality, deterministic total ordering and comparison protocol. | Existing byte/record comparison primitives are reusable, not a registry-conformance claim. | Package ordering and collection keys; exact equality/order laws, deterministic results and comparison-work bounds. |
| F05 `Foundationˉmemory` | Budgets, allocation limits and ownership-return failures. | [Memory source](../../Libraries/Foundation/Memory/Memory.wv); [owned-payload reclamation](../Evidence/2026-09-15-Owned-Payload-Runtime-Reclamation.json) proves selected paths. | Collection and parser consumers; complete registry mapping, exhaustion, release, stale/consumed budget rejection and measured bounds. |
| F06 `Foundationˉcollections` | Arrays, Vector, Sequence, slices, maps, sets, iterators and arenas. | [Collection source](../../Libraries/Foundation/Collections/Collections.wv) and compiler operations cover subsets. Raw Vector helper observations do not complete collection mutation/access. | Package and database consumers; every operation, borrow invalidation, capacity failures, iterator rules and teardown. |
| F07 `Foundationˉbytes` | Immutable bytes/ranges, buffers, reserved builders, registered endian/decimal appends and freeze. | Seed [byte construction](../../Foundation/Byte-Construction.wv) is reusable, not the registered budget-aware module. | Compiler, package and codecs; boundary/malformed inputs, builder ownership and deterministic output. |
| F08 `Foundationˉtext` | Strict UTF-8 decoding, scalar observations/ranges, builders and formatting. | Existing compiler/runtime text support is not a complete canonical module. | Source diagnostics and data codecs; malformed UTF-8, limits, formatting determinism and allocation failures. |
| F09 `Foundationˉresource` | Local-release protocol and mutation outcomes. | Compiler `using` support and [resource store](../../Libraries/Foundation/Resources/Resource-Store.wv) are distinct; the latter does not establish this whole module. | Resource-owning file/service consumers; release on every exit, exact partial/indeterminate completion and failure behavior. |
| F10 `Foundationˉoperation` | Opaque Hosted operation context and its accepted deadline/cancellation semantics. | [Operation source](../../Libraries/Foundation/Operations/Operation.wv), [bounded core](../../Libraries/Foundation/Operations/Bounded-Operation-Core.wv); bounded profiles exist. | File/network/database providers; exact context generations, cancellation and reusable provider integration. Mutation outcomes belong to F09. |
| F11 `Foundationˉtask` | Structured task/scope family, limits, join, cancellation and teardown. | [Task source](../../Libraries/Foundation/Tasks/Task.wv) and qualified compiler subset; full hosted library delivery remains separate. | Concurrent bounded service; provider loss, resource limits, completion ordering and owner release. |
| F12 `Foundationˉunsafe` | Exact accepted ABI/scratch/region/pointer operations, System only. | [Unsafe source](../../Libraries/Foundation/Unsafe/Unsafe.wv) and [Slice 8 qualification](../Evidence/2026-09-04-Language-1.0-Slice-8-Qualification.json) establish the selected boundary. | Preserve qualified Foreign consumers; nonnull conversion, region length, scratch slice and remaining opaque families need complete mapping/evidence without widening authority or implying OS completion. |

Existing verifier entry points include `language-1-front-door`,
`language-1-memory-budget-accounting`, `language-1-memory-budget-split-execution`,
`language-1-callable-semantics`, `unsafe-wvb`, and `libraries` in the
[owner registry](../../Tests/Native/Verification-Owners.txt). Inspect their exact
selectors and coverage before selecting a row's final gate; do not run their
complete cold plans by default.

## B. Required Data families; public APIs still candidate

The product requires data formats, validation and reusable algorithms. The
[suite catalog](../../Specifications/Windvale-Libraries-1.0.md),
[Backend API catalog](../../Specifications/Windvale-Backend-Libraries-1.0.md),
and [binary-data profile](../../Specifications/Windvale-Binary-Data-1.0.md)
are draft owners of the names below. Exact signatures, limits and package
identities need acceptance before dependent implementation is called 1.0.
Each named catalog section's entire declaration set is the finite candidate
inventory, not authorization to implement its unresolved design choices.

| ID / candidate family | Existing owner or concrete gap | Required consumer / conformance gate |
| --- | --- | --- |
| D01 `Dataˉjson` | Strict admission exists in [Json-Value](../../Libraries/Database/Json-Value.wv); general observations/writer remain open. Extract one shared general owner, retain database envelopes under WVDB; JCS is optional. | Existing WVDB protocol and selected HTTP/configuration users; strict parse, deterministic writer, numeric/duplicate policy and malformed/deep/oversized input. Extend `database-storage --development-target json`; canonical package text keeps its own format. |
| D02 `Dataˉcbor` | No qualified general document codec; deterministic profile, supported tags/numbers/map keys and duplicate policy await acceptance. | Same bounded service commands as JSON; reader/views/writer, malformed/truncated/oversized inputs and schema-approved typed-value equivalence. |
| D03 `Dataˉuri` | Candidate authority/origin/path/query API; do not confuse parsing with network authority. | HTTP routing/client; exact normalization/encoding and hostile authority/path inputs. |
| D04 `Dataˉvalidation` | Candidate bounded validation reports and deterministic failure order. | Typed service commands; finite fields/errors/work and stable diagnostics. |
| D05 `Dataˉencoding` | Candidate hex, Base64 and Base64url facade; inventory reusable leaves first. | Packages and service data; exact alphabets/padding, limits, rejection and round trips. |
| D06 `Algorithmsˉsequence` | Candidate stable sort, selection/search/bounds facade; existing specialized algorithms are not one accepted public API. | Package ordering and database queries; simple oracle, deterministic comparison accounting, bounded memory and worst-case work. |
| D07 `Securityˉdigest` | Reuse ordinary/streaming SHA-256 in `Foundation/`; settle public facade and exact size contract without cloning the implementation. | Packages and storage; golden/differential vectors, streaming length boundaries, deterministic bytes and allocation/work limits. |

## C. Required hosted and security families; public APIs still candidate

Hosted operations in these rows require provider identity, generation/revocation,
denial, teardown, cancellation and failure evidence on each host. Pure path,
address and certificate parsers require Core evidence, not a provider binding.
Mutations distinguish
rejection, exact partial progress, completion and indeterminate completion;
neither retry nor remote receipt is inferred from local acceptance.

| ID / candidate family | Existing owner or concrete gap | Required consumer / conformance gate |
| --- | --- | --- |
| H01 `Filesystemˉpath` | [Filesystem semantics](../../Libraries/Platform/Filesystem/Filesystem-Semantics.wv) supplies reusable checks. | Bounded file copy and package paths; semantic relative paths, bounds, traversal/identity rejection. |
| H02 `Platformˉfile` | [File source](../../Libraries/Platform/Filesystem/File.wv) is the narrow compiler-supplied source-file boundary, not the candidate general open/read/write/durable-finish API. | File copy and publication; snapshot/exclusive-create/positioned mutation/durable finish, local release, uncertain completion and crash boundaries. |
| H03 `Platformˉdirectory` | [Read-only directory](../../Libraries/Platform/Filesystem/Read-Only-Directory.wv) is narrower than the candidate management profile. | Package namespace; bounded enumeration, generations and identity-gated management. |
| H04 `Platformˉpublication` | Existing WVB publication transaction and host adapters are reusable. | Normal package replacement; no partial visibility, exact durability, interruption and recovery. |
| H05 `Platformˉrandomˉaccessˉstorage` | [Typed storage facade](../../Libraries/Platform/Storage/Random-Access-Storage.wv) and native database leaves; generalized endpoint binding remains open. | WVDB storage consumer; writer fencing, partial/indeterminate progress, restart and durability. |
| H06 `Storageˉblob` | Candidate semantic namespace/object API; accepted package identity and complete providers missing. | File-backed and in-memory test providers; bounded content, publication, lookup and failure equivalence. |
| H07 `Networkˉaddress` | [Address/authority](../../Libraries/Network/Address-Authority.wv) and [platform authority](../../Libraries/Platform/Networking/Network-Authority.wv) need one reconciled public model. | Resolver/client/listener; exact scope reduction and capability denial, never ambient socket authority. |
| H08 `Platformˉnetworkˉstream`, `Platformˉnetworkˉdatagram` | [Connect/stream core](../../Libraries/Network/Connect-Stream-Core.wv) and hosted leaves are selected boundaries, not complete resolver/listener/datagram delivery. | Isolated peers; bounded queues, exact local write acceptance, peer exit, cancellation, reconnect without uncertain replay. |
| H09 `Platformˉclock`, `Platformˉentropy` | Candidate bound monotonic/civil clock, timers and secure entropy; deterministic test entropy must stay distinct. | Task/network/service consumers; stale generations, deadlines, refusal and production/test separation. |
| H10 `Securityˉcertificate`, `Platformˉtrust` | Existing hosted TLS work is reusable; public parser/trust snapshot identities and selected certificate profile await acceptance. | Isolated TLS peers; malformed chains, trust generations, hostname/time/policy failures and finite verification work. |
| H11 `Platformˉkeyˉstore` | Non-exportable key custody, rotation/revocation and destruction need selected provider contracts. | TLS service identity; deny unauthorized operations and preserve custody through restart. Production-listener claims wait for this gate. |
| H12 `Networkˉtls` | Bounded hosted TLS implementations have isolated evidence, not complete Language 1.0 API/provider qualification. | Client/service streams on both hosts; typed peer evidence, truncation, cancellation, provider loss and clean shutdown. |

## D. Backend application integration; public APIs still candidate

| ID / candidate family | Required outcome and current gap | Consumer / final evidence |
| --- | --- | --- |
| B01 `Networkˉhttpˉcore`, `Networkˉhttpˉbody` | One bounded portable framing/body owner; exact HTTP baseline, limits and API identity need acceptance. | Malformed/ambiguous framing, partial bodies, deterministic encoding and backpressure. |
| B02 `Networkˉhttpˉclient` | Origin-bound HTTPS client with exact redirect, body, cancellation and uncertain-write behavior. | Isolated host client; no authority expansion or silent mutation replay. |
| B03 `Networkˉhttpˉrouter`, `Networkˉhttpˉpipeline` | Typed routing, request limits, identity/authorization and error mapping. | Bounded item service; deterministic matching and denial before mutation. |
| B04 `Networkˉhttpˉservice` | Bind listener, stop accepting, drain/cancel and release providers. | Same service on both hosts, with limits, peer/provider loss and shutdown. |
| B05 `Networkˉhttpˉcors`, `Networkˉhttpˉcookie`, `Networkˉhttpˉstatic` | Recommended initial browser-backend helpers; exact required subset needs scope acceptance. | Explicit origins/credentials, bounded cookies, immutable resources; no ambient filesystem or implicit cookie jar. |
| B06 `Backendˉidentity` | Required authentication/authorization behavior; reusable profiles must be selected from concrete service needs. | Denied/expired/revoked identity cannot mutate data; no invented authentication scheme. |
| B07 `Backendˉconfiguration` | Typed explicit-source configuration; no ambient secret or environment lookup. | Installed service startup/reload and bounded invalid-input diagnostics. |
| B08 `Diagnosticsˉlog`, `Diagnosticsˉmetrics` | Bounded structured sinks and metric cardinality; exact signatures/providers open. | Service faults and load; omit secrets/private data, preserve bounds and deterministic test observations. |
| B09 `Testingˉbackend` | Reusable virtual clock, fake entropy/trust/key/HTTP providers and fault plans. | Same accepted contracts as production; test identities cannot satisfy production capabilities. |
| B10 `Databaseˉwvdb` | Typed transactions/commands/cursors must follow accepted WVDB owners, not introduce an ORM or second transaction model. | CRUD service, conflicts and cancellation. Durability, full backup/restore and recovery are separately tracked WVDB/product dependencies, not invented facade operations. |
| B11 `Backendˉcache` | Candidate bounded cache API; reusable extraction waits for demonstrated application need and selected policy. | Capacity, eviction, stale generation and backing-store failures; inclusion in the required public surface needs acceptance. |

The proposed integration workload is a bounded HTTPS item service with GET,
POST, PATCH and DELETE, JSON/CBOR content negotiation, explicit WVDB transactions,
authorization, configuration, diagnostics and service drain. Its exact methods,
limits and schema become requirements only through the selected contract.
Paper workloads remain design inputs, not executable consumers.

## E. Compiler, package and installed-product gates

| ID | Existing checkpoint | Remaining required gate / evidence owner |
| --- | --- | --- |
| T01 Frozen compiler baseline | [Slice 8](../Evidence/2026-09-04-Language-1.0-Slice-8-Qualification.json) qualifies its exact source/tool state. | Preserve frozen identities and earlier conformance while integrating library-driven additions; `language-1-front-door` and exact containment/ABI owners. |
| T02 Library-driven execution | [Borrowed Vector payloads](../Evidence/2026-09-22-Borrowed-Vector-Payloads.json) prove selected candidate WVB 1.40 scalar execution. | Source/WIR/WVB, complete verification, interpreter and required native lowering agree for every selected API; native minor-40 and installed promotion are not proved by that checkpoint. |
| T03 Ordinary build/publication | [Source-edition package integration](../Evidence/2026-09-15-Source-Edition-Package-Integration.json) closes the selected package consumer. | Reconcile build/verify/run/inspect/assemble/link/package/publication tools and target/package identities for the complete selected suite. |
| T04 Package lifecycle | [Package plan](Windvale-Package-System-Implementation-Plan.md) and existing package/generation/activation/uninstall owners provide subset evidence. | General dependency/admission and capability binding, clean installation, update, rollback, uninstall/data preservation and offline recovery; no inference from two pinned packages to arbitrary packages. |
| T05 Service lifecycle | Existing provider and database services have bounded subsets. | Start/stop/restart, health, upgrade, recovery and explicit privileged service installation; selected contracts and independent host execution required. |
| T06 Development verification | Focused selectors and exact reusable construction products exist. | Ordinary CI passes for the current state; fix failures causally without weakening frozen inputs, hiding failed owners, or raising resource limits to pass. |
| T07 Candidate promotion | No claim that the complete Libraries 1.0 installed candidate is selected or qualified. | Choose exact identities and run independent Windows/Linux reconstruction, conformance, resource, clean-install and recovery gates before promotion. |
| T08 Release readiness | Signed preview remains history. | Close support/security/compatibility and required WVDB/product gates; signing, tagging and public publication need explicit authorization. Compiler/library completion alone is not a product release. |

The [WVDB specification plan](WVDB-1.0-Specification-Plan.md) owns the dependency
set for B10/T05/T08: types/values, entities/tables, relationships/constraints,
indexes, queries/results, transactions/snapshots, catalog/migration,
storage/reclamation, durability/integrity/full backup/restore, service,
operations, and conformance. Accepted upper-layer direction does not close its
unfinished normative or implementation rows. Each used dependency must name
its accepted contract and executable evidence before the integrated consumer
can pass; do not label the database complete from isolated storage tests.

## Optional and unresolved choices

These entries cannot silently enlarge or shrink the required baseline:

| Choice | Current direction | Decision still needed |
| --- | --- | --- |
| O01 CDDL, CBOR Sequences, CBOR artifact/index profile | Optional in the suite proposal. | Admit only for a named consumer with finite signatures, limits and tests. |
| O02 COSE and prevalidated artifact trust | Optional suite profiles; existing signed release format remains its own contract. | Select algorithms/trust/receipt identity and custody before a trusted fast path; never bypass ordinary admission on an unauthenticated cache claim. |
| O03 Compression, graph algorithms | Optional bounded profiles. | Concrete consumer, selected algorithms, output/work limits and independent failure evidence. |
| O04 SSE and WebSocket | Plan recommends optional; Backend review still contains an inclusion question. | Explicitly select or defer; do not infer required status from a catalog entry. |
| O05 Pure time and Id128 modules | Proposed `Foundationˉtime` and `Foundationˉidentity`, absent from the frozen twelve-module registry. | Separate module/contract decision if a consumer requires them; no silent Foundation registry addition. |
| O06 UI, OS, accelerator, model-provider and specialized database profiles | Outside this automatic host goal. | Separate explicit scope and qualification before claiming support. |

Before starting D/H/B implementation, resolve the relevant candidate's API,
module/package identities, limits, provider/cryptography choices, HTTP and
filesystem semantics, and migration of existing users. Prefer one family review
at its dependency boundary to treating the entire draft catalog as approved.
No new public signature, standard profile or security choice is accepted by
this inventory.

### Required contract reconciliation

The completion plan calls for Decimal128, while the frozen `Foundationˉnumeric`
block contains integer/floating operations and whole-u64 decimal parsing, not a
Decimal128 type or arithmetic API. The accepted WVDB type direction includes
exact decimal with a 128-bit coefficient and precision up to 38. Track this as
a required unresolved cross-owner contract, not an optional deletion or an
implicit registry addition. Settle arithmetic, rounding, scale/overflow, module
ownership and signature identity with the maintainer before implementing a
public Decimal128 surface; the
[WVDB type review](WVDB-1.0-Types-Sizes-Documents-Graphs-And-Backup.md) remains its
database-side owner.

## Current checkpoint and next action

1. Restore the frozen Foundation identity accidentally changed by editorial
   implementation notes. The [focused repair evidence](../Evidence/2026-09-22-Frozen-Foundation-Identity-Repair.json)
   records passing Windows/Debian checks; it is not a green whole-CI claim.
2. Keep F01/F02/F06 as the active feature chunk. The concrete migration candidate
   is [Package-Lock](../../Libraries/Package/Package-Lock.wv): replace its private
   repeatedly concatenated part-directory bytes with an owned typed directory,
   then query locked parts and dependency/path checks without consuming it.
   This is not an implemented consumer. It requires accepted immutable
   `Vectorˉborrowˉat` (currently absent from compiler intrinsic binding),
   record-element Vector support, explicit allocation-budget threading outside
   Main, and scanner return/aggregate ownership. Implement indexed observation
   with exact nominal/loan identity and bounds trapping first, then integrate
   the complete consumer. More length-only fixtures do not satisfy this gate.
   Preserve lock bytes, validation/failure ordering, bounded resource use and
   the existing `package-format` consumer oracle; document any unavoidable
   public API migration rather than hiding it in a parser refactor.
3. Expand each active row into declaration-level implementation and evidence
   mappings, using its existing owner rather than adding parallel verifiers.
4. Ask for maintainer decisions at unresolved contract boundaries. Required rows
   remain open until resolved; optional rows are never silently substituted for
   missing required behavior.

This is a source/document audit and delivery inventory, not a fresh conformance
run of every listed family. No overall percentage or completion date is inferred
from row counts. Normal local verification keeps its ten-minute wall-clock
budget; longer commands need advance approval of command and maximum duration.
