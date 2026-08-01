# Windvale development roadmap

## Active goal

Evolve Windvale from the qualified C# Stage 0 and portable bytecode foundation into a small, understandable, self-hosted computing stack whose normal Windows, Linux, and Windvale OS workflows require no .NET dependency. Build useful Windvale-written binary tools and an explicit Foundation library first; then grow the language, compiler, assembler, object model, linker, shared JIT/AOT native backend, runtime, memory system, and reproducible bootstrap; finally boot a minimal virtual-machine operating system that can load and run the same verified Windvale modules used on native Windows and Linux hosts.

The destination is stable, but the route is not frozen. An intermediate design may be revised or replaced when implementation evidence shows that it is impractical or that a materially better alternative is available. Consequential changes require an updated specification or an accepted decision, preserved verification evidence, and a clear migration of current fixtures. Adaptability must not weaken deterministic semantics, mandatory verification, explicit platform boundaries, or the end-to-end portability proof.

## Status

This roadmap expresses the active long-term goal and its current best route. The destination is durable; intermediate phases are adaptable. When experiments reveal an impractical contract or a clearly better alternative, update the relevant specification or decision and revise this roadmap rather than preserving accidental early designs.

## Sequencing principle

Windvale remains bytecode-first for as long as that reduces bootstrap loops. A new Windvale-written tool should become useful and reproducible on Windows and Linux before Windvale OS depends on it. Portable logic remains separate from hosted I/O, and each qualified phase requires deterministic artifacts, mandatory verification, adversarial coverage, and real cross-host evidence. C#/.NET remains the active reference and recovery path until [Decision 0057's native-retirement gate](../Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md#native-retirement-gate); after that gate it leaves normal automation rather than becoming a permanent host dependency.

## Phases

| Phase | Deliverable and qualification gate | Status |
| --- | --- | --- |
| 0. Seed and byte primitives | C# Stage 0, typed WIR, verified runtime, `u8`, `u32`, immutable bytes, and Windows/Debian equality. | Qualified |
| 1. `Wvˉdumpˉcore` | Windvale source safely walks complete WVB headers and section envelopes over supplied bytes, including hostile lengths and malformed cases. | Qualified |
| 2. Structured inspection | Add only the records, enums, structured results/errors, and bounded formatting demanded by useful section descriptions. | Qualified |
| 3. Hosted resource boundary | Explicit arguments, file-byte input, diagnostics, and output capabilities with portable parsing kept independent. | Qualified |
| 4. Useful `wvdump` | Inspect the same real modules identically on Windows and Debian with golden machine-readable reports. | Qualified |
| 5. Object foundation | Deterministic byte construction, sections, symbols, relocations, and the smallest shared object contracts needed by an assembler. | Qualified |
| 6. Assembler and linker | Windvale-written assembler and linker running first as verified bytecode on Windows and Linux. | Qualified |
| 7. Foundation modules | Compact reusable collections, text, binary-format, diagnostics, testing, and I/O-adapter modules driven by tool needs. | Current focus |
| 8. Self-hosted compiler | Windvale-written lexer, parser, semantics, and code generation for a meaningful subset, followed by a reproducible bootstrap closure. | Qualified bytecode self-reproduction on Windows and Debian |
| 9. Shared native backend | Native WIR/WVB lowering, x86-64 ABI, WVO/AOT output, baseline JIT, and interpreter/JIT/AOT differential tests. | ABI 9 candidate adds enums, immutable arena records, strict UTF-8, and the native Windvale `wvdump` structural parser; cross-host qualification is pending |
| 10. Native host tools and .NET retirement | Produce and qualify native Windvale tools, runtime, JIT/AOT execution, and bootstrap recovery on Windows and Linux without a normal .NET dependency. | Planned |
| 11. Boot path and kernel | x86-64 UEFI/QEMU boot, diagnostics, memory foundation, minimal kernel boundary, and Hyper-V qualification. | Firmware probe 11 candidate rebuilds through ABI 9/context 2; cross-host/pinned-QEMU qualification, guest loading, traps, runtime, shutdown, and Hyper-V remain |
| 12. Runtime on Windvale OS | Load, verify, and run one identical WVB through equivalent Windvale-native execution contracts across Windows, Linux, and Windvale OS. | Planned |
| 13. Public foundation | Reproducible recovery bootstrap, security limits, licensing, governance, contribution rules, and public-release criteria. | Repository policies prepared; private GitHub import, settings, and initial publication baseline pending |

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
| 6G. Windvale linker | A Windvale-written verified-bytecode linker implementing the accepted contract. | Qualified on Windows and Debian at `40ac57d`; the exact WVB, 24-byte image, 1,721-byte map, normalized contract, success publication, deterministic no-write failures, existing-output preservation, and host-write failure boundary agree. |

Phase 6 is complete only after 6G. A parser demo, hard-coded object producer, or host-only wrapper is useful evidence but is not a substitute for the accepted assembler or linker.

### Phase 7 - Foundation modules driven by real tools

The first enabling slice, bounded static source-module composition, is qualified on Windows and Debian at `df80f91` under Decision 0019. It deliberately changes neither WVB 1.6 nor runtime loading. The first two-consumer module, `Foundationˉmachineˉcontracts`, is cross-host qualified at `d46af86` under Decision 0020. The next measured extraction, `Foundationˉbyteˉordering`, is cross-host qualified at `4fdea22` under Decision 0021 for the object core, assembler, and linker. Static contracts with dependency records/enums and `Foundationˉdecimalˉparsing` are cross-host qualified together at `6d2a351` under Decisions 0022 and 0023. `Foundationˉbyteˉconstruction` is cross-host qualified at `26e2fd1` under Decision 0024; it replaces duplicated assembler/linker repeat and patch logic and supplies the immutable backpatching seam needed by a future WVB encoder.

1. Identify duplicated bounded scanning, byte construction, name validation, diagnostics, result/status, and test behavior in the qualified assembler and linker.
2. Introduce the smallest module/import and collection facilities needed to express those reusable contracts without hidden mutation or unbounded allocation.
3. Extract one capability at a time into explicit Foundation modules while preserving exact tool outputs.
4. Keep portable algorithms independent from hosted file, argument, console, clock, environment, and process behavior.
5. Add module-level conformance suites, resource limits, ownership rules, and deterministic serialization tests.
6. Publish a compact Foundation surface only after at least two real consumers justify each shared abstraction.

The completion gate is a documented, versioned Foundation layer used by the assembler and linker on both hosts, not a speculative general-purpose standard library.

### Phase 8 - self-hosted compiler

The first slice is cross-host qualified at `d91dbfb` under Decision 0025: a streaming Windvale-written lexer over immutable UTF-8 bytes. It preserves the complete implemented Seed keyword/operator identities, byte spans, UTF-16-compatible source positions, integer classification, strict string validation, and bounded failures without introducing a token collection. This intentionally overlaps Phase 7: parser pressure, rather than a speculative library roadmap, will determine the next collection or diagnostic facility.

The declaration pass is cross-host qualified at `fc87a3e` under Decision 0026. It parses module headers and complete top-level declaration shapes into streaming immutable source views, then identifies balanced function-body spans for the later statement pass. It parses both the real lexer and its own declaration source without a token/declaration collection.

The body parser is cross-host qualified at `ddfa9e3` under Decision 0027. It reproduces the complete Stage 0 statement/expression grammar as flat parent/child source views, validates the lexer, declaration parser, and itself, and still retains no syntax collection. The parser evidence did not justify a token, declaration, syntax, or recoverable-diagnostic collection. Semantic binding is the next pressure test; it starts with bounded rescanning and may introduce a packed node/index facility only when measured correctness, ownership, or performance evidence requires one.

Semantic input pressure produced WVSS 1, cross-host qualified at `00ef0b1` under Decision 0029. This compiler-owned packed byte contract carries one root plus canonically ordered dependencies, provides indexed immutable source views, validates every member with the qualified frontend, and preserves dependency profile/shape rules without exposing host paths or collections. Windows and Debian pass all 43 tests, including the 64-module boundary and the real five-module frontend set, with matching normalized reports and byte-identical direct artifacts. Its current 4 MiB aggregate limit is explicit. The later complete compiler closure uses 677,073 source bytes, so this limit is sufficient for bytecode self-hosting; parity with the Stage 0 source-set envelope remains a separate future contract decision.

Decision 0030's Windvale-written import graph is cross-host qualified at `09c6f54`. It resolves exact module names, rejects repeated and missing imports, computes the complete root closure, and proves acyclicity over WVSS without host collections. Windows and Debian pass all 44 tests, including an exact 64-module/63-edge chain and the real seven-module compiler closure, with matching normalized reports and byte-identical direct artifacts. Declaration namespaces and signature/body binding remain the next semantic slice.

Decision 0033's Windvale-written declaration/signature phase is cross-host qualified at `d57a6d8`. It enforces global namespace and capability policy, binds visible nominal signature types, assigns canonical nominal indices, and publishes an independently validated `WVSD 1` declaration directory plus a bounded transitive-visibility matrix. A repeated-rescan prototype exhausted 4,000,000,000 instructions on the real closure; the retained packed evidence removes that impractical path. Windows and Debian pass all 45 tests and the complete native CLI verifier with matching normalized reports and 42 byte-identical direct artifacts. Body/local/call binding and typed expression/control-flow semantics are next.

Decision 0034's Windvale-written body/local/call phase is cross-host qualified at `9185b28`. It assigns stable parameter/local slots and scopes, binds reads and assignments, resolves visible constructors/functions/capabilities and Foundation intrinsics, checks arity, and publishes an independently validated `WVLB 1` directory. Measured temporary-directory and per-candidate source-slicing variants exceeded the fixed 4,000,000,000-instruction ceiling; the retained packed-span design binds the real nine-module closure within that ceiling. Windows and Debian pass all 46 tests and the complete native CLI verifier with matching normalized reports and 45 byte-identical direct artifacts. Complete expression types, field/operator validation, control-flow proof, and typed WIR are next.

Decision 0035's Windvale-written typed source IR is cross-host qualified at `bf77f70`. It performs complete implemented expression typing, field/operator/call validation, explicit basic-block and temporary construction, return and reachability proof, and publication through an independently checked `WVIR 1` directory. Windows and Debian pass all 47 tests and the complete native verifier with matching normalized reports and 48 byte-identical direct artifacts. The control-heavy fixture remains fast, while full ten-module self-lowering stays outside the development loop until local discovery and IR construction share one body traversal under the unchanged instruction ceiling.

Decision 0036's first Windvale-written WVB backend is cross-host qualified and published at `d65d286`; its tree is byte-identical to exact qualified candidate `ca56996`. Decision 0037 extends that backend with canonical WVSD-to-WVB function/data translation, arbitrary valid declaration ordering, `[i32]`, text and bytes data, deterministic escaped-Unicode literal interning, and the primitive Foundation intrinsic surface. Exact commit `636627c` is cross-host qualified: the original four-function fixture remains byte-identical to Stage 0 and executes with result `6`, while the interleaved data/text fixture is also byte-identical to Stage 0, includes a synthetic-name collision and surrogate-pair escape, and executes with result `13`. At that decision boundary, nominal types, capabilities, imports, multi-module translation, and full bootstrap closure remained later expansions; Decision 0038 closes the nominal-type part only.

Decision 0038 adds canonical WVB Types serialization, nominal shapes in functions and compiler temporaries, immutable record construction/field access, and enum constants/equality/inequality/names. Exact commit `f39ff73` is cross-host qualified: its deliberately interleaved nominal fixture is byte-identical to Stage 0 and executes with result `11`, while the preceding primitive and data/text fixtures retain their exact identities and results. At that decision boundary, capabilities, imports, multi-module backend translation, and full bootstrap closure remained later expansions; Decision 0039 closes the capability/profile part only.

Decision 0039 preserves portable/hosted/system profiles, serializes the exact seven-entry Seed capability catalog in canonical name order, translates WVSD capability identities, and lowers WVIR capability calls. Exact commit `98117c1` is cross-host qualified: its deliberately unsorted hosted fixture is byte-identical to Stage 0, exposes all seven call indices, and executes its authorized no-argument path with result `0` without file mutation. At that decision boundary, imports, multi-module backend translation, and full bootstrap closure remained later expansions; Decision 0040 closes the static multi-module part only.

Decision 0040 lowers a complete validated WVSS graph to one ordinary WVB without adding runtime linkage. It resolves every global WVSD identity through its owner source, internalizes dependency functions and nominal types, preserves root data/profile/capabilities/exports, and discovers text literals across canonical global function order. Exact commit `cb1db23` is cross-host qualified: its three-module fixture is byte-identical to Stage 0, verifies, exposes only `Main`, and returns `42`; noncanonical dependency order produces no output. Source-envelope/performance closure and full compiler bootstrap closure remain later work.

Decision 0041 fuses parameter/local WVLB discovery with typed-WVIR construction in one successful-path statement traversal while preserving the standalone binding API and binding-error diagnostic oracles. Exact commit `b124115` is cross-host qualified: Windows and Debian pass all 48 tests and the complete native verifier, their normalized contracts match, and all 61 portable artifacts are byte-identical. The exact ten-module typed-IR input still reaches bounded diagnostic `WVR3011` at the unchanged 4,000,000,000-instruction ceiling, so remaining lookup/typed-lowering performance and full compiler self-hosting remain later work.

Decision 0042 bounds keyword, ordinary-identifier, and Unicode-whitespace dispatch in the Windvale lexer and adds opt-in per-function instruction reporting to the C# reference runtime and CLI. Exact commit `5d67463` is cross-host qualified: Windows and Debian pass all 48 tests and the complete native verifier, their normalized contracts match, and all 61 portable artifacts are byte-identical. The original fixed lexer workload falls by 28.2%, and the focused typed-WVIR workload falls by 29.0%. The exact ten-module input still reaches `WVR3011` at 4,000,000,000 instructions; structural symbol-directory and name-evidence work is next.

Decision 0050 keeps public `WVSD 1.0` unchanged and advances the private `WVSI` index to 1.1 with deterministic mappings between source-order directory entries and canonical nominal ordinals. Binding and typed-WVIR consumers use those mappings directly, packed directory scans avoid unsuccessful match materialization, and equality paths reject unequal byte lengths before comparison. The real nine-module binding closure falls from 2,972,056,275 to 2,600,859,185 instructions despite a larger source-derived workload. Exact commit `e37204f` is cross-host qualified: Windows and Debian pass all 48 tests and the complete native verifier, their normalized contracts match, and all 61 portable artifacts are byte-identical. The ten-module typed-WVIR input still exceeds four billion; repeated lexical/parser traversal is the next measured performance slice.

Decision 0055 reuses complete-source lexical and declaration evidence inside the compiler, retains checked standalone boundaries, replaces valid function-body token skipping with a bounded string/comment/brace scanner, contains over-deep checked body spans iteratively, and narrows nominal lookup through existing WVSI canonical ranges. The focused typed-WVIR fixture falls from 5,715,847 to 3,626,693 instructions. Exact commit `1a4fca7` is cross-host qualified: Windows and Debian pass all 48 tests and the complete native verifier, their normalized contracts match, and all 61 portable artifacts are byte-identical. The exact ten-module typed-WVIR input completes at 3,912,239,584 instructions under the unchanged four-billion ceiling, clearing the performance entry gate for Stage 0 → Stage 1 → Stage 2 convergence without yet claiming self-hosting.

Decision 0058 implements and qualifies reproducible compiler bootstrap at exact commit `5c16547`. Equality-only source lookups use reverse span equality, WVB emission builds immutable canonical entry/rank tables once per declaration kind, and accepted declaration offsets are consumed without rescanning module prefixes for line/column coordinates. The canonical 12-module inventory contains 677,073 source bytes. Stage 0 produces a verified 599,868-byte Stage 1 compiler; Stage 1 compiles the same inventory in 6,700,562,174 VM instructions and produces a verified, byte-identical Stage 2 compiler. Both artifacts have SHA-256 `9673bf3331763181f443ec67b7a513bc66daa718969f7f6b0d197a4186071066`. The dedicated verifier reconstructed this proof from the exact committed inventory on Windows and isolated Debian QA; both ordinary qualification suites, normalized reports, and all 61 portable artifacts also matched. This completes the Phase 8 bytecode self-hosting gate while leaving Decision 0057's native execution and .NET-retirement work to Phases 9 and 10.

1. Freeze the meaningful compiler subset required to compile its own lexer, parser, semantic model, and bytecode encoder.
2. Add language facilities only from concrete compiler pressure: likely bounded collections, richer aggregates, explicit result/error flow, and controlled memory ownership.
3. Build a Windvale lexer and parser that reproduce Stage 0 syntax decisions over the accepted subset.
4. Build name/type/control-flow semantics and typed WIR construction with independent validation.
5. Emit canonical WVB and compare decoded structure, verifier results, runtime behavior, and exact bytes where canonicalization promises equality.
6. Compile the compiler with Stage 0, compile it again with the Windvale compiler, and compare the defined self-hosting artifacts.
7. Preserve the C# implementation as the active reference/recovery compiler through convergence and the native-retirement gate. Archive its final source, dependencies, instructions, and exact evidence before it leaves normal automation under Decision 0057.

The completion gate is reproducible compiler self-hosting on Windows and Debian, including a clean-environment recovery procedure and exact dependency inventory.

### Phase 9 - shared native backend

1. Define the x86-64 calling convention, value representation, stack discipline, register ownership, traps, and portable/native semantic equivalence rules.
2. Define a structured native machine-IR or fragment boundary whose instruction selection, register assignment, encoding, and typed patches can serve WVO/AOT and in-memory JIT sinks.
3. Extend WIR, WVB lowering, and WVA only with operations demanded by measured native cases, including internal control flow, calls, data addressing, runtime services, and address materialization.
4. Lower a small verified pure WVB subset and the matching typed-WIR subset to WVO through the same object contract used by handwritten assembly.
5. Implement the first low-latency baseline-JIT experiment with WVA-generated machine stencils or another explicitly accepted mechanism, writable-or-executable publication, checked in-memory relocation, and bounded code-cache accounting.
6. Add PE/COFF, ELF, and later Windvale-native container output through explicit linker/loader target adapters rather than host conditionals in portable code.
7. Differentially run the same programs in the verified interpreter, baseline JIT, native sandbox, and AOT image, comparing acceptance, results, output, diagnostics, traps, capabilities, and defined resource counters.
8. Add content-addressed native caching, lazy compilation, compact micro-operations, an optimizing tier, or profile-guided AOT only after the preceding baseline supplies measurements and stable safety boundaries.
9. Expand through integers, calls, aggregates, memory, text, bytes, hosted bridges, and reclamation only after each preceding slice is qualified.

[Decision 0049](../Decisions/0049-First-Compiler-Generated-Windvale-Boot-Item.md) supplies an early bounded instance of steps 1, 3, and 4 for the special kernel-entry target: typed WIR lowers to verified code-only WVO, obeys handoff version 1, and links into the explicit UEFI adapter. It deliberately does not satisfy this phase's general ABI or bytecode/native differential gate.

[Decision 0059](../Decisions/0059-First-Shared-Native-Wvb-Slice.md) implements the first general instance of steps 2, 4, 5, and 7 for one constant-return program. A verified portable WVB lowers to explicit native operations, one versioned x86-64 fragment feeds both WVO/AOT and in-memory sinks, the runtime publishes memory writable-then-executable, and interpreter/JIT/AOT results agree on Windows and Debian x64 at exact commit `962bb85`. The 79-byte WVO and six code bytes are deterministic. Every wider operation remains open, so Phase 9 is not complete.

[Decision 0060](../Decisions/0060-Checked-Native-I32-Arithmetic-And-Traps.md) adds the first checked computation and recoverable native trap. Verified straight-line add, subtract, multiply, and negate lower through numbered machine-IR values into one bounded x86-64 frame. `jo` reaches a checked epilogue that returns packed overflow status without a host signal; the runtime translates it to `WVR3007`. The independent fragment decoder admits only the exact allowed instructions, initialized contiguous slots, overflow targets, and balanced epilogues. Exact commit `84dd908` is qualified on Windows and Debian x64: all 49 tests and the complete CLI verifier pass, normalized reports match, and all 61 portable artifacts are byte-identical. Boolean comparisons and structured control flow are the next Phase 9 slice.

[Decision 0061](../Decisions/0061-Typed-Native-Blocks-And-Forward-Control-Flow.md) replaces the straight-line operation list with typed locals, typed static values, canonical blocks, and explicit terminators. It lowers all signed i32 comparisons plus bool equality/inequality/negation, forward branches, early returns, and mutable frame-backed locals through the same WVO/AOT and W^X fragment. The strict decoder proves complete frame initialization, admitted instruction groups, forward boundary targets, reachability, and balanced exits. Exact commit `f0a53a9` is qualified on Windows and Debian x64: all 49 tests and the complete CLI verifier pass, normalized reports match, and all 61 portable artifacts are byte-identical. Backward edges intentionally fail until a native execution-budget or safe-point contract makes loops safe.

[Decision 0062](../Decisions/0062-Dynamic-Native-Instruction-Budgets-And-Backward-Control-Flow.md) gives each execution a positive dynamic instruction maximum and charges every lowered WVB instruction through a shared `RDX`/`R11` convention whose bytes are identical under Windows and System V x64. Packed status 2 maps to `WVR3011`; all control targets land on charge boundaries; cyclic reachability and both trap epilogues are independently decoded. Exact commit `2b67c8a` is qualified on Windows and Debian x64: finite JIT/AOT loops agree with the reference interpreter at the success and exhaustion boundary, a nonterminating loop is bounded, all 49 tests pass, normalized reports match, and all 61 portable artifacts are byte-identical.

[Decision 0063](../Decisions/0063-Shared-Budget-Native-Calls-And-Static-Data.md) extends that shared counter across a real function graph and adds a separate exact call-depth counter. The version-5 selector supports as many as four i32/bool parameters and results, nested and recursive calls, immutable i32 array length/load operations, recoverable depth and bounds traps, RIP-relative data patches, and deterministic WVO `.rodata`. One strict decoder verifies all functions, call edges, counter transitions, trap propagation, patches, symbol ranges, and reachable bytes before either sink. Exact commit `1af2eca` is qualified on Windows and Debian x64: interpreter/JIT/AOT success and resource boundaries agree, all 49 tests and the complete CLI verifier pass, normalized reports match, and all 61 portable artifacts are byte-identical.

[Decision 0064](../Decisions/0064-First-Shared-Native-Wvb-In-Windvale-Os.md) adopts ABI 5 in the first downstream OS consumer. One ordinary portable module compiles to canonical verified WVB, then shared native WVO, and executes internal calls, a bounded loop, and immutable `.rodata` on the kernel-owned stack before the special system-profile Main may complete. Exact candidate `708242e` passes all 15 OS tests, the 48-test Development tier, the 49-test Standard tier, the pinned QEMU/OVMF environment check, and the complete version-7 boot gate. This is AOT consumption and does not yet provide an in-guest WVB verifier or runtime loader.

[Decision 0065](../Decisions/0065-Versioned-Native-Execution-Context-And-Console-Service.md) advances the qualified target to ABI 6. One 32-byte versioned context replaces positional resource arguments and carries an optional 16-byte service table. The first closed service lowers immutable static UTF-8 through `console.write_line`, requires explicit authorization and implementation before W^X publication, uses identical generated bytes plus tiny runtime-owned Windows/System V thunks, and contains service failure as a packed trap. The OS bridge constructs the same context with an empty service table. Exact candidate `2fcf531` passes all 50 tests and complete CLI verification on Windows and Debian, byte-identical portable-artifact comparison, all 15 OS tests, and the pinned version-8 QEMU gate.

[Decision 0066](../Decisions/0066-Borrowed-Bytes-And-Unsigned-Native-Values.md) qualifies ABI 7's first compiler-tool data representation: immutable module bytes become pointer/length descriptors in zero-initialized 16-byte value cells; bounded slicing and fixed-width little-endian reads return `WVR3008` instead of a host fault; and `u8`/`u32` constants, comparisons, conversion, and checked arithmetic share the JIT/WVO selector. Up to four internal parameters may now include borrowed bytes, copied into the callee frame. The independent decoder rejects corrupt descriptors, argument forms, bounds branches, and scalar retyping. Exact candidate `8d375bf` passes complete Windows/Debian qualification, byte-identical portable-artifact comparison, all 15 OS tests on both hosts, and the pinned firmware-probe-9 QEMU gate.

[Decision 0067](../Decisions/0067-Borrowed-Hosted-Input-And-First-Native-Wvb-Inspector.md) qualifies ABI 8's first hosted input boundary at exact candidate `d970c27`. Borrowed text and bytes share an execution-bounded descriptor shape; service-table version 2 admits explicitly authorized argument count, argument text, file snapshot input, and console output. The checked-in `Wvb-Header-Inspector.wv` reads a real compiler-produced WVB and validates `WVB1`/version `1.6` identically under the reference interpreter and real Windows/System V W^X paths. All 52 tests and 61 portable artifacts agree across Windows and Debian, and firmware probe 10 passes pinned QEMU. Full `wvdump` still needs native nominal aggregates, bounded dynamic text formatting, and diagnostic policy.

[Decision 0068](../Decisions/0068-Bounded-Native-Nominal-Values-And-Wvdump-Structural-Core.md) defines the ABI-9 candidate. Enums use canonical dword values; immutable records use checked offsets in one 1 MiB execution arena; service-table version 3 adds pure strict UTF-8 validation. The existing structural portion of `Wv-Dump-Core.wv` now validates complete envelope and payload fixtures identically under the reference interpreter, W^X JIT, and linked WVO/AOT. The full 54-test Windows Seed suite and all 15 focused OS tests pass. Cross-host and pinned-QEMU qualification remain pending; full `wvdump` output still needs bounded dynamic text, descriptor returns, void calls, and diagnostic policy.

The completion gate is deterministic native AOT output, a qualified baseline-JIT path, and interpreter/JIT/AOT semantic agreement for a documented WVB subset on Windows and Linux. Full language coverage and an optimizing tier are not required yet.

### Phase 10 - native host tools and .NET retirement

1. Produce native compiler, semantic WVB verifier, interpreter/baseline JIT, assembler, linker, inspector, test runner, and build-driver artifacts from the qualified backend.
2. Define the native value representation, allocation/reclamation boundary, runtime-service table, traps, process entry, and narrow Windows/Linux adapters for executable memory, files, arguments, diagnostics, and exit behavior.
3. Keep portable tool cores identical and test adapters through shared capability contracts and a Windvale-owned internal ABI with small platform thunks.
4. Rebuild representative artifacts with the .NET-hosted reference path and native Windvale tools, comparing every promised output; then prove Stage 1 and Stage 2 through the native path.
5. Run repository verification, packaging, and clean-environment recovery on both hosts without invoking .NET. Inventory every remaining system library, platform loader, firmware tool, or external build utility.
6. Archive the final qualified .NET Stage 0 release and publish the native seed identity, provenance, previous-compiler bootstrap procedure, and rollback path.
7. Remove .NET from the normal build, test, packaging, release, and execution automation only when every Decision 0057 retirement condition passes from one committed source state.

The completion gate is a controlled and recoverable Windvale-native toolchain on Windows and Linux with no silent semantic fork, no normal .NET invocation, and matching native bootstrap evidence. This retires .NET as a dependency without erasing the Stage 0 historical record.

### Phase 11 - boot path and minimal kernel

1. Use the accepted Decision 0044 x86-64 UEFI 2.11, pinned QEMU Q35/TCG, and exact EDK II environment; record the first deterministic image and internal calling-convention decisions from boot evidence.
2. Make the linker produce the smallest bootable image format through a dedicated target adapter.
3. Boot to deterministic serial diagnostics, then add memory-map capture, page allocation, traps, and shutdown one bounded slice at a time.
4. Port the semantic WVB verifier and initial native interpreter behind system-profile capabilities rather than adding a kernel-specific language dialect. Keep later JIT compilation in user space or an isolated system service; kernel and driver code remain AOT.
5. Define the first package/resource source and load one embedded or image-contained verified module.
6. Automate QEMU success, failure, timeout, serial transcript, and image-digest evidence.
7. Qualify the accepted image under Hyper-V after QEMU automation is stable, documenting firmware or device differences explicitly.

Decisions 0044 through 0049 complete the environment, image, firmware-exit, handoff, and first compiler-generated boot slices. Decision 0052 completes the first memory part of step 3: firmware probe version 6 claims only one 64 KiB conventional-memory arena, exercises a zeroing page allocator, copies the handoff, and runs compiler-generated Main on an 8 KiB owned stack under exact QEMU evidence. Decisions 0054 and 0056 establish the bidirectional WVA/WV execution seam, move memory-through-Hello evidence into `.wv`, and assign future machine mechanics to WVA and kernel policy to Windvale source. Decisions 0064 through 0067 advance the qualified shared consumer through ABI 8 and firmware probe 10. Decision 0068 assigns firmware probe 11 to the ABI-9/context-2 rebuild without exposing host services or a record arena in the guest. This is host-built AOT evidence; in-guest WVB verification/loading, general reclamation, paging, traps, shutdown, and Hyper-V remain open.

The completion gate is a reproducible VM image that boots, reports machine-readable status, runs a verified module, and shuts down cleanly. A desktop, network stack, and broad device support remain later work.

### Phase 12 - one module across three environments

1. Select one non-trivial portable module with deterministic inputs, output, failure behavior, and bounded resource use.
2. Package the exact same verified WVB bytes for Windows, Linux, and Windvale OS.
3. Run the module through equivalent Windvale-native capability contracts. Record interpreter, baseline-JIT, cached/install-time, or AOT mode explicitly rather than allowing the tier to change observable semantics.
4. Compare module digest, verifier result, return value, output bytes, diagnostics, native ABI/runtime versions, and defined resource counters.
5. Treat any host-specific observable difference as either a defect or a proposed contract change requiring a recorded decision.

The completion gate is the central Windvale portability proof: one module artifact, three environments, one specified result.

### Phase 13 - public foundation

1. Keep the accepted MIT license, [E-Worker Inc](https://eworker.ca) stewardship, vendor-neutral AI authorship, and public contribution foundation visible in source distributions; [Decisions 0028](../Decisions/0028-MIT-License-And-E-Worker-Stewardship.md), [0031](../Decisions/0031-AI-Authorship-And-Vendor-Neutrality.md), and [0032](../Decisions/0032-Public-Contribution-And-Governance-Foundation.md) define the current policy.
2. Publish the recovery bootstrap, pinned prerequisites, artifact provenance, cross-host qualification procedure, and release manifests.
3. Apply the repository-wide AI-authorship default, recording a specific model or vendor only when technically material to reproducibility, qualification, or a third-party obligation.
4. Publish contribution, review, security, support, conduct, governance, and project-identity policies; import the unchanged history privately under `eworker-inc/Windvale`, then configure the corresponding GitHub reporting, DCO, role, and branch settings before public visibility.
5. Audit parsers, verifiers, resource limits, capability authorization, hostile inputs, and reproducible builds against the public threat model.
6. Separate stable public contracts from experimental ones and label compatibility expectations precisely.
7. Prepare small tutorials that build from source language to bytecode, object, linked image, and the VM demonstration without hiding bootstrap dependencies.

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
- Decision 0057 accepts one shared native ABI/backend family for JIT and AOT. Its exact value layout, calling convention, machine IR, runtime table, memory system, stencil shape, tier policy, and cache contract still wait for measured bytecode/native cases and linked-image requirements.
- Compiler folder names describe implementation roles rather than lifecycle status. `Compiler/Windvale` owns the Windvale-written implementation, `Compiler/Reference` owns the active C# Stage 0 reference/recovery implementation, and `Bootstrap` is reserved for the staged transition, provenance, and recovery process. This layout is cross-host qualified at `4fdc6bf`; calling the Windvale implementation a compiler does not claim that it is already self-hosting. After Decision 0057's retirement gate, a separate implementation change may archive or remove the C# project from normal automation without renaming Windvale's owned compiler around another lifecycle label.
- Assembler folder names describe implementation roles rather than maturity. `Assembler/Windvale` owns the qualified Windvale-written WVA implementation, `Assembler/Reference` owns the independent C# Stage 0 reference/recovery implementation, and `Examples/Assembler` retains only canonical WVA inputs. Decision 0051 changes ownership paths without changing WVA, WVO, assembly names, namespaces, module identities, or artifact contracts.
- Linker folder names describe implementation roles rather than target parity. `Linker/Windvale` owns the qualified Windvale-written flat-image implementation, `Linker/Reference` owns the independent C# Stage 0 reference/recovery implementation plus the currently C#-only UEFI target adapter, and `Examples/Linker` retains canonical WVA inputs. Decision 0053 changes ownership paths without changing linking, UEFI, assembly, namespace, module, or artifact contracts.
- UEFI PE32+ is the accepted first boot-container family. Its exact deterministic adapter waits for boot evidence; later PE host, ELF, and flat-image priorities must not redefine portable language behavior.
- The kernel/process boundary waits for the smallest successful verified-runtime boot experiment.
- Public compatibility and support windows wait for the licensed release foundation.

At each checkpoint the project may keep, revise, or replace the proposed mechanism. It may not silently lower the verification gate or declare a narrower demonstration to be the original milestone.

## Current focus

Phase 6 is qualified. Its WVA 1 Stage 0 contract is qualified at `3bfc6bb`, Windvale scanner at `e5fd109`, semantic inspector at `cc57bf9`, object encoder and hosted assembler at `a689617`, and Stage 0 link oracle at `9c4b9f5`. The complete Windvale linker is qualified at `40ac57d` after the prerequisite, object-view, layout, image, relocation, and independent-reconstruction slices. Windows and Debian produced the same WVB, exact 24-byte image, exact 1,721-byte map, and normalized contract while exercising maximum image/map boundaries and publish-after-success failures. Phase 7 remains active: its source-module prerequisite and four evidence-driven Foundation modules are cross-host qualified through `26e2fd1`. Phase 8 is qualified through Decision 0058 at `5c16547`: the exact committed 12-module inventory produces byte-identical 599,868-byte Stage 1 and Stage 2 compilers on Windows and Debian under the unchanged eight-billion ceiling, and the clean archive recovery path, normalized reports, and all 61 portable artifacts agree. In parallel, Decisions 0044 through 0056 establish the first x86-64 UEFI/QEMU environment and compiler-generated `.wv` kernel seam. Decisions 0059 through 0063 cross-host qualify constants, checked arithmetic/traps, typed blocks and locals, signed/bool comparisons, forward/backward branches, early returns, dynamic exact instruction/depth exhaustion, internal calls, bounded recursion, immutable i32 data, and `.rodata` relocation through the shared native WVO/AOT and W^X paths. Decision 0064 qualifies the first OS consumer of that ABI at exact candidate `708242e`: a portable module gates version-7 boot after executing on the kernel-owned stack. The immediate native gate is now a narrow runtime-service/capability boundary for the first useful host tool. A functioning kernel runtime, in-guest verified bytecode loading, clean shutdown, and Hyper-V evidence remain later work and are not claimed yet.
