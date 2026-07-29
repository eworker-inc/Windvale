# Windvale Seed language specification

## Status and scope

This document specifies the source-language subset implemented by Windvale Seed. It is deliberately small and may break during early development. Source is UTF-8 and identifiers are case-sensitive ASCII names matching `[A-Za-z_][A-Za-z0-9_]*`.

## Module shape

Every source file contains one module declaration followed by zero or more capability declarations, immutable data declarations, and function declarations.

```text
module <Name> profile <portable|hosted|system>;
capability <qualified.name>;
data <Name>: <text|[i32]> = <literal>;
[export] fn <Name>(<parameters>) -> <type> { <statements> }
```

Seed accepts `system` as a profile value so the serialized contract is explicit, but it exposes no system capabilities or unsafe operations.

## Types

- `void` is valid only as a function or capability return type.
- `i32` is a signed 32-bit integer. Arithmetic overflow traps deterministically.
- `bool` contains only `true` or `false`.
- `text` is immutable, valid Unicode stored canonically as UTF-8 in modules.
- `[i32]` is immutable module data. It is not a general runtime array type in Seed.

Parameters and local variables may have `i32`, `bool`, or `text` type. Module data may be `text` or `[i32]`.

## Declarations

- Names within each declaration category are unique.
- Function and data names occupy separate namespaces.
- A module exports functions explicitly with `export`.
- `windvale run` looks for an exported function named `main` with signature `fn() -> i32`.
- Capability declarations must be unique and use qualified lowercase names.
- A portable module cannot declare or call hosted capabilities.

Seed defines one hosted capability:

```text
console.write_line(text) -> void
```

## Statements

```text
let <name>: <type> = <expression>;
<name> = <expression>;
<expression>;
if <expression> { <statements> } [else { <statements> }]
while <expression> { <statements> }
return [<expression>];
```

Blocks use lexical scope. Parameters and locals must have unique names within their function in Seed; nested shadowing is rejected to keep diagnostics and lowering simple. Statements after an unconditional return in the same block are rejected as unreachable.

## Expressions

Seed supports:

- Decimal `i32` literals
- `true` and `false`
- String literals with `\"`, `\\`, `\n`, `\r`, `\t`, and `\uXXXX` escapes
- Local and parameter reads
- Immutable text data reads
- Immutable integer data indexing: `Values[index]`
- Immutable integer data length: `length(Values)`
- Calls to declared functions
- Calls to declared capabilities
- Parentheses
- Unary `-` for `i32` and `!` for `bool`
- `*`, `+`, and `-` on `i32`
- `<`, `<=`, `>`, and `>=` on `i32`
- `==` and `!=` on two `i32` values or two `bool` values

Operators use conventional precedence. Binary operands are evaluated from left to right. Seed does not include implicit conversions.

## Runtime behavior

- Integer overflow traps.
- Immutable data indexing traps when the index is negative or outside the declared data length.
- Calling consumes arguments from left to right and creates a new frame.
- Bytecode local slots have deterministic defaults (`0`, `false`, or empty text); Windvale source still requires every `let` declaration to have an initializer.
- The runtime enforces implementation limits for instructions and call depth.
- Module capability imports must be authorized explicitly by the embedding host.
- Pure portable execution cannot observe the host operating system.

## Diagnostics

Compile diagnostics contain a stable code, one-based line and column, and a concise message. Compilation produces no bytecode when any error exists.
