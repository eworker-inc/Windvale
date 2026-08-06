# Decision 0324: Exact compiler WebAssembly value layout

- Date: 2026-08-06
- Status: Implemented for all exact compiler signatures and locals
- Advances: [Decision 0323](0323-Complete-Compiler-U32-WebAssembly-Operations.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The direct dispatcher previously represented every admitted value as Wasm
`i32`. That is sufficient for primitive scalars and enum backing values but not
for the portable compiler's `text` and `bytes` descriptors or its wide scalar
types. Emitting descriptor operations before fixing function and local layouts
would either corrupt local ordinals or split the compiler across incompatible
calling conventions.

WebAssembly groups local declarations by type, while WVB local indices follow
source order and may alternate value classes. The layout must preserve every
WVB parameter and local ordinal so existing verified instructions and call
signatures remain authoritative.

## Decision

- Add focused portable `WebAssembly-Value-Layout.wv` as the canonical mapping
  from WVB 1.11 value shapes to direct Wasm value types.
- Represent `text`, `bytes`, `i64`, and `u64` as Wasm `i64`. Descriptor payload
  and wide-scalar semantics remain distinct even though they share the machine
  value type.
- Represent `i32`, `bool`, `u8`, `u32`, enum backing values, and bounded record,
  variant, sequence, and builder handles as Wasm `i32`.
- Emit exact mixed-parameter function types in declared order and an exact
  zero- or one-result type. Reject unknown or void non-result shapes.
- Emit local declaration runs in original WVB order. Merge only adjacent equal
  Wasm types, so the expanded local index remains identical to the WVB index.
  Append four private `i32` dispatcher/arithmetic scratch locals followed by
  one private `i64` descriptor/wide scratch local.
- Make the dispatcher consume this owner for every reachable function type and
  local declaration. Descriptor-returning calls spill through the private
  `i64` scratch local before checking shared failure status; scalar and handle
  calls retain the private `i32` scratch path.
- Do not yet assign pointer/length encoding, linear-memory regions, allocation,
  ownership, record backing, or root `bytes -> bytes` transport. This decision
  fixes representation and ordinal layout, not descriptor execution semantics.

## Focused evidence

The capability-free value-layout adapter is WVB SHA-256
`7cbfe268c02608fe2c1b066fd51dfb809002f9b25cbe1050a4806291b7125a3d`.
It reads the exact 920 KiB compiler WVB
`2bf84dc2a8cbb80c52ec7fb6cb2e29eef27def1707f398a276c61063d73df06e`
and emits one deterministic type/local entry for each of its 417 functions.
The complete response has SHA-256
`bc7099df2ba2525ab28f26c7b891ba71e4d24eddbdca2199010fdbb0e817552d`.

The independent C# model reconstructs every function type, every run-length
local declaration, and all per-function `i32`/`i64` counters from the verified
module. All 417 entries agree, a repeated Windvale run reproduces both response
bytes and instruction count, and the focused test passes in 4.496 seconds.

The integrated dispatcher memory tool advances to WVB SHA-256
`051c8eee830ca875e246330352633de3f44f3cece41ee64a06a5b219cf62c63c`.
The previously proven scalar/control output remains byte-identical at 5,881
bytes and SHA-256
`3f90a6641648ae55a3b4ddf3a50ae2d2ad7d52ae434bf15c1718076f04232e79`;
its focused regression contract passes in 1.013 seconds. No broad Seed or
WebAssembly-engine verifier was run.

## Consequences

Every exact compiler function now has a deterministic direct-Wasm signature
and local layout, including descriptor and nominal-bearing functions. The next
descriptor slice can emit `text`/`bytes` constants, views, reads, slices, and
calls without changing function types or renumbering locals.

The representation deliberately does not claim that an `i32` handle is a valid
record, variant, or collection object, or that an `i64` descriptor names valid
memory. Those values remain fail-closed until their storage owners and bounds
checks are emitted. Root transport, call-depth containment, static data, dynamic
allocation, and browser replacement are still pending.

## Reconsider when

- multi-value Wasm becomes preferable to packed descriptors;
- a nominal value requires a representation wider than one bounded handle;
- descriptor ownership requires additional per-function scratch values;
- execution ABI 3 fixes a different public memory layout; or
- exact compiler emission makes the standalone layout adapter redundant.
