# Decision 0376: Windvale-owned native file-input table

- Status: Accepted current-host normal-path initial `WVFI` construction transfer; Linux execution and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0375](0375-Windvale-Owned-Native-File-Output-Table.md), [Decision 0371](0371-Windvale-Owned-Native-File-Input-Leaves.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native file-input-table construction](../../Specifications/Windvale-Native-File-Input-Table-Construction.md)

## Context

Decision 0371 moved both file-input leaves to Windvale and Decision 0373 moved
their executable-image placement. The normal runtime still wrote every
immutable field of the 136-byte initial `WVFI` table in C#: platform, four
arena targets and bounds, zero snapshot state, and seven Windows functions.

Arena allocation, export lookup, and post-execution snapshot verification
remain host duties. The initial table format and platform-presence rules do
not: Windvale can validate exact limits and copy opaque target ranges without
interpreting native pointers.

## Decision

- Define exact 136-byte `WVNQ 1` input and 32-byte `WVNR 1` response envelope.
- Let portable Windvale validate the platform, nonzero arena targets, exact
  snapshot/name/data/scratch bounds, zero initial count and reserved fields,
  and seven-function presence policy before constructing unchanged initial
  `WVFI 1` bytes.
- Treat every arena and function target as an opaque eight-byte range. Do not
  add native-pointer, memory-allocation, export-lookup, or snapshot-publication
  semantics to portable Windvale.
- Retain host ownership of arena allocation, `kernel32.dll` loading and export
  resolution, native table allocation/copy, post-execution count/record/name
  verification, and teardown.
- Consume one exact digest-bound service-free WVNF in the normal runtime and
  keep its source/WVB only for reproduction, qualification, and recovery.
- Keep the constructor and artifact/response verifier separate from the host
  context and existing leaf owner.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| File-input-table core WVB | 5,078 | `0c6b66ae7fcef5a0b73df1d56bbfd0a5376ae2978f6ae762470abcf544b6a438` |
| Retained file-input-table bridge WVB | 5,084 | `e7d33fc579c0bc2d001a3e7e2ad68e6403091cae6bda270e51578e10f04c4bd9` |
| Retained file-input-table bridge WVNF | 52,334 | `378240d8f8770a4707d7f2ae86daae24036fc2eb9fd273d5ab737c9c03e3e70d` |

## Evidence and consequences

The reviewed focused case pins and reproduces all source/WVB/WVNF identities,
confirms that the runtime embeds no constructor WVB, compares Windows and Linux
models plus nineteen malformed requests through the reference interpreter and
verified native fragment, checks exact expected initial `WVFI` bytes,
reproduces the bridge through the ordinary native source front door, and
creates/rereads then corrupts a real current-host context. The single selected
test passes 1/1 in 1.398 seconds through the Release test application.

The broader real-file and hosted-input tests were reviewed but not run under
the goal's deferred-broad-verification rule. Development, Standard,
Qualification, and Linux gates were also not run.

The normal runtime no longer writes immutable initial `WVFI` fields in C#. It
still allocates sparse arenas and scratch, loads and pins the Windows library,
resolves functions, projects the request, independently verifies/copies the
Windvale table, permits the native leaf to advance snapshot count only after
publishing a complete record, rereads every record and strict-UTF-8 name, owns
the application W^X lifetime, invokes the entry, and tears resources down.
All three specialized binding-table layouts are now Windvale-owned. The shared
service table and execution context are the next static construction seams.

## Reconsideration triggers

Replace this request boundary when a native host adapter owns arena allocation,
export acquisition, and post-execution snapshot validation. Keep dynamic
targets out of WVB, WVO, caches, and retained artifacts. Version the contract
if any capacity, ordering, platform, or mutation rule changes.
