# Decision 0603: First bounded database schemas and typed rows

- Date: 2026-08-16
- Status: Implemented candidate with focused Windows native evidence
- Advances: [Windvale Database proposal](../Project/Windvale-Database-Proposal.md)
- Defines: [`WVSC 1` and `WVTR 1`](../../Specifications/Windvale-Database-Typed-Rows-And-Schemas.md)
- Extends: [`WVKR 1`](../../Specifications/Windvale-Database-Logical-Records.md) with schema key kind `3`

## Context

The logical record layer persisted a nonzero schema identity but treated its
payload as opaque. The collection catalog named a primary schema but no schema
body existed. JSON, SQL, indexes, and E-Worker migration cannot share one
correct query boundary while application fields remain unvalidated bytes.

## Decision

- Persist each schema under a collection-scoped `WVKR 1` schema key, separate
  from catalog and application record keys.
- Admit 1 through 64 ordered, bytewise-unique UTF-8 field names.
- Start with Boolean, I64, U64, UTF-8 text, and bytes. Every field has exact
  nullability and a canonical maximum byte count.
- Encode one typed value per declared field in exact schema order. Missing,
  extra, differently typed, oversized, malformed, or forbidden-null values
  reject the whole row.
- Keep a complete typed row within the existing 61,408-byte `WVRD 1` payload
  ceiling.
- Validate schemas and rows without storage authority. Typed put preparation
  may produce logical tree bytes only after full schema and row validation.
- Keep the row hot path allocation-light by scanning field shapes without
  copying field names or descriptors.
- Retain exact encoded bytes and little-endian widths so future JSON and SQL
  front ends lower to this one typed boundary.

## Evidence

The focused project builds through the Windvale compiler and native x64
lowerer. Two builds produced byte-identical 65,476-byte WVB files with SHA-256
`55187a43bf0ba2231bfe059ff5471ecf99c9cc885464d7259f0fb4faf6ac1c2b` and
byte-identical 857,787-byte WVO files with SHA-256
`f369519bea535788535528bd87a904011341657359ef45fd01e1e9fa283d2495`.
Its Windows hosted application exits zero after exact-format,
determinism, operation, maximum-boundary, and malformed-input cases. The
fixture includes a 64-field schema and a maximum 61,408-byte row.

This is focused Windows evidence. Independent Linux execution and broader
qualification remain required before a cross-host conformance claim.

## Consequences

Records can now be rejected before mutation when their field count, kind,
nullability, scalar width, UTF-8, or byte limit differs from the persisted
schema. Schema definitions can be stored durably without colliding with record
identities. The strict JSON protocol and parameterized SQL front end can now
target a concrete shared representation.

The first format deliberately omits defaults, nested values, arrays, decimals,
floating point, migration, and compatibility coercion. Adding any of those
requires explicit deterministic encoding, bounds, and query semantics.

## Reconsideration triggers

Revisit this decision when a real E-Worker migration requires another scalar
kind, when schema evolution needs compatibility rules, or when measured row
validation shows that the exact representation or scan strategy is a material
performance bottleneck.
