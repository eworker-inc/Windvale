# Decision 0919: advance the frozen source identity for WVB 1.37 containment

## Status

Accepted on 2026-09-02 and implemented by the Language 1.0 source-amendment
candidate named below. This decision repairs exact source-design provenance; it
does not execute WVB 1.37, form a native pointer, authenticate a Foreign call,
or claim Linux qualification.

## Context

[Decision 0917](0917-Advance-The-Frozen-Source-Identity-For-Candidate-Wvb-1.37.md)
bound the accepted Language 1.0 design through candidate write-pointer
serialization.
[Decision 0918](0918-Verify-WVB-1.37-Write-Pointer-Lifetime-Containment.md)
then admitted the exact candidate through complete compiler-aligned affine
verification while deliberately keeping all execution consumers closed.

That accepted checkpoint changes two frozen explanatory inputs:
`Specifications/Windvale-Language-1.0-Foundation.md` and
`Documents/Project/Windvale-Language-1.0-Migration.md`. The prior manifest must
remain immutable, and the front-door fixture verifier must reject an unrecorded
source identity.

## Decision

1. Accept
   `Documents/Project/Windvale-Language-1.0-Source-Amendment-0919-Candidate.txt`
   as amendment version 12 over the immutable Decision-0917 manifest.
2. Bind the current exact bytes of all 13 core design inputs. Relative to the
   version-11 base, only the Foundation specification and migration record
   change.
3. Preserve the 16 accepted source-design decisions and both frozen paper
   corpora byte-for-byte. The complete candidate remains 251 identity inputs.
4. Bind the replacement identity to these exact values:

   | Evidence | Exact value |
   | --- | ---: |
   | Manifest bytes | 3,789 |
   | Manifest SHA-256 | `6654ca1e547dc73d0b1d48bc69d1bd365156f20dd0277f8bb28f589b1c74cb6e` |
   | Frozen inputs | 251 |
   | Frozen input bytes | 1,776,773 |
   | Entry-stream bytes | 46,260 |
   | Entry-stream SHA-256 | `cf16aa868688a2dfa1dad099fad497e71b0f0be40e38d02a1e7d8ff7e185b888` |
   | Foundation bytes | 86,015 |
   | Foundation SHA-256 | `83d86d91ecbe94c4a901793e34a77306c9233ae822d7161ea5770c577f1a0c67` |
   | Migration bytes | 62,835 |
   | Migration SHA-256 | `bc3baea24c0a32c64e2b9c407143af100a1cf412f570e6343f9dd76ea6daa98a` |

5. Rebind the migration-fixture verifier and fixture inventory to version 12.
6. Route the new amendment through the same development classification,
   Language 1.0 front-door owner, and verification-planner self-test as every
   retained amendment.

## Consequences

- The frozen source identity matches the accepted WVB 1.37 compiler-aligned
  pointer-containment description and fails closed on later unrecorded drift.
- Prior manifests continue to identify their historical source states.
- This provenance amendment adds no source syntax, WVIR operation, WVB opcode,
  runtime service, native ABI, capability, or compatibility behavior beyond the
  separately accepted Decision 0918 verification boundary.
- Candidate WVB 1.37 remains non-executable. Provider/native address formation
  and authenticated no-retain Foreign calls remain future work.
- Windows-local development evidence is recorded separately; paired Windows and
  Linux qualification remains required before the 1.0 claim.

## Reconsideration triggers

Create another immutable amendment instead of editing this one if an accepted
Language 1.0 core identity changes again. Reconsider the manifest chain only if
a replacement provenance format preserves all prior exact identities and at
least the same malformed-input and routing guards.
