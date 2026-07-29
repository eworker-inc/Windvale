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
- Module, capability, data, function, local, and type binding
- Typed, stack-independent WIR with explicit blocks and terminators
- Deterministic lowering from WIR into stack bytecode

WIR uses virtual temporaries and local slots. Bytecode lowering assigns every WIR temporary a bytecode local, keeping the operand stack empty between WIR operations and at block boundaries. This is intentionally verbose but makes the first backend easy to inspect and verify.

### Bytecode

`Runtime/Windvale.Bytecode/` owns:

- Immutable module records
- Canonical six-section serialization
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

The initial golden modules are:

- `Sumˉdata`: `faf44208d41c852f575e4f3025b0722c8fe6ee2d1c1a55b71b9e109c3eb54ef2`
- `Helloˉwindvale`: `fafbc14e7e82626bcfacf358f777c1b6ce6821a335677a35148da9f857eefed5`

Changes to those hashes require a reviewed bytecode/compiler-contract change rather than an automatic fixture refresh.

## Safety boundaries

- Source and module sizes are bounded before sustained processing.
- Binary lengths and offsets use checked conversions and remaining-buffer checks.
- The reader rejects malformed UTF-8, unsupported flags, version mismatches, missing bytes, and trailing bytes.
- The verifier rejects unknown opcodes, truncated operands, bad indices, invalid data uses, stack underflow, type mismatches, invalid branches, inconsistent merges, unreachable instructions, and invalid maximum-stack declarations.
- Hosted capabilities must be declared in the module and separately authorized by the embedding host.
- Runtime overflow, array bounds, instruction limits, and call-depth limits fail with stable runtime codes.

## Deliberate Seed limits

Seed does not include optimization, heap allocation, garbage collection, mutable arrays, structs, floating point, exceptions, threads, async work, raw pointers, foreign calls, dynamic linking, a native backend, assembler, linker, or operating-system code.

These are scope boundaries, not assertions that the current language model is final.
