# Decision 0104: WebAssembly checked addition and execution ABI

- Date: 2026-08-02
- Status: Implemented with Windows development and Node.js engine evidence; cross-host and browser-worker qualification pending
- Extends: [Decision 0102](0102-First-Windvale-WebAssembly-Backend-Slice.md)

## Context

The first Windvale-authored WebAssembly slice emits a direct `Main() -> i32` constant and therefore cannot expose the difference between WebAssembly's wrapping integer operations and Windvale's checked arithmetic. Windvale requires deterministic `WVR3007` overflow behavior and charges an instruction before attempting it. A WebAssembly engine trap is not a suitable substitute: integer addition does not trap in WebAssembly, and an unrelated engine trap would lose Windvale's status identity and instruction evidence.

A browser host will also need a stable way to distinguish a program result from a Windvale runtime failure. Returning only `Main`'s `i32` cannot represent both without reserving valid language values. Imports or linear memory would add capability and memory contracts before they are otherwise required.

## Decision

- Add exact WVB profile 2 for the current compiler-produced `Main() -> i32` shape `return <left> + <right>`, including its three synthesized locals and ten verified instructions.
- Continue to implement the selector and encoder in portable Windvale source. C# remains the Stage 0 compiler, reference runtime, and independent structural oracle rather than the WebAssembly emitter.
- Define execution ABI 1 as an import-free function/global boundary:
  - immutable global `Windvale.abi` is `1`;
  - function `Windvale.run() -> i32` returns `0` or Windvale status `3007`;
  - mutable global `Windvale.result` contains the result only on status zero; and
  - mutable global `Windvale.instructions` contains the exact attempted WVB instruction count.
- Reset result and instruction globals before every run.
- Detect signed addition overflow in generated WebAssembly with the wrapped-sum identity `((left xor sum) and (right xor sum)) < 0`.
- On overflow, publish seven attempted instructions and status `3007` without using an engine trap. On success, publish the sum, ten instructions, and status zero.
- Preserve profile 1's exact 37-byte direct-`Main` output and use execution ABI 1 only for profile 2.
- Keep calls, source control flow, memory, browser capabilities, and arbitrary WVB instruction streams outside this revision.

## Consequences

Windvale now has a direct WebAssembly path that preserves one observable trap semantic rather than only successful values. The explicit status/result/counter boundary can be consumed by a future playground worker without teaching browser code how the overflow probe is implemented. It also gives later operations one versioned seam through which to report Windvale failures.

The ABI currently uses an `i32` instruction global because profile 2 can report only seven or ten instructions. A future profile that can exceed the signed 32-bit range must revise the ABI or introduce another bounded representation before widening the accepted code.

The result and instruction globals are mutable because the generated function writes them and WebAssembly export mutability is shared with the host. The host adapter is trusted not to rewrite evidence after `Windvale.run`; the function resets both globals so host changes before a run cannot affect its reported result.

Profile selection remains deliberately exact. This establishes checked-code generation and host-visible failure evidence, but it is not yet a general stack-machine-to-structured-WebAssembly translator.

## Evidence

The successful fixture WVB has SHA-256 `54fccbb837dc47dad0f40dca1356d046dd9beb6dab13a3a2574b867791e10466`. Its 176-byte WebAssembly output has SHA-256 `4057797732dd7250413f44aa71e012222591ae7e219e27a7680f246b2cedeb8a`.

The overflow fixture WVB has SHA-256 `fbba878513eabf1d8c47fdbab887f314117a8ee5184c42a23edc94190926a583`. Its 176-byte WebAssembly output has SHA-256 `984139ccb136981e4d6382e4c547012be13df38af056cd09abebec10cc1a6f52`.

Node.js 24.18.0 validates and instantiates both modules. The successful module reports ABI `1`, status `0`, result `2147483647`, and ten instructions. The overflow module reports ABI `1`, status `3007`, result `0`, and seven instructions without throwing a WebAssembly exception.

Focused tests compare the complete successful module bytes and both output digests, parse every section and instruction independently from the `.wv` encoder, compare success and overflow status/result/count tuples with the reference runtime, cover both signed extrema and mixed-sign cases, repeat output generation, and reject a substituted WVB arithmetic opcode without publishing output.

The current Windvale core WVB has SHA-256 `aa5086df27c993ec76d92b7680517c60777936a55c1e00644ad03d736ddd2f9f`; the hosted tool WVB has SHA-256 `1ef6274dd7a7188464c7cd7c2fdb2ed71656a0d901c9d8c9aa6535f7ebe738bd`; and the portable encoder demo WVB has SHA-256 `0da57f8ae5f3dfc420d1bd57286bf77f5012a71ee8121cfd09b8fcb05c5e0588`.

This is Windows development evidence and an independently identified engine run. It is not Windows/Linux byte equality or playground-worker qualification.

## Rejected alternatives

Returning a wrapped result was rejected because it contradicts Windvale checked arithmetic.

Using `unreachable` or another WebAssembly trap was rejected because it would not preserve `WVR3007` or the exact failing instruction count.

Reserving an `i32` return value for failure was rejected because every `i32` is a valid `Main` result.

Precomputing whether the two constants overflow was rejected because it would test selector arithmetic rather than generated checked execution.

Adding a host import for trap publication was deferred because pure arithmetic needs no capability and the import would prevent standalone deterministic execution.

## Reconsider when

- A general instruction-stream lowerer shows that mutable exported globals cannot preserve results and counters coherently.
- Multiple return types require a typed result layout or bounded linear-memory record.
- Instruction counts can exceed the execution ABI 1 `i32` representation.
- Browser-worker containment or another conforming engine exposes a portability issue in the selected instructions or exports.
