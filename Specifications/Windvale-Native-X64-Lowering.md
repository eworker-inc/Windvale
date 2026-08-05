# Windvale-native x86-64 lowering

## Status and purpose

`Compilerˉnativeˉx64ˉlowering` is the first portable Windvale-written slice of the shared x86-64 backend. It consumes one bounded WVB 1.11 module, independently verifies the metered scalar-control and direct-call subset described below, and emits the canonical WVO 1.0 object used by the existing ABI-22 Stage 0 backend.

This is algorithmic machine-byte selection, not a lowering plan, private intermediate format, or collection of whole-program stencils. The bounded core now has a paired [native WVB-to-WVO application candidate](Windvale-Native-Wvb-To-Wvo.md). C# remains the normal complete WVB-to-native backend outside this subset, the independent fragment and differential oracle, and the candidate constructor until the grouped retirement gate.

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

- WVB 1.11 with seven canonical sections, an explicit absent-metadata byte, no trailing bytes, and a valid Seed module identifier;
- portable profile, no capabilities, no data, and no nominal types;
- either one exported `Main() -> i32`, or exactly one non-`Main` helper at canonical index zero followed by exported `Main` at index one;
- parameterless `Main`, an `i32` return from every function, zero through four `i32` or `bool` helper parameters, no more than 1,024 combined parameters and declared `i32`/`bool` locals per function, a declared maximum stack depth from one through 1,024, at most 8,192 code bytes per function, adjacent exact code ranges, and no unclaimed function-section bytes;
- one control-flow graph per function of at most 1,024 instructions drawn from `i32.const`, `bool.const`, `local.load`, `local.store`, checked `i32.add`, `i32.subtract`, `i32.multiply`, `i32.negate`, all six signed `i32` comparisons, `bool.equal`, `bool.not_equal`, `bool.not`, `jump`, `branch.false`, `call`, and `return`; direct calls are permitted only from `Main` to helper index zero, consume exactly its declared typed arguments, and produce one `i32`; and
- instruction-aligned forward or backward targets, empty stacks at every block edge, complete fixed-point reachability from entry, valid typed local uses and stack effects, an exact declared maximum depth, and a combined local/value frame of at most 2,048 ABI cells. Locals retain WVB's zero-initialized entry semantics.

Every read is preceded by a checked range or exact payload-size test. Unknown versions, reordered/truncated/extended sections, inconsistent lengths, alternate function metadata, unknown opcodes, invalid local indices, type or stack mismatches, malformed branch or call targets, calls from the helper, unreachable blocks or cycles, and mismatched maximum-stack declarations fail closed. General call graphs, recursion, stack-passed or non-scalar call parameters, data references, capabilities, and nominal identities remain outside the subset. Every emitted WVB instruction, including every backward jump and call, executes the existing ABI-22 instruction charge before its operation; a nonterminating loop therefore reaches the bounded instruction-limit status rather than escaping resource accounting.

## Selected object

The output is one standard WVO 1.0 x86-64 object:

- one 16-byte-aligned `.text` section containing the computed code bytes;
- one exported `Main` function symbol, plus a local `$function_0000` symbol when the helper is present, each spanning its exact function bytes; and
- no relocations or platform imports.

The selector assigns one deterministic 16-byte ABI cell to each parameter or local. It reuses separately typed `i32` and `bool` value cells at empty-stack block boundaries exactly as Stage 0 does. `Main` establishes the shared execution context; the helper reuses its `R11` instruction budget, `R10` call-depth budget, and `R15` context. Every function entry charges depth and every normal, propagated, or trap exit restores it. Before a call, scalar arguments are loaded in source order into ABI 22's `R8D`, `R9D`, `ECX`, and `EDX` positions; helper entry stores them into its first ordinary local cells. A direct call is patched to helper offset zero, tests the packed high status word, propagates failure before another WVB instruction, and stores successful `EAX`. The selector otherwise charges every WVB instruction, selects checked scalar and comparison operations, patches signed forward or backward control displacements from computed block offsets, and appends the canonical trap-status tails. The same bytes therefore link under the existing Windows and Linux consumers without introducing a host-specific selector.

The emitted layout is versioned by this contract and must change whenever its ABI, target, metering, trap, frame, or verification contract changes. A changed complete backend is not silently accepted because it happens to return the same scalar.

## Adapters

`Compiler/Windvale/Native-X64-Lowering-Memory-Adapter.wv` exposes `Main(Input: bytes) -> bytes` and returns either the complete WVO or empty bytes. `Compiler/Windvale/Native-X64-Lowering-Tool.wv` is the hosted shell:

```text
wvnative <input.wvb> <output.wvo>
```

It reads the input once, calls the portable core in memory, writes exactly once only after success, and reports the ABI and exact output sizes. Invalid or unsupported input produces a deterministic diagnostic and no output call. The checked-in Project 1 manifests build both adapters as WVB. The paired `WVHN 1` profile packages the hosted shell as a direct Windows/Linux candidate without adding platform assembly.

## Conformance and limits

The existing shared-backend conformance case compiles the Windvale modules once and compares produced WVO bytes with Stage 0 across constants, each checked arithmetic operator, nested expressions, both scalar types, all comparison operations, boolean negation, both conditional routes, forward and backward jumps, loops, early returns, the bounded direct call, and mixed scalar arguments in all four register positions. The combined arithmetic oracle is exactly 1,871 code bytes and a 1,944-byte WVO. The retained nested-control oracle is exactly 4,835 code bytes and a 4,908-byte WVO. The metered loop oracle is exactly 1,665 code bytes and a 1,738-byte WVO; native execution succeeds at its exact 157-instruction requirement and reaches `WVR3011` at 156. The parameterless call oracle is exactly 795 code bytes and a 902-byte WVO, with exact shared instruction and two-entry depth boundaries retained by Stage 0 execution. The four-parameter mixed-scalar oracle is exactly 2,581 code bytes and a 2,688-byte WVO. The same test executes every retained input through the hosted lowering shell as native x86-64; exercises memory and hosted adapters over malformed stack, invalid-local, invalid-branch, unreachable-cycle, changed-call-target, and mismatched-parameter-type streams; and verifies that truncated input leaves a sentinel output unchanged. The same generated tool fragment is host-neutral; current-host evidence is not a Windows/Linux qualification claim.

The mixed-scalar call code and WVO SHA-256 identities remain `1a0a541d2bd59378b4fa6df53248c3c359e909a0b7446198ebb1a58ca5a79721` and `cb7d2c74edb7aa3443e1e23cf0d762d4c15b79c39ea4f363531b2ec80633c13f`. Direct `Bytesˉfromˉi32ˉlittle` emission replaces the former text-formatting conversion without changing any generated object byte. The current WVB identities are `761822053ecee061422571758b1297e6451447a255b3b529cb6546c4ef2a78f7` for the 89,708-byte core, `7a806d8ed92fb4121c2017f3e0ebcfa5e174715e4655a87b0d8e7d0dcf3e3c9b` for the 85,713-byte memory adapter, and `e1a795dd07be21ccb150823bd8790a8766af28d4361b8151cdf224a48f1c4389` for the 86,741-byte hosted tool. The hosted-tool WVB lowers through the complete Stage 0 backend to 1,109,643 code bytes and a 1,112,045-byte WVO; those sizes are evidence, not a permanent optimization promise.

This slice does not yet transfer stack-passed or descriptor parameters, Boolean or descriptor returns, deeper call graphs, recursion, data, descriptors, capabilities, relocations, fragment verification, W^X publication, PE/ELF construction, or the complete compiler. It does not satisfy the native-retirement gate by itself.
