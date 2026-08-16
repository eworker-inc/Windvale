# Decision 0693: Echo package, approval, and Launch Record 3

- Date: 2026-08-16
- Status: Implemented with paired-host command evidence
- Contract: [Capability approval and launch records](../../Specifications/Windvale-Capability-Approval-And-Launch.md)
- Builds on: [Decision 0606](0606-First-Windvale-Echo-Application.md)

## Context

Decision 0606 produced the first real Shell 1 application but deliberately left
package identity, active-generation selection, approval, and launch composition
to a later increment. Calling the hosted application directly proves its
behavior, but it does not prove that an installed command resolves to those
bytes, that only its declared capabilities are bound, or that substitutions and
temporary native hosts fail closed.

Launch Record 2 cannot describe Echo honestly because it fixes one host-path
argument and a read-only file provider. Echo accepts a variable immutable text
vector and must receive no file or diagnostic provider.

## Decision

- Publish exact Package 1, Lock 1, provenance, and Bundle 1 records for
  `windvale.echo` version `0.1.0`. The bundle contains the existing exact
  813-byte WVB and has SHA-256
  `0502051930bddd016924e7858e0c32c0c481774edae9e755ca926f3cc3b3e966`.
- Approve exactly `console.write_line`, `process.argument`, and
  `process.argument_count`. Keep diagnostics, filesystem, environment, network,
  process launch, clock, and entropy absent or explicitly denied.
- Add Launch Record 3 for a direct target host with provider table 3. Bind
  standard LF line output and one immutable 0..67 argument snapshot, with 4,096
  strict-UTF-8 bytes per value and 65,536 bytes in aggregate.
- Add `echo` to an exact active Generation 1 and resolve it through the existing
  Windvale-written installation command resolver.
- Extend only the bounded Windows/Linux dispatcher profile. It verifies the
  package, approval, launch, WVB, host, capability table, and argument limits;
  copies the admitted host to private storage; starts it directly without a
  host command shell; and removes the private copy after every outcome.
- Register a focused ten-case paired-host owner for two executions plus bundle,
  host, approval, capability-binding, per-argument, count, aggregate-byte, and
  unknown-command rejection. This owner does not claim an interactive shell.

## Consequences

Echo is now the first Shell 1 catalog application with a complete portable
package-to-command launch closure. Its Approval 1 identity is
`386b8c983be8f4c633f27beb0d60b0d135ff3df88819a9c20262c1a8ce257790`.
The Windows and Linux Launch Record 3 identities are respectively
`493bac26e83edf995f87e31939a981fef7a1c021494bc23e154f61922dc2aa5b` and
`447df010898a98022a915c46d11c42c41a1099024e5fcbba3009735347459099`.

The dispatcher remains a development adapter receiving already acquired object
paths. The package manager, durable object lookup, user approval UI, terminal,
shell loop, structured completion, cancellation, browser worker, and Windvale OS
clean-spawn provider remain separate work.

## Reconsideration triggers

Version the record rather than widening it in place when a command needs input
or diagnostic streams, mutable or filesystem authority, environment values,
resource-domain and cancellation bindings, installed-object discovery, or a
Windvale OS kernel admission plan.
