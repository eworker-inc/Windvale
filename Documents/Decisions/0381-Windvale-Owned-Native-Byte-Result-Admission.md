# Decision 0381: Windvale-owned native byte-result admission

- Status: Accepted current-host normal-path byte-result admission transfer; Linux execution and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0380](0380-Windvale-Owned-Native-Entry-Bridge.md), [Decision 0080](0080-Native-Byte-Result-And-Live-Stencil-Consumption.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native byte-result admission](../../Specifications/Windvale-Native-Byte-Result-Admission.md)

## Context

Decision 0380 moved entry-bridge construction and descriptor parsing into a
focused owner, but the large managed executor still decided whether the
untrusted returned pointer/length belonged to static fragment data, the
committed execution arena, or the immutable entry input. That portable
containment policy was the next remaining result-path semantic owner.

The baseline native backend does not yet lower general `u64` values. The
admission rule only needs to compare an opaque address with a bounded range, so
two checked `u32` limbs are sufficient without changing the native ABI or
dereferencing memory.

## Decision

- Define exact variable-size `WVRQ 1` and bounded `WVRR 1` envelopes carrying
  one result descriptor, the committed arena and optional input ranges, and at
  most 4,096 verified immutable static-data ranges.
- Let portable Windvale validate every bound and admit null-empty, arena,
  input, or static-data results using checked unsigned two-limb containment.
- Treat invalid descriptors and results outside all owners as ordinary
  rejection. Treat malformed arena/input/static evidence as a host invariant
  failure.
- Keep construction of live range evidence, response identity verification,
  real-memory result copying, and teardown in the host adapter.
- Consume one exact digest-bound service-free WVNF in ordinary execution. Keep
  the former C# containment algorithm only as a frozen service-free bootstrap
  oracle to avoid recursive admission while executing the constructor itself.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Byte-result admission core WVB | 7,078 | `eacc3c6bce78f9b07d11b13a46059e92cf8a34fc1f659b896d444e7e3c937c04` |
| Retained byte-result admission WVB | 7,057 | `9106356cf441c995b7c8478b3a5a779628328cd82acac87621de9a45bbb2becf` |
| Retained byte-result admission WVNF | 68,608 | `35c29fa9bbc41a00e8797f7812eb1bbf0f95c7f07b96227ca666cc5bf8fd38c2` |

## Evidence and consequences

The reviewed focused case pins and reproduces all source/WVB/WVNF identities;
confirms that the runtime embeds no constructor WVB; compares null-empty,
arena, input, static-data, exact-end, cross-`u32` boundary, malformed
descriptor, and outside-range cases through the reference interpreter,
retained native fragment, response verifier, and frozen Stage 0 oracle; admits
the last of 4,096 static ranges; checks twelve malformed request cases; rebuilds
through the normal source front door; and executes real static and arena-backed
byte returns. The single selected test passes 1/1 in 1.795 seconds through the
zero-warning Release test application. The affected runtime also builds in
Release with zero warnings and errors.

The exact compiler, Development, Standard, Qualification, Linux, and broader
hosted gates were not run under the goal's deferred-broad-verification rule.

`X64ˉnativeˉexecutor` no longer performs byte-result range arithmetic. It
projects verified live ranges, invokes Windvale admission, and copies only an
admitted result. Real-memory copying, entry-input and arena allocation,
invocation, W^X platform authority, and teardown remain host responsibilities.
The three frozen service-free bootstrap oracles remain explicit later slices.

## Reconsideration triggers

Version this request if a new result owner, descriptor representation, arena or
fragment limit, native address width, or result lifetime rule is admitted.
Never serialize live addresses or result bytes into retained artifacts.
