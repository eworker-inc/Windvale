# Decision 0938: lower authenticated WVB 1.38 Foreign calls through the native x64 ABI

## Status

Accepted implementation checkpoint on 2026-09-03. The Windvale-written native
x64 lowerer now verifies and emits the exact registered WVB 1.38 Foreign call,
and the focused current-source owner passes on Windows and Debian. This decision
does not admit general FFI, migrate a real runtime or OS boundary, complete
Slice 8, or claim final paired-host qualification.

## Context

[Decision 0937](0937-Execute-Authenticated-WVB-1.38-Foreign-Calls-In-The-Bounded-Scalar-Provider.md)
executed binding `1` through a bounded scalar provider without forming a host
address. The next checkpoint must connect the same verified affine pointer to
machine code while preserving exact target, symbol, geometry, ownership, and
failure boundaries.

A native implementation cannot treat the pointer record as ambient address
authority. The lowerer must construct the backing inside its own bounded frame,
prove the selected region before deriving an address, require immediate affine
consumption, and publish only a typed relocation to a linker-resolved symbol.

## Decision

1. Admit WVB 1.38 only for the exact System target
   `linux.x86_64.sysv_amd64_c_v1`. Other targets and older WVB minors cannot use
   opcode `E0`.
2. Reserve four 16-byte backing cells only in a function that contains `E0`.
   Scratch construction records the private frame address, write-region
   validation records the checked start-adjusted address, and pointer derivation
   retains the exact writable length. No address may escape through a local
   copy, ordinary call, return, or retained aggregate.
3. Require binding `1`, the canonical write-pointer and ABI identities, and the
   compiler-generated `pointer`, `capacity`, `generation`, `E0` sequence. Reject
   a null pointer handle, null address, length/capacity disagreement, capacity
   other than 64, or address not aligned to eight bytes before calling.
4. Pass the checked address, capacity, and generation through SysV `RDI`, `RSI`,
   and `RDX`. Align helper stacks for the call and store the signed `i64` result
   from `RAX` in an ordinary wide scalar cell.
5. Emit one imported function symbol named
   `wv_paper_buffer_source_read_v1` and one ordered `Relative_i32` relocation
   with addend `-4` for each `E0`. Symbol resolution remains a linker operation;
   the lowerer does not load a dynamic library or grant ambient authority.
6. Keep measurement and emission exact. Scratch construction is 316 bytes,
   write-region construction is 363 bytes, pointer derivation is 61 bytes, and
   `E0` is 101 bytes in `Main` or 109 bytes in a helper, including the common
   instruction-budget charge.
7. Add the bounded native `i64` subset required by the registered result:
   constants, negation, signed comparison, local movement, direct calls, and
   returns. General `i64` arithmetic and formatting remain later work.
8. Promote the current lowerer as a 747,242-byte WVB and reconstruct
   byte-identical Windows and Linux application wrappers from the same staged
   native image. Retain the previous Return-42 and metadata fixture bytes.
9. Exercise success and stale-generation paths with a conforming test provider,
   inspect the exact import and relocation, link both applications, and reject
   malformed target, opcode, binding, nominal identity, ABI, stack-kind, and
   affine-order inputs on both permanent hosts.
10. Refresh the paired WVB runner from the same WVB 1.38-capable source state so
    retained launchers do not preserve the earlier WVB 1.37 execution ceiling.
    One canonical staged image must reproduce both host wrappers.
11. Refresh the segmented WVO staging producer from the same lowerer source set.
    Retain the compiler-image staging and canonical-transport families byte for
    byte when their declared inputs do not change.

## Implementation standing

The focused owner contains 25 cases: five valid cases and twenty malformed or
containment cases. Windows reconstructs the current lowerer, checks and links
both registered objects, and retains the existing native pointer execution.
Debian independently reconstructs the same lowerer WVB and object identities,
then executes the pointer case plus both registered Foreign outcomes to result
`42`.

The promoted lowerer identities are:

- WVB: 747,242 bytes, SHA-256
  `a1cac3efb911dcb20c50311a8638c824da259e99c3223a804f1790beefbfbe48`;
- Windows x64 application: 10,656,768 bytes, SHA-256
  `24737454d3c03a979153ad99a56808462c0635cedd647caecde51c4dcc63ff15`;
- Linux x64 application: 10,657,792 bytes, SHA-256
  `8ff82a18b567655c1133b11a1d6395b29e584d871622abb9af04a42927604ad1`.

The refreshed runner identities are:

- WVB: 1,040,878 bytes, SHA-256
  `4e50301efe5e2260608eb994f21ece89e83ad102aac28cebb705d35d06e3d86b`;
- Windows x64 application: 10,547,712 bytes, SHA-256
  `8942e7c0a17182ff15ed79eaf63f7aeb8a8ab7cd4cde5015cd489612c3494972`;
- Linux x64 application: 10,547,200 bytes, SHA-256
  `4b9f7ea9eb30e4aa9b713f17d6065a413b4585a27eb8c62e021e95375e27436e`.

The refreshed segmented WVO staging identities are:

- WVB: 774,524 bytes, SHA-256
  `3f6c792c0318d44e51e3969862bd1ab245b9406ee92f4c8e363a0bd11d665dae`;
- Windows x64 application: 11,184,128 bytes, SHA-256
  `dc508c5d568fe3aaaebd4dbc028569f5758b4f77bb1048fe0ad04d1453833bf1`;
- Linux x64 application: 11,186,176 bytes, SHA-256
  `3f7d3cec2cbff7a796cd88563e7f8827147e90a3419389b98db519fdf8de9eee`.
The other six segmented compiler-toolset artifacts remain byte-identical.

Its segmented staging object is 10,547,710 bytes across fifteen chunks. The
linked image is 10,529,580 bytes with entry offset 150,541 across eleven chunks;
canonical transport retains three chunks.

The promotion verification run also exposed a stale Seed-plan oracle after the
source-built verifier began reporting exact semantic substeps. WVNT 6 now pins
those five already-implemented diagnostics rather than weakening comparison or
changing verifier behavior.

## Consequences

- The verified WVB 1.38 binding now reaches real x64 machine code, a typed WVO
  import, linker resolution, and external provider execution without opening a
  general native loader.
- The promoted lowerer candidate contains the current WVB 1.38 implementation
  instead of leaving it available only through development reconstruction.
- The retained WVB runner candidate now contains the same WVB 1.38 scalar
  provider source state instead of preserving a WVB 1.37 execution ceiling.
- The test provider is a conformance shim, not the required real migrated
  boundary. Slice 8 still requires one real runtime or OS consumer and final
  exact-source Windows/Linux qualification.
- Browser, WebAssembly, other native targets, arbitrary symbols, retained
  pointers, other bindings, and general FFI remain closed.

## Reconsideration triggers

Revisit this decision before adding another binding or target, changing the
64-byte geometry, retaining a pointer beyond the call, accepting an indirect or
runtime-selected symbol, adding dynamic-library loading, or changing the
provider's partial-progress semantics. Any replacement must retain verification
before address formation, exact target and symbol identity, affine non-escape,
bounded resource use, deterministic reconstruction, and explicit failure.
