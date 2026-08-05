# .NET retirement inventory

> Inventory snapshot: 5 August 2026

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
| T1 | Fixed native portable tests | `native-candidate` | `Tools/Native/Test-Seed.cmd` and `.sh` run a twenty-two-case digest-bound plan covering scalar/control, static text/bytes, records, enums, three exact runtime failures, five malformed WVB envelope rejections, eight typed-execution corruptions, and one control-reachability corruption. Remaining limits, unsafe bytecode, randomized malformed data, and broader native fixture orchestration remain. |
| A1 | WVA assembly | `native-qualified` | Digest-bound `wvasm` applications are promoted for the accepted assembler surface. |
| I2 | WVO verification and inspection | `native-candidate` | The Windvale read-only core and paired applications exist as source-built candidates. Promotion and ordinary launchers remain. |
| L1 | WVO linking | `native-candidate` | The standard flat-linker core and paired applications exist as candidates. Grouped cross-host qualification and promotion remain. |
| P1 | Version-1 console PE/ELF packaging | `native-candidate` | Paired materializers exist for the bounded console profile. General hosted packaging and release integration remain. |
| N1 | Accepted-subset WVB to WVO lowering | `native-candidate` | The current Windvale lowerer handles one through eight scalar/control functions with arbitrary exported-Main order, bounded forward/self/cyclic calls, `i32`/`bool`/`u8`/`u32` helper shapes, complete bounded `u32`/`u8` comparisons, up to eight immutable text/bytes/i32 declarations with borrowed descriptor locals, conversion, slicing, length, and little-endian reads, service-backed text concatenation, UTF-8 validation/conversion and quoting, bounded generation-owned bytes concatenation, bounded enum locals/constants/comparisons/name lookup including nonzero-first tables, plus multi-block direct record construction, local copying, field reads, parameters, caller-owned returns, and scalar- or record-returning calls over deterministic frame storage. It reproduces Stage 0's exact general-call, `Sum-Data.wv`, compiler-produced `Function-Only.wv`, `Data-And-Text.wv`, and `Nominal-Types.wv`, static-descriptor, text-service, focused enum, direct-record, and record-call WVO objects and composes through link/package/execute. More functions, remaining scalar and byte-builder operations, descriptor calls/returns, capabilities, broader nominal shapes, and the complete backend remain. |
| N2 | Complete native backend and baseline JIT | `managed-normal` | The qualified Stage 0 backend remains the complete implementation. Windvale must own general lowering, verification, publication, runtime services, and deterministic JIT/AOT behavior before cutover. |
| T2 | Complete Seed, OS, golden, malformed, and differential suites | `managed-normal` | The C# harness owns the broad suite. Reusable cases must move to versioned fixtures and manifests rather than being ported line for line. |
| O1 | Windvale OS image construction and boot probes | `managed-normal` | `Operating-System/Windvale.Bootstrap` and `Verify-Os-Boot.ps1` still build and orchestrate the current images. |
| W1 | Complete WebAssembly generation and verification | `managed-normal` | The static browser demo executes without .NET, but the full generator and verification route still uses the managed tool. |
| G1 | Independent dual-host qualification | `managed-normal` | `.github/workflows/verify.yml` installs .NET and runs the managed Seed gate. Native replacement must retain Windows/Linux independence and fail-closed verification. |
| R1 | Homepage and playground release | `managed-normal` | Both release workflows publish the Blazor host with .NET. The static native browser path does not yet replace that release surface. |
| D1 | Local editable browser playground | `managed-normal` | `npm run dev:playground` still starts the managed Blazor host after building the editor package. |
| C1 | Clean bootstrap from documented native seeds | `missing` | Current recovery scripts reconstruct pinned native artifacts through Stage 0. A previous native release must rebuild the accepted toolchain without .NET. |
| C2 | Final digest-bound Stage 0 recovery archive | `missing` | Produce and verify one final Windows/Linux recovery release before deleting retired managed source. |

The next transfer should close a complete row or a clearly bounded part of one row. Candidate tools are not promoted merely because they compose once; their decoders, failure behavior, deterministic bytes, and both host packages remain part of the grouped gate.

## Direct managed entry points

The companion JSON currently records 11 operational files across four lanes:

- development: the local editable playground command;
- verification: Seed, bootstrap, OS, WebAssembly, and GitHub qualification;
- release: GitHub homepage/playground publication; and
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
| `Tools/Windvale.Playground` and `Tools/Windvale.Playground.Engine` | Editable browser host and current release packaging | Retire from normal use after a complete static/native browser build and release route exists. |

Deleting any owner early would destroy recovery or independent evidence. After every normal responsibility has a qualified native owner, the managed projects move behind explicit recovery commands; source deletion is a final action after the complete Decision 0057 gate and archived recovery proof.

## Verification rhythm for the active retirement goal

Each coherent slice gets its affected tests reviewed first and then one narrow, quick local check. Passing results are reused while their relevant inputs remain unchanged. Temporary failures outside the slice are acceptable during the migration. Local Standard/Qualification and repeated GitHub qualification stay deferred until the remaining transfers are ready. Immediately before that final broad gate, update from the shared upstream branch, reconcile changes, regenerate current artifact identities once, and run the complete Windows/Linux qualification once.
