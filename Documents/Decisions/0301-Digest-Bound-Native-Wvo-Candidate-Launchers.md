# Decision 0301: Digest-bound native WVO candidate launchers

- Date: 2026-08-06
- Status: Implemented candidate; grouped dual-host qualification and promotion pending
- Advances: [Decision 0222](0222-First-Native-Wvo-Read-Only-Front-Door.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native WVO inspector](../../Specifications/Windvale-Native-Wvo-Inspector.md)

## Context

Decision 0222 moved complete WVO 1.0 verification and inspection into one
Windvale-owned read-only core and produced exact Windows and Linux native
applications. Those files remained loose candidate artifacts. Normal users had
no digest-bound launcher, so the inventory correctly retained promotion and
ordinary-path cutover as open work.

The active retirement goal defers broad Windows/Linux verification until the
remaining slices are ready. This decision can close the provenance and
launcher-construction gap now without treating a current-host focused result as
cross-host promotion evidence.

## Decision

- Add a candidate manifest that pins the exact canonical WVB, Windows PE, and
  Linux ELF identities, their target profiles, sizes, source relationship, and
  pending qualification status.
- Add `Verify-Wvo.cmd` / `.sh` and `Inspect-Wvo.cmd` / `.sh`. Each launcher
  checks the complete current-host application digest before executing it,
  accepts exactly one `.wvo` path, and invokes the Windvale tool's explicit
  `verify` or `inspect` command.
- Keep these launchers candidate entry points until their exact containing
  commit passes the grouped Windows/Linux gate. The existing Stage 0 object
  commands remain the ordinary recovery/differential route meanwhile.
- Test the new boundary with the existing canonical object vector. Check the
  manifest and both hosts' pinned digests, execute only the current-host
  launcher pair, require exact successful reports, and require deterministic
  rejection of a wrong extension. Do not regenerate the packages or rerun the
  broader native-package test for this unchanged source state.

## Evidence and consequences

- The canonical `Wvo-Object.wvb` is 57,297 bytes at SHA-256
  `3940e5aebb8dc25581080e5af3a73eb81eec5b7144c34fb2b7f4014e155b73a7`.
- The Windows application is 577,024 bytes at SHA-256
  `9f85375a9223fdc8c8bfe81f82b6b428432a21594a11179d1ab1375aa6c6886f`.
- The Linux application is 577,536 bytes at SHA-256
  `dc9fff2a13256cd0dfabed4c7e9369a9d446408a00aec3eee5fd95876ce88b37`.
- The reviewed focused object-model selection passes 1/1 in 0.654 test
  seconds after a 10.89-second zero-warning Release build; the complete command
  takes 16.3 seconds.
- The launcher test compares the current candidate against the retained
  structural oracle while it is available. After final retirement, fixed
  vectors, structural assertions, malformed outcomes, and pinned identities
  remain sufficient; a live C# result generator is not a permanent dependency.
- No native package, WVO contract, WebAssembly implementation, or source
  semantics changed. Development, Standard, Qualification, promotion, and
  ordinary-path cutover remain deferred to the grouped end-of-goal gate.

## Reconsideration triggers

Regenerate the manifest and launcher digests if either application changes.
Revisit the split command surface if a future read-only object tool gains
authority beyond the five capabilities pinned by Decision 0222. Never preserve
an old digest as an implicit compatibility path after its artifact is replaced.
