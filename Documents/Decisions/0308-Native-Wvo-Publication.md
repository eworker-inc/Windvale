# Decision 0308: Native WVO publication

- Date: 2026-08-06
- Status: Implemented current-host candidate; grouped Windows/Linux qualification pending
- Advances: [Decision 0214](0214-Exact-Native-Wvb-Publication-Step.md), [Decision 0222](0222-First-Native-Wvo-Read-Only-Front-Door.md), [Decision 0301](0301-Digest-Bound-Native-Wvo-Candidate-Launchers.md), and [Decision 0304](0304-Digest-Bound-Native-Wvb-To-Wvo-Candidate.md)
- Contract: [Native WVO publisher](../../Specifications/Windvale-Native-Wvo-Publisher.md)

## Context

The accepted-subset lowerer already verifies and constructs canonical WVO, but
its raw hosted `file.write_bytes` capability writes the requested path directly.
Decision 0305 therefore proved deterministic lowering and execution without
proving atomic durable whole-object publication. Keeping that gap would leave a
normal lowerer replacement path dependent on a managed wrapper.

The qualified native WVB publisher already owns the required cross-host mutation
mechanics. WVO admission also already existed in the native inspector, but it was
embedded in the large report-oriented module rather than exposed as one focused
portable dependency.

## Decision

- Extract the complete WVO reader and validator into
  `Wvo-Object-Verification.wv`. Keep inspection, reporting, SHA-256, and the
  command shell in `Wvo-Object-Core.wv`; do not duplicate format logic.
- Keep strict bounded UTF-8 validation inside the portable verification module
  so both inspection and publication use the same exact WVO contract without
  adding a sixth hosted service.
- Add a focused hosted Windvale publisher that accepts distinct `.wvo` paths and
  validates one complete candidate before mutation begins.
- Reuse the existing publication transaction, startup, Windows/Linux adapters,
  SHA-256 object, runtime services, import layout, and outer container builders.
  Generalize only the module identity, exact service directory, private bridge
  names, and metadata magic.
- Give the profile distinct `WVPO 1` metadata and public construction targets.
  Preserve the exact existing WVB and console-application publisher outputs.
- Repin the inspector after the cohesive source extraction, and pin the new WVO
  publisher WVB plus both paired applications with explicit Stage 0 provenance.
- Add digest-bound `Publish-Wvo.cmd` and `.sh` launchers. Change the accepted-
  subset lowerer launchers to write a private candidate and publish it through
  this boundary, cleaning only their named temporary file and empty directory.
- Keep hard-link, concurrency, and injected transaction-fault ownership in the
  existing shared WVB publisher tests. Focus this slice on WVO admission, exact
  packages, successful atomic replacement, invalid-candidate preservation,
  scratch cleanup, read-only inspector compatibility, and AOT composition.

## Evidence and consequences

- The repinned inspector WVB is 60,974 bytes at SHA-256
  `b0d0568cb6861c84ea9cad0b77f9722a9141b30c94952e5662aaa3afc47eae0f`.
  The Windows inspector is 606,720 bytes at SHA-256
  `2a8f6f8ca8fc6054fff23441f7971c0b90900383d5bed0fecc54f9cac102a300`;
  the Linux inspector is 606,208 bytes at SHA-256
  `bdc4817c252ecf2592299a6646161b396bfb251acabc68d3f5d75ff40891541e`.
- The publisher WVB is 41,365 bytes at SHA-256
  `4e8c81da38f5eb06f9334c2d2c5e35120a13e73bac3a9375b5e6a2eff04438c5`.
  The Windows application is 430,080 bytes at SHA-256
  `035a1baaada6be8d057b782804a8650d978da53dd008337ab00258f2ab597cb7`;
  the Linux application is 426,949 bytes at SHA-256
  `ac2bb513e2145e9eb911a9be142fc2f1f990a1bab21f278dd841043042b51f7a`.
- The focused WVO-publication test passes 1/1 in 2.943 test seconds after a
  13.23-second zero-warning build; the complete command takes 20.7 seconds. It
  constructs both containers, performs real current-host replacement, checks
  independent C# object admission as recovery evidence, proves invalid-candidate
  destination preservation and scratch cleanup, and observes no
  CLR/hostfxr/hostpolicy module.
- The repinned native verifier/inspector test passes 1/1 in 4.236 test seconds
  after a 7.82-second zero-warning build. The digest-bound launcher test passes
  1/1 in 0.671 test seconds after a 6.95-second zero-warning build.
- The native AOT-chain launcher passes in 1.7 seconds and reports
  `native aot chain status=Passed result=42`; it now exercises private WVO
  candidate construction and native atomic publication.
- No WebAssembly implementation or compiler semantics changed. Native
  host-container construction, Linux execution, grouped qualification,
  promotion, broader backend completion, and release integration remain.

## Reconsideration triggers

Use segmented WVO publication only when a complete native lowerer must exceed
the ordinary 4 MiB byte-value limit. Reuse the already-specified staged-WVO
manifest and snapshot design rather than weakening the single-snapshot `WVPO 1`
contract or introducing another object parser.
