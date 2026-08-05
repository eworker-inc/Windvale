# Decision 0243: Native process argument capability

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0242](0242-First-Hosted-Capability-In-Native-Lowering.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0242 admits exact hosted capability tables and lowers the parameterless scalar `process.argument_count` leaf. The real `wvnative` tool next obtains its input and output resource names through `process.argument(index) -> text`. That call adds one scalar input, one borrowed descriptor output, and an explicit runtime-service failure edge, making it the smallest next ownership step before file input or output.

## Decision

### Admit the exact descriptor-returning call

Allow opcode 65 to name `process.argument(u32) -> text`. Require one `u32` stack input, consume it, and produce one borrowed-text descriptor value. Other newly validated capabilities remain rejected when called.

Emit the exact ABI 22 sequence: load the index from its frame cell into `EAX`, move it to service argument `R8D`, pass the destination descriptor address in `R9`, load service-table slot 24 through the existing `R15` execution context, call it, and branch on a nonzero result to the existing runtime-service status tail. Charge the ordinary ten-byte instruction meter plus thirty-six operation bytes.

The service owns argument storage for the execution lifetime; the result is borrowed text, not a new dynamic allocation. The existing descriptor-copy operations may move it without changing ownership. WVO 1.0 still does not serialize the required service, so independently loading the object outside the verified hosted package remains unsupported.

### Require focused real-argument evidence

Add `Wvb-To-Wvo-Process-Argument.wv`. With one argument equal to `A`, it converts the returned text to bytes, validates its length and first byte, and returns 42. Require exact signature checks, reference interpretation, Stage 0 native execution with only `process.argument` authorized, byte-for-byte WVO agreement through both Windvale adapters, and exact current-host package output. Retain the prior malformed capability-index and signature rejections.

Build the memory and hosted adapters through the qualified native source front door and require their identities to match Stage 0. The affected shared-backend and standalone package selections are the only local behavioral checks for this slice. Local Standard, Qualification, the full Seed/OS suites, and artifact promotion remain deferred to the grouped end-of-goal gate.

## Consequences

- The accepted Windvale lowerer now covers both process-input capabilities needed to inspect command-line shape and resource names.
- The focused capability module owns scalar and descriptor service-call emission without expanding runtime-service policy across the large instruction core.
- The current core, memory-adapter, and hosted-tool WVB hashes are `a1fbd67627f806677873eb17eff38c26459b427bba8a79983a0de10429bb6df3`, `ab9219d04e3a3fb7eaced0dd9665b624cef2d4bbc17b78161d2fade38228e6bc`, and `8632b76445d6ea953694a8ae8c366aaa12e3b9b85e2aad34be67ef5648cec5c6`. The latter two contain 283,331 and 284,359 bytes.
- The hosted tool lowers through Stage 0 to 3,978,752 code bytes and a 3,990,198-byte WVO. Current unpromoted packages are 3,997,184 Windows bytes at SHA-256 `3556b23a969f841286d2f43526d6c4e38f448a48ab244171afa9771ad56d9e8f` and 3,997,696 Linux bytes at SHA-256 `b0a5babd6f77834909c4daec4d1d9843cb1a28fef1c01c149952bdf76d2b611b`.
- No C# implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Transfer `file.read_bytes(text) -> bytes` next if the goal remains direct lowering of the real hosted tool. Keep file mutation and void-returning output calls separate so their exact progress, failure, and publication rules remain independently reviewable.
