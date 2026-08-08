# Windvale language design

## Status

Accepted evolution direction under [Decision 0179](../Decisions/0179-Language-Application-And-Capability-Metadata-Direction.md), [Decision 0184](../Decisions/0184-Language-Syntax-And-Operator-Evolution.md), [Decision 0199](../Decisions/0199-Nominal-Payload-Variants-And-Recoverable-Results.md), and [Decision 0200](../Decisions/0200-Bounded-Sequences-Affine-Builders-And-For.md). Proposed [Decision 0198](../Decisions/0198-Next-Integrated-Architecture-Defaults.md) adds recommended successor defaults for review. The implemented language remains exactly [Windvale Seed](../../Specifications/Seed-Language.md); examples explicitly marked as future syntax are not accepted source today.

Windvale is in active early development. Through at least September 3, 2026, and until a later named decision says otherwise, obsolete source spellings, compiler models, and experimental binary encodings may be replaced without backward readers or migration layers. The repository moves as one contract; old qualification artifacts remain evidence rather than supported inputs.

## Product character

Windvale is a deterministic, capability-oriented language for applications and systems. It should feel direct and approachable without making host behavior, allocation, authority, mutation, or failure implicit.

The language therefore prefers:

- immutable `let` and visible mutable `var`;
- explicit typed constants without mutable module globals;
- explicit public types and local inference only from one initializer;
- nominal records, enums, and payload variants;
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
- explicitly typed, storage-free scalar and enum constants;
- explicit-value enums and immutable nominal records;
- typed functions and parameters plus explicitly typed or initializer-inferred `let` and `var` locals;
- expression statements, simple and compound assignment to mutable locals, `if`/`else if`/`else`, `while`, nearest-loop `break` and `continue`, and `return`;
- checked `i32`, `i64`, `u32`, and `u64` arithmetic, byte values, short-circuit Boolean logic, numeric comparison, enum equality, calls, indexing, and field access; and
- named and retained positional record construction plus bounded Foundation operations for text, bytes, encoding, formatting, and SHA-256; and
- trailing commas in multiline parameter, argument, named-record, positional-record, and static-data lists;
- explicit import aliases, private-by-default declarations, qualified exported data/constants/types/functions, and independent module metadata;
- exhaustive enum and payload-variant matching with explicit recoverable-result values;
- bounded immutable sequences, affine builders, consuming `freeze`, explicit `push`, and deterministic `for`; and
- checked division/remainder, unsigned bitwise/shifts, and exact text/bytes equality.

Seed intentionally lacks dynamic linking, typed capability values, structured resource scope, package-backed resources, maps/sets and other general collections, generics, closures, async, floating point, and unsafe source operations.

## Near-term source direction

The first syntax-growth batch is mostly sugar over existing semantics. Local inference, multiline trailing commas, typed constants, named records, and block-form `else if` are implemented:

```text
let Length = Bytesˉlength(Value);
var Index = 0u32;

const MAXIMUM_REQUESTS: u32 = 256u32;

let Request = Readˉrequest {
    Name: Name,
    Offset: 0u64,
    Maximum: 4096u32,
};

if Ready {
    Process(Request);
} else if Retry {
    Retryˉlater(Request);
}
```

Local inference never changes public signatures or invokes overload resolution. A typed constant uses only deterministic literal, enum, earlier-constant, and checked-operator expressions, rejects cycles and would-trap evaluation at compile time, and has no observable storage identity. Named records require every field once, evaluate field expressions in source order, and retain declaration-order value layout. Braces and semicolons remain mandatory; multiline comma-separated lists may retain a trailing comma.

The second syntax-growth slice implements Boolean-only, left-to-right short-circuit `&&` and `||`, nearest-loop `break` and `continue`, and `+=`, `-=`, and `*=` assignment to mutable locals:

```text
if Ready && !Closed {
    Process(Request);
} else if Retry {
    continue;
}

Attempts += 1u32;
```

The skipped operand of a short-circuit expression is not evaluated and therefore cannot call, mutate, allocate, or trap. `break` exits and `continue` restarts the nearest enclosing `while`; both are rejected outside a loop. Compound assignment reads its target exactly once before evaluating the right operand, applies the same checked arithmetic and exact-type rules as the underlying operator, and stores the result only if evaluation succeeds.

## Module and authority direction

Modules now have private helpers, explicit exports, aliases, qualified references, and independent metadata:

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

The source spelling and WVB 1.8+ encoding are implemented. Platform scope, authority, required capabilities, optional capabilities, root approval, concrete grants, and provider binding remain separate. Typed capability values and provider instances still require the later focused decision described below.

The recommended encoding gives the platform dimension structured, independently ordered fields for environment, architecture, ABI, and named extension requirements. Authority is a separate closed role value. Required and optional capability tables contain canonical ASCII-safe interface identity, major contract version, exact signature-set identity, and declared limit profile. Provider identity, user approval, and granted capability references never enter those requirement tables.

The current `portable`, `hosted`, and `system` profile remains defined only by its existing WVB version. The metadata revision should use a new source edition and WVB version with no overloaded legacy byte. Migration recompiles current source and fixtures into the new canonical tables. Whether a legacy reader remains for one named recovery case is a separate compatibility decision; normal development does not preserve the old format automatically, and no old profile receives inferred new authority. A part's compatibility is derived from its complete dependency graph, not from a blanket portable label.

The exact surface spelling remains reviewable. A preferred source shape keeps declarations simple while allowing the compiler to serialize structured records, for example:

```text
platform windows, linux, windvale;
architecture x64;
authority application;
requires capability filesystem.directory version 1;
optional capability terminal.surface version 1;
```

Omitting a dimension is an error rather than an implicit host default. A later `architecture any` or shared-environment shorthand may be admitted only when it expands to one canonical explicit meaning.

## Recoverable values and matching

Windvale implements exhaustive statement-form matching over enums and nominal payload variants:

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

Match has no fallthrough and is exhaustive. Result flow remains explicit. A later `try` expression may provide visible propagation after success/failure shape, ownership, cleanup, and return compatibility are exact. Traps remain for contract violations, corrupted state, invalid bounds, and other runtime invariants; expected provider outcomes use typed results.

The recommended variant contract is nominal, immutable, closed, and verifier-bounded. Every case has a stable declaration ordinal and zero or more named fields. Construction names the variant and case and supplies every payload field exactly once. There is no implicit null case, default value, open extension, integer conversion, or layout inspection.

WVB records the nominal type, ordered cases, field types, ownership classes, and maximum admitted value pressure. Construction consumes or retains payload evidence according to each field's ownership class. `match` validates one arm per case, refines the selected payload types inside that arm, has no fallthrough, and rejoins only with compatible stack and ownership state. The native inline-or-descriptor representation remains an ABI choice and cannot be observed by source.

The first typed results are ordinary two-case nominal variants, conventionally `Success` and `Failure`, with explicit exhaustive `match`. Windvale does not need general generics, exceptions, or a magic built-in result to gain recoverable operations. A later visible propagation expression requires one recognized result contract, compatible failure type, exact cleanup ordering, and no hidden capability calls.

## Bounded collections and resources

The first dynamic collection family is the implemented immutable bounded sequence plus uniquely owned builder:

```text
var Pending: builder<Request, 256> = builder<Request, 256>();
push Pending, First;
push Pending, Second;

let Published: sequence<Request, 256> = freeze Pending;

for Request in Published {
    Submit(Request);
}
```

The spelling and WVB 1.10 contract are selected by Decision 0200. The semantics retain an explicit maximum, checked allocation, visible mutation, consuming freeze, immutable publication, deterministic iteration, and compile-time rejection of builder use after freeze.

The recommended first family treats the maximum as part of the exact type. `sequence<Item, 256>` and `sequence<Item, 512>` are different types unless an explicit checked conversion copies or republishes the value. Length varies from zero through the maximum; iteration order is insertion order. Indexing outside current length traps as a contract violation.

`builder<Item, N>` is uniquely owned and move-only. A one-item `Push` either completes or reports the builder unchanged; a later bulk operation may report an exact completed prefix. Capacity exhaustion is a typed recoverable result rather than a trap, and no operation grows beyond `N`. `freeze` consumes the builder and publishes one immutable sequence. A sequence may later share backing or provide borrowed slices, but storage identity, capacity beyond the declared maximum, and reference count remain unobservable.

The first implementation may use contiguous bounded storage and explicit ownership without a tracing collector. Cycles, unbounded growth, lazy iterators, general collection generics, covariance, and concurrent mutation remain outside this family.

A later `using` declaration may scope an affine resource or capability value only
when its contract gives the caller an ordinary close operation. A prebound or
shared provider reference is not implicitly owned or closed by lexical scope.
Terminal process cleanup remains the runtime or kernel boundary; the language does
not pretend that arbitrary user cleanup executes after corruption. Package
resources may use typed declarations supplied by an immutable content-addressed
manifest rather than native paths.

## Operators

### Assignment and equality

`=` is declaration initialization or assignment to a `var` local. It is not an expression, cannot be chained, and cannot appear as a condition. `==` and `!=` are equality operators over two values of the same exact admitted type. Windvale does not add `===` or expose backing identity for ordinary immutable values.

Scalar, Boolean, same-nominal-enum, text, and bytes equality is implemented. Text compares the exact Unicode scalar sequence represented by strict UTF-8 without normalization or locale behavior; bytes compares exact octets. Records, variants, sequences, and maps require explicit bounded derived-equality rules before receiving operators. Builders, capabilities, functions, and resources do not have general equality.

### Arithmetic

`+`, binary `-`, and `*` remain checked same-type numeric operations. Unary `-` is signed-only. `u8` must be widened explicitly before arithmetic. Text, bytes, sequences, and user types do not overload arithmetic punctuation.

`/` and `%` trap on zero. Signed minimum with divisor `-1` traps for both operators, quotient otherwise truncates toward zero, and remainder follows the dividend sign while satisfying the quotient/remainder identity for every accepted pair.

### Ordering and Boolean logic

`<`, `<=`, `>`, and `>=` are numeric-only. Text collation, byte order, enum order, version order, and locale behavior use named contracts. `!`, `&&`, and `||` are Boolean-only; there is no truthiness conversion.

### Bitwise operations

`&`, `|`, `^`, `~`, `<<`, and `>>` operate on unsigned fixed-width integers. Shift counts are `u32`, must be below the operand width, and otherwise trap. Right shift fills with zero; left shift discards bits beyond the fixed width. Rotates remain named operations. Signed shifts wait for explicit reinterpretation and measured consumers.

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

## Post-.NET-retirement product-lane proposal

Completing native retirement should make the Windvale toolchain normal before it
makes the language broader. The proposed
[post-.NET-retirement language and library stage](../Project/Post-Dotnet-Retirement-Language-And-Libraries.md)
therefore recommends a package-backed useful application and compact library model
before a larger syntax batch. Source modules remain explicit dependencies with local
import aliases rather than a global hierarchical namespace; `Foundation`,
`Platform`, `Protocol`, and later `System` are cross-cutting library roles, not
ambient source names or an exhaustive folder hierarchy.

The recommended language order is typed rights-limited capability references and,
separately, scoped ownership for values with an explicit caller-controlled close
contract; then a narrow visible result-propagation form and one bounded associative
collection selected by measured consumers. General generics, richer aggregate
shapes, floating point, and structured concurrency remain later consumer-driven
features. A feature becomes available on native, WebAssembly, or Windvale OS only
after the affected target path implements and verifies it; source-only lowering
into existing verified operations need not create a new backend contract. Target
backends are not separate source languages and must not inherit support from another
runtime by implication.

## Evolution order

The first eight slices are implemented locally and await the final coherent-batch verification and cross-host qualification where required:

1. Local inference, typed constants, trailing commas, named records, and `else if`.
2. `break`, `continue`, `&&`, `||`, and mutable-local `+=`, `-=`, and `*=`.
3. Module privacy, broader exports, aliases, and qualification.
4. Independent platform, authority, required-capability, and optional-capability syntax and encoding.
5. Exhaustive enum match.
6. Payload variants and typed recoverable results.
7. Bounded sequences/builders, consuming freeze, and bounded `for`.
8. Division/remainder, unsigned bitwise/shifts, and text/bytes equality.
9. A package/library product baseline using current source semantics.
10. Typed capability references and scoped ownership where an exact close contract
    exists.
11. Narrow result propagation and one bounded associative collection, each only
    from measured consumers.
12. Later operators and advanced syntax only from measured consumers.

Every pre-freeze implemented slice advances the reference and Windvale compilers,
editor package, specifications, WIR/WVB contracts where affected,
interpreter/native/Wasm consumers where supported, malformed cases, deterministic
bytes, and cross-host evidence together. Under [Decision 0213](../Decisions/0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md), successor source features advance through the Windvale-owned compiler rather than adding new breadth to the frozen C# recovery compiler. They still require specifications, Windvale-owned verifier evidence, editor support, deterministic fixtures, and explicit target support before a native, WebAssembly, or OS claim.
