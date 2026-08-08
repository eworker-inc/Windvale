# Decision 0379: Windvale-owned native argument table

- Status: Accepted current-host normal-path argument-table construction transfer; Linux execution and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0378](0378-Windvale-Owned-Native-Execution-Context.md), [Decision 0073](0073-Native-Argument-Table-And-Process-Input-Services.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native argument-table construction](../../Specifications/Windvale-Native-Argument-Table-Construction.md)

## Context

Decision 0073 removed managed argument callbacks and qualified the immutable
native argument snapshot, but Stage 0 still wrote each 16-byte
pointer/length/reserved descriptor. Decision 0378 moved context construction
and made the remaining descriptor writer the next removable layout owner
inside execution-buffer setup.

The launcher already validates at most 67 strict-UTF-8 arguments, 4 KiB each
and 64 KiB total. Packing those bytes and acquiring real process addresses are
host memory duties. Descriptor layout and complete bounded coverage are not.

## Decision

- Define variable-size `WVAQ 1` input and `WVAR 1` response envelopes bounded
  by the existing argument count and byte limits.
- Let portable Windvale validate the entry count, canonical payload extent,
  nonzero opaque targets, per-entry lengths, running offsets, and exact final
  coverage before constructing each unchanged 16-byte borrowed-text
  descriptor.
- Keep strict UTF-8 validation, immutable payload packing, pointer projection,
  allocation, independent descriptor/range/byte reread, and teardown in the
  host owner.
- Consume one exact digest-bound service-free WVNF in the normal runtime and
  keep source/WVB only for reproduction, qualification, and recovery.
- Construct no table for a zero-argument snapshot; retain the existing zero
  context pointer and count.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Argument-table core WVB | 4,362 | `08df8569d091fc0c860988dceff1320d7a8e407b54ce571515af601c10120d75` |
| Retained argument-table bridge WVB | 4,374 | `080be2dea127948697222c23efe4be828410450b602dee5cf2a63abc11627788` |
| Retained argument-table bridge WVNF | 44,775 | `4a4cc1d6171126a821c1f96de11c4ffcb78ea83e98d06d5e0802e5921e9062d8` |

## Evidence and consequences

The reviewed focused case pins and reproduces all source/WVB/WVNF identities;
confirms that the runtime embeds no constructor WVB; compares empty-text,
mixed-width, and maximum-67-entry requests through the reference interpreter,
retained native fragment, independent response verifier, and exact structural
oracle; checks ten malformed requests covering every status family;
reproduces the bridge through the ordinary native source front door; and
executes a real hosted `process.argument` lookup through the new table. The
single selected test passes 1/1 in 1.589 seconds through the Release test
application. The affected runtime builds in Release with zero warnings and
errors.

The existing larger hosted file-input case, exact compiler, Development,
Standard, Qualification, and Linux gates were reviewed but not run under the
goal's deferred-broad-verification rule.

`Nativeˉexecutionˉbuffers` no longer writes descriptor fields. It still owns
the prevalidated host strings, strict encoding, packed allocation, opaque
address projection, independent table and byte verification, and reverse-order
release. Arena allocation, entry/result bridge cells, invocation, W^X platform
authority, and teardown remain later host slices.

## Reconsideration triggers

Version this request if arguments stop being one immutable strict-UTF-8
snapshot, the table representation or limits change, or a native launcher can
bind its own validated `argc`/`argv` without this projection. Never serialize
process pointers or live argument bytes into retained build artifacts.
