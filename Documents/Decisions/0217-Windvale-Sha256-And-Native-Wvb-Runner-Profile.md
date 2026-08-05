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

Add `Foundation/Sha256.wv` as a portable, capability-free SHA-256 implementation over immutable bytes and specified `u32` bitwise operations. Because ordinary `u32` arithmetic is checked, the implementation performs explicit wrapping addition with bitwise carry propagation. Known-answer coverage includes the empty message, `abc`, and the 56-byte two-block padding boundary.

The scalar WVB interpreter imports this module and calls its exported digest function. It preflights the fixed 64-byte guest result allocation before hashing, preserving bounded heap-failure behavior for large inputs. The semantic `Bytesˉsha256ˉhex` intrinsic remains available to targets as an optimization and oracle; it is no longer required by the native runner fragment.

### Share the exact four-byte native representation

Lower `Bytesˉfromˉi32ˉlittle` to its own typed native operation and verify its `i32` input. Its emitter shares the existing exact four-byte little-endian representation with `Bytesˉfromˉu32ˉlittle`; only the static type differs. A focused negative-value round trip proves the signed representation.

### Add a fixed runner package profile

Add `windows-x64-wvb-runner-v1` and `linux-x64-wvb-runner-v1` under `WVHV 1` metadata profile `5`. The verified runner fragment must have exactly one exported `Main() -> i32`, the established five read-only capabilities, and these nine services in canonical order:

1. `console.write_line`;
2. `process.argument_count`;
3. `process.argument`;
4. `file.read_bytes`;
5. `text.utf8_is_valid`;
6. `diagnostic.write_line`;
7. `text.concat`;
8. `i32.format`;
9. `u32.format`.

Reuse the qualified inspector startup template rather than adding assembly. Its unused enum-name and quoting slots are zero in the runner profile and unreachable from the independently verified fragment. The package retains no file-output service or mutating capability.

The current native-compiler candidates are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Runner WVB | 86,061 | `117b1f1ed26e18fe6be7d1d732a9317ba17469666d57260c87089caca6d78229` |
| Windows runner | 770,560 | `86059a076ca2bedc26d466c16c2c6f3efd8008a20b11ebc6e5f0305cacada690` |
| Linux runner | 770,048 | `75162565b70066c8a2816c2bd2b6937d0d1e7e8791564cd7f3d408dcf0f98c9f` |

The exact WVB, Windows application, and Linux application are pinned in the native-front-door inventory with qualification marked `pending`. The Windows candidate directly executes a compiler-produced portable guest and reports `Result: 42`. The focused Stage 0 conformance case verifies Foundation known answers through both the reference runtime and native executor, reconstructs both package profiles, and runs the current-host startup. These are current-host implementation results; they do not replace the pending Windows/Linux Qualification gate.

## Consequences

- WVB decoding, execution, and SHA-256 remain Windvale source rather than new assembly or C# implementations.
- The runner uses a narrower service set than the inspector while sharing its already-bounded host startup.
- The scalar interpreter is now a composed fifteen-function WVB, and WebAssembly verification builds its explicit project instead of compiling one source file in isolation.
- Stage 0 still performs AOT selection, package construction, test orchestration, and recovery. Digest-checking candidate `Run-Wvb` launchers are present for qualification, but they are not accepted as the ordinary front door until the exact pinned candidate passes on both hosts.

## Reconsideration triggers

Reconsider the pure SHA implementation if a streaming Foundation hash contract is specified, if measured immutable-byte pressure requires a cohesive bounded builder, or if a target intrinsic is qualified as a transparent optimization. Reconsider startup reuse when a shared Windvale-native launcher or service manager can bind the same exact capability profile without fixed platform startup templates.
