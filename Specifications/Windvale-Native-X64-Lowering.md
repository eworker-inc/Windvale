# Windvale-native x86-64 lowering

## Status and purpose

`Compilerˉnativeˉx64ˉlowering` is the first portable Windvale-written slice of the shared x86-64 backend. It consumes one bounded WVB 1.6 module, independently verifies the acyclic scalar-control subset described below, and emits the canonical WVO 1.0 object used by the existing ABI-22 Stage 0 backend.

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
- one forward-only control-flow graph of at most 1,024 instructions drawn from `i32.const`, `bool.const`, `local.load`, `local.store`, checked `i32.add`, `i32.subtract`, `i32.multiply`, `i32.negate`, all six signed `i32` comparisons, `bool.equal`, `bool.not_equal`, `bool.not`, `jump`, `branch.false`, and `return`; and
- instruction-aligned forward targets, empty stacks at every block edge, complete reachability from entry, a `return` on every reachable path, valid typed local uses and stack effects, an exact declared maximum depth, and a combined local/value frame of at most 2,048 ABI cells. Locals retain WVB's zero-initialized entry semantics.

Every read is preceded by a checked range or exact payload-size test. Unknown versions, reordered/truncated/extended sections, inconsistent lengths, alternate function metadata, unknown opcodes, invalid local indices, type or stack mismatches, malformed targets, backward edges, unreachable blocks, and mismatched maximum-stack declarations fail closed. Because this subset has no loops, calls, data references, capabilities, parameters, or nominal identities, those checks complete verification for the admitted subset rather than deferring executable validation to ambient host state.

## Selected object

The output is one standard WVO 1.0 x86-64 object:

- one 16-byte-aligned `.text` section containing the computed code bytes;
- one exported `Main` function symbol spanning the complete section; and
- no relocations or platform imports.

The selector assigns one deterministic 16-byte ABI cell to each local. It reuses separately typed `i32` and `bool` value cells at empty-stack block boundaries exactly as Stage 0 does. It emits the shared execution-context prologue and zeroed frame, charges every WVB instruction, selects checked scalar and comparison operations, balances call-depth state on success and failure, patches explicit jump and conditional-branch displacements from computed block offsets, and appends the canonical trap-status tails. The same bytes therefore link under the existing Windows and Linux consumers without introducing a host-specific selector.

The emitted layout is versioned by this contract and must change whenever its ABI, target, metering, trap, frame, or verification contract changes. A changed complete backend is not silently accepted because it happens to return the same scalar.

## Adapters

`Native-X64-Lowering-Memory-Adapter.wv` exposes `Main(Input: bytes) -> bytes` and returns either the complete WVO or empty bytes. `Native-X64-Lowering-Tool.wv` is the hosted shell:

```text
wvnative <input.wvb> <output.wvo>
```

It reads the input once, calls the portable core in memory, writes exactly once only after success, and reports the ABI and exact output sizes. Invalid or unsupported input produces a deterministic diagnostic and no output call. The checked-in Project 1 manifests build both adapters as WVB.

## Conformance and limits

The existing shared-backend conformance case compiles the Windvale modules once and compares produced WVO bytes with Stage 0 across constants, each checked arithmetic operator, nested expressions, both scalar types, all comparison operations, boolean negation, both conditional routes, jumps, and early returns. The combined arithmetic oracle is exactly 1,871 code bytes and a 1,944-byte WVO. The retained nested-control oracle is exactly 4,835 code bytes and a 4,908-byte WVO. The same test executes constant, arithmetic, and control inputs through the hosted lowering shell as native x86-64; exercises memory and hosted adapters over malformed stack, invalid-local, and backward-target streams; and verifies that truncated input leaves a sentinel output unchanged. The same generated tool fragment is host-neutral; current-host evidence is not a Windows/Linux qualification claim.

The pinned WVB identities are `3ba83ebe5d848273b42f9a4fed3aa110b7396ba54796b03bf43324c5077c584b` for the core, `8c01614988c24d17070b9bbb55276fd9d35a34a917d4fcfb70bc77dc16532a4f` for the memory adapter, and `f6d44af0a3ea8d4dd5c8c3367358d970fb9bd0b2252305d8058bec791a041a57` for the hosted tool. The current hosted-tool WVB lowers through the complete Stage 0 backend to 893,997 code bytes and an 896,179-byte WVO; those sizes are evidence, not a permanent optimization promise.

This slice does not yet transfer loops and their execution-budget contract, calls, multiple functions, data, descriptors, capabilities, relocations, fragment verification, W^X publication, PE/ELF construction, or the complete compiler. It does not satisfy the native-retirement gate by itself.
