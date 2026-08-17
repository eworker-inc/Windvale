# Windvale scripting 1

## Status and scope

Scripting 1 is implemented for the installed Windows x64 and Linux x64
products under [Decision 0735](../Documents/Decisions/0735-Implement-The-First-Windvale-Scripting-Slice.md).
It is a convenience launch contract over ordinary Windvale source, canonical
WVB, the complete WVB verifier, and the bounded WVB runner. It is not a second
language, source mode, module format, or ambient-authority profile.

## Command contract

```text
wv run <source.wv> [argument ...]
```

The source path is the first token after `run`. Every later token is an
immutable script argument, including `--` and tokens beginning with `-`.
Scripting 1 therefore has no `--` separator and no per-capability `--allow`
options.

The command accepts exactly one ordinary `.wv` source file. That source must
define one explicit module and export `Main() -> i32`. Imports and project
manifests are outside Scripting 1.

## Mandatory launch sequence

For every invocation, the launcher:

1. creates one private temporary directory;
2. compiles the named source to canonical WVB with `wvbuild`;
3. admits that WVB with `wvverify`;
4. invokes the bounded runner only after successful admission; and
5. removes the temporary WVB and directory on success or failure.

Compilation and verifier reports are hidden on success. Diagnostics and
nonzero statuses remain visible on failure. Scripting 1 has no cache; adding
one later must not bypass step 3.

## Base authority

The launcher automatically grants only capabilities that the verified module
both declares and uses from this fixed base set:

```text
console.write_line(text) -> void
diagnostic.write_line(text) -> void
process.argument(u32) -> text
process.argument_count() -> u32
```

The two output capabilities retain separate standard-output and diagnostic
sinks. Each call appends one LF under the hosted-resource contract. The first
slice deliberately omits `console.write`, arbitrary byte output, standard
input, environment, clock, randomness, files, directories, network, process
launch, and system authority. A declaration outside the fixed set fails as an
unsupported script profile before guest execution; it is never silently
granted.

## Bounds and results

- At most 65 script arguments are accepted because the outer runner consumes
  two of the hosted boundary's 67 argument slots.
- Each argument is at most 4 KiB of strict host-provided UTF-8 and their total
  UTF-8 payload is at most 64 KiB.
- Standard output and diagnostic output are each bounded to 65,536 bytes.
- Guest execution is bounded to 1,000,000 instructions and call depth eight.
- Guest exit values from 0 through 255 become the host process status exactly.
  Any other `i32` result fails with `Invalidˉexitˉstatus` and host status 1.
- Invocation errors use status 64. Compilation, verification, unsupported
  authority, malformed execution responses, and runtime failures use a
  nonzero status and a diagnostic.

The paired `Test-Scripting` owner proves the command on each host with a
portable script, dash-prefixed and spaced arguments, separated output and
diagnostics, guest status 7, unsupported file authority, malformed source,
and hidden temporary WVB cleanup.
