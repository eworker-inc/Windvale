# Windvale-native x86-64 lowering

## Status and purpose

`Compilerˉnativeˉx64ˉlowering` is the first portable Windvale-written slice of the shared x86-64 backend. It consumes one bounded WVB 1.6 module, independently verifies the metered scalar-control and direct-call subset described below, and emits the canonical WVO 1.0 object used by the existing ABI-22 Stage 0 backend.

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
- either one exported `Main() -> i32`, or exactly one non-`Main` helper at canonical index zero followed by exported `Main` at index one;
- no parameters, `i32` returns, zero through 1,024 `i32` or `bool` locals per function, a declared maximum stack depth from one through 1,024, at most 8,192 code bytes per function, adjacent exact code ranges, and no unclaimed function-section bytes;
- one control-flow graph per function of at most 1,024 instructions drawn from `i32.const`, `bool.const`, `local.load`, `local.store`, checked `i32.add`, `i32.subtract`, `i32.multiply`, `i32.negate`, all six signed `i32` comparisons, `bool.equal`, `bool.not_equal`, `bool.not`, `jump`, `branch.false`, `call`, and `return`; direct calls are permitted only from `Main` to parameterless helper index zero and produce one `i32`; and
- instruction-aligned forward or backward targets, empty stacks at every block edge, complete fixed-point reachability from entry, valid typed local uses and stack effects, an exact declared maximum depth, and a combined local/value frame of at most 2,048 ABI cells. Locals retain WVB's zero-initialized entry semantics.

Every read is preceded by a checked range or exact payload-size test. Unknown versions, reordered/truncated/extended sections, inconsistent lengths, alternate function metadata, unknown opcodes, invalid local indices, type or stack mismatches, malformed branch or call targets, calls from the helper, unreachable blocks or cycles, and mismatched maximum-stack declarations fail closed. General call graphs, recursion, call parameters, data references, capabilities, and nominal identities remain outside the subset. Every emitted WVB instruction, including every backward jump and call, executes the existing ABI-22 instruction charge before its operation; a nonterminating loop therefore reaches the bounded instruction-limit status rather than escaping resource accounting.

## Selected object

The output is one standard WVO 1.0 x86-64 object:

- one 16-byte-aligned `.text` section containing the computed code bytes;
- one exported `Main` function symbol, plus a local `$function_0000` symbol when the helper is present, each spanning its exact function bytes; and
- no relocations or platform imports.

The selector assigns one deterministic 16-byte ABI cell to each local. It reuses separately typed `i32` and `bool` value cells at empty-stack block boundaries exactly as Stage 0 does. `Main` establishes the shared execution context; the helper reuses its `R11` instruction budget, `R10` call-depth budget, and `R15` context. Every function entry charges depth and every normal, propagated, or trap exit restores it. A direct call is patched to helper offset zero, tests the packed high status word, propagates failure before another WVB instruction, and stores successful `EAX`. The selector otherwise charges every WVB instruction, selects checked scalar and comparison operations, patches signed forward or backward control displacements from computed block offsets, and appends the canonical trap-status tails. The same bytes therefore link under the existing Windows and Linux consumers without introducing a host-specific selector.

The emitted layout is versioned by this contract and must change whenever its ABI, target, metering, trap, frame, or verification contract changes. A changed complete backend is not silently accepted because it happens to return the same scalar.

## Adapters

`Native-X64-Lowering-Memory-Adapter.wv` exposes `Main(Input: bytes) -> bytes` and returns either the complete WVO or empty bytes. `Native-X64-Lowering-Tool.wv` is the hosted shell:

```text
wvnative <input.wvb> <output.wvo>
```

It reads the input once, calls the portable core in memory, writes exactly once only after success, and reports the ABI and exact output sizes. Invalid or unsupported input produces a deterministic diagnostic and no output call. The checked-in Project 1 manifests build both adapters as WVB.

## Conformance and limits

The existing shared-backend conformance case compiles the Windvale modules once and compares produced WVO bytes with Stage 0 across constants, each checked arithmetic operator, nested expressions, both scalar types, all comparison operations, boolean negation, both conditional routes, forward and backward jumps, loops, early returns, and the bounded direct call. The combined arithmetic oracle is exactly 1,871 code bytes and a 1,944-byte WVO. The retained nested-control oracle is exactly 4,835 code bytes and a 4,908-byte WVO. The metered loop oracle is exactly 1,665 code bytes and a 1,738-byte WVO; native execution succeeds at its exact 157-instruction requirement and reaches `WVR3011` at 156. The call oracle is exactly 795 code bytes and a 902-byte WVO, with exact shared instruction and two-entry depth boundaries retained by Stage 0 execution. The same test executes constant, arithmetic, control, loop, and call inputs through the hosted lowering shell as native x86-64; exercises memory and hosted adapters over malformed stack, invalid-local, invalid-branch, unreachable-cycle, and changed-call-target streams; and verifies that truncated input leaves a sentinel output unchanged. The same generated tool fragment is host-neutral; current-host evidence is not a Windows/Linux qualification claim.

The call code and WVO SHA-256 identities are `5687bce4c0a13535256d4d8c238153ecb8a48c27e77248a307b203ca33303424` and `790d2436ef6f45a6379494038dbbc4ba8987d597ee32e711eb3ef2ab3aeda133`. The pinned WVB identities are `26cde3077eca627ca50763113178f68206c52b3df833ec3fd0b70ca261c6af89` for the core, `d9556132e930dd226e77b50ab963b3783e3368bb1375fc61326a9aa6e6ef6ffc` for the memory adapter, and `654d893551b923d707a46ba1d41a99672cdeceafbd14cce65a75461022d2c0b4` for the hosted tool. The current hosted-tool WVB lowers through the complete Stage 0 backend to 1,055,451 code bytes and a 1,057,785-byte WVO; those sizes are evidence, not a permanent optimization promise.

This slice does not yet transfer scalar call parameters, Boolean or descriptor returns, deeper call graphs, recursion, data, descriptors, capabilities, relocations, fragment verification, W^X publication, PE/ELF construction, or the complete compiler. It does not satisfy the native-retirement gate by itself.
