# Decision 0244: Native file-read-bytes capability

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0243](0243-Native-Process-Argument-Capability.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0243 transfers both process-input leaves needed by the real `wvnative` shell. Its next normal action is `file.read_bytes(resource_name) -> bytes`, which obtains the WVB payload to verify and lower. This call consumes a borrowed-text descriptor, produces a borrowed-bytes snapshot descriptor, and can fail through the existing runtime-service status path.

Adding the operation directly to the already-large instruction core also exceeded the native compiler's 2,048-cell per-function frame limit. That limit exposed a real ownership boundary: capability stack analysis, service-call selection, and capability-result state belong together in the focused capability module rather than as more branches in the general instruction orchestrator.

## Decision

### Admit the exact borrowed file snapshot call

Allow opcode 65 to name `file.read_bytes(text) -> bytes`. Require one text value, consume it, and produce one borrowed-bytes descriptor value. Keep file mutation, console output, and diagnostic output calls rejected pending their separate slices.

Emit the exact ABI 22 sequence: load the source descriptor pointer into `R8` and length into `R9D`, pass the destination descriptor address in `RCX`, load service-table slot 32 through the existing `R15` execution context, call it, and branch on a nonzero result to the shared runtime-service tail. Charge the ordinary ten-byte instruction meter plus forty-two operation bytes, for fifty-two planned bytes.

The native file-input service owns the immutable snapshot storage for the execution lifetime. The returned compiler value is borrowed bytes; descriptor copies do not create or transfer dynamic ownership. WVO 1.0 still does not serialize its required service, so independently loading this object outside the verified hosted package remains unsupported.

### Keep capability state out of the general instruction core

Move capability-specific stack analysis and emitted-result state into `Native-X64-Lowering-Capabilities.wv`. The core now delegates the capability kind, current typed stack, frame groups, and runtime failure target, then consumes one explicit result record. This restores the native frame bound and prevents each hosted-service slice from further expanding the general instruction branch.

### Require focused file-read evidence

Add `Wvb-To-Wvo-File-Read-Bytes.wv`. It obtains a temporary file name through the already-transferred process-argument leaf, reads one byte, checks that the snapshot contains `A`, and returns 42. Require exact capability signatures, reference interpretation through a bounded injected reader, Stage 0 native execution through the real current-host file-input service, and byte-for-byte WVO agreement through both Windvale adapters and the standalone native package.

Build the memory and hosted adapters through the qualified native source front door and require their identities to match Stage 0. The affected shared-backend and standalone package selections are the only local behavioral checks for this slice. Local Standard, Qualification, the full Seed/OS suites, and artifact promotion remain deferred to the grouped end-of-goal gate.

## Consequences

- The accepted Windvale lowerer now covers the read side of the real hosted WVB-to-WVO shell without using the C# lowerer for the input module.
- Capability-specific analysis and emission have a focused owner instead of growing the 4,562-line general instruction core.
- The current core, memory-adapter, and hosted-tool WVB hashes are `baac80e43995fa80de544470d712925a6c908be5620b56599b53d1788f228c70`, `6c483fa9445dc0f3e1c51327e63500764464718f4b40e5fb9ea3fe0e97707dac`, and `4edd7a9fcc63b3cd7936190f0b401effd02d1be14d51ab243b343408b0de4f1f`. The latter two contain 288,933 and 289,961 bytes and reproduce exactly through the native build driver.
- The hosted tool lowers through Stage 0 to 4,050,000 code bytes and a 4,061,616-byte WVO. Current unpromoted packages are 4,068,352 Windows bytes at SHA-256 `49825b98c596ad9778dace1b83f4c16fcc3ae215840aa0ea7f2119238d0310d7` and 4,067,328 Linux bytes at SHA-256 `fa41fe798e30a62a7b576033f6462b020585ba4842e81c929e9ce09642ae33b1`.
- No C# implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Transfer `file.write_bytes(text, bytes) -> void` next if the goal remains direct lowering of the real hosted tool. Review its exact mutation, failure, and completion contract independently before adding console or diagnostic output leaves.
