# Decision 0066: Borrowed bytes and unsigned native values

- Date: 2026-08-01
- Status: Cross-host qualified on Windows x64 and Debian x64; OS consumer qualified on pinned Windows QEMU
- Refines: [Decision 0065](0065-Versioned-Native-Execution-Context-And-Console-Service.md)'s ABI-6 scalar/static-text value boundary
- Advances: The first byte-decoding requirements of a native Windvale inspection tool

## Context

ABI 6 established an extensible execution context and one explicitly authorized output service, but it could not natively express the central operation of a compiler or binary tool: inspect bounded immutable bytes. The existing Windvale-written `wvdump`, assembler, linker, and compiler already use byte slices, unsigned offsets, and fixed-width little-endian reads through the reference runtime. Adding file services before defining the in-process byte value would only move the missing representation behind a host callback.

The smallest useful next slice is borrowed immutable bytes. It must work identically in JIT, linked WVO/AOT, and the OS image; preserve exact WVB instruction charging; fail bounds checks without a host signal; and leave ownership, allocation, dynamic buffers, and file policy for later evidence.

## Decision

- Advance the experimental target to `x86-64-wvb-baseline-v7` and native ABI version 7. Do not accept ABI-6 fragments through a compatibility branch; their qualified artifacts remain historical evidence.
- Widen every native local/temporary frame cell from four to 16 zero-initialized bytes. Scalars use the low dword. A borrowed-byte descriptor uses pointer `u64` at offset 0, length `u32` at offset 8, and zero reserved `u32` at offset 12.
- Admit immutable module `bytes` data and create descriptors only through exact verified RIP-relative references into fragment read-only data. This is a borrow for the duration of execution, not allocation or ownership transfer.
- Lower `Bytesˉlength`, checked `Bytesˉslice`, and checked little-endian `u8`, `u16`, `u32`, and `i32` reads. Complete ranges are checked with unsigned arithmetic before a pointer is dereferenced. A byte range failure returns packed status 6 and maps to `WVR3008`.
- Admit `u8` constants and equality, `u32` constants and unsigned comparisons, checked `u32` add/subtract/multiply, and `U32ˉfromˉu8`. Retain `WVR3007` for checked integer overflow.
- Permit as many as four scalar or borrowed-byte parameters. Scalars retain the low-32-bit register convention. A byte argument is a pointer in the corresponding 64-bit argument register to a caller-owned descriptor; the callee immediately copies the two machine words into its frame. Function returns remain scalar-only.
- Extend native IR validation and the independent machine-code decoder over typed 16-byte frame cells, descriptor creation/copy, byte-source provenance, argument kinds, unsigned bounds edges, reads, conversions, arithmetic, and comparisons. A scalar write may not silently retype a proven descriptor slot; corrupt data targets, descriptor lengths, byte argument forms, or trap branches fail before WVO publication or W^X allocation.
- Keep execution-context version 1 and service-table version 1 unchanged because their bytes and meanings do not change. Dynamic/owned bytes, file and argument services, dynamic text, records, allocation, stack arguments, and byte returns remain outside ABI 7.
- Advance the ordinary portable OS probe to version 3. It retains the i32 loop and result 29, adds immutable bytes, passes a borrowed slice through an internal function, checks byte length and `u8`/`u32` values, and requires exact instruction/depth budgets 271/2. Advance the firmware probe to version 9 so the changed native and EFI artifacts do not overwrite version-8 evidence under the same identity.
- Retain C#/.NET as selector/verifier implementation, host adapter, reference oracle, and recovery path. This slice makes more Windvale programs eligible for native execution; it does not make the compiler self-hosting in native code.

## Qualification evidence

The focused differential program exercises all four byte-read forms, slicing, length, a borrowed-byte internal parameter, scalar return, `u8`/`u32` comparisons, conversion, and checked unsigned arithmetic. Interpreter, W^X execution, and linked WVO/AOT return 42 under the exact same WVB instruction/depth limits. Deliberately corrupt data references, descriptor lengths, descriptor slot types, byte argument forms, and bounds targets fail independent verification. Out-of-bounds reads and unsigned overflow agree on `WVR3008` and `WVR3007`.

Exact candidate commit `8d375bf3fae099e904f4536626b670d640b5e375`, tree `b9d9738ce04c8a011087c5591e69606f7ed03332`, was published to both configured remotes. Its 2,841,701-byte Git archive has SHA-256 `4abf83ea5f6a19f97112defc81afa435a09ea0dbd0851413d49d5b940c3c74af`; Debian computed the same digest before extraction into the isolated E-Worker QA directory. The focused Linux native-byte case then passed the actual System V `mmap`/`mprotect` path in 201 milliseconds.

Windows Qualification completed in 428.9 seconds with a 208.902-second suite. Debian Qualification completed in 428.5 seconds with a 217.291-second suite. Both hosts used .NET SDK `10.0.302`, passed zero-warning Release builds, all 51 tests, exact Stage 1 to Stage 2 compiler reproduction, and the complete native CLI verifier. The 15,563-byte Windows report has SHA-256 `6780bd68cfcf7dacea10d34f5a4b9d7eeb6cdc2c2c4a70cf055c6830429330f5`; its 11,065-byte timing report has SHA-256 `71ae04a67e8cf7ff8a2f0b79071fc704ac6b2f4d9fe680383dbe1f91d7ee17f5`. The 15,473-byte Debian report has SHA-256 `1fec4c222425f2737a7f41c415b1841f88aab0a8e7458b40d4e7d170e1d9c35d`; its 10,698-byte timing report has SHA-256 `5418a3fce6854c594ba5c49d77213685993b002e5e4558cf139b3435b41330a5`. Their normalized contracts match exactly.

All 61 directly retrieved portable artifacts, totaling 7,752,612 bytes, match Windows byte for byte. The 2,298,060-byte Debian evidence bundle has SHA-256 `e6774f3f2495e3cacc17963adc398a8a3ded7bc80d6bcae35d5cc002db513789`. Debian and Windows both pass all 15 deterministic OS tests. Pinned QEMU 11.0/Q35/TCG executes the exact 15,360-byte firmware-probe-9 image with SHA-256 `ac92cd4759961c7a046ede49af8dce7626016fbcf8bb46e7d90027f5974bffa4`, emits the complete version-9 transcript, and returns guest-controlled host exit code 1. After evidence retrieval, the exact QA directory, transferred archive, and remote evidence bundle were removed and confirmed absent.

## Consequences

Windvale native code now has the first general immutable binary view needed by a decoder, inspector, assembler, linker, or compiler frontend. The representation is deliberately borrowed and allocation-free, so the verifier can prove its origin and bounds without a garbage collector or lifetime protocol. The same feature already crosses the host JIT/AOT and OS AOT consumers.

The 16-byte universal frame cell increases code size and stack use because the current baseline selector uses a simple cell-per-value layout and clears every cell. This is accepted for transparency and verification at this stage; liveness-based slot reuse, compact scalar cells, register allocation, and larger kernel-stack policy require separate measured decisions.

This is not yet a native `wvdump` or compiler. No native file/argument input exists, returned or allocated bytes are unavailable, WVO does not serialize hosted service requirements, and the selector/verifier/runtime remain C# Stage 0 components.

## Reconsider when

- File or argument services need to return owned buffers or retain data after a call.
- A byte value must cross an asynchronous boundary, escape a frame, or be stored in a heap aggregate.
- Measured compiler-sized programs make 16-byte cell stack/code cost unacceptable.
- Stack arguments, records, dynamic text, allocation, or garbage collection require a more general value ABI.
