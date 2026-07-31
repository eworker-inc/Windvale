# Windvale Seed implementation

## Status

Implemented by the Windvale Seed milestone.

## End-to-end path

```text
UTF-8 source
    |
lexer and parser
    |
bounded static source-module composition
    |
semantic symbols and typed WIR
    |
deterministic stack-bytecode lowering
    |
canonical sectioned .wvb module
    |
bounded decoder and verifier
    |
verified-module boundary
    |
reference runtime and explicit host capabilities

Windvale source, WVA 1, or Stage 0 producer
    |
canonical WVO 1.0 object bytes
    |
bounded object decoder and verifier
```

The runtime cannot execute raw module bytes or an unverified `Bytecodeˉmodule`. Its public constructor accepts only `Verifiedˉmodule`, which can be obtained through `Moduleˉverifier` or `Moduleˉcodec.Readˉandˉverify`.

## Ownership

### Windvale compiler

`Compiler/Windvale/` owns the compiler implementation written in Windvale:

- The first Windvale-written streaming lexer over strict UTF-8 bytes, with Stage 0 token identities and bounded source/failure coordinates
- A Windvale-written declaration pass exposing module/declaration/body byte spans and counts through streaming cursors without token or declaration collections
- A compiler-owned canonical packed source-set reader that gives portable semantic phases indexed immutable views over the root and ordered dependencies
- A Windvale-written import-graph phase that owns bounded module resolution, root reachability, and cycle rejection without host paths or collections
- A Windvale-written declaration/signature symbol phase with independently validated packed declaration evidence, transitive visibility, deterministic nominal indices, and stable namespace/type failures
- A Windvale-written parameter/local and body-reference binder with independently validated packed WVLB evidence
- A Windvale-written typed WVIR producer with explicit blocks, temporaries, source spans, and an independent packed-directory validator
- A Windvale-written WVIR-to-WVB backend that emits one canonical verified module from a validated source graph

This implementation is correctly named as a compiler even before it completes self-hosting qualification.

### Reference compiler

`Compiler/Reference/` owns the independent C# Stage 0 and recovery implementation:

- Source locations and stable compiler diagnostics
- Tokenization and strict string-literal handling
- Recursive-descent and precedence parsing
- Explicit bounded source-module graph validation and deterministic static composition of dependency records, enums, and functions
- Module, capability, data, record, enum, function, local, and nominal type binding
- Typed, stack-independent WIR with explicit blocks and terminators
- Deterministic lowering from WIR into stack bytecode
- A bounded x86-64 kernel-entry target that lowers the specified linear system-profile WIR subset into independently verified WVO

WIR uses virtual temporaries and local slots. The C# reference compiler lowers its typed WIR to bytecode and, for the narrow target specified by [Windvale-X64-Kernel-Target.md](../../Specifications/Windvale-X64-Kernel-Target.md), to a code-only WVO kernel entry. That target accepts only one linear `Main`, constant text output through an explicit capability adapter, and a constant result; it is not the general native backend. The portable Windvale compiler publishes the separate WVIR 1 contract. Its backend assigns every WIR temporary a bytecode local and emits one complete WVB 1.6 module from a validated WVSS graph whose root is portable, hosted, or system. It statically internalizes portable dependency functions and nominal types while preserving root static data, explicit catalog capabilities, profile, and exports. WVIR retains stable WVSD declaration identities; the backend resolves each identity through its owner source, translates it to ordinal WVB function, data, and capability indices, uses canonical nominal identities as Types indices, and emits canonical functions, root exports, types, capabilities, explicit data, and cross-module interned text literals. The operand stack stays empty between WIR operations and at block boundaries. This intentionally verbose form remains easy to inspect, verify, and compare byte for byte with the reference compiler. Runtime linkage remains separate and is not required by static source composition.

### Bytecode

`Runtime/Windvale.Bytecode/` owns:

- Immutable bytecode contracts and nominal record schemas
- Tagged nominal enum schemas and exact value shapes
- Canonical seven-section serialization
- Strict UTF-8 and little-endian binary decoding
- Size, count, range, canonical-order, and signature limits
- Instruction decoding
- Control-flow and operand-stack type verification
- Module inspection, disassembly, and SHA-256 identity

Every function is checked for valid branch boundaries, index and type use, matching stack states at merges, reachable instructions, valid returns, and an exact declared maximum stack.

### Object model

`Object-Model/Windvale.ObjectModel/` owns:

- Immutable WVO 1.0 section, symbol, and relocation contracts
- Canonical x86-64-first object serialization
- Strict ASCII machine names and bounded little-endian decoding
- Size, alignment, range, canonical-order, symbol-reference, relocation-placeholder, and overlap verification
- Object inspection and SHA-256 identity

The object verifier returns a `Verifiedˉobject`; both CLI object commands decode and verify before reporting. This Stage 0 project is an independent oracle for Windvale-written producers and is not a linker or a host object-format adapter.

### Windvale assembler

`Assembler/Windvale/` owns the assembler implementation written in Windvale:

- Bounded WVA 1 line and token scanning over immutable source bytes
- Complete initial grammar, declaration, ordering, context, reference, and limit validation
- Named-definition offset and size derivation
- The first explicit x86-64 instruction and data encodings
- Canonical WVO relocation and object-byte construction
- Hosted input/output composition with publication only after complete success

### Reference assembler

`Assembler/Reference/` owns the independent C# Stage 0 and recovery implementation:

- WVA 1 line/token parsing and stable source diagnostics
- Canonical symbol and section declaration validation
- Named-definition offset and size derivation
- The same explicit x86-64 instruction and data encodings
- WVO relocation creation and production through the independent object verifier

Both implementations own the same WVA contract and remain byte-for-byte differential oracles. The reference assembler depends only on the object model and returns no bytes until the resulting object passes `Objectˉverifier`; qualification routes Windvale-written output through that independently owned verifier. Neither implementation resolves symbols, chooses final addresses, applies relocations, defines an ABI, or produces an executable image.

### Windvale linker

`Linker/Windvale/` owns the linker implementation written in Windvale:

- Complete immutable WVO validation and link-wide aggregate limits
- Object-private locals plus unique global export/import resolution
- Actual-address alignment and deterministic section contribution order
- Bounded flat-image construction with zero padding and materialized BSS
- Checked `absolute-u32` and `relative-i32` relocation application
- Independent complete-image reconstruction before publication
- Path-free canonical map construction and hosted publish-after-success composition

### Reference linker and target adapters

`Linker/Reference/` owns the independent C# Stage 0 and recovery implementation:

- WVO decoding through the independently verified object-model boundary
- The same symbol resolution, layout, relocation, reconstruction, and canonical map contract
- SHA-256 flat-image evidence and deterministic failure diagnostics
- The currently C#-only deterministic UEFI PE32+ application adapter and its independent verifier

Both linker implementations own the same `flat-x86-64-v1` contract and remain byte-for-byte differential oracles. They do not parse WVA, encode instructions, mutate input objects, or define an ABI. The raw flat image remains a deterministic memory snapshot; the narrow UEFI adapter consumes successful flat link evidence without changing portable link semantics. Later PE host, ELF, and Windvale OS target adapters remain explicit contracts.

### Runtime

`Runtime/Windvale.Runtime/` owns:

- Typed runtime values
- Deterministic local defaults
- Checked `i32` arithmetic
- Checked `u32` arithmetic and `u8` values
- Immutable byte sequences and zero-copy slice views
- Immutable nominal record values and field access
- Nominal enum values, names, equality, and invariant bounded formatting
- Bounded `u8`, little-endian `u16`, `u32`, and signed `i32` reads
- Strict UTF-8 validation/encoding/decoding, safe ASCII quoting, and explicit `u8` to `u32` conversion
- Immutable byte concatenation, fixed-width little-endian construction, and exact SHA-256 identity
- Immutable data access and bounds traps
- Function frames and call-depth limits
- Instruction accounting and execution limits
- Capability authorization, host-support preflight, invocation, and return-value validation
- Bounded launcher arguments, first-read immutable hosted file snapshots, bounded file output, deterministic console output, and separate diagnostics

The interpreter uses ordinary portable .NET APIs and has no Windows-specific or Linux-specific execution path. Immutable byte values use height-balanced persistent trees so concatenation and slicing share storage while reads remain bounded by tree height; strict UTF-8, SHA-256, and hosted output materialize one contiguous value only at their native boundary. Resources are injected through `ICapabilityˉhost`, keeping execution independent from ambient process arguments, files, and console state. The CLI owns the native path adapter and maps it into the hosted file contract.

### CLI

`Tools/Windvale.Tool/` owns argument parsing, strict UTF-8 compiler and assembly input, bounded object input, native hosted-file adaptation, file output, diagnostic presentation, capability grants, and command exit codes. It does not reimplement compiler, assembler, linker, verifier, inspector, or runtime behavior.

## Determinism

- Capabilities, data, functions, and exports are serialized in ordinal name order.
- Source file paths and timestamps are not stored in Seed modules.
- Integers have defined widths and little-endian representation.
- Text is encoded as strict UTF-8.
- The conformance suite compares complete output bytes and fixed SHA-256 identities.

The current bytecode 1.6 golden modules are:

- `Sumˉdata`: `6f3a272d37dd8893995c7f85c236414ed2864bf59de2f3775c08afd426013f8c`
- `Helloˉwindvale`: `bcf6597a27384661d2796f1dd8ee6e24cce8e6c7cb84def3b7826a564acb7d54`
- `Readˉwvbˉheader`: `72ae31559bb3335b320328c26e70518b6a0f3e617d099d41b328b066bb3784c7`
- `Compositionˉdemo`: `0980b7178943be516cd9b6924f179d5977ca147e11bf105c5063ea078c645b60`
- `Foundationˉmachineˉcontracts`: `9f909a4c47d6f7fb41570b58615a533e79e0219a780c686a64995826b322219a`
- `Machineˉcontractsˉdemo`: `b505d3335fa5a4b1dabe2d5e64e4c7a557e0028666cbebe1e2557a0255772f1a`
- `Foundationˉbyteˉordering`: `194e4b5c4eb7f4641a39098abce3dabb93187af7149e184b56b76f978ed2f4f1`
- `Foundationˉbyteˉorderingˉdemo`: `0b41e8f615630e0734812ba8cd8e7c06e975592b86327c2fe8220f5e29c10cab`
- `Foundationˉdecimalˉparsing`: `39f6c1c3d5a2233d5296e777e798450571c5f4ba837120a25a6487bf8014ee1f`
- `Foundationˉdecimalˉparsingˉdemo`: `16a20ee595eb708095f6e8c38c809a24774989110780dbefbacbc36ee468e695`
- `Foundationˉbyteˉconstruction`: `6f26865069333c02b15ab83d48f2a0cb0e3a05db98bcd841f31e232485b76207`
- `Foundationˉbyteˉconstructionˉdemo`: `a9b577dc08ac6e4a0d786f04d6667eb0347c57a0c1abbd81f3481fb0e0bc6c29`
- `Compilerˉsourceˉlexer`: `4d48af0c208e88d9e84d48c80324f35bed1985a799bd275b65b6a07f70111706`
- `Compilerˉsourceˉlexerˉdemo`: `5422673a70ecf92f99f9a2db144f9b7a691d6281a98284dde6c6bc796ada60a4`
- `Compilerˉsourceˉdeclarationˉparser`: `593e841ce9b751015e3de9f3100f4defe83d575b29324784839c38e227ff1276`
- `Compilerˉsourceˉdeclarationˉparserˉdemo`: `3ed1fc6ff4453da1cbfb100e6978029c3db2bb9baaec98c230db6ef1f6267e38`
- `Compilerˉsourceˉdeclarationˉparserˉtool`: `143f9c991de2cc309861aa9ea2beb948bca06cfd22b0f932c8f7abcc41ba9408`
- `Compilerˉsourceˉbodyˉparser`: `7b56ea4d25f2d13467d19123654bb8d617ae2e1b0dd43f2497e1ff9644cc3839`
- `Compilerˉsourceˉbodyˉparserˉdemo`: `07f9b2d94b4ebefaa3260d04b2cc7400b56007f664ef93a8db97207679039005`
- `Compilerˉsourceˉbodyˉparserˉtool`: `9c8b88f9b6aaa27df5d39fc671319ed4890510535321f637a533cf2f01ddeadc`
- `Compilerˉsourceˉset`: `c2a420a984a9bd39754a9e842d14e1e94030cd8ff6a0e313cc1703ae2e244386`
- `Compilerˉsourceˉsetˉdemo`: `960c973b7014b9e77b33b55e9fffa7db0a4a3d0a2b87737d54603f09cec022c0`
- `Compilerˉsourceˉsetˉtool`: `dc8645c9b73fe8bfe10409e2fbd34fd29f125eea42409617ede5256b36a03e2e`
- `Compilerˉsourceˉgraph`: `4b45616ff0304f59f16c44afea637fabd0f66f68ae4b5d7b149e23a9e8e70662`
- `Compilerˉsourceˉgraphˉdemo`: `309c7ca3815c709c759b4673036ffd747d95450c651aa21c3dbf66e59dbe903c`
- `Compilerˉsourceˉgraphˉtool`: `4a07816c65d82b6594270a9b253b999700196d07e10faed0221fc6b1ca7e1e9e`
- `Compilerˉsourceˉsymbols`: `063a9ff3cb37196b1e6d9c8fb5be39916fe9fc21fab8032b0415d5f8ab0677d2`
- `Compilerˉsourceˉsymbolsˉdemo`: `9e7826d354d80d06702555e857f348e08a6e396cca64b11c5eda4895e7294a25`
- `Compilerˉsourceˉsymbolsˉtool`: `54289a4ebfe778c6c2b6ebfb7cb7afaf3d4899af41e68a989eaa26b9dd2f28c4`
- `Compilerˉsourceˉbindings`: `2c253a188c96d5bbdf7e8b81d44dbd776c27c043ba9df7cc6230972c278335e5`
- `Compilerˉsourceˉbindingsˉdemo`: `11621f87f3fe198c97ea141f41ac1a4d69bd34e03f10c17d4bd23083b3087b18`
- `Compilerˉsourceˉbindingsˉtool`: `2e7f2885da27a5ba12f15a831bb2429a21acb7e23ad46061009b4b2209b713f4`
- `Compilerˉsourceˉwir`: `e11a017226c8b357732f02f5bdc6ff581c876276165e267b1875a2daa247cef0`
- `Compilerˉsourceˉwirˉdemo`: `1af3a5d7523ce5ace0091e4892e35c54a4835487186c4e49e653a8af98ecc721`
- `Compilerˉsourceˉwirˉtool`: `3096f68bd7c1ab9e08b5112937d75ef6481b724ba84a9571fd4ce9076156cfa2`
- `Compilerˉsourceˉwvb`: `32c739a08fd70e3df8551a4c15571f5f53da8d661300f55500eb15cc2c909468`
- `Compilerˉsourceˉwvbˉdemo`: `426fcdba5267db8390dfa301d16ea93f5391f25a4fd139ba17268736aefb306e`
- `Compilerˉsourceˉwvbˉtool`: `d68581ed1e89e22eee9c59d051cd1e79e2e75104287f401697528380198c0527`
- `Wvˉdumpˉcore`: `38af93371f5ed737946092092c67f6c363b340c7b2a2e8d0588c05a3e94b730b`
- `Wvoˉobjectˉcore`: `e35939e46ca63f6c284ae457be12de23bb6bc8cb28fac52ce76c833d5fe6bb74`
- `Wvaˉassemblerˉcore`: `a5f4e913078295a323eac315f9df818877ac519de97028e581cab8577f1dd150`
- `Wvˉlinkerˉcore`: `091383174f0ca6e535881f31949c65d46542f8b452905f0a82c713707cada1aa`

The canonical WVO 1.0 representative object is 189 bytes with SHA-256 `006fd80183da7fbc71d3c6d63b65e6f3551765508fe9dba6f38ba80e002eb28a`.

The canonical WVA 1 `Hello-Object.wva` output is SHA-256 `992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85`.

The canonical Windvale Linking 1 two-object image is 24 bytes with SHA-256 `0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a`; its canonical map SHA-256 is `31bc6a8e90d5f3049ae3e2eb0735a901923186d6a03ed40f22762b557b2ba5f4`.

Changes to those hashes require a reviewed bytecode/compiler-contract change rather than an automatic fixture refresh.

## Safety boundaries

- Individual and aggregate source-module input, assembly, module, and object sizes are bounded before sustained processing.
- Binary lengths and offsets use checked conversions and remaining-buffer checks.
- The reader rejects malformed UTF-8, unsupported flags, version mismatches, missing bytes, and trailing bytes.
- The verifier rejects unknown opcodes, truncated operands, bad indices, invalid data uses, stack underflow, type mismatches, invalid branches, inconsistent merges, unreachable instructions, and invalid maximum-stack declarations.
- The assembler rejects malformed structure, noncanonical declarations, mismatched definitions, invalid section contexts, unknown references, numeric-width violations, and objects that fail independent WVO verification.
- The linker rejects malformed objects, aggregate-limit violations, duplicate exports, unresolved or kind-mismatched imports, invalid entry selection, image/address overflow, relocation overflow, and candidates that fail independent whole-image reconstruction.
- Hosted capabilities must be declared in the module, separately authorized, supported by the selected adapter, and validated again on return.
- Hosted arguments and file-byte reads/writes have strict count, UTF-8, snapshot, and allocation bounds; normal and diagnostic output remain separate.
- Runtime signed or unsigned overflow, array bounds, byte-range bounds, strict UTF-8 decoding, bounded text construction, instruction limits, call-depth limits, and hosted resource failures use stable runtime codes.

## Deliberate Seed limits

Seed does not include optimization, a general heap contract, garbage collection, mutable arrays or record fields, nested records, flags enums, general text builders, floating point, catchable exceptions, threads, async work, raw pointers, foreign calls, file enumeration or mutation beyond bounded whole-file replacement, runtime module linkage, package discovery, a host executable-container writer, native compiler backend, or operating-system code. The qualified Windvale and Stage 0 linkers are deliberately limited to verified WVO inputs and one raw flat-memory-image target.

These are scope boundaries, not assertions that the current language model is final.
