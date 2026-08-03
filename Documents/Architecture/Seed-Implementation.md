# Windvale Seed implementation

## Status

The core Seed milestone is implemented and qualified. Later compiler-bootstrap, shared-native, and operating-system slices consume these contracts and are summarized here only where they clarify current ownership.

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

This implementation is the qualified Windvale compiler: Stage 0 builds Stage 1, and Stage 1 reproduces the exact Stage 2 WVB from the committed 12-module inventory on Windows and Debian. Native execution of that compiler remains a later gate.

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

### Shared native execution

`Runtime/Windvale.Native/` owns the current bounded ABI-16 lowering, strict fragment reconstruction, W^X JIT and WVO/AOT execution, versioned execution context, twelve native service leaves including exact Windows/Linux file output, verified internal calls through 64 parameters, publication layout, and narrow platform adapters. Windvale-written source under `Compiler/Windvale/` validates and constructs the two live process-input leaves and plans every executable-image extent and canonical service placement. C# Stage 0 still owns WVB loading/lowering, OS allocation and protection, cache publication, invocation, arenas, and teardown until the measured transfers and [native-retirement gate](Native-Execution-And-Dotnet-Retirement.md#native-retirement-gate) are complete.

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
- Complete grammar, declaration, ordering, context, local-label, reference, register-width, and limit validation
- Named-definition offset and size derivation
- Typed 32/64-bit GPR, REX/ModRM/SIB, local-control, immediate scalar, multiply, shift/rotate, stack, indirect-control, RIP-relative, indexed-memory, machine, and data encodings
- Canonical WVO relocation and object-byte construction
- Hosted input/output composition with publication only after complete success

### Reference assembler

`Assembler/Reference/` owns the independent C# Stage 0 and recovery implementation:

- WVA 1 line/token parsing, typed register/memory/immediate operands, local fixups, and stable source diagnostics
- Canonical symbol and section declaration validation
- Named-definition offset and size derivation
- The same expanded x86-64 instruction and data encodings through an independent encoder
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
- Portable paired-console layout, sparse exact construction, segmented completed-container verification, and recovered-native evidence

### Reference linker and target adapters

`Linker/Reference/` owns the independent C# Stage 0 and recovery implementation:

- WVO decoding through the independently verified object-model boundary
- The same symbol resolution, layout, relocation, reconstruction, and canonical map contract
- SHA-256 flat-image evidence and deterministic failure diagnostics
- The C#-only deterministic UEFI PE32+ application adapter and its independent verifier
- The first deterministic import-free Windows x64 console adapter for capability-free scalar ABI-20 fragments, including its exact startup/context boundary, sparse-recipe materialization, and independent PE recovery verifier
- The paired deterministic sectionless Linux x64 static-PIE adapter over the same verified fragment, including its bounded mapped stack, exact syscall boundary, sparse-recipe materialization, and independent ELF recovery verifier

Both linker implementations own the same `flat-x86-64-v1` contract and remain byte-for-byte differential oracles. They do not parse WVA, encode instructions, mutate input objects, or define portable semantics. The raw flat image remains a deterministic memory snapshot. The narrow UEFI adapter consumes successful flat-link evidence without changing portable link semantics; the Windows and Linux console targets additionally consume one independently verified ABI-20 fragment, reproduce it through WVO/link, and supply target-specific exact process-entry/context adapters. Portable Windvale now plans and describes every container byte and verifies completed PE/ELF bytes through two bounded chunks; C# retains materialization, evidence checks, and independently structured PE/ELF recovery. General hosted-service PE/ELF and Windvale OS target adapters remain later explicit contracts.

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
- `Compilerˉsourceˉlexer`: `ca91d5aa9889540250be552b5563dacba8deba2abb70ea557d0e4f8089ee749f`
- `Compilerˉsourceˉlexerˉdemo`: `2a7a2f8c1276c252fa8ddb53a362c6560dfa06ba8c2a8be0fb56f507e820df87`
- `Compilerˉsourceˉdeclarationˉparser`: `4bbaaaa6293ab1fb5a4eb92c3e8a52c078943ba88652b27f69fdc3c5ab76fda7`
- `Compilerˉsourceˉdeclarationˉparserˉdemo`: `ab28936fe0961261a0f243009d5c9b93af52069326618e03e428d1cc024fea11`
- `Compilerˉsourceˉdeclarationˉparserˉtool`: `94134e28bef9544b0fbb4b4ae6dfd3deb3aa52598475023d37b01a5de8686d45`
- `Compilerˉsourceˉbodyˉparser`: `3df42c7b6e81343194340b8f6f44e44fb83f3d6f18c249c9d9ed4e58df69ec73`
- `Compilerˉsourceˉbodyˉparserˉdemo`: `afa07f843679e89f84a5a55887af834575d43d4a3ac3f1a76cd4395a103e62b6`
- `Compilerˉsourceˉbodyˉparserˉtool`: `342fadc0886e5b8b2910cb65c8495730a902364a526fd34df58c574a32a91890`
- `Compilerˉsourceˉset`: `ab6a6afc5cc90e8db508a9ce4d22acc42cf2cbc5293afad977881a71c3b2658a`
- `Compilerˉsourceˉsetˉdemo`: `dda97ec276bc2c56552e765854322b1177f5b6c27d36fec25d9360f39451b7e1`
- `Compilerˉsourceˉsetˉtool`: `58d29de0ea3b92a83f0cd84bba22910c2c826e7f01d93d0aa5a04f8d0a029322`
- `Compilerˉsourceˉgraph`: `a6ef5896e45593f45b136cc73f3e8c57dd33274ff4736eff18795276fb0c8885`
- `Compilerˉsourceˉgraphˉdemo`: `7c0e191c6a931617aee23fbc91dac61648ebc2f8f2a40a1690ff648a6b9d60de`
- `Compilerˉsourceˉgraphˉtool`: `ffbbea564754c667961680497d3b077f38626a4993ed9c1e1a0d5966e5378aba`
- `Compilerˉsourceˉsymbols`: `230701dc73c8b18e4beedbaad1ce09fa02e83ab5d65e1152ed9ad945e0846105`
- `Compilerˉsourceˉsymbolsˉdemo`: `02ca6d2b9d3dd18efe5aafaf329f787226fc68051a10e63cfb053dd51e4654d0`
- `Compilerˉsourceˉsymbolsˉtool`: `6c83cd9813efb88e86252ce428248245a3f1c0c5d9f8cdb7eeefef4172b126c3`
- `Compilerˉsourceˉbindings`: `3922ca780b11162a9a331b7ca2fc6d3bb070e89134190eabbb978640a05ca128`
- `Compilerˉsourceˉbindingsˉdemo`: `26c778d4676d9dfa969cfebe41593d7733a6c5ad8fc54c0ee7b1b9a2dc6a5880`
- `Compilerˉsourceˉbindingsˉtool`: `d989caa9573ca0b69df46b6a5cf0bd385011d65311463043bcc0b74e25b5a28c`
- `Compilerˉsourceˉwir`: `f94c96ce84ea05e7802bf4780bb8c0ef5d818303ac83730b50273006aaf6a35e`
- `Compilerˉsourceˉwirˉdemo`: `0fcf0bf2a6eda1ae271bbe83169af9acfb339e24436c5e76e77b5e273b54301b`
- `Compilerˉsourceˉwirˉtool`: `cf1421565f9888b23864253b722feac9fa3aa053a0dccfd06d695ca10162ff87`
- `Compilerˉsourceˉwvb`: `eee0cbffcd6f615d1d7805ece8dfc1a8747d265de4c8fe6cae0b426e0770178f`
- `Compilerˉsourceˉwvbˉdemo`: `c6e6fcbfd674df8d5e147b7c3bb52dfbdd4e26fcae4089ccb6c6fb00ffbb26db`
- `Compilerˉsourceˉwvbˉtool`: `db0b76432f531da40cfb91617673ea0df2981102f294765e7e60c544a8129d0e`
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

The core Seed language/runtime contract does not include optimization, a general heap contract, garbage collection, mutable arrays or record fields, nested records, flags enums, general text builders, floating point, catchable exceptions, threads, async work, raw pointers, foreign calls, file enumeration or mutation beyond bounded whole-file replacement, runtime module linkage, package discovery, or a general host executable-container writer. Later repository layers provide a bounded shared native backend, narrow capability-free Windows and Linux console containers, and an experimental OS probe, but those do not silently broaden Seed semantics. The qualified Windvale and Stage 0 linkers remain deliberately limited to verified WVO inputs and one raw flat-memory-image target; UEFI, Windows, and Linux packaging are separate target adapters.

These are scope boundaries, not assertions that the current language model is final.
