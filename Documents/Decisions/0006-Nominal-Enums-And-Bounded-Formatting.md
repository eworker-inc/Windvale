# Decision 0006: Nominal enums and bounded formatting

- Date: 2026-07-29
- Status: Accepted; cross-host qualification pending

## Context

Immutable records let `Wvˉdumpˉcore` return section counts and failure offsets, but its `Status` field still contained magic integers. The tool also could not turn a structured result into deterministic text without host code. Adding general reflection, interpolation, format strings, or locale-aware formatting would create a large library and runtime surface before the first binary tool needs it.

The WVB 1.2 Types section encoded only records without a declaration tag. Adding a separate enum section would preserve that short-lived encoding but make every future nominal type a new top-level section and index space. Generalizing Types now is the smaller long-term format.

## Decision

- Add nominal enums with explicit member names and values.
- Make enum identity part of source types, WIR types, bytecode value shapes, verifier stack state, and runtime values.
- Permit enums in function signatures, locals, and record fields. Continue to defer nested records.
- Provide exact enum equality and inequality only between the same nominal enum.
- Add `Enumˉname` for deterministic declared member names; do not add integer conversions or reflection.
- Add invariant decimal `I32ˉformat`, `U8ˉformat`, and `U32ˉformat` operations.
- Add `Textˉconcat` with a strict 1 MiB UTF-8 result limit checked before concatenation. Overflow traps rather than truncates.
- Generalize the Types section with tagged record and enum declarations and advance the early-development bytecode format from 1.2 to 1.3 without a backward reader.
- Group canonical nominal declarations by kind and sort names ordinally within each kind. Type names remain unique across kinds.
- Replace `Wvbˉinspection.Status: i32` with the `Wvbˉstatus` enum and exercise bounded summary formatting inside the portable WvDump core.

## Consequences

- Tool status values are readable and type-safe without depending on host constants.
- Record schemas can carry scalar enum fields while remaining immutable and nonrecursive.
- Portable reports have host-independent number and enum text.
- Formatting allocates bounded immutable text in the Stage 0 runtime; it does not establish a future native allocator design.
- WVB 1.2 golden modules and reports are intentionally replaced. The qualified 1.2 evidence remains historical, not supported input compatibility.
- The Types-section tag avoids a new mandatory module section for each future nominal declaration family.
- Seed still lacks efficient multi-part builders; repeated concatenation is acceptable only for small bounded diagnostic lines.

## Reconsider when

- Useful reports need streaming writers, reusable builders, padding, hexadecimal values, escaping, or machine-readable structured output.
- Flags, implicit numbering, negative source members, or enum/integer conversions become necessary for object or ABI formats.
- Another nominal declaration family demonstrates that the tagged Types grammar is too rigid.
- Formatting measurements show repeated immutable concatenation materially obstructs Windvale-written tools.
