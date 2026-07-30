# Windvale Seed language specification

## Status and scope

This document specifies the source-language subset implemented by Windvale Seed. It is deliberately small and may break during early development. Source is strict UTF-8. Identifiers are case-sensitive ASCII segments joined by U+02C9 and match `[A-Za-z_][A-Za-z0-9_]*(ˉ[A-Za-z_][A-Za-z0-9_]*)*`. Official source follows [Windvale source naming conventions](Source-Naming.md).

## Module shape

Every source file contains one module declaration followed by zero or more capability declarations, immutable data declarations, record declarations, enum declarations, and function declarations.

```text
module <Name> profile <portable|hosted|system>;
capability <qualified.name>;
data <Name>: <text|[i32]|bytes> = <literal>;
record <Name> { <Field>: <primitive-or-enum-type>; ... }
enum <Name> { <Member> = <nonnegative-i32>; ... }
[export] fn <Name>(<parameters>) -> <type> { <statements> }
```

Seed accepts `system` as a profile value so the serialized contract is explicit, but it exposes no system capabilities or unsafe operations.

## Types

- `void` is valid only as a function or capability return type.
- `i32` is a signed 32-bit integer. Arithmetic overflow traps deterministically.
- `u8` is an unsigned 8-bit integer used for individual byte values.
- `u32` is an unsigned 32-bit integer used for byte offsets, lengths, and binary fields. Arithmetic overflow and underflow trap deterministically.
- `bool` contains only `true` or `false`.
- `text` is immutable, valid Unicode stored canonically as UTF-8 in modules.
- `bytes` is an immutable sequence of bytes. A slice is an immutable view over an existing sequence.
- A declared record name is an immutable nominal product type with fixed, named fields.
- A declared enum name is an immutable nominal scalar type with explicitly valued, named members.
- `[i32]` is immutable module data. It is not a general runtime array type in Seed.

Parameters and local variables may have `i32`, `u8`, `u32`, `bool`, `text`, `bytes`, or a declared record type. Functions may return the same value types. Module data may be `text`, `[i32]`, or `bytes`.

## Declarations

- Names within each declaration category are unique.
- Function and data names occupy separate namespaces.
- Record names define both a nominal type and its positional constructor. A function cannot use the same name as a record constructor, and records or functions cannot take a reserved intrinsic call name.
- A record contains 1 through 64 uniquely named fields in declaration order. Seed record fields may be primitive values or enums; nested record fields are deferred.
- An enum contains 1 through 256 uniquely named members with unique explicit `i32` values. Seed source currently accepts nonnegative values; bytecode preserves signed `i32` values for forward evolution.
- A module exports functions explicitly with `export`.
- `windvale run` looks for an exported function named `Main` with signature `fn() -> i32`.
- Capability declarations must be unique and use qualified lowercase names.
- A portable module cannot declare or call hosted capabilities.

Seed defines one hosted capability:

```text
console.write_line(text) -> void
```

## Statements

```text
let <name>: <type> = <expression>;
var <name>: <type> = <expression>;
<name> = <expression>;
<expression>;
if <expression> { <statements> } [else { <statements> }]
while <expression> { <statements> }
return [<expression>];
```

`let` locals and parameters are immutable. `var` locals may be assigned after initialization. Blocks use lexical scope. Parameters and locals must have unique names within their function in Seed; nested shadowing is rejected to keep diagnostics and lowering simple. Statements after an unconditional return in the same block are rejected as unreachable.

## Expressions

Seed supports:

- Decimal `i32` literals
- Decimal `u8` literals with a `u8` suffix, from `0u8` through `255u8`
- Decimal `u32` literals with a `u32` suffix, from `0u32` through `4294967295u32`
- `true` and `false`
- String literals with `\"`, `\\`, `\n`, `\r`, `\t`, and `\uXXXX` escapes
- Local and parameter reads
- Immutable text data reads
- Immutable byte-data reads
- Immutable integer data indexing: `Values[index]`
- Immutable integer data length: `length(Values)`
- Calls to declared functions
- Calls to declared capabilities
- Positional record construction in field declaration order: `Pair(Left, Right)`
- Named field access from a local or parameter: `Value.Left`
- Nominal enum member values: `Wvbˉstatus.Valid`
- Parentheses
- Unary `-` for `i32` and `!` for `bool`
- `*`, `+`, and `-` on two `i32` values or two `u32` values
- `<`, `<=`, `>`, and `>=` on two `i32` values or two `u32` values
- `==` and `!=` on two values of the same `i32`, `u8`, `u32`, `bool`, or nominal enum type

Operators use conventional precedence. Binary operands are evaluated from left to right. Seed does not include implicit conversions.

Records are nominal rather than structural: separately declared record types are incompatible even when their fields have identical names and types. Construction creates an immutable value, field access returns the selected value, and Seed provides no field assignment or record equality. Record values can cross function boundaries and be stored in `let` or `var` locals; `var` permits replacing the whole value, not mutating a field.

Enums are also nominal. A member expression produces its declared enum type, separately declared enums are incompatible, and there is no implicit conversion to or from `i32`. `Enumˉname(Value)` returns the exact declared member name as immutable text.

`bytes` data uses an array literal whose elements are unsuffixed decimal values in the range 0 through 255 or `u8` literals. Seed exposes these reserved Foundation intrinsics without requiring declarations:

```text
Bytesˉlength(Value: bytes) -> u32
Bytesˉslice(Value: bytes, Offset: u32, Length: u32) -> bytes
Bytesˉreadˉu8(Value: bytes, Offset: u32) -> u8
Bytesˉreadˉu16ˉlittle(Value: bytes, Offset: u32) -> u32
Bytesˉreadˉu32ˉlittle(Value: bytes, Offset: u32) -> u32
I32ˉformat(Value: i32) -> text
U8ˉformat(Value: u8) -> text
U32ˉformat(Value: u32) -> text
Textˉconcat(Left: text, Right: text) -> text
Enumˉname(Value: <enum>) -> text
```

The little-endian reads consume exactly 1, 2, or 4 bytes beginning at `Offset`. Numeric formatting uses invariant base-10 text with no host locale, grouping, or leading padding. Concatenation preserves exact text and traps if the result would exceed the 1 MiB UTF-8 value limit. Foundation intrinsic names cannot be redefined by source functions. This initial contract deliberately has no ambient file access: bytes enter portable code as module data or parameters supplied through a future explicit capability or package boundary.

## Runtime behavior

- Integer overflow traps.
- Unsigned integer underflow traps as well as overflow.
- Immutable data indexing traps when the index is negative or outside the declared data length.
- Byte reads and slices trap unless their entire requested range is inside the source sequence. A zero-length slice at the end is valid.
- Text concatenation traps before allocation when the combined UTF-8 length exceeds the value limit.
- Calling consumes arguments from left to right and creates a new frame.
- Bytecode local slots have deterministic defaults (`0`, `false`, empty text, empty bytes, the first declared enum member, or a recursively defaulted immutable record); Windvale source still requires every `let` or `var` declaration to have an initializer.
- The runtime enforces implementation limits for instructions and call depth.
- Module capability imports must be authorized explicitly by the embedding host.
- Pure portable execution cannot observe the host operating system.

## Diagnostics

Compile diagnostics contain a stable code, one-based line and column, and a concise message. Compilation produces no bytecode when any error exists.
