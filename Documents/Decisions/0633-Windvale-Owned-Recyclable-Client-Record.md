# Decision 0633: Windvale-owned recyclable-client record

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0632](0632-Windvale-Owned-Directory-Provider-Image-And-Context.md)
- Contract: [recyclable-client record emission](../../Specifications/Windvale-Os-X64-Process-Client-Record-Emission.md)

## Context

The kernel now reserves and retains a private 122-page recyclable-client
extent, but the record that binds its admitted interpreter, program, budgets,
addresses, and rights remained bootstrap-authored. That record must be exact
and remain unpublished until the rest of the address space is complete.

## Decision

- Emit the exact 518-byte client selection and record-construction slice at
  fixture offsets 4,341 through 4,858.
- Require exact 32-byte interpreter and program digests before emitting bytes.
- Preserve separate rights-limited resource and directory capabilities with
  generation-tagged endpoint references.
- Retain the existing private 122-page geometry and explicit execution budgets.
- Keep paging, input copies, context/resource completion, readiness publication,
  rollback, and QEMU execution as later mandatory composition steps.

## Evidence and consequences

The exact slice has SHA-256
`b7f96df2b0a39f201b1c1bbe83c2cefab455c0417be19892877078839965562e`.
The self-test WVB is 16,843 bytes at
`6182088b7f1ae89766d2a8cb20b2b022a4ca54571ba63312c7111379c1b15ef3`.
Its Windows executable is 251,392 bytes at
`2cbedd60fd226415ba274cffb121b7c39505fa74a6ed854fa628770d844d406b`;
the paired Linux image is 254,064 bytes at
`08911fe6297712035388dd9ae1baaa9e03ddb6d905fd82aba485a33dc192f484`.
The focused owner passes 84 cases across fourteen projects with local results
50/51/52/53/54/55/56/57/58/59/60/61/62/63. The retirement inventory is 70
suites and 3,648 cases.

Windvale source now reconstructs the first 4,859 process-machine bytes and all
33 relocation fields in that interval. The next boundary is recyclable-client
paging, not live publication.

## Reconsideration triggers

Replace fixed addresses and budgets when admitted metadata drives general
layout. Preserve exact digest admission, checked geometry, explicit capability
rights and generations, private construction, readiness-only publication, and
failure-atomic rollback.
