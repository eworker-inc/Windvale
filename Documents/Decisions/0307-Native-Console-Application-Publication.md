# Decision 0307: Native console-application publication

- Date: 2026-08-06
- Status: Implemented current-host candidate; grouped Windows/Linux qualification pending
- Advances: [Decision 0214](0214-Exact-Native-Wvb-Publication-Step.md), [Decision 0223](0223-First-Native-Console-Application-Packager.md), and [Decision 0303](0303-Digest-Bound-Native-Console-Packager-Candidate.md)
- Contract: [Native console-application publisher](../../Specifications/Windvale-Native-Console-Application-Publisher.md)

## Context

The native console packager already validates and materializes the canonical
version-1 PE/ELF recipe, but its raw hosted `file.write_bytes` capability writes
the requested path directly. Decision 0305 therefore proved deterministic
packaging and execution without proving atomic durable publication. Keeping that
gap would leave the ordinary replacement path dependent on a managed wrapper.

The qualified native WVB publisher already owns the required cross-host mutation
mechanics. Those mechanics are format-neutral after a portable Windvale entry
point admits an immutable candidate, so a second platform implementation is not
justified.

## Decision

- Add a focused hosted Windvale publisher that accepts same-kind `.exe` or `.elf`
  paths, rejects textual equality, and validates the candidate through the
  existing portable console-application verifier before mutation begins.
- Reuse the existing publication transaction, startup, Windows/Linux adapters,
  SHA-256 object, runtime services, import layout, and outer container builders.
  Generalize only the module identity, private bridge names, and metadata magic.
- Give the profile distinct `WVPA 1` metadata and distinct public construction
  targets. Preserve the exact existing `WVPB 1` WVB publisher outputs.
- Pin the 56,375-byte WVB and both paired applications in an exact candidate
  manifest. Record Stage 0 construction because qualified native source building
  reaches the documented `Sourceˉbindings` accepted-subset boundary.
- Add digest-bound `Publish-Console.cmd` and `.sh` launchers. Change the native
  console-packager launchers to write a private candidate and publish it through
  this boundary, cleaning only their named temporary file and empty directory.
- Keep hard-link, concurrency, and injected transaction-fault ownership in the
  existing shared WVB publisher tests. The new focused test owns application
  admission, exact packages, successful replacement/execution, invalid-candidate
  preservation, cleanup, and no-CLR evidence.

## Evidence and consequences

- The publisher WVB is 56,375 bytes at SHA-256
  `1e35f7cc9e53322ebcc70c332486eef983ff59370246c62ec4e8cbcd144d8403`.
  The Windows application is 642,048 bytes at SHA-256
  `1bd3bbd24fc22940b96badb7e809899d42e42a25a5247dfededb00048232675d`;
  the Linux application is 639,941 bytes at SHA-256
  `2edc7ebe23660e299d9db4bf55d4537ec102b7a3b2d46ba833e549cd355a0af7`.
- Direct Windows execution atomically publishes the fixed return-42 PE, reports
  its exact 2,560-byte identity, and the published application returns 42.
- The reviewed focused linker test passes 1/1 in 3.261 test seconds after a
  9.54-second zero-warning build; the complete command takes 17.2 seconds.
  It constructs both containers, performs real current-host replacement, proves
  invalid-candidate destination preservation and scratch cleanup, and observes no
  CLR/hostfxr/hostpolicy module.
- The updated digest-bound AOT-chain launcher still passes in 1.4 seconds and
  reports `native aot chain status=Passed result=42`.
- The console packager remains Stage 0-constructed and its raw application keeps
  its documented non-atomic capability contract. The digest-bound launcher now
  supplies native atomic publication. Native host-container construction, Linux
  execution, grouped qualification, promotion, and release integration remain.
- No WebAssembly implementation or compiler semantics changed.

## Reconsideration triggers

Replace the shared adapter family only if console-application publication needs a
different transaction or authority contract. Add segmented candidates only when
a native bounded multi-snapshot admission design covers the verifier's theoretical
maximum without silently widening the 4 MiB byte-value limit.
