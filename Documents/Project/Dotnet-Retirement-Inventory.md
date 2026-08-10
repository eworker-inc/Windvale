# .NET retirement inventory

> Inventory snapshot: 10 August 2026

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
| E1 | WVB execution | `native-candidate` | Decision 0509 reconstructs the profile-5 runner from its complete source closure. Decision 0510 aligns its fixed budget with the ordinary 1,000,000-instruction CLI default, reconstructs the resulting exact WVB/WVO/fragment/paired applications, and transfers three more capability-free Foundation executions. Independent Linux execution, capability-bearing execution, the 4 MiB Byte Construction value shape, per-function profiling, exact-descendant dual-host qualification, and the complete execution surface remain open. |
| T1 | Fixed native portable tests | `native-candidate` | The digest-bound [native retirement suite](../../Specifications/Windvale-Native-Retirement-Test-Suite.md) composes 43 focused Windows/Linux command lanes and 3,204 fixed cases without a normal .NET dependency. Its owned families include Seed behavior, twenty fixed unsafe WVB boundaries, hostile WVB/WVO admission, four exact assembler golden objects, a 269-case WVA differential/positive corpus, assembler/linker/lowerer/publisher rejection and hosted-verifier publisher-construction and read-only admission contracts, the three-case current-compiler reconstruction inventory, the three-case segmented compiler toolset reconstruction inventory, the three-case source-built WVB runner reconstruction and execution inventory, the three-case WVO inspector reconstruction inventory, the three-case standard Wv-Linker reconstruction inventory, the three-case console-application-verifier reconstruction and two-snapshot compatibility inventory, the three-case console-application-publisher reconstruction and independent publication inventory, the six-case baseline-JIT patch-plan and W^X publication contract, the four-case verified WVO export-renaming contract, the eleven-case OS Probe object-producer contract, the seven-case system-kernel target contract, the two-case process-policy source-object contract, the two-case process-object reconstruction contract, WVO differential corpora, source/WVB/WVO containment, version-1 and hosted format-2 PE/ELF containment and valid-shaped mutations, segmented console maximum-plus-one rejections and maximum-valid construction, exact console-packager WVB source reconstruction, hostile-size WVO consumers, native-link-to-UEFI construction/repetition/rejection, the four-case native Probe 40 build lane, and the fixed source-to-AOT chain. Decision 0458 makes the local changed-file front door select only these focused owners and refuse named evidence gaps without a managed or unfiltered fallback. Complete Linux execution of the current plan, remaining gap closure, promotion, and the grouped gate remain. |
| A1 | WVA assembly | `native-qualified` | Digest-bound `wvasm` applications are promoted for the accepted assembler surface. Probe 40 recovery now uses them for all seven WVA products selected by a scenario: three top-level objects plus the init, directory, boot-resource, and client process-image objects. The complete embedded-source C# path remains frozen recovery/differential evidence, but the normal recovery command no longer executes it. |
| I2 | WVO verification and inspection | `native-candidate` | One focused Windvale verification module now owns complete WVO admission for both the read-only shell and native publisher. The repinned paired inspector applications, exact candidate manifest, and digest-bound Windows/Linux launchers exist. Decision 0322's fixed matrix requires both launchers to agree on all thirteen stable rejection families without a live managed oracle. Decision 0502 additionally consumes and pins one exact console-verifier WVO oracle during current-host construction; that evidence does not widen WVO inspection coverage or change this standing. Grouped dual-host qualification and promotion to the ordinary path remain. |
| L1 | WVO linking | `native-candidate` | The standard flat-linker core, clean paired applications, exact candidate manifest, and digest-bound Windows/Linux launcher exist. Scale-safe relocation emission now handles the fourteen-input, 498-relocation Probe 40 image on the current Windows host without enlarging the 128 MiB native arena. Decision 0496 reconstructs the segmented staging, linking, and transport toolset. Decision 0501 then uses that distinct path plus an exact raw-lowerer WVO oracle to reconstruct the standard linker's WVB, fragment, and paired applications on the current Windows host without asking either target standard linker to construct itself. The route consumes retained same-release seeds; independent Linux execution, grouped cross-host qualification, clean previous-seed renewal, atomic installation, and promotion to the ordinary path remain. |
| P1 | Version-1 console PE/ELF packaging | `native-candidate` | The bounded Windvale materializer, paired packager and publisher applications, exact Stage 0 provenance manifests, and digest-bound Windows/Linux launchers exist. The launcher materializes to a private candidate, revalidates the completed PE/ELF in Windvale, and reuses the native durable sibling/reread/atomic-replacement transaction. Nineteen canonical structural mutations pass through the public publisher without .NET; the two larger-than-4-MiB rejection values pass through a distinct two-snapshot read-only verifier. A separate recipe streamer constructs both maximum valid applications as two bounded chunks plus a `WVCS 1.0` manifest and matches the complete Stage 0 PE/ELF identities. The ordinary and segmented projects now reconstruct exact WVB and WVO identities through digest-bound native front doors. Decision 0459 extends the verifier to the current WVB 1.11 variant surface and both verifier and publisher rebuild as exact native WVBs. Decision 0460 closes the measured native hosted enum-request rejection and repins the candidate hosted toolset. Decisions 0461 through 0465 add distinct Windvale-native construction/admission of exact `WVHV 1` verifier metadata, the complete compiler-verifier runtime header, exact `WVVR 1` projection, immutable native hashing of the verifier plus six service leaves, and retained recovery-only Windows/Linux request-tool containers; all production WVBs build through the native compiler and final requests match the frozen Windows/Linux oracles without using the C# source compiler. Decision 0466 adds those request products to the digest-bound hosted toolset and reconstructs both exact containers through the shared native packager without a C# process. Decision 0467 adds Windvale construction of the six-service `WVSQ 2` and uses the shared Windvale materializer to reproduce both complete service bundles. Decision 0468 admits the exact native-assembled verifier startup objects, derives their format-4 runtime/service layout, and reproduces both startup payloads through the shared Windvale instantiator. Decision 0469 derives the exact PE/ELF platform-owned regions from that admitted layout. Decision 0470 verifies the complete bundle against `WVHV` digests and composes exact format-4 PE/ELF bytes from the four native response products; its process products extend the candidate to 72 artifacts with no managed writer or target. Decision 0471 executes the packaged constructor and its generated verifier on Windows without loading .NET. Decision 0472 extracts one shared 45/24-entry startup target model for both production and completed-application verification. Decision 0473 requires all shared relocation targets and the canonical normalized startup digest before final construction. Decision 0474 adds exact completed-application release admission, native-built admission and publisher WVBs, digest-bound Windows/Linux publisher launchers, current-host durable publication and installed-verifier execution without .NET, plus permanent native rejection coverage. Decision 0475 moves the exact `WVVP 1` publisher metadata record, both target requests, embedded digests, private transaction offsets, and independent admission into a service-free Windvale constructor. Decision 0476 then admits the exact publisher WVB, native-lowered WVO, target startup/adapter/SHA objects, discovers `Main` plus both private transaction offsets, and emits versioned Windows/Linux layout, output-digest, and complete ordered external-target requests through eight native-built focused WVBs. Decision 0477 adds service-free Windvale instantiation of the startup, publication-adapter, and two-section SHA objects, resolving all 114/52 target-specific relocations and reproducing the six exact component slices already present in the canonical applications. Decision 0478 constructs the exact publisher-only Windows import page in Windvale from the admitted address, including all three descriptors and 17 lookup/IAT bindings. Decisions 0479 and 0480 materialize the complete Linux ELF and Windows PE in Windvale from the native base, admitted construction/object records, exact metadata, and Windows import response, matching both frozen applications byte for byte. Decision 0481 adds five hosted Windvale file tools that non-circularly construct exact metadata, assemble and instantiate object requests, construct Windows imports, and perform final PE/ELF materialization. One focused two-target pipeline reproduces both canonical publisher applications and preserves destinations on corruption and alias rejection. Recovery export plus native linking supplies the exact verifier image and entry; no target-specific publisher image writer remains semantically owned only by frozen Stage 0. Decision 0482 adds the two missing WVSQ-to-WVHV-to-WVHR file boundaries, packages all eleven publisher-construction commands for both hosts, and gives one ordinary candidate command plus a six-case native owner the complete WVB-to-Windows/Linux-publisher path without .NET. Decision 0483 adds a separate exact Windvale admission module and natively built WVB for the two completed publisher identities without creating a self-digest cycle. Decision 0484 replaces only its unsupported platform-name comparison shape, then natively lowers and pins the exact admission WVO. Decision 0485 allocates distinct read-only `WVHV` profile 8 and constructs paired digest-pinned admitters. Decisions 0486 and 0487 add a separate non-circular promoter and extend the native container pipeline with exact publisher/promoter roles, paired promoter applications, current-host durable installation, and publisher-to-verifier execution without C#. Decision 0488 adds exact role 2 for the general WVB publisher, natively rebuilds and lowers its current source, and constructs paired applications matching the frozen Stage 0 writer without using it. Decision 0491 restores current-host build-driver self-convergence; Decision 0492 reconstructs and repins the complete 72-artifact hosted-container toolset and returns the focused Windows and cross-target Linux package lane to 5/5. Decision 0493 reconstructs all three publisher-overlay bases, the publisher/promoter/WVB-publisher applications, the profile-8 admitter pair, and the 48-artifact construction inventory without a managed writer. After the affected report assertion was reviewed, the remaining exact promoter-to-publisher-to-verifier and WVB-publication cases passed 3/3 without restarting the first twelve passing cases. Independent Linux execution, grouped qualification, promotion, and release integration remain. |
| P2 | Hosted format-2 WVB packaging | `native-candidate` | `Tools/Native/Package-Hosted-Wvb.cmd` and `.sh` compose the complete digest-bound 19-tool compiler-family path and passed paired success, invalid-input, preservation, and private-cleanup smokes on Windows and Linux for the last candidate snapshot. Both accept an explicit cross target; the focused Windows path reconstructed the established Linux container exactly, and Decision 0438 uses it to retain paired native-built UEFI packager containers. Decision 0460 added exact Windows/Linux request and service candidates that admit WVB 1.11 records and variants, including a nominal directory with zero enum members. The 57-artifact snapshot was fully repinned and all three focused current-host package cases passed with exact Windows and cross-target Linux output plus rejection preservation/cleanup. Under [Decision 0415](../Decisions/0415-Managed-Hosted-Tool-Aot-Recovery-Lane.md), the corresponding 38 managed tool-container targets are no longer accepted by ordinary `windvale compile` or `windvale aot`; `windvale recovery-aot` retains them explicitly for Stage 0 reconstruction and differential evidence. Decision 0492 reconstructs and repins the complete 72-artifact candidate after the shared startup, file-input leaves, and profile-2 geometry changed. The focused Windows owner passes all five exact, cross-target, rejection-preservation, and cleanup cases without .NET. A distinct read-only verifier profile is a P1 extension rather than an alias of compiler-family profile 2. Independent Linux execution, grouped qualification, and promotion remain before this row becomes `native-qualified`. |
| N1 | Accepted-subset WVB to WVO lowering | `native-candidate` | The current Windvale lowerer admits up to 512 scalar/control functions with arbitrary exported-Main order, fewer than 2,048 combined parameters/locals, 32,768 code bytes, and 8,192 instructions per function under the retained 2,048-cell frame check, bounded forward/self/cyclic calls, zero through 64 `i32`/`bool`/`text`/`u8`/`u32`/`bytes`, enum, or record helper parameters across ABI 22's four register positions and canonical 16-byte outgoing stack cells, checked `u32` add/subtract/multiply/divide/remainder, complete bounded `u32`/`u8` comparisons, lossless `u8`-to-`u32` conversion, up to 64 immutable text/bytes/i32 declarations with borrowed descriptor locals, conversion, slicing, length, and little-endian reads, service-backed text concatenation, UTF-8 validation/conversion, quoting, and unsigned formatting, bounded generation-owned bytes concatenation plus one-byte `u8`, bounded two-byte `u16`, and four-byte signed/unsigned little-endian construction, up to 64 nominal declarations with enum locals/constants/comparisons/name lookup including nonzero-first tables, and record-bearing functions with up to 256 compactly indexed declared record locals, 1,024 blocks, 8,192 instructions, 128 record values per block, and canonical scalar capability calls. Packed versioned storage evidence, staged layout entries, append-only leader/reachability and record-local evidence, bounded per-block liveness construction, validated segmentable WVO region ownership, bounded function-code/relocation batching, and an immutable exact-position object-publication cursor preserve those limits without constructing one complete code value. A focused hosted shell writes nonempty cursor values as distinct bounded chunks and writes a versioned `WVOP 1` manifest last without claiming atomic publication. One shared capability-free reader owns canonical serialization and rejects malformed magic, version, size, limit, index, position, length, and final-coverage evidence before host mutation; a scalar ABI-22 bridge carries the same statuses and every revalidated object/count/entry extent to a fixed native caller without adding a service or platform parser. A second segmented reader validates the compiler-produced WVO header, `.text`, optional `.rodata`, exact metadata boundaries, 32 MiB section extents, and minimum symbol/relocation tail from bounded chunks; its scalar bridge exposes only revalidated evidence. A third bounded reader validates the complete compiler-produced symbol chunk: sequential data and helper names, Main's omitted ordinal and exact code gap, section coverage, optional padding, and the final relocation-table extent. A fourth reader validates canonical ordered section-zero `Relative_i32` relocation records and their data-symbol targets, exposes the exact text-chunk count, and checks every owned zero placeholder plus the separate `0x90` padding chunk. A fifth typed cursor replays the retained publication plan, binds every nonempty actual value to its exact manifest position and length, compares arbitrary code, data, and metadata bytes completely, and requires final coverage without joining one whole WVO. A sixth resource plan reserves native snapshot ordinals zero and one for the input and manifest, admits at most 62 exact canonical chunk names into the remaining 64-entry table, rejects exact control/chunk collisions, and drives one hosted admission execution that reads each name once and validates the same borrowed chunk without touching the destination. Fixed Windows/Linux WVA adapters now validate that table and every reopened resource against its immutable snapshot, reject destination aliases by native file identity, reload the immutable snapshot ordinal for every multi-iteration write and reread, write only admitted chunks to an exclusive sibling, flush and reread through exact EOF, atomically replace, and clean pre-replacement scratch. Exact Windows/Linux staging producer packages now preserve the existing six-capability/ten-service Windvale tool, and the current-host native producer/publisher processes compose on the small canonical fixture without loading .NET. An exact five-artifact manifest and digest-bound Windows/Linux launchers now expose the current whole-object accepted-subset lowerer plus its native-produced fixed WVO vector; the tool WVB builds through the qualified native front door, and Decision 0497 reconstructs both exact host containers through the retained segmented native toolset on the current Windows host. Those launchers now lower into a private whole-object candidate and invoke a digest-bound five-service WVO publisher that shares complete portable admission with the inspector and reuses the native durable replacement transaction. It reproduces Stage 0's exact general-call, descriptor-call, unsigned-arithmetic and conversion, large-module, large-function, large-record-planner, `Sum-Data.wv`, compiler-produced `Function-Only.wv`, `Data-And-Text.wv`, and `Nominal-Types.wv`, static-descriptor, text-service including maximum `u32` formatting, focused enum/signature, direct-record, record-call, multiple-record-call, process-input, file-read, file-write, console-output, and diagnostic-output WVO objects and composes through link/package/execute. The ordinary and segmented console-packager closures reconstruct exact 692,425-byte and 789,653-byte WVOs through that same native launcher. Verifier-scale native staging now lowers, publishes, independently verifies, and exactly reconstructs a seven-chunk 1,049,615-byte WVO without widening the emitter arena or loading .NET. Bounded producer/publisher staging clears the former 128 MiB lifetime blocker. Decisions 0420 and 0422 then reconstruct the exact current 409-function lowerer as paired two-fragment Windows and Linux native applications and use both to reproduce the descriptor-entry and baseline-JIT bridge WVOs. Paired artifact promotion, remaining scalar conversions and construction operations, broader nominal shapes, required-service serialization, the complete backend, and grouped qualification remain. |
| N2 | Complete native backend and baseline JIT | `managed-normal` | The qualified Stage 0 backend remains the complete implementation. A capability-free `WVJP 1` candidate proves Windvale-owned typed patch-plan production and independent runtime materialization for the exact canonical `Main() -> i32` constant-return WVB shape. A standalone Windows/Linux x64 publisher now runs that actual Windvale producer over canonical WVB inputs in a bounded RW/NX arena, copies its two returned plans into private storage, releases the producer arena, independently admits each plan, binds it to the exact `WVLT 1` graph, allocates RW/NX memory, copies six admitted bytes, transitions to RX, invokes results `42` and `-1`, forces a seal failure, and releases every allocation without loading .NET. The bridge WVB rebuilds natively. It was first reconstructed by the Windvale lowerer through the Stage 0 reference execution host after N1 admitted descriptor-returning `Main() -> bytes`; Decision 0420's current 409-function paired Windows/Linux native lowerers now independently reproduce the same retained WVO. Current-lowerer reconstruction passes on both hosts; paired lowerer promotion remains pending. Decision 0424's paired workflow reconstructs and executes both exact baseline-JIT publisher applications on Windows and Debian, including RW-to-RX publication, results `42` and `-1`, forced seal failure, and teardown without loading .NET. Decision 0495 gives the patch-plan self-test plus the five explicit publication behaviors one fixed six-case native retirement owner. This transfers durable evidence only: the Windows import-patching constructor remains recovery PowerShell, and general lowering, full WVB admission integration, runtime services, code-cache accounting, deterministic JIT/AOT breadth, current paired-host qualification, and promotion remain before cutover. |
| T2 | Complete Seed, OS, golden, malformed, and differential suites | `managed-normal` | The C# harness owns the broad suite. A digest-bound native retirement coordinator now aggregates 3,204 transferred fixed cases, including twenty fixed unsafe WVB boundaries, four exact positive assembler products, the four WVO export-renamer cases, eleven OS Probe object-producer cases, seven system-kernel target cases, two process-policy source-object cases, two process-object reconstruction cases, the fifteen hosted-verifier publisher construction/admission, durable-promotion, and WVB-publisher cases, three current-compiler reconstruction cases, three segmented compiler toolset reconstruction cases, three source-built WVB runner reconstruction/execution cases, three WVO inspector reconstruction cases, three standard Wv-Linker reconstruction cases, three console-application-verifier reconstruction and two-snapshot compatibility cases, three console-application-publisher reconstruction and independent-publication cases, six baseline-JIT patch-plan/publication cases, the source-to-AOT chain, 200 hostile linker inputs, 256 hostile PE/ELF candidates, 19 canonical version-1 mutations, 15 valid/mutated hosted format-2 candidates, two segmented maximum-plus-one rejections, two maximum-valid segmented constructions, two exact console-packager source reconstructions, four hostile-size WVO consumer cases, three native-link-to-UEFI cases, four native Probe 40 construction/preservation cases, 256 frozen WVO differential inputs, 200 frozen WVA mutations plus 69 positive register/control/relocation vectors, and the 2,000-case random-containment corpus independently of a live C# oracle; remaining reusable cases must move to versioned fixtures and manifests rather than being ported line for line. |
| O1 | Windvale OS boot-probe execution | `native-candidate` | `Verify-Os-Boot.ps1` now accepts one caller-supplied EFI application only after exact SHA-256 admission, runs it through the pinned QEMU/firmware boundary, and preserves both the supplied and run-private images without invoking the Stage 0 builder or `dotnet`. The normal, invalid-opcode, and general-protection native Probe 40 images pass their complete digest-bound serial contracts under pinned QEMU; the architecture faults report exact vectors 6 and 13 with error code 0. The two contained process-fault scenarios and promotion remain. |
| O2 | Windvale OS probe-image construction | `native-candidate` | Portable Windvale owns exact UEFI v3 construction plus independent untrusted-byte verification. A hosted Windvale packager consumes the real digest-bound native linker's exact flat image and reported entry, matches the frozen Stage 0 writer byte for byte, preserves a destination on rejection, and rebuilds through its native Project 1 front door. Its paired 278,528-byte PE/ELF containers now reconstruct without .NET. A manifest-bound normal-scenario seed initially froze eleven Stage 0-produced WVOs from Decision 0444. Decisions 0446 through 0456 now construct all eleven in the ordinary Windvale-native build: the final process object regenerates 463,531 embedded payload bytes from canonical Windvale sources and versioned records while retaining only a 46,678-byte reviewed architecture fixture. Decision 0489 extends the memory-object producer and ordinary builder to the exact invalid-opcode and general-protection variants. The Windows/Linux launchers construct eleven objects through native producers, assemble three top-level objects, link fourteen inputs, and package each of those three exact EFI images without invoking `.NET`; the frozen object inventory is empty. The current-host eleven-case producer, seven-case kernel-target, two-case policy, two-case process-object, and four-case image lanes pass exact construction, admission, malformed-input or overwrite rejection, and existing-output preservation as applicable. Stage 0 remains the explicit regeneration/differential path. Independent Linux execution, a UEFI-specific durable publication transaction, the two contained process-fault scenarios, and final qualification remain required before promotion. |
| W1 | Complete WebAssembly generation and verification | `native-candidate` | The normal static playground compiles source through direct import-free ABI-4 Windvale compiler Wasm, then verifies and executes the returned WVB through the separate ABI-3 interpreter Wasm. Decision 0504 makes the complete Windows generation-and-verification command native: it builds the source corpus and compiler WVBs through the native front doors, invokes the digest-bound native WVB-to-Wasm backend through a generic success-only launcher, and runs the strict Node.js engine and probe evidence without loading .NET. That removes `Verify-WebAssembly.ps1` from the direct-managed-entry inventory. Independent Linux execution of the same package and verification contract, reconstruction of the retained segmented generator/backend packages, cross-browser evidence, grouped qualification, and promotion remain. |
| G1 | Independent dual-host qualification | `managed-normal` | `.github/workflows/verify.yml` installs .NET and runs the managed Seed gate. Native replacement must retain Windows/Linux independence and fail-closed verification. |
| R1 | Homepage and playground release | `native-candidate` | The homepage workflow publishes the static playground and its digest-pinned native package without installing .NET. Independent release promotion evidence remains before this row is called qualified. |
| D1 | Local editable browser playground | `native-candidate` | `npm run dev:playground` builds and serves the static Monaco/native-WebAssembly application without starting Blazor or .NET. Cross-host promotion remains. |
| C1 | Clean bootstrap from documented native seeds | `native-candidate` | The versioned compiler WVB and paired native compiler seeds, copied-seed host launchers, exact accepted compiler output, and Stage 0 reconstruction route exist. The ordinary bootstrap verifier performs native Stage 1/Stage 2 self-convergence for the pinned baseline. Decision 0491 restores current-candidate convergence: the Windows-hosted native build driver reproduces its exact 1,101,068-byte WVB without loading .NET. Decision 0494 reconstructs the exact current 921,640-byte compiler WVB and paired Windows/Linux applications through the native source/bootstrap, staged lower/link, canonical transport, and package path without a managed writer. Decision 0496 reconstructs the three segmented compiler-process WVBs and paired applications from the retained native candidate on the current Windows host. Decisions 0501 and 0502 add narrow same-release construction ownership for the exact standard linker and console-application-verifier candidates; neither is clean or previous-seed bootstrap evidence. Independent Linux reconstruction and execution, the current full Stage-2 run, paired-host promotion, reconstruction of other accepted tools, a non-circular previous-seed renewal, and consumption from a later release remain. |
| C2 | Final digest-bound Stage 0 recovery archive | `missing` | Produce and verify one final Windows/Linux recovery release before deleting retired managed source. |

N1 support note: Decision 0496 exercises the existing segmented lowering,
linking, transport, and packaging contracts while reconstructing all nine exact
process artifacts on the current Windows host. It adds construction evidence
for the accepted subset but no new source semantics, WVO shapes, lowering
operations, or service serialization. The retained segmented candidate remains
the seed, and independent Linux reconstruction and execution remain open.

Current N1/C1 update: [Decision 0497](../Decisions/0497-Native-Wvb-To-Wvo-Reconstruction.md)
supersedes the N1 row's older current-generation and Stage 0-construction
wording. The current 414,298-byte accepted-subset lowerer WVB and its exact
5,972,480-byte Windows and 5,971,968-byte Linux applications now reconstruct
through the retained segmented native toolset on the current Windows host, and
the unchanged fixed WVB/WVO vector remains exact. This closes only the managed
application-writer seam for that current candidate. The retained seed,
independent Linux reconstruction and execution, complete-backend coverage,
promotion, grouped qualification, and non-circular bootstrap evidence remain.

Decision 0498 T1/T2/P1/C1 update: at that decision, the native retirement plan
contained 39 focused suites and 3,192 fixed cases.
[Decision 0498](../Decisions/0498-Native-Console-Packager-Application-Reconstruction.md)
preserves the existing two-case WVB/WVO source owner and adds a separate
four-case owner for the two candidate inventories and both paired application
reconstructions. The P1 ordinary and segmented packager candidates now
reconstruct through the retained native source, lowering, linking, and
hosted-container path on the current Windows host. This advances C1
reconstruction of accepted tools, but it consumes retained same-release
candidates. Independent Linux reconstruction and execution, clean
previous-seed renewal, promotion, grouped qualification, broader P1 coverage,
and the final recovery archive remain.

Decision 0499 narrow P1/C1 support update:
[Decision 0499](../Decisions/0499-Native-Wvo-Publisher-Reconstruction.md)
reconstructs the exact WVO publisher WVB-to-WVO-to-paired-application closure
through the retained native raw lowerer, exact WVO oracle, and role-3 publisher
pipeline on the current Windows host. This removes the managed writer as the
only constructor for that exact candidate. At that decision, its two-case
focused retirement owner raised the plan to 38 suites and 3,189 fixed cases
without changing an inventory standing, direct managed-entry count, or the machine-readable
inventory. Independent Linux reconstruction and execution, clean bootstrap,
qualification, promotion, and recovery deletion remain.

Decision 0500 WVO-inspector reconstruction update:
[Decision 0500](../Decisions/0500-Native-Wvo-Inspector-Reconstruction.md)
added one three-case owner for the exact candidate inventory, native
WVB-to-WVO-to-paired-application reconstruction, and current-host
compatibility/profile isolation. The active plan at that decision advanced to 39
suites and 3,192 fixed cases; the focused Windows owner passes 3/3 in 28.1
seconds. The route consumes retained same-release compiler,
lowerer, linker, and hosted-container candidates; independent Linux execution,
clean previous-seed renewal, promotion, grouped qualification, and recovery
deletion remain.

Decision 0501 narrow L1/C1 support update:
[Decision 0501](../Decisions/0501-Native-Wv-Linker-Reconstruction.md)
reconstructs the exact standard Wv-Linker WVB, raw-lowerer WVO oracle,
independently staged and transported fragment, and paired profile-4
applications on the current Windows host. Neither target standard linker
participates in its own construction. This advances accepted-tool construction
ownership without changing an inventory standing or the machine-readable
inventory. The route still consumes retained same-release seeds; independent
Linux reconstruction and execution, clean previous-seed renewal, grouped
qualification, atomic installation, promotion, and recovery deletion remain.

Current narrow I2/C1 support update:
[Decision 0502](../Decisions/0502-Native-Console-Application-Verifier-Reconstruction.md)
reconstructs the exact console-application-verifier WVB, raw-lowerer WVO
oracle, linked fragment, and paired profile-7 applications on the current
Windows host. Its three-case owner covers inventory, exact reconstruction, and
the current-host two-snapshot compatibility/rejection boundary. Together with
Decision 0501's three-case owner, the active plan advances from 39 suites and
3,192 cases to 41 suites and 3,198 cases. This adds narrow I2 oracle-consumption
and C1 accepted-tool construction evidence without changing either standing or
the machine-readable inventory. The route consumes retained same-release seeds;
independent Linux reconstruction and execution, clean previous-seed renewal,
grouped qualification, atomic installation, promotion, and recovery deletion
remain.

Current narrow P1/C1 support update:
[Decision 0503](../Decisions/0503-Native-Console-Application-Publisher-Reconstruction.md)
reconstructs the exact console-application-publisher WVB, raw-lowerer WVO
oracle, linked fragment, target bases, and paired applications through explicit
publisher-overlay variant 4 on the current Windows host. Neither target
publisher participates in its own construction. This removes the managed
application writer as the only constructor for that exact P1 candidate and
adds narrow C1 accepted-tool construction evidence without changing either
standing, the direct managed-entry count, or the machine-readable inventory.
It advances the fixed native plan from 41 suites and 3,198 cases to 42 suites
and 3,201 cases. The route consumes retained same-release seeds;
independent Linux reconstruction and execution, clean previous-seed renewal,
grouped qualification, atomic installation, promotion, and recovery deletion
remain.

The Decision 0503 focused current-Windows-host owner now passes 3/3 in 68.6
seconds, and the established roles 0-through-3 publisher pipeline passes 15/15
in 188.7 seconds after the variant-4 extension. This completes the narrow
current-host P1/C1 evidence recorded above without changing either standing or
the machine-readable inventory. Independent Linux execution, grouped
qualification, clean bootstrap, promotion, and recovery deletion remain.

Current E1/C1 support update:
[Decision 0509](../Decisions/0509-Native-Wvb-Runner-Source-Reconstruction-And-Step-Reporting.md)
closes the runner's omitted-module boundary and builds the exact 121,593-byte
WVB from current source before producing its 1,078,577-byte WVO and paired
profile-5 applications. [Decision 0510](../Decisions/0510-Native-Foundation-Build-Inspect-And-Execution-Transfer.md)
aligns the fixed execution budget with the ordinary CLI default and advances
all four current product digests without changing their sizes. The normal
digest-bound launchers consume that current candidate, and its focused Windows
owner passes 3/3. The fixed coordinator
remains 43 suites and 3,204 cases, and the direct managed-entry inventory is
unchanged. Independent Linux execution, capability-bearing execution, grouped
qualification, promotion, and recovery deletion remain.

Current W1/direct-entry update:
[Decision 0504](../Decisions/0504-Native-WebAssembly-Generation-And-Verification.md)
replaces every normal managed invocation inside `Verify-WebAssembly.ps1` with
the native source-to-WVB front doors and a manifest-bound native WVB-to-Wasm
launcher. The complete current-Windows verifier passes in 1,619.5 seconds; its
strict engine phase passes in 1,239.5 seconds, followed by the record-arena and
compiler probes. The command remains the direct standalone owner rather than
being added to the 3,204-case fixed coordinator because it is a long,
Windows-only focused gate. Until a paired Linux owner exists, changed-file
planning reports the explicit `webassembly-native-verification` evidence gap
instead of invoking a managed or unfiltered fallback. This removes one normal
direct managed entry point without changing W1's `native-candidate` standing.
Independent Linux execution, package reconstruction, cross-browser evidence,
grouped qualification, promotion, and recovery retirement remain.

Current T2 front-door transfer:
[Decision 0505](../Decisions/0505-Native-Seed-Front-Door-Qualification-Smoke.md)
moves four representative Project 1 builds, two WVB verifications, two WVB
inspections, and one malformed-project preservation check inside each broad
Seed qualification script to one paired five-case native helper. That removes
nine managed invocations from each host script while retaining exact products
and diagnostics. The current Windows helper passes in 2.8 seconds. T2 remains
`managed-normal`, and the direct inventory remains twelve files: the broad
Seed scripts still use .NET for profiling and capability-bearing execution,
the harness, and many later qualification phases, while the GitHub workflow
remains managed. Independent
Linux execution, the reported changed-file evidence gap, the remaining
broad-suite transfers, grouped qualification, and recovery retirement remain
open.

Current T2 console-AOT transfer:
[Decision 0506](../Decisions/0506-Native-Seed-Console-Aot-Qualification-Smoke.md)
consumes that helper's exact 494-byte `Sum-Data.wvb`, lowers and independently
admits its 3,288-byte WVO, requires the complete flat-link map and 3,104-byte
image, and packages both established version-1 console applications through
native front doors. The Windows helper passes in 1.1 seconds and executes the
5,120-byte PE to result `29`. This removes two more managed invocations from
each host script, eleven cumulatively with Decision 0505, without changing the
three normal plus nine recovery direct-entry inventory. T2 remains
`managed-normal` because the broad scripts still use .NET for general
execution, the harness, and later qualification phases. Independent Linux
execution, the explicit `seed-native-console-aot` gap, remaining broad-suite
transfers, GitHub cutover, grouped qualification, and recovery retirement
remain open.

Current T2 WVB-execution transfer:
[Decision 0508](../Decisions/0508-Native-Seed-Wvb-Execution-Qualification-Smoke.md)
extends the paired native front-door helper from five to eight cases and runs
the exact Sum, Foundation-header, and composed-project WVBs through the current
Decision 0507 runner. The current Windows helper passes in about four seconds,
requires exact results `29`, `1`, and `42`, and proves that execution preserves
all three input modules. This removes three more managed invocations from each
host script, fourteen cumulatively with Decisions 0505 and 0506, without
changing the three normal plus nine recovery direct-entry inventory. T2 remains
`managed-normal` because profiling, capability-bearing execution, the broad
harness, and later qualification phases still use .NET. Independent Linux
execution, remaining broad-suite transfers, GitHub cutover, grouped
qualification, and recovery retirement remain open.

Current T2 instruction-report transfer:
[Decision 0509](../Decisions/0509-Native-Wvb-Runner-Source-Reconstruction-And-Step-Reporting.md)
extends the paired native front-door helper from eight to nine cases and moves
the Sum fixture's exact overall-instruction report into the current source-built
runner. The current Windows helper passes all nine cases in 3.6 seconds and
requires result `29`, instruction count `203`, and input preservation. This
removes one more managed invocation from each broad host script, fifteen
cumulatively with Decisions 0505, 0506, and 0508. T2 remains `managed-normal`:
per-function profiling, capability-bearing execution, the broad harness, and
later qualification phases still use .NET. The three-normal plus nine-recovery
direct-entry inventory remains unchanged.

Current T2 Foundation transfer:
[Decision 0510](../Decisions/0510-Native-Foundation-Build-Inspect-And-Execution-Transfer.md)
extends the paired native front-door helper from nine to 24 transferred calls
and from four to twelve exact artifacts. It natively builds four Foundation
modules and four demos, inspects all four modules, and executes the Machine
Contracts, Byte Ordering, and Decimal Parsing demos to exact result `0`. The
current Windows helper passes in 6.3 seconds; the reconstructed runner owner
passes 3/3 in 49.8 seconds. This removes fifteen additional managed invocations
from each broad host script, thirty cumulatively. The Byte Construction demo's
plain execution and dynamic-value reports remain in the managed differential
lane because the native runner returns bounded failure `3015` for its current
4 MiB value shape. T2 therefore remains `managed-normal`, and the three-normal
plus nine-recovery direct-entry inventory is unchanged. Independent Linux
execution, capability-bearing execution, remaining broad-suite transfers,
GitHub cutover, grouped qualification, and recovery retirement remain open.

Decision 0511 T2 native service-source transfer:
[Decision 0511](../Decisions/0511-Native-Service-Source-Build-And-Inspection-Transfer.md)
extends the paired native front-door helper from 24 to 39 transferred calls
and from twelve to twenty exact artifacts. It natively builds the
native-stencil core/demo/bridge, UTF-8 core/bridge, integer-format core/bridge,
and shared service-code builder, then natively inspects the seven ownership
surfaces. Its focused Windows evidence passes in 16.1 seconds. This removes eight
managed compiles and seven managed inspections from each broad host script,
forty-five managed invocations cumulatively. The 20-million-step Stencil demo
execution remains managed, so T2 remains `managed-normal`; the three-normal
plus nine-recovery direct-entry inventory is unchanged. Independent Linux
execution, capability-bearing execution, remaining broad-suite transfers,
GitHub cutover, grouped qualification, and recovery retirement remain open.

Current T2 native I/O-service transfer:
[Decision 0512](../Decisions/0512-Native-Io-Service-Build-And-Inspection-Transfer.md)
extends the paired native front-door helper from 39 to 53 transferred calls
and from twenty to 31 exact artifacts. It natively builds the complete output,
file-output, and file-input source closures and natively inspects the three
public bridges. The current Windows helper passes in 31 seconds. This removes
eleven managed compiles and three managed inspections from each broad host
script, fifty-nine managed invocations cumulatively. Retained bridge-WVB and
platform-leaf comparisons remain in both broad scripts. T2 therefore remains
`managed-normal`; the three-normal plus nine-recovery direct-entry inventory is
unchanged. Independent Linux execution, capability-bearing execution,
remaining broad-suite transfers, GitHub cutover, grouped qualification, and
recovery retirement remain open.

## Normal-path audit result

Decision 0485 closes the read-only publisher-admitter packaging item left by
Decision 0484: distinct `WVHV` profile 8 now constructs paired digest-pinned
Windows/Linux applications through the native path, and the nine-case focused
owner covers exact construction, current-host acceptance, role swaps,
wrong-digest rejection, and input preservation. Durable one-snapshot promotion,
independent Linux execution, grouped qualification, and release integration
remain open under P1.

Decision 0486 begins that durable-promotion boundary without reopening the
snapshot race: a distinct Windvale promoter imports exact publisher admission
and the existing atomic publication state machine, then builds and lowers
through the native front doors to pinned WVB/WVO identities.

Decision 0487 completes the next boundary: explicit publisher/promoter roles
flow through the existing native metadata, identity, structure, target, object,
import, and PE/ELF materialization records while the original publisher bytes
remain unchanged. Paired digest-pinned promoter applications now install both
exact publisher subjects through the reused immutable-snapshot transaction, and
the installed current-host publisher installs an exact verifier. Independent
Linux execution, grouped qualification, promotion, and release integration
remain open under P1.

Decision 0488 adds a third exact overlay role for the current general WVB
publisher. The native build and lowerer reproduce its WVB/WVO, the paired
candidate applications match the independent Stage 0 writer, and the
current-host candidate durably publishes a canonical portable WVB. The
retained and candidate publishers still expose the same current
compiler/build-driver semantic-verifier boundary, so self-convergence and
front-door promotion remain open rather than being bypassed. Decision 0490
then indexes typed-local and control-flow evidence inside the shared portable
compiler-WVB verifier, retaining the 16-billion-instruction ceiling while
admitting the current build driver in 8.4 seconds. The native role-2 pipeline
reproduces the repinned Windows and Linux WVB publishers byte-for-byte without
.NET. Decision 0491 then removes redundant compiler-front-end allocation,
gives build-driver profile 2 a bounded 224 MiB text arena and 8 KiB snapshot
name stride, and preserves exact runtime failures through the hosted startup.
The current Windows native build-driver application reproduces its exact
1,101,068-byte WVB in one current-host run. Decision 0492 reconstructs the
complete 72-artifact hosted-container toolset and passes its focused Windows and
cross-target Linux package lane. Decision 0493 reconstructs the dependent
48-artifact publisher construction inventory and all four Windows/Linux
publisher-overlay application pairs; the affected final three current-host
publication cases pass without rerunning the preceding twelve. Independent Linux execution, dual-host
qualification, and front-door promotion remain open.

[Decision 0457](../Decisions/0457-Normal-Path-Dotnet-Audit.md) confirms that
ordinary build, inspect, execute, assemble, lower, link, package, publish, Probe,
website, and deployment routes are already .NET-free. Twelve files now contain
direct managed invocations: three are normal broad verification/release entry
points and nine are explicit recovery commands. Decision 0458 removes the one indirect normal
call from `Verify-Changed.ps1`: it now selects focused native owners and refuses
named gaps without invoking .NET. Decision 0459 then closes WVB 1.11 variant
admission in the Windvale verifier while leaving native variant execution and
hosted packaging explicit. Decision 0504 then removes the standalone
WebAssembly verifier from that direct list. The remaining retirement work is
therefore focused backend/runtime and broad-suite gap closure, paired Linux
evidence, final GitHub orchestration cutover, and the digest-bound recovery
archive—not a line-for-line rewrite of the managed harness.

Decisions [0423](../Decisions/0423-Compiler-Scale-Native-Lowerer-Admission.md),
[0425](../Decisions/0425-Compiler-Scale-Native-Wvo-Resource-Staging.md), and
[0426](../Decisions/0426-Fixed-Space-Compiler-Scale-Hosted-Sha256.md)
advance N1 beyond the older limits embedded in the ledger row. The accepted
candidate now owns 256 static-data declarations, 64 records plus 64 enums,
1,024 declared record locals, 256 produced record values per block, packed
record interference, and the complete admitted `u32` bitwise/shift family.
The final staging producer preserves non-code boundaries and coalesces only
consecutive code steps. It stages the pinned 413-function compiler into the
exact 27,458,862-byte WVO as 36 resources plus a 456-byte manifest, clearing
the 62-resource immutable snapshot gate. The segmented native linker consumes
the same boundary contract in focused reconstruction without loading .NET.
The fixed-space portable SHA-256 path now clears the 27 MiB metadata region,
and the complete Windows native hosted pipeline reconstructs the exact
27,467,776-byte compiler seed application. That compiler passes a byte-exact
`Sum-Data.wv` smoke against the qualified native front door. Linux execution,
promotion, and grouped qualification remain open.

Current C1 update: the [native compiler seed bootstrap](../../Specifications/Windvale-Native-Compiler-Seed-Bootstrap.md)
pins the semantic-freeze compiler WVB and paired format-3 applications, consumes
the qualified native publisher, and is repinned to require the exact 921,640-byte
current compiler WVB without invoking .NET. Its copied-seed and altered-seed
contracts retain the earlier qualified evidence, but the repinned long convergence
route remains pending the current full Stage-2 run and final gate.
Decision 0491 separately proves that the current Windows-hosted native build
driver reproduces its own byte-identical 1,101,068-byte WVB, and Decision 0492
reconstructs the current hosted-container toolset. Decision 0493 reconstructs
the dependent publisher-overlay chain and its exact paired candidate
applications. Decision 0494 reconstructs the exact current 921,640-byte compiler
WVB and paired 27,666,432-byte Windows and 27,668,480-byte Linux applications
through the native source/bootstrap, staged lower/link, canonical transport, and
package path without a managed writer. Independent Linux reconstruction and
execution, the current full Stage-2 run, dual-host promotion, remaining
accepted-tool reconstruction, and a later release's use of this candidate as its
previous seed remain before C1 is complete. Decision 0496 also reconstructs the
three segmented compiler-process WVBs and paired applications on the current
Windows host. Because it consumes the retained same-candidate toolset, it closes
their managed-writer seam without satisfying the previous-seed condition.

[Decision 0427](../Decisions/0427-Native-Normal-Bootstrap-Verification.md)
makes that candidate the ordinary `Verify-Bootstrap` route. The former managed
Stage 0 → Stage 1 → Stage 2 convergence scripts now live under `Tools/Recovery`
with explicit managed names; they remain independent recovery and differential
evidence rather than a normal verification dependency.
[Decision 0428](../Decisions/0428-Native-Compiler-Self-Convergence.md) adds the
native Stage 1 → Stage 2 coordinator, whose Windows route now passes exact
byte equality after packaging and executing the newly built Stage 1 compiler.
Linux execution, promotion, and later-release consumption remain open.

Current N1 update: [Decision 0419](../Decisions/0419-Descriptor-Returning-Native-Main.md)
closes the smaller entry-shape gap exposed after Decision 0394's pruned staged
publisher closure. The Windvale lowerer now admits parameterless
`Main() -> bytes`, preserves the caller-owned result cell and shifted execution
context, publishes the descriptor while its caller retains the entry arena, and
restores the host's nonvolatile context register. It reconstructs the
baseline-JIT producer bridge WVO byte for byte, and the resulting object
executes inside the Windows publisher without loading .NET. Decisions 0420 and
0422 reconstruct the current 409-function lowerer and this bridge on both
permanent hosts. Tool promotion, native reconstruction of the remaining host
package inputs, remaining backend subset work, and grouped qualification remain
open.

[Decision 0420](../Decisions/0420-Multi-Fragment-Current-Lowerer-Reconstruction.md)
closes that Windows reconstruction gap. The shared hosted packager now places
the variable enum service after one through eight canonical fragments instead
of at a one-fragment-only fixed filename. The native source, segmented staging,
linking, transport, and hosted composition path reproduces the exact current
5,792,768-byte Windows and 5,791,744-byte Linux lowerer applications. Both
processes emit the descriptor-entry and retained baseline-JIT bridge WVOs byte
for byte. Paired candidate promotion, ordinary-launcher cutover, and grouped
qualification remain open.

[Decision 0422](../Decisions/0422-Atomic-Linux-Hosted-Executable-Mode.md)
records the first paired run's next exact finding: Debian produced the correct
hosted ELF bytes through native composition, but the shared transaction had
created the destination with staged-data mode `0600`. The native transaction
now consumes an admitted final-mode policy, preserves `0600` for WVO data, and
creates hosted application siblings at `0755` before atomic replacement.
GitHub run 31290136463 passes the ordinary and segmented smokes 2/2 on both
Windows and Debian. Candidate promotion, ordinary-launcher cutover, and grouped
qualification remain open.

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

Current L1 update: [Decision 0416](../Decisions/0416-Digest-Bound-Segmented-Compiler-Process-Front-Door.md)
checks both process families into one six-artifact digest-bound candidate and
adds paired Windows/Linux launchers for `WVB -> WVOP/chunks` and
`WVOP/chunks -> WVLI/image chunks`. The managed CLI no longer exposes either
application constructor through ordinary compile or AOT; exact regeneration
is recovery-only. The image process now reports its validated decimal `Main`
entry and chunk count, so orchestration does not decode `WVLI`. Canonical
4 MiB image rechunking, hosted-package composition, native replacement of the
two recovery-built host containers, Linux execution, and the grouped gate
remain open.

[Decision 0417](../Decisions/0417-Canonical-Compiler-Image-Transport.md)
closes the next L1 transport sub-item. A 23,836-byte Windvale tool validates a
strict `WVLI` plus at most 62 semantic source chunks, reads each immutable
source once, and emits one through eight canonical hosted fragment chunks with
every non-final chunk exactly 4 MiB. It preserves the image and `Main` entry,
writes a new `WVLI` last, runs without CLR modules, and is pinned behind paired
digest-bound launchers. Its managed container targets are recovery-only.
Hosted-package composition, complete compiler execution, native reconstruction
of the three process containers, Linux execution, and the grouped gate remain
open.

[Decision 0418](../Decisions/0418-Segmented-Compiler-Hosted-Package-Composition.md)
connects the digest-bound staging producer, segmented linker, canonical image
transport, and hosted-container toolset without a managed child process. The
shared hosted packager now has a validated canonical-image mode instead of a
duplicated second pipeline. The Windows composition reproduces the exact
profile-6 staging application byte for byte, and the unchanged ordinary
packaging smoke remains 2/2. Linux execution, the complete compiler run, native
construction of the three process containers, console-v3 recovery cutover,
and the grouped gate remain open.

Current L1/C1 update: [Decision 0496](../Decisions/0496-Native-Segmented-Compiler-Toolset-Reconstruction.md)
reconstructs those three WVBs plus their paired Windows/Linux applications
through the current-Windows-host native cross-target path. All nine products
match their exact candidate identities without a managed application writer.
The construction consumes the retained segmented candidate, so it does not
establish a non-circular bootstrap. Current full Stage 2, independent Linux
reconstruction and execution, promotion, later-release consumption, and the
grouped gate remain open.

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

[Decision 0395](../Decisions/0395-Standalone-Native-Hosted-Container-Planner.md)
adds the missing process-level plan producer. A 37,289-byte Windvale tool reads
the exact runtime header and produces the same `WVCD 1` layout and target plan
as the retained service-free fragment; paired packages and the public CLI target
exist, and current-host execution loads no CLR. Runtime/region resource
production, process-pipeline composition, Linux execution, promotion, and the
grouped gate remain open.

[Decision 0396](../Decisions/0396-Standalone-Native-Hosted-Container-Platform-Bytes.md)
adds the next process-level resource producer. One 29,793-byte Windvale tool
selects the existing portable Windows or Linux constructor from a successful
plan and emits the exact `WVWB 1` PE-header/import/relocation bundle or `WVLB 1`
ELF-header bundle. Paired packages and public CLI targets exist, and current-host
execution agrees byte-for-byte with the retained fragment without loading the
CLR. Startup instantiation, remaining runtime/resource production, complete
process composition, Linux execution, promotion, and the grouped gate remain.

[Decision 0397](../Decisions/0397-Standalone-Native-Hosted-Container-Startup.md)
removes managed startup-target projection from the candidate process pipeline.
One 42,508-byte Windvale tool admits the plan and exact canonical Windows/Linux
startup WVO, constructs `WVSI 1`, and emits the same `WVSD 1` instantiated code
as the retained fragment. Paired packages and public CLI targets exist, and
current-host execution loads no CLR. Remaining runtime/service resources,
segment-request orchestration, complete composition, Linux execution, promotion,
and the grouped gate remain.

[Decision 0398](../Decisions/0398-Standalone-Native-Hosted-Container-Runtime.md)
removes managed runtime-header request projection from the candidate pipeline.
One 22,956-byte Windvale tool admits a raw canonical metadata record, constructs
`WVHR 1`, verifies `WVHS 1`, and writes the exact 4,096-byte raw header consumed
by the standalone planner. Paired packages and public CLI targets exist, and
current-host execution loads no CLR. Metadata request construction,
service-bundle production, segment requests, complete composition, Linux
execution, promotion, and the grouped gate remain.

[Decision 0399](../Decisions/0399-Standalone-Native-Hosted-Container-Metadata.md)
removes managed metadata-constructor invocation from the candidate pipeline.
One 26,748-byte Windvale tool validates `WVHM 1`, constructs and admits
`WVHD 1`, and writes the exact raw 1,024-byte metadata record consumed by the
standalone runtime-header producer. Paired packages and public CLI targets
exist, and current-host execution loads no CLR. Native request construction
from immutable resources, service-bundle production, segment requests,
complete composition, Linux execution, promotion, and the grouped gate remain.

[Decision 0400](../Decisions/0400-Standalone-Native-Hosted-Service-Bundle.md)
removes managed service-bundle constructor invocation for one canonical segment.
One 20,144-byte Windvale tool validates `WVSQ 2`, constructs and admits
`WVSI 2`, and writes the exact immutable response consumed by the next evidence
boundary. Paired packages and public CLI targets exist, and current-host
execution loads no CLR. Native resource acquisition, ordered bundle requests,
complete composition, Linux execution, promotion, and the grouped gate remain.

[Decision 0402](../Decisions/0402-Native-Hosted-Metadata-Request.md) removes
managed digest projection and `WVHM` request construction from the candidate
pipeline. One 54,135-byte Windvale tool admits the canonical publication plan
and manifest, reads the actual immutable chunk resources, recomputes the native
fragment and ten service SHA-256 leaves, and emits the exact 576-byte request.
Paired packages and public CLI targets exist, current-host execution loads no
CLR, and the native build front door reproduces the Stage 0 WVB exactly.
Ordered service-bundle resource/request orchestration, segment requests,
complete composition, Linux execution, promotion, and the grouped gate remain.

[Decision 0403](../Decisions/0403-Native-Hosted-Service-Bundle-Request.md)
removes managed service-bundle segment selection and `WVSQ 2` byte construction
from the candidate pipeline. The reusable `WVSG 1` geometry maps bounded raw
fragment/service resources into planned image regions without conflicting with
the existing `WVRS` package store or trusting geometry as identity. One paired
native command produces an exact canonical request for a selected segment and
matches the frozen recovery oracle without loading the CLR. Ordered invocation,
`WVSI` response/evidence composition, final hosted-container segment requests,
Linux execution, promotion, and the grouped gate remain.

[Decision 0404](../Decisions/0404-Native-Hosted-Container-Segment-Request.md)
reuses `WVSG` for the exact six final application source regions and removes
managed `WVHT 1` segment selection and request-byte construction from the
candidate pipeline. A shared capability-free append state keeps both native
request roots focused while file acquisition remains explicit. Both current-
host request producers cross a two-resource boundary, match frozen recovery
oracles, and load no CLR. Ordered request/response execution, manifest
lifecycle, Linux execution, promotion, and the grouped gate remain.

[Decision 0405](../Decisions/0405-Native-Hosted-Publication-Request.md)
removes the preceding managed `WVPQ 1` construction seam. One focused paired
native command derives the fragment and canonical ten-service sizes from the
admitted `WVSG`, executes the Windvale publication planner internally, and
publishes only when all placements reproduce that geometry. The public current-
host application matches the frozen request oracle and loads no CLR. Ordered
request/response execution, manifest lifecycle, Linux execution, promotion,
and the grouped gate remain.

[Decision 0406](../Decisions/0406-Native-Hosted-Source-Geometry-Production.md)
removes managed `WVSG` construction from that candidate path. A paired native
command reads the canonical bounded fragment and ten service resources once,
derives exact logical extents and aligned placements, and self-admits the
manifest before publication. Raw resource production, ordered process and
manifest lifecycle, Linux execution, promotion, and the grouped gate remain.

[Decision 0407](../Decisions/0407-Native-Hosted-Enum-Service-Production.md)
removes the bounded managed constructor for variable service 7. Two focused
native processes derive the existing single-group `WVEQ 2` from verified WVB
nominal types and turn its admitted `WVEN 1` into the exact leaf-plus-metadata
resource. The focused current-host processes agree byte for byte with the frozen
recovery oracle and loads no CLR. Fragment resource production, the nine fixed
service resources, ordered manifest/process lifecycle, larger segmented enum
metadata, Linux execution, promotion, and the grouped gate remain.

[Decision 0408](../Decisions/0408-Native-Enum-Service-Fragment-Reconstruction.md)
removes the enum-service fragment from that remaining list. Focused source
extraction lets the existing digest-bound native lowerer reproduce the exact
WVO without a larger arena, and the digest-bound native linker reproduces the
exact raw fragment. The nine fixed service resources, ordered manifest/process
lifecycle, larger segmented metadata, Linux execution, promotion, and the
grouped gate remain.

[Decision 0409](../Decisions/0409-Native-Fixed-Service-Acquisition.md)
removes managed platform selection and staging for those nine fixed resources.
One paired Windvale process admits their exact target-specific sizes, snapshots
each once, and places them around the separately produced service-7 slot. The
downstream native metadata-request producer remains the single SHA-256 identity
gate over the actual staged resources. Ordered process and private-resource
lifecycle, complete composition, Linux execution, promotion, and the grouped
gate remain.

[Decision 0410](../Decisions/0410-Native-Hosted-Orchestration-Control.md)
removes the remaining managed `WVMI` serialization and `WVSG`-to-`WVHS`
projection from the candidate metadata path. One focused Windvale process now
owns both single-output control modes. Tool-package acquisition, ordered child
execution, final source-set production, segment iteration, private cleanup,
Linux execution, promotion, and the grouped gate remain.

[Decision 0411](../Decisions/0411-Native-Hosted-Container-Source-Set.md)
removes managed producer-response extraction, whole-bundle concatenation, and
six-region final `WVSG` construction. One paired native process admits all
producer envelopes, streams the native fragment and ten service digests against
runtime-bound metadata, emits bounded raw chunks, preserves empty Linux region
ordinals, and writes the self-admitted manifest last. Native final segment-set
`WVHM` construction, digest-bound ordered child execution, private cleanup,
Linux execution, promotion, and the grouped gate remain.

[Decision 0412](../Decisions/0412-Native-Hosted-Container-Segment-Manifest.md)
removes the last identified managed hosted-container format seam. One paired
native command derives canonical final segment resources from the admitted
plan, binds exact `WVHT`/`WVHU` envelopes and lengths, constructs `WVHM`, and
self-admits it through the existing segment-set core. The downstream native
admission/publisher remains the independent payload gate. Digest-bound ordered
child execution, private cleanup, complete composition, Linux execution,
promotion, and the grouped gate remain.

[Decision 0413](../Decisions/0413-Native-Hosted-Segment-Iteration-Control.md)
removes host-side decoding and failure-sentinel discovery from both remaining
bounded loops. The existing native request producers now admit their complete
plans, geometry, and immutable resources before reporting exact service-bundle
or final-container segment counts. The host adapter therefore owns only
digest-bound tool acquisition, ordered child execution, and private cleanup.
Complete Windows composition, Linux execution, promotion, and the grouped gate
remain.

[Decision 0414](../Decisions/0414-Digest-Bound-Native-Hosted-Container-Composition.md)
closes that Windows composition boundary. One exact inventory binds 19 WVBs and
their paired applications; the reviewed launcher sequences only native tools,
uses one private temporary directory, and reproduces the independent hosted
container byte for byte. Its end-to-end run also corrected a previously hidden
logical-source versus aligned-image metadata mismatch and added a focused
alignment-gap test. A paired .NET-free smoke script now also fixes exact output,
invalid-WVB preservation, and private-cleanup checks; its Windows half passes
2/2. Linux execution, artifact promotion, managed-entry-point cutover, and the
grouped gate remain.

Decision 0329 further advances T1 with a separate five-case unsafe-WVB matrix:
both digest-bound read-only launchers require exact semantic or typed-execution
reports and preserve each compact fixed input without a live .NET oracle.
Broader nominal/limit unsafe cases and seeded randomized containment remain.

[Decision 0347](../Decisions/0347-Fixed-Native-Nominal-Wvb-Rejections.md)
adds five fixed nominal-type cases to that same cohesive lane. Missing record
types and fields, duplicate record and nominal names, and mismatched enum
comparison now pass through both native readers without a live managed oracle.
Decision 0429 adds three positive exact-byte assembler objects through one
focused golden lane, and Decision 0432 adds the remaining compact scalar/SIB
source from the managed positive surface. Decisions 0430 and 0431 add ten
compact typed and nominal WVB rejections without adding a lane. Decision 0433
adds 17 typed byte/word positive WVA vectors to the existing differential lane,
and Decision 0434 adds 52 expanded register/control/relocation vectors. The
current 31-suite coordinator therefore owns 3,147 fixed cases after Decision
0456's two process-object reconstruction cases. Hostile
nominal/value-size limits and additional WVA inventory remain.

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

The companion JSON currently records 13 operational files across three lanes:

- verification: Seed, WebAssembly, and GitHub qualification;
- release: the managed independent-qualification workflow that gates publication; and
- recovery: the explicit Stage 0 compiler, tool, bootstrap, WebAssembly, and OS-image reconstruction scripts.

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
| `Operating-System/Windvale.Bootstrap` | Host-side Probe 40 image construction | Keep behind `Tools/Recovery/Rebuild-Os-Probe.ps1` until a native constructor reproduces every admitted scenario image. Boot execution no longer calls this owner. |
| `Tools/Windvale.Playground` and `Tools/Windvale.Playground.Engine` | The static files under `Tools/Windvale.Playground` are the normal browser product; its managed host and engine remain recovery/differential evidence | Keep the static application. Move or retire only the managed project/engine after the remaining WebAssembly artifact-production seam and final recovery gate close. |

Deleting any owner early would destroy recovery or independent evidence. After every normal responsibility has a qualified native owner, the managed projects move behind explicit recovery commands; source deletion is a final action after the complete Decision 0057 gate and archived recovery proof.

## Verification rhythm for the active retirement goal

Each coherent slice gets its affected tests reviewed first and then one narrow, quick local check. Passing results are reused while their relevant inputs remain unchanged. Temporary failures outside the slice are acceptable during the migration. Local Standard/Qualification and repeated GitHub qualification stay deferred until the remaining transfers are ready. Immediately before that final broad gate, update from the shared upstream branch, reconcile changes, regenerate current artifact identities once, and run the complete Windows/Linux qualification once.
