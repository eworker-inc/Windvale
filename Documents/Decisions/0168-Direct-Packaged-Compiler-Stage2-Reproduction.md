# Decision 0168: Direct packaged-compiler Stage 2 reproduction

- Date: 2026-08-03
- Status: Cross-host qualified at exact commit `db20fef` in GitHub Verify run 30816153900
- Advances: both format-3 compilers to byte-identical canonical Stage 2 reproduction without loading .NET
- Retains: ABI 22, exact compiler/service bytes, the 64 MiB stack bound, the bounded runtime-data plan, and the Stage 0 recovery oracle

## Context

Decision 0167 proves that the raw Windows PE can compile a small source through real arguments, file snapshots, and output services. Canonical Stage 2 is materially stronger: twelve source modules drive the exact compiler through its largest measured ownership plan, 64,476,249 bytes of text-arena use, deep generated call paths, repeated file snapshots, and the complete 599,868-byte WVB encoder result.

The first direct attempt reached native code but faulted at image RVA `0x001F8825`. The function subtracted a 16,240-byte frame and immediately stored at the new `RSP`. PE had reserved the exact 64 MiB stack but committed only 64 KiB. Current Windvale native prologues do not probe Windows guard pages, so a frame larger than one page can jump over the guard and turn otherwise bounded stack use into access violation `0xC0000005`. The retained Stage 0 executor already reserves and commits the complete 64 MiB stack before calling the same fragment.

## Decision

- Keep the 64 MiB stack reserve and set the PE stack commit to the same 64 MiB. This matches the already verified Stage 0 execution contract, preserves the fixed bound and RW/NX protection, and relies on ordinary demand paging for physical residency. General Windows stack probing remains a future native-backend contract rather than an implicit property of this package.
- Advance the canonical 17,157,120-byte Windows application identity to SHA-256 `356bd9c6be1a927017e987728b479d105f9852c0c7aad1b8b9e93202ba64010f`. Decision 0167's earlier hash remains evidence for the first small-source candidate; it is not the current Stage 2-qualified container.
- In the existing exact-compiler AOT transport case, reuse the one compiled ABI-22 fragment, one verified platform bundle, and already constructed container. Replace the packaged small-source smoke run with the same canonical twelve-source inventory used by the retained native-executor oracle. Do not add another native compiler construction or another child run.
- Require process status zero, the exact status line `functions=328 code-bytes=481356 module-bytes=599868`, and byte identity with the canonical Stage 0 WVB, whose SHA-256 remains `9673bf3331763181f443ec67b7a513bc66daa718969f7f6b0d197a4186071066`.
- While that same Windows child is running, refresh and union its native module snapshots. Require the declared `KERNEL32.DLL` and `SHELL32.DLL` adapters and reject `clr.dll`, `mscoree.dll`, `mscorwks.dll`, `coreclr`, `hostfxr`, or `hostpolicy` modules. The independent PE verifier continues to require a zero CLR directory and the exact thirteen ordinary imports.
- Add the equivalent conditional Linux gate over the already constructed pinned ELF. It writes exact mode, passes the same source inventory, requires the same Stage 2 WVB, samples `/proc/<pid>/maps`, requires the raw ELF mapping, and rejects .NET host/runtime mappings. The ELF bytes remain SHA-256 `42f3f947cccca8e44c279afce1b6e944682dc440e0e9cda6546883898d951f31`.
- Share one inventory writer between the packaged and retained-executor tests so source order, UTF-8 encoding, filenames, and output placement cannot drift.

## Local evidence

The focused Release case passes on Windows with zero build warnings. The raw PE runs for the complete compile, reports success, and produces exactly 599,868 bytes equal to the Stage 0 compiler WVB. Live module sampling observes both declared Windows adapter libraries and no named .NET loader or runtime module. The parent test process uses .NET to construct and independently verify the candidate; the child does not.

The local Windows host has no WSL, container engine, Linux user-mode emulator, or other configured Linux execution environment. The equivalent Linux-kernel claim therefore comes from the independently checked clean GitHub host described below rather than from local emulation.

This milestone verifies compiler output identity, not atomic distribution publication. The current exact `file.write_bytes` capability deliberately performs durable but non-atomic replacement under its existing contract. A later gate must place verified compiler/executable artifacts through the repository's outer unique-sibling plus atomic-replacement publication workflow without silently changing that source-visible capability.

## Cross-host qualification

Exact commit `db20fefaa3333b7b78392ba12141d1ae2b6bb0c2` passes GitHub [Verify run 30816153900](https://github.com/eworker-inc/Windvale/actions/runs/30816153900). Windows and digest-pinned Debian 12 each complete a zero-warning Release build, all 87 Seed tests, all 38 OS tests, the golden compiler contract, and the native CLI gate. Windows reports the exact-compiler case in 22.324 seconds; Debian reports it in 20.393 seconds.

Both hosts independently report the same 17,130,441-byte native compiler, 17,147,219-byte WVO, link map, platform bundles, `WVHA` records, runtime headers, 17,157,120-byte PE, and 17,158,144-byte ELF with every pinned SHA-256 unchanged. The current-host branch on Windows executes the PE; the current-host branch on Debian writes mode `0755` and executes the raw ELF. Each child consumes the same twelve-source inventory, returns zero, emits the exact status line, reproduces the 599,868-byte canonical WVB, exposes its required host boundary, and rejects named CLR/.NET host or runtime mappings. This qualifies paired direct reproduction without claiming that the Stage 0 parent processes are .NET-free.

## Consequences

Both candidates are now real self-hosting compiler executables in the narrow Stage 2 sense: each consumes the canonical compiler sources and reproduces the canonical compiler WVB without loading .NET. Stage 0 remains necessary to reconstruct, parse, compare, and recover the native package until the broader documented native-retirement gate is complete.

Decision 0169 supplies recoverable atomic publication around the already verified artifacts. The remaining package work is to retain clean-checkout Stage 0 recovery provenance and replay the public project-manifest targets under the independent dual-host gate; the broader native-tool and runtime conditions of Decision 0057 remain separate.

## Reconsider when

- The native backend gains an independently verified Windows stack-probe contract for every frame larger than the guard interval.
- A qualified Windows host cannot commit the fixed 64 MiB bounded stack at process creation.
- Linux direct execution reveals a System V startup, stack, syscall, mapping, or file-service mismatch.
- Atomic output becomes a new explicitly versioned capability rather than an outer packaging operation.
