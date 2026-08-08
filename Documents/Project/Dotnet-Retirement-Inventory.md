# .NET retirement inventory

> Inventory snapshot: 7 August 2026

This is the operational ledger for moving .NET out of Windvale's normal Windows and Linux workflows under [Decision 0057](../Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and the [native-execution architecture](../Architecture/Native-Execution-And-Dotnet-Retirement.md). It records direct managed entry points, the replacement standing of each product surface, and the retained recovery owners. It does not declare .NET retired while any complete gate remains open.

The machine-readable [companion inventory](Dotnet-Retirement-Inventory.json) is checked by `Tools/Verify/Verify-Dotnet-Retirement-Inventory.ps1`. Its file-level list deliberately counts an entry point once even when that file invokes .NET many times. Documentation examples and C# source files are not direct operational entry points; their ownership is recorded separately below.

## Standing vocabulary

| Standing | Meaning |
| --- | --- |
| `native-qualified` | The ordinary Windows/Linux route is native and has independent cross-host evidence. |
| `native-candidate` | A bounded native replacement exists, but its grouped qualification or promotion is pending. |
| `managed-normal` | An ordinary or required qualification/release route still invokes .NET. |
| `recovery-retained` | The managed route is intentionally kept for Stage 0 reconstruction or independent evidence. |
| `missing` | No adequate native replacement exists yet. |

## Product-surface ledger

| ID | Surface | Standing | Current route and retirement condition |
| --- | --- | --- | --- |
| B1 | Project source to canonical WVB | `native-qualified` | `Tools/Native/Build-Wvb.cmd` and `.sh` are the ordinary route. The Stage 0 compiler remains `recovery-retained`. |
| V1 | WVB verification | `native-qualified` | Digest-bound `wvverify` applications protect the ordinary native build route on Windows and Linux. |
| I1 | WVB inspection | `native-qualified` | Digest-bound `wvdump` applications provide deterministic inspection on both hosts. |
| E1 | WVB execution | `native-candidate` | The pinned bounded `wvrun` profile exists, but its exact-descendant dual-host qualification is pending; the complete execution surface remains managed. |
| T1 | Fixed native portable tests | `native-candidate` | The digest-bound [native retirement suite](../../Specifications/Windvale-Native-Retirement-Test-Suite.md) composes 23 focused Windows/Linux command lanes and 3,030 fixed cases without a normal .NET dependency. Its owned families include Seed behavior, unsafe and hostile WVB/WVO admission, assembler/linker/lowerer/publisher rejection contracts, WVA/WVO differential corpora, source/WVB/WVO containment, version-1 and hosted format-2 PE/ELF containment and valid-shaped mutations, segmented console maximum-plus-one rejections and maximum-valid construction, exact console-packager WVB source reconstruction, hostile-size WVO consumers, and the fixed source-to-AOT chain. Each child owns exact reports plus applicable input, destination, and scratch preservation. Complete Linux execution of the current plan, broader unsafe bytecode and extended WVA evidence, large-native segmented-object transfer, native host-container reconstruction, promotion, and the grouped gate remain. |
| A1 | WVA assembly | `native-qualified` | Digest-bound `wvasm` applications are promoted for the accepted assembler surface. |
| I2 | WVO verification and inspection | `native-candidate` | One focused Windvale verification module now owns complete WVO admission for both the read-only shell and native publisher. The repinned paired inspector applications, exact candidate manifest, and digest-bound Windows/Linux launchers exist. Decision 0322's fixed matrix requires both launchers to agree on all thirteen stable rejection families without a live managed oracle. Grouped dual-host qualification and promotion to the ordinary path remain. |
| L1 | WVO linking | `native-candidate` | The standard flat-linker core, clean paired applications, exact candidate manifest, and digest-bound Windows/Linux launcher exist. Grouped cross-host qualification, native replacement of the Stage 0 package constructor, and promotion to the ordinary path remain. |
| P1 | Version-1 console PE/ELF packaging | `native-candidate` | The bounded Windvale materializer, paired packager and publisher applications, exact Stage 0 provenance manifests, and digest-bound Windows/Linux launchers exist. The launcher materializes to a private candidate, revalidates the completed PE/ELF in Windvale, and reuses the native durable sibling/reread/atomic-replacement transaction. Nineteen canonical structural mutations pass through the public publisher without .NET; the two larger-than-4-MiB rejection values pass through a distinct two-snapshot read-only verifier. A separate recipe streamer constructs both maximum valid applications as two bounded chunks plus a `WVCS 1.0` manifest and matches the complete Stage 0 PE/ELF identities. The ordinary and segmented projects now reconstruct exact WVB and WVO identities through digest-bound native front doors. The verifier and publisher projects also rebuild WVB natively, but their broader hosted closures remain outside the accepted lowerer subset and all four host-container pairs remain Stage 0-constructed candidates. Native host-container reconstruction, durable segmented publication, Linux execution of these exact slices, grouped qualification, ordinary-path promotion, general hosted packaging, and release integration remain. |
| N1 | Accepted-subset WVB to WVO lowering | `native-candidate` | The current Windvale lowerer admits up to 512 scalar/control functions with arbitrary exported-Main order, fewer than 2,048 combined parameters/locals, 32,768 code bytes, and 8,192 instructions per function under the retained 2,048-cell frame check, bounded forward/self/cyclic calls, zero through 64 `i32`/`bool`/`text`/`u8`/`u32`/`bytes`, enum, or record helper parameters across ABI 22's four register positions and canonical 16-byte outgoing stack cells, checked `u32` add/subtract/multiply/divide/remainder, complete bounded `u32`/`u8` comparisons, lossless `u8`-to-`u32` conversion, up to 64 immutable text/bytes/i32 declarations with borrowed descriptor locals, conversion, slicing, length, and little-endian reads, service-backed text concatenation, UTF-8 validation/conversion, quoting, and unsigned formatting, bounded generation-owned bytes concatenation plus one-byte `u8`, bounded two-byte `u16`, and four-byte signed/unsigned little-endian construction, up to 64 nominal declarations with enum locals/constants/comparisons/name lookup including nonzero-first tables, and record-bearing functions with up to 256 compactly indexed declared record locals, 1,024 blocks, 8,192 instructions, 128 record values per block, and canonical scalar capability calls. Packed versioned storage evidence, staged layout entries, append-only leader/reachability and record-local evidence, bounded per-block liveness construction, validated segmentable WVO region ownership, bounded function-code/relocation batching, and an immutable exact-position object-publication cursor preserve those limits without constructing one complete code value. A focused hosted shell writes nonempty cursor values as distinct bounded chunks and writes a versioned `WVOP 1` manifest last without claiming atomic publication. One shared capability-free reader owns canonical serialization and rejects malformed magic, version, size, limit, index, position, length, and final-coverage evidence before host mutation; a scalar ABI-22 bridge carries the same statuses and every revalidated object/count/entry extent to a fixed native caller without adding a service or platform parser. A second segmented reader validates the compiler-produced WVO header, `.text`, optional `.rodata`, exact metadata boundaries, 32 MiB section extents, and minimum symbol/relocation tail from bounded chunks; its scalar bridge exposes only revalidated evidence. A third bounded reader validates the complete compiler-produced symbol chunk: sequential data and helper names, Main's omitted ordinal and exact code gap, section coverage, optional padding, and the final relocation-table extent. A fourth reader validates canonical ordered section-zero `Relative_i32` relocation records and their data-symbol targets, exposes the exact text-chunk count, and checks every owned zero placeholder plus the separate `0x90` padding chunk. A fifth typed cursor replays the retained publication plan, binds every nonempty actual value to its exact manifest position and length, compares arbitrary code, data, and metadata bytes completely, and requires final coverage without joining one whole WVO. A sixth resource plan reserves native snapshot ordinals zero and one for the input and manifest, admits at most 62 exact canonical chunk names into the remaining 64-entry table, rejects exact control/chunk collisions, and drives one hosted admission execution that reads each name once and validates the same borrowed chunk without touching the destination. Fixed Windows/Linux WVA adapters now validate that table and every reopened resource against its immutable snapshot, reject destination aliases by native file identity, reload the immutable snapshot ordinal for every multi-iteration write and reread, write only admitted chunks to an exclusive sibling, flush and reread through exact EOF, atomically replace, and clean pre-replacement scratch. Exact Windows/Linux staging producer packages now preserve the existing six-capability/ten-service Windvale tool, and the current-host native producer/publisher processes compose on the small canonical fixture without loading .NET. An exact five-artifact manifest and digest-bound Windows/Linux launchers now expose the current whole-object accepted-subset lowerer plus its native-produced fixed WVO vector; the tool WVB builds through the qualified native front door while host containers remain Stage 0-constructed. Those launchers now lower into a private whole-object candidate and invoke a digest-bound five-service WVO publisher that shares complete portable admission with the inspector and reuses the native durable replacement transaction. It reproduces Stage 0's exact general-call, descriptor-call, unsigned-arithmetic and conversion, large-module, large-function, large-record-planner, `Sum-Data.wv`, compiler-produced `Function-Only.wv`, `Data-And-Text.wv`, and `Nominal-Types.wv`, static-descriptor, text-service including maximum `u32` formatting, focused enum/signature, direct-record, record-call, multiple-record-call, process-input, file-read, file-write, console-output, and diagnostic-output WVO objects and composes through link/package/execute. The ordinary and segmented console-packager closures reconstruct exact 692,425-byte and 789,653-byte WVOs through that same native launcher. Verifier-scale native staging now lowers, publishes, independently verifies, and exactly reconstructs a seven-chunk 1,049,615-byte WVO without widening the emitter arena or loading .NET. The separate 423,241-byte publisher self-lowering probe reaches the unchanged 128 MiB text-arena boundary with exact runtime status rather than producing output. Resolving that measured lifetime pressure, Linux composition, native replacement of Stage 0 application constructors, remaining scalar conversions and construction operations, broader nominal shapes, required-service serialization, the complete backend, promotion, and grouped qualification remain. |
| N2 | Complete native backend and baseline JIT | `managed-normal` | The qualified Stage 0 backend remains the complete implementation. Windvale must own general lowering, verification, publication, runtime services, and deterministic JIT/AOT behavior before cutover. |
| T2 | Complete Seed, OS, golden, malformed, and differential suites | `managed-normal` | The C# harness owns the broad suite. A digest-bound native retirement coordinator now aggregates 3,030 transferred fixed cases, including the source-to-AOT chain, 200 hostile linker inputs, 256 hostile PE/ELF candidates, 19 canonical version-1 mutations, 15 valid/mutated hosted format-2 candidates, two segmented maximum-plus-one console rejections, two maximum-valid segmented constructions, two exact console-packager source reconstructions, four hostile-size WVO consumer cases, 256 frozen WVO differential inputs, 200 frozen WVA differential inputs, and the 2,000-case random-containment corpus independently of a live C# oracle; remaining reusable cases must move to versioned fixtures and manifests rather than being ported line for line. |
| O1 | Windvale OS image construction and boot probes | `managed-normal` | `Operating-System/Windvale.Bootstrap` and `Verify-Os-Boot.ps1` still build and orchestrate the current images. |
| W1 | Complete WebAssembly generation and verification | `native-candidate` | The normal static playground compiles source through direct import-free ABI-4 Windvale compiler Wasm, then verifies and executes the returned WVB through the separate ABI-3 interpreter Wasm. Pinned native routes regenerate both WVB inputs and the interpreter Wasm without .NET; a pinned segmented Wasm generator reproduces the direct compiler artifact during normal maintenance. Reconstructing that generator and the broader differential/qualification gate still retain managed recovery oracles pending grouped promotion. |
| G1 | Independent dual-host qualification | `managed-normal` | `.github/workflows/verify.yml` installs .NET and runs the managed Seed gate. Native replacement must retain Windows/Linux independence and fail-closed verification. |
| R1 | Homepage and playground release | `native-candidate` | The homepage workflow publishes the static playground and its digest-pinned native package without installing .NET. Independent release promotion evidence remains before this row is called qualified. |
| D1 | Local editable browser playground | `native-candidate` | `npm run dev:playground` builds and serves the static Monaco/native-WebAssembly application without starting Blazor or .NET. Cross-host promotion remains. |
| C1 | Clean bootstrap from documented native seeds | `missing` | Current recovery scripts reconstruct pinned native artifacts through Stage 0. A previous native release must rebuild the accepted toolchain without .NET. |
| C2 | Final digest-bound Stage 0 recovery archive | `missing` | Produce and verify one final Windows/Linux recovery release before deleting retired managed source. |

Current N1 update: [Decision 0346](../Decisions/0346-Bounded-Native-Publisher-Self-Lowering.md)
supersedes the earlier self-lowering probe result recorded in the ledger row.
The refreshed 440,994-byte publisher WVB now lowers through the current-host
native producer and publisher in multiple bounded chunks and exactly matches
the 6,449,889-byte Stage 0 WVO without loading .NET or widening the native
arena. Linux execution, native host-package construction, remaining backend
subset work, promotion, and grouped qualification remain open.

Current L1 update: [Decision 0351](../Decisions/0351-Immutable-Snapshot-Compiler-Image-Staging.md)
adds the first hosted Windvale owner of the segmented compiler-image path. It
acquires a strict `WVOP` manifest plus at most 62 canonical WVO chunks into one
execution's immutable snapshot table, reuses those values for metadata,
producer, and independent-verifier passes, writes only accepted bounded image
chunks, and publishes `WVLI` last. The small eight-chunk fixture crosses this
complete Windvale boundary with nine underlying reads and exact linked bytes.
Publisher-scale transfer, native file-identity and durable replacement,
canonical map evidence, Linux execution, promotion, and grouped qualification
remain open.

[Decision 0352](../Decisions/0352-Digest-Bound-Compiler-Image-Staging-Applications.md)
closes the next L1 sub-item: exact 849,920-byte Windows and 851,968-byte Linux
application candidates now package the 75,337-byte Windvale staging root. The
current-host Windows process preserves all source chunks, emits exact linked
chunks plus `WVLI`, and loads no CLR component. The Linux package is
structurally verified but not executed. Stage 0 still constructs both host
containers, and publisher-scale transfer, durable public publication,
canonical map evidence, Linux execution, promotion, and grouped qualification
remain open.

[Decision 0353](../Decisions/0353-Native-Compiler-Image-Staging-Source-Build.md)
removes Stage 0 from construction of that 75,337-byte staging WVB. Its complete
checked-in project closure now builds through the ordinary native source front
door and matches the frozen Stage 0 oracle byte for byte. Stage 0 remains only
the differential/recovery oracle for this source boundary and still constructs
both host containers. Linux source-build/execution evidence, native container
construction, publisher-scale transfer, durable publication, promotion, and
the grouped gate remain open.

[Decision 0354](../Decisions/0354-Native-Compiler-Image-Staging-Reconstruction.md)
composes that native-built WVB through the current-host segmented WVO producer
and immutable-snapshot image staging processes. Their complete WVO, linked
image, and entry match the independent Stage 0 oracles byte for byte, and
neither native child loads a managed runtime. Stage 0 still constructs both
tool-container families and remains the temporary differential oracle. Native
service-bundle and host-container construction, a promoted composition
launcher, durable image publication, canonical map evidence, Linux execution,
and the grouped gate remain open.

[Decision 0355](../Decisions/0355-Windvale-Owned-Native-Utf8-Service-Construction.md)
removes the normal C# byte-emission algorithm for the shared strict UTF-8
native service. A focused Windvale core now constructs and patches the exact
800-byte x64 leaf, and its capability-free retained bridge builds identically
through Stage 0 and the ordinary native source front door. The recovery wrapper
only verifies, lowers, executes, and caches the Windvale result; every existing
service bundle and application retains its exact identity. Managed native-WVB
loading, W^X execution/publication, bundle orchestration, Linux evidence, and
the grouped gate remain separate open items.

[Decision 0356](../Decisions/0356-Windvale-Owned-Native-Integer-Format-Construction.md)
removes the shared C# emission algorithm for the exact signed and unsigned
integer-format leaves. One focused Windvale generator now owns both variants;
its retained capability-free bridge builds identically through Stage 0 and the
ordinary native source front door, then returns the unchanged 225-byte and
191-byte leaves. The recovery wrapper only verifies, lowers, executes, splits,
and caches them. Enum-name, quoting, retained-WVB loading, W^X publication,
service-bundle orchestration, Linux evidence, and the grouped gate remain open.

[Decision 0357](../Decisions/0357-Windvale-Owned-Native-Text-Concatenation-Construction.md)
removes the C# emission algorithm for the exact text-concatenation leaf. A
focused Windvale generator uses the new shared service-code builder to produce
the unchanged 249-byte x64 leaf, and its retained capability-free bridge builds
identically through Stage 0 and the ordinary native source front door. The
recovery wrapper only verifies, lowers, executes, identity-checks, and caches
the result. Existing dynamic-text evidence continues to own copy, value-limit,
arena-exhaustion, and mixed-allocation semantics. Enum-name, retained-WVB
loading, W^X publication, service-bundle orchestration, Linux evidence, and
the grouped gate remain open.

[Decision 0358](../Decisions/0358-Windvale-Owned-Native-Text-Quote-Leaf.md)
removes the large C# emission algorithm for the exact deterministic text-quote
leaf. One compact Windvale machine-template source now owns the unchanged
1,165-byte x64 implementation, and its retained capability-free bridge builds
identically through Stage 0 and the ordinary native source front door. Quote
semantics remain in the specification and existing dynamic-text coverage;
the recovery wrapper only verifies, lowers, executes, identity-checks, and
caches the leaf. Enum-name metadata construction, retained-WVB loading, W^X
publication, service-bundle orchestration, Linux evidence, and the grouped gate
remain open.

[Decision 0359](../Decisions/0359-Windvale-Owned-Native-Enum-Name-Leaf.md)
removes the last C# native-service byte emitter and its now-unused general code
builder. One compact Windvale machine-template source owns the unchanged
323-byte enum-name leaf, and its retained capability-free bridge builds
identically through Stage 0 and the ordinary native source front door. The
temporary C# wrapper still constructs and validates the type-dependent adjacent
`WVEN` block, then combines it with the verified Windvale leaf. Transferring
that metadata builder, retained-WVB loading, W^X publication, service-bundle
orchestration, Linux evidence, and the grouped gate remain open.

Decision 0329 further advances T1 with a separate five-case unsafe-WVB matrix:
both digest-bound read-only launchers require exact semantic or typed-execution
reports and preserve each compact fixed input without a live .NET oracle.
Broader nominal/limit unsafe cases and seeded randomized containment remain.

[Decision 0347](../Decisions/0347-Fixed-Native-Nominal-Wvb-Rejections.md)
adds five fixed nominal-type cases to that same cohesive lane. Missing record
types and fields, duplicate record and nominal names, and mismatched enum
comparison now pass through both native readers without a live managed oracle.
The current 23-suite coordinator therefore owns 3,035 fixed cases; this
supersedes the 3,030 counts in the compact T1 and T2 ledger rows above. Nominal
count/value-size limits and other typed opcode families remain.

Decision 0330 originally composed every then-current T1 command through one 787-byte
digest-bound manifest. The paired direct coordinators fix ten suite names, 74
cases, child summaries, empty-error requirements, and fail-fast ordering without
adding another managed wrapper. The focused Windows unsafe-WVB filter passes;
the complete Windows/Linux execution remains part of the grouped gate.

Decision 0332 adds a separate 200-case hostile-linker lane from one compact
portable corpus. The current 868-byte plan now fixes eleven suites and 274 cases;
the focused Windows lane passes exact `WVL1002`, input preservation, and output
preservation for every value without consulting .NET.

Decision 0334 adds a separate 256-case console-container hostile lane from one
digest-bound portable corpus. The current 971-byte plan now fixes twelve suites and
530 cases; the focused Windows run drives both PE and ELF admission through the
native publisher with exact rejection, input/destination preservation, and zero
scratch without consulting .NET.

Decision 0335 adds a separate 256-case WVO differential lane from the exact
frozen Stage 0 sequence. The current 1,049-byte plan now fixes thirteen suites
and 786 cases; the focused Windows run agrees on 32 accepted mutations and 224
rejections through the native verifier while preserving every input without a
live .NET oracle.

Decision 0336 adds a separate 200-case WVA differential lane from the exact
seeded Stage 0 sequence. The current 1,127-byte plan now fixes fourteen suites
and 986 cases; the focused Windows run agrees on all 199 rejection codes and the
sole accepted 243-byte WVO while preserving every source and rejected
destination without a live .NET oracle.

Decision 0337 adds separate 500-source, 1,000-WVB, and 500-WVO containment
lanes over the exact one-stream legacy sequence. The current 1,364-byte plan now
fixes seventeen suites and 2,986 cases; focused Windows runs contain every value
through the import-free compiler adapter or digest-bound native tool while
preserving every input and rejected assembler destination without a live .NET
oracle.

Decisions 0338 through 0340 add 19 version-1 mutations, four hostile-size WVO
consumer checks, and 15 hosted format-2 cases. Decision 0341 transfers the two
segmented console maximum-plus-one boundaries through a dedicated two-snapshot
read-only verifier. Decision 0342 then streams both maximum valid applications
into bounded chunks and matches their complete Stage 0 identities. The current
1,982-byte plan fixes 23 suites and 3,035 cases; all three focused Windows
commands pass 2/2 without consulting a live .NET oracle. Native reconstruction
now owns both console-packager WVBs, while Linux execution, native host-container
construction, and the grouped gate remain deferred.

The next transfer should close a complete row or a clearly bounded part of one row. Candidate tools are not promoted merely because they compose once; their decoders, failure behavior, deterministic bytes, and both host packages remain part of the grouped gate.

## Direct managed entry points

The companion JSON currently records 11 operational files across three lanes:

- verification: Seed, bootstrap, OS, WebAssembly, and GitHub qualification;
- release: the managed independent-qualification workflow that gates publication; and
- recovery: the explicit Stage 0 reconstruction scripts and bootstrap evidence.

The verifier searches website package commands, GitHub workflows, `Tools/Verify`, and `Tools/Recovery`. A direct .NET invocation added to those scopes must be entered in the JSON in the same change. Removing an invocation requires removing its inventory entry and updating the corresponding surface row.

## Retained managed source owners

| Owner | Current retained responsibility | Removal rule |
| --- | --- | --- |
| `Compiler/Reference` | Frozen source compiler oracle and recovery compiler | Keep through the final Stage 0 archive; only bounded recovery, security, qualification, or evidence corrections are allowed. |
| `Runtime/Windvale.Bytecode` and `Runtime/Windvale.Runtime` | Canonical decoding, verification, and interpreter oracle | Keep until the native decoder/runtime and differential fixtures satisfy the complete gate. |
| `Compiler/Native` and `Runtime/Windvale.Native` | Complete Stage 0 native lowering, ABI, publication, and host execution | Keep until the Windvale-owned backend and runtime cover the accepted product surface on both hosts. |
| `Assembler/Reference`, `Object-Model/Windvale.ObjectModel`, and `Linker/Reference` | Independent object/assembly/link oracles and managed CLI implementations | Move to recovery/differential-only after each promoted native route covers its contract. |
| `Tools/Windvale.Project` and `Tools/Windvale.Tool` | Managed command orchestration and project parsing | Remove from normal use as native launchers and project tooling become complete. |
| `Tests/Windvale.Seed.Tests` and `Tests/Windvale.Os.Tests` | Broad conformance, differential, malformed-input, OS, and artifact evidence | Retain as independent evidence until equivalent native manifests/fixtures and the final gate qualify. |
| `Operating-System/Windvale.Bootstrap` | Host-side image construction and probe orchestration | Retire only after the native OS build/probe route reproduces the qualified images and reports. |
| `Tools/Windvale.Playground` and `Tools/Windvale.Playground.Engine` | The static files under `Tools/Windvale.Playground` are the normal browser product; its managed host and engine remain recovery/differential evidence | Keep the static application. Move or retire only the managed project/engine after the remaining WebAssembly artifact-production seam and final recovery gate close. |

Deleting any owner early would destroy recovery or independent evidence. After every normal responsibility has a qualified native owner, the managed projects move behind explicit recovery commands; source deletion is a final action after the complete Decision 0057 gate and archived recovery proof.

## Verification rhythm for the active retirement goal

Each coherent slice gets its affected tests reviewed first and then one narrow, quick local check. Passing results are reused while their relevant inputs remain unchanged. Temporary failures outside the slice are acceptable during the migration. Local Standard/Qualification and repeated GitHub qualification stay deferred until the remaining transfers are ready. Immediately before that final broad gate, update from the shared upstream branch, reconcile changes, regenerate current artifact identities once, and run the complete Windows/Linux qualification once.
