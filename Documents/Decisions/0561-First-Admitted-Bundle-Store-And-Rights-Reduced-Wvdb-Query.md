# Decision 0561: First admitted bundle, immutable store, and rights-reduced WVDB Query

- Status: Qualified and implemented
- Date: 2026-08-15
- Extends: Decisions 0530, 0557, and 0560
- Scope: Milestone 2 package-backed host application

## Context

Package 1 and Lock 1 already selected the complete WVDB Query closure and
reproduced its WVB offline. Milestone 2 still lacked three connected product
boundaries: a deterministic independently admitted transport, immutable local
publication, and real execution through the declared read-only directory
capability. The existing native lowerer also rejected the application's variant
results, and its generic hosted service profile did not bind
`filesystem.directory_read_v1`.

A general registry, online resolver, installer, database server, and ambient
filesystem binding would widen this slice without strengthening the selected
application proof.

## Decision

- Implement bounded in-memory Bundle 1 with distinct Windvale-written writer
  and verifier implementations. Admit the header, canonical index, every blob,
  Package 1 and Lock 1 agreement, item references, target, and executable before
  publication. Retain the 4 MiB implementation policy while the format keeps
  its separate streaming boundary.
- Publish admitted blobs and the admitted bundle under immutable SHA-256
  fan-out identities. A first publish creates private reread-verified objects;
  a repeated publish accepts only byte-identical existing identities. Host paths
  are absent from portable bundle evidence.
- Extend the shared x86-64 lowerer with bounded variant declaration, create,
  case-test, and payload lowering. A variant is a nominal record-backed value
  containing its case tag and bounded payload; record payloads are copied into
  the variant's caller-owned backing.
- Raise only the native function directory and segmented symbol ordinal bound
  from 512 to 1,024 entries. The variant-capable staging compiler contains 532
  functions and 581 total WVO symbols; per-function, type, data, aggregate-code,
  relocation, and 4 MiB staging limits remain unchanged. Module-scale function
  rejection records the rejected count in the plan detail field.
- Cross the one-time bootstrap edge through a detached prior-source bridge that
  changes only the function-directory constant, is packaged by the retained
  qualified candidate, and stages the current successor. The refreshed
  successor then reproduces its complete paired toolset through the ordinary
  .NET-free native path; the bridge is not a checked-in product.
- Admit the exact three-argument `filesystem.directory_read_v1` capability
  through ABI 23's provider table. Construct one execution-owned five-entry
  capability directory and bind ordinal two to a provider that owns only one
  fixed read-only object named `Windvale-Database-Storage.bin`.
- Keep Windows and Linux leaf mechanics separate. The Windows leaf owns one
  `GENERIC_READ`, `FILE_SHARE_READ`, `OPEN_EXISTING` handle; the Linux leaf owns
  one `O_RDONLY|O_CLOEXEC` descriptor. Neither accepts a source-provided path or
  contains a mutation operation.
- Remove WVDB Query's dependency on the generic hosted `i32.format` service by
  formatting its result through portable checked byte construction and the
  already admitted `u32.format` surface. This keeps the application inside the
  exact package capability profile rather than granting an unrelated service.
- Make Bundle 1/store and WVDB capability execution permanent native retirement
  owners. Each owner emits numbered progress before its final summary. The
  changed-file planner must select them for their format, application, lowerer,
  provider, and host-adapter inputs with no uncovered gap.

## Deterministic candidate identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Locked WVDB Query WVB | 26,294 | `61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2` |
| WVDB Query Bundle 1 | 43,725 | `3d7f035e15fa839d9a7a3f8df6a7fa152e115aba42c1b48bdd1ae0b1ba998474` |
| Variant-capable lowerer core WVB | 499,757 | `abd640dc79f2065caf3e2e5818a3388fe531fb6f25b22dd4c8d8369fd31895c8` |
| Variant-capable lowerer tool WVB | 501,344 | `4ef35324a2e5ba3bd0cf8751fb2b6beb3a8c6108767734ea719b5dab063c8746` |
| Windows WVB-to-WVO tool | 7,275,520 | `d41ba4a438156bf3cd0e886ab59fcf5ff0b7474f2dfee4307a2ff60c5972225f` |
| Linux WVB-to-WVO tool | 7,274,496 | `328640d04a2cdff6d1fe943b076554933a7538652185e0e1002fcc4cacbd3579` |
| Segmented WVO producer WVB | 526,914 | `c20f22cec9ce735ccde9904fc08d64c8c9c91e086bfd35292abd1832731ccff7` |
| Windows segmented WVO producer | 7,711,232 | `4b7d9cc05e98fbe0277b86e7a1a5a288892caf8c2f9e9ca91e8050f479064884` |
| Linux segmented WVO producer | 7,712,768 | `fe95f58555d63b2b09fe166958e9a4e25bd6093d130683c8128658a9cfa51c60` |
| Compiler-image staging WVB | 75,553 | `5795ccd8f12266f0228b7191680dc6881f5a09ddb81973ee6225d24fa38a60bb` |
| Windows compiler-image staging tool | 852,480 | `bbef433e11eb63d265cee5a7439d5e500163a27723d72ac7a805fb7eb0181844` |
| Linux compiler-image staging tool | 851,968 | `0762483a8c4d68bdb246100f757890a1ee22b42e1b2f4b67cd08d1d2d102aa0b` |
| Lowered WVDB Query WVO | 237,210 | `b3d3bbde00136c230f6804215c352490bae9603b338d25186dba827be137edbf` |
| Windows linked image | 238,413 | `60bdf794d8fba0889a077eeec35fab75de9fd174a5a894eb78ef316ad1c8872c` |
| Linux linked image | 237,437 | `76b8327d6f970c467d76a4e9c2f64d7473897d2afe2a444c007f840e42a35632` |
| Windows hosted application | 258,048 | `7cd60860e07294d9a45064495da33a42cc752849accfc672c35a69454cd963d8` |
| Linux hosted application | 258,048 | `29b4d4db7505daec94865d423e3805b02bde95751343b1fb7e4ceee8045a202d` |

The permanent owners return values `42` and `-5`, report a missing key, reject
a same-length unauthorized object name, and report an unavailable authorized
object without crashing or falling back to ambient access. The
Bundle 1 owner writes the exact candidate twice, admits both independently,
creates five objects plus one bundle on first publication, and reuses all six
immutable identities on the second publication.

GitHub Verify run
[31872089188](https://github.com/eworker-inc/Windvale/actions/runs/31872089188)
passes the exact Bundle 1/store owner on Windows and Linux at commit
`d9795e0e15944b3342ea7c4a42105eee38420708`. Run
[31872429140](https://github.com/eworker-inc/Windvale/actions/runs/31872429140)
passes the exact five-case capability owner on both hosts at commit
`204e8082fdaabbc7333ac40ed6ca7ff8564de123`. The latter commit changes no
Bundle 1 input, so the paired reports qualify one coherent final state.

## Consequences

- Milestone 2 is complete. Its settled Bundle 1/store and WVDB capability owners
  have matching Windows and Linux reports, and later commits need not rerun
  them while their declared inputs remain unchanged.
- Bundle admission and object publication are useful foundations for the 0.1
  installer, but they are not a signed release envelope, activation database,
  updater, or general package-store service.
- WVDB Query proves an application capability closure; it does not make the
  experimental WVDB format a required part of the base Windvale installation.
  Database servers and applications can remain separately versioned packages.
- Variant lowering is admitted because the selected application requires it.
  Other variant layouts, mutable payloads, and unbounded nominal surfaces remain
  outside the accepted native subset.

## Reconsideration triggers

- A Bundle 1 larger than the in-memory policy becomes a required release input.
- The first installer requires activation, rollback, signing, or durable store
  behavior not represented by the bounded publisher.
- A second directory object or capability instance requires configurable
  provider construction rather than this one-object proof.
- Variant payload or lifetime behavior cannot be expressed safely through the
  bounded caller-owned record backing.
