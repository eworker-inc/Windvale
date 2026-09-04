# Decision 0942: advance the frozen source identity for Foreign and payload borrowing

## Status

Accepted on 2026-09-04 and implemented by the Language 1.0 source-amendment
candidate named below. This decision repairs exact source-design provenance; it
does not by itself complete Slice 8, make the payload-borrow checkpoint
executable, or claim paired-host qualification.

## Context

[Decision 0919](0919-Advance-The-Frozen-Source-Identity-For-WVB-1.37-Containment.md)
bound the accepted Language 1.0 design through compiler-aligned WVB 1.37
pointer containment. Subsequent accepted work documented authenticated WVB
1.38 Foreign publication and containment in the migration plan, and the
Foundation specification gained the direct-owner immutable Option/Result
payload-borrow checkpoint.

Those changes affect two inputs protected by the immutable source-freeze
chain: `Documents/Project/Windvale-Language-1.0-Migration.md` and
`Specifications/Windvale-Language-1.0-Foundation.md`. The prior manifest must
remain historical, while the active front-door fixture verifier must reject
any later unrecorded drift.

## Decision

1. Accept
   `Documents/Project/Windvale-Language-1.0-Source-Amendment-0942-Candidate.txt`
   as amendment version 13 over the immutable Decision-0919 manifest.
2. Bind the current exact bytes of all 13 core design inputs. Relative to the
   version-12 base, only the Foundation specification and migration plan
   change.
3. Preserve the 16 accepted source-design decisions and both frozen paper
   corpora byte-for-byte. The complete candidate remains 251 identity inputs.
4. Bind the replacement identity to these exact values:

   | Evidence | Exact value |
   | --- | ---: |
   | Manifest bytes | 3,792 |
   | Manifest SHA-256 | `ede1ccee0a91282f3e34bff520c0e67646d73183f671b9c005d84aab27c92a2f` |
   | Frozen inputs | 251 |
   | Frozen input bytes | 1,781,706 |
   | Entry-stream bytes | 46,260 |
   | Entry-stream SHA-256 | `d91a5e0e31bf895b920ee3e9682c466f4d768eb6e75d920969837f1679e1fe8b` |
   | Foundation bytes | 87,284 |
   | Foundation SHA-256 | `28915c949c34244d03f6a3c99967788ddff530855a100573c0441bc222f5d409` |
   | Migration bytes | 66,499 |
   | Migration SHA-256 | `84706301ad7a3aeb30d84d721863a5850e37f383d3c7f7beb887b4b977043a2d` |

5. Rebind the migration-fixture verifier and fixture inventory to version 13.
6. Route the new amendment through the same development classification,
   Language 1.0 front-door owner, and verification-planner self-test as every
   retained amendment.

## Consequences

- The active frozen source identity now matches the accepted authenticated
  Foreign and immutable payload-borrow descriptions and fails closed on later
  unrecorded changes.
- Prior manifests continue to identify their historical source states.
- This provenance amendment adds no syntax, WVIR operation, WVB opcode,
  runtime service, capability, compatibility behavior, or hidden fallback.
- Executable payload-borrow representation and final Slice 8 paired-host
  qualification remain separate implementation gates.

## Reconsideration triggers

Create another immutable amendment instead of editing this one if an accepted
Language 1.0 core identity changes again. Replace the manifest chain only if a
new provenance format preserves every prior exact identity and at least the
same malformed-input and routing guards.
