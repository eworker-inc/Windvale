# Decision 0606: First Windvale echo application

- Date: 2026-08-16
- Status: Implemented application with paired hosted evidence; generation resolution and shell launch remain pending
- Contract: [Windvale Shell 1](../../Specifications/Windvale-Shell-1.md)
- Architecture: [Windvale shell](../Architecture/Windvale-Shell.md)
- Readiness: [shell implementation readiness](../Project/Windvale-Shell-Implementation-Readiness.md)
- Builds on: [Decision 0602](0602-Shell-1-Parser-Contract-And-First-Portable-Core.md)

## Context

The accepted Shell 1 catalog starts with `echo`, but a catalog name is not an
application. Advancing from parsing toward launch requires one real Windvale
entry point whose argument ownership, output bytes, capability requirements,
native identities, and resource ceilings are visible before resolver or
dispatcher policy is expanded.

A host-language `echo` replacement would prove only a host adapter. Folding
resolution into the application would also erase the intended boundary between
the portable shell, immutable generation metadata, rights-limited launch, and
ordinary Windvale programs.

## Decision

- Implement `Applications/Shell/Echo.wv` as an ordinary hosted Windvale
  application with one exported `Main() -> i32` entry.
- Declare exactly `console.write_line`, `process.argument`, and
  `process.argument_count`. The application receives no filesystem,
  diagnostic, environment, native-process, or ambient path authority.
- Preserve every immutable launcher argument, including empty text and Unicode.
  Join arguments with one ASCII space and submit the complete result through one
  `console.write_line` call, producing exactly one final LF. Zero arguments
  therefore produce exactly one LF.
- Represent first-versus-following spacing as the nominal enum
  `Windvaleˉechoˉposition`. This is application state, not host parsing, and
  supplies real nominal metadata to the current hosted container pipeline.
- Inherit the hosted argument snapshot ceilings: at most 67 arguments, at most
  4,096 strict-UTF-8 bytes per argument, and at most 64 KiB in the complete
  snapshot. Invalid snapshots are rejected before `Main` executes.
- Register `echo-application` as a focused nine-case owner. It must rebuild the
  WVB twice, inspect the exact three-capability directory, construct both hosted
  targets, execute the current host application, test the two individual
  argument ceilings, and leave no private build directory.
- Do not add `echo` to a generation or widen the fixed dispatcher in this
  increment. Exact Windvale package, approval, launch, resolver, capability
  mismatch, identity-substitution, and dispatcher-cleanup evidence belong to the
  next composition increment.

## Consequences

The first Shell 1 external command now exists independently of the shell. Its
canonical WVB is 813 bytes with SHA-256
`5d827b98be518a07a8dea60d79e70073535f78f07cf875d750021fa795c13c64`.
The current hosted container pipeline produces an exact 22,016-byte Windows
application with SHA-256
`024cfac66fa760b705a48e72942103a79e24342d3e59886e9ccd127dfd3cdbcb`
and an exact 24,576-byte Linux application with SHA-256
`0e5a91887381adb23a84d745ce06902be99e53d70e58a598465939881638b576`.

The focused owner passes independently on Windows and Debian for empty, single,
multiword, empty-string, Unicode, 4,096-byte, 67-argument, 4,097-byte-rejected,
and 68-argument-rejected cases. The paired result proves the application and
current native adapters, not an interactive shell, browser command worker,
Windvale OS launch provider, signed package, or active-generation command.

The bootstrap hosted containers still carry their fixed service resource
bundle. The verified WVB capability directory remains the application contract
and contains only the three declared requirements; future container slimming
does not change `echo` semantics.

## Reconsideration triggers

Revisit construction if Windvale gains a bounded text builder that avoids
repeated concatenation while preserving one output operation. Revisit the
argument ceilings only by versioning the hosted resource contract and Shell 1
launch profile. Revisit the external name or output bytes only through a new
shell/application contract, not a host-specific alias or console convention.
