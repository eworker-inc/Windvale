# Windvale source-to-WVB backend

## Status and purpose

`Compilerˉsourceˉwvb` is the first portable Windvale-written executable backend. It consumes a validated WVSS 1 source set through `Compilerˉsourceˉwir`, lowers the accepted `WVIR 1` subset to a complete canonical WVB 1.6 module, and returns the bytes without using hosted capabilities.

This slice is intentionally narrow. Its purpose is to prove the complete source → symbols/bindings → typed WVIR → WVB → verifier → runtime path before data, nominal metadata, capabilities, and multi-module remapping enlarge the backend.

## Public result

```text
Compilerˉcompileˉsourceˉwvb(Input: bytes)
    -> Compilerˉsourceˉwvbˉsummary
```

On success, `Status` and `Wirˉstatus` are `Valid`, `Bytecode` contains one complete WVB 1.6 module, and the summary reports function and code-byte counts. On failure, `Bytecode` is empty and the summary identifies the first function and WVIR operation involved.

The status contract distinguishes upstream WVIR rejection, unsupported module counts, profiles, declarations, shapes and operations, noncanonical function order, and WVB limits.

## Initial accepted subset

The first backend accepts:

- exactly one `portable` source module;
- one or more private or exported functions already declared in strict ordinal name order;
- `void`, `i32`, `u8`, `u32`, and `bool` function returns, parameters, locals, and temporaries;
- constants, parameter/local load and store, function calls, signed and unsigned arithmetic, comparisons, equality, signed negation, and boolean negation; and
- explicit jump, branch, and return terminators produced by `if`, `else`, and `while`.

It deterministically rejects imports, capabilities, data, records, enums, `text`, `bytes`, Foundation intrinsic operations, and capability calls. These are expansion boundaries, not silently degraded programs.

Requiring source function declarations to be already ordinal keeps WVSD function identities, WVIR call targets, WVB function indices, code order, and export order identical in the first slice. A later metadata-remapping pass will remove that temporary restriction when multi-module and mixed-declaration lowering is added.

## Lowering contract

Every WVIR temporary becomes a WVB local after the function's parameter and user-local slots. Each operation loads its temporary operands, executes one WVB instruction, and stores a result temporary when present. The operand stack is therefore empty between WVIR operations and at every basic-block boundary.

The backend makes two deterministic passes over each function. The first computes every block byte offset, exact function code length, and maximum operand-stack depth. The second emits code using those offsets, so branches never require mutable backpatching.

Primitive WVIR shapes map to WVB shapes as follows:

| WVIR shape | Meaning | WVB shape byte |
| ---: | --- | ---: |
| 0 | `void` | 0 |
| 1 | `i32` | 1 |
| 2 | `u8` | 4 |
| 3 | `u32` | 5 |
| 4 | `bool` | 2 |

The encoder writes the fixed WVB 1.6 header followed by the canonical Module, Capabilities, Data, Functions, Code, Exports, and Types section envelopes. Capabilities, Data, and Types contain canonical zero counts in this subset. Function metadata includes user locals followed by temporary locals, contiguous code offsets, exact code lengths, and the computed maximum stack depth.

## Verification

The focused conformance test compiles the backend core, runs its portable acceptance/rejection demo, runs the hosted tool over `Tests/Fixtures/Source-Wvb/Function-Only.wv`, verifies the returned WVB with the mandatory Stage 0 verifier, executes it, and compares it byte for byte with the Stage 0 compiler output.

The fixture contains four functions and exercises all four accepted value shapes, function calls, mutable locals, `while`, `if`, arithmetic, comparisons, and boolean negation. Both backends currently produce the exact 815-byte WVB module with SHA-256 `9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761`; it executes with result `6`.

Current candidate bootstrap artifacts are:

- `Source-Wvb-Core.wvb`: 517,874 bytes, SHA-256 `d4846b2c0eed11e35a3f715e61efd84c676c1055c340c08acf582a2558bca9db`.
- `Source-Wvb-Demo.wvb`: 519,338 bytes, SHA-256 `d2477d6de0e90753c3f93b9ffc9db71da02a30472aca0a813ee4b6bf3ef5ec16`.
- `Source-Wvb-Tool.wvb`: 519,455 bytes, SHA-256 `58a337338aa98a225c563a49cfdffc9133988d332c1000391b99c0ef31e2edac`.

These identities are local candidate evidence until the exact commit passes Windows and Debian qualification.

## Expansion path

The next backend slices should add canonical remapping and serialization in measured order: text/bytes and static data, nominal records/enums, Foundation intrinsics, capabilities and hosted profiles, then multi-module input. Full bootstrap closure remains separate and still requires closing the current source-envelope and repeated-body-traversal performance gaps.

Optimization, native code, object emission, executable containers, and OS-specific lowering are not part of this contract.
