# Decision 0602: Shell 1 parser contract and first portable core

- Date: 2026-08-15
- Status: Implemented portable parser with paired native and bounded browser evidence
- Contract: [Windvale Shell 1](../../Specifications/Windvale-Shell-1.md)
- Architecture: [Windvale shell](../Architecture/Windvale-Shell.md)
- Readiness: [shell implementation readiness](../Project/Windvale-Shell-Implementation-Readiness.md)
- Builds on: [Decision 0191](0191-Windvale-Console-Shell-And-Cli-Architecture.md)

## Context

Windvale needs one recognizable command environment across Windows, Linux,
Windvale OS, and the browser Workbench without making PowerShell, POSIX shell,
JavaScript, or a host process launcher the semantic definition. The permanent
terminal, command-resolution, clean-launch, standard-byte-stream, and Windvale
OS service contracts are not yet complete, but command parsing itself requires
no authority and can become useful, testable portable code now.

Leaving syntax implicit would let each host prototype drift. Allocating a host
array of decoded words would also hide ownership behind the bootstrap adapter
and prevent the parser from being ordinary Windvale source.

## Decision

- Accept Windvale Shell 1 as the first parser and command contract.
- Limit one submitted strict-UTF-8 line to 4,096 bytes and 68 words: one command
  plus at most 67 immutable application arguments.
- Require an unquoted lowercase ASCII one-token command matching
  `[a-z][a-z0-9]*(-[a-z0-9]+)*`. Quoted words remain arguments only; a quoted
  command returns `WVSH1010` at its opening quote.
- Accept the exact single-quote, double-quote escape, reserved-operator,
  non-concatenation, control-character, diagnostic, and byte-offset rules in the
  specification.
- Keep `cat` as the only required Shell 1 alias and canonicalize it once to
  `file-read` before resolver access.
- Implement the parser as a capability-free portable library. One immutable
  scan result records status, word count, and failure offset; indexed word views
  rescan the bounded input; explicit materialization removes delimiters and
  decodes the five double-quote escapes. No global table, callback-owned array,
  ambient host state, or exception is part of the result.
- Register one focused native verification owner covering valid, boundary,
  malformed, deterministic, command, alias, quoting, UTF-8, and word-view cases.
- Do not present this parser slice as an interactive shell, a Windvale OS shell,
  or complete browser conformance. Those claims wait for their named providers
  and the complete same-corpus execution evidence.

## Consequences

Shell syntax can now be built and tested once in Windvale source while terminal,
resolver, launcher, and stream work proceeds independently. The bounded rescan
representation trades a small amount of repeated work for simple immutable
ownership and avoids introducing a general collection solely for the shell.

The focused owner proves deterministic WVB/WVO construction, executes all 47
cases through the real local native adapter, and constructs both hosted target
images. The current slice passes that owner independently on Windows and Debian.
It also rebuilds one exact 27,088-byte portable WVB with SHA-256
`ffa2723513b4f3846beabbd89b7a4d67fb8bb7999ad79c7684e72756b1ea302f`
and executes 11 representative checks through the pinned WebAssembly
interpreter. Node 24 and Chromium 151 agree on result `42`, empty output, 81,619
guest instructions, and 80,257,283 outer instructions.

The browser smoke is bounded evidence over the same parser implementation; it
is not the specification's complete 47-case same-corpus browser-conformance
gate. `file-read`, `cat`, command launch, and the five built-ins are contracted
names and behavior, not implemented applications in this slice.

## Reconsideration triggers

Revisit the representation if Windvale gains a bounded immutable sequence whose
ownership is simpler than indexed rescanning; revisit the grammar only through a
new shell version when composition or expansion has accepted resource and
security contracts; and revisit the alias table when signed generation metadata
can own aliases without changing valid Shell 1 lines.
