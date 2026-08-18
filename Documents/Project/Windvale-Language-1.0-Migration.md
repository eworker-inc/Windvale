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
It does not authorize implementation before the Language 1.0 source-freeze
decision. It defines how the repository will advance once the
[semantic specification](../../Specifications/Windvale-Language-1.0.md),
[grammar](../../Specifications/Windvale-Language-1.0-Grammar.md),
[machine grammar](../../Specifications/Windvale-Language-1.0.ebnf),
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
10. the change-aware verifier maps every affected boundary to a focused owner.

The source freeze may permit staged implementation. It may not leave implemented
features semantically target-dependent.

## Compatibility boundary

Seed and edition 1 are distinct source editions. The active-development policy
allows one planned repository transition:

- current tools continue accepting Seed until the edition-1 front door reaches
  the agreed migration checkpoint;
- the compiler then accepts explicit `edition 1;` source through the new path;
- repository modules migrate in dependency order;
- once the last required repository module and fixture migrates, the current
  front door removes Seed parsing unless a named recovery case says otherwise;
- no source file is guessed as Seed or edition 1;
- no keyword, profile, positional constructor, or result-propagation alias is
  retained merely for convenience; and
- historical source remains buildable only from its pinned release or restored
  recovery workspace.

A temporary development compiler may accept both explicit editions while the
repository is migrating. That is one versioned front door with two explicit
grammars, not two compiler architectures. The overlap has a named removal
checkpoint and is not a product compatibility promise.

## Source mapping

| Seed contract | Edition-1 migration |
| --- | --- |
| No source edition declaration | Add `edition 1;` as the first declaration. |
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
| Edition header, module metadata, and target-scope registry | Lexer, declaration parser, source graph, build target admission, editor | Only when serialized metadata requires a new format. |
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
- Record baseline compiler time, verification time, memory, WIR size, WVB size,
  and representative application artifact size.

### Slice 1: edition, metadata, and naming

- Add explicit edition dispatch.
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

## Removal checkpoint

Seed removal occurs only when:

- every maintained repository `.wv` file declares edition 1;
- no current fixture or generator emits Seed source;
- editor and formatter default to edition 1;
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
