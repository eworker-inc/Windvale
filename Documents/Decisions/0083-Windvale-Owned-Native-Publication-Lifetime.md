# Decision 0083: Windvale-owned native publication lifetime

- Date: 2026-08-01
- Status: Accepted and cross-host qualified
- Extends: [Decision 0082](0082-Windvale-Owned-Native-Publication-Layout.md)
- Preserves: Native ABI 14, execution-context version 6, service-table version 4, WVB 1.6, WVO 1.0, kernel bridge 9, both qualified firmware-probe-17 identities, and all final runtime-service leaf identities

## Context

Decision 0082 makes Windvale the authority for executable-image extent and service placement, but its executor still contains unstructured allocation, copying, protection, invocation, and release calls. A `finally` block guarantees release, yet the allowed lifecycle is implicit in C# control flow and the raw executable address is shared across that larger executor.

Moving operating-system calls into ordinary Windvale source would require raw pointers, native handles, platform imports, and a general lifetime-safe FFI before the runtime can safely publish that same source. The next transfer should instead separate policy from authority: Windvale chooses the closed lifecycle, while one internal host owner performs only the selected Windows or Linux operation.

## Decision

- Accept the bounded internal [`WVLQ 1` request and `WVLT 1` response contract](../../Specifications/Windvale-Native-Publication-Lifetime.md).
- Add portable `Compiler/Windvale/Native-Publication-Lifetime-Core.wv`. It validates one accepted image extent and emits the complete exact nine-transition graph for unallocated, writable, copied, executable, invoked, and released states.
- Add hosted `Compiler/Windvale/Native-Publication-Lifetime-Bridge.wv` and retain its exact WVB in `Runtime/Windvale.Native/Consumers`. It reads one immutable in-memory request through its sole `file.read_bytes` capability and returns the plan through Decision 0080's bounded byte-result path under the Stage 0 reference interpreter.
- Make every ordinary native execution evaluate and independently reconstruct the Windvale lifetime plan before allocating executable memory. Planner rejection maps to `WVN4015`; a malformed response or forged in-process plan maps to `WVN4016`.
- Move all executable-image operating-system calls and actual state into one internal `Nativeˉexecutableˉimage` owner. Its raw address is not public outside the runtime assembly. Before allocation, copy, seal, invocation, or release, the owner requires the exact accepted transition; an invalid attempt maps to `WVN4017` before the operation.
- Permit `Release` from writable, copied, executable, and invoked states so deterministic cleanup does not depend on completing the normal path. Preserve the existing policy that a failed platform release is surfaced rather than silently swallowed.
- Keep service/context construction, runtime arenas, result cells, hosted-resource tables, status mapping, and native fragment compilation/verification in their existing Stage 0 owners. This decision moves executable-image lifecycle policy and isolates platform authority; it does not claim complete execution-lifetime ownership.

## Why the ABI and OS images are retained

The fragment bytes, entry convention, context, service table, runtime-private tables, service leaves, invocation arguments, packed statuses, and observable results do not change. The lifecycle plan governs when the host may allocate, copy, protect, invoke, and release one already-verified image. The service-free OS probe consumes a linked AOT image and does not use this host JIT publication owner. Advancing ABI 14 or rebuilding probe 17 would describe no guest-visible contract change.

## Evidence contract

- Portable `Native-Publication-Lifetime-Core.wvb`: 4,954 bytes, SHA-256 `52b1cb6dd0d7fa9d17c1cba50b527912876e4acf1cd9663846ce915b4c56aed5`.
- Retained hosted `Native-Publication-Lifetime-Bridge.wvb`: 4,857 bytes, SHA-256 `74dfaf40bb6ea83f0fd72757c9c4cb85f5c8dd28a41f3993325871d348e88d32`.
- The bridge has exactly one capability, `file.read_bytes`, and exactly one `Main() -> bytes` export.
- Focused coverage must exercise minimum and maximum extents, deterministic output, every request status family, every response header and transition field, unknown/maximum values, forged plans, every normal transition, release from partial writable/copied/executable states, duplicate/out-of-order host actions, retained-source reproduction, and live native execution through the owner.
- Complete qualification must reproduce both WVBs and the retained bridge on Windows and Debian x64, compare all portable artifacts and normalized contracts, pass both Seed and OS suites, and retain both pinned-QEMU probe-17 identities unless an applicable contract changes.

## Qualification

Exact implementation commit `a898fe84a34e96c3768b77ab50ffc5bed6401fb0`, tree `70ae2e901d1eebd192d47b0f9a6b3a45e588adf1`, was published to both configured remotes. Its 2,806,844-byte archive has SHA-256 `69124612af949ada890b844c76814975b23717e04269077321a76d070635e042` and retained that exact identity after transfer to the isolated Debian GNU/Linux 12 x64 QA host with .NET SDK `10.0.302`.

Windows and Debian pass zero-warning Release builds, all 64 Seed tests, exact compiler and retained-WVB reproduction, and the complete native CLI gate. Complete Qualification takes 484.1 and 513.7 seconds wall-clock, with suite times of 240.113 and 256.330 seconds. The new publication-lifetime case takes 12 and 14 milliseconds.

The 15,798-byte Windows report has SHA-256 `9196b6f7049cd810c8bfd09037886e6c8628c1631c053543250290bc2acd50d7`; its 13,237-byte timing report has SHA-256 `48da2991322f68ec4b711d06b0da9edecdcd433d27e315151a537c7cc8b3d99a`. The 15,705-byte Debian report has SHA-256 `c05f408eeb3d356e719382321705918bb6f9e6da803cd8e4efd5ddcfb1ebcc1f`; its 12,803-byte timing report has SHA-256 `c38e69979a60259924bb49811b0ceb69b9e36bb58bbe5286719bd73851c5ff7b`. Their normalized contracts match exactly with SHA-256 `77cd1d742509fc235f23b2dd743518a5694bd94c80d4bb6d81340dfe37e506e4`.

All 69 portable artifacts, totaling 7,846,538 bytes, match byte for byte. Their canonical name/size/SHA-256 manifest has SHA-256 `ae7377b88e16be49566ac634878564c64e9a3d8e6500fa53436526b675f15711`. The retrieved 2,313,699-byte Debian evidence bundle has SHA-256 `d2de5b3acdfa8189be14f10a3c27f82fd7f5797324f51ee6068bf5c5ed5df9b0`.

Both hosts pass all 17 OS tests. Pinned QEMU 11.0/Q35/TCG retains both exact 17,920-byte probe-17 images: ordinary boot SHA-256 `d2c0a7e4e5e1605fc8639c05ab27ad07ee2b015ad2dc151d8637830b8acb3f18` returns guest-controlled host exit code 1, while invalid-opcode SHA-256 `26ccfaf862024e022339ca9fa8114c71b4fe601fe59a806d366e1d330b6d106d` emits the terminal vector-6 panic and returns host code 3. GitHub [Verify run 30717660874](https://github.com/eworker-inc/Windvale/actions/runs/30717660874), attempt 2, independently passes classification plus Windows and Linux verification for the exact implementation commit.

## Consequences and limits

Windvale now owns the allowed executable-image state/action graph, and one small internal host owner contains the raw address plus every executable-memory platform call. The larger executor no longer imports or calls `VirtualAlloc`, `VirtualProtect`, `FlushInstructionCache`, `VirtualFree`, `mmap`, `mprotect`, or `munmap` directly.

The platform functions themselves remain C# P/Invoke authority. Windvale does not yet own native pointer types, platform imports, service/context/arena/result-cell lifetime, a native loader, code cache, concurrent publication, signal containment, or process isolation. The next transfer should be selected from measured owner boundaries rather than widening this contract into a general FFI.

## Reconsider when

- A Windvale-native platform adapter can receive opaque owned regions without arbitrary pointer escape.
- A code cache requires shared, concurrent, or reference-counted executable-image lifetime.
- An isolated compiler/runtime process changes failure containment or release guarantees.
- Native compiler execution makes service/context/arena lifetime a larger bottleneck than image publication.
