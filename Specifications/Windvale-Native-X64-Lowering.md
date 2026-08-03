# Windvale-native x86-64 lowering

## Status and purpose

`Compilerˉnativeˉx64ˉlowering` is the first portable Windvale-written slice of the shared x86-64 backend. It consumes one bounded WVB 1.6 module, independently admits the exact constant-return subset described below, and emits the canonical WVO 1.0 object used by the existing ABI-22 Stage 0 backend.

This is real machine-byte selection, not a lowering plan or a private intermediate format. It transfers only one deliberately small selector slice. C# remains the normal complete WVB-to-native backend, native fragment verifier, linker/package constructor, and recovery oracle.

## Public result

```text
Compilerˉlowerˉwvbˉnativeˉx64(Input: bytes)
    -> Compilerˉnativeˉx64ˉsummary
```

The summary contains a stable status, WVO object bytes on success, the selected `i32` value, native ABI `22`, and the 406-byte code size. Failure returns an empty object and never publishes a partial WVO.

The status vocabulary is:

| Status | Meaning |
| --- | --- |
| `Valid` | A complete canonical WVO was emitted. |
| `Invalidˉwvb` | Header, section envelope, length, order, trailing bytes, or module identity is malformed. |
| `Unsupportedˉprofile` | The module is not portable. |
| `Unsupportedˉmodule` | Capabilities, static data, or nominal types are present. |
| `Unsupportedˉfunction` | The function or export shape is outside the first slice. |
| `Unsupportedˉcode` | The instruction stream is outside the first slice. |
| `Outputˉlimit` | The pinned stencil or object did not produce its exact bounded size. |

## Accepted WVB subset

The core accepts exactly:

- WVB 1.6 with seven canonical sections, no trailing bytes, and a valid Seed module identifier;
- portable profile, no capabilities, no data, and no nominal types;
- one exported `Main() -> i32` with one compiler temporary of type `i32` and maximum stack depth one; and
- the canonical 16-byte sequence `i32.const <value>`, `local.store 0`, `local.load 0`, `return`.

Every read is preceded by a checked range or exact payload-size test. Unknown versions, reordered/truncated/extended sections, inconsistent lengths, alternate function metadata, extra instructions, and nonzero indices fail closed. Because this exact straight-line shape has no branches, calls, data references, capabilities, or nominal identities, those checks complete verification for the admitted subset rather than deferring executable validation to ambient host state.

## Selected object

The output is one standard WVO 1.0 x86-64 object:

- one 16-byte-aligned `.text` section containing 406 bytes;
- one exported `Main` function symbol spanning the complete section; and
- no relocations or platform imports.

The ABI-22 stencil includes the shared execution-context prologue, instruction-budget charges for all four WVB instructions, call-depth accounting, balanced success return, and the canonical trap-status tails. The source `i32` is the only variable four-byte field. The same bytes therefore link under the existing Windows and Linux consumers without introducing a host-specific selector.

The stencil is versioned by this contract and must change whenever its ABI, target, metering, trap, frame, or verification contract changes. A changed complete backend is not silently accepted because it happens to return the same scalar.

## Adapters

`Native-X64-Lowering-Memory-Adapter.wv` exposes `Main(Input: bytes) -> bytes` and returns either the complete WVO or empty bytes. `Native-X64-Lowering-Tool.wv` is the hosted shell:

```text
wvnative <input.wvb> <output.wvo>
```

It reads the input once, calls the portable core in memory, writes exactly once only after success, and reports the ABI and exact output sizes. Invalid or unsupported input produces a deterministic diagnostic and no output call. The checked-in Project 1 manifests build both adapters as WVB.

## Conformance and limits

The shared-backend conformance case compiles its source fixture once, compares the Windvale-produced WVO byte for byte with the Stage 0 oracle, repeats the comparison for a different signed immediate, parses and links the object through the existing independent boundaries, executes the hosted lowering shell as native x86-64, and verifies that truncated input leaves a sentinel output unchanged. The same generated tool fragment is host-neutral; current-host evidence is not a Windows/Linux qualification claim.

The pinned WVB identities are `654251d1aad3f8099bedb49193ec3a4a92ebeab99f0a7315c4fed780b4535620` for the core, `e5c7472f9eca2a36fa7b63009fb01bdeb38c97229e5a8c7e880ea7c5800a8252` for the memory adapter, and `a0e1894ce9ca79cb9181936f8d5f0ca0a114da3eb62a5c26c149720a1f707fe7` for the hosted tool. The current hosted-tool WVB lowers through the complete Stage 0 backend to 343,453 code bytes and a 344,531-byte WVO; those sizes are evidence, not a permanent optimization promise.

This slice does not yet transfer machine IR construction, arithmetic, control flow, calls, data, descriptors, capabilities, relocations, fragment verification, W^X publication, PE/ELF construction, or the complete compiler. It does not satisfy the native-retirement gate by itself.
