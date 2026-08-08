# Decision 0380: Windvale-owned native entry bridge

- Status: Accepted current-host normal-path entry-bridge construction transfer; Linux execution and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0379](0379-Windvale-Owned-Native-Argument-Table.md), [Decision 0360](0360-Native-Bounded-Byte-Entry-Input.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native entry-bridge construction](../../Specifications/Windvale-Native-Entry-Bridge-Construction.md)
- Advanced by: [Decision 0381](0381-Windvale-Owned-Native-Byte-Result-Admission.md)

## Context

Decision 0360 defined the two-cell bridge for `Main(bytes) -> bytes`, but the
managed executor still allocated a byte array, wrote the result and input
descriptor fields, copied them to unmanaged memory, and decoded the result
cell directly. Decision 0379 removed the adjacent argument-table writer and
left this entry/result bridge as the next small ABI-layout owner.

The host must still acquire immutable input storage, allocate call-visible
memory, invoke platform code, admit returned ranges, copy accepted bytes, and
release every allocation. Initial descriptor layout is portable Windvale work.

## Decision

- Define exact 32-byte `WVJQ 1` request and bounded `WVJR 1` response envelopes
  for parameterless and byte-input descriptor entries.
- Let portable Windvale validate input presence, opaque pointer, length, and
  reserved state before constructing the exact zero result cell and optional
  immutable input descriptor.
- Put allocation, copy, post-call reread, and release in one focused host owner.
  It permits the first result descriptor to change and requires every byte of
  the optional input descriptor to remain unchanged, even after a trap.
- Pass the parsed result descriptor to the existing independent range and
  lifetime admission step; do not weaken static-data, entry-input, arena, size,
  or reserved-word checks.
- Consume one exact digest-bound service-free WVNF in ordinary execution. Keep
  a frozen C# construction oracle only for explicitly service-free bootstrap
  execution so retained Windvale constructors do not recursively construct
  themselves.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Entry-bridge core WVB | 3,385 | `8eab863c7b214e559c48c822381b822eef22bd852ce16252bb392ebdfbcefdae` |
| Retained entry-bridge WVB | 3,401 | `d66a34430da6db3271103cfb9c2064a3a5a9de455c564ed87144cf4a0a4994c1` |
| Retained entry-bridge WVNF | 37,374 | `2abde6462aa470f4037aa87ae486f16f2a106932d3022344e85fa5763d44623b` |

## Evidence and consequences

The reviewed focused case pins and reproduces all source/WVB/WVNF identities;
confirms that the runtime embeds no constructor WVB; compares parameterless,
zero-length-input, and maximum-length-input requests through the reference
interpreter, retained native fragment, independent response verifier, and
frozen Stage 0 oracle; checks eight malformed requests covering every status
family; proves result mutation is admitted while input-descriptor mutation is
rejected; reproduces the bridge through the ordinary native source front door;
and executes real `Main() -> bytes` and `Main(bytes) -> bytes` fragments. The
single selected test passes 1/1 in 1.378 seconds through the zero-warning
Release test application. The affected runtime also builds in Release with
zero warnings and errors.

The exact compiler, Development, Standard, Qualification, Linux, and broader
hosted gates were not run under the goal's deferred-broad-verification rule.

`X64ˉnativeˉexecutor` no longer writes or directly decodes bridge descriptor
fields and no longer owns the result-cell allocation/free sequence. It retains
entry-input allocation, executable invocation, result-range admission and
copying, arena/W^X platform authority, and teardown. Those host duties and the
two frozen service-free bootstrap oracles remain later retirement slices.

## Reconsideration triggers

Version this request if an exported entry shape, hidden-result convention,
descriptor representation, byte limit, or mutable region changes. Never
serialize process pointers or live entry bytes into retained artifacts.
