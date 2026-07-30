# Decision 0014: Windvale linker object views

- Date: 2026-07-30
- Status: Accepted and implemented; cross-host qualification pending

## Context

The Windvale linker must repeatedly inspect as many as 64 verified WVO inputs while resolving symbols, laying out sections, applying relocations, independently reconstructing the image, and emitting map evidence. Seed does not yet have general arrays or module imports. Delegating WVO decoding to C# would leave the most security-sensitive link input boundary outside the Windvale implementation, while introducing an abstract collection system before the real resolution passes would guess at future requirements.

WVO is already canonical and bounded. Section and symbol records contain variable-length names and inline data, but their limits permit deterministic rescanning from known region starts.

## Decision

- Implement the complete WVO 1.0 structural acceptance boundary in `Wvˉlinkerˉcore` using immutable byte values and checked bounded scans.
- Return an immutable scan record containing aggregate counts and exact section, symbol, and relocation region offsets.
- Represent individual section, symbol, and relocation records as immutable views containing primitive fields plus byte offsets and lengths into the original object.
- Re-find variable records through bounded passes rather than storing host objects, mutable cursors, or a premature general collection.
- Check global section-name uniqueness explicitly and exploit canonical binding/name ranges to check symbol-name uniqueness across local, export, and import ranges with a bounded merge.
- Keep this parser internal to the Windvale linker. The C# WVO codec remains an independent Stage 0 oracle and recovery implementation, not a runtime service used by Windvale code.
- Qualify acceptance against representative canonical objects, deterministic mutations, bounded random bytes, real CLI hosted scanning, and exact Windows/Debian conformance reports before building resolution on this boundary.

## Consequences

- Later link passes can reread exact object fields from deterministic first-read file snapshots without trusting a second decoder or ambient file state.
- The current algorithm has repeated scans and some quadratic bounded work, especially section lookup and name validation. Those costs are visible and constrained by WVO limits, but they may justify a narrow bounded collection once full linker measurements exist.
- Parser statuses are development diagnostics for the internal scan boundary; the completed linker must still map failures to the accepted WVL diagnostic contract.
- This milestone does not produce or publish an image and therefore does not qualify the Windvale linker or complete Phase 6.

## Reconsider when

- Resolution and map construction require retained records whose repeated-scan cost exceeds practical instruction limits.
- A shared Foundation collection is justified by both the qualified assembler and measured linker rather than by this parser alone.
- WVO changes during early development; update the Windvale scanner and independent oracle together and requalify exact reports rather than adding an obsolete-format compatibility path.
