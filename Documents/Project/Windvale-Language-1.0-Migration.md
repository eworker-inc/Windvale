# Windvale Seed to Language 1.0 migration plan

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
| Function values and closures | Type checker, capture analysis, WIR, runtime/native calling convention | Likely requires versioned callable-value representation. |
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

## Current Slice 4/5 recoverable Vector-append checkpoint

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
