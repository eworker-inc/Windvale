# Windvale development roadmap

## Active goal

Evolve Windvale from the qualified C# Stage 0 and portable bytecode foundation into a small, understandable, increasingly self-hosted computing stack. Build useful Windvale-written binary tools and an explicit Foundation library first; then grow the language, compiler, assembler, object model, linker, native backend, and reproducible bootstrap; finally boot a minimal virtual-machine operating system that can load and run the same verified Windvale modules used on Windows and Linux.

The destination is stable, but the route is not frozen. An intermediate design may be revised or replaced when implementation evidence shows that it is impractical or that a materially better alternative is available. Consequential changes require an updated specification or an accepted decision, preserved verification evidence, and a clear migration of current fixtures. Adaptability must not weaken deterministic semantics, mandatory verification, explicit platform boundaries, or the end-to-end portability proof.

## Status

This roadmap expresses the active long-term goal and its current best route. The destination is durable; intermediate phases are adaptable. When experiments reveal an impractical contract or a clearly better alternative, update the relevant specification or decision and revise this roadmap rather than preserving accidental early designs.

## Sequencing principle

Windvale remains bytecode-first for as long as that reduces bootstrap loops. A new Windvale-written tool should become useful and reproducible on Windows and Linux before Windvale OS depends on it. Portable logic remains separate from hosted I/O, and each qualified phase requires deterministic artifacts, mandatory verification, adversarial coverage, and real cross-host evidence.

## Phases

| Phase | Deliverable and qualification gate | Status |
| --- | --- | --- |
| 0. Seed and byte primitives | C# Stage 0, typed WIR, verified runtime, `u8`, `u32`, immutable bytes, and Windows/Debian equality. | Qualified |
| 1. `Wvˉdumpˉcore` | Windvale source safely walks complete WVB headers and section envelopes over supplied bytes, including hostile lengths and malformed cases. | Qualified |
| 2. Structured inspection | Add only the records, enums, structured results/errors, and bounded formatting demanded by useful section descriptions. | Qualified |
| 3. Hosted resource boundary | Explicit arguments, file-byte input, diagnostics, and output capabilities with portable parsing kept independent. | Qualified |
| 4. Useful `wvdump` | Inspect the same real modules identically on Windows and Debian with golden machine-readable reports. | Qualified |
| 5. Object foundation | Deterministic byte construction, sections, symbols, relocations, and the smallest shared object contracts needed by an assembler. | Qualified |
| 6. Assembler and linker | Windvale-written assembler and linker running first as verified bytecode on Windows and Linux. | Current focus |
| 7. Foundation modules | Compact reusable collections, text, binary-format, diagnostics, testing, and I/O-adapter modules driven by tool needs. | Planned |
| 8. Self-hosted compiler | Windvale-written lexer, parser, semantics, and code generation for a meaningful subset, followed by a reproducible bootstrap closure. | Planned |
| 9. Native backend | Native WIR lowering, first x86-64 subset, calling convention, object output, and bytecode/native differential tests. | Planned |
| 10. Native host tools | Produce and qualify native Windvale programs in controlled Windows and Linux environments. | Planned |
| 11. Boot path and kernel | x86-64 UEFI/QEMU boot, diagnostics, memory foundation, minimal kernel boundary, and Hyper-V qualification. | Planned |
| 12. Runtime on Windvale OS | Load, verify, and run one identical Windvale module across Windows, Linux, and Windvale OS. | Planned |
| 13. Public foundation | Reproducible recovery bootstrap, security limits, licensing, governance, contribution rules, and public-release criteria. | Planned |

## Detailed execution plan

### Phase 6 - assembler and linker

Phase 6 is split so that parsing, object production, and link semantics can fail or evolve independently.

| Gate | Deliverable | Qualification evidence |
| --- | --- | --- |
| 6A. WVA contract oracle | Versioned WVA 1 grammar, strict Stage 0 parser, x86-64 encoder, independent WVO verification, and canonical examples. | Qualified on Windows and Debian at `3bfc6bb`; exact object bytes agree. |
| 6B. Windvale source scanner | A Windvale-written bounded UTF-8/line/token scanner that recognizes WVA 1 without host text parsing. | Qualified on Windows and Debian at `e5fd109`; exact module bytes and hosted reports agree. |
| 6C. Windvale semantic inspector | Multi-pass symbol, section, definition, statement, reference, ordering, and limit validation expressed in verified bytecode. | Qualified on Windows and Debian at `cc57bf9`; exact module bytes, accepted/rejected classifications, and hosted reports agree. |
| 6D. Windvale object encoder | Instruction/data encoding, derived offsets and sizes, symbol records, and relocations emitted as WVO 1.0. | Qualified on Windows and Debian at `a689617`; canonical, boundary, complete-statement, register, multi-definition, line-ending, empty, and accepted mutation outputs are byte-for-byte identical to Stage 0 and pass the independent WVO verifier. |
| 6E. Hosted assembler shell | Explicit input/output arguments and byte capabilities around a portable assembler core; output is written only after complete validation. | Qualified on Windows and Debian at `a689617`; real CLI output agrees, rejected input invokes no writer, and native failure cases leave no new or modified object. |
| 6F. Linker contract and oracle | A separate link specification covering inputs, duplicate/undefined symbols, layout, alignment, relocation arithmetic, limits, map output, and the first flat-image target. | Qualified on Windows and Debian at `9c4b9f5`; 31 tests, real multi-object CLI output, exact image/map bytes, hostile objects, all resolution failures, aggregate/map limits, layout/address overflow, both relocation overflows, independent image reconstruction, and no-output failures agree. |
| 6G. Windvale linker | A Windvale-written verified-bytecode linker implementing the accepted contract. | WVB 1.6 SHA-256 identity and deterministic bounded file snapshots are qualified at `348c82a`; balanced persistent bytes for practical image assembly are qualified at `89ce80b`; the complete Windvale WVO scanner and immutable object views are implemented with cross-host qualification pending; resolution, layout, relocation, independent reconstruction, map construction, and publish-after-success output remain. |

Phase 6 is complete only after 6G. A parser demo, hard-coded object producer, or host-only wrapper is useful evidence but is not a substitute for the accepted assembler or linker.

### Phase 7 - Foundation modules driven by real tools

1. Identify duplicated bounded scanning, byte construction, name validation, diagnostics, result/status, and test behavior in the qualified assembler and linker.
2. Introduce the smallest module/import and collection facilities needed to express those reusable contracts without hidden mutation or unbounded allocation.
3. Extract one capability at a time into explicit Foundation modules while preserving exact tool outputs.
4. Keep portable algorithms independent from hosted file, argument, console, clock, environment, and process behavior.
5. Add module-level conformance suites, resource limits, ownership rules, and deterministic serialization tests.
6. Publish a compact Foundation surface only after at least two real consumers justify each shared abstraction.

The completion gate is a documented, versioned Foundation layer used by the assembler and linker on both hosts, not a speculative general-purpose standard library.

### Phase 8 - self-hosted compiler

1. Freeze the meaningful compiler subset required to compile its own lexer, parser, semantic model, and bytecode encoder.
2. Add language facilities only from concrete compiler pressure: likely bounded collections, richer aggregates, explicit result/error flow, and controlled memory ownership.
3. Build a Windvale lexer and parser that reproduce Stage 0 syntax decisions over the accepted subset.
4. Build name/type/control-flow semantics and typed WIR construction with independent validation.
5. Emit canonical WVB and compare decoded structure, verifier results, runtime behavior, and exact bytes where canonicalization promises equality.
6. Compile the compiler with Stage 0, compile it again with the Windvale compiler, and compare the defined bootstrap artifacts.
7. Preserve the C# implementation as a documented recovery bootstrap until a separate decision proves that removing it improves recoverability.

The completion gate is a reproducible bootstrap closure on Windows and Debian, including a clean-environment recovery procedure and exact dependency inventory.

### Phase 9 - native backend

1. Define the x86-64 calling convention, value representation, stack discipline, register ownership, traps, and portable/native semantic equivalence rules.
2. Extend WIR and WVA only with operations demanded by measured native lowering cases, including internal control flow and address materialization.
3. Lower a small pure subset to WVO through the same object contract used by handwritten assembly.
4. Add executable layout or platform-container output in the linker through explicit target adapters rather than host conditionals in portable code.
5. Differentially run the same programs in the verified bytecode runtime and native sandbox, comparing results, diagnostics, traps, and resource-boundary behavior.
6. Expand the subset through integers, calls, aggregates, memory, and hosted bridges only after each preceding slice is qualified.

The completion gate is deterministic native output and semantic agreement for a documented subset; full language coverage is not required yet.

### Phase 10 - native host tools

1. Produce native assembler, linker, inspector, and selected compiler artifacts from the qualified backend.
2. Define narrow Windows and Linux host adapters for files, arguments, diagnostics, memory, and process exit behavior.
3. Keep portable tool cores identical and test adapters through shared capability contracts.
4. Rebuild representative artifacts with bytecode-hosted and native-hosted tools and compare the promised outputs.
5. Document every remaining dependency on .NET, system libraries, platform loaders, firmware tooling, or external build utilities.

The completion gate is a controlled native toolchain on both hosts with no silent semantic fork from the bytecode implementation.

### Phase 11 - boot path and minimal kernel

1. Record the first firmware, machine, image, and calling-convention decisions; x86-64 UEFI plus QEMU remains the leading proposal until accepted.
2. Make the linker produce the smallest bootable image format through a dedicated target adapter.
3. Boot to deterministic serial diagnostics, then add memory-map capture, page allocation, traps, and shutdown one bounded slice at a time.
4. Port the verifier and bytecode runtime behind system-profile capabilities rather than adding a kernel-specific language dialect.
5. Define the first package/resource source and load one embedded or image-contained verified module.
6. Automate QEMU success, failure, timeout, serial transcript, and image-digest evidence.
7. Qualify the accepted image under Hyper-V after QEMU automation is stable, documenting firmware or device differences explicitly.

The completion gate is a reproducible VM image that boots, reports machine-readable status, runs a verified module, and shuts down cleanly. A desktop, network stack, and broad device support remain later work.

### Phase 12 - one module across three environments

1. Select one non-trivial portable module with deterministic inputs, output, failure behavior, and bounded resource use.
2. Package the exact same verified WVB bytes for Windows, Linux, and Windvale OS.
3. Run the module through equivalent capability contracts and capture machine-readable reports.
4. Compare module digest, verifier result, return value, output bytes, diagnostics, and defined resource counters.
5. Treat any host-specific observable difference as either a defect or a proposed contract change requiring a recorded decision.

The completion gate is the central Windvale portability proof: one module artifact, three environments, one specified result.

### Phase 13 - public foundation

1. Select and add the source license; no earlier open-source-intent statement substitutes for it.
2. Publish the recovery bootstrap, pinned prerequisites, artifact provenance, cross-host qualification procedure, and release manifests.
3. Define contribution, review, AI-assistance disclosure, security-reporting, supported-version, and vulnerability-response policies.
4. Audit parsers, verifiers, resource limits, capability authorization, hostile inputs, and reproducible builds against the public threat model.
5. Separate stable public contracts from experimental ones and label compatibility expectations precisely.
6. Prepare small tutorials that build from source language to bytecode, object, linked image, and the VM demonstration without hiding bootstrap dependencies.

The completion gate is a source release that another person can inspect, build, verify, and recover from documented inputs.

## Cross-cutting qualification rules

Every gate that changes portable semantics or serialized bytes must provide:

- An accepted or explicitly experimental contract with strict limits and ownership boundaries.
- Positive, boundary, malformed, adversarial, and determinism coverage proportional to its attack surface.
- Independent verification before execution or artifact publication.
- Exact Windows and real Debian evidence from the same committed source archive.
- Digests for compared source archives, reports, and binary artifacts.
- No timestamps, machine paths, locale, host newline conventions, or unordered host collections in canonical output.
- Updated current fixtures rather than compatibility readers for obsolete development formats.
- A short decision record when evidence changes architecture, semantics, or phase order.

Documentation-only planning changes require repository hygiene checks but do not manufacture qualification evidence. A milestone status changes to **Qualified** only after its implementation and cross-host evidence are committed.

## Decision checkpoints

The following choices are intentionally deferred until the preceding experiment supplies evidence:

- The ergonomic assembly layer waits until canonical WVA and linker pressure reveal whether sorting, expressions, labels, or macros belong in WVA or in a source layer above it.
- Collection and memory facilities wait for concrete assembler, linker, and compiler algorithms rather than being designed as an abstract standard library exercise.
- The permanent bytecode shape waits for self-hosted compiler experience; versioned development formats may break before the public stability decision.
- The native ABI waits for bytecode/native differential cases and the first linked image requirements.
- PE, ELF, UEFI, and flat-image target priority waits for the native-tool and boot experiments; target adapters must not redefine portable language behavior.
- The kernel/process boundary waits for the smallest successful verified-runtime boot experiment.
- Public compatibility and support windows wait for the licensed release foundation.

At each checkpoint the project may keep, revise, or replace the proposed mechanism. It may not silently lower the verification gate or declare a narrower demonstration to be the original milestone.

## Current focus

Phase 6's WVA 1 Stage 0 contract is qualified at `3bfc6bb`, its Windvale-written bounded scanner at `e5fd109`, the complete semantic inspector at `cc57bf9`, the object encoder plus hosted assembler shell at `a689617`, and the separately owned Windvale Linking 1 contract plus C# Stage 0 `flat-x86-64-v1` oracle at `9c4b9f5`. The exact linker archive passed 31 tests and real CLI validation on Windows and Debian; both hosts produced identical provider objects, 24-byte images, 1,721-byte canonical maps, and normalized reports. Gate 6G is in progress. WVB 1.6 SHA-256 identity, first-read immutable file snapshots, and the exact launcher capacity needed by the full 64-input contract are qualified at `348c82a`; balanced persistent bytes for efficient immutable image construction and patching are qualified at `89ce80b`. The next implementation work is the Windvale WVO parser and multi-pass resolution/layout core. Phase 6 remains incomplete until the Windvale-written linker produces identical images and maps on both hosts.
