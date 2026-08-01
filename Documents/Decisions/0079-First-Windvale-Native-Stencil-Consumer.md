# Decision 0079: First Windvale native-stencil consumer

- Date: 2026-08-01
- Status: Accepted, implemented, and cross-host qualified
- Extends: [Decision 0078](0078-Multi-Patch-Windvale-Native-Stencil.md)
- Preserves: Native ABI 14, execution-context version 6, service-table version 4, WVB 1.6, WVO 1.0, kernel bridge 9, firmware probe 16, and both final process-input leaf identities

## Context

Decisions 0077 and 0078 transfer the two process-input machine shells and their patch descriptions into WVA assembled by the Windvale-written assembler. The live C# native owner still validates the resulting WVO objects and applies the patches. The next ownership step must prove that Windvale can consume the exact retained artifacts without introducing a permissive executable-object loader or a cycle through the services those artifacts construct.

The current native executor returns an `i32` process result, not an arbitrary immutable `bytes` value. Invoking a Windvale module through hosted file or argument capabilities before the argument-service leaves exist would also create a bootstrap cycle. Replacing the live C# loader in this slice would therefore require a new host/native data-return boundary rather than a simple ownership move.

## Decision

- Add `Compiler/Windvale/Native-Stencil-Core.wv` as a portable, capability-free Windvale module.
- Accept only the two exact retained WVO/WVSP contracts. Validate every header, section, name, symbol, metadata, patch record, zero-hole template byte, total size, and final instantiated byte. Digest comparison is evidence, not the acceptance predicate.
- Expose a closed `Nativeˉstencilˉstatus`, a closed semantic `Nativeˉstencilˉpatchˉkind`, and an immutable `Nativeˉstencilˉresult` containing status, constructed bytes, and failure offset.
- Derive current patch bytes from semantic kinds: execution-context offsets, borrowed-text descriptor offsets, and the argument-index failure detail. Do not accept caller-selected positional values.
- Apply each byte through immutable slice/concatenation construction in the exact accepted order. The core uses only operations already supported by the reference interpreter and baseline native backend.
- Add `Examples/Compiler/Native-Stencil-Demo.wv` with the exact canonical 166- and 321-byte WVOs. Its `Main` checks both successful outputs, deterministic repetition, short input, representative corrupt object metadata, symbols, patch records, fixed-shell bytes and holes, then changes every byte of each object in turn and requires rejection.
- Tie the demo data back to the embedded production WVO resources in C# conformance tests, then execute the same composed WVB through the reference interpreter, native JIT, and linked WVO/AOT.
- Retain the C# consumer as the live loader and independent oracle until a separate measured change adds a bounded native byte-result boundary or another non-cyclic integration seam.

## Native-subset constraint

The first implementation imported `Foundation/Byte-Construction.wv`. Native compilation rejected the composed module because that Foundation module also exports an otherwise-unused repeat path containing `Bytesˉfromˉu8`, which is outside the current baseline native subset. The accepted core therefore owns a small exact one-byte replacement operation implemented with `U32ˉfromˉu8`, `Bytesˉfromˉu32ˉlittle`, `Bytesˉslice`, and `Bytesˉconcat`.

This is not a general alternative byte-construction library. It exists inside the bounded consumer because every function in an imported source module is currently composed and lowered even when the root does not call it. A later dead-function elimination or native `Bytesˉfromˉu8` case may permit reuse without changing the stencil contract.

## Evidence contract

The qualified identities are:

- `Native-Stencil-Core.wvb`: 21,295 bytes, SHA-256 `d40fc83c3288043c7af80a261e351066bf3507913b34371a9839014b51ed4b2f`.
- `Native-Stencil-Demo.wvb`: 26,330 bytes, SHA-256 `651d9435c2b11b4f102a086615bdd159eb981096e2a2324027d5f86a29e36a15`.
- Canonical `process.argument_count` WVO: 166 bytes, SHA-256 `e2057943b9c79e10a432ea20a77da5ed0a261e3effdd36511cbb34e77e55c10b`.
- Canonical `process.argument` WVO: 321 bytes, SHA-256 `307e61dcb2a156eb0d4b77f7d93676d7b1ac24f9bb6fe1f31217837213352bad`.
- Final five-byte leaf: SHA-256 `2358e7e2c72d6476cfe05134db4f0eb5e6987fcca1b10894a8588a28d3929829`.
- Final 70-byte leaf: SHA-256 `2253e1435f141df5b68f9f7e9e9aa0de448410c42dcf33ad76dcf131afea65d1`.

Exact commit `f3a4ba4ebf4d2e9de28dfa73e34e14362691919b`, tree `e0eac295bc42c19bf6a1461b5e32e2c7c5fae33b`, is cross-host qualified. Its 6,727,680-byte archive has SHA-256 `073e3465d0f19516f9c26c6bad55d6bbac0cbfdacb1336eb30589b981e0c6b05` and retained that size and digest on the isolated Debian GNU/Linux 12 x64 QA host with .NET SDK `10.0.302`.

Windows and Debian pass zero-warning Release builds, all 61 Seed tests, exact compiler reproduction, and the complete CLI/reproduction gate in approximately 492.4 and 490.8 seconds. Their suite times are 246.227 and 248.495 seconds; the new consumer case takes 0.390 and 0.358 seconds, and the golden contract takes 184.619 and 187.213 seconds. The 15,798-byte Windows report has SHA-256 `9196b6f7049cd810c8bfd09037886e6c8628c1631c053543250290bc2acd50d7`; its 12,741-byte timing report has SHA-256 `b7b8f6a2b9f71d567cab97188dbaf7c242dcec7ca8afd685d4ab30456439773a`. The 15,705-byte Debian report has SHA-256 `c05f408eeb3d356e719382321705918bb6f9e6da803cd8e4efd5ddcfb1ebcc1f`; its 12,322-byte timing report has SHA-256 `280b039eb7fbd0e08f38847664221b950942d7effc43acccf025dc1f4333348d`. Their normalized contracts match with SHA-256 `77cd1d742509fc235f23b2dd743518a5694bd94c80d4bb6d81340dfe37e506e4`.

All 64 portable artifacts, totaling 7,800,986 bytes, match byte for byte; their canonical manifest has SHA-256 `8ed951d59d31c0f9d2975f5ada128c2ee7faf84f69e76bf930b58482749b3374`. The 2,320,237-byte retrieved Debian evidence bundle has SHA-256 `0c9d142897eee8279a7a4f6ab25f5d051b2c4b604e77b916304f17f46ef6cf15`. Both hosts pass all 15 OS tests, pinned QEMU boots the unchanged 15,872-byte probe-16 image, and GitHub [Verify run 30711006342](https://github.com/eworker-inc/Windvale/actions/runs/30711006342) passes independent Windows and Linux verification. After retrieval, the resolved exact Debian QA directory, source archive, and evidence bundle were removed and confirmed absent.

## Consequences

- Windvale now contains executable logic for exact WVO/WVSP validation and semantic patch application, rather than only the assembler source that describes the artifacts.
- The same Windvale source executes under all three existing runtime routes without .NET semantics inside the program.
- C# still supplies compilation, module loading, native lowering, executable publication, process invocation, and the live byte-return boundary for this milestone.
- No general WVO execution permission, arbitrary patch namespace, wider patch, relocation, code-cache policy, W^X ownership transfer, in-guest loader, or .NET-retirement claim is introduced.
