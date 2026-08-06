# Decision 0326: Static descriptor WebAssembly lowering

- Date: 2026-08-06
- Status: Implemented for immutable data, borrowed views, and bounded reads
- Advances: [Decision 0324](0324-Exact-Compiler-WebAssembly-Value-Layout.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The direct compiler path had exact Wasm `i64` layouts for `text` and `bytes`
but no memory or pointer/length representation behind those values. The exact
portable compiler has 104 immutable text declarations and relies heavily on
descriptor length, slicing, and little-endian reads. Continuing without a
bounded static-data owner would either make invalid descriptors executable or
duplicate the older monolithic WebAssembly compiler's repeated data scans.

Failure paths also previously produced an `i32` zero for every value-returning
function. That is invalid Wasm in a function returning an `i64` descriptor and
would surface first on metering, checked arithmetic, or a failed descriptor
range inside a descriptor-returning helper.

## Decision

- Add focused portable `WebAssembly-Static-Data.wv`. Parse the WVB data section
  once, admit at most 256 text or byte declarations, cap concatenated payload
  at 15,360 bytes, and retain a compact declaration directory plus the exact
  immutable payload.
- Preserve the established static base of 50,176 and encode descriptors as one
  Wasm `i64`: pointer in the low 32 bits and length in the high 32 bits.
- Emit one fixed 129-page Wasm memory and one active static data segment when
  declarations exist. Static descriptors borrow this memory and require no
  allocation or reclamation.
- Add focused portable `WebAssembly-Descriptor-Operations.wv` for text and byte
  constants, `text.to_utf8`, byte length, borrowed slicing, and little-endian
  u8, u16, u32, and i32 reads.
- Validate slice and read ranges before address arithmetic or memory access.
  Publish status `3008` and return the correctly typed zero value instead of
  permitting a Wasm memory trap.
- Make all existing metering, scalar-failure, and call-propagation paths emit
  `i64.const 0` when their current function returns a descriptor. Scalar
  functions retain the byte-identical `i32` failure encoding.
- Keep concatenation, construction, UTF-8 validation, dynamic allocation,
  ownership, nominal storage, root `bytes -> bytes` transport, and call-depth
  containment fail-closed for later slices.

## Focused evidence

The dispatcher memory tool is WVB SHA-256
`ffd7db5f1944e9f17c43cc3d3fbd1c877fd5bccf74b56943c26a0b564936711b`.
The previous scalar fixture still emits byte-identical 5,881-byte Wasm with
SHA-256
`3f90a6641648ae55a3b4ddf3a50ae2d2ad7d52ae434bf15c1718076f04232e79`.

The descriptor fixture has WVB SHA-256
`0ee2ac5b3f0e71bbfb4b941df632f873ada08e3075056b4e3068a881da3b1ebc`.
It emits deterministic 4,199-byte Wasm with SHA-256
`3da1365714f5320376855efb4f233f39f9eb2861e4aa33a526fe35965f27833d`.
Node 24.18 validates it and returns result 42 in exactly 109 WVB
instructions after exercising both static kinds, `text.to_utf8`, length,
borrowed slice, and every admitted read width.

The descriptor-returning range-failure fixture has WVB/Wasm SHA-256 values
`0c75aa907c458f10c5add7a76bdf8b9dda43cb4aa1f1569dfb6580e50f9e8a81`
and
`1abaff8dd657fb0bf33d48ffde73572a5d1c10e343c65ac680926b004ccd4eea`.
Its 1,454-byte module validates in Node and reports `3008`, result zero, and 14
instructions without trapping.

The single focused Seed contract passes in 1.811 test seconds after its
incremental compile. No broad Seed, changed-file, Standard, Qualification, or
full WebAssembly-engine verifier was run.

## Consequences

The direct path now gives the exact compiler's immutable text constants valid
bounded memory and can execute its non-allocating descriptor views and reads.
This removes another interpreter-only semantic family while retaining exact
instruction accounting and byte-identical scalar output.

The exact compiler is not yet directly executable. Its allocation-heavy byte
construction and concatenation, UTF-8 validation, nominal records and enums,
recursive call-depth budget, and public browser transport remain pending.

## Reconsider when

- execution ABI 3 assigns a different static or public memory region;
- descriptors require flags or ownership metadata beyond pointer and length;
- multi-memory becomes a useful separation between immutable and dynamic data;
  or
- the exact compiler artifact supersedes the focused descriptor fixtures.
