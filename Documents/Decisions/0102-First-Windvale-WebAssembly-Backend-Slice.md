# Decision 0102: First Windvale-authored WebAssembly backend slice

- Date: 2026-08-02
- Status: Implemented; extended by [Decision 0104](0104-WebAssembly-Checked-Addition-And-Execution-Contract.md); cross-host and browser qualification pending
- Extends: [WebAssembly and browser playground exploration](../Project/WebAssembly-Playground-Exploration.md)

## Context

The Stage 0 playground runs the C# reference compiler, verifier, and interpreter through .NET WebAssembly. That proves the browser host but not direct Windvale-to-WebAssembly lowering or Windvale ownership of the implementation. The complete Windvale compiler already reproduces canonical WVB, and Windvale-written binary tools already construct deterministic WVB, WVO, and linked image bytes. A bounded direct backend can therefore begin in `.wv` without waiting for complete native compiler execution or making WebAssembly a permanent target.

Beginning with the complete compiler, a general interpreter, linear memory, browser capabilities, or arbitrary WVB control flow would combine several unresolved contracts before any WebAssembly-owned byte was independently testable. One constant-return function is sufficient to prove the implementation language, untrusted-input boundary, binary encoding, signed LEB128 behavior, exact artifact identity, and interpreter/WebAssembly result seam.

## Decision

- Define experimental target `wasm32-browser-v1-experimental` without accepting WebAssembly as a permanent target.
- Keep canonical WVB 1.6 as the portable input identity.
- Implement the encoder and selector in portable Windvale source under `Compiler/Windvale/WebAssembly-Core.wv`.
- Accept only one exact compiler-produced WVB shape: portable exported `Main() -> i32` returning one constant through its synthesized return local.
- Revalidate the complete WVB envelope and every selected field before emission even though the permanent execution path still requires independent WVB verification.
- Emit a deterministic import-free, memory-free WebAssembly version-1 module containing one type, function, export, and body.
- Use minimal unsigned and signed LEB128 encodings, including all five possible signed `i32` widths.
- Keep the hosted argument/file shell separate from the portable encoder. Failed selection publishes no bytes.
- Retain C# only as the Stage 0 compiler, runtime host, and independent test oracle for this slice. It does not implement the production encoder.
- Require a conforming WebAssembly engine result before claiming generated execution, and require real browser-worker evidence before integrating the path into the public playground.

## Consequences

Windvale now owns the first direct WebAssembly artifact construction path in `.wv`. The implementation can be compiled to canonical WVB and run under the current reference runtime today; a later qualified native or WebAssembly execution path can run the same portable backend without changing its input or output contract.

This slice deliberately does not share x86-64 machine IR. WebAssembly structured control flow, linear memory, and host imports differ from the native object and ABI contracts. The portable semantic source remains verified WVB, while target-specific lowering remains behind its own explicit boundary.

The first generated module is 37 bytes, has SHA-256 `1b62162dbc97b579c02834e9623e3ac9eccc7bc444e4b48a9e4d6c39b77ea3f1`, validates under the Node.js 24.18.0 WebAssembly engine, and returns `42`, matching the reference runtime. At this profile-1 checkpoint, the Windvale core WVB had SHA-256 `86e68b7a5874c5d10c1948711a67a0d52af1c2e45db48c428d5d0a0741f53271`; the hosted tool WVB had SHA-256 `e109e22d0922cc18cec4aebfb096d501e4b6548989871c81e57f96906626f067`; and the portable encoder demo WVB had SHA-256 `d5f70a9d44b0311b6c256858a1405fecfdfd3b098067794514df3d5bd31b8b32`. Decision 0104 records their identities after the checked-addition extension.

Focused tests compare exact bytes and digests, independently parse the complete emitted structure, compare results over every signed-LEB width including `int.MinValue` and `int.MaxValue`, repeat the build, and reject truncated, oversized, inconsistent, hosted, and wider-code inputs without output. This is Windows development evidence, not cross-host qualification.

## Rejected alternatives

A C# WebAssembly backend was rejected as the primary implementation because Windvale already has enough portable byte construction and compiler ownership to implement this bounded encoder itself. C# remains useful as an independent oracle.

A complete Windvale-written interpreter compiled to WebAssembly was deferred because the current Windvale interpreter is a bounded OS profile rather than a general runtime, and the required memory and browser-service boundaries remain open.

Direct source-to-WebAssembly lowering was deferred because it would couple the first binary proof to the complete frontend and make canonical WVB differential evidence less direct.

Hard-coding one 37-byte module was rejected because it would not prove target-owned section sizing, general signed constants, or reusable deterministic encoding.

## Reconsider when

- Checked arithmetic cannot preserve Windvale trap and resource semantics without a materially different ABI.
- A typed WIR input proves safer or substantially simpler than verified WVB for general structured lowering.
- Browser engine constraints require a different import, result, or memory boundary.
- Measured encoder execution shows that immutable byte construction cannot support the next bounded profile.
