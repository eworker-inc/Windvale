# Decision 0415: Managed hosted-tool AOT recovery lane

- Status: Implemented candidate; grouped promotion pending
- Date: 2026-08-08
- Advances: [Decision 0414](0414-Digest-Bound-Native-Hosted-Container-Composition.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

Decision 0414 establishes one digest-bound native path that packages a WVB as
a hosted format-2 Windows or Linux application. Its 19 Windvale tools and both
host launchers pass the focused cross-host composition check. The managed Seed
CLI nevertheless continued to accept all 38 paired tool-container targets
through ordinary `windvale compile` and `windvale aot`, obscuring whether they
were product or Stage 0 construction paths.

## Decision

Remove those target pairs from ordinary managed compilation. Ordinary
`windvale compile` requires WVB output for these sources, while ordinary
`windvale aot` rejects the tool-container targets with exit 64 and directs the
caller to the explicit `windvale recovery-aot` command. The recovery command
accepts only the bounded hosted-tool target set; it rejects unrelated AOT
targets rather than becoming a second general compiler command.

Keep the target classification in the focused
`Tools/Windvale.Tool/Stage0-Recovery-Aot-Targets.cs` source instead of adding a
large target block to the already broad CLI root. Update every affected managed
test to request the recovery command explicitly. The C# application writers
remain frozen recovery/differential evidence and are not deleted by this
decision.

## Evidence and consequences

The reviewed focused hosted-container segment-request test first proves that
ordinary `compile` and `aot` reject its selected host target as recovery-only,
then proves that `recovery-aot` constructs the exact pinned application and
completes the existing native process and front-door checks. It passes 1/1 in
5.859 seconds after a zero-warning build. No broad verifier ran under the
grouped end-of-goal verification policy.

The ordinary hosted packaging path is now the paired native launcher rather
than a managed tool-container constructor. Stage 0 remains available by an
explicitly named command for recovery releases, deterministic reconstruction,
and independent evidence. Grouped qualification must still pass before the
candidate is called `native-qualified`, and deletion remains bound to the final
Decision 0057 recovery archive.

## Reconsideration triggers

Change the recovery target set only when a named tool enters or leaves the
digest-bound hosted packaging inventory. Do not widen `recovery-aot` into a
parallel ordinary AOT surface or route native launchers through it.
