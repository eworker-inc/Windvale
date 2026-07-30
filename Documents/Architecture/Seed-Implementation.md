# Windvale Seed implementation

## Status

Implemented by the Windvale Seed milestone.

## End-to-end path

```text
UTF-8 source
    |
lexer and parser
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

The assembler depends only on the object model. It returns no bytes until the resulting object passes `Objectˉverifier`. It does not resolve symbols, choose final addresses, apply relocations, define an ABI, or produce an executable image. The C# project is the Stage 0 oracle for the planned Windvale-written assembler and must not become a parallel permanent object path.

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
- Immutable byte concatenation and fixed-width little-endian construction
- Immutable data access and bounds traps
- Function frames and call-depth limits
- Instruction accounting and execution limits
- Capability authorization, host-support preflight, invocation, and return-value validation
- Bounded launcher arguments, immutable hosted file input and output, deterministic console output, and separate diagnostics

The interpreter uses ordinary portable .NET APIs and has no Windows-specific or Linux-specific execution path. Resources are injected through `ICapabilityˉhost`, keeping execution independent from ambient process arguments, files, and console state. The CLI owns the native path adapter and maps it into the hosted file contract.

### CLI

`Tools/Windvale.Tool/` owns argument parsing, strict UTF-8 compiler and assembly input, native hosted-file adaptation, file output, diagnostic presentation, capability grants, and command exit codes. It does not reimplement compiler, assembler, verifier, inspector, or runtime behavior.

## Determinism

- Capabilities, data, functions, and exports are serialized in ordinal name order.
- Source file paths and timestamps are not stored in Seed modules.
- Integers have defined widths and little-endian representation.
- Text is encoded as strict UTF-8.
- The conformance suite compares complete output bytes and fixed SHA-256 identities.

The current bytecode 1.5 golden modules are:

- `Sumˉdata`: `64134dfd779b353c5e501c9c23337a0c3849bfef2c97a63a07913705b0f10c6b`
- `Helloˉwindvale`: `43d565c304cf2e2f5d886ee30b1fabf0b2fbfb0c8cd28bd932d85d5add0bf504`
- `Readˉwvbˉheader`: `0cdf05f6c9e1fb1db0d5ab449207870b5e47cc248f187cd43cd9a5c3c9eee995`
- `Wvˉdumpˉcore`: `2957fc5523ae3ca16cf1aaeb9104c14a3342a0aefde9ac591bb689f744f1467f`
- `Wvoˉobjectˉcore`: `a5d574ea646946b159d95bd7e51434bfcbf7545083a54541438a79a2e5e999df`

The canonical WVO 1.0 representative object is 189 bytes with SHA-256 `006fd80183da7fbc71d3c6d63b65e6f3551765508fe9dba6f38ba80e002eb28a`.

The canonical WVA 1 `Hello-Object.wva` output is SHA-256 `992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85`.

Changes to those hashes require a reviewed bytecode/compiler-contract change rather than an automatic fixture refresh.

## Safety boundaries

- Source, assembly, module, and object sizes are bounded before sustained processing.
- Binary lengths and offsets use checked conversions and remaining-buffer checks.
- The reader rejects malformed UTF-8, unsupported flags, version mismatches, missing bytes, and trailing bytes.
- The verifier rejects unknown opcodes, truncated operands, bad indices, invalid data uses, stack underflow, type mismatches, invalid branches, inconsistent merges, unreachable instructions, and invalid maximum-stack declarations.
- The assembler rejects malformed structure, noncanonical declarations, mismatched definitions, invalid section contexts, unknown references, numeric-width violations, and objects that fail independent WVO verification.
- Hosted capabilities must be declared in the module, separately authorized, supported by the selected adapter, and validated again on return.
- Hosted arguments and file-byte reads/writes have strict count, UTF-8, and allocation bounds; normal and diagnostic output remain separate.
- Runtime signed or unsigned overflow, array bounds, byte-range bounds, strict UTF-8 decoding, bounded text construction, instruction limits, call-depth limits, and hosted resource failures use stable runtime codes.

## Deliberate Seed limits

Seed does not include optimization, a general heap contract, garbage collection, mutable arrays or record fields, nested records, flags enums, general text builders, floating point, catchable exceptions, threads, async work, raw pointers, foreign calls, file enumeration or mutation beyond bounded whole-file replacement, dynamic linking, a Windvale-written assembler, linker, executable-image writer, native compiler backend, or operating-system code. The implemented Stage 0 assembler is deliberately limited to WVA 1 and WVO production.

These are scope boundaries, not assertions that the current language model is final.
