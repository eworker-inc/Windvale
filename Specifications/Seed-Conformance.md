# Windvale Seed conformance

## Purpose

The Seed conformance suite proves that the Windvale compiler, bytecode verifier,
runtime, object model, assembler, and linker agree on one portable contract. Its
current owners are native Windows/Linux applications and repository source.

## Required checks

- Portable code-and-data compilation and execution
- Hosted capability declaration, refusal without authorization, and successful authorized output
- Ordered hosted arguments, bounded native file input and output, separate output and diagnostic sinks, unsupported-host refusal, stable resource failures, and host-result validation
- Exact deterministic module bytes and canonical declaration ordering
- Bounded compile-time source-module composition, transitive nominal record/enum use, dependency-order independence, profile-compatible capability-bearing libraries, explicit transitive capability approval, root-only WVB exports, dependency semantic isolation, source-specific diagnostics, graph/shape rejection, and no-partial-output CLI behavior
- Standalone and composed `Foundationˉmachineˉcontracts` validation, exact alignment/name boundaries, two real tool consumers, dependency internalization, and unchanged assembler/linker outputs
- Standalone and composed `Foundationˉbyteˉordering` validation, exact ordinal span boundaries, three real tool consumers, and preserved tool ceilings and outputs
- Standalone and composed `Foundationˉdecimalˉparsing` validation, imported nominal results, exact range/digit/overflow boundaries, assembler/linker consumers, and unchanged binary outputs
- Standalone and composed `Foundationˉbyteˉconstruction` validation, exact 4 MiB repeat and total replacement boundaries, assembler/linker consumers, and preserved tool outputs and ceilings
- Composed Windvale-written Seed lexer validation covering the complete keyword/operator identities, streaming cursors, UTF-8 byte spans, UTF-16-compatible positions, integer suffix/range behavior, strict string escapes, surrogate pairs, and bounded malformed cases
- Windvale-written declaration parsing over streaming source views, including every top-level declaration shape, data/type/literal boundaries, import ordering, flat failure evidence, balanced body spans, real lexer parsing, and declaration-parser self-parsing
- Windvale-written statement/expression parsing over flat child-span views, including complete Stage 0 precedence and statement grammar, exact body boundaries, nesting/item limits, real lexer/declaration parsing, and body-parser self-parsing
- Canonical WVSS 1 source-set scanning and views, including malformed envelopes, exact layout, source/body validation, duplicate/order/profile/shape rejection, the 64-module boundary, snapshot reuse, and real frontend-set validation
- Windvale-written source-graph validation, including exact-name resolution, duplicate/missing imports, root reachability, direct/self cycles, stable failure evidence, the 64-module/63-edge chain, snapshot reuse, and the real compiler closure
- Windvale-written declaration and signature binding, including namespace and constructor conflicts, capability policy, aggregate limits, record/enum/parameter uniqueness, transitive type visibility, canonical nominal indices, independent packed-directory validation, stable failure evidence, and the real compiler closure
- Windvale-written body, local, and call binding, including stable slots/scopes, initializer visibility, whole-function uniqueness, mutability, local types, data/name/call visibility, intrinsic/constructor/function/capability arity, independent WVLB validation, stable failure evidence, and the real compiler closure below its fixed instruction ceiling
- Exact codec read/write round trips
- Inspector metadata and disassembly
- Functions, `if`, `while`, booleans, immutable text, immutable integer data, indexing, and `length`
- `u8`, `u32`, immutable `bytes`, slice views, bounded unsigned and signed little-endian reads, fixed-width byte construction, immutable concatenation, strict UTF-8 encoding, and explicit `u8` widening
- Strict UTF-8 validation/decoding, ASCII-safe deterministic quoting, and decoded/quoted text-limit traps
- Immutable nominal record construction, field access, function parameters/results, canonical encoding, and verifier rejection cases
- Nominal enum constants, record fields, equality, declared names, canonical encoding, and verifier rejection cases
- Invariant signed and unsigned integer formatting plus bounded text-concatenation traps
- Windvale-written WVB envelope and payload decoding with structured results plus valid, wrong-kind, nonzero-flags, hostile-length, truncated, trailing-byte, bad-payload, and unknown-opcode cases
- Canonical WVO 1.0 sections, symbols, relocations, strict decoding, deterministic encoding, malformed-input rejection, and bounded random object inputs
- Exact byte equality between the Windvale-written WVO producer and the independent C# object oracle, including native hosted-file persistence
- WVA 1 parsing, stable diagnostics, canonical symbol/section requirements, the complete initial x86-64 instruction/data subset, inferred definition ranges, deterministic WVO output, and bounded random source
- Windvale-written WVA scanning, multi-pass semantics, exact object measurement, instruction/data encoding, definition ranges, canonical section/symbol/relocation records, and whole-object hosted persistence
- Complete byte equality between Windvale and Stage 0 for canonical, numeric-boundary, all 8/16/32/64-bit register families, byte/word extension and condition forms, typed RIP/SIB memory, line-ending, multi-definition, empty-object, and accepted deterministic-mutation cases; rejected width, immediate, register, count, memory, and context inputs invoke no writer and produce no native output
- Windvale-written complete WVO structural validation, immutable section/symbol/relocation views, representative-object acceptance, deterministic mutation and random-byte differential classification, capability refusal, and real hosted scanning
- Windvale-written aggregate validation, duplicate-export detection, import-kind resolution, exported-function entry selection, actual-address alignment, deterministic section placement, defined-symbol address validation, and exact analysis comparison with Stage 0
- Windvale-written immutable padding/data/zero-fill construction, local/export/import target address resolution, checked absolute and relative relocation arithmetic, persistent four-byte patching, and exact candidate-image SHA-256 comparison with Stage 0
- Independently structured actual-address placement, unrelocated-image reconstruction, full export rescanning, reverse-order relocation with separate arithmetic, byte-for-byte candidate comparison, and injected `WVL1011` mismatch rejection
- Windvale-written canonical map construction over once-validated immutable snapshots, exact 1 MiB enforcement, `WVL1012` rejection, and one publish-after-success image write only after independent reconstruction and complete map success
- Windvale Linking 1 input validation, unique export resolution, strict import-kind matching, required exported-function entry, canonical kind/input/source layout, actual-address alignment, zero padding and BSS, checked `absolute-u32` and `relative-i32` application, and complete independent image reconstruction
- Exact deterministic flat-image and canonical map bytes, including input-order sensitivity, aggregate limits, map limits, malformed objects, undefined and duplicate symbols, address and relocation overflow, no-output rejection, and real multi-object CLI linking
- Checked `u32` overflow and underflow plus byte-read and slice bounds traps
- U+02C9 source identifiers, immutable `let`, mutable `var`, immutable parameters, and exported `Main`
- Rejection of malformed or confusable identifier separators
- Stable source diagnostic codes with line and column information
- Malformed header, version, section, length, UTF-8, truncation, trailing-data, and oversize rejection
- Unknown opcode, truncated operand, invalid branch, invalid local, unreachable instruction, inconsistent stack merge, and maximum-stack rejection
- Runtime integer-overflow, data-bounds, instruction-limit, call-depth, capability-authorization, argument-bound, file-resource, and invalid-host-result traps
- Deterministic bounded random source and module inputs remain contained by result or diagnostic boundaries

## Host verification

Ordinary changes use `Tools/Verify/Verify-Changed.ps1`. Its native dependency
planner selects the affected fixed suites, verification-plan checks,
WebAssembly owner, or workflow audit and refuses uncovered active boundaries.
Run it once after a coherent edit rather than running increasingly broad levels
for the same source state.

One named native owner can be selected directly with:

```powershell
pwsh -NoProfile -File Tools/Verify/Invoke-WindvaleTests.ps1 -Owner <owner-name>
```

The complete fixed manifest, WebAssembly owner, and compiler convergence run on
both permanent hosts only for an explicitly selected qualification state. GitHub
shards the native manifest without changing its exact owner inventory or
aggregate result.

## Comparing hosts

Cross-host conformance is established by running the same committed manifests,
fixed inputs, expected reports, and exact artifact identities on Windows and the
pinned Debian environment. Portable format versions, normalized outputs, result
codes, diagnostics, object/link products, and deterministic bytes must agree
where the contract declares them portable. Host labels, paths, process details,
and elapsed time are evidence metadata rather than portable values.

Historical managed comparison reports remain in the immutable Stage 0 recovery
release. They may diagnose an old contract but are not a live oracle for forward
source or repository layout.

## Evidence discipline

A Windows pass proves Windows behavior; a Linux pass proves Linux behavior. A
cross-host qualification claim requires both results from the same exact commit.
Focused development feedback permits integration but must not be cited as a
complete conformance or release claim.
