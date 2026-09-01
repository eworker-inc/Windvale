# Windvale Seed to Language 1.0 migration plan

> Status: Current candidate migration plan
> Authority: Informative; linked specifications and decisions own exact contracts
> Last reviewed: 2026-08-31

## Status

This is the candidate repository migration plan required by
[Decision 0751](../Decisions/0751-Accept-Windvale-Language-1.0-Direction.md)
and refined by
[Decision 0752](../Decisions/0752-Complete-Language-1.0-Collection-And-Package-Data-Boundaries.md)
and
[Decision 0753](../Decisions/0753-Require-Language-1.0-AI-Accelerator-Evidence.md),
with the first paper findings resolved by
[Decision 0754](../Decisions/0754-Resolve-First-Language-1.0-Paper-Findings.md)
and the command workload findings resolved by
[Decision 0755](../Decisions/0755-Resolve-Language-1.0-Command-Workload-Findings.md)
and the file-copy workload findings resolved by
[Decision 0756](../Decisions/0756-Resolve-Language-1.0-File-Copy-Findings.md)
and the database-transaction findings resolved by
[Decision 0757](../Decisions/0757-Resolve-Language-1.0-Database-Transaction-Findings.md)
and the compiler-front-end findings resolved by
[Decision 0758](../Decisions/0758-Resolve-Language-1.0-Compiler-Front-End-Findings.md)
and the HTTP-handler findings resolved by
[Decision 0759](../Decisions/0759-Resolve-Language-1.0-Http-Handler-Findings.md),
the concurrent-service findings resolved by
[Decision 0760](../Decisions/0760-Resolve-Language-1.0-Concurrent-Service-Findings.md),
and the retained-GUI findings resolved by
[Decision 0761](../Decisions/0761-Resolve-Language-1.0-Retained-Gui-Findings.md),
and the numeric/graphics findings resolved by
[Decision 0762](../Decisions/0762-Resolve-Language-1.0-Numeric-Graphics-Findings.md),
and the package-parser findings resolved by
[Decision 0763](../Decisions/0763-Resolve-Language-1.0-Package-Parser-Findings.md),
and the System/FFI findings resolved by
[Decision 0764](../Decisions/0764-Resolve-Language-1.0-System-Ffi-Findings.md),
with complete-suite reconciliation accepted by
[Decision 0765](../Decisions/0765-Complete-Language-1.0-Source-Freeze-Candidate.md).
The five localized-source workloads and replacement reconciliation are accepted
by
[Decision 0766](../Decisions/0766-Complete-Language-1.0-Localized-Source-Reconciliation.md).
It does not authorize implementation before the Language 1.0 source-freeze
decision. It defines how the repository will advance once the
[semantic specification](../../Specifications/Windvale-Language-1.0.md),
[grammar](../../Specifications/Windvale-Language-1.0-Grammar.md),
[machine grammar](../../Specifications/Windvale-Language-1.0.ebnf),
[localized-source and source-vocabulary specification](../../Specifications/Windvale-Language-1.0-Localized-Source.md),
[source-profile artifact formats](../../Specifications/Windvale-Language-1.0-Source-Profile-Formats.md),
[Foundation contract](../../Specifications/Windvale-Language-1.0-Foundation.md),
and
[Foundation signature registry](../../Specifications/Windvale-Language-1.0-Foundation-Registry.md)
are frozen.

The implemented source remains
[Windvale Seed](../../Specifications/Seed-Language.md).

## Migration objective

Advance the existing Windvale compiler, libraries, tools, applications, and
operating system to one edition-1 source contract without:

- creating a parallel compiler;
- keeping permanent Seed syntax aliases;
- routing compilation through textual assembly;
- treating WVB or native layout as the definition of source semantics;
- performing a repository-wide blind rewrite before compiler support exists;
- rerunning every verification tier after every slice; or
- claiming complete Language 1.0 support from a partial target.

The migration preserves historical releases and qualification evidence rather
than preserving obsolete readers in current code.

## Preconditions

Implementation begins only after:

1. a named decision freezes the complete edition-1 specification identities;
2. every paper-corpus source bundle has an accepted semantic walkthrough;
3. ownership, cleanup, and concurrency freeze blockers are resolved;
4. the Foundation required signatures have canonical identities;
5. editor and formatter grammar is synchronized;
6. the feature responsibility matrix below is approved;
7. compiler and runtime development limits are recorded;
8. package-data manifest binding, accounting, malformed-input, and
   non-duplicating shipment are specified;
9. the local AI accelerator paper workload assigns every discovered need to the
   general language or an explicit library, target extension, verified
   representation, provider, or backend owner; and
10. source-lexicon, public-interface source-vocabulary, Unicode/security,
    conversion, editor, diagnostic, shipment/cache, cross-host, and bounded-
    performance paper contracts are accepted, with executable and measured
    results retained as implementation qualification; and
11. the change-aware verifier maps every affected boundary to a focused owner.

The source freeze may permit staged implementation. It may not leave implemented
features semantically target-dependent.

## Compatibility boundary

Seed and edition 1 are distinct source editions. The active-development policy
allows one planned repository transition:

- current tools continue accepting Seed until the edition-1 front door reaches
  the agreed migration checkpoint;
- the compiler then accepts explicit `#!wv/1 <profile>@<version>` source through
  the new path;
- repository modules migrate in dependency order;
- once the last required repository module and fixture migrates, the current
  front door removes Seed parsing unless a named recovery case says otherwise;
- no source file is guessed as Seed, Language 1.0, or a source profile;
- no unselected keyword/source-vocabulary spelling, profile, positional
  constructor, or result-propagation alias is retained merely for convenience;
  and
- historical source remains buildable only from its pinned release or restored
  recovery workspace.

A temporary development compiler may accept both explicit editions while the
repository is migrating. That is one versioned front door with two explicit
grammars, not two compiler architectures. The overlap has a named removal
checkpoint and is not a product compatibility promise.

## Source mapping

| Seed contract | Edition-1 migration |
| --- | --- |
| No source edition declaration | Add `#!wv/1 en@1` as the first physical line. |
| ASCII/U+02C9 Seed identifiers | Preserve the admitted subset while adding exact edition-pinned normalized Unicode project identifiers and U+02C9 semantic-concept separation. |
| Canonical keyword and public-library spellings only | Select one explicit immutable composite source profile; lower its exact spellings to canonical token and imported declaration identities. |
| Inline module declaration with portable, hosted, or system profile | Use standalone module, language profile, platform, authority, and capability declarations. |
| Legacy plain `capability` declaration | Replace with required or optional versioned capability metadata and explicit bound references. |
| `void` | Replace with `unit` and `()` where an ordinary value is required. |
| `i32`, `i64`, `u8`, `u32`, `u64` only | Retain exact meanings and admit `i8`, `i16`, `u16`, strict floats, and runes where selected. |
| Positional record construction | Rewrite to named construction; no compatibility constructor remains. |
| Single-payload variants | Retain valid cases and migrate payloads to named multi-field form where needed. |
| Statement-only narrow `try` | Rewrite to value-producing exact `Result<T,E>` propagation. |
| `sequence<T,N>` and affine local builder | Select `Array`, `Vector`, `Sequence`, slice, map, arena, byte builder, or text builder by ownership and budget. |
| Packed bytes for mutable typed state | Replace with owned records, vectors, maps, or typed arenas unless bytes are the actual format. |
| Manual `Valid`/`Status`/`Error` guards | Replace with standard Option, Result, named domain variants, exhaustive match, and explicit adapters. |
| Repeated immutable concatenation | Replace construction hot paths with bounded byte or text builders. |
| Explicit resource close on selected paths | Acquire move-only instances in `using` and keep fallible finish/commit explicit. |
| No function values or closure capture | Add first-class functions and explicit copy/move/borrow capture only where needed. |
| No structured tasks | Introduce lexical task scopes only after ownership and resource slices are qualified. |
| No unsafe source | Introduce System-profile unsafe/FFI only at audited machine boundaries. |

Automated migration may perform syntax-preserving mechanical changes such as
edition headers, `void` to `unit`, and unambiguous named-record rewrites.
Ownership, result-domain, collection, resource, capability, and concurrency
changes require semantic review.

## Implementation responsibility matrix

| Language 1.0 area | Primary implementation owners | WVB change expected? |
| --- | --- | --- |
| Source descriptor, module metadata, and target-scope registry | Descriptor reader, declaration parser, source graph, build target admission, editor | Only when serialized metadata requires a new format. |
| Source profiles and lexicons, Unicode identifiers, and localized public-library source labels | Descriptor/profile reader, lexer, parser spans, public-name resolver, package/build plan, library interface catalogs, diagnostics, formatter/editor, cache, verification | No semantic WVB change for imported labels; exact Unicode project names may require updated source/debug metadata and ASCII native mangling. |
| Bounded immutable package data | Parser, source graph, package/build plan, WVB/package formats, loader, publisher | Likely requires a typed content-reference table unless the value is embedded once without duplication. |
| Names and short private identities | Bindings, WIR directory, object/debug symbol mapping | Not for private compiler identity alone. |
| `unit` and `never` | Type model, control-flow validation, WIR lowering | Possibly shape metadata; no opcode by default. |
| New fixed numerics and strict floats | Type checker, operators, Foundation, verifier, runtimes, native backend | Likely for new scalar operations. |
| Named update and multi-field variants | Parser, bindings, WIR construction | Only when current aggregate operations cannot encode the result. |
| Value `if`/`match` and destructuring | Body parser, ownership analysis, WIR control flow | Prefer lowering through existing blocks and values. |
| Generics and protocols | Symbols, exact argument-type matcher, full-arity named explicit arguments, type checker, specialization cache, package interfaces | Specialized output should remain ordinary typed operations; no overload or result-context search. |
| Ownership and borrowing | Type/ownership analysis, WIR evidence, diagnostics | Runtime move opcodes are not required when static lowering suffices. |
| Foundation collections and arenas | Libraries, allocation runtime, optional intrinsics | Only for justified bulk primitives or verified handles. |
| Checked slices, byte views, strict slice decode, and decimal byte formatting | Foundation collections/bytes/text, ownership analysis, optional intrinsics | Ordinary borrow and builder lowering; no new opcode by default. |
| Option, Result, and `try` | Foundation identity, parser, type checker, WIR lowering | Prefer existing variant and branch operations. |
| Function values and closures | WVCF/WVIC catalog, synthetic WVLB/WVIR lowering, WVB 1.30/1.31 verifier and scalar runtime, and selected native x86-64 ABI | Named functions plus copy, move, and confined immutable-borrow scalar/enum captures execute through exact structural descriptors and frame-owned native environments. |
| Application entry and root binding | Package/build plan, launcher profile, capability catalog, runtime resource domain | Entry selection remains metadata; no special source function or ambient allocator. |
| Resources and `using` | Ownership analysis, cleanup lowering, capability runtime | May need owned instance and generation representation. |
| Opaque operation contexts and exact stream progress | Hosted operation/network providers, launcher, capability runtime, generation verifier | Prefer ordinary opaque values and capability calls; no language opcode. |
| Builders and formatting | Foundation libraries, bounds analysis, optional intrinsics | No syntax-specific opcode required; interpolation syntax is outside edition 1. |
| Structured concurrency | Effects, captures, Foundation tasks, runtime providers, target schedulers | Requires an explicit verified task/runtime contract. |
| Unsafe and FFI | System type checker, ABI specifications, native backend, verifier | Target- and ABI-specific additions are likely. |
| Accelerator host and custom-kernel boundary | Libraries, capability runtime, target analysis, WIR/verifier, software oracle, provider backends | General host logic should use ordinary WIR; target kernels or new numeric operations may require separately versioned verified representations. |

The assembler is not the source compiler's next stage. WVA textual input and the
native compiler backend share instruction encoding, relocation, object
construction, and ABI contracts. The compiler must not emit WVA text and then
reparse it.

## Vertical implementation slices

### Slice 0: frozen contracts and reference examples

- Publish canonical specification identities.
- Convert paper programs into parser/type/ownership fixtures.
- Retain the accepted generic-call, capability-root, Foundation-call,
  launcher-entry, and target-scope cases from Decision 0754.
- Retain the accepted command sequence, strict parsing, reserved-builder,
  stream-authority, and launcher-status cases from Decision 0755.
- Retain the accepted byte-buffer, release/completion, known-partial,
  filesystem-authority, and cancellation/lifecycle cases from Decision 0756.
- Retain the accepted runtime arena, first-item construction, checked borrowed
  observation, typed-schema, explicit-commit, and fresh-recovery cases from
  Decision 0757.
- Retain the accepted full-arity explicit generic calls, empty bounded owners,
  rank-based one-owner observation, Copy read-through, immutable arena
  publication, scalar source positions, diagnostic saturation, and exact byte
  emission from Decision 0758.
- Retain the accepted checked slice/immutable byte views, strict slice decode,
  invariant byte decimal append, opaque operation context, and exact
  reliable-stream progress cases from Decision 0759.
- Retain the accepted task construction, derived context, cancellation,
  creation-order result collection, and runtime/provider failure separation
  cases from Decision 0760.
- Retain the accepted arena replace/remove, closed events, parent-only stale
  result application, identity tombstone, and exact immutable-frame publication
  cases from Decision 0761.
- Retain the accepted contextual array, checked mutable slice, strict f32,
  policy-bearing conversion, canonical numeric formatting, and bit-identical
  parallel-equivalence cases from Decision 0762.
- Retain the accepted map completion/publication, complete ordered-set,
  ordering-law, explicit bounded-parser, package-content dedup/accounting, and
  canonical-topology cases from Decision 0763.
- Retain the accepted concrete ABI target, registered ABI contract, pointer-kind,
  caller-owned scratch/region, recoverable-data/terminal-containment, explicit
  status/unwind, and safe-publication cases from Decision 0764.
- Add editor grammar tests.
- Retain accepted localized-source descriptor/profile, Unicode/security,
  public-interface vocabulary, conversion, editor, shipment/cache, cross-host,
  and bounded-performance cases.
- Record baseline compiler time, verification time, memory, WIR size, WVB size,
  and representative application artifact size.

### Slice 1: edition, metadata, and naming

- Add explicit edition dispatch.
- Add the bounded universal descriptor, exact profile/lexicon/catalog admission, normalized
  Unicode identifier lexer, and public-label-to-canonical resolver before
  ordinary semantic analysis.
- Implement standalone profile and independent metadata.
- Resolve opaque platform keys through the canonical target-scope registry and
  retain the structured environment/architecture/ABI/extension descriptor.
- Preserve exact import and private identity behavior.
- Add source-to-short-machine-name inspection without changing public identity.
- Add `package data` parsing and exact manifest binding without native paths,
  filesystem authority, or duplicate content objects.

### Slice 2: values and control

- Add `unit`, `never`, missing fixed integers, strict floats, and runes.
- Add named update, multi-field variants, destructuring, and value-producing
  control flow.
- Update exact diagnostics and malformed-input coverage.

### Slice 3: typed failure

- Publish exact Foundation Option and Result identities.
- Implement value-producing `try` and explicit error adapters.
- Migrate manual status families in coherent library/compiler areas.

### Slice 4: generics and collections

- Implement unique argument-derived structural generic resolution, full-arity
  explicit arguments for one resolved named declaration, bounded specialization,
  and retained solution evidence without result-context or overload search.
- Add arrays, vectors, immutable sequences, slices, ordered maps, ordered sets,
  arenas, and builders.
- Migrate repeated concatenation and packed mutable state with before/after
  performance measurements.

### Slice 5: ownership, borrowing, and resources

- Add Copy/shared/owned/borrowed classification.
- Implement moves, borrow checking, freeze, handles, reverse-order release, and
  `using`.
- Admit the exact effect-clause front end required to express allocation and
  release, while leaving complete resolution, inference, function-value, and
  capture enforcement owned by Slice 6.
- Admit the exact `using Name = Expression Block` front end and retain its
  binding, acquisition, and body spans before connecting owned-resource
  classification, reverse-order release, and exit-path lowering.
- Migrate Foundation and one hosted file/resource consumer before broad rollout.

### Slice 6: functions and effects

- Add named call arguments, function values, exact effect sets, and explicit
  captures.
- Bind required module capability roots separately from lexical captures and
  prove that generic calls and closures cannot hide authority or borrowed
  lifetime.

### Slice 7: hosted structured concurrency

- Implement the accepted `Construct`, semantic `Spawn`, and consuming `Await`
  calls with task scopes, typed handles, capture, join, cancellation, and
  teardown.
- Qualify one sequential scheduler and one parallel-capable host without changing
  source semantics.

### Slice 8: system and FFI

- Add unsafe definitions/invocation blocks, registered ABI identities, concrete
  target predicates, distinct nullable/non-null pointers, and the accepted
  aligned scratch/write-region Foundation surface.
- Implement only named ABI and machine contracts with hostile-input, checked
  address/range/alignment/lifetime/alias, safe-publication, and isolated terminal-
  containment tests.
- Migrate one real runtime or OS boundary before expanding.

Each slice includes lexer/parser, semantic analysis, WIR, Foundation, runtime,
editor, migration, and focused verifier changes required by that feature. A
slice does not publish an incomplete artifact as complete Language 1.0.

The accelerator paper workload is a source-freeze oracle, not a ninth core
implementation slice. After the general language slices are implemented, its
portable host/framework code can become a consumer. Accelerator operations,
kernel admission, verified representations, and physical providers advance under
their own measured contracts without creating a second language or compiler.

## Repository migration order

Migrate source consumers in dependency order:

1. minimal Foundation contracts required by the current slice;
2. compiler source models and front door;
3. runtime, verifier, and inspection tools;
4. reusable libraries;
5. command-line and hosted applications;
6. native producer and publisher tools;
7. operating-system services and system boundaries; and
8. examples, tests, fixtures, generated artifacts, and documentation.

The compiler is a major stress test but not the only ergonomics oracle.
Applications and the paper corpus must prevent compiler-specific patterns from
becoming the whole language design.

## WVB and native-format policy

Language 1.0 is a source contract. For each feature:

1. define typed WIR lowering;
2. determine whether existing WVB can represent the semantics;
3. add a WVB version only when representation or verification genuinely changes;
4. update verifier rejection and hostile-input cases before execution;
5. preserve interpreter, JIT, AOT, and native semantic agreement; and
6. version native ABI, callable, object, or relocation changes independently.

Named arguments, named record order, `try`, ownership checks, generic
specialization, and many control constructs may compile away. Source syntax is
not a reason to add an opcode.

Portable semantics, bytecode, serialization, runtime behavior, and golden hashes
require paired-host evidence before cross-host conformance is claimed.

## Verification and performance

Use the narrowest reliable changed-file verifier for each coherent slice. Do not
run Fast, Development, Standard, and Qualification sequentially for the same
tree. A broader passing gate replaces narrower gates for that source state.

Every material slice records:

- input source and module count;
- host, target, compiler identity, and profile;
- elapsed compiler and verifier time;
- peak or working-set memory when practical;
- emitted WIR operations and retained evidence;
- WVB and native artifact bytes;
- allocation and retained collection maxima; and
- comparison with the pre-slice reference workload.

Optimized implementations keep a simple semantic oracle. Performance work must
not weaken deterministic output, bounds, malformed-input rejection, or
portability.

## Current Slice 5 fallible Vector-construction checkpoint

[Decision 0840](../Decisions/0840-Bind-Fallible-Vector-Construction-In-Wvir.md)
connects the frozen public `Vectorˉconstructˉreserved::<T>` call to exact source
binding and typed WVIR operation 172. The call accepts one directly named
available `Memoryˉbudget`, consumes that owner, evaluates an exact `u64`
maximum-items value, and produces only the canonical
`Result<Vector<T>, Allocationˉfailure>` for the same explicit scalar `T`.

The persisted operation contains no pointer, capacity buffer, heap identity, or
lease-table representation. It carries the maximum temporary, consumed budget
slot, and canonical Memory module needed to reconstruct and independently
verify the complete public contract. WVIR 1.5/1.6 now represents operation 171
or 172. The existing bounded ownership dataflow rejects an unavailable budget
and a second consumption.

The current WVB boundary is intentionally closed: a valid 456-byte WVIR reaches
the emitter and returns exact `Unsupportedˉoperation` without publication. This
keeps allocation leases, provider debit/refusal, Vector representation, and
teardown as one later executable checkpoint instead of freezing a partial
runtime design. Eight malformed directories, five typed source failures, and
one use-after-consumption case protect the boundary.

The front-door summary advances to 478 cases and 114 source fixtures. The
registry remains 112 owners and advances to 5,332 cases at SHA-256
`ae29842cedcd3eda416b8008cf77e03b9346faba5bf0779d4ebacc7468be51f0`.
Executable construction, physical-provider connection, recoverable append,
semantic effect enforcement, general owned calls, `using`, reverse release,
and a real hosted resource consumer remain before Slice 5 completes. Internal
compiler micro-optimization remains deferred until Language 1.0 becomes the
seed; exact semantics, bounded verification, and workflow-blocker fixes remain
appropriate during migration.

## Current Slice 5 allocation-lease accounting checkpoint

[Decision 0841](../Decisions/0841-Prove-Generation-Safe-Allocation-Leases.md)
adds the provider-independent owner transition needed beneath executable
construction. Budget generations are odd; successful lease conversion advances
to the following even generation and makes the old budget stale before
publication. Reused budget slots advance to the next odd generation, while the
maximum generation remains retired.

The private 28-byte lease evidence binds domain identity, generation, maximum
retained bytes, current retained bytes, and alignment ceiling. It is carried by
the bounded runtime oracle and is explicitly not source data, WVB, ABI, or a
required target layout. Construction validates positive and ordered byte limits,
power-of-two alignment through 4,096, and available authority. Budget or
generation exhaustion preserves the input state and reports exact recoverable
evidence. Release requires the same exact metadata, invalidates the owner, and
recursively credits the parent; teardown handles live budgets and leases under
the existing 65-domain bounds.

The focused accounting owner now passes 29 cases. Its deterministic 35,799-byte
WVB has SHA-256
`3f156ef17f29c5673c0d383c713e04814327243783d2047cce2fc8fe6be117fb`.
The registry remains 112 owners and advances to 5,344 cases at SHA-256
`b34bd9e5ce73255db7da366b908dda29249df9514aff6f7dbb1918ce4d4489e1`.
The next checkpoint connects operation 172 to one bytecode instruction,
compiler-aligned verification, physical Vector backing, typed success/refusal,
and collection-owned lease release.

## Current Slice 5 executable fallible Vector checkpoint

[Decision 0842](../Decisions/0842-Execute-Fallible-Vector-Construction-As-Wvb-1.24.md)
connects operation 172 to WVB 1.24 opcode `CF`. The instruction consumes one
exact `u64` maximum and the named available budget, then produces only exact
`Result<Vector<T>, Allocationˉfailure>`. The verifier derives Vector and failure
identity from that Result and tracks it as affine; WVB contains no lease token,
provider generation, heap pointer, or backing layout.

The runner converts the budget to one private generation-safe lease, allocates
one reserved scalar Vector backing, and binds lease lifetime to the descriptor.
Success and target refusal both consume the source budget and return typed
Result paths; local refusal cleanup releases whichever owner remains. Zero is a
violated precondition and traps with `WVR3008`. Requested-byte evidence
saturates rather than wrapping, and the runner's 2,047-cell backing ceiling
remains an explicit target profile rather than a Language maximum.

Generic materialization remains dependency-ordered, but WVB emission now uses a
separate bounded nominal-category rank and remaps private references. The exact
Result may therefore point forward to its Vector descriptor while the Types
directory retains canonical category order.

The successful fixture is deterministic 747-byte WVB 1.24 at SHA-256
`e25ff63b466d3e4a219afdc03a64c2ff53418dffc9039fea0678ff3328d2dcd1`.
The combined focused owner passes 32 cases: five valid modules, 19 malformed
mutations, both Split and Vector provider outcomes, deterministic publication,
and the zero trap. The registry remains 112 owners and advances to 5,361 cases
at SHA-256
`7da8ebac77d31f21554b198e9ee90598280c31c72cf65c1c7344835eddc4b8a4`.

At that checkpoint recoverable append, general owned calls and joins, semantic
`using`, reverse-order release, and one hosted resource consumer remained in
Slice 5. Broad
transitional-compiler micro-optimization remains deferred until self-hosting;
measured packaging/cache waste and correctness blockers remain appropriate
current work.

## Slice 4/5 recoverable Vector-append checkpoint

[Decision 0843](../Decisions/0843-Execute-Recoverable-Vector-Append-As-Wvb-1.25.md)
connects the frozen public `Vectorˉappend::<T>` contract to WVIR 1.7/1.8
operation 173 and WVB 1.25 opcode `D0`. The first executable profile requires a
direct mutable non-parameter Vector local and resource-free scalar element. It
does not weaken the source contract: success consumes one item and appends
atomically; capacity refusal leaves the Vector unchanged and returns the item
with exact `Capacityˉexhausted(Maximumˉitems)` evidence.

The compiler now separates generic dependency planning from serialized type
identity. Final WVB Types are grouped as records, enums, variants, arrays,
Vectors, and Sequences, sorted by ordinal name inside each category, and every
nominal reference is remapped to that order. This keeps byte identity stable
when the future self-hosting compiler changes its internal discovery algorithm.

The deterministic fixture is 3,096-byte WVB 1.25 at SHA-256
`6478cc8b302e91caa54ff3aea835ef3ea1c1722161cd4f12aa587aa432b6918f`.
The focused owner passes 47 cases: six valid modules, 31 malformed mutations,
deterministic publication, construction and append success/refusal, and the
construction precondition trap. The registry remains 112 owners and advances
to 5,376 cases at SHA-256
`cf78e39ec42551a9fc1715e4582a1a0971aeb35ad2e547a1f7587c0d72da267d`.

General owned calls and joins, semantic `using`, reverse-order release, and one
hosted resource consumer remain in Slice 5. Seed-specific micro-tuning remains
deferred until self-hosting; the canonical ordering, bounded ownership, failure
semantics, and focused verification completed here survive that transition.

## Slice 5 owned Vector calls and forward joins checkpoint

[Decision 0844](../Decisions/0844-Prove-Owned-Vector-Calls-And-Forward-Joins-In-Wvir.md)
adds an independent bounded ownership proof for exact kind-11 `Vector<T>`
parameters, locals, temporaries, ordinary calls, results, returns, and forward
joins. By-value calls consume their source owner, borrowed calls preserve it,
and joins retain availability only when every incoming state agrees. The proof
is limited to 64 blocks, 64 slots, and 4,096 operations; Vector phis, backward
control, and temporary escape remain closed.

The analyzer continues to publish provisional WVIR evidence, while the emitter
is the independent trust boundary. One positive fixture reaches exact
`Unsupportedˉshape` because WVB 1.25 does not yet encode parameter transfer
modes or deterministic callee cleanup. Three negative fixtures reach exact
`Invalidˉanalysis` / `Invalidˉwir` for borrow-after-move, duplicate transfer,
and asymmetric-join reuse. No WVB version or existing golden product changes.

The combined focused owner passes 51 cases and preserves the existing 752-byte
Split, 1,107-byte Vector-construction, and 3,096-byte Vector-append identities.
The registry remains 112 owners and advances to 5,380 cases; its 17,078 LF-only
bytes have SHA-256
`832449b3d8cce925d5cd34ef6c0e478ce7b0d95aa8603a63f60df08c3d1e3b0c`.

Aggregate-owned fields, loop fixed points, WVB call transfer and callee cleanup,
semantic `using`, reverse-order release, one hosted resource consumer, and the
fallible elastic budget and Vector-growth path below remain before Slice 5
completes. These ownership and validation boundaries survive self-hosting;
Seed-specific micro-tuning remains deferred.

## Slice 5 executable owned Vector call checkpoint

[Decision 0845](../Decisions/0845-Execute-Owned-Vector-Calls-As-Wvb-1.26.md)
connects the validated WVIR call proof to WVB 1.26 without a new opcode or
mode trailer. An exact Vector parameter shape is `23` for value, `26` for
immutable borrow, or `27` for mutable borrow; borrowed tags are illegal in
returns, non-parameter locals, fields, payloads, collections, and Types
entries. Any Vector parameter selects 1.26, while modules without one retain
their prior lowest version and exact bytes.

The emitter uses `local.take` for by-value transfer and retaining loads for
borrows. The verifier reconstructs each target signature and rejects mode
mismatches. The scalar runner retains one bounded internal mode byte per
parameter, normalizes borrowed cells to ordinary Vector representation, and
releases surviving descriptors in reverse slot order. A borrow therefore
balances only its temporary retain; a value call transfers and eventually
releases the owner.

The positive fixture deterministically emits a 1,733-byte WVB 1.26 module at
SHA-256
`ab79d05bb03afddbe6430adc127c8cdf084ea6499b16e3e25ebb3e477c408387`.
The combined focused owner passes 58 cases with seven valid modules, 37
malformed modules, four owned-call WVIR cases, and result `42`. Six new byte
corruptions cover version downgrade, invalid or substituted parameter modes,
a borrowed return, and a borrowed local. The three earlier source ownership
failures still reject before WVB publication.

The registry remains 112 owners and advances to 5,387 cases; its 17,187
LF-only bytes have SHA-256
`d482947c65e6c10dcb3b192c57d5f7bcb19fde0fe45cec71d5be92908ce3909b`.
That checkpoint still left loop fixed points, semantic `using`, nested-resource
destruction, aggregate-owned fields, one hosted resource consumer, and the
fallible elastic budget and Vector-growth path before Slice 5 completes.

## Current Slice 5 compact WVIR and semantic using checkpoint

[Decision 0846](../Decisions/0846-Compact-Wvir-Operation-Records.md) replaces
the internal 32-byte WVIR operation record with a 28-byte form: kind and operand
count are independently bounded `u16` fields, while owning block, shape,
temporary, first operand, target, and auxiliary retain `u32`. Ordinary,
specialized, memory, and append directories advance atomically to WVIR 1.9
through 1.14. Versions 1.1 through 1.8 are rejected rather than preserved by a
parallel decoder. The 4 MiB ceiling is unchanged; the current compiler source
graph now uses 3,853,556 bytes and leaves 340,748 bytes of real headroom.

[Decision 0847](../Decisions/0847-Lower-Semantic-Using-And-Prove-Loop-Ownership.md)
connects the already frozen `using` syntax to exact owned-Vector cleanup. The
initializer is evaluated before its immutable binding-kind-4 name exists. The
name is visible only inside its body. Typed WVIR operation 174 releases the
direct local on fallthrough, return, failed `try` propagation, `break`, and
`continue`, emitting only scopes actually exited and ordering nested releases
from innermost to outermost. A non-Vector resource rejects as
`Invalidˉresource`; moving the Vector before cleanup rejects at the independent
ownership boundary.

The same checkpoint opens bounded ownership-invariant loops. Forward joins
retain their conservative agreement rule, while every backedge must match the
saved loop-header state exactly. Operation 174 lowers to existing
`local.take <slot>; pop`, so WVB needs no new opcode or minor version. Four
positive fixtures contain seven releases, and the executable fallthrough case
is a deterministic 1,211-byte WVB at SHA-256
`f541cd186564d1e696820a53c4a17baf50ba0d393dbb4bc8b1c381960b595257`
that returns `42`.

The combined focused owner advances to 70 cases: 11 valid products, 38
malformed modules, four retained owned-call cases, 12 `using` cases, and seven
release sites. The registry remains 112 owners and advances to 5,399 cases; its
17,351 LF-only bytes have SHA-256
`75683af614bde5f4d6b8aa4c7439bf7c1a0b7df5c3160553900ab2173af5f6e7`.
Aggregate-owned fields and one hosted resource consumer remain before Slice 5
completes. Provider-backed expansion of a budget's authority is a later hosted
capability checkpoint, distinct from Core Vector growth under already-held
authority.

## Current Slice 5 transactional Vector-growth checkpoint

[Decision 0848](../Decisions/0848-Execute-Transactional-Vector-Growth-As-Wvb-1.27.md)
adds the explicit Core path for applications whose bounded demand changes at
runtime. The Foundation call names both exclusive mutable owners and returns
typed allocation refusal:

```text
Vectorˉgrowˉreserved::<T>(
    borrow mut Vector,
    borrow mut Budget,
    Newˉmaximumˉitems,
) -> Result<unit, Allocationˉfailure>
```

WVIR 1.15/1.16 operation 175 carries the Vector and budget slots separately.
WVB 1.27 opcode `D1` carries those slots plus the exact Result type in thirteen
bytes. The scalar runtime reserves the complete replacement while the old
backing is live, copies only initialized cells, then releases and swaps exactly
once. Ordinary refusal preserves the original Vector and supplied budget;
non-increasing growth is a precondition trap.

The deterministic fixture is 3,628-byte WVB 1.27 at SHA-256
`30de39bdd12ad7718ad1fb465b14bc42f8463b6ecfc6ba1f10494cb6e67c5b59`.
It first proves exact 40-byte refusal against 24 available bytes and unchanged
length `1`; it then grows to maximum `2`, appends the second item, and returns
`42`. Fifteen byte-level mutations reject. The combined focused owner passed
the exact same 88 cases on Windows and Linux: 12 valid modules and 53 malformed
modules plus the retained owned-call and semantic-`using` evidence. Portable
WVB sizes and SHA-256 identities matched across both hosts.

“Use what the OS can provide” remains a separate rights-limited hosted provider
operation that may extend an application's budget under current policy. It will
not mean an unbounded or infallible allocation promise, and Core `D1` cannot
acquire that authority ambiently.

## Current Slice 5 aggregate-owned-field checkpoint

[Decision 0850](../Decisions/0850-Own-Vector-Containing-Aggregates-As-Wvb-1.28.md)
closes the copyability gap around records, variants, and fixed arrays that
recursively contain a Vector. Construction, local storage, by-value calls, and
returns move the complete aggregate. Field and element observation preserves
the parent, owned-field extraction by value rejects, and an explicit mutable
field borrow requires a mutable parent binding. The bounded WVIR proof still
rejects duplicate transfer, use after move, asymmetric joins, and owned phis.

WVB 1.28 retains ordinary shapes for whole owned aggregates and introduces only
three verifier-confined temporary-local views: shape `28` for record, `29` for
variant, and `30` for fixed array. Each view must appear in the exact generated
load/store/load/observer sequence and cannot be taken, passed, returned, or
otherwise escape. The scalar runtime uses the existing fixed arena and heap,
then deterministically traces surviving roots and releases nested Vector
descriptors and leases during return and top-level teardown.

The positive generic fixture emits a deterministic 1,538-byte WVB 1.28 module
at SHA-256
`b9810655b33c79cf980ea05f7fbca5511d3c34219f37e1b6a046a630a3e1c395`
and returns `42`. Four source failures cover use after move, duplicate move,
partial owned-field move, and mutable borrow from `let`; six byte corruptions
cover version, borrowed parameter, view identity, owner/view substitution, and
illegal takes. The combined focused owner passes 101 cases with 13 valid and 59
malformed modules on both permanent hosts with identical portable identities.
The registry remains 112 owners and advances to 5,430 cases; its 17,601 LF-only
bytes have SHA-256
`5e9d388aa6c744f1f865af15386ae0c652bb1768b3c7e8b434fcd555dc3acd87`.

## Completed Slice 5 hosted source-resource checkpoint

[Decision 0851](../Decisions/0851-Transfer-A-Rights-Limited-Source-File-As-Wvb-1.29.md)
closes Slice 5 with one rights-limited hosted resource consumer. The exact
`Platformˉfile.Sourceˉfile` identity is compiler supplied and move-only; only
exported `Main(Sourceˉfile) -> i32` can receive it from the launcher. Source
moves the owner into a semantic `using` scope and may observe only its `u64`
length through `Platformˉfile.Sourceˉlength(borrow File)`.

WVB 1.29 serializes the unforgeable resource as shape `34` and the length
observation as opcode `D2`. The runner's `--source-file` mode snapshots at most
1 MiB before guest execution, transfers one exact read right plus equal nonzero
provider/resource generations, and exposes no host path, handle, open
operation, or arbitrary byte read. The verifier confines the shape to the
exact entry and moved local, and ownership cleanup releases it exactly once.

The deterministic fixture is 373 bytes at SHA-256
`01065b752d7ea6d64e3bf36bdd4d8a0d2e5b7faf6794de173580003ed3935d05`.
The focused Windows owner passes 113 cases with 14 valid and 65 malformed
modules, 12 source-file cases, and retained budget, Vector, aggregate, call,
and `using` evidence. The registry remains 112 owners and advances to 5,442
cases; its 17,742 LF-only bytes have SHA-256
`9c966034fedace67e7b7ab32267badf9e8ecfbf814c77fcf1fa8049bea964b22`.
Independent Linux reproduction remains the next paired-host integration gate.

Slice 5 is complete. Borrowed aggregate signatures, partial moves, owned phis,
user-defined destruction, and resource-bearing Vector elements remain
deliberately closed. Slice 6 owns canonical effect resolution, call enforcement,
function values, and capture checking; broader asynchronous file operations
wait for those semantics.

## Completed Slice 6 callable checkpoint

Decisions 0852 through 0860 now connect exact structural function types,
explicit capture validation, bounded transitive effect analysis, concrete
callable cataloging, WVIR function references/indirect calls/plain-capture
environment creation, WVB 1.30/1.31 verification, and source-built scalar
execution. The executable subset is deliberately exact: synchronous safe
nongeneric targets, explicit empty effects, by-value parameters and results,
and either no captures or a copy, move, or confined immutable-borrow prefix of
inline scalars and enums.

The deterministic 400-byte callable fixture has SHA-256
`30eab353a6187ead317438d2c63a2bd6aa53d9ec682bc5c59d9d3b82530edfaf`
and returns `42` in 24 guest instructions. The focused owner passes 38 cases;
five byte mutations prove that the verifier rejects version, target, reference,
call, and descriptor mismatches.

The deterministic 325-byte closure-environment oracle has SHA-256
`397f716af132192697c77d9f4f03e72c937e188aca78cf0474c9faaa2234e0e2`.
It captures `40`, supplies public argument `2`, and returns `42` through WVB
1.31 `D5` followed by the existing `D4`. Nine mutations prove version, target,
type, count, capture-shape, reference-backed-capture, indirect-call, and
descriptor rejection. The runtime creates at most 1,024 environments and
retains at most 536,576 bytes (524 KiB) of immutable environment records for
one execution. WVCL 1.0 now gives each accepted source closure site a
deterministic synthetic-function ordinal after ordinary functions and final
generic instances. The capture analyzer can reconstruct the closure-local
binding phase from its validated evidence without repeating whole-body
semantic analysis. WVLB 1.4 retains that WVCL catalog and gives every
synthetic range an exact capture prefix, public-parameter suffix, real parent
declaration, and inherited generic identity. The integrated compiler emits the
synthetic body and `Closureˉcreate`, invalidates moved outer slots, confines an
immutable-borrow callable to its owner's lifetime, and rejects mutable borrow.

The selected native x86-64 ABI represents a callable in one 16-byte frame cell,
keeps copied capture environments inside the creating frame, revalidates target,
type, and environment before indirect entry, and reuses the ordinary direct-call
ABI and meters. The focused owner passes 60 cases across eleven deterministic
evidence modules, including two current-host native AOT successes and six native
rejections. Slice 6 is complete for this explicitly bounded Language 1.0
profile. Nonempty-effect or flag-bearing function values, write-through mutable
captures, retained captures, general same-signature dispatch, and escaping
environments remain separately versioned extensions; browser and OS execution,
paired-host qualification, and candidate promotion remain separate target gates.

## Current Slice 7 queued structured-task checkpoint

[Decision 0861](../Decisions/0861-Execute-Structured-Tasks-As-Wvb-1.32.md)
connects the accepted lexical task surface to the real edition-1 compiler,
WVIR 1.21, WVB 1.32, compiler-aligned verifier, and source-built scalar runner.
The compiler recognizes only canonical Foundation task identities, records the
exact async/effect/mode descriptor evidence, and lowers construction, context
derivation, spawn, consuming await, cancellation, and policy-bound scope exit.
Fallthrough, return, propagation, break, and continue emit scope teardown before
outer resource release. Affine scope and handle state must agree at every
control-flow join; work, context, and handles cannot escape their admitted
lifetime or be silently copied.

The current correctness oracle is one bounded queued scheduler. It supplies
the root memory budget and operation context through dedicated execution-request
major `6`, admits at most 64 children and 1 MiB of retained task state, charges
one work unit per dispatched verified WVB instruction, retains completed
aggregate results until await or teardown, and reports exact child traps and
call-depth/work exhaustion as typed outcomes. Request major `5` remains the
independent source-file snapshot contract. [Decision 0864](../Decisions/0864-Reserve-Structured-Task-Completion-Slots-Before-Spawn.md)
reserves one outcome position per accepted live child. [Decision 0865](../Decisions/0865-Reserve-Structured-Task-Retained-Memory-Before-Spawn.md)
also reserves the exact pending continuation, terminal cell, child locals, and
newly suspended parent frame before capture acceptance. Insufficient retained
capacity returns exact typed memory failure and the original work without state
mutation. [Decision 0866](../Decisions/0866-Observe-Structured-Task-Runtime-Environment.md)
adds a bounded cooperative observation point with exact deadline priority,
cancellation, and task-runtime loss/restart generations. The task-state core
has a standalone 46-case native self-test, while
executable source fixtures cover success, child trap, retained-result pressure,
work exhaustion, call-depth exhaustion, and retained-memory refusal through the
interpreter.

[Decision 0867](../Decisions/0867-Inject-Structured-Task-Environment-Through-Request-Major-6.md)
advances execution-request major `6` to minor `1` with one exact 72-byte header
for launcher-owned context, clock, deadline, tick, and task-runtime generations.
The public runner preserves its generation-1 default and adds one strict
`--task-environment` mode. A real edition-1 executable now proves deadline,
runtime loss/restart, stale context, and unavailable runtime through 17 focused
execution cases; malformed numeric and request input is rejected before module
execution.

[Decision 0868](../Decisions/0868-Queue-Structured-Task-Children-Before-Await.md)
removes the first runner's eager spawn behavior. Accepted work now enters one
bounded same-scope queue and returns its typed handle before child execution.
The reference scheduler selects four task-slot lanes in order `3, 1, 0, 2`,
while consuming awaits and report construction remain in source creation order.
One permanent source fixture proves that four sibling handles coexist and all
four observe a cancellation requested before the first await. Its prior eager
execution result was `0`; the queued runner returns `42` and preserves the six
earlier task-execution results.

[Decision 0869](../Decisions/0869-Expose-Structured-Task-Completion-Order.md)
makes that scheduling difference observable without adding a scheduler API.
Execution-request major `6`, minor `2` admits only the exact existing
`console.write_line(text) -> void` capability and a bounded standard-output
buffer. A real four-child source program now prints `3`, `1`, `0`, `2` as its
children finish, then returns `42` only when four consuming awaits still yield
values `0`, `1`, `2`, `3` through their creation-ordered handles. The
zero-capability task request remains minor `1`; source syntax, WVIR 1.21, and
WVB 1.32 are unchanged.

This completes the queued single-thread implementation checkpoint, not the
whole Slice 7 qualification promise. The child-provider generation and recovery
workload, a parallel-capable Windows host, a parallel-capable Linux host,
paired-host reconstruction, candidate promotion, and broad Qualification remain
explicit final Slice 7 gates rather than per-edit tests.

### Current Slice 8 unsafe-memory checkpoint

[Decision 0897](../Decisions/0897-Lower-Lexical-Unsafe-Invocation-Contexts.md)
implements the first typed-WIR boundary of Slice 8. Statement and value
`unsafe` blocks establish a compiler-private lexical depth bounded at 64. The
depth is restored when the block finishes and resets to zero at every new
ordinary or synthetic function boundary. Direct calls of unsafe functions or
Foreign declarations and indirect calls through a WVFT instance retaining the
unsafe bit fail with exact `Unsafeˉcontextˉrequired = 47` outside that context.
An unsafe declaration does not implicitly make its own body unsafe, and merely
constructing an unsafe named-function reference does not invoke it.

The wrapper is deliberately erased after checking: it adds no WVIR operation,
temporary, operand, block, serialized flag, or version. A 13-case focused
Analyzer-injection harness covers ten valid paths, three exact rejections, and
two structural-transparency comparisons.

[Decision 0898](../Decisions/0898-Publish-Canonical-Foundation-Unsafe-Type-Identities.md)
publishes the exact compiler-owned pointer, scratch, write-region, and failure
identities while ordinary construction and field observation remain forbidden.
[Decision 0899](../Decisions/0899-Lower-Canonical-Unsafe-Scratch-Construction-To-Wvir.md)
then lowers exact `Foundationˉunsafe.Constructˉscratch::<Abi>` to typed WVIR
operation 186. The operation carries an affine canonical
`Result<Foreignˉscratch<Abi>, Foreignˉmemoryˉfailure>`, one explicit
`Memoryˉbudget` slot, `u64` length and alignment operands, and the declared ABI
enum shape. WVIR 1.23/1.24 and its independent validation are implemented.

[Decision 0902: represent unsafe scratch construction in candidate WVB
1.33](../Decisions/0902-Represent-Unsafe-Scratch-Construction-In-Candidate-Wvb-1.33.md)
now maps operation 186 to opcode `DC`. Its exact budget-local,
construction-Result, and ABI-enum indexes are serialized and independently
checked. This is still a bounded implementation checkpoint rather than a
completed Slice 8 claim.

[Decision 0903](../Decisions/0903-Verify-Candidate-Wvb-1.33-Without-Opening-Execution.md)
now admits that candidate through the compiler-aligned structural, semantic,
typed-stack, control-reachability, and affine-ownership verifier. The verifier
checks exact materialized Foundation layouts, consumes the named budget owner,
and bounds each module to 4,096 scratch instructions and 256 distinct
scratch/ABI bindings.

[Decision 0904](../Decisions/0904-Execute-Wvb-1.33-Unsafe-Scratch-In-A-Bounded-Scalar-Provider.md)
opens the source-built scalar runner's first capability-free System-profile
provider. It admits 1-through-64-byte requests with power-of-two alignment
through 8, uses the existing budget lease and bounded heap, checks exact zero
initialization, returns canonical validation failures, keeps backing and lease
state private behind a non-address-like carrier, and finalizes ownership at
invocation teardown. The focused oracle executes three success/failure cases
and rejects a forged WVB 1.33 module with no `DC` before bytecode execution.

[Decision 0905](../Decisions/0905-Transfer-Affine-Memory-Budgets-Through-Ordinary-Calls.md)
closes the by-value callable-budget gap without a format bump. The source-WIR
validator now records each budget temporary's source slot and consumes that
slot only when the callee parameter is by-value. The WVB 1.33 scalar envelope
admits the canonical `Split` result's budget field. Entry and 64-byte child
budgets reach an ordinary helper, while a 32-byte child asked for 64 bytes
returns exact `Budgetˉexhausted` with 64 requested and 32 available. Duplicate
transfer is rejected before WVB publication. Ordinary immutable budget
borrowing remains valid through WIR.

[Decision 0906](../Decisions/0906-Represent-Immutable-Borrowed-Memory-Budget-Calls-In-Wvb-1.34.md)
now serializes that immutable boundary as WVB 1.34 shape `36`. The source
writer, complete verifier, scalar runner, and native x86-64 lowerer preserve
the caller's shape-`25` owner while confining the view to one canonical direct
call. The focused program observes the budget, then transfers the same owner to
a 64-byte scratch allocation and returns `42`; six shape/version corruptions
reject in both verification and native lowering.

[Decision 0907](../Decisions/0907-Observe-Immutable-Borrowed-Unsafe-Scratch-In-Wvb-1.35.md)
implements the first safe observation over that constructed scratch. Exact
`Scratchˉlength::<Abi>(Scratch: borrow Foreignˉscratch<Abi>)` emits WVIR
operation 187 and WVB 1.35 opcode `DD`, reusing shape `28` only as an exact
nominal immutable scratch view. The verifier relates each observed scratch to
its construction ABI, the scalar provider returns its private retained length,
and native x86-64 lowering reads the same private field in constant time. The
64-byte program returns `42` through all nine scalar/native cases; eight WVB
and seven WVIR corruptions reject at their admission boundaries. Mutable budget
or scratch borrowing, pointer and write-region borrowing, authenticated Foreign
calls, one migrated runtime or OS boundary, Linux reproduction, and paired-host
evidence remain pending.

[Decision 0909](../Decisions/0909-Lower-Mutable-Unsafe-Write-Region-Borrowing-To-Wvir.md)
implements the next typed compiler boundary without prematurely opening
execution. Exact `Borrowˉwriteˉregion::<Abi>` requires a lexical unsafe context,
one directly named mutable scratch borrow, three exact `u64` arguments, and the
canonical ABI-matched region/pointer-failure Result. WVIR operation 188 carries
the scratch slot, start, length, alignment, and ABI identity; WVIR 1.27/1.28
carry its non-specialized/specialized forms. Three valid source cases and seven
wrong-context, borrow, result, ABI, or label cases pass the focused local
Windows matrix; seven malformed WVIR mutations reject independently. WVB
encoding, affine region lifetime containment, provider and native execution,
pointer derivation, and authenticated Foreign calls remain pending.

[Decision 0910](../Decisions/0910-Represent-Mutable-Write-Region-Borrowing-In-Candidate-Wvb-1.36.md)
now preserves that exact operation as candidate WVB 1.36 opcode `DE`. The
13-byte instruction consumes ordered `u64` start, length, and required
alignment values and retains direct scratch-local, canonical region-Result,
and ABI-enum indexes. Exact mutable scratch parameters reuse nominal borrowed
record shape `28`, while the writer classifies the produced Result as affine.
The independent reader accepts two publications and rejects five malformed
WVB mutations. The current front-door verifier still rejects minor 36 at the
semantic phase. Compiler-aligned lifetime/non-escape verification, provider
and native execution, pointer derivation, authenticated Foreign calls, one
migrated runtime or OS boundary, Linux reproduction, and paired-host evidence
remain pending.

[Decision 0911](../Decisions/0911-Verify-WVB-1.36-Write-Region-Lifetime-Containment.md)
now admits that candidate through the compiler-aligned verifier without
opening execution. The verifier checks the exact region Result and seven-case
pointer failure, retains a bounded explicit scratch/region/ABI relation, moves
the Result affinely, and makes the source scratch unavailable through every
branch and function exit. It rejects ordinary payload extraction, call or
return escape, and scratch reuse. The complete inherited WVB 1.33/1.35 scratch
oracle remains passing. Scalar/provider and native region execution, pointer
derivation, authenticated Foreign calls, one migrated runtime or OS boundary,
Linux reproduction, and paired-host evidence remain pending.

[Decision 0908](../Decisions/0908-Bound-Compiler-Scale-Staging-Arena-Per-Resource.md)
removes the compiler-scale staging-memory blocker exposed while rebuilding this
checkpoint. Each resource is now constructed and written in one scalar-returning
helper invocation, so its dynamic byte arena is reclaimed before the next
resource. The exact 1,552,090-byte analyzer stages to 50,761,605 WVO bytes in
50 resources; every resource and the 624-byte manifest match the trusted
predecessor. The segmented reconstruction owner passes 5/5. This is a bounded
compiler-workflow repair and does not expand or complete Slice 8 semantics.

## Removal checkpoint

Seed removal occurs only when:

- every maintained repository `.wv` file begins with an explicit Language 1.0
  source descriptor;
- no current fixture or generator emits Seed source;
- editor and formatter emit an explicit selected Language 1.0 source profile;
- the compiler rebuilds itself from edition-1 source through the accepted native
  front door;
- required Foundation and runtime surfaces are qualified;
- both permanent hosts pass the named migration gate; and
- the exact recovery release for historical Seed remains documented.

At that checkpoint, remove Seed parser branches, aliases, obsolete WVB encodings,
and migration-only tests from current `main` unless a named recovery case retains
one. Do not leave dead compatibility code as a second compiler.

## Rollback and recovery

Migration rollback uses version control, the last qualified release, and a
separate restored workspace. It does not require current tools to accept
half-migrated or obsolete source.

A slice that fails qualification reverts or advances as one coherent semantic
unit. Generated artifacts and caches from the failed candidate are not promoted.
Historical evidence remains immutable and clearly labeled with its source and
format versions.

## Completion

The migration is complete only when:

- edition 1 is the sole current source edition;
- the compiler, Foundation, libraries, applications, and OS source use it;
- every implemented profile reports honest target support;
- current WVB and native paths agree with the frozen source semantics;
- no legacy source alias or parallel compiler remains;
- the paper corpus has become executable conformance coverage; and
- paired-host qualification records the final migration identities.
