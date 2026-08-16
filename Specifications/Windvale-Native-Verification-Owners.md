# Windvale native verification owners

## Status and purpose

This is the current native verification registry. It is not a .NET-retirement
milestone and it is not evidence that every owner ran on every commit.

An owner is one named, independently runnable check for a maintained boundary.
Changed-file development verification selects only the affected owners.
Explicit qualification composes all owners into four cold shards on Windows and
Debian. The same owner commands are reused so development and qualification do
not create parallel test implementations.

The historical retirement claim remains frozen at the immutable `v0.1.0` tag
and in [the retirement archive](Windvale-Native-Retirement-Test-Suite.md).

## Registry identity and grammar

`Tests/Native/Verification-Owners.txt` is 11,622 LF-only bytes with SHA-256
`cce1b77180a7e2729589e143a1ecfa15bb4ac6b8fea6fef033bd7ce3bddf747d`.
Its first line is exactly:

```text
windvale-native-verification-owners 1
```

Every remaining line has five pipe-separated fields:

```text
owner-name|command-stem|case-count|qualification-shard|expected-summary
```

The digest fixes owner order, commands, declared case counts, qualification
allocation, and accepted terminal summaries. Each command stem resolves under
`Tools/Native` to matching Windows `.cmd` and Linux `.sh` commands.

The current registry contains exactly 92 owners and 4,336 declared cases:

| Qualification shard | Owners | Cases |
| ---: | ---: | ---: |
| 1 | 1 | 32 |
| 2 | 25 | 1,771 |
| 3 | 29 | 1,363 |
| 4 | 37 | 1,170 |

The manifest is the canonical detailed inventory. Documentation must not copy
its entire evolving table because duplicated inventories become stale.

## Invocation modes

`Tools/Native/Test-Verification-Owners.cmd` and `.sh` support:

- `--filter <owner-name>` for one exact development owner;
- `--shard <1-4>` for one explicit qualification shard; and
- no arguments for a deliberate complete local qualification run.

Ordinary development must use the changed-file planner or an exact filter. A
commit or push is not a reason to run all owners. Complete and sharded runs are
reserved for release candidates, promotions, security boundaries, or another
named qualification need.

The historical `Test-Retirement-Suite.cmd` and `.sh` paths are compatibility
entry points that delegate to the current coordinator. New automation and
documentation use the verification-owner name.

## Coordinator contract

Before running a selected owner, the coordinator must verify the complete
registry digest and validate every command and shard entry. For each selected
owner it must:

1. emit bounded progress before invoking the child;
2. invoke exactly the registered host command;
3. require exit code `0` and empty standard error;
4. require the last nonempty output line to equal the registered summary;
5. count cases only from the reviewed registry; and
6. report owner and total elapsed time outside the semantic child summary.

The first child failure stops that coordinator process and exposes the captured
child output. Invalid arguments and unknown filters return `64`. GitHub runs all
four qualification shards independently with matrix fail-fast disabled, then an
aggregate gate requires both host matrices and the independent WebAssembly and
compiler-convergence jobs.

## Boundary

An owner result is focused development evidence. A complete paired-host run is
qualification evidence for one exact source state. Neither result changes
language semantics, grants release approval, or revives managed Stage 0 as a
live dependency.
