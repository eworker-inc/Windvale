# .NET retirement inventory

> Inventory snapshot: 8 August 2026

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

[Decision 0360](../Decisions/0360-Native-Bounded-Byte-Entry-Input.md)
adds the bounded native `Main(bytes) -> bytes` entry needed by variable-input
Windvale constructors such as the next `WVEN` metadata builder. The existing
cross-host result bridge now has an exact two-cell form only for that new entry
shape: one output descriptor and one immutable, execution-owned input
descriptor. Parameterless entry bytes, ABI 22, the execution context, and
capabilities remain unchanged. The C# executor still owns temporary input
copying, W^X invocation, and output copying; moving those host responsibilities
remains part of the larger retirement gate.

[Decision 0361](../Decisions/0361-Windvale-Owned-Bounded-Native-Enum-Metadata.md)
moves canonical `WVEN` construction through the ordinary 4 MiB Windvale byte
limit into one portable, strict request consumer. Its retained bridge builds
identically through Stage 0 and the native source front door, then validates
and converts a versioned `WVEQ` projection through native `Main(bytes) ->
bytes`. The managed wrapper still projects the request, loads and executes the
retained WVB, independently validates the result, and preserves an explicit
recovery writer only for valid 4-to-32-MiB metadata. A streaming or
session-owned result seam must close that last size lane without reducing the
existing contract; retained-WVB loading, W^X publication, service-bundle
orchestration, Linux evidence, and the grouped gate remain open.

[Decision 0362](../Decisions/0362-Windvale-Owned-Segmented-Native-Enum-Metadata.md)
closes that last size lane without widening a Windvale byte value or shrinking
the 32 MiB `WVEN` contract. Versioned requests partition only between complete
nominal types, and the Windvale core returns exact header, directory, member,
and name sections in bounded envelopes. The temporary managed session validates
and concatenates those sections but no longer contains a `WVEN` field writer;
the former 4-to-32-MiB recovery writer is removed. Managed nominal projection,
retained-WVB loading and lowering, W^X execution, final independent validation,
service-bundle orchestration, Linux evidence, and the grouped gate remain open.

[Decision 0363](../Decisions/0363-Direct-Native-Enum-Name-Leaf-Consumption.md)
removes live managed WVB decoding, x64 lowering, temporary W^X execution, and
result copying from the normal fixed enum-name leaf path. The runtime now
embeds and digest-checks the exact 323-byte artifact generated by the retained
Windvale source; the source and WVB remain reproducible qualification/recovery
evidence but the WVB is absent from the normal runtime assembly. Segmented
metadata construction still uses its variable-input retained WVB, and managed
bundle assembly, final W^X publication, Linux evidence, and the grouped gate
remain open. The same direct-artifact pattern can now move the other fixed
Windvale-owned service leaves out of live generator execution.

[Decision 0364](../Decisions/0364-Direct-Fixed-Native-Service-Leaf-Consumption.md)
applies that proven boundary to strict UTF-8 validation, text concatenation,
text quoting, and signed and unsigned integer formatting. The normal runtime
assembly now embeds all six fixed exact service leaves and none of their five
generator WVBs. Sources, projects, and retained WVBs remain exact
qualification/recovery evidence, but no fixed pure service performs managed
WVB decode, lowering, temporary W^X generator execution, result copying, or
teardown in the normal path. Variable-input constructors, managed bundle
assembly, final W^X publication, Linux evidence, and the grouped gate remain
open.

[Decision 0365](../Decisions/0365-Native-Publication-Planner-Execution.md)
removes the managed reference interpreter and synthetic `file.read_bytes`
capability from normal executable-image layout and publication-lifetime
planning. Both retained bridges are now capability-free portable
`Main(bytes) -> bytes` modules lowered to verified native fragments. One small
service-free bootstrap publishes only those planners and is checked against
their accepted Windvale results; final application layout and lifetime still
come from Windvale. Retained-WVB decoding/lowering, segmented enum-metadata
transport, managed service-bundle assembly, the platform W^X adapter, Linux
evidence, and the grouped gate remain open.

[Decision 0366](../Decisions/0366-Direct-Native-Argument-Service-Leaf-Consumption.md)
removes the last fixed-output generator WVB from the normal runtime assembly.
The 20,800-byte native-stencil bridge remains exact qualification, differential,
and recovery evidence, while ordinary process-input service assembly consumes
its separately digest-bound 5-byte and 70-byte leaves directly. Normal use no
longer decodes, lowers, publishes, invokes, copies, or splits that generator.
The three variable-input retained WVBs, managed service-bundle assembly, final
W^X ownership, Linux evidence, and the grouped gate remain open.

[Decision 0367](../Decisions/0367-Versioned-Verified-Native-Fragment-Artifact.md)
defines the bounded `WVNF 1.0` handoff needed by the three variable-input
consumers. Unlike raw code or WVO alone, it preserves the explicit target, ABI,
final code, symbols, patches, nominal types, and required services consumed by
the existing native verifier. Focused valid, boundary, malformed, and hostile
tests pass locally. Production artifact generation and loader cutover are the
next slice; the managed WVB loaders remain active until that cutover.

[Decision 0368](../Decisions/0368-Direct-Verified-Native-Fragment-Consumption.md)
completes that cutover for segmented enum metadata and both publication
planners. Their exact WVBs remain source-reproducible recovery/differential
evidence but are absent from the normal runtime assembly, which now embeds no
WVB helpers or generators. Ordinary first use verifies the digest-bound WVNF
directly and no longer performs managed WVB decoding, semantic verification,
or x86-64 lowering. Managed WVNF parsing, application lowering, bundle and W^X
ownership, Linux evidence, and the grouped gate remain open.

[Decision 0369](../Decisions/0369-Windvale-Owned-Native-Output-Leaves.md)
removes C# instruction emission and branch patching for the four Windows/Linux
console and diagnostic output leaves. Two focused Windvale source modules and
one native-front-door-reproducible bridge own their unchanged exact bytes. The
normal runtime embeds and digest-checks only the four generated leaves; the
bridge WVB remains repository recovery/differential evidence. Platform output
table binding, file-input/output leaf generation, bundle assembly, final W^X
ownership, Linux execution evidence, and the grouped gate remain open.

[Decision 0370](../Decisions/0370-Windvale-Owned-Native-File-Output-Leaves.md)
removes the corresponding C# instruction emission and branch patching for the
Windows and Linux `file.write_bytes` leaves. Shared and platform-focused
Windvale modules plus one native-front-door-reproducible bridge own their
unchanged 787-byte and 823-byte machine adapters. The normal runtime embeds and
digest-checks only those two generated leaves; the bridge WVB remains recovery
and differential evidence. The managed `WVFO` table owner, file-input leaf
construction, service-bundle construction, W^X ownership, invocation, Linux
evidence, and grouped gate remain open.

[Decision 0371](../Decisions/0371-Windvale-Owned-Native-File-Input-Leaves.md)
removes the final C# platform-leaf instruction generator for Windows and Linux
`file.read_bytes`. Shared and platform-focused Windvale modules plus one
native-front-door-reproducible bridge own the unchanged 1,218-byte and 996-byte
adapters. The normal runtime embeds and digest-checks only those two generated
leaves; the bridge WVB remains recovery and differential evidence. Managed
`WVFI`/`WVFO` table binding, service-bundle construction, W^X ownership,
invocation, Linux execution evidence, and the grouped gate remain open.

[Decision 0372](../Decisions/0372-Windvale-Owned-Bounded-Service-Bundle-Materialization.md)
removes C# image copying and alignment-fill policy from every service bundle
whose complete request and response fit the ordinary 4 MiB byte-value limit.
One digest-bound service-free WVNF consumes the exact Windvale publication
plan plus already verified fragment/leaf bytes and returns the completely
materialized image; the host independently checks every placement and byte.
Compiler-scale bundles retain an explicitly named Stage 0 large-image fallback
until segmented Windvale construction replaces it. Managed adapter/table-slot
metadata, `WVFI`/`WVFO`/`WVIO` binding, W^X ownership, contexts, arenas,
invocation, Linux evidence, and the grouped gate remain open.

[Decision 0373](../Decisions/0373-Windvale-Owned-Segmented-Service-Bundle-Materialization.md)
removes the remaining compiler-scale C# bundle writer. Canonical bounded
requests carry the complete publication plan and only the source ranges that
intersect one image segment; Windvale constructs every fragment, leaf, zero,
and NOP byte. One managed session projects requests, validates returned
segments independently, and concatenates them in order. Both small and large
bundles now use this sole path. Managed adapter/table-slot metadata,
`WVFI`/`WVFO`/`WVIO` binding, W^X ownership, contexts, arenas, invocation,
Linux evidence, and the grouped gate remain open.

[Decision 0374](../Decisions/0374-Windvale-Owned-Native-Output-Table.md)
removes C# ownership of the 48-byte `WVIO` layout. Windvale validates an exact
bounded request containing already acquired opaque targets and constructs the
complete table; the managed host retains only channel acquisition/pinning,
Windows writer resolution, response verification/copy, and teardown. `WVFI`,
`WVFO`, the service table, execution context, W^X ownership, arenas, invocation,
Linux evidence, and the grouped gate remain open.

[Decision 0375](../Decisions/0375-Windvale-Owned-Native-File-Output-Table.md)
removes C# ownership of the 80-byte `WVFO` layout. Windvale validates the
already allocated scratch target/capacity and six opaque Windows-function
ranges, constructs the complete table, and rejects Linux function targets. The
managed host retains allocation, library/export acquisition, response
verification/copy, and teardown. `WVFI`, the service table, execution context,
W^X ownership, arenas, invocation, Linux evidence, and the grouped gate remain
open.

[Decision 0376](../Decisions/0376-Windvale-Owned-Native-File-Input-Table.md)
removes C# ownership of the immutable initial 136-byte `WVFI` layout. Windvale
validates four already allocated arena targets, exact capacities, zero initial
snapshot state, and seven opaque Windows-function ranges before constructing
the complete table. The managed host retains allocation, library/export
acquisition, response verification/copy, mutable snapshot publication checks,
and teardown. All specialized binding tables are now Windvale-owned; the
shared service table, execution context, W^X ownership, arenas, invocation,
Linux evidence, and the grouped gate remain open.

[Decision 0377](../Decisions/0377-Windvale-Owned-Native-Service-Table.md)
removes C# ownership of the 104-byte shared service-table layout and its
twelve-way byte-offset switch. Windvale validates the exact closed required
mask and target-presence relation, then constructs version 5 from already
published opaque targets. The managed host retains executable publication,
placement-to-address calculation, response verification/copy, invocation, and
teardown. The execution context, W^X ownership, arenas, result admission,
Linux evidence, and the grouped gate remain open.

[Decision 0378](../Decisions/0378-Windvale-Owned-Native-Execution-Context.md)
removes C# ownership of the ordinary 112-byte execution-context layout and
direct mutable-word reads from the large executor. Windvale validates exact
budgets, arenas, initial state, arguments, and optional table pointers; one
focused host owner independently verifies/copies the result and bounds the
three permitted completion mutations. Service-free Windvale constructors
still require an explicitly named frozen Stage 0 context oracle to avoid a
bootstrap recursion cycle, and focused evidence compares its bytes exactly.
That oracle, W^X ownership, arena allocation, invocation, result admission,
Linux evidence, and the grouped gate remain open.

[Decision 0379](../Decisions/0379-Windvale-Owned-Native-Argument-Table.md)
removes the remaining C# writer for ABI 12's immutable 16-byte argument
descriptors. Windvale validates the bounded count, opaque targets, per-entry
lengths, canonical offsets, and exact packed-payload coverage before returning
the complete table. The managed host retains strict UTF-8 validation, payload
packing and allocation, address projection, independent descriptor/range/byte
verification, and reverse-order release. Arena allocation, entry/result bridge
cells, invocation, W^X ownership, Linux evidence, and the grouped gate remain
open.

[Decision 0380](../Decisions/0380-Windvale-Owned-Native-Entry-Bridge.md)
removes C# ownership of the native entry/result bridge layout and its direct
descriptor decode from the large executor. Windvale constructs the exact
16-byte zero result cell and optional immutable 16-byte input descriptor from
one bounded request. A focused host owner allocates and copies the result,
admits result-cell mutation, rejects any input-descriptor mutation, and passes
the parsed result to the unchanged range verifier. The service-free constructor
lane retains one named frozen Stage 0 bridge oracle to avoid recursion. Input
allocation, result copying, arenas, invocation, W^X ownership, Linux evidence,
and the grouped gate remain open.

[Decision 0381](../Decisions/0381-Windvale-Owned-Native-Byte-Result-Admission.md)
removes the remaining C# result-range arithmetic from ordinary execution.
Windvale admits the untrusted descriptor only inside the committed arena,
immutable entry input, or one of at most 4,096 verified static-data ranges,
including checked address-boundary cases. The host retains evidence projection,
response verification, admitted real-memory copying, and teardown. The
service-free constructor lane retains the former algorithm as a named frozen
Stage 0 admission oracle. Arena and input allocation, invocation, W^X ownership,
Linux evidence, and the grouped gate remain open.

[Decision 0382](../Decisions/0382-Windvale-Owned-Hosted-Tool-Runtime-Header.md)
removes C# ownership of the shared initial 4,096-byte hosted-tool runtime
header. Windvale validates the exact fixed metadata directory and constructs
all five initial ABI tables, metadata placement, and reserved bytes for every
implemented compiler-family profile on both targets. Normal packaging invokes
and independently verifies one retained service-free WVNF. The former C# byte
writer is recovery/differential-only under an explicit Stage 0 oracle name;
the managed invocation/verification bridge is temporary and must disappear
when native host-container construction consumes the same contract directly.
Metadata and service-bundle construction, outer PE/ELF construction, Linux
execution, and the grouped gate remain open.

[Decision 0383](../Decisions/0383-Windvale-Owned-Hosted-Tool-Metadata-Construction.md)
removes C# ownership of the embedded 1,024-byte `WVH* 1` hosted-tool metadata.
Windvale derives every profile field, capability and service identity, table
slot, target adapter, fixed limit, layout field, and reserved byte from a
bounded request containing only verified bundle extents and raw digests. The
normal runtime-data path consumes and independently verifies the retained
service-free WVNF; the former C# constructor is now `Buildˉstage0` and serves
only recovery/differential tests. The managed request/response adapter and C#
test are explicitly temporary until native host-container construction and
native qualification consume the same contract. Service-bundle and outer
PE/ELF construction, Linux execution, and the grouped gate remain open.

[Decision 0384](../Decisions/0384-Windvale-Owned-Hosted-Startup-Instantiation.md)
removes C# ownership of the normal Windows and Linux hosted startup bytes.
Windvale now validates and instantiates the exact WVOs assembled from the
canonical WVA sources, so no second copy of their 2,275 machine-code bytes is
introduced. The former C# template patchers are `Buildˉstage0` recovery and
differential oracles. The managed bridge still projects final symbol targets,
executes the retained WVNF, verifies its response, and passes the result to the
outer PE/ELF writers; those responsibilities retire with native container
planning and the final grouped gate.

[Decision 0385](../Decisions/0385-Windvale-Owned-Hosted-Container-Construction.md)
transfers hosted compiler-family layout, startup targets, PE/ELF headers,
Windows imports and relocation, and segment positions to four bounded
Windvale-native fragments. The former complete C# application builders are
`Buildˉstage0` recovery/differential oracles, and neither former C# layout
planner is called by the normal relay.

[Decision 0386](../Decisions/0386-Windvale-Owned-Segmented-Hosted-Container-Materialization.md)
removes the remaining complete managed region writer. Windvale now constructs
the final application as canonical sub-4-MiB segments from intersecting opaque
source ranges and owned zero/padding bytes. Managed dispatch, bounded request
projection, response checking, ordered concatenation, direct publication,
Linux execution, promotion, and the grouped gate remain open.

[Decision 0387](../Decisions/0387-Standalone-Native-Hosted-Container-Segmenter.md)
adds the paired profile-7 native process that consumes those same bounded
requests. Its exact source reconstructs through the native Project 1 front
door; the current-host process agrees with the retained fragment and preserves
an existing output on rejection. The new C# writer is deletion-bound Stage 0
package wiring. The next transfer joins an immutable ordered segment set to the
existing native durable multi-chunk transaction, then removes managed dispatch,
concatenation, and publication from the normal route.

[Decision 0388](../Decisions/0388-Immutable-Hosted-Container-Segment-Set.md)
adds the `WVHM 1` ordered-set boundary and a Windvale admission tool. It binds
each request to the selected layout header, reruns the shared constructor, and
compares every response byte before any mutation. The C# fixture/harness is
explicitly deletion-bound. Native durable multi-chunk publication is the
remaining boundary for this hosted-container slice.

[Decision 0389](../Decisions/0389-Shared-Immutable-Snapshot-Sequence.md)
makes the first durable-publication extraction reusable. Native x64 now has one
focused immutable snapshot-table verifier plus thin WVO and hosted-container
selection policies. Existing WVO atomic publication still passes its focused
current-host behavior test. Connecting the hosted wrapper to Windows/Linux
durable replacement remains open; no new managed product semantics were added.

[Decision 0390](../Decisions/0390-Reusable-Linux-Durable-Multi-Chunk-Publication.md)
moves the complete Linux durable mutation transaction behind a format-neutral
snapshot-range interface. The WVO-specific adapter is now acquisition and
identity policy only; hosted containers can reuse the same transaction without
managed concatenation. Windows extraction, hosted packaging, and Linux runtime
evidence remain open.

[Decision 0391](../Decisions/0391-Reusable-Windows-Durable-Multi-Chunk-Publication.md)
moves the equivalent Windows handle-relative transaction behind the same
selection contract. Its current-host success, rejection, alias preservation,
and cleanup evidence passes. Both platform mutation owners are now independent
of WVO; connecting the hosted-container admission root and deleting managed
final publication remain open.

[Decision 0392](../Decisions/0392-Shared-Immutable-Snapshot-Publisher-Shells.md)
removes the last WVO-specific duplication from platform acquisition. One
Windows and one Linux shell now own argument/runtime setup, immutable resource
reopening and byte comparison, native identity, destination-alias rejection,
and durable-transaction dispatch. Four 14-line policy wrappers select WVO or
hosted snapshot rules. The Windows WVO route passes its focused behavior test;
Linux execution, hosted application packaging, and deletion of managed hosted
dispatch, concatenation, and final publication remain open.

[Decision 0393](../Decisions/0393-Paired-Native-Hosted-Container-Publishers.md)
closes that hosted application connection. Exact Windows/Linux packages compose
Windvale admission, alternating-response selection, shared acquisition, and
durable replacement; the public CLI builds the current-host target and Windows
executes success and rejection paths without loading .NET. A shared WVA object
also replaces the private WVB-fragment publication-state bridge. Managed
response concatenation and final publication are no longer candidate runtime
logic. Native reconstruction of Stage 0 package layout, Linux execution,
promotion, and grouped qualification remain open.

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
