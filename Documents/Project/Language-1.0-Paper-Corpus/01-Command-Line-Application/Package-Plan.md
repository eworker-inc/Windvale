# Workload 1 package and build plan

## Status and identity

This is the exact paper package plan for canonical package identity
`windvale.paper.language1.inspect` version 1. It binds source and one immutable
usage resource without host-path lookup, build-script execution, dynamic import,
runtime package-name lookup, or filesystem authority.

## Module mapping

| Canonical module | Source | Profile | Target scope |
| --- | --- | --- | --- |
| `Inspectˉtypes` | [`Source/Inspect-Types.wv`](Source/Inspect-Types.wv) | Core | Windows, Linux, Windvale |
| `Inspectˉpackage` | [`Source/Inspect-Package.wv`](Source/Inspect-Package.wv) | Core | Windows, Linux, Windvale |
| `Inspectˉarguments` | [`Source/Inspect-Arguments.wv`](Source/Inspect-Arguments.wv) | Core | Windows, Linux, Windvale |
| `Inspectˉsummary` | [`Source/Inspect-Summary.wv`](Source/Inspect-Summary.wv) | Core | Windows, Linux, Windvale |
| `Inspectˉapplication` | [`Source/Inspect-Application.wv`](Source/Inspect-Application.wv) | Hosted application | Windows, Linux, Windvale |

The build also supplies the exact accepted normative-candidate Foundation calls
and the paper-only `Platformˉstream` signatures recorded in
[Command-Contract.md](Command-Contract.md). Those modules are dependencies, not
searched source.

## Package-resource binding

| Declaration identity | Resource identity | Type | Declared maximum | Exact length | SHA-256 |
| --- | --- | --- | ---: | ---: | --- |
| `Inspectˉpackage.Usage` | `windvale.paper.inspect.usage.v1` | `bytes` | 73 | 73 | `3834b674a9f9df457e7e678f3682d9b5fc8fbc02bce71f87e3866b6d8773cc05` |

The exact payload is:

```text
Usage: windvale-inspect --operation bytes|runes [--maximum-bytes NUMBER]\n
```

Its canonical bytes are:

```text
55 73 61 67 65 3A 20 77 69 6E 64 76 61 6C 65 2D
69 6E 73 70 65 63 74 20 2D 2D 6F 70 65 72 61 74
69 6F 6E 20 62 79 74 65 73 7C 72 75 6E 65 73 20
5B 2D 2D 6D 61 78 69 6D 75 6D 2D 62 79 74 65
73 20 4E 55 4D 42 45 52 5D 0A
```

Canonical shipment contains one content object and one declaration reference.
The 73 retained bytes are charged once to the application resource domain.
Mapping or sharing the object does not multiply that charge. The declaration
does not expose a path, file handle, provider, or capability.

## Launcher profile

The package selects launcher profile `windvale.launch.command.v1` and exported
entry `Inspectˉapplication.Run` with the exact signature:

```text
fn Run(
    Arguments: Foundationˉcollections.Sequence<text>,
    Budget: Foundationˉmemory.Memoryˉbudget,
) -> Inspectˉtypes.Processˉstatus
```

`Run` is an ordinary source name. Before source execution the launcher:

1. admits the exact package, source edition, modules, concrete target, and
   launcher profile;
2. constructs one immutable argument sequence excluding the executable or
   package display name;
3. verifies at most 16 arguments, at most 256 UTF-8 bytes per argument, and at
   most 2,048 aggregate argument UTF-8 bytes;
4. constructs and transfers one owned 98,304-byte root memory budget;
5. approves exactly three required capabilities and binds their module roots;
6. binds the arguments and budget by exact parameter type and position; and
7. invokes no source when any binding, maximum, target, or capability check
   fails.

Argument spelling is already decoded strict UTF-8 by the launcher profile.
There is no ambient `argv`, executable path, environment table, current
directory, locale, or host console handle.

## Capability closure

| Capability | Major | Purpose | Does not grant |
| --- | ---: | --- | --- |
| `standard.input` | 1 | Read one complete bounded strict UTF-8 input value. | Filesystem, paths, terminal control, network, replay, or environment access. |
| `standard.output` | 1 | Attempt one exact byte mutation on normal output. | Diagnostic output, remote receipt, durability, retry, or terminal control. |
| `standard.diagnostic` | 1 | Attempt one exact byte mutation on diagnostic output. | Normal output, logging authority, durability, retry, or terminal control. |

There are no optional capabilities and no provider instances stored in source
locals. Each root is module-bound under Decision 0754 and remains in the exact
function effect set and transitive approval closure.

## Root-domain ceiling: 98,304 bytes

| Charge | Maximum bytes |
| --- | ---: |
| Usage package object | 73 |
| Argument UTF-8 payloads | 2,048 |
| Argument sequence and metadata | 512 |
| Reserved diagnostic builder | 256 |
| Strict input text | 65,536 |
| Summary text builder | 32 |
| Summary byte builder | 32 |
| Bound provider state | 4,096 |
| Launcher/runtime/accounting state | 8,192 |
| Reserved failure and teardown capacity | 4,096 |
| Admitted unassigned headroom | 13,431 |
| **Total** | **98,304** |

The headroom is charged authority, not permission for unbounded growth. Package,
argument, and launcher state are charged before entry. Source first reserves the
256-byte diagnostic child, then reserves input and both 32-byte output builders
before requesting input. Every failed split occurs before the input provider or
output providers are called.

## Other exact limits

| Boundary | Maximum |
| --- | ---: |
| Arguments | 16 |
| One argument | 256 UTF-8 bytes |
| Aggregate arguments | 2,048 UTF-8 bytes |
| Numeric option input | 20 UTF-8 bytes |
| Default input | 4,096 UTF-8 bytes |
| Absolute input | 65,536 UTF-8 bytes |
| Successful output | 32 bytes |
| Diagnostic output | 256 bytes |
| Source recursion | 0 |
| Runtime child tasks, queues, timers | 0 |
| Output mutations per path | 1 |
| Diagnostic mutations per path | 1 |
| Input reads per path | 1 |

Argument parsing examines at most 16 entries and at most 2,048 UTF-8 bytes.
Strict input decoding and rune counting examine at most 65,536 bytes or runes.
Formatting appends at most 32 UTF-8 bytes and encoding appends the same maximum.

## Status translation

The launcher maps the returned enum members to exact process statuses:

| Member | Integer | Meaning |
| --- | ---: | --- |
| `Success` | 0 | Usage or requested summary was accepted completely. |
| `Argumentsˉfailed` | 2 | Argument rejection and complete diagnostic acceptance. |
| `Inputˉfailed` | 3 | Input rejection and complete diagnostic acceptance. |
| `Outputˉfailed` | 4 | Normal or diagnostic output rejected, accepted partially, or reported an inconsistent completed count. |
| `Outputˉindeterminate` | 5 | Normal or diagnostic output progress cannot be proved. |
| `Resourceˉfailed` | 6 | A bounded memory or rendering operation failed and any attempted diagnostic completed. |

If constructing a diagnostic fails, status 6 is returned without a second
allocation attempt. If writing a diagnostic rejects or partially accepts, status
4 replaces its primary argument/input/resource status; indeterminate diagnostic
progress returns status 5. The source never retries either condition.

## Construction and publication order

1. Verify module identities, target scopes, package-resource type, length,
   maximum, and digest.
2. Admit the argument table and root-domain ceiling.
3. Approve and bind exactly three capability roots.
4. Invoke `Run` and reserve diagnostic capacity.
5. Parse arguments without allocation or provider calls.
6. Return usage directly, or reserve input/text/byte child budgets.
7. Read and validate one complete input value.
8. Measure and construct one immutable output.
9. Attempt exactly one normal or diagnostic mutation.
10. Return the deterministic status and release all retained values in reverse
    successful acquisition order.

No failure publishes a package or executable artifact. No source path can call
both normal and diagnostic output for one terminal result.
