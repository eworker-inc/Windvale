# Decision 0576: Verified active-command dispatch

- Status: Implemented and paired-host verified in GitHub run `31904886608`
- Date: 2026-08-15
- Advances: Milestone 4 and Decision 0568
- Depends on: Decision 0574
- Contract: [Generation 1 and Activation 1](../../Specifications/Windvale-Installation-Generation.md)

## Context

The Windvale-written resolver selects the package, part, approval, and launch
identities for an active command, but selection deliberately grants no authority
and creates no process. The first useful offline lifecycle needs both real
Generation 1 commands to execute without trusting caller-selected policy files
or an unverified host executable.

The existing launch records already define two narrow profiles: `wvdump` reads
one explicit file, while `wvquery` receives one fixed read-only database object
and one unsigned key. The dispatcher can bind these profiles without designing
a general ambient launcher.

## Decision

Add a bounded Windows/Linux host dispatcher that:

- reopens the public activation and digest-named immutable generation;
- consumes only an exact success report from the Windvale resolver;
- hashes the selected package bundle, approval, launch record, and host image;
- verifies their package, version, lock, executable, target, and policy closure;
- validates the command-specific argument profile; and
- copies the verified host bytes into private storage, flushes and rereads that
  copy, and invokes it directly without a command shell.

The focused owner constructs the exact two bundles and the target-specific WVDB
host, publishes Generation 1 and Activation 1 through their durable adapters,
executes both `wvdump` and `wvquery`, and rejects bundle, approval, launch, host,
argument, command, and invocation substitutions.

## Consequences

- Active selection and host execution now have distinct, testable failure
  boundaries.
- A caller cannot redirect an admitted command to a different bundle, approval,
  launch profile, target, or executable merely by supplying another path.
- The two initial commands retain their existing rights-reduced providers.
- This is a development host adapter, not yet the installed `wv run` client.
- Revocation, durable object-path lookup, provider unavailability after binding,
  and arbitrary future launch profiles remain explicit later work.

## Reconsideration triggers

Reconsider when installed object lookup replaces caller-supplied paths, a typed
revocation snapshot is available, provider construction becomes dynamic, or a
third command requires a new argument/provider profile.
