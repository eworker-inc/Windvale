# Windvale language design

## Status

Accepted evolution direction under [Decision 0179](../Decisions/0179-Language-Application-And-Capability-Metadata-Direction.md) and [Decision 0184](../Decisions/0184-Language-Syntax-And-Operator-Evolution.md). The implemented language remains exactly [Windvale Seed](../../Specifications/Seed-Language.md); examples in this document marked as future syntax are not accepted source today.

## Product character

Windvale is a deterministic, capability-oriented language for applications and systems. It should feel direct and approachable without making host behavior, allocation, authority, mutation, or failure implicit.

The language therefore prefers:

- immutable `let` and visible mutable `var`;
- explicit typed constants without mutable module globals;
- explicit public types and local inference only from one initializer;
- nominal records, enums, and future payload variants;
- checked fixed-width arithmetic and no implicit conversions;
- recoverable typed results rather than general exceptions;
- bounded builders and immutable published collections;
- explicit capability requirements and scoped resource ownership;
- canonical verified WVB independent of interpreter, JIT, AOT, OS, or browser execution; and
- simple delimited grammar with deterministic diagnostics and tooling.

Windvale does not seek distinctiveness through unusual punctuation. Its identity comes from verification, capabilities, bounded resources, deterministic artifacts, and one language that can cross application and system boundaries.

## Implemented Seed surface

Seed currently has:

- one module per source file with `portable`, `hosted`, or `system` profile;
- source imports and required capability declarations;
- immutable `text`, `bytes`, and `[i32]` module data;
- explicit-value enums and immutable nominal records;
- typed functions, parameters, `let`, and `var` locals;
- expression statements, assignment to mutable locals, `if`/`else`, `while`, and `return`;
- checked `i32`, `i64`, `u32`, and `u64` arithmetic, byte values, Boolean logic, numeric comparison, enum equality, calls, indexing, and field access; and
- bounded Foundation operations for text, bytes, encoding, formatting, and SHA-256.

Seed intentionally lacks module aliases and private dependency helpers, scalar constants, local inference, named record literals, short-circuit Boolean operators, `break`, `continue`, match, payload variants, general collections, structured resource scope, generics, closures, async, floating point, and unsafe source operations.

## Near-term source direction

The first syntax growth is mostly sugar over existing semantics:

```text
let Length = Bytesˉlength(Value);
var Index = 0u32;

const MAXIMUM_REQUESTS: u32 = 256u32;

let Request = Readˉrequest {
    Name: Name,
    Offset: 0u64,
    Maximum: 4096u32,
};

if Ready && !Closed {
    Process(Request);
} else if Retry {
    continue;
}
```

Local inference never changes public signatures or invokes overload resolution. A typed constant uses only deterministic literal, enum, earlier-constant, and checked-operator expressions, rejects cycles and would-trap evaluation at compile time, and has no observable storage identity. Named records require every field once, evaluate field expressions in source order, and retain declaration-order value layout. `&&` and `||` are Boolean-only and short-circuit from left to right. `break` and `continue` target the nearest loop. Braces and semicolons remain mandatory; multiline comma-separated lists may retain a trailing comma.

## Module and authority direction

Modules need private helpers, explicit exports, aliases, and qualified references before the library graph becomes much larger:

```text
module Imageˉtool;

platform windows, linux, windvale;
authority application;
requires capability filesystem.directory version 1;
optional capability window.surface version 1;

import Foundationˉbytes as Bytes;
import Platformˉfilesystem as Filesystem;

export record Imageˉsummary {
    Width: u32;
    Height: u32;
}

export const MAXIMUM_IMAGE_BYTES: u32 = 4194304u32;

fn Decodeˉheader(Value: bytes) -> Imageˉsummary {
    // Private helper.
}
```

The metadata spelling remains a candidate until the versioned source/WVB encoding is selected. The durable rule is that platform scope, authority, required capabilities, optional capabilities, root approval, concrete grants, and provider binding remain separate.

## Recoverable values and matching

Windvale first adds exhaustive statement-form matching over existing enums. A later nominal `variant` carries payloads:

```text
variant Readˉresult {
    Success(Value: bytes);
    Failure(Error: Readˉerror);
}

match Result {
    case Readˉresult.Success(Value) {
        return Value;
    }
    case Readˉresult.Failure(Error) {
        return Recover(Error);
    }
}
```

Match has no fallthrough and is exhaustive. The first result flow remains explicit. A later `try` expression may provide visible propagation after success/failure shape, ownership, cleanup, and return compatibility are exact. Traps remain for contract violations, corrupted state, invalid bounds, and other runtime invariants; expected provider outcomes use typed results.

## Bounded collections and resources

The first dynamic collection family is an immutable bounded sequence plus uniquely owned builder:

```text
var Pending: builder<Request, 256> = builder<Request, 256>();
Pending.Push(First);
Pending.Push(Second);

let Published: sequence<Request, 256> = freeze Pending;

for Request in Published {
    Submit(Request);
}
```

The exact constructor and member spelling remains part of the focused collection decision. The semantic requirements are explicit maximum, checked allocation, visible mutation, consuming freeze, immutable publication, deterministic iteration, and compile-time rejection of builder use after freeze.

A later `using` declaration scopes one owned capability and closes it on ordinary control-flow exits. Terminal process cleanup remains the runtime or kernel boundary; the language does not pretend that arbitrary user cleanup executes after corruption. Package resources may use typed declarations supplied by an immutable content-addressed manifest rather than native paths.

## Operators

### Assignment and equality

`=` is declaration initialization or assignment to a `var` local. It is not an expression, cannot be chained, and cannot appear as a condition. `==` and `!=` are equality operators over two values of the same exact admitted type. Windvale does not add `===` or expose backing identity for ordinary immutable values.

Scalar, Boolean, and same-nominal-enum equality is implemented. Future text equality compares the exact Unicode scalar sequence represented by strict UTF-8 without normalization or locale behavior; bytes equality compares exact octets. Both are bounded and charged content operations. Records, variants, sequences, and maps require explicit bounded derived-equality rules before receiving operators. Builders, capabilities, functions, and resources do not have general equality.

### Arithmetic

`+`, binary `-`, and `*` remain checked same-type numeric operations. Unary `-` is signed-only. `u8` must be widened explicitly before arithmetic. Text, bytes, sequences, and user types do not overload arithmetic punctuation.

Future `/` and `%` trap on zero. Signed minimum with divisor `-1` traps for both operators, quotient otherwise truncates toward zero, and remainder follows the dividend sign while satisfying the quotient/remainder identity for every accepted pair.

### Ordering and Boolean logic

`<`, `<=`, `>`, and `>=` are numeric-only. Text collation, byte order, enum order, version order, and locale behavior use named contracts. `!`, future `&&`, and future `||` are Boolean-only; there is no truthiness conversion.

### Bitwise operations

Future `&`, `|`, `^`, `~`, `<<`, and `>>` begin with unsigned fixed-width integers. Shift counts are `u32`, must be below the operand width, and otherwise trap. Right shift fills with zero; left shift discards bits beyond the fixed width. Rotates remain named operations. Signed shifts wait for explicit reinterpretation and measured consumers.

### Precedence

Strongest to weakest:

1. postfix call, index, field, and qualification;
2. unary `!`, `~`, and `-`;
3. `*`, `/`, `%`;
4. `+`, `-`;
5. `<<`, `>>`;
6. `<`, `<=`, `>`, `>=`;
7. `==`, `!=`;
8. `&`;
9. `^`;
10. `|`;
11. `&&`;
12. `||`.

Binary operators are left-associative and operands evaluate left to right. Assignment has no precedence because it is not an expression. User code cannot define operators or precedence.

## Later surface

General generics, value-producing conditionals and matches, bounded interpolation, async/await, function values, closures, floating point, and visible unsafe blocks require focused ownership, ABI, resource, and consumer decisions. Unsafe machine or memory operations must eventually be visible both on their declaration and at their use; system profile alone is not sufficient syntax.

Classes, inheritance, implicit null, implicit conversions, general exceptions, operator overloading, inferred overload selection, unrestricted macros, preprocessors, ambient reflection, whitespace-sensitive blocks, hidden capability acquisition, and unbounded collections are not accepted directions.

## Evolution order

1. Local inference, typed constants, trailing commas, named records, and `else if`.
2. `break`, `continue`, `&&`, and `||`.
3. Module privacy, broader exports, aliases, and qualification.
4. Independent platform, authority, required-capability, and optional-capability syntax and encoding.
5. Exhaustive enum match.
6. Payload variants and typed recoverable results.
7. Bounded sequences/builders, consuming freeze, and bounded `for`.
8. Typed capabilities, `using`, and package-backed resources.
9. Later operators and advanced syntax only from measured consumers.

Every implemented slice advances the reference and Windvale compilers, editor package, specifications, WIR/WVB contracts where affected, interpreter/native/Wasm consumers where supported, malformed cases, deterministic bytes, and cross-host evidence together.
