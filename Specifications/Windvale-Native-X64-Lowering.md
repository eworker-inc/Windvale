# Windvale-native x86-64 lowering

## Status and purpose

`Compilerˉnativeˉx64ˉlowering` is the first portable Windvale-written slice of the shared x86-64 backend. It consumes one bounded WVB 1.6 module, independently verifies the metered scalar-control subset described below, and emits the canonical WVO 1.0 object used by the existing ABI-22 Stage 0 backend.

This is algorithmic machine-byte selection, not a lowering plan, private intermediate format, or collection of whole-program stencils. C# remains the normal complete WVB-to-native backend, native fragment verifier, linker/package constructor, and recovery oracle.

## Public result

```text
Compilerˉlowerˉwvbˉnativeˉx64(Input: bytes)
    -> Compilerˉnativeˉx64ˉsummary
```

The summary contains a stable status, WVO object bytes on success, a reserved zero `Value` field, native ABI `22`, and the computed code size. Failure returns an empty object and never publishes a partial WVO.

The status vocabulary is:

| Status | Meaning |
| --- | --- |
| `Valid` | A complete canonical WVO was emitted. |
| `Invalidˉwvb` | Header, section envelope, length, order, trailing bytes, or module identity is malformed. |
| `Unsupportedˉprofile` | The module is not portable. |
| `Unsupportedˉmodule` | Capabilities, static data, or nominal types are present. |
| `Unsupportedˉfunction` | The function or export shape is outside the first slice. |
| `Unsupportedˉcode` | The instruction stream is outside the first slice. |
| `Outputˉlimit` | Projected or emitted code/object bytes exceed or disagree with the bounded layout. |

## Accepted WVB subset

The core accepts exactly:

- WVB 1.6 with seven canonical sections, no trailing bytes, and a valid Seed module identifier;
- portable profile, no capabilities, no data, and no nominal types;
- one exported `Main() -> i32`, no parameters, zero through 1,024 `i32` or `bool` locals, a declared maximum stack depth from one through 1,024, and at most 8,192 code bytes;
- one control-flow graph of at most 1,024 instructions drawn from `i32.const`, `bool.const`, `local.load`, `local.store`, checked `i32.add`, `i32.subtract`, `i32.multiply`, `i32.negate`, all six signed `i32` comparisons, `bool.equal`, `bool.not_equal`, `bool.not`, `jump`, `branch.false`, and `return`; and
- instruction-aligned forward or backward targets, empty stacks at every block edge, complete fixed-point reachability from entry, valid typed local uses and stack effects, an exact declared maximum depth, and a combined local/value frame of at most 2,048 ABI cells. Locals retain WVB's zero-initialized entry semantics.

Every read is preceded by a checked range or exact payload-size test. Unknown versions, reordered/truncated/extended sections, inconsistent lengths, alternate function metadata, unknown opcodes, invalid local indices, type or stack mismatches, malformed targets, unreachable blocks or cycles, and mismatched maximum-stack declarations fail closed. Calls, data references, capabilities, parameters, and nominal identities remain outside the subset. Every emitted WVB instruction, including every backward jump, executes the existing ABI-22 instruction charge before its operation; a nonterminating loop therefore reaches the bounded instruction-limit status rather than escaping resource accounting.

## Selected object

The output is one standard WVO 1.0 x86-64 object:

- one 16-byte-aligned `.text` section containing the computed code bytes;
- one exported `Main` function symbol spanning the complete section; and
- no relocations or platform imports.

The selector assigns one deterministic 16-byte ABI cell to each local. It reuses separately typed `i32` and `bool` value cells at empty-stack block boundaries exactly as Stage 0 does. It emits the shared execution-context prologue and zeroed frame, charges every WVB instruction, selects checked scalar and comparison operations, balances call-depth state on success and failure, patches signed forward or backward jump and conditional-branch displacements from computed block offsets, and appends the canonical trap-status tails. The same bytes therefore link under the existing Windows and Linux consumers without introducing a host-specific selector.

The emitted layout is versioned by this contract and must change whenever its ABI, target, metering, trap, frame, or verification contract changes. A changed complete backend is not silently accepted because it happens to return the same scalar.

## Adapters

`Native-X64-Lowering-Memory-Adapter.wv` exposes `Main(Input: bytes) -> bytes` and returns either the complete WVO or empty bytes. `Native-X64-Lowering-Tool.wv` is the hosted shell:

```text
wvnative <input.wvb> <output.wvo>
```

It reads the input once, calls the portable core in memory, writes exactly once only after success, and reports the ABI and exact output sizes. Invalid or unsupported input produces a deterministic diagnostic and no output call. The checked-in Project 1 manifests build both adapters as WVB.

## Conformance and limits

The existing shared-backend conformance case compiles the Windvale modules once and compares produced WVO bytes with Stage 0 across constants, each checked arithmetic operator, nested expressions, both scalar types, all comparison operations, boolean negation, both conditional routes, forward and backward jumps, loops, and early returns. The combined arithmetic oracle is exactly 1,871 code bytes and a 1,944-byte WVO. The retained nested-control oracle is exactly 4,835 code bytes and a 4,908-byte WVO. The metered loop oracle is exactly 1,665 code bytes and a 1,738-byte WVO; native execution succeeds at its exact 157-instruction requirement and reaches `WVR3011` at 156. The same test executes constant, arithmetic, control, and loop inputs through the hosted lowering shell as native x86-64; exercises memory and hosted adapters over malformed stack, invalid-local, invalid-target, and unreachable-cycle streams; and verifies that truncated input leaves a sentinel output unchanged. The same generated tool fragment is host-neutral; current-host evidence is not a Windows/Linux qualification claim.

The pinned WVB identities are `d4df72b19fa1222cfffa32e87de798b5073c24b7b2037c3ed2711799e006303d` for the core, `6b06e9c9ceb10ebecee11d1d6533d9586e99cac04d407c602bbc29821770f8ab` for the memory adapter, and `05c379c6d09b4eadb3b7db68212db42c51e2762b36d5fc453b81998398c92e0d` for the hosted tool. The current hosted-tool WVB lowers through the complete Stage 0 backend to 900,877 code bytes and a 903,093-byte WVO; those sizes are evidence, not a permanent optimization promise.

This slice does not yet transfer calls, multiple functions, data, descriptors, capabilities, relocations, fragment verification, W^X publication, PE/ELF construction, or the complete compiler. It does not satisfy the native-retirement gate by itself.
