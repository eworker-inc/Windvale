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

### Compiler

`Compiler/Windvale.Compiler/` owns:

- Source locations and stable compiler diagnostics
- Tokenization and strict string-literal handling
- Recursive-descent and precedence parsing
- Explicit bounded source-module graph validation and deterministic static composition
- Module, capability, data, record, enum, function, local, and nominal type binding
- Typed, stack-independent WIR with explicit blocks and terminators
- Deterministic lowering from WIR into stack bytecode

WIR uses virtual temporaries and local slots. Bytecode lowering assigns every WIR temporary a bytecode local, keeping the operand stack empty between WIR operations and at block boundaries. This is intentionally verbose but makes the first backend easy to inspect and verify.

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

### Assembler

`Assembler/Windvale.Assembler/` owns:

- WVA 1 line/token parsing and stable source diagnostics
- Canonical symbol and section declaration validation
- Named-definition offset and size derivation
- The first explicit x86-64 instruction and data encodings
- WVO relocation creation and verified object production

The assembler depends only on the object model. It returns no bytes until the resulting object passes `Objectˉverifier`. It does not resolve symbols, choose final addresses, apply relocations, define an ABI, or produce an executable image. The C# project remains the Stage 0 recovery oracle for the qualified Windvale-written assembler and must not become a parallel permanent object path.

### Linker

`Linker/Windvale.Linker/` owns:

- Link-wide WVO input validation and aggregate limits
- Object-private locals plus unique global export/import resolution
- Actual-address alignment and deterministic section contribution order
- Bounded flat-image construction with zero padding and materialized BSS
- Checked `absolute-u32` and `relative-i32` relocation application
- Independent complete-image reconstruction before publication
- Path-free canonical map construction and SHA-256 image evidence

The linker depends only on the object model. It does not parse WVA, encode instructions, mutate objects, select a host executable format, or define an ABI. The raw flat image is a deterministic memory snapshot; later PE, ELF, UEFI, and Windvale OS adapters remain explicit targets over verified link evidence.

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
- `Compositionˉdemo`: `5d27c9667eb66e1abbf46b40d02ab3d4e01b94a421a93bffd0375a550440a612`
- `Wvˉdumpˉcore`: `38af93371f5ed737946092092c67f6c363b340c7b2a2e8d0588c05a3e94b730b`
- `Wvoˉobjectˉcore`: `76f5a414bdc8feab35cedb28ecfc56d0ed24b0abcfc3c5c128e4f71fd0e5232b`
- `Wvaˉassemblerˉcore`: `9fe0e79a4895281908df13b31f127dd9dd019282263da71874bbefb7d9d3cb3a`
- `Wvˉlinkerˉcore`: `8d3cb567f6985077b3ad487627bf77a20326b4bc02bcab8d938354f48d339cfd`

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
