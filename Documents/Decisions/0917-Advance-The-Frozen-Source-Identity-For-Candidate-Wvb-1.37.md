# Decision 0917: advance the frozen source identity for candidate WVB 1.37

## Status

Accepted on 2026-09-02 and implemented by the Language 1.0 source-amendment
candidate named below. This decision repairs exact source-design provenance; it
does not admit WVB 1.37 to verification or execution and does not claim Linux
qualification.

## Context

[Decision 0915](0915-Advance-The-Frozen-Source-Identity-For-Unsafe-Memory-Progress.md)
bound the accepted Language 1.0 source design through typed write-pointer WVIR.
[Decision 0916](0916-Represent-Contained-Write-Pointer-Derivation-In-Candidate-Wvb-1.37.md)
then made that typed operation serializable while deliberately keeping the
complete verifier and all execution consumers closed.

That accepted checkpoint changes two frozen explanatory inputs:
`Specifications/Windvale-Language-1.0-Foundation.md` and
`Documents/Project/Windvale-Language-1.0-Migration.md`. The previous manifests
must remain immutable, and the front-door verifier must continue to fail on an
unrecorded source identity.

## Decision

1. Accept
   `Documents/Project/Windvale-Language-1.0-Source-Amendment-0917-Candidate.txt`
   as amendment version 11 over the immutable Decision-0915 manifest.
2. Bind the current exact bytes of all 13 core design inputs. Relative to the
   version-10 base, only the Foundation specification and migration record
   change.
3. Preserve the 16 accepted source-design decisions and both frozen paper
   corpora byte-for-byte. The complete candidate remains 251 identity inputs.
4. Bind the replacement identity to these exact values:

   | Evidence | Exact value |
   | --- | ---: |
   | Manifest bytes | 3,780 |
   | Manifest SHA-256 | `b468c7b2f39e1df2e5e644906f5a8db08d3e8eac15598df50eea78f40814dcab` |
   | Frozen inputs | 251 |
   | Frozen input bytes | 1,775,892 |
   | Entry-stream bytes | 46,260 |
   | Entry-stream SHA-256 | `0d5e6ef4d9bef7c7d2686905a79ff01490a65302ab3b99efcdd5652aef9b60d0` |
   | Foundation bytes | 85,696 |
   | Foundation SHA-256 | `97fe00232092e1de12d9dac4f7d8385ae3c41cf9079a9b8617d1f2ad7e1f6fb7` |
   | Migration bytes | 62,273 |
   | Migration SHA-256 | `d23ba0259d76ce58207d0d40bb8b2517ccd84adfd3a2bbffd4e29e584f01c526` |

5. Rebind the migration-fixture verifier and fixture inventory to version 11.
6. Route the new amendment through the same development classification,
   Language 1.0 front-door owner, and verification-planner self-test as every
   retained amendment.

## Consequences

- The frozen source identity matches the accepted candidate WVB 1.37
  description and fails closed on a future unrecorded change.
- Prior manifests continue to identify their historical source states.
- This provenance amendment adds no source syntax, WVIR operation, WVB opcode,
  runtime service, native ABI, capability, or compatibility behavior beyond the
  separately accepted Decision 0916 contract.
- Candidate WVB 1.37 remains non-executable. Pointer lifetime verification,
  provider and native address formation, and authenticated Foreign calls remain
  future work.
- Windows-local development evidence is recorded separately; paired Windows
  and Linux qualification remains required before the 1.0 claim.

## Reconsideration triggers

Create another immutable amendment instead of editing this one if an accepted
Language 1.0 core identity changes again. Reconsider the manifest chain only if
a replacement provenance format preserves all prior exact identities and at
least the same malformed-input and routing guards.
