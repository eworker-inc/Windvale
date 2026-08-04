# Decision 0184: Language syntax and operator evolution

- Date: 2026-08-03
- Status: Accepted architecture direction; implementation-sequence slices 1 through 8 and the coherent local change-aware gate pass, with independent cross-host qualification pending
- Refines: [Decision 0179](0179-Language-Application-And-Capability-Metadata-Direction.md) and [Decision 0180](0180-Compiler-Runtime-And-Native-Toolchain-Boundaries.md)
- Architecture: [Language design](../Architecture/Language-Design.md)
- Retains: explicit types at public boundaries, immutable `let`, mutable `var`, checked arithmetic, no implicit conversions, deterministic left-to-right evaluation, braces, semicolons, and canonical WVB semantics

## Context

Windvale Seed already supports modules, imports, capabilities, immutable data, nominal records and enums, typed functions and locals, lexical blocks, `if`/`else`, `while`, return, calls, field access, indexing, checked arithmetic, comparison, text, and bytes. Its main limitations are source verbosity, global declaration pressure during composition, status-record control flow, the absence of bounded runtime collections, and the absence of structured capability lifetime.

Syntax growth affects the Stage 0 compiler, Windvale-written lexer/parser/bindings/WIR/backend, WVB verifier and runtime where semantics expand, editor grammar, examples, malformed-input coverage, and bootstrap convergence. The first additions should therefore improve useful programs while lowering to existing WIR and WVB contracts whenever possible.

## Implementation progress

The first candidate slice implements exact initializer inference for `let` and `var` plus trailing commas in multiline parameter, call, positional-record, and static-data lists. The second implements typed constants in Stage 0 and the Windvale-written compiler. Constants lower to retained literal and enum operations and create no runtime storage or WVB export. The third implements named record literals and block-form `else if` in both compilers and editor tooling. Named fields lower through the existing record-create operation after source-order evaluation and declaration-order operand placement; `else if` lowers through existing conditional blocks and branches.

The fourth candidate implements nearest-loop `break` and `continue`, Boolean-only left-to-right short-circuit `&&` and `||`, and `+=`, `-=`, and `*=` for mutable locals. Stage 0 and the Windvale-written compiler agree on exact WVB bytes for a control oracle whose skipped operand would trap. WVIR appends a structurally verified `Boolˉphi = 64`; WVB adds no opcode and materializes that value on the two predecessor jumps. Compound assignment uses one target load and one store. The current Windvale WVB 1.6 lowering admits `i32` and `u32` compound arithmetic while Stage 0 also follows its existing `i64` and `u64` arithmetic contract.

The next two candidates implement private-by-default source modules with explicit export, alias qualification, deterministic internal identities, and the independently encoded platform/authority/required/optional metadata contract. Dependency data, constants, nominal types, variants, and functions are visible only through an explicit alias and export. WVB 1.8 carries the metadata contract; later feature-bearing minors retain an explicit metadata-presence byte.

Exhaustive enum match, nominal single-payload variants, and explicit recoverable-result flow are implemented through Decisions 0184 and 0199. WVB 1.9 adds variant types and create/test/payload operations. Decision 0200 then implements bounded sequences, affine builders, consuming `freeze`, `push`, and deterministic `for` through WVB 1.10.

The operator candidate implements checked `/` and `%`, unsigned `&`, `|`, `^`, `~`, `<<`, and `>>`, plus exact text and bytes equality. WVB 1.11 appends the complete Stage 0 opcode family; the Windvale-written compiler emits its current `i32`, `u8`, `u32`, text, and bytes subset. The shared operator oracle is 2,623 bytes with SHA-256 `26fcb52bc7e893d306d6f12343fa596bdbf919c95c74f033ef67c2afaf46a210`, compares byte-for-byte across both compilers, verifies, and executes with result `28`.

Typed capability values, `using`, and package-backed resources are not silently inferred from this sequence. The accepted architecture still requires a focused decision selecting their value representation, provider protocol, cleanup-failure semantics, and immutable resource manifest before implementation.

Stage 0 covers the complete current scalar widths. The Windvale-written compiler covers `i32`, `u8`, `u32`, `bool`, text, bytes, nominal values, variants, and collections pending its separately verified `i64`/`u64` adoption. These are local deterministic implementation results, not a new cross-host qualification claim.

## Decision

### Keep active development free of compatibility debt

Through at least September 3, 2026, and afterward until a named decision replaces this policy, Windvale source syntax, compiler IR, WVB, and related experimental tool formats carry no backward-compatibility promise. An accepted replacement may remove the obsolete spelling or encoding directly and migrate repository source, fixtures, generated artifacts, tools, and tests together. Readers need not accept superseded experimental versions, and implementations must not add dual parsers, translation shims, or compatibility aliases merely to preserve them.

This policy does not permit silent semantic drift: the current contract remains explicit, versioned, bounded, and verified. Historical qualification evidence remains historical evidence, not a supported-input promise. A compatibility case becomes binding only when a later named decision identifies the exact source or binary contract, supported versions, duration, consumers, and retirement rule.

### Keep the grammar explicit and bounded

- Retain braces and semicolons. Do not make indentation or automatic semicolon insertion semantic.
- Retain explicit-width numeric literals, exact same-type operators, and no implicit numeric, Boolean, enum, text, or capability conversions.
- Require explicit parameter, return, record-field, exported-data, resource, and public capability types.
- Permit local `let` or `var` type inference only from one initializer with one exact type. Inference does not cross function boundaries and does not select overloads.
- Permit a trailing comma in multiline comma-separated parameter, argument, and record-literal field lists. Declaration bodies retain semicolon terminators for record fields, enum members, variant cases, data declarations, and imports.
- Preserve deterministic left-to-right operand and argument evaluation.
- Keep parsing independent of host types, reflection, and arbitrary user extensions.

### Improve the existing surface before adding value shapes

Add, in order:

1. local initializer type inference;
2. explicitly typed module `const` declarations;
3. named record literals with every field exactly once and declaration-independent field order;
4. block-form `else if`;
5. `break` and `continue` scoped to the nearest loop;
6. short-circuit Boolean `&&` and `||`; and
7. compound assignment for a mutable local only after the underlying operator is implemented.

The first constant form is deliberately smaller than general compile-time execution:

```text
const MAXIMUM_RECORDS: u32 = 4096u32;
```

A constant has an explicit scalar, Boolean, or enum type. Its initializer uses literals, enum members, earlier constants, and already accepted checked operators only; it cannot call a function or capability, allocate a dynamic value, inspect a target, or depend on compiler traversal order. Forward references and constant cycles are rejected. Evaluation uses the ordinary checked operator contract, and any would-trap operation is a compile-time diagnostic rather than a runtime initializer. Constants are private unless exported, have no observable address or backing identity, and may be inlined deterministically. Windvale does not add mutable module globals.

Named record construction becomes the preferred surface:

```text
let Request = Readˉrequest {
    Name: Name,
    Offset: 0u64,
    Maximum: 4096u32,
};
```

Every declared field must appear exactly once; missing, duplicate, and unknown fields are rejected. Field expressions evaluate left to right in source order, while the constructed nominal value retains canonical declaration-order layout and identity. The compiler may retain positional record construction only during repository migration. A later implementation decision may remove it rather than maintain two permanent construction styles.

Compound assignment evaluates its target once and uses the same checked semantics as the underlying operation. Do not add `++` or `--`.

### Make module visibility and qualification explicit

- Permit `export` on functions, records, enums, constants, immutable data, and later resources. Unmarked declarations are private to their module.
- Stop requiring every function in an imported module to be exported. Private helpers internalize with their owning module but cannot be named by importers.
- Add one explicit alias per import and resolve exported declarations through that alias, using a qualified form such as `Bytes.Length(Value)`.
- Do not add wildcard imports, ambient open namespaces, transitive import leakage, or resolution based on dependency order.
- Keep declaration identities canonical and module-owned even when a local import alias changes.

Decision 0179's platform scope, authority, required capabilities, and optional capabilities remain four separate source/package/module dimensions. Candidate declaration-style spelling may place them after the module header, for example:

```text
module Imageˉtool;

platform windows, linux, windvale;
authority application;
requires capability filesystem.directory version 1;
optional capability window.surface version 1;
```

Those lines demonstrate separation, not a frozen platform-identity grammar. Application approval, provider grants, and runtime bindings remain outside library source.

### Add exhaustive matching before payload variants

First add a statement-form `match` over one nominal enum value:

```text
match Status {
    case Readˉstatus.Valid {
        return Value;
    }
    case Readˉstatus.Notˉfound {
        return Empty;
    }
}
```

The first form is exhaustive, has no fallthrough, admits no implicit integer cases, and requires every member unless a later decision defines an explicit wildcard policy. It is not initially a value-producing expression.

After enum matching is qualified, add a distinct nominal payload-bearing `variant` rather than changing the existing explicit-`i32` enum contract:

```text
variant Readˉresult {
    Success(Value: bytes);
    Failure(Error: Readˉerror);
}
```

`match` binds the selected case payload. Construction, matching, ownership, default values, WVB encoding, verifier flow, native representation, and malformed-input behavior require one focused value-shape decision.

Use explicit `match` as the first recoverable-result flow. A later `try` expression may unwrap one recognized success case or return a compatible failure from the current function after result representation and ownership are qualified. Do not introduce general exceptions, catchable traps, or punctuation-only propagation first.

### Add bounded collections and lifetime syntax after results

The first collection surface uses a type, element type, and explicit maximum, conceptually:

```text
sequence<Item, 256>
builder<Item, 256>
```

A uniquely owned builder supports bounded mutation. `freeze Builder` consumes the builder, publishes one immutable sequence, and makes later builder use a compile error. The first `for` iterates only over statically recognized bounded sequences and lowers to checked index/length control. General iterators, lazy streams, generators, and unbounded collection types remain later work.

When typed capability values exist, add a scoped owned-resource declaration such as `using File = ...`. It closes the resource on ordinary block exit and recoverable early return. Process termination remains the runtime/kernel cleanup boundary after a terminal trap; user cleanup code is not promised to run after terminal corruption. General `defer` waits for exact ordering and cleanup-failure semantics.

Package-backed immutable data may later use a typed declaration such as `resource Schema: bytes;`. A package manifest supplies the exact content identity; source does not include ambient native paths.

### Keep arithmetic operators narrow and checked

`+`, binary `-`, and `*` remain defined only for two operands of the same admitted numeric type. Results use that same type and trap deterministically on signed overflow or unsigned overflow/underflow. Unary `-` remains signed-only. `u8` remains a byte value and requires explicit widening before arithmetic.

Do not use `+` for text concatenation, byte concatenation, sequence append, or user-defined types. Those operations can allocate and have different limits; keep them visible through exact library operations, builders, or later bounded interpolation.

Add `/` and `%` together only for a measured consumer. Their eventual contract is:

- operands and result have the same integer type;
- division by zero traps;
- signed minimum with divisor `-1` traps for both `/` and `%`, keeping the pair's exceptional preconditions exact across native and WebAssembly engines;
- signed quotient truncates toward zero;
- for every accepted operand pair, remainder satisfies `Dividend = Quotient * Divisor + Remainder` and has the dividend's sign or is zero; and
- no host divide behavior, signal, or machine exception becomes the language result.

### Keep assignment distinct from equality

- `=` initializes a declaration or assigns to an already declared mutable local. It is a statement construct and produces no value.
- Assignment to a `let`, parameter, immutable field, sequence, published value, or borrowed view is rejected.
- Do not add chained assignment, assignment inside conditions, or `:=` inference.
- `==` and `!=` compare two values of the same exact admitted type and always produce `bool`. No conversion participates.
- Do not add `===` or expose backing-storage identity for ordinary immutable values.

Existing scalar, Boolean, and same-nominal-enum equality remains. Add text and bytes content equality when bounded implementations exist: text compares the exact Unicode scalar sequence represented by strict UTF-8 without normalization or locale behavior, and bytes compares exact octets. Comparison work is charged and bounded by the admitted values. The result cannot observe shared backing, slice origin, or host encoding.

Do not automatically give every record, variant, sequence, map, builder, capability, function, or resource `==`. A later explicit derived-equality contract may admit immutable nominal values only when every field is equality-capable and the maximum comparison work is bounded. Builders, capabilities, functions, and resources do not receive general equality.

### Keep ordering and Boolean operations type-specific

- `<`, `<=`, `>`, and `>=` remain same-type numeric ordering.
- Text collation, byte ordering, enum ordering, semantic-version ordering, and locale behavior use explicitly named functions or separately accepted contracts rather than these operators.
- `!`, `&&`, and `||` accept and return only `bool`.
- `&&` and `||` evaluate the left operand first and evaluate the right operand only when required.
- Windvale has no integer truthiness, null truthiness, implicit optional test, or overloaded Boolean conversion.

### Add bitwise operators only with fixed-width unsigned semantics

When a measured cryptographic, binary, compiler, runtime, or OS consumer requires them, add `&`, `|`, `^`, unary `~`, `<<`, and `>>` first for `u8`, `u32`, and `u64` only.

- Operands of `&`, `|`, and `^` have the same type and produce that type.
- `~` flips exactly the bits of its operand width.
- A shift count is `u32` and must be less than the left operand width; an out-of-range count traps rather than being masked.
- `>>` is logical and shifts in zero bits.
- `<<` is a fixed-width bit operation and discards bits shifted beyond the width; this is deliberately different from checked numeric multiplication.
- Rotates remain explicitly named operations unless a later syntax decision finds a clear operator form.
- Signed bitwise and right-shift behavior wait for a measured case and explicit reinterpretation rules.
- `&` and `|` are never Boolean substitutes for `&&` and `||`.

### Freeze conventional precedence

When all listed families exist, precedence from strongest to weakest is:

1. postfix call, index, field, and qualified access;
2. unary `!`, `~`, and unary `-`;
3. `*`, `/`, and `%`;
4. `+` and binary `-`;
5. `<<` and `>>`;
6. `<`, `<=`, `>`, and `>=`;
7. `==` and `!=`;
8. `&`;
9. `^`;
10. `|`;
11. `&&`; and
12. `||`.

All binary operators are left-associative. Assignment is not an expression and therefore has no precedence. Chained comparison such as `A < B < C` is rejected by types; write `A < B && B < C`. Formatters and diagnostics should encourage parentheses when bitwise, equality, and Boolean families are mixed.

### Defer features with larger semantic cost

General generics, value-producing `if`/`match`, string interpolation, asynchronous functions, `await`, function values, closures, floating point, visible `unsafe fn`/`unsafe` blocks, and user-extensible derivation follow only after their ownership, resource, ABI, and consumer evidence exists.

Do not add classes, inheritance, implicit null, operator overloading, overload resolution by inferred argument types, unrestricted macros, preprocessors, ambient reflection, whitespace-sensitive blocks, hidden capability acquisition, or unbounded collections.

## Implementation sequence

1. Specify and implement local inference, typed constants, uniform trailing commas, named record literals, and `else if` in both compiler paths and editor tooling without changing WVB semantics.
2. Add `break`, `continue`, `&&`, `||`, and mutable-local compound assignment through typed WIR control and existing WVB branches.
3. Add private dependency helpers, `export` on nominal declarations, import aliases, and qualified name resolution while preserving canonical declaration identities.
4. Select and version the independent platform/authority/required/optional metadata grammar and encoding under Decision 0179.
5. Add exhaustive statement-form enum `match` through existing enum and branch operations.
6. Specify and qualify one payload `variant` WVB shape and typed recoverable-result flow.
7. Add bounded sequence/builder values, consuming `freeze`, and bounded `for`.
8. Add typed capability values, scoped `using`, and package-backed `resource` only from real filesystem or service consumers.
9. Add later operator families and advanced syntax only from measured consumers.

Each slice updates Stage 0, the Windvale compiler front end and WIR, WVB only when required, runtime/backends, editor grammar, exact fixtures, malformed cases, byte reproducibility, and cross-host evidence together. Do not accumulate a second experimental parser or compiler pipeline.

## Consequences

Near-term Windvale source becomes substantially easier to read without weakening explicit types, mutability, authority, allocation, or checked execution. Module qualification and privacy allow libraries to grow without globally prefixed declaration names or accidental visibility.

Exhaustive enum matching provides immediate value before payload variants change the value model. Variants then support typed operational results and optional values without exceptions or null. Bounded sequences and affine builders now carry their verified ownership contract; resource scopes still wait for a separate provider/value/cleanup/manifest decision.

Operators remain predictable and non-extensible. Numeric notation cannot silently allocate text, invoke user code, acquire authority, compare native handles, or inherit host overflow and shift behavior.

The implemented candidate slices append the `const`, loop-control, Boolean, and compound-assignment tokens; append named-record expression, loop-control statement, loop-placement failure, and Boolean-phi identities; advance WVLB and WVSD minor versions to 1.1; update editor/examples; and change compiler artifact identities. They preserve retained fixture bytes where the source semantics are unchanged: inferred annotations and constant declarations disappear during typed lowering, named fields remap to the existing declaration-order record operation, `else if`, `break`, and `continue` use ordinary branches, and `Boolˉphi` is materialized without a new WVB opcode.

The completed coherent batch expands beyond this decision's intermediate capacity measurements. [Decision 0201](0201-Expanded-Exact-Compiler-Native-Capacity.md) owns the current 26,299,864-byte native image, 32 MiB explicit large-native admission, measured 104,885,093-byte dynamic peak, 128 MiB ordinary/version-2/3 arena, and 48,000,000,000-instruction compiler ceiling. ABI 22, context 7, the 32-bit capacity field, individual value limits, and narrow version-1 container capacities remain unchanged. The successor capacity is locally verified and still requires dual-host qualification before it becomes a new cross-host claim.

## Rejected alternatives

- **Add classes, methods, inheritance, generics, exceptions, and async together:** creates several interacting object, lifetime, dispatch, and control-flow systems before the current compiler is retired.
- **Make every convenience an operator:** hides allocation, large comparison work, capability behavior, and user dispatch behind punctuation.
- **Use `=` for equality or allow assignment expressions:** makes conditions and data flow less reviewable and creates accidental mutation pressure.
- **Make arithmetic wrap like the host machine:** violates the accepted checked language semantics and breaks differential execution.
- **Give every immutable value automatic deep equality:** creates hidden work and unclear behavior for later resources, cycles, and large collections.
- **Use signed shifts immediately:** forces an arithmetic-versus-logical right-shift choice before a consumer and reinterpretation contract exist.
- **Add a general preprocessor or unrestricted macros:** weakens source identity, diagnostics, tooling, and reproducible compilation.

## Reconsider when

- named record literals or aliases materially harm compact generated or systems source;
- a real language workload requires positional product values distinct from records;
- exhaustive statement-form matching prevents clear immutable control flow;
- explicit text or byte operations are too verbose after builders and bounded interpolation exist;
- measured cryptographic or systems code requires signed bitwise behavior;
- a useful generic or asynchronous abstraction cannot be expressed without changing the proposed ordering; or
- implementing a slice in both compiler paths creates more risk than completing the relevant native-retirement gate first.
