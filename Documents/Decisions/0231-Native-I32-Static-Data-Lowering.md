# Decision 0231: Native i32 static-data lowering

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0228](0228-Bounded-Acyclic-Native-Call-Directory.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

The Windvale-written WVB-to-WVO lowerer already owned bounded scalar control and decreasing-ordinal calls, but rejected all static data. Canonical `Examples/Seed/Sum-Data.wv` was therefore still forced through the frozen C# backend even though its exact 493-byte WVB, immutable `[3,5,8,13]` data, loop, call, instruction count, and result 29 are already qualified across Windows, Linux, and Windvale OS.

Adding data decoding, object sections, symbols, and relocations directly to the 2,887-line instruction core would also reverse the focused-source extraction established by Decision 0228.

## Decision

### Admit one bounded immutable i32 array

Accept either no data declarations or exactly one canonical `[i32]` declaration of at most 262,144 elements. The independent parser validates the bounded name and exact payload before exposing immutable little-endian data bytes. Text, byte data, multiple declarations, capabilities, and nominal types remain outside this candidate.

Add typed support for WVB `data.length` and `data.load.i32`. Both validate data index zero and preserve the existing empty-stack block contract. `data.load.i32` emits the Stage 0 unsigned bounds comparison and exact ABI-22 `WVR3005` data-bounds trap before its RIP-relative load.

### Emit the canonical relocatable object

When data is present, pad `.text` to 16 bytes exactly as Stage 0 does, emit a 16-byte-aligned `.rodata` section, add local `$data_0000` before function symbols, clear each RIP-relative displacement field, and emit ordered `Relative_i32` relocations with symbol index zero and addend `-4`. Code-only objects remain byte-identical.

The core records relocation field offsets while scanning the already verified WVB instruction stream. WVO construction validates those ordered zero placeholders before publication.

### Keep source responsibilities focused

Add the 99-line `Native-X64-Lowering-Data.wv` for the bounded WVB data payload and the 154-line `Native-X64-Lowering-Object.wv` for canonical WVO projection. Reduce `Native-X64-Lowering-Layout.wv` from 146 to 82 lines so it owns only the function directory and helper names. The instruction core grows to 3,069 lines because typed stack analysis, machine-size accounting, bounds branches, and data-load selection remain instruction-lowering responsibilities; object serialization and data-format parsing do not move into it.

### Extend the affected tests before execution

Retain every existing exact code-only comparison. Add canonical `Sum-Data.wv` as the real Stage 0 differential oracle: the Windvale memory adapter and hosted/native tool must produce its exact 3,288-byte WVO, including 3,088 `.text` bytes, 16 `.rodata` bytes, `$data_0000`, and one relative relocation. An out-of-range encoded data index fails closed as `Unsupportedˉcode`.

Refresh the derived hosted-tool and paired package identities once. Run only the two affected Fast selections locally; defer Standard, Qualification, full Seed/OS, Linux execution, GitHub verification, and promotion to the grouped end-of-goal gate.

## Consequences

- The accepted-subset native backend now lowers the already-qualified canonical data/loop/call program instead of only synthetic data-free inputs.
- Data parsing and WVO serialization have explicit focused owners rather than enlarging the already-large instruction source with unrelated format logic.
- The C# backend remains the frozen complete recovery and differential oracle. This candidate does not support multiple declarations, text/bytes data, nominal or descriptor values, capabilities, recursion, general calls, complete fragment verification, or the complete compiler.
- No native artifact is promoted and no normal launcher changes in this decision.

## Reconsideration triggers

Extend the data directory only when a real next backend input needs multiple declarations or text/bytes data. Extend object emission only through canonical WVO symbols and typed relocations; do not add a private object format. Reconsider the instruction-core boundary when a cohesive machine-selection or control-analysis module can be extracted without duplicating mutable state or obscuring frame and trap invariants.
