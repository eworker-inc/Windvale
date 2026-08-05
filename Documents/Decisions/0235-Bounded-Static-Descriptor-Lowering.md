# Decision 0235: Bounded static descriptor lowering

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0234](0234-Bounded-Native-Scalar-Comparisons.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

After Decision 0234 completed the bounded scalar comparisons, compiler-produced `Data-And-Text.wv` was blocked first by static text/bytes descriptors and multiple immutable data declarations. The accepted ABI 22 representation is already a 16-byte descriptor containing an address, length, and reserved word. This fixture needs only borrowed views over immutable module data: it does not allocate, concatenate, pass, or return descriptors.

Adding that complete narrow family to the existing 3,800-line lowering core initially pushed its generated `Compilerˉnativeˉx64ˉemitˉcontrolˉcode` helper from below the 2,048-cell frame limit to 2,150 cells. Raising the safety limit would hide a source-ownership problem and make the native tool harder to review.

## Decision

### Admit bounded immutable descriptor data

Accept zero through eight immutable text, bytes, or `[i32]` declarations. Bound each text at 1 MiB of valid UTF-8, each byte declaration at 4 MiB, and each i32 array at 262,144 elements. Preserve declaration order, pack their exact payloads into one `.rodata` section, emit one exact `$data_NNNN` symbol per declaration, and retain typed relocation ownership for every reference.

Admit `text` and `bytes` declared locals while keeping parameters and returns scalar-only. Lower `text.const`, `bytes.const`, `text.to_utf8`, `bytes.length`, `bytes.slice`, and little-endian byte reads of `u8`, `u16`, `u32`, and `i32`. Local loads and stores copy the complete 16-byte descriptor. Static constants and UTF-8 conversion are borrowed views; slicing adjusts a checked address/length pair. Reads and slices use ABI 22's byte-bounds status rather than the i32-array bounds status.

Do not admit dynamic concatenation, descriptor parameters or returns, arena ownership, generation changes, capabilities, nominal values, mutable data, or a general relocation family in this slice.

### Extract descriptor instruction ownership

Keep low-level descriptor machine-byte templates in `Native-X64-Lowering-Descriptors.wv`. Move descriptor stack selection and state transitions into the focused 280-line `Native-X64-Lowering-Descriptor-Instructions.wv` module. The lowering core retains orchestration and consumes one explicit descriptor state result. This restores native compilation below the existing frame limit and reduces the core without splitting it into numbered fragments or weakening invariants.

### Extend exact differential and malformed evidence

Use `Wvb-To-Wvo-Static-Descriptors.wv` as the focused vector. It combines one text, one bytes value, one i32 array, descriptor locals/copies, UTF-8 conversion, slicing, length, and all four admitted reads. Stage 0 interpretation and native execution return 42; the memory adapter, hosted tool, and generated native tool reproduce Stage 0's exact 7,626-byte WVO with two sections, three data symbols, and three relocations.

Reject both an out-of-range descriptor data index and a text constant naming bytes data before publication. Retain all earlier exact WVO vectors unchanged. The reviewed shared-backend selection is the only local verifier for this coherent slice; Standard, Qualification, Linux execution, GitHub verification, and artifact promotion remain deferred to the grouped end-of-goal gate.

## Consequences

- The native lowerer now owns the complete static borrowed descriptor surface needed by the next real compiler-produced fixture.
- Multiple immutable declarations share one deterministic `.rodata` section and typed symbol/relocation model.
- Exact comparison still uses Stage 0 as independent evidence during migration; the values are not hardcoded behavioral substitutes. Derived artifact identities are retained only after behavior and exact-object checks pass.
- The generated hosted tool is 146,392 WVB bytes and lowers through Stage 0 to 1,942,164 code bytes and a 1,947,548-byte WVO.
- No normal .NET dependency is removed and no candidate artifact is promoted by this local proof.

## Reconsideration triggers

Choose the next real fixture blocker by measurement. Any dynamic descriptor operation must first preserve ABI 22 ownership, allocation, generation, failure, and cleanup contracts. Descriptor calls require a separate parameter/return decision rather than extending this borrowed-local rule implicitly.
