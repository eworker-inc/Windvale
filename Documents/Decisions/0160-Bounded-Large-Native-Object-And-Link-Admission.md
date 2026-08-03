# Decision 0160: Bounded large-native object and link admission

- Date: 2026-08-03
- Status: Qualified on Windows and digest-pinned Debian
- Adds: explicit `Largeˉnative` WVO admission and `flat-x86-64-large-v1`
- Retains: WVO 1.0 bytes, `flat-x86-64-v1`, the ordinary 4 MiB Windvale byte-value limit, ABI 22, and all version-1/version-2 PE and ELF bytes

## Context

Decision 0150 qualified the exact 328-function native compiler and Decision 0156 qualified the first paired hosted console containers, but their existing transport seam admitted only a 4 MiB WVO and a 4 MiB flat image. Raising those limits by guess would couple unrelated application, UEFI, and portable Windvale contracts to compiler packaging. Splitting the compiler without first measuring the real object would add a new boundary without evidence.

The native object sink now projects the same sections, symbols, and relocations that it writes and measures their exact encoded, materialized, and linked sizes. The exact ABI-22 compiler requires 17,147,219 encoded WVO bytes, 17,130,441 materialized bytes, and a 17,130,441-byte linked image. Its 17,129,584-byte text and 857-byte read-only-data sections carry 402 symbols and 167 relocations. A bounded 20 MiB profile therefore admits the artifact with 3,824,301 encoded bytes and 3,841,079 image bytes of headroom; compiler splitting is not justified at this boundary.

## Decision

- Keep standard WVO admission at 4 MiB encoded data/object bytes and 16 MiB materialized memory. Keep standard flat linking at a 4 MiB image and target name `flat-x86-64-v1`.
- Add an explicit large-native admission profile over the same WVO 1.0 encoding. It allows at most 20 MiB encoded object bytes and 20 MiB materialized memory.
- Require callers to pass the admission profile to object writing, reading, verification, and linking. No serialized input may grant itself larger limits.
- Add `flat-x86-64-large-v1` as the canonical map identity for the existing linker's large-native profile. Bound both aggregate encoded inputs and the final image to 20 MiB.
- Reuse one object codec, object verifier, linker, placement engine, relocation implementation, independent image verifier, and map writer. Do not introduce a second object format or compiler-specific linker.
- Keep ordinary Windvale `bytes` at 4 MiB. The Windvale-written assembler and linker retain standard admission until a bounded segmented or sparse transport can reproduce the same complete WVO bytes.
- Measure through the production native-object projection rather than a separate estimator. Require the measured encoded length to equal the emitted WVO length.
- Test the exact compiler twice for deterministic WVO, image, and map bytes; require default-profile rejection, explicit-profile admission, aggregate-input containment, exact 20 MiB image admission, plus-one memory/object rejection, and unknown-profile rejection in one focused case.

## Local evidence

The focused compiler/object/link test passes after a zero-warning Release solution build. It measures and emits:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| ABI-22 native compiler code | 17,130,441 | `af8db63675a2441e57a763ca4caa411419a84879cf01a1eb62b4be7556487cab` |
| Large-native WVO | 17,147,219 | `ee1c77763ad7440ad87ec10c4b7def67f9ec296eb366277cfe219617c76dda4b` |
| Linked image | 17,130,441 | `af8db63675a2441e57a763ca4caa411419a84879cf01a1eb62b4be7556487cab` |
| Canonical large-native map | bounded below 1 MiB | `86e32c67a41ccb31053d4905191e32dd2aaafd59e4b73416ea2b401e83adc973` |

The restored linked image is byte-identical to the already-qualified native fragment. Standard WVO writing fails with `WVO2017`; standard reading of the large artifact fails with `WVO1001`; standard linking fails with `WVL1002`. Explicit large-native admission succeeds deterministically. Aggregate input beyond 20 MiB fails with `WVL1003`, 20 MiB materialized image input succeeds, plus-one memory fails with `WVO2017`, plus-one encoded input fails with `WVO1001`, and an unknown link profile fails with `WVL1001`.

This is focused local Windows evidence. GitHub's independent Windows and digest-pinned Debian Qualification jobs remain responsible for the cross-host claim.

## Cross-host evidence

Exact descendant `db20fefaa3333b7b78392ba12141d1ae2b6bb0c2` passes GitHub [Verify run 30816153900](https://github.com/eworker-inc/Windvale/actions/runs/30816153900). Windows and digest-pinned Debian 12 each complete a zero-warning Release build, all 87 Seed tests including the golden compiler contract, all 38 OS tests, and the native CLI gate. The exact large WVO, linked image, and standard-profile rejection remain deterministic on both hosts.

## Consequences

The exact compiler now crosses the WVO and linker boundary without changing existing application bytes or weakening every consumer's limits. [Decision 0161](0161-Exact-Compiler-Service-Bundle-And-Manifest.md) implements the next packaging slice: paired exact service bundles and a fixed manifest serialize the compiler's six capabilities and ten actually required services before PE/ELF startup work begins.

This decision does not yet produce a compiler executable, expose a public large-link CLI, transfer large-object transport into Windvale, add compiler service adapters, directly run a packaged compiler, reproduce Stage 2 without .NET, or satisfy the native-retirement gate. Stage 0 remains the reference/recovery path.

## Rejected alternatives

Raising the global 4 MiB constants was rejected because the same limits protect ordinary WVO, UEFI, portable linker, and console-container paths. It would silently widen unrelated trust boundaries.

Splitting the compiler object was rejected because the measured artifact fits one 20 MiB object and image with useful headroom. Splitting remains available if a later measured artifact requires independently useful modules rather than size-only partitions.

Versioning WVO was rejected because admission limits are reader policy, not a change to serialized semantics. The same logical object has the same bytes under either profile when it fits both.

## Reconsider when

- A qualified native tool exceeds 20 MiB or aggregate multi-object compiler linking requires more than 20 MiB.
- A 20 MiB admission causes unacceptable memory duplication in an independently measured writer, verifier, or container path.
- The Windvale-owned segmented or sparse transport reveals a format requirement that WVO 1.0 cannot express.
- Windows and Debian produce different WVO, image, or canonical-map bytes from the exact compiler inputs.
