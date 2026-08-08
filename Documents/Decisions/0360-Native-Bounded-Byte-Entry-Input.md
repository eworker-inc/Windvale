# Decision 0360: Native bounded byte entry input

- Status: Accepted current-host prerequisite; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0080](0080-Native-Byte-Result-And-Live-Stencil-Consumption.md), [Decision 0359](0359-Windvale-Owned-Native-Enum-Name-Leaf.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native execution context](../../Specifications/Windvale-Native-Execution-Context.md#entry-convention)

## Context

Windvale can already execute a capability-free native `Main() -> bytes`, but
the next type-dependent `WVEN` transfer needs one bounded request and one byte
response. Routing that request through the reference interpreter would retain
.NET on the live construction path. Adding a file or process capability would
grant unrelated authority and would not define a reusable in-memory compiler
boundary.

The managed cross-host bridge previously normalized two physical registers by
duplicating result and execution-context pointers across Windows and System V
argument positions. A third duplicated register would collide with one of
those values. The existing result pointer itself supplies a smaller common
anchor for a new entry shape that was previously invalid.

## Decision

- Admit exactly capability-free portable `Main(bytes) -> bytes` in addition to
  the existing parameterless scalar and byte entries. Continue rejecting text,
  scalar-result, multi-parameter, and hosted input entries.
- Keep ABI 22, execution-context version 7, WVB 1.11, and WVO 1.0. Existing
  entry shapes and bytes do not change, and old independent decoders reject the
  newly admitted parameter prologue.
- For the byte-input shape only, allocate an exact 32-byte bridge. Cell zero is
  the existing result descriptor. Cell one is the initialized, bounded,
  reserved-zero descriptor of one execution-owned immutable input copy.
- After ordinary frame initialization, emit exact `lea r8, [rcx + 16]`, then
  use the already verified first borrowed-descriptor parameter sequence. The
  independent decoder requires both sequences together, admits exactly one
  descriptor input, and requires a descriptor result.
- Limit input to the WVB 4 MiB byte-value bound. Reject default or oversized
  input as `WVN4020` before executable publication. Keep the parameterless and
  byte-input executor APIs distinct and reject a shape mismatch as `WVN4011`.
- Permit a successful result to borrow any complete range of the current input
  copy, in addition to the already accepted exact static-data and committed
  execution-arena ranges. Copy the result before releasing every run-owned
  allocation; no descriptor escapes.
- Use this seam for variable-input Windvale construction rather than adding a
  managed-interpreter, file, callback, or host-FFI detour.

## Evidence and consequences

The focused test reviews the exact source signature and generated bridge,
compares reference and x64 results for a constructed value, accepts empty and
exact 4 MiB borrowed results, rejects default and oversized inputs, rejects an
executor-shape mismatch, corrupts the bridge for independent-decoder
rejection, keeps text-input and scalar-result forms outside native admission,
and proves the earlier parameterless byte and scalar entry shapes still run.
The affected Release solution built with zero warnings and errors in 22.34
seconds. After the test-only regression expansion, its focused Release project
build also passed with zero warnings and errors in 9.49 seconds. The single
named case passed 1/1 in 0.435 seconds.

This slice does not itself transfer `WVEN`. C# still verifies the native
fragment, copies the temporary request, allocates and protects executable
memory, invokes the entry, validates/copies the result, and tears the run down.
Those are explicit remaining retirement items rather than hidden new product
dependencies. Linux execution and the broad grouped gate remain deferred.

## Reconsideration triggers

Advance the ABI if another entry parameter, mutable input, retained descriptor,
asynchronous lifetime, or a change to an already admitted entry shape is
required. Replace the managed bridge allocation only when a qualified native
host owns the same bounded lifetime and independent range checks.
