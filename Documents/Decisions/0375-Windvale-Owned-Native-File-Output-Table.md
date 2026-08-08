# Decision 0375: Windvale-owned native file-output table

- Status: Accepted current-host normal-path `WVFO` construction transfer; Linux execution and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0374](0374-Windvale-Owned-Native-Output-Table.md), [Decision 0370](0370-Windvale-Owned-Native-File-Output-Leaves.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native file-output-table construction](../../Specifications/Windvale-Native-File-Output-Table-Construction.md)

## Context

Decision 0370 moved both file-output leaves to Windvale and Decision 0373 moved
their executable-image placement. The normal runtime still wrote every byte of
the 80-byte `WVFO` table in C#, including platform capacity, the scratch target,
and six Windows function pointers.

Scratch allocation and function lookup remain host duties. The established
table format and platform-presence rules do not: Windvale can validate those
inputs and copy their opaque byte ranges without interpreting native pointers.

## Decision

- Define exact 80-byte `WVFQ 1` input and 32-byte `WVFR 1` response envelope.
- Let portable Windvale validate the platform, nonzero scratch target, exact
  Windows/Linux scratch capacity, reserved field, and six-function presence
  policy before constructing unchanged `WVFO 1` bytes.
- Treat scratch and function targets as opaque eight-byte ranges. Do not add
  native-pointer semantics or host export lookup to portable Windvale.
- Retain host ownership of scratch allocation, `kernel32.dll` loading, six
  export resolutions, native table allocation/copy, independent reread, and
  teardown.
- Consume one exact digest-bound service-free WVNF in the normal runtime and
  keep its source/WVB only for reproduction, qualification, and recovery.
- Keep the constructor and artifact/response verifier separate from the host
  context and existing leaf owner.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| File-output-table core WVB | 3,926 | `fb6fd67339561f517967b326cc4299132699dc6f098a38595bbb3aabbf1fbc7f` |
| Retained file-output-table bridge WVB | 3,930 | `94cc057b655c58be3ccd2db333cff4e7a755482c52983c4031196ab060a89e06` |
| Retained file-output-table bridge WVNF | 42,302 | `9333d4573b87b829e6e577d8a27c937bf2fb433a93d4a4b11b783b372d31d08a` |

## Evidence and consequences

The reviewed focused case pins and reproduces all source/WVB/WVNF identities,
confirms that the runtime embeds no constructor WVB, compares Windows and Linux
models plus ten malformed requests through the reference interpreter and
verified native fragment, checks exact expected `WVFO` bytes, reproduces the
bridge through the ordinary native source front door, and creates/rereads then
corrupts a real current-host context. The first run stopped after 1.2 seconds
because its Windows missing-function mutation cleared only half of an opaque
64-bit field; the test was corrected to clear the complete field. The final
single test passes 1/1 in 0.838 seconds, and the affected Release build passes
with zero warnings and errors in 6.95 seconds.

The extended end-to-end real file-write test was reviewed but not run under the
goal's deferred-broad-verification rule. Development, Standard, Qualification,
and Linux gates were also not run.

The normal runtime no longer writes `WVFO` fields in C#. It still allocates
scratch memory, loads and pins the Windows library, resolves functions, projects
the request, independently verifies/copies/rereads the Windvale table, owns the
application W^X lifetime, invokes the entry, and tears resources down. `WVFI`
is now the final specialized binding-table construction seam.

## Reconsideration triggers

Replace this request boundary when a native host adapter owns scratch and
export acquisition. Keep dynamic targets out of WVB, WVO, caches, and retained
artifacts. Version the contract if capacity, ordering, or platform rules change.
