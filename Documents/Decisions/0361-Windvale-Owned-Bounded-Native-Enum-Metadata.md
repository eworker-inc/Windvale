# Decision 0361: Windvale-owned bounded native enum metadata

- Status: Accepted historical bounded transfer; oversized lane advanced by Decision 0362; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0072](0072-Final-Pure-Runtime-Native-Services.md), [Decision 0359](0359-Windvale-Owned-Native-Enum-Name-Leaf.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Requires: [Decision 0360](0360-Native-Bounded-Byte-Entry-Input.md)
- Advanced by: [Decision 0362](0362-Windvale-Owned-Segmented-Native-Enum-Metadata.md)
- Contract: [Windvale native execution context](../../Specifications/Windvale-Native-Execution-Context.md#dynamic-text-and-byte-arena)

## Context

Decision 0359 moved the invariant 323-byte enum-name leaf into Windvale but
left construction of its adjacent, type-dependent `WVEN` block in C#. Decision
0360 then admitted one capability-free native `Main(bytes) -> bytes` entry,
which is sufficient to pass a bounded nominal-type projection to a portable
Windvale constructor without adding file, process, callback, or interpreter
authority.

The existing `WVEN` contract permits 32 MiB, while one ordinary Windvale byte
value is limited to 4 MiB. Silently reducing the qualified metadata contract
would reject previously valid fragments. This slice therefore transfers the
complete bounded lane and keeps the larger lane visible as temporary recovery
work rather than disguising a contract change.

## Decision

- Define runtime-private `WVEQ` version 1 as the strict request supplied to the
  Windvale constructor. Its 24-byte header records magic, version, total bytes,
  nominal type count, enum member count, and directory offset.
- Encode each request type in 8 bytes as kind and member count. Encode each
  member in 12 bytes as signed value, absolute request-relative name offset,
  and name length, followed by concatenated canonical identifier bytes.
- Limit one request to 4 MiB. The portable Windvale core independently checks
  every header, count, kind, offset, length, exact final extent, identifier,
  duplicate enum value, and duplicate enum name before constructing output.
  Invalid requests return an empty byte result.
- Construct canonical `WVEN` version 1 in Windvale whenever its result is at
  most 4 MiB. Preserve every existing byte, ordering rule, absolute name
  offset, reserved zero field, enum-name leaf identity, and bundle placement.
- Give this bounded constructor an explicit 100,000,000-instruction ceiling.
  This is a maximum containment allowance for the legal 256-member boundary,
  not a required or expected execution count.
- Keep the managed request projection and final independent `WVEN` parser as
  temporary bridge evidence. Require the final parser to compare every value,
  offset, length, reserved field, name byte, and final extent against the
  already verified nominal declarations.
- Isolate the former C# writer as an explicitly named recovery path used only
  when a valid `WVEN` result is greater than 4 MiB and no greater than the
  existing 32 MiB contract. Do not route smaller normal results through it.
- Move enum-metadata responsibilities out of the broad native text-services
  source into one focused owner. The text-services file falls from 507 to 304
  lines; the new owner keeps bounded construction, recovery, and independent
  verification together rather than splitting them into numbered fragments.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Windvale bounded enum-metadata core WVB | 9,640 | `30f1fa4c85ad50991d68ac5eecfb9ada2ed63bd36229b777aff899b4cd0f0e3d` |
| Retained bounded enum-metadata bridge WVB | 9,619 | `595dc56d36ed75bd9857bf5011e59d17271cf03ea6a346079474291842bd5a47` |

## Evidence and consequences

The focused test pins both WVB identities, compares the retained bridge byte
for byte, and reproduces it through the ordinary native source front door. It
requires the reference interpreter, verified x64 backend, and frozen managed
recovery writer to agree on the exact normal `WVEN` result. Malformed magic,
truncation, duplicate values, duplicate names, and invalid identifiers all
return empty through both Windvale execution modes.

The same test covers 256 members and a 255-byte name at the legal boundary. A
separate 1,024-type fixture proves that a request can fit the 4 MiB input seam
while its projected result exceeds 4 MiB: Windvale rejects that bounded lane
before construction, while the explicitly isolated recovery writer preserves
the existing result and passes the independent validator. After review, the
focused Release project built with zero warnings and errors in 3.13 seconds;
the single named test passed 1/1 in 2.321 seconds.

This is a real normal-path transfer for every `WVEN` at or below 4 MiB, but it
is not complete `WVEN` retirement. C# still projects the temporary request,
loads and lowers the retained WVB, owns W^X execution and copying, independently
validates the result, and constructs valid 4-to-32-MiB metadata. Linux execution
and the grouped broad gate remain deferred. The next enum-metadata slice should
introduce bounded streaming or session-owned output so the oversized recovery
writer can be removed without changing the 32 MiB contract.

## Reconsideration triggers

Version or replace `WVEQ` if a native nominal-declaration owner can pass its
verified model directly without serialization. Replace the 4 MiB result seam
when native execution owns bounded streaming, chunk publication, or a larger
session result with equivalent validation and teardown. Keep the C# writer
until that replacement covers the complete qualified 32 MiB range.
