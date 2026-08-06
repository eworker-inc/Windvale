# Decision 0248: Measured native-lowering module envelope

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0247](0247-Native-Diagnostic-Write-Line-Capability.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

After Decision 0247, the real compiler-produced hosted `wvnative` module declares 33 immutable data items, 29 nominal types, and 297 functions. Its instruction surface is not yet completely accepted, but the Windvale lowerer rejected the module earlier at three unrelated prototype guards that allowed only eight entries in each table.

These guards were useful while the candidate owned only small fixtures. Keeping them now would split one measured module-envelope problem into artificial per-table slices and would prevent later instruction work from reaching the real tool. Raising only the numeric guards would also be incomplete because canonical WVO helper and data symbol names were hard-coded only through index 7.

## Decision

### Admit one bounded measured envelope

Allow at most 512 functions, 64 immutable data declarations, and 64 nominal types. These limits cover the real tool with bounded headroom while retaining explicit validation before table allocation or iteration.

The 64-type limit also preserves the current one-byte native nominal tags: enum tags occupy 128 through 191 and record tags occupy 192 through 255. This decision does not expand nominal shapes, data-item sizes, function signatures, instructions, machine-code budgets, relocation counts, or runtime limits.

### Generate canonical symbol names

Replace the eight hard-coded `$function_0000` through `$function_0007` and `$data_0000` through `$data_0007` cases with one layout-owned canonical D4 name builder. Function names remain `$function_` plus the zero-padded ordinal and data names remain `$data_` plus the zero-padded ordinal, matching Stage 0 and WVO 1.0 exactly.

Keep the generator in the focused layout module, which already owns function-directory layout and helper naming. The object writer reuses it for data symbols. Do not add hundreds of duplicated branches or enlarge the already-large instruction core for a formatting responsibility.

### Require one crossing fixture

Add `Wvb-To-Wvo-Large-Envelope.wv` with nine data declarations, nine nominal types, and ten functions. Require `$data_0008` and `$function_0008` to be present and require byte-for-byte WVO equality between Stage 0, the Windvale memory adapter, and the hosted Windvale tool.

Retain the existing small fixtures for indices zero through seven and packaged execution. Build the memory and hosted adapters through the qualified native source front door and require identities equal to the Stage 0 output. Local Standard, Qualification, the full Seed/OS suites, and artifact promotion remain deferred to the grouped end-of-goal gate.

## Consequences

- The real hosted tool now passes its data-count, type-count, and function-count admission boundaries. Unsupported instructions and shapes later in the module remain explicit next blockers; this decision does not claim complete self-lowering.
- The current core, memory-adapter, and hosted-tool WVB hashes are `4e5c31c3bcb73520333db3bfac0d2eb2a4991fa1fec51577d76916ad239e1a01`, `4323858f697be2fcb9f84689c5504c1c45586cd72ae7c60128f03fc78b132d42`, and `554da065fed54111d0cd2bd119b8cd630f3f11eb6ef917c991e07bf7758d745f`. The latter two contain 294,515 and 295,543 bytes and reproduce exactly through the native build driver.
- The hosted tool lowers through Stage 0 to 4,113,478 code bytes and a 4,124,898-byte WVO. Current unpromoted packages are 4,131,840 Windows bytes at SHA-256 `fff0074df7f9c9c6b352932b7d6404188599db0a584df62457fc03b022469988` and 4,132,864 Linux bytes at SHA-256 `65ca225b08d9c10a919ff14d56aec39645caacfe139e9bad72ab356877e1ab19`.
- Generated names reduce source duplication and package size while preserving exact object bytes for the previously accepted fixtures.
- No C# implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Inspect the first remaining failure after the real tool crosses this envelope and select the next measured instruction, shape, or required-service boundary. Revisit the numeric limits only when a real accepted module reaches one, and preserve a bounded tag and serialization design rather than widening them speculatively.
