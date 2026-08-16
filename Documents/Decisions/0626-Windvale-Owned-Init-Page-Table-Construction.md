# Decision 0626: Windvale-owned init page-table construction

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0625](0625-Windvale-Owned-Init-Process-Record-Construction.md)
- Contract: [init process paging emission](../../Specifications/Windvale-Os-X64-Process-Paging-Emission.md)

## Context

The source-owned init record names user addresses inside the validated extent,
but those addresses are not safe until a private root preserves required kernel
mappings and exposes only the intended user pages with exact permissions.

## Decision

- Emit the exact 80-byte copy of three retained kernel table pages and the exact
  436-byte init-private paging construction.
- Fill one bounded 512-entry identity PTE page, preserve the null-page hole when
  applicable, and bind it through the private lower tables.
- Map only the init code, stack, data, response, runtime input/budget, and store
  pages required by the retained profile.
- Enforce W^X in the emitted entries: code is non-writable and executable;
  writable pages and read-only data inputs are NX.
- Preserve both local branch fields explicitly and add no external import.
- Keep the paging constructor disconnected from live process publication until
  user-image copy, context creation, endpoint/process publication, dispatcher
  entry, rollback, and QEMU evidence are composed.

## Evidence and consequences

The exact 516-byte slice has SHA-256
`9ad8bfc3fe718503a4b1ff8d456e99125020e45e58ecb9293f7aafd5167456a0`.
The self-test WVB is 14,379 bytes at
`e2f712fb99ecc186211c957a4bdf9f9b0991ad7c735dcb8d47c643e85f9fd50d`.
Its Windows executable is 206,848 bytes at
`857d384d8e62ccfb435986c4b607d8a7615b9d9bc8c78d1bd73efa38f0dc832e`;
the paired Linux image is 213,104 bytes at
`fd20a386a8a0e03a9efce86444498e119f7dffbd67263c3845659d1a7f949ef2`.
The focused owner passes 42 cases across seven projects with local results
50/51/52/53/54/55/56. The retirement inventory is 70 suites and 3,606 cases.

Windvale source now reconstructs the first 2,949 process-machine bytes and all
15 relocation fields in that interval. The next source boundary is private
user-image copy and execution-context construction, not process publication.

## Reconsideration triggers

Replace the fixed identity-window geometry when general process construction
owns arbitrary physical extents or more than one PTE page. Preserve bounded
table walks, null-page denial, W^X, user/supervisor isolation, checked indices,
generation-safe publication, and failure-atomic rollback.
