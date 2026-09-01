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

`Tests/Native/Verification-Owners.txt` is 22,307 LF-only bytes with SHA-256
`9ad8431958621480e99d9eda356268ed3aeb2964270916cdc5c7baeb9e2fadf5`.
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

The current registry contains exactly 125 owners and 5,936 declared cases:

| Qualification shard | Owners | Cases |
| ---: | ---: | ---: |
| 1 | 1 | 57 |
| 2 | 45 | 2,847 |
| 3 | 38 | 1,783 |
| 4 | 41 | 1,249 |

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

An owner may expose an explicit development mode when its qualification command
constructs evidence that is unnecessarily broad for an ordinary edit. The
changed-file dispatcher must select that mode explicitly, keep its inputs and
oracle in the development dependency registry, and leave the no-argument owner
command as the complete qualification contract. A development-mode pass must
not be reported as the qualification evidence it intentionally omits.

One coherent source state receives one final selected plan. A failure
invalidates that owner and owners whose declared inputs changed; it does not
invalidate unrelated passing owners. A future persistent result cache must key
each reusable pass by the complete input set, command, verifier version, tool
identities, host contract, and execution mode. Until that cache is implemented,
maintainers may resume the explicit owner list manually and must record which
owners passed on the unchanged source state.

Independent work may run concurrently only under a bounded resource policy.
Shared compiler reconstruction, cache publication, storage, and other
contention-heavy owners remain serialized unless measurement proves a safe
limit. An individual owner may use bounded internal concurrency for independent
products; `language-1-callable-semantics`, for example, packages at most two
distinct fixtures at once while preserving all registered cases. Parallelism
changes scheduling, not the accepted terminal summary or evidence boundary.

The historical `Test-Retirement-Suite.cmd` and `.sh` compatibility aliases were
removed from `main` because they added no coverage and only delegated to the
current coordinator. They remain available from the immutable `v0.1.0` tag and
Git history. Current automation and documentation use the verification-owner
name.

## Coordinator contract

Before running a selected owner, the coordinator must verify the complete
registry digest and validate every command and shard entry. For each selected
owner it must:

1. emit bounded progress before invoking the child;
2. invoke exactly the registered host command;
3. stream child output live while retaining at most 8 MiB separately for each
   output channel;
4. after 30 seconds without complete-line child activity, emit an external
   heartbeat, capped at 240 lines and excluded from the retained child log;
5. require exit code `0` and empty standard error;
6. require the last nonempty output line to equal the registered summary;
7. count cases only from the reviewed registry; and
8. report owner and total elapsed time outside the semantic child summary.

Owner-log parents are validated component by component with filesystem
metadata. Symbolic links, junctions, and non-directory components reject;
different legitimate spellings of the same Windows directory, including an
NTFS 8.3 ancestor alias, do not constitute link evidence by themselves. The
registered `verification-owner-stream` owner keeps this boundary executable.

The first child failure stops that coordinator process after its output has
already been exposed live. Invalid arguments and unknown filters return `64`. GitHub runs all
four qualification shards independently with matrix fail-fast disabled, then an
aggregate gate requires both host matrices and the independent WebAssembly and
compiler-convergence jobs.

## Boundary

An owner result is focused development evidence. A complete paired-host run is
qualification evidence for one exact source state. Neither result changes
language semantics, grants release approval, or revives managed Stage 0 as a
live dependency.
