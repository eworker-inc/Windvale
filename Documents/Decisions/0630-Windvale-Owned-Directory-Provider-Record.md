# Decision 0630: Windvale-owned directory-provider record

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0629](0629-Windvale-Owned-Directory-Provider-Allocation.md)
- Contract: [directory-provider record emission](../../Specifications/Windvale-Os-X64-Process-Directory-Record-Emission.md)

## Context

The isolated directory extent is allocated and validated but remains anonymous.
Its complete process identity, rights, budgets, addresses, runtime profile, and
verified immutable snapshot identity must be constructed privately before page
tables or readiness publication can be safe.

## Decision

- Emit the exact 462-byte directory record construction at fixture offsets
  3,323 through 3,784.
- Require exact 32-byte service and snapshot identities as constructor inputs.
- Clear the complete record before writing directory-specific identities,
  addresses, budgets, capability, role, runtime, and generation fields.
- Bind the retained directory endpoint address privately but do not publish the
  process or endpoint.
- Keep paging, immutable image/snapshot copy, context/descriptor construction,
  readiness publication, rollback, and QEMU evidence as mandatory later steps.

## Evidence and consequences

The exact slice has SHA-256
`8f76ecc8d4d2b74a55c1cc26ffd78f4f1e4ec9bf53847bbf5034a489a33c1b60`.
The self-test WVB is 16,076 bytes at
`b549bbb7566023e09cb8dfa65ad774c6c99a6d4cb4b5f7239d0be317833d40b3`.
Its Windows executable is 236,032 bytes at
`865f82f369212f100f46d8e630bfef5a1aa5468e211ac8e15258bfe7c95f4b19`;
the paired Linux image is 241,776 bytes at
`b4c32f4820655131c2ba596f8003d78c3ffd16179a599c4f4fe77c9e36267e23`.
The focused owner passes 66 cases across eleven projects with local results
50/51/52/53/54/55/56/57/58/59/60. The retirement inventory is 70 suites and
3,630 cases.

Windvale source now reconstructs the first 3,785 process-machine bytes and all
29 relocation fields in that interval. The next boundary is directory-private
page-table and W^X mapping construction, not provider publication.

## Reconsideration triggers

Replace fixed directory fields when verified provider metadata drives general
process construction. Preserve complete zeroing, measured identities, minimal
rights, exact budgets, readiness-only publication, and rollback.
