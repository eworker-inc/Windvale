# Decision 0698: Metadata-aware WVB publisher source candidate

- Date: 2026-08-16
- Status: Implemented source-candidate checkpoint with local Windows and Debian WSL2 evidence
- Advances: [Decision 0593](0593-Metadata-Aware-Wvb-Inspector-Reconstruction.md)
- Contract: [native WVB publisher](../../Specifications/Windvale-Native-Wvb-Publisher.md)

## Context

The shared verifier adapter already validates and normalizes independent WVB
module metadata before applying the compiler-aligned semantic verifier. The
general WVB publisher still imported the absent-form semantic verifier directly.
Its promoted applications therefore rejected a valid metadata-bearing package
candidate even after source compilation, inspection, and native lowering had
advanced.

The current publisher source had also moved beyond the retained variant-2
construction identity. Treating that drift as only a hash update would hide the
larger contract: publisher construction records admit exact WVB, WVO, fragment,
layout, and final application geometry.

## Decision

- Route the WVB publisher's immutable candidate snapshot through
  `Compilerˉwvbˉverifyˉmetadata`, the existing shared metadata-aware adapter.
- Add the adapter and metadata normalizer to the publisher project closure. Do
  not add another metadata parser or change publication transaction behavior.
- Record the exact current WVB, WVO, and linked-fragment candidate identities on
  both hosts.
- Keep retained construction variant 2 and the ordinary front-door publisher
  unchanged. The focused owner must test current source-candidate construction
  separately from retained application reconstruction and execution.
- Defer the construction-record, admission, promoter, paired-application, and
  front-door refresh to one coherent artifact slice. Do not migrate Echo's
  source header until that ordinary publisher path accepts replacement metadata.

## Evidence

Windows and Debian produce the same 181,772-byte WVB at SHA-256
`c90f5325ea409d0710254812e1d434cce712de68385dec74d23eef5a475cf3c4`,
the same 1,523,708-byte WVO at SHA-256
`c1ce50f68e12dc94e56fa848c6f09f707ad117294af5e19f15659b7901c0bf35`,
and the same 1,520,746-byte linked fragment at SHA-256
`98aba65ccfdb0455f9fcb78ad3ffa0ecbe7aa942fcbf9064d179018dec12178a`.
`Main` remains at offset zero.

The complete 15-case `hosted-verifier-publisher-files` owner passes in 165,880
milliseconds on Windows and 189,000 milliseconds on Debian WSL2. It proves the
current source identities and separately reconstructs and executes the retained
publisher pipeline. These are local operational measurements, not release
qualification or a promotion claim.

## Consequences

- Publisher source now shares the same independent-metadata validation contract
  as the compiler-verifier adapter instead of bypassing it.
- The previously stale focused owner is green without relabeling the retained
  publisher artifacts as current.
- Exact package sizes and digests will change during promotion, but they are
  consequences of the semantic and construction update, not the underlying
  language issue.
- Echo remains on the legacy source header until the refreshed publisher is
  constructed, admitted, promoted, and exercised on both hosts.

## Reconsideration triggers

Reconsider the split checkpoint when variant 2 is refreshed, when publisher
verification must preserve metadata rather than normalize only for semantic
admission, or when a new module-metadata version requires a new shared adapter.
