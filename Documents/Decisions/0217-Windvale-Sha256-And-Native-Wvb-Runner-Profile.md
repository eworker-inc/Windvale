# Decision 0217: Windvale SHA-256 and native WVB runner profile

- Date: 2026-08-05
- Status: Implemented and pinned as a candidate; dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0216](0216-Bounded-Compiler-Temporary-Slots-And-Current-Wvb-Inspection.md)
- Contracts: [Foundation bytes](../../Specifications/Foundation-Bytes.md), [WebAssembly](../../Specifications/Windvale-WebAssembly.md), and [hosted WVB tools](../../Specifications/Windvale-Hosted-Verifier-Application.md)

## Context

The composed WVB runner reached the native backend with two unsupported operations: four-byte construction from `i32` and SHA-256. Adding WVB interpretation or hashing to host assembly would duplicate semantics and move the project away from Windvale ownership. Binding another host SHA service would also widen the fixed runner profile for an operation the language can now express portably.

The existing inspector launcher already owns the unavoidable argument, bounded read-only file, output, and process-entry boundary on Windows and Linux. Its report-only enum-name and quoting services are not required by the runner.

## Decision

### Implement SHA-256 in Windvale

Add `Foundation/Sha256.wv` as a portable, capability-free SHA-256 implementation over immutable bytes and specified `u32` bitwise operations. Because ordinary `u32` arithmetic is checked, the implementation performs explicit wrapping addition with bitwise carry propagation. Its focused `Foundationˉsha256ˉhex(bytes) -> bytes` function returns exactly 64 lowercase ASCII digest bytes. Known-answer coverage includes the empty message, `abc`, and the 56-byte two-block padding boundary.

The scalar WVB interpreter imports this module and calls its exported digest function. It preflights the fixed 64-byte guest result allocation before hashing, preserving bounded heap-failure behavior for large inputs. The semantic `Bytesˉsha256ˉhex` intrinsic remains available to targets as an optimization and oracle; it is no longer required by the native runner fragment.

### Share the exact four-byte native representation

Lower `Bytesˉfromˉi32ˉlittle` to its own typed native operation and verify its `i32` input. Its emitter shares the existing exact four-byte little-endian representation with `Bytesˉfromˉu32ˉlittle`; only the static type differs. A focused negative-value round trip proves the signed representation.

### Add a fixed runner package profile

Add `windows-x64-wvb-runner-v1` and `linux-x64-wvb-runner-v1` under `WVHV 1` metadata profile `5`. The verified runner fragment must have exactly one exported `Main() -> i32`, the established five read-only capabilities, and these eight source-required services in canonical order:

1. `console.write_line`;
2. `process.argument_count`;
3. `process.argument`;
4. `file.read_bytes`;
5. `diagnostic.write_line`;
6. `text.concat`;
7. `i32.format`;
8. `u32.format`.

Reuse the qualified inspector startup template rather than adding assembly. The fixed application bundle retains the already-qualified startup-internal `text.utf8_is_valid` leaf, while the independently verified runner fragment no longer depends on that service. Its unused enum-name and quoting slots remain zero. The package retains no file-output service or mutating capability.

The current native-compiler candidates are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Runner WVB | 90,009 | `3b881147e5e6c8298cf249e6e02c9f18ed4a677d49ef0a307427465795a1c626` |
| Windows runner | 778,752 | `91b046015660f5f9e2710ed9cb41d5da9a79a1c87f4cf9ed87790c013a6dcce4` |
| Linux runner | 778,240 | `8fcfa1fe8dbdb3228c484f284655690d0bf14f4c595eaf820d55cc4ab4f6a294` |

The exact WVB, Windows application, and Linux application are pinned in the native-front-door inventory with qualification marked `pending`. The Windows candidate directly executes a compiler-produced portable guest and reports `Result: 42`. The focused Stage 0 conformance case verifies Foundation known answers through both the reference runtime and native executor, reconstructs both package profiles, and runs the current-host startup. These are current-host implementation results; they do not replace the pending Windows/Linux Qualification gate.

## Consequences

- WVB decoding, execution, and SHA-256 remain Windvale source rather than new assembly or C# implementations.
- The runner uses a narrower service set than the inspector while sharing its already-bounded host startup.
- The scalar interpreter is now a composed two-function WVB, and the complete runner composition contains five functions. WebAssembly verification and native runner construction both build their explicit projects instead of compiling one source file in isolation.
- Stage 0 still performs AOT selection, package construction, test orchestration, and recovery. Digest-checking candidate `Run-Wvb` launchers are present for qualification, but they are not accepted as the ordinary front door until the exact pinned candidate passes on both hosts.

## Reconsideration triggers

Reconsider the pure SHA implementation if a streaming Foundation hash contract is specified, if measured immutable-byte pressure requires a cohesive bounded builder, or if a target intrinsic is qualified as a transparent optimization. Reconsider startup reuse when a shared Windvale-native launcher or service manager can bind the same exact capability profile without fixed platform startup templates.
