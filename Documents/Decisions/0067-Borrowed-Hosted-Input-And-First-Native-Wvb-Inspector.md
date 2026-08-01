# Decision 0067: Borrowed hosted input and first native WVB inspector

- Date: 2026-08-01
- Status: Cross-host qualified on Windows x64 and Debian x64; OS consumer qualified under pinned QEMU
- Refines: [Decision 0066](0066-Borrowed-Bytes-And-Unsigned-Native-Values.md)'s ABI-7 borrowed value boundary
- Advances: The first useful Windvale-written hosted program through native execution

## Context

ABI 7 could decode immutable bytes already present in a module, but a native program could not receive a resource name or read a real file. That left every useful compiler and binary-inspection program dependent on the reference interpreter even when its byte operations were otherwise native-eligible.

The input boundary must not introduce ambient process access, native paths or handles, caller-owned mutable memory, unbounded callback allocation, or values that outlive a run. It must preserve the reference runtime's argument limits, file snapshot behavior, capability authorization, and `WVR302x` failures on Windows and Linux while keeping generated fragment bytes platform-neutral.

## Decision

- Advance the experimental target to `x86-64-wvb-baseline-v8` and native ABI version 8. ABI-7 artifacts remain historical evidence and are not accepted through a compatibility branch.
- Represent both borrowed `text` and borrowed `bytes` in zero-initialized 16-byte value cells as pointer `u64`, length `u32`, and zero reserved `u32`. Static text now uses a real descriptor instead of a private data identity. Text parameters and initialized text locals copy descriptors exactly like bytes.
- Retain execution-context version 1 and its 32-byte layout. Advance the runtime-service table to version 2 and 40 bytes with closed entries for `console.write_line`, `process.argument_count`, `process.argument`, and `file.read_bytes`.
- Admit hosted modules only when every declared capability is one of those four exact canonical signatures. Required native services are distinct and ordered canonically. Authorization and implementation availability are checked before executable memory is published.
- `process.argument_count` returns the prevalidated bounded count. `process.argument` writes one borrowed UTF-8 descriptor for the requested index. `file.read_bytes` accepts a borrowed UTF-8 resource name and writes one borrowed-byte descriptor. `console.write_line` consumes any verified borrowed-text descriptor, including text returned by the host.
- Give every native execution one buffer owner. It encodes each requested argument at most once, copies each distinct file snapshot at most once, validates all service input ranges against fragment data or its own allocations, and frees every allocation after native return. Returned descriptors are immutable borrows valid only for that run.
- Move bounded file snapshot acquisition onto `Hostedˉresourceˉcontext.Readˉfileˉbytes` so the reference host and native adapter share one source of limits, snapshot reuse, adapter error mapping, and resource identity. Native callbacks preserve exact `Runtimeˉexception` codes such as `WVR3020` and `WVR3022`; no managed exception unwinds through machine code.
- Keep one Windvale-owned generated-code service convention and use runtime-owned exact Windows/System V thunks to adapt it. Thunks preserve the shared instruction, call-depth, and context registers. Generated code remains byte-identical across hosts.
- Extend native IR validation and the independent x86-64 decoder over borrowed text construction/copy, all four service-table offsets, argument/result slots, failure edges, immutable UTF-8 provenance, canonical service lists, and scalar/descriptor separation. Corrupt service loads or noncanonical metadata fail before WVO publication or W^X allocation.
- Add `Examples/Foundation/Wvb-Header-Inspector.wv`. It receives one filename, prints the host-returned name, reads one real canonical WVB through a shared snapshot, checks `WVB1` plus major/minor format `1.6`, prints `wvb-header=pass`, and returns deterministic exit codes for malformed headers.
- Advance the firmware probe identity to version 10 because the OS AOT consumer is rebuilt through ABI 8. The guest continues to use a zero service-table pointer; this decision does not add hosted file/process services to the OS.
- Retain C#/.NET as the Stage 0 selector, verifier, Windows/Linux adapter, differential oracle, and recovery implementation. Native hosted input is progress toward a self-hosted stack, not permission to retire the reference implementation.

## Qualification evidence

The focused Windows test compiles the checked-in inspector twice and obtains identical native code. The reference interpreter and W^X executor both read a real compiler-produced WVB, emit the same two output lines, return zero, and call the file adapter only once despite two language-level reads. Independent verification rejects corruption of every service-table load and a noncanonical service list. Native failure tests agree with the reference resource contract on out-of-range arguments (`WVR3020`) and missing files (`WVR3022`).

Exact candidate commit `d970c2789058d37c7059264fcbd76e164a01a939`, tree `1723f1ee8342e36e3237f14b21927e334c9bddae`, was published to both configured remotes. Its 2,851,048-byte source archive has SHA-256 `1188c5fdf12c71426d4a5735eb533326b739432f75618c2e8aba134ba1862374`; Debian computed the same digest before extracting it into the isolated E-Worker QA directory. The focused Linux native hosted-input case passed the real System V `mmap`/`mprotect` and callback path in 178 milliseconds.

Windows Qualification completed in 408.1 seconds with a 201.569-second suite. Debian Qualification completed in 421.9 seconds with a 213.068-second suite. Both hosts used .NET SDK `10.0.302` and runtime `10.0.10`, passed a zero-warning Release build, all 52 tests, exact Stage 1 to Stage 2 compiler reproduction, and the complete native CLI verifier. The 15,563-byte Windows report has SHA-256 `6780bd68cfcf7dacea10d34f5a4b9d7eeb6cdc2c2c4a70cf055c6830429330f5`; its 11,244-byte timing report has SHA-256 `350a72a7c699a9bde83e330c674fd108e1ae6742c4cc1538082da2951945a4bb`. The 15,473-byte Debian report has SHA-256 `1fec4c222425f2737a7f41c415b1841f88aab0a8e7458b40d4e7d170e1d9c35d`; its 10,869-byte timing report has SHA-256 `66358cacf8220d5b8231c2800961d8ad1546c0267ec0a3b4b8e1e642865ef029`. Their normalized contracts match exactly.

All 61 directly retrieved portable artifacts, totaling 7,752,612 bytes, matched Windows byte for byte. Their canonical name/size/SHA-256 manifest has SHA-256 `723354f893e2bb1c4bfaa6f7a04dc23995a51a2d5444d9a55759710d76b47f39`. The 2,293,338-byte Debian evidence bundle has SHA-256 `d02a98ce1ef1e48998ef4e06d33a58a368233382883068722d766ae9c2e8acca`. After retrieval and comparison, the exact QA directory, transferred source archive, and remote evidence bundle were removed and confirmed absent.

All 15 deterministic OS tests pass on Windows and Debian. Pinned QEMU `11.0.0`/Q35/TCG boots the deterministic 15,872-byte firmware-probe-10 image with SHA-256 `9228995f3b2522e15bd87ca63dc2637cc290f93b37f3e32b24cd8e3906671b75`, emits the complete version-10 success transcript, and returns the guest-controlled host exit code 1.

## Consequences

Windvale native code can now cross the first useful hosted boundary without exposing an operating-system handle or retaining host memory beyond one run. A file-backed decoder can be written and tested as ordinary Windvale source while the same resource owner continues to serve the interpreter.

The buffer owner copies host snapshots into unmanaged execution storage. This is intentionally simple and bounded, but it is not a general allocator, zero-copy file mapping, asynchronous I/O model, or ownership-transfer protocol. Output strings, dynamically constructed bytes, returned descriptors from Windvale functions, records, and heap values remain unavailable.

`Wvb-Header-Inspector.wv` is a staged native tool, not full `wvdump`. Moving the existing `Wv-Dump-Core.wv` requires at least nominal enum/record representation, dynamic text construction/formatting, diagnostic output policy, and the corresponding verifier/runtime work.

## Reconsider when

- A host result must outlive one native run or cross an asynchronous boundary.
- Measured file sizes make one execution-owned copy unacceptable.
- Dynamic text/bytes, record values, allocation, or garbage collection establish a more general ownership ABI.
- Hosted service requirements become serialized in a native container rather than retained beside `Nativeˉfragment` metadata.
