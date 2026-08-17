# Language 1.0 workload 1: command-line application

## Status

Complete first-author paper bundle for workload 1 under the
[Language 1.0 paper corpus](../../Windvale-Language-1.0-Paper-Corpus.md).
It awaits project-owner review. It is not frozen source, an implemented
application, or a published standard-stream capability. Current compilers
continue to accept Windvale Seed.

## Result first

The bundle expresses one small useful command without ambient arguments,
environment variables, locale, filesystem access, hidden allocation, implicit
numeric conversion, catchable exceptions, or Boolean output success. The
candidate command is:

```text
windvale-inspect --operation bytes|runes [--maximum-bytes NUMBER]
```

The launcher supplies a bounded immutable argument sequence and one owned
98,304-byte root memory budget. The application reads one complete strict UTF-8
value through `standard.input`, reports either its UTF-8 byte length or rune
count, writes exact UTF-8 bytes through `standard.output`, and uses an
independently bound `standard.diagnostic` capability for failures.

The default input maximum is 4,096 bytes and the accepted application maximum is
65,536 bytes. Output rejection, exact partial acceptance, and indeterminate
progress produce different terminal statuses and are never retried.

## Bundle contents

| Item | Owner |
| --- | --- |
| [`Source/`](Source/) | Five complete candidate edition-1 modules for types, package data, argument parsing, measurement/rendering, and application orchestration. |
| [Package plan](Package-Plan.md) | Exact module graph, usage-resource binding, launcher profile, capabilities, budgets, and construction order. |
| [Command contract](Command-Contract.md) | Paper-only argument, input, output, diagnostic, and process-status contracts plus the exact Foundation calls selected by the source. |
| [Semantic review](Semantic-Review.md) | Metadata, values, ownership, effects, failures, limits, cleanup, cancellation, and common-corpus review answers. |
| [Rejected cases](Rejected-Cases.md) | Compile, build, launcher, input, mutation, and diagnostic boundary cases. |
| [Expected outputs](Expected-Outputs.md) | Exact successful and failing semantic observations independent of a backend. |
| [Implementation responsibilities](Implementation-Responsibilities.md) | Compiler, Foundation, launcher, capability, runtime, verifier, editor, and evidence ownership. |
| [Review findings](Review-Findings.md) | Acceptance matrix, source-freeze findings, recommendations, quantitative record, and review status. |

## Source graph

```text
Inspectˉapplication
  -> Inspectˉarguments
       -> Inspectˉtypes
       -> Foundationˉcollections/numeric/option/result
  -> Inspectˉsummary
       -> Inspectˉtypes
       -> Foundationˉbytes/memory/result/text
  -> Inspectˉpackage
  -> Platformˉstream
  -> Foundationˉbytes/collections/memory/resource/result

Inspectˉpackage
  -> one immutable 73-byte package resource
```

The build supplies Foundation and `Platformˉstream` by canonical module
identity. Source imports do not search a path or choose a provider.

## Example behavior

```text
$ windvale-inspect --operation bytes
bytes=9

$ windvale-inspect --operation runes --maximum-bytes 6
runes=4
```

The first example's input is the nine UTF-8 bytes for `Windvale` followed by LF.
The second input is `AˉΩ` followed by LF: six UTF-8 bytes and four runes.

`--help` is the only successful path that does not acquire input. No arguments,
unknown or repeated options, missing values, invalid operation names, malformed
or overflowing numeric values, and values above 65,536 are argument failures.

## Scenario boundary

This workload proves ordinary command usability, not a shell grammar or a
general process ABI. It intentionally excludes:

- filesystem paths and file opening;
- environment variables, current directory, locale, terminal presentation, and
  host encoding;
- streaming or interactive input;
- tasks, deadlines, cancellation, and provider-instance resources;
- automatic retry or idempotency keys; and
- dynamic command discovery or reflection-based option parsing.

Workload 2 owns resource-bearing file I/O and exact chunk progress. Workloads 5
and 6 own network streams, deadlines, cancellation, and concurrent service
behavior.

## Review rule

Review source and evidence together. A reviewer must not make the source appear
convenient by treating arguments, memory, text decoding, formatting, output
progress, or exit mapping as ambient implementation behavior. Conversely, the
application should not repeat provider mechanics when a small semantic
capability can expose the exact bounded operation.
