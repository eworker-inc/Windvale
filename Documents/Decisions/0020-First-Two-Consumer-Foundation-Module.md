# Decision 0020: First two-consumer Foundation module

- Date: 2026-07-30
- Status: Accepted, implemented, and cross-host qualified at `d46af86`

## Context

The qualified Windvale assembler and linker independently implement the same accepted WVO alignment set and machine-name grammar. These functions are pure, bounded, host-independent, and exercised at security-sensitive parsing boundaries. Decision 0019 now permits static source composition, so retaining both complete implementations would create avoidable drift while extracting broader scanners, record models, or speculative collections would be premature.

The linker's WVO string scanner also needs the exact offset of the first invalid character. A whole-value Boolean API cannot preserve that diagnostic by itself.

## Decision

Create `Foundation/Machine-Contracts.wv` as portable source module `Foundationˉmachineˉcontracts` with exactly two exported functions:

- `Foundationˉalignmentˉisˉvalid(Value: u32) -> bool` accepts powers of two from 1 through 4,096.
- `Foundationˉmachineˉnameˉisˉvalid(Input: bytes) -> bool` accepts 1 through 255 ASCII bytes using the WVO machine-name alphabet: ASCII letters, `_`, `.`, or `$` at the first position, with decimal digits additionally allowed afterward.

Both functions are total over their declared values, allocate no unbounded state, use no capability, and expose no host behavior. The Windvale assembler validates symbol, section, definition, and reference spans through the shared whole-value function. The Windvale linker uses it for the requested entry name and uses the shared alignment function for WVO sections. The linker retains a local character predicate only inside its object scanner so it can report the exact first invalid byte; that diagnostic-specific predicate is not published as Foundation API.

Both tools explicitly import the module at compile time. The functions are internalized into each final WVB, and their outputs, capabilities, hosted behavior, WVO bytes, flat image, and canonical map remain unchanged. The assembler and linker WVB identities change because the shared function names and call graphs are now part of their canonical compiled modules.

## Consequences

This is the first Foundation source module justified by two production-like Windvale consumers. It establishes a narrow precedent: share accepted semantic contracts, keep diagnostic ownership local when the result shape differs, and do not turn one extraction into a generic parser or collection framework.

The module does not define Unicode identifiers, native paths, package names, general string validation, arbitrary alignments, address placement, or host ABI policy. Its early-development API may still change with an explicit decision and regenerated evidence.

## Verification gate

The standalone module must compile to a verified WVB with exactly its two declared exports. A portable demo exercises alignment endpoints and rejection, accepted first/subsequent name characters, empty and leading-digit rejection, punctuation rejection, and non-ASCII rejection. The complete assembler and linker suites must retain exact WVO, image, map, diagnostic, and hosted-publication behavior.

Cross-host qualification requires the exact committed archive to pass the complete Windows and Debian verifier, equal normalized Foundation/module/tool contracts, and direct byte comparison of the standalone Foundation WVB plus both composed tool WVB files.

Candidate `d46af86` satisfied this gate on Windows and Debian GNU/Linux 12 x64. Both hosts passed all 36 conformance tests and the complete native CLI verifier with zero build warnings or errors. The standalone Foundation module, its boundary demo, both composed tools, the canonical WVO, and the linked image/map were byte-for-byte equal across hosts; the existing WVO, image, and map identities did not change.
