# Decision 0246: Native console-write-line capability

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0245](0245-Native-File-Write-Bytes-Capability.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0245 transfers the file-output side of the real hosted `wvnative` shell. After publishing a successful WVO, that tool reports the exact native ABI and output sizes through `console.write_line(text) -> void`. Transferring this call completes its successful reporting path while keeping diagnostic usage and rejection output as a separate final capability slice.

The shared console contract writes the supplied UTF-8 text followed by one exact LF. Output is externally visible and nontransactional: a provider may fail after partial visibility, and the caller must not infer rollback or retry an uncertain write without a separately specified idempotency contract.

## Decision

### Admit the exact successful-output call

Allow opcode 65 to name `console.write_line(text) -> void`. Require one text value, consume it, and produce no result. Keep `diagnostic.write_line(text) -> void` rejected pending its separate slice.

Emit the exact ABI 22 sequence: pass the borrowed-text pointer and length in `R8` and `R9D`, load service-table slot 8 through the existing `R15` execution context, call it, and branch on a nonzero result to the shared runtime-service tail. Charge the ordinary ten-byte instruction meter plus thirty-four operation bytes, for forty-four planned bytes.

The lowering neither rewrites line endings nor buffers, retries, or combines output calls. WVO 1.0 still does not serialize its required service, so independently loading this object outside the verified hosted package remains unsupported.

### Require focused output evidence

Add `Wvb-To-Wvo-Console-Write-Line.wv`. It writes one immutable `A` line and returns 42. Require the exact capability signature, exact `A` plus LF output through the reference interpreter and Stage 0 native execution, and byte-for-byte WVO agreement through both Windvale adapters and the standalone native package.

Reuse the existing native output-service suite for rejected and partial sinks, exact `WVR3029` failure mapping, authorization and support preflight, service identity, and linked-image execution. Do not duplicate that platform-service coverage in the lowering fixture.

Build the memory and hosted adapters through the qualified native source front door and require their identities to match Stage 0. The affected shared-backend and standalone package selections are the only local behavioral checks for this slice. Local Standard, Qualification, the full Seed/OS suites, and artifact promotion remain deferred to the grouped end-of-goal gate.

## Consequences

- The accepted Windvale lowerer now covers the successful console report emitted by the real hosted WVB-to-WVO shell without using the C# lowerer for the input module.
- Console output retains exact text-plus-LF behavior and the existing partial-visibility failure boundary; it adds no transaction, rollback, or retry promise.
- The current core, memory-adapter, and hosted-tool WVB hashes are `fa6e973abeca5fe1f4be9f2cb42f5b5257169044d2dd5899d435eeeb32d8e966`, `1376cb361b600e613410529ad1214d1ae1f3ca7aa3fd30001b67705e4983f8cc`, and `895f6ec7841cb0f9b52ec62d9fb9a440897e9bf357dcd0d15f794704b25d6828`. The latter two contain 295,106 and 296,134 bytes and reproduce exactly through the native build driver.
- The hosted tool lowers through Stage 0 to 4,119,200 code bytes and a 4,131,020-byte WVO. Current unpromoted packages are 4,137,472 Windows bytes at SHA-256 `e96fc63a7d5e7cc914940eabec07fb01ec027343298f4495a4291c28110d49de` and 4,136,960 Linux bytes at SHA-256 `c45f2b373b094846f5eae4d01e026331736638c26b08ab3ed9ca554354e33336`.
- No C# implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Transfer `diagnostic.write_line(text) -> void` next to complete all six admitted hosted capability calls used by the real tool, including usage and rejection reporting.
