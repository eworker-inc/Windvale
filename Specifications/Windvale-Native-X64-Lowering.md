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
| `Unsupportedˉmodule` | Capabilities, nominal types, or static data outside the accepted bounded i32 profile are present. |
| `Unsupportedˉfunction` | The function or export shape is outside the first slice. |
| `Unsupportedˉcode` | The instruction stream is outside the first slice. |
| `Outputˉlimit` | Projected or emitted code/object bytes exceed or disagree with the bounded layout. |

## Accepted WVB subset

The core accepts exactly:

- WVB 1.11 with seven canonical sections, an explicit absent-metadata byte, no trailing bytes, and a valid Seed module identifier;
- portable profile, no capabilities or nominal types, and either no data or exactly one canonical immutable `[i32]` declaration of at most 262,144 elements;
- one through eight functions with exactly one exported parameterless `Main() -> i32` at any ordinal and every other function non-exported;
- parameterless `Main`, an `i32` return from every function, zero through four `i32` or `bool` helper parameters, no more than 1,024 combined parameters and declared `i32`/`bool` locals per function, a declared maximum stack depth from one through 1,024, at most 8,192 code bytes per function, adjacent exact code ranges, and no unclaimed function-section bytes;
- one control-flow graph per function of at most 1,024 instructions drawn from `i32.const`, `bool.const`, `local.load`, `local.store`, `data.length`, `data.load.i32`, checked `i32.add`, `i32.subtract`, `i32.multiply`, `i32.negate`, all six signed `i32` comparisons, `bool.equal`, `bool.not_equal`, `bool.not`, `jump`, `branch.false`, `call`, and `return`; every data operation names the accepted declaration, and every direct call names any in-range function, consumes that function's exact declared typed arguments, and produces one `i32`; and
- instruction-aligned forward or backward targets, empty stacks at every block edge, complete fixed-point reachability from entry, valid typed local uses and stack effects, an exact declared maximum depth, and a combined local/value frame of at most 2,048 ABI cells. Locals retain WVB's zero-initialized entry semantics.

Every read is preceded by a checked range or exact payload-size test. Unknown versions, reordered/truncated/extended sections, inconsistent lengths, alternate function metadata, unknown opcodes, invalid local, data, or call indices, type or stack mismatches, malformed branch targets, unreachable control blocks, and mismatched maximum-stack declarations fail closed. Forward, backward, self-recursive, mutually recursive, and cyclic call edges share the same exact signature checks. Every function entry consumes ABI 22's shared call-depth budget before its frame executes, and every emitted WVB instruction, including every data operation, backward jump, and call, consumes the shared instruction budget before its operation. Recursion and nonterminating cycles therefore reach `WVR3004` or `WVR3011` rather than escaping resource accounting. Stack-passed or non-scalar call parameters, multiple or non-i32 data declarations, capabilities, and nominal identities remain outside the subset.

## Selected object

The output is one standard WVO 1.0 x86-64 object:

- one 16-byte-aligned `.text` section containing the computed code bytes and, when static data exists, the canonical alignment padding;
- an optional 16-byte-aligned `.rodata` section containing the exact little-endian i32 array bytes;
- canonical local `$function_NNNN` symbols for every non-main ordinal followed by one exported `Main` function symbol, plus optional local `$data_0000`, each spanning its exact range; and
- one ordered `Relative_i32` relocation with addend `-4` for each static-data load, and no platform imports.

The selector assigns one deterministic 16-byte ABI cell to each parameter or local. It reuses separately typed `i32` and `bool` value cells at empty-stack block boundaries exactly as Stage 0 does. `Main` establishes the shared execution context at its exported ordinal; every helper reuses its `R11` instruction budget, `R10` call-depth budget, and `R15` context. Every function entry charges depth and every normal, propagated, or trap exit restores it. Before a call, scalar arguments are loaded in source order into ABI 22's `R8D`, `R9D`, `ECX`, and `EDX` positions; helper entry stores them into its first ordinary local cells. A first bounded pass collects every signature, a second measures every function against that complete set and fixes a 16-byte-per-function directory containing machine offset, length, parameter count, and four padded scalar types, and a third emits code. Every direct call reads its target's signature and offset from that directory, tests the packed high status word, propagates failure before another WVB instruction, and stores successful `EAX`. Static-data loads use the exact unsigned bounds branch to ABI 22's data-bounds status and a zeroed RIP-relative field represented by canonical WVO relocation. The selector otherwise charges every WVB instruction, selects checked scalar and comparison operations, patches signed forward or backward control displacements from computed block offsets, and appends the canonical trap-status tails. The same bytes therefore link under the existing Windows and Linux consumers without introducing a host-specific selector.

The emitted layout is versioned by this contract and must change whenever its ABI, target, metering, trap, frame, or verification contract changes. A changed complete backend is not silently accepted because it happens to return the same scalar.

## Adapters

`Compiler/Windvale/Native-X64-Lowering-Memory-Adapter.wv` exposes `Main(Input: bytes) -> bytes` and returns either the complete WVO or empty bytes. `Compiler/Windvale/Native-X64-Lowering-Tool.wv` is the hosted shell:

```text
wvnative <input.wvb> <output.wvo>
```

It reads the input once, calls the portable core in memory, writes exactly once only after success, and reports the ABI and exact output sizes. Invalid or unsupported input produces a deterministic diagnostic and no output call. The checked-in Project 1 manifests build both adapters as WVB. The paired `WVHN 1` profile packages the hosted shell as a direct Windows/Linux candidate without adding platform assembly.

## Conformance and limits

The existing shared-backend conformance case compiles the Windvale modules once and compares produced WVO bytes with Stage 0 across constants, each checked arithmetic operator, nested expressions, both scalar types, all comparison operations, boolean negation, both conditional routes, forward and backward jumps, loops, early returns, bounded calls, mixed scalar arguments in all four register positions, general call order, recursion, and canonical static data. The combined arithmetic oracle is exactly 1,871 code bytes and a 1,944-byte WVO. The retained nested-control oracle is exactly 4,835 code bytes and a 4,908-byte WVO. The metered loop oracle is exactly 1,665 code bytes and a 1,738-byte WVO; native execution succeeds at its exact 157-instruction requirement and reaches `WVR3011` at 156. The parameterless call oracle is exactly 795 code bytes and a 902-byte WVO, with exact shared instruction and two-entry depth boundaries retained by Stage 0 execution. The four-parameter mixed-scalar oracle is exactly 2,581 code bytes and a 2,688-byte WVO. The existing three-function `Add -> Build -> Main` fixture additionally calls both lower ordinals through control flow and returns 42. A verifier-approved `Alpha, Main, Zeta` oracle combines a real forward edge with same-signature forward/back mutual recursion; it returns 42 at exact call depth five, reaches `WVR3004` at four, and produces Stage 0's exact 4,350-byte `.text` and 4,491-byte WVO. Canonical 493-byte `Sum-Data.wv` combines one `[i32]` declaration, `data.length`, a bounds-checked load, backward control, and a scalar call; it produces the exact 3,088-byte `.text`, 16-byte `.rodata`, one relocation, and 3,288-byte WVO emitted by Stage 0. The same test executes every retained input through the hosted lowering shell as native x86-64; exercises memory and hosted adapters over malformed stack, invalid-local, invalid-data, invalid-branch, unreachable control, out-of-range call target, and mismatched-parameter-type streams; and verifies that truncated input leaves a sentinel output unchanged. The same generated tool fragment is host-neutral; current-host evidence is not a Windows/Linux qualification claim.

The mixed-scalar call code and WVO SHA-256 identities remain `1a0a541d2bd59378b4fa6df53248c3c359e909a0b7446198ebb1a58ca5a79721` and `cb7d2c74edb7aa3443e1e23cf0d762d4c15b79c39ea4f363531b2ec80633c13f`. Existing code-only objects remain byte-identical. The current WVB identities are `9a32b3854270bcace52f615633f4b110d9f0777ba5fb5338157af0965dcc8ed4` for the data module, `23298b46f524e31f4eca6e63d0815ff96a969182cce8872887c0650ab098a572` for the layout module, `e9237fe0bef27b4c4d4cb682872e25aae0532c9e3a7b1e2f4f43aab739b55046` for the object module, `d4b7fcf12301de8d2be955e95a629467fcd45e719404449b3f8cc2938b82602b` for the core closure, `77ab37967363200d7bf75b6f86689e6cbebe50701d47bb601e8c9e26e32f5a21` for the memory adapter, and `dbc2b2f75baceb8659d4f2c0977b3e3290abbaf38b3d47b6e40aed6b399fd2bd` for the 103,043-byte hosted tool. The hosted-tool WVB lowers through the complete Stage 0 backend to 1,273,630 code bytes and a 1,277,080-byte WVO; those sizes are evidence, not a permanent optimization promise.

This slice does not yet transfer stack-passed or descriptor parameters, Boolean or descriptor returns, more than eight functions, multiple/text/bytes data, descriptors, capabilities, general relocations, fragment verification, W^X publication, PE/ELF construction, or the complete compiler. It does not satisfy the native-retirement gate by itself.
