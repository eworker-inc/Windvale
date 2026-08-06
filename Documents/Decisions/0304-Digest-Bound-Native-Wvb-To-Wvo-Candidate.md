# Decision 0304: Digest-bound native WVB-to-WVO candidate

- Date: 2026-08-06
- Status: Implemented accepted-subset candidate; complete backend, native application construction, atomic publication, grouped dual-host qualification, and promotion pending
- Advances: [Decision 0224](0224-First-Native-Wvb-To-Wvo-Front-Door.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native WVB-to-WVO application](../../Specifications/Windvale-Native-Wvb-To-Wvo.md)

## Context

Decision 0224 packages Windvale's accepted-subset x64 lowerer, and subsequent
decisions substantially expand its bounded scalar, descriptor, nominal,
capability, and staged-output coverage. The loose artifact directory still held
an obsolete early prototype rather than the current 383-function tool, so it
could not serve as a digest-bound front door.

The current tool project now builds exactly through the qualified native
source-to-WVB path. Stage 0 still constructs its outer hosted PE/ELF
applications because native construction of those tool profiles is a distinct
open retirement item. The lowerer itself can nevertheless own generation of a
fixed WVO vector and candidate execution without a managed runtime.

## Decision

- Rebuild the complete current tool WVB through `Build-Wvb.cmd` / `.sh` into a
  clean candidate directory. Construct its exact Windows and Linux hosted
  applications through the explicit Stage 0 recovery writer.
- Build the fixed return-42 input WVB through the qualified native source front
  door, then invoke the native lowerer to create the canonical WVO. Pin both
  vector artifacts beside the tool.
- Record all five identities plus separate native-WVB and Stage 0 application
  construction ownership in one candidate manifest.
- Add digest-bound `Lower-Wvb-To-Wvo.cmd` and `.sh` launchers. They accept
  exactly one `.wvb` input and one `.wvo` output, check the current-host tool
  digest, and invoke the Windvale lowerer.
- Keep this route explicitly accepted-subset. Unsupported modules continue to
  fail closed and use the complete Stage 0 backend only through a named
  recovery/differential route; the launcher must not silently fall back.
- Test only the added boundary: manifest and cross-host digest pins, byte-exact
  fixed-vector reproduction, independent WVO admission, and extension
  rejection. The existing lowerer test remains the owner of the broad retained
  corpus, package reconstruction, deterministic repetition, malformed-input
  preservation, and no-CLR process evidence.

## Evidence and consequences

- The current tool WVB is 372,514 bytes at SHA-256
  `f2283d33fdcae404a6dd15f6a888c3d1efa359328110fca6d54be1aa67cc1d5c`.
- The Windows application is 5,348,864 bytes at SHA-256
  `0e0d0c87f82f6576b11f888cfa26469f86f157064ea605a4bb188bcee5e3b280`.
- The Linux application is 5,349,376 bytes at SHA-256
  `c6ba202ffcb32a261bfd9c997e4bab754ab5a636e2d0b95e5de5f55e598c6358`.
- The fixed input WVB is 174 bytes at SHA-256
  `7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31`;
  its native-produced 479-byte WVO is SHA-256
  `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5`.
- Native tool-WVB construction took 18.9 seconds. Paired Stage 0 application
  construction, native fixture-WVB construction, and native fixed-WVO
  production took 11.7 seconds and reproduced every pinned identity.
- The reviewed focused compiler selection passes 1/1 in 0.380 test seconds
  after a 9.41-second zero-warning Release build; the complete command takes
  14.3 seconds.
- No backend semantics, WebAssembly implementation, or source-language
  behavior changed. Development, Standard, Qualification, complete backend
  transfer, native hosted-application construction, atomic WVO publication,
  and ordinary-path promotion remain deferred.

## Reconsideration triggers

Regenerate the inventory when the accepted subset, ABI, WVB/WVO contract,
native backend, hosted profile, startup, service bundle, or application writer
changes. Replace the whole-value output boundary with the already-designed
staged native publication chain before promoting workloads that exceed the
ordinary value limit; do not widen that limit as a shortcut.
