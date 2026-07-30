# Windvale development roadmap

## Status

This roadmap expresses the active long-term goal and its current best route. The destination is durable; intermediate phases are adaptable. When experiments reveal an impractical contract or a clearly better alternative, update the relevant specification or decision and revise this roadmap rather than preserving accidental early designs.

## Sequencing principle

Windvale remains bytecode-first for as long as that reduces bootstrap loops. A new Windvale-written tool should become useful and reproducible on Windows and Linux before Windvale OS depends on it. Portable logic remains separate from hosted I/O, and each qualified phase requires deterministic artifacts, mandatory verification, adversarial coverage, and real cross-host evidence.

## Phases

| Phase | Deliverable and qualification gate | Status |
| --- | --- | --- |
| 0. Seed and byte primitives | C# Stage 0, typed WIR, WVB 1.1, verified runtime, `u8`, `u32`, immutable bytes, and Windows/Debian equality. | Qualified |
| 1. `Wvˉdumpˉcore` | Windvale source safely walks complete WVB headers and section envelopes over supplied bytes, including hostile lengths and malformed cases. | Implementation candidate |
| 2. Structured inspection | Add only the records, enums, structured results/errors, and bounded formatting demanded by useful section descriptions. | Planned |
| 3. Hosted resource boundary | Explicit arguments, file-byte input, diagnostics, and output capabilities with portable parsing kept independent. | Planned |
| 4. Useful `wvdump` | Inspect the same real modules identically on Windows and Debian with golden machine-readable reports. | Planned |
| 5. Object foundation | Deterministic byte construction, sections, symbols, relocations, and the smallest shared object contracts needed by an assembler. | Planned |
| 6. Assembler and linker | Windvale-written assembler and linker running first as verified bytecode on Windows and Linux. | Planned |
| 7. Foundation modules | Compact reusable collections, text, binary-format, diagnostics, testing, and I/O-adapter modules driven by tool needs. | Planned |
| 8. Self-hosted compiler | Windvale-written lexer, parser, semantics, and code generation for a meaningful subset, followed by a reproducible bootstrap closure. | Planned |
| 9. Native backend | Native WIR lowering, first x86-64 subset, calling convention, object output, and bytecode/native differential tests. | Planned |
| 10. Native host tools | Produce and qualify native Windvale programs in controlled Windows and Linux environments. | Planned |
| 11. Boot path and kernel | x86-64 UEFI/QEMU boot, diagnostics, memory foundation, minimal kernel boundary, and Hyper-V qualification. | Planned |
| 12. Runtime on Windvale OS | Load, verify, and run one identical Windvale module across Windows, Linux, and Windvale OS. | Planned |
| 13. Public foundation | Reproducible recovery bootstrap, security limits, licensing, governance, contribution rules, and public-release criteria. | Planned |

## Current focus

Phase 1 is the current implementation focus. It is complete only after the exact committed source passes the Windows verifier, the Debian QA verifier, and portable report comparison. Phase 2 begins from concrete limitations observed in that qualified tool rather than from a speculative general-purpose type system.
