# Decision 0423: Compiler-scale native lowerer admission

- Status: Implemented candidate; compiler-scale native staging completes
- Date: 2026-08-08
- Advances: [Decision 0420](0420-Multi-Fragment-Current-Lowerer-Reconstruction.md) and [Decision 0304](0304-Digest-Bound-Native-Wvb-To-Wvo-Candidate.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The 413-function semantic-freeze compiler WVB is a materially larger lowerer
input than the previously reconstructed 409-function lowerer application. Its
measured envelope contains 110 static-data declarations, 63 records plus 16
enums, and record-bearing functions with as many as 674 declared record locals
and 214 produced record values in one block. The Windvale lowerer rejected that
input at several conservative prototype bounds before reaching the actual
record-liveness workload.

Those rejections were difficult to distinguish because every invalid control
analysis collapsed to the same `Unsupportedˉcode` status. The record-storage
scanner also omitted the already implemented `u32` bitwise and shift opcodes,
so record-bearing functions using those scalar operations were rejected even
though the ordinary scalar lowerer supported them.

## Decision

Admit a bounded compiler-scale envelope without weakening the retained
per-entry, code-size, instruction-count, frame, or malformed-input checks:

- at most 256 immutable static-data declarations;
- at most 64 record declarations and 64 enum declarations, with 128 nominal
  declarations in total;
- at most 1,024 declared record locals in a record-bearing function; and
- at most 256 produced record values in one basic block.

Keep the complete function frame below 2,048 ABI cells and the function body
below 32,768 code bytes and 8,192 instructions. Encode record interference as
packed bits. Preserve the deterministic descending-width, first-fit allocation
order and exact Stage 0 WVO output.

Classify `u32.bitwise_and`, `u32.bitwise_or`, `u32.bitwise_xor`,
`u32.bitwise_not`, `u32.shift_left`, and `u32.shift_right` in the record-storage
scanner with their existing scalar stack effects. No new source or bytecode
semantics are introduced.

Retain one general failure sentinel, but report a function ordinal and a
bounded failure detail for function-specific control-analysis rejection. Detail
values identify coarse analysis stages; values beginning at 1,000 identify the
failing basic block. The hosted staging diagnostic prints both fields. This is
diagnostic evidence only and does not change valid object bytes.

## Evidence and consequences

The pinned compiler input is 914,746 bytes at SHA-256
`48ff781359d9bab96ec3e19e4edba19a26ba82552d5bfd1c1a72d64b75f224a6`.
It has 413 functions, 110 data declarations, 63 records plus 16 enums, and six
standard capabilities.

Focused differential tests admit the exact 110-data and 63-record/16-enum
compiler envelopes, 674 declared record locals, and 216 produced record values
in one block. The record-capability owner additionally requires all six extended
`u32` bitwise/shift operations while a record is live. Each focused owner
compares the Windvale-produced WVO with the Stage 0 native backend byte for
byte.

Compiler probes successively clear function 39's 350-local dense-interference
failure, function 245's missing shifts, function 246's missing bitwise family,
function 308's 214-value block, and function 312's former 512-local admission
failure. Function 312's Stage 0 plan uses 229 persistent and 147 scratch field
cells, so frame capacity is not the blocker. The remaining failures were native
lifetime failures caused by repeatedly rebuilding growing immutable byte
buffers; increasing another semantic or frame limit was not justified.

The first lifetime slice stores the fixed-point `Liveˉin` table as row-aligned
packed bits, reducing function 312's table from 178,610 bytes to 22,525 bytes
without changing uses, definitions, interference, allocation order, or WVO
output. The focused differential owner passes, and the native compiler probe
falls from about 46 seconds to about 35 seconds before the same pre-status exit.
Repeated immutable interference-row replacement is therefore the next measured
hotspot; the packed fixed-point table is retained as an independently verified
improvement.

The second lifetime slice records interference evidence per control-flow block,
folds each bounded event set into a row-aligned packed matrix, and constructs
the expanded local-offset directory sequentially. Function 312's complete
persistent and scratch storage plan then returns in about five seconds. The
first newly reachable emission failure is function 306,
`Compilerˉsourceˉwirˉcompileˉexpression`, whose 1,303 locals approach the
2,048-cell frame envelope. Bounded 4 KiB code and prologue chunks with a 64 KiB
aggregation tier preserve exact machine bytes without repeatedly rebuilding a
large function prefix.

A direct Windows native staging probe now completes the 914,746-byte compiler
input in about 98 seconds. It reports a 27,458,862-byte WVO split into 167
bounded chunks with a 2,028-byte manifest. The focused record-lifetime,
674-record-local, and maximum-frame chunked-emission owners pass exact
differential comparison. This is candidate evidence, not the final dual-host
qualification gate.

The current ordinary candidate is a 408,243-byte WVB with SHA-256
`a118468779796449dfadbdaeba202b4460748963573fa74fbd9fda4b1ff2e755`.
The segmented producer is a 432,433-byte WVB with SHA-256
`cf305684a62441bce0d532872e04b9ad9e62a200be62be01ecf120f31f9a8835`.
Its Stage 0-constructed Windows and Linux containers remain candidate artifacts,
not qualified replacements. No C# compiler or runtime implementation changes
are part of this decision.

## Reconsideration triggers

Revisit these bounds when a qualified source closure exceeds them or when a
smaller representation can preserve the same deterministic allocation. Do not
raise a bound solely to clear a generic failure. Add measured evidence and a
focused boundary test first. Version serialized storage evidence if its external
shape changes; internal packed representations may evolve while exact valid WVO
bytes remain unchanged.

[Decision 0425](0425-Compiler-Scale-Native-Wvo-Resource-Staging.md) advances
this candidate by reducing the exact compiler WVO from 167 publication
resources to 36 staging resources without changing shared publication policy or
WVO bytes.
