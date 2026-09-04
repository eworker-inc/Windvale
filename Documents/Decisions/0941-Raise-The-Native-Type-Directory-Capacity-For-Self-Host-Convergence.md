# Decision 0941: raise the native type-directory capacity for self-host convergence

## Status

Accepted implementation checkpoint on 2026-09-03. The native x64 type reader
and the hosted enum-request producer admit at most 256 total type declarations
while retaining their narrower per-kind limits. The current Windows self-host
chain now completes through exact WVB-runner reconstruction. Independent Linux
reconstruction and final paired-host qualification remain pending.

## Context

The current self-hosted emitter produced a structurally valid 1,557,114-byte
WVB with 129 type declarations: 97 records and 32 enums. The native lowering
reader rejected it as `Unsupported_module` detail `105` because the older
aggregate bound was 128 even though neither per-kind limit was close to
exhaustion.

After the lowerer bound was raised, hosted packaging still rejected the same
module because the separately pinned `wvhostenumrequest` application embedded
the old reader. The source already imported the shared reader; its candidate
artifacts and digest pins had to be reconstructed rather than adding a second
limit or compatibility path.

## Decision

1. Raise only the aggregate native type-directory bound from 128 to 256.
2. Retain at most 116 records, 64 enums, 64 variants, and 128 callable
   descriptors, plus every existing item, name, payload, recursion, and WVB
   envelope bound.
3. Add one focused exact boundary fixture containing 116 records, 64 enums,
   and 76 callable descriptors. Admit its 256 entries and reject declared count
   257 before entry allocation.
4. Reuse that shared reader in the lowerer, staging producer, and hosted
   enum-request producer. Do not add a parallel parser or preserve the obsolete
   aggregate limit.
5. Reconstruct and repin the standalone enum-request candidate used by hosted
   packaging. Keep its profile, capabilities, four-MiB request bound, and
   enum-specific behavior unchanged.
6. Reconstruct the segmented WVO staging producer from the same source state,
   then require exact self-hosted compiler and WVB-runner reproduction.

## Implementation standing

The focused boundary builds an 80,451-byte WVB, lowers it to an 898,222-byte
WVO, links an 894,508-byte image at entry 38,522, and returns `42` through the
Profile-6 Windows native application. The WVB is SHA-256
`f2c8c147dca71fe7a97378973b1e335512caa047a073df28f783029b9c1e1400`.

The refreshed standalone enum-request identities are:

- WVB: 82,115 bytes, SHA-256
  `69a4ef3b33875e26f068e1545c60a0ae7bee60ac566869c05e55ad27c0aa9b36`;
- Windows x64 application: 888,832 bytes, SHA-256
  `adabe0902e164bcb68561796ef2d60d446399cd51e70e326daf366623365ced0`;
- Linux x64 application: 888,832 bytes, SHA-256
  `06f6b9fe4812ec9f1c4c37fd47ae3153ac9b870ffb9b4173b2705c6517c586f8`.

The refreshed segmented staging identities are:

- WVB: 774,524 bytes, SHA-256
  `427e7ee4424ecf7ff53a1a23eafd1e211873c15f666c46255685d364f4e5761f`;
- Windows x64 application: 11,184,128 bytes, SHA-256
  `f289d608d6545dfeece35dfd325bf0a62ef862aeae0b069b47157fb97652820e`;
- Linux x64 application: 11,186,176 bytes, SHA-256
  `cafd9627383fdbd681bdcc5906a6fe0aedcb423ba0b7f380b39f43e7fd5aa0b8`.

The refreshed WVB-to-WVO lowerer identities are:

- WVB: 747,242 bytes, SHA-256
  `7cc1867200d747c3b694f7bd35b3f9128dbb7bcc8223ebd46ead234a22680a3f`;
- Windows x64 application: 10,656,768 bytes, SHA-256
  `0a0894901341d71ef09712fb63ed0a9f7ac2b93c64b357d123dd09674045cfda`;
- Linux x64 application: 10,657,792 bytes, SHA-256
  `4f7aa0abdf870ada362defee6258ba4e6b8ce1f0f67329563d20ed3eb6c9ff24`.

The refreshed enum-request tool accepts the exact 129-type self-hosted emitter
and produces its 15,274-byte `WVEQ 2` request. The resulting emitter application
then produces the exact retained 1,040,878-byte WVB runner and its byte-identical
10,547,712-byte Windows application.

## Consequences

- Current compiler growth no longer fails merely because independent safe type
  categories sum past 128.
- Malformed or excessive type tables still fail before unbounded allocation,
  and no individual category becomes wider.
- The retained Seed WVB 1.11 recovery root is unchanged. Once paired
  qualification passes, the self-hosted compiler can replace Seed for normal
  development while Seed remains the immutable bootstrap and recovery anchor.
- Linux application bytes are reconstructed cross-target, but Linux execution
  and the final paired qualification claim remain open.

## Reconsideration triggers

Revisit this decision before admitting more than 256 total types, widening any
per-kind limit, changing nominal index encoding, allowing a hosted enum request
larger than four MiB, or claiming self-host acceptance without byte-identical
Windows and Linux reconstruction.
