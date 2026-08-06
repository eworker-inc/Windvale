# Decision 0292: Bounded direct WebAssembly static descriptors

- Date: 2026-08-06
- Status: Implemented with focused Windows-local native, reference-runtime, and Node.js evidence
- Advances: [Decision 0278](0278-Native-WebAssembly-Artifact-Regeneration.md) and [Decision 0289](0289-Bounded-WebAssembly-Interpreter-Warmup.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The normal browser playground executes the 919,577-byte portable compiler as
a guest of the generated Windvale interpreter. Its exact first compilation
requires 1,183,292 guest instructions and 1,513,523,789 outer instructions,
which took 58.271 seconds after warmup on the measured Chromium host. The
direct native WebAssembly backend rejects that compiler in milliseconds, but
previously stopped at its first nonempty WVB section: static data.

The exact browser compiler has 104 immutable text/bytes declarations totaling
929 payload bytes and 220 `text.const` or `bytes.const` instructions. Execution
ABI 3 already reserves page zero. Bytes 50,176 through 65,535 were outside the
SHA scratch, reclaiming-allocation metadata, allocator roots, host input, and
guest output regions, leaving one bounded 15,360-byte static range without
changing memory size or the public ABI.

## Decision

- Admit at most 256 immutable WVB data declarations of exact kind `text` or
  `bytes`, with at most 15,360 concatenated payload bytes. Completely consume
  every name, kind, length, and payload extent before code selection.
- Require `text.const` and `bytes.const` to reference an in-range declaration
  of the corresponding kind during both one-function and descriptor-call graph
  validation.
- Concatenate payloads in canonical WVB declaration order into one active
  WebAssembly data segment beginning at byte 50,176. Lower each constant to an
  immutable packed `(pointer, length)` descriptor.
- Treat static descriptors as borrowed values. Their pointers are outside the
  reclaiming arena, so the existing reference helper ignores retain and release
  requests for them. No static literal is copied merely to establish ownership.
- Omit WebAssembly section 11 when the aggregate payload is empty, preserving
  earlier no-data output byte for byte.
- Reconstruct and pin both native `wvwasm` containers from source commit
  `6295c5e345afbefad873f8c04b98f32b5a15b417`. Stage 0 remains the explicit
  recovery constructor; normal use and WebAssembly publication remain .NET-free.

## Exact evidence

The retained two-function fixture builds to 364 WVB bytes with SHA-256
`6e606418a136b5e426279efd03f10c8ed640f752747b18777390703c5c946090`.
It combines static text `Windvale`, input bytes `[1, 2, 3]`, and static bytes
`[33, 42]` after exactly 26 Windvale instructions. The backend emits 2,339
import-free ABI-3 WebAssembly bytes with SHA-256
`2246c38d2cbc765271926c5f709e8a13cd062d82ee529e5a22dd346206a1772c`.
The reconstructed Windows native backend produced that identity in 77 ms.

The focused retained test additionally accepts the exact 15,360-byte boundary
and rejects 15,361 bytes, 257 declarations, a hostile oversized payload extent,
wrong-kind constants, and an out-of-range constant index. It passed in 1.635
seconds of test execution. The independent reference runtime, structural Wasm
decoder, deterministic repeat, and Node.js engine agree on the exact fixture
result and static bytes at offset 50,176. The normal manifest-verifying native
publisher produced the same module, and Node.js executed it as status zero,
output length 13, and 26 charged instructions.

The rebuilt backend lowers the unchanged 112,216-byte scalar-interpreter WVB in
502 ms to the existing 839,104-byte Wasm SHA-256
`f65c4e203d4b244ec52e0619f9d1a99ce1d2809296313cb154bba8316c6d916c`.
The result is byte-identical to the browser package, proving that the no-data
path retains its published identity.

The exact browser compiler now clears static-data validation and reaches the
next ordered module gate, its 82 nominal record/enum declarations. It returns
`Unsupportedˉmodule` in 38 ms and writes no output. This advances the measured
direct-lowering frontier; it does not claim that the compiler itself has been
lowered to WebAssembly.

The pinned artifact compiler WVB is 330,708 bytes with SHA-256
`7b8fc9ef7dd19ab545de47dc5dc84ea418794ffdf8f3f05aad57af3b611c7258`.
Its paired recovery containers are:

- Windows: 5,342,720 bytes, SHA-256
  `da586061cc569eb24255ca797fcb335044dc4e34ba8332fd019b407df8d3d187`;
- Linux: 5,341,184 bytes, SHA-256
  `87e8390e0a87df3a2225d14e031f4a200e4fa7a8a1f7857cf16d55536f312f4b`.

The Linux container is constructed and digest-pinned but has not received a
fresh independent Linux execution report in this slice.

## Consequences

The first direct-lowering blocker is removed without changing execution ABI 3,
fixed memory, host authority, browser packaging, or normal .NET-free operation.
Static literals are available without dynamic allocation or per-run copying,
which is also the cheaper ownership representation for eventual direct compiler
execution.

The browser still uses the interpreted compiler and therefore does not become
faster from this slice alone. The exact compiler still exceeds the direct
backend's nominal-type, sixteen-function, recursion, and general call-ordering
contracts. Nominal records/enums are now the immediate measured boundary; the
417-function graph and recursive/cyclic edges remain subsequent boundaries.

## Reconsider when

- page-zero scratch or allocator metadata needs any byte at or above 50,176;
- a supported compiler exceeds 256 declarations or 15,360 static bytes;
- mutable static storage is introduced and borrowed ownership is no longer exact;
- multiple segments materially reduce validation or publication cost without
  weakening deterministic layout; or
- direct compiler WebAssembly makes the interpreter-only warmup and package
  path obsolete.
