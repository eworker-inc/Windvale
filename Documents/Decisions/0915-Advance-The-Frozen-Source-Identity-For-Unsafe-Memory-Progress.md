# Decision 0915: advance the frozen source identity for unsafe-memory progress

## Status

Accepted on 2026-09-02 and implemented by the Language 1.0 source-amendment
candidate named below. This decision repairs exact source-design provenance; it
does not widen the executable boundary or claim Linux qualification.

## Context

[Decision 0901](0901-Advance-The-Frozen-Source-Identity-For-Unsafe-Scratch.md)
bound the accepted Language 1.0 source design before the subsequent immutable
borrow, mutable write-region, bounded-provider execution, native x64 lowering,
and typed write-pointer checkpoints. Those accepted implementation checkpoints
changed two frozen explanatory inputs:
`Specifications/Windvale-Language-1.0-Foundation.md` and
`Documents/Project/Windvale-Language-1.0-Migration.md`.

The Language 1.0 front-door owner therefore rejected the tree with
`Frozen core input differs`. That failure correctly detected an unrecorded
source identity. The prior manifests remain historical evidence and must not be
rewritten or the comparison weakened.

## Decision

1. Accept
   `Documents/Project/Windvale-Language-1.0-Source-Amendment-0915-Candidate.txt`
   as amendment version 10 over the immutable Decision-0901 manifest.
2. Bind the current exact bytes of all 13 core design inputs. Relative to the
   version-9 base, only the Foundation specification and migration record
   change.
3. Preserve the 16 accepted source-design decisions and both frozen paper
   corpora byte-for-byte. The complete candidate remains 251 identity inputs.
4. Bind the replacement identity to these exact values:

   | Evidence | Exact value |
   | --- | ---: |
   | Manifest bytes | 3,794 |
   | Manifest SHA-256 | `2c17a758f2d2ece2063a08e6ff9acbc384a60d8f40d136de73506ff34f02ea50` |
   | Frozen inputs | 251 |
   | Frozen input bytes | 1,774,532 |
   | Entry-stream bytes | 46,260 |
   | Entry-stream SHA-256 | `02f3e6963266f3268be69caa27e0ed8383f752aa559110a91d09cd83e8aaaf68` |
   | Foundation bytes | 85,247 |
   | Foundation SHA-256 | `c97ab92da0e714e87febabc53a465b576ed6946cfb03e3dd54ee5f78e0827edd` |
   | Migration bytes | 61,362 |
   | Migration SHA-256 | `9280d99f85186f655b8c6f43938c4d6306c9bf7a27e94b170e013eb1552a5fe7` |

5. Rebind the migration-fixture verifier and fixture inventory to version 10.
6. Route the new amendment through the same development classification,
   Language 1.0 front-door owner, and verification-planner self-test as every
   retained amendment.

## Consequences

- The frozen source identity again matches the accepted compiler and migration
  description and fails closed on a future unrecorded change.
- Prior manifests continue to identify their historical source states.
- This provenance amendment adds no source syntax, WVIR operation, WVB opcode,
  runtime service, native ABI, capability, or compatibility behavior.
- Decision 0914's typed write pointer remains WVIR-only. Executable pointer
  representation and authenticated foreign calls remain future work.
- Windows-local development evidence is recorded separately; paired Windows
  and Linux qualification remains required before the 1.0 claim.

## Reconsideration triggers

Create another immutable amendment instead of editing this one if an accepted
Language 1.0 core identity changes again. Reconsider the manifest chain only if
a replacement provenance format preserves all prior exact identities and at
least the same malformed-input and routing guards.
