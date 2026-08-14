# Decision 0328: Direct enum WebAssembly lowering

- Date: 2026-08-06
- Status: Implemented for constants, comparisons, names, calls, and local defaults
- Advances: [Decision 0326](0326-Static-Descriptor-WebAssembly-Lowering.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The exact portable compiler reaches 3,394 enum operations: 2,414 constants,
632 equal comparisons, 346 not-equal comparisons, and two name lookups. WVB
enum values are nominal, have unique member values within each type, pass
through locals and calls, and produce their exact member spelling through
`Enumˉname`.

Direct Wasm does not need the original signed backing value as an observable
integer because WVB provides no enum-to-integer conversion. It does need a
stable nominal identity, deterministic default locals, and a bounded way to
resolve names without a host import or dynamic allocation.

## Decision

- Add focused portable `WebAssembly-Enum-Operations.wv` as the owner of enum
  metadata and lowering.
- Assign every verified enum member one deterministic nonzero `i32` handle in
  canonical WVB type/member order. `enum.const` emits that handle; equality and
  inequality lower to direct Wasm integer comparisons. The verifier already
  excludes cross-type comparisons and duplicate member values.
- Append one eight-byte packed descriptor per enum member followed by exact
  UTF-8 member names to the bounded immutable static-data payload. `enum.name`
  validates its handle, computes one table address, and performs one Wasm
  `i64.load`; it does not allocate or call a host service.
- Retain the combined immutable payload limit of 15,360 bytes. The exact
  compiler has 23 enums, 452 members, 5,429 name bytes, and 929 ordinary data
  bytes. Metadata plus all names and data occupy 9,974 bytes.
- Emit explicit function-prologue stores for enum locals so Wasm zero defaults
  become WVB's first-declared-member defaults. These stores are target setup,
  not semantic WVB instructions, and therefore do not alter exact instruction
  metering.
- Treat zero or out-of-range enum handles as status `3015` before a metadata
  load. Verified ordinary execution cannot construct such a handle; the guard
  remains an independent target-memory boundary.
- Keep records, variants, sequences, builders, dynamic text/byte construction,
  recursion containment, and public `bytes -> bytes` transport fail-closed.

## Focused evidence

The dispatcher memory tool is WVB SHA-256
`87868a95c5f259c3f7a0e1ba842cbf2bab2ec2204cbc46dba576d73623d72d03`.
The prior scalar and descriptor outputs retain their exact golden bytes and
hashes in the same focused regression.

The reused enum fixture has WVB SHA-256
`5966408913655f233509a0ab6f6c80dcc9d893d2ad22844a35661fa486527a30`.
It passes an enum through a typed helper, exercises both comparison directions,
resolves `Windy`, converts the borrowed text descriptor to bytes, reads its
first byte, and returns 42. The emitted 2,558-byte Wasm has SHA-256
`be287ba7ac40d001d3c3fe91796b15b09505614555004b4870d0a51eb6c6c4f8`.
Node 24.18 validates it and reports status zero, result 42, and exactly 64 WVB
instructions.

The independent test decoder checks fixed 129-page memory plus all three packed
member descriptors and exact `Calm`, `Windy`, and `Storm` name bytes. The one
focused Seed contract passes in 2.044 test seconds after its incremental
compile. No broad Seed, changed-file, Standard, Qualification, or full
WebAssembly-engine verifier was run.

## Consequences

All enum operations reachable from the exact portable compiler now have a
direct import-free Wasm lowering. Enum metadata shares the same immutable
memory owner as text and byte constants, so name lookup adds no allocator and
preserves descriptor representation across helper calls.

The compiler is still not directly executable. Records account for the next
large nominal boundary, followed by allocation-heavy bytes/text operations,
UTF-8 validation, recursive call-depth containment, and root browser transport.

## Reconsider when

- WVB exposes enum backing values as integers;
- imported or mutable enum values require a public validation contract;
- enum metadata no longer fits the bounded static payload; or
- a shared nominal table should also own record and variant descriptors.
