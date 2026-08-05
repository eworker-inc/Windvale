# Decision 0247: Native diagnostic-write-line capability

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0246](0246-Native-Console-Write-Line-Capability.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0246 transfers successful reporting for the real hosted `wvnative` shell. Its usage and invalid-input paths report through `diagnostic.write_line(text) -> void`. This is the last of the six hosted capability signatures already admitted by the candidate's capability table but not yet callable through Windvale source.

The shared diagnostic contract writes the supplied UTF-8 text followed by one exact LF to a sink separate from standard output. Diagnostic output is externally visible and nontransactional: a provider may fail after partial visibility, and the caller must not infer rollback or retry an uncertain write without a separately specified idempotency contract.

## Decision

### Admit the exact diagnostic-output call

Allow opcode 65 to name `diagnostic.write_line(text) -> void`. Require one text value, consume it, and produce no result. The candidate now accepts calls to all six hosted signatures that its capability table admits.

Emit the exact ABI 22 sequence: pass the borrowed-text pointer and length in `R8` and `R9D`, load service-table slot 48 through the existing `R15` execution context, call it, and branch on a nonzero result to the shared runtime-service tail. Charge the ordinary ten-byte instruction meter plus thirty-four operation bytes, for forty-four planned bytes.

Console and diagnostic output share one focused text-output emitter parameterized by the verified service-table offset. Their capability identities, required services, authorization, and output channels remain distinct. The lowering neither rewrites line endings nor buffers, retries, or combines output calls. WVO 1.0 still does not serialize its required service, so independently loading this object outside the verified hosted package remains unsupported.

### Require focused channel evidence

Add `Wvb-To-Wvo-Diagnostic-Write-Line.wv`. It writes one immutable `A` diagnostic line and returns 42. Require the exact capability signature, exact `A` plus LF output on the diagnostic channel through the reference interpreter and Stage 0 native execution, and byte-for-byte WVO agreement through both Windvale adapters and the standalone native package.

Reuse the existing native output-service suite for separate console and diagnostic routing, Unicode and empty-line behavior, rejected and partial sinks, exact `WVR3029` failure mapping, authorization and support preflight, service identity, and linked-image execution. Do not duplicate that platform-service coverage in the lowering fixture.

Build the memory and hosted adapters through the qualified native source front door and require their identities to match Stage 0. The affected shared-backend and standalone package selections are the only local behavioral checks for this slice. Local Standard, Qualification, the full Seed/OS suites, and artifact promotion remain deferred to the grouped end-of-goal gate.

## Consequences

- The accepted Windvale lowerer now covers every hosted capability call used by the real WVB-to-WVO shell, including successful, usage, and rejection reporting paths.
- Console and diagnostic output retain exact text-plus-LF behavior, separate channels, and the existing partial-visibility failure boundary; no transaction, rollback, or retry promise is added.
- The current core, memory-adapter, and hosted-tool WVB hashes are `867ae362331f764c918b0c6203c1b514e83467a0bfd74e369b3c7e81066463b2`, `98456a3870cd467e1699b1a007a091e8c57c86a7e98530ac5c61274ebb11157b`, and `61de8ec6152a117bf0bd16c44c5709164534dc7ce4d0b75fccbbb99a545c8982`. The latter two contain 295,554 and 296,582 bytes and reproduce exactly through the native build driver.
- The hosted tool lowers through Stage 0 to 4,122,256 code bytes and a 4,134,110-byte WVO. Current unpromoted packages are 4,140,544 Windows bytes at SHA-256 `9a1e48792d601bdaa28414ff89c7bb5650198786f0e953cd13cdf422e616597f` and 4,141,056 Linux bytes at SHA-256 `dd50e84e7a2be9c3f3d27aa5b449946a3d2f5eaf4e1a73c176e6be3b74b40d82`.
- No C# implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Reassess the remaining N1 gaps against the compiler-produced hosted tool and select the next measured self-lowering blocker. Do not add another capability slice unless a real accepted application requires it; remaining limits include function count, scalar and byte-builder operations, descriptor calls and returns, broader nominal shapes, required-service serialization, and the complete backend.
