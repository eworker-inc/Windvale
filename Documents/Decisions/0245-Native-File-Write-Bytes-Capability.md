# Decision 0245: Native file-write-bytes capability

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0244](0244-Native-File-Read-Bytes-Capability.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0244 transfers the read side of the real hosted `wvnative` shell. Its successful path next publishes the exact lowered WVO through `file.write_bytes(resource_name, value) -> void`. This is the first mutating capability lowered by Windvale source, so its whole-value bound, externally visible replacement, durable-success condition, and failure propagation must remain explicit.

The shared hosted contract creates or replaces the named file, accepts at most one 4 MiB immutable byte value, performs a durable flush before success, does not create missing parents, and does not promise atomic replacement. A failure can follow externally visible mutation; the caller must not infer rollback or retry an uncertain mutation outside a separately specified idempotency contract.

## Decision

### Admit the exact whole-value output call

Allow opcode 65 to name `file.write_bytes(text, bytes) -> void`. Require a text resource name followed by one bytes value, consume both, and produce no result. Keep console and diagnostic output calls rejected pending their separate slices.

Emit the exact ABI 22 sequence: pass the borrowed-text pointer and length in `R8` and `R9D`, pass the borrowed-bytes pointer and length in `RCX` and `EDX`, load service-table slot 96 through the existing `R15` execution context, call it, and branch on a nonzero result to the shared runtime-service tail. Charge the ordinary ten-byte instruction meter plus forty-nine operation bytes, for fifty-nine planned bytes.

The lowering does not reinterpret resource names, retry writes, or weaken the service's size, replacement, flush, and failure contract. WVO 1.0 still does not serialize its required service, so independently loading this object outside the verified hosted package remains unsupported.

### Require focused mutation evidence

Add `Wvb-To-Wvo-File-Write-Bytes.wv`. It obtains a temporary output name through `process.argument`, writes one immutable `A` byte, and returns 42. Require exact capability signatures, reference interpretation through an injected capturing writer, Stage 0 native execution through the real current-host durable file-output service, exact written bytes, and byte-for-byte WVO agreement through both Windvale adapters and the standalone native package.

Reuse the existing native file-output suite for replacement, empty output, the 4 MiB maximum, authorization and support preflight, service identity, mapped host errors, and linked-image execution. Do not duplicate that platform-service coverage in the lowering fixture.

Build the memory and hosted adapters through the qualified native source front door and require their identities to match Stage 0. The affected shared-backend and standalone package selections are the only local behavioral checks for this slice. Local Standard, Qualification, the full Seed/OS suites, and artifact promotion remain deferred to the grouped end-of-goal gate.

## Consequences

- The accepted Windvale lowerer now covers both file-byte leaves used by the real hosted WVB-to-WVO shell without using the C# lowerer for those input modules.
- The first Windvale-lowered mutating capability preserves the existing exact whole-value and durable-success boundary; it adds no retry or atomicity claim.
- The current core, memory-adapter, and hosted-tool WVB hashes are `60e68f8f2b58134a79d11692c5f4d3232f723583363a5140334c24021daee11b`, `8c3cc880094d2aac95254211089d01218d3b13754269632b4004848e7aa5caba`, and `ab3fe35a0a402737350085578731b066aa4926a207d962b12d5d7fac955795ce`. The latter two contain 292,627 and 293,655 bytes and reproduce exactly through the native build driver.
- The hosted tool lowers through Stage 0 to 4,090,736 code bytes and a 4,102,454-byte WVO. Current unpromoted packages are 4,108,800 Windows bytes at SHA-256 `228c4476a24f88490fd678a062f5b878088f5d09d445226e64b97872955245e2` and 4,108,288 Linux bytes at SHA-256 `dfcdb7b7a90789c14ad8678461534d17f32eb8a9be139fe589d008a77d92575e`.
- No C# implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Transfer `console.write_line(text) -> void` next to complete the real tool's successful reporting path. Keep diagnostic output separate because it owns usage and rejection reporting rather than successful publication.
