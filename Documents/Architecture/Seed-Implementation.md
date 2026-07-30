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
- Bounded `u8`, little-endian `u16`, and little-endian `u32` reads
- Immutable data access and bounds traps
- Function frames and call-depth limits
- Instruction accounting and execution limits
- Capability authorization and host invocation

The implementation uses ordinary portable .NET APIs and has no Windows-specific or Linux-specific execution path. Console output is injected through `ICapabilityˉhost`, keeping the interpreter independent from process-global console state.

### CLI

`Tools/Windvale.Tool/` owns argument parsing, strict UTF-8 file input, file output, diagnostic presentation, capability grants, and command exit codes. It does not reimplement compiler, verifier, inspector, or runtime behavior.

## Determinism

- Capabilities, data, functions, and exports are serialized in ordinal name order.
- Source file paths and timestamps are not stored in Seed modules.
- Integers have defined widths and little-endian representation.
- Text is encoded as strict UTF-8.
- The conformance suite compares complete output bytes and fixed SHA-256 identities.

The current bytecode 1.3 golden modules are:

- `Sumˉdata`: `63ad39f6dbfff9b5ec31deb2d99d235dc59069a14a77033cf0a8284063578947`
- `Helloˉwindvale`: `e113e56fef9bd108722fb8b16da93a42eec74699952d9055334c7ae0fe9db79b`
- `Readˉwvbˉheader`: `66e3ec061c06428b3b6fb7f43c45386e1a34f68e4d93ffb0c2a046f2ecca2bed`
- `Wvˉdumpˉcore`: `d2fe00ed4dec255547d40325b8b220ff09c71c00cb1e170ffee0f5d60e566511`

Changes to those hashes require a reviewed bytecode/compiler-contract change rather than an automatic fixture refresh.

## Safety boundaries

- Source and module sizes are bounded before sustained processing.
- Binary lengths and offsets use checked conversions and remaining-buffer checks.
- The reader rejects malformed UTF-8, unsupported flags, version mismatches, missing bytes, and trailing bytes.
- The verifier rejects unknown opcodes, truncated operands, bad indices, invalid data uses, stack underflow, type mismatches, invalid branches, inconsistent merges, unreachable instructions, and invalid maximum-stack declarations.
- Hosted capabilities must be declared in the module and separately authorized by the embedding host.
- Runtime signed or unsigned overflow, array bounds, byte-range bounds, instruction limits, and call-depth limits fail with stable runtime codes.

## Deliberate Seed limits

Seed does not include optimization, a general heap contract, garbage collection, mutable arrays or record fields, nested records, flags enums, general text builders, floating point, exceptions, threads, async work, raw pointers, foreign calls, filesystem access, dynamic linking, a native backend, assembler, linker, or operating-system code.

These are scope boundaries, not assertions that the current language model is final.
