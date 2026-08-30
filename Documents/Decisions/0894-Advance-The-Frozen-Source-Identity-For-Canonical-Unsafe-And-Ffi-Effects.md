# Decision 0894: advance the frozen source identity for canonical unsafe and FFI effects

## Status

Proposed provenance-repair checkpoint on 2026-08-30. The exact replacement
manifest and local deterministic checks must pass before acceptance. Paired
Windows/Linux development evidence remains part of Decision 0893's acceptance
gate and must not be inferred from this source-identity repair.

## Context

[Decision 0870](0870-Enforce-Awaited-Provider-Calls-And-Recovery.md) binds the
current Language 1.0 verifier to an immutable 251-input amendment manifest.
Commit `3783a05b783d60237afa8c8c64e68bcac95e8b7c` subsequently reconciled the
language specification with the already selected System/FFI design by adding
the canonical `unsafe.address` and `ffi.call` language-effect identities. That
normative file grew from 64,778 to 64,992 bytes, but the verifier and fixture
inventory still named the prior exact identity.

The mismatch is not a compiler-behavior failure. The preceding main commit and
the Decision 0893 candidate both reached the same exact guard on Windows and
Linux: `Frozen core input differs: Specifications/Windvale-Language-1.0.md`.
Weakening that guard or rewriting the Decision 0870 manifest would destroy the
provenance chain. Retaining an intentionally stale identity would instead keep
main's focused Language 1.0 verification deterministically red.

## Decision

1. Preserve the Decision 0870 manifest and every earlier amendment manifest as
   immutable provenance.
2. Publish
   `Windvale-Language-1.0-Source-Amendment-0894-Candidate.txt` as amendment
   candidate v8. It derives from the exact Decision 0870 manifest and changes
   only `Specifications/Windvale-Language-1.0.md`.
3. Bind the replacement manifest to these exact identities:

   | Evidence | Exact value |
   | --- | ---: |
   | Manifest bytes | 3,704 |
   | Manifest SHA-256 | `47d2f8adbf2cf3f7fde7a91b368bb614cfceb0a8096379239847143aa5ebe5fc` |
   | Frozen inputs | 251 |
   | Frozen input bytes | 1,759,688 |
   | Entry-stream bytes | 46,260 |
   | Entry-stream SHA-256 | `d5976bddf9637b7559117ceff76087cf6554e1d0b9509a57b70a5b58929fea6e` |
   | Changed specification bytes | 64,992 |
   | Changed specification SHA-256 | `77e4da73996ce78efcbdc0e8da7b51564404613acfb4012b641faadcbc6ffc35` |

4. Advance the executable verifier and fixture inventory to the v8 manifest.
   Keep the 251-input membership, 46,260-byte entry-stream size, 482 front-door
   cases, and native-owner registry counts unchanged.
5. Route every retained post-0815 amendment manifest, including v8, through the
   same development classification, native front-door owner, and planner
   self-test. A new amendment path must not fall through to an unrelated broad
   classification or appear as an uncovered native path.
6. This checkpoint changes no source spelling beyond the already present
   specification bytes. `unsafe.address` and `ffi.call` remain explicit effects,
   grant no capability by themselves, and do not authorize a foreign call.

## Required evidence

- `Verify-Language-1.0-Migration-Fixtures.mjs` recomputes the exact manifest,
  each core input, the unchanged corpus selections, and the replacement
  aggregate identity.
- `Verify-Verification-Plan.ps1` proves classification and native-owner routing
  for the complete amendment chain.
- The focused `language-1-front-door` owner passes before this decision is
  accepted.
- The same commit then proceeds through Decision 0893's paired Windows/Linux
  development gate; that result is recorded separately and is required before
  claiming cross-host evidence.

## Consequences

- The stale hash cannot hide a real future semantic-specification change.
- Prior manifests remain independently inspectable rather than being rewritten
  to make current files appear historically unchanged.
- The canonical unsafe and FFI effects are part of the effective Language 1.0
  frozen source identity, while compiler binding and native foreign execution
  remain unfinished Slice 8 work.
- The repair adds no compiler, runtime, linker, ABI, or capability behavior.

## Reconsideration triggers

Create another immutable amendment rather than editing this one if an accepted
Language 1.0 core identity changes again. Reconsider the fixed manifest chain
only if a replacement provenance format preserves every prior exact identity,
selection rule, and malformed-input guard with at least equivalent evidence.
