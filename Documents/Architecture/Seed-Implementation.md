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
- Strict UTF-8 validation/decoding, safe ASCII quoting, and explicit `u8` to `u32` conversion
- Immutable data access and bounds traps
- Function frames and call-depth limits
- Instruction accounting and execution limits
- Capability authorization, host-support preflight, invocation, and return-value validation
- Bounded launcher arguments, immutable hosted file input, deterministic output, and separate diagnostics

The interpreter uses ordinary portable .NET APIs and has no Windows-specific or Linux-specific execution path. Resources are injected through `ICapabilityˉhost`, keeping execution independent from ambient process arguments, files, and console state. The CLI owns the native path adapter and maps it into the hosted file contract.

### CLI

`Tools/Windvale.Tool/` owns argument parsing, strict UTF-8 compiler input, native hosted-file adaptation, file output, diagnostic presentation, capability grants, and command exit codes. It does not reimplement compiler, verifier, inspector, or runtime behavior.

## Determinism

- Capabilities, data, functions, and exports are serialized in ordinal name order.
- Source file paths and timestamps are not stored in Seed modules.
- Integers have defined widths and little-endian representation.
- Text is encoded as strict UTF-8.
- The conformance suite compares complete output bytes and fixed SHA-256 identities.

The current bytecode 1.4 golden modules are:

- `Sumˉdata`: `6a40e6172787ae294361b3a5d9abc92e7b3f004b1e59eabb999a7b844a21bf78`
- `Helloˉwindvale`: `5b9101e15ae42acb333a8a05c60e6d6dbb548e5a04b9c96fdb717dbc58bf9cbe`
- `Readˉwvbˉheader`: `26176eac5e2f00bb96a4b1ad95ad79238045932b64d8220edcfdea13af202c6a`
- `Wvˉdumpˉcore`: `74c5400120f01f8d4a3e0fa87c3bb20d2edd645208d8ccb930e994a416c497f1`

Changes to those hashes require a reviewed bytecode/compiler-contract change rather than an automatic fixture refresh.

## Safety boundaries

- Source and module sizes are bounded before sustained processing.
- Binary lengths and offsets use checked conversions and remaining-buffer checks.
- The reader rejects malformed UTF-8, unsupported flags, version mismatches, missing bytes, and trailing bytes.
- The verifier rejects unknown opcodes, truncated operands, bad indices, invalid data uses, stack underflow, type mismatches, invalid branches, inconsistent merges, unreachable instructions, and invalid maximum-stack declarations.
- Hosted capabilities must be declared in the module, separately authorized, supported by the selected adapter, and validated again on return.
- Hosted arguments and file-byte results have strict count, UTF-8, and allocation bounds; normal and diagnostic output remain separate.
- Runtime signed or unsigned overflow, array bounds, byte-range bounds, strict UTF-8 decoding, bounded text construction, instruction limits, call-depth limits, and hosted resource failures use stable runtime codes.

## Deliberate Seed limits

Seed does not include optimization, a general heap contract, garbage collection, mutable arrays or record fields, nested records, flags enums, general text builders, floating point, catchable exceptions, threads, async work, raw pointers, foreign calls, file writing or enumeration, dynamic linking, a native backend, assembler, linker, or operating-system code.

These are scope boundaries, not assertions that the current language model is final.
