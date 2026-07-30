# Decision 0005: Immutable nominal Seed records

- Date: 2026-07-29
- Status: Accepted and qualified on Windows and Debian Linux

## Context

The qualified `Wvˉdumpˉcore` returned only an integer status. That proved bounded envelope walking, but it could not carry a section count, a failure offset, or a reusable section descriptor. Encoding several facts into more status numbers would make the first tool harder to extend and would postpone an aggregate model that the future compiler, assembler, linker, and Foundation library will all need.

The first aggregate must not accidentally decide Windvale's eventual general heap, ownership, garbage collection, class, or mutable object model. It should have exact bytecode types, deterministic representation, and straightforward verification.

## Decision

- Add immutable nominal record product types to Seed.
- A record declaration defines a named type, ordered named fields, and a positional constructor.
- Permit record values in function parameters, results, and locals.
- Restrict Seed record fields to primitive value types. Defer nested records until a concrete tool data structure requires them.
- Use exact nominal identity in semantic analysis, WIR, bytecode verification, and runtime values. Equal field layouts do not make two record types interchangeable.
- Add a canonical Types section and record value shapes to WVB 1.2.
- Add `record.create` and `record.field`; do not add field mutation or record equality.
- Bound modules to 1,024 records and each record to 64 fields.
- Replace `Wvˉdumpˉcore`'s status-only result with `Wvbˉinspection`, and use `Wvbˉsection` as its bounded internal descriptor.
- Keep the status field numeric until the next tool slice establishes the smallest useful enum model.
- Do not read obsolete WVB 1.1 modules. The format remains an early-development contract and all current fixtures move together.

## Consequences

- Structured values now cross function boundaries without host-specific behavior.
- The bytecode verifier can prove constructor operands, field reads, and nominal identities before runtime execution.
- The runtime representation is immutable and bounded, but it allocates a small managed record object in Stage 0. This does not establish the future native memory model.
- WVB gains a seventh mandatory section. Golden hashes and both host reports must change together.
- Positional construction is compact for the bootstrap, but field reordering is an observable contract change.
- Nested aggregate graphs, mutation, record equality, patterns, methods, generics, and ABI layout remain outside Seed.

## Reconsider when

- A compiler or assembler data model needs nested records, recursive structures, variant payloads, or collections.
- Native lowering needs a stable ABI layout rather than VM-level nominal values.
- Allocation measurements show that the Stage 0 representation materially obstructs tool workloads.
- Named construction or destructuring would prevent recurring field-order defects.
