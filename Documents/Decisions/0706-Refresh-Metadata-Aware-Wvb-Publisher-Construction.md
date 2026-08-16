# Decision 0706: Refresh metadata-aware WVB publisher construction

- Date: 2026-08-16
- Status: Implemented candidate refresh with local Windows and Debian WSL2 evidence
- Advances: [Decision 0698](0698-Metadata-Aware-Wvb-Publisher-Source-Candidate.md)
- Contract: [native WVB publisher](../../Specifications/Windvale-Native-Wvb-Publisher.md)

## Context

Decision 0698 produced one exact metadata-aware publisher WVB, WVO, and linked
fragment but deliberately left construction variant 2 bound to the preceding
publisher. That split kept the source improvement visible, but the ordinary
candidate constructor could not yet reproduce an application containing the
new verifier adapter.

The gap spans more than three file hashes. Variant 2 records encode module and
object geometry, identity size, symbol and relocation counts, target addresses,
PE and ELF layout, Windows import placement, independent publisher metadata,
and final application digests. Those values must move as one verified slice.

## Decision

- Bind construction variant 2 to the exact Decision 0698 WVB, WVO, and linked
  fragment.
- Refresh structure, identity, target, object, Windows-import, metadata, PE,
  ELF, and final-digest contracts together. Preserve every other construction
  variant byte for byte.
- Rebuild the affected portable construction WVBs and paired hosted worker
  applications, then retain them in construction-candidate format 21.
- Retain the paired metadata-aware WVB publisher applications as candidate
  format 4. Do not replace `Artifacts/Native-Front-Door` in this decision.
- Treat the historical managed backend only as a detached layout oracle. The
  current native lowerer, linker, constructor, and target workers remain the
  authority for retained bytes and digests; no managed source or direct
  `dotnet` entry point returns to `main`.

## Evidence

The exact input remains the 181,772-byte WVB at SHA-256
`c90f5325ea409d0710254812e1d434cce712de68385dec74d23eef5a475cf3c4`.
It lowers to the 1,523,708-byte WVO at SHA-256
`c1ce50f68e12dc94e56fa848c6f09f707ad117294af5e19f15659b7901c0bf35`
and links to the 1,520,746-byte fragment at SHA-256
`98aba65ccfdb0455f9fcb78ad3ffa0ecbe7aa942fcbf9064d179018dec12178a`.

Native construction produces:

| Target | Base bytes / SHA-256 | Final bytes / SHA-256 |
| --- | --- | --- |
| Windows x64 | 1,537,024 / `6385eac0d7c326f9dbded708a064eecb113fcf41c036b59b519938ee1a5b5e8c` | 1,544,192 / `0fdb432aa54cc7b9cc4a1d42a438d2b56a29695e06b2369540dac845989751c1` |
| Linux x64 | 1,536,000 / `1e3049360820c321df5489e2df6f2cbb748565f20e95e130c1ff08edbe7622c4` | 1,541,109 / `7bf4593566401853ab7f551ca5d45125ac0ea3a6c4e34315703785ed7d6cdfb6` |

The focused owner passes locally on Windows. Debian WSL2 independently runs
the Linux worker set, reconstructs the exact Linux candidate, and exercises its
metadata-aware publication behavior. These are local focused results, not the
complete dual-host Qualification gate or a release claim.

## Consequences

- Package sizes and hashes change because the publisher now contains the
  independent-metadata verifier adapter; they are evidence, not the language
  improvement itself.
- The candidate publisher can admit absent-form and valid replacement metadata
  through the same semantic verifier boundary.
- The retained native front door and Echo package remain unchanged. Their
  cutover is explicit follow-up work after candidate review and broader gates.
- Other publisher construction variants retain their existing identities.

## Reconsideration triggers

Reconsider this decision if module-metadata framing changes, if variant 2 gains
another target or capability profile, if the paired candidate fails independent
host reconstruction, or when the metadata-aware candidate is selected for the
ordinary native front door.
