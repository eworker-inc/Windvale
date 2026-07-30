# Windvale Seed immutable records

## Purpose

Seed records are the smallest aggregate contract needed to return structured inspection results without committing Windvale to a general heap, mutable object, inheritance, or garbage-collection model. They are useful compiler data now and remain portable across Windows, Linux, and the future Windvale OS.

## Source contract

```text
record Pair {
    Left: i32;
    Right: u32;
}

fn Make(Left: i32, Right: u32) -> Pair {
    return Pair(Left, Right);
}

fn Readˉleft(Value: Pair) -> i32 {
    return Value.Left;
}
```

- A record name introduces a nominal value type and a positional constructor.
- Constructor arguments follow declared field order and require exact types.
- A field is read by name from a local or parameter.
- Records may be function parameters, function results, and locals.
- Record values and their fields are immutable. A mutable `var` may receive a replacement record value, but fields cannot be assigned.
- Record equality, methods, optional fields, default arguments, generics, and destructuring are not part of Seed.

## Bounds

- A module may declare at most 1,024 record types.
- A record contains 1 through 64 fields.
- Field names are unique within the record.
- A field may be `i32`, `u8`, `u32`, `bool`, `text`, or `bytes`.
- Nested record fields are deferred. This keeps default construction, runtime representation, verification, and lifetime behavior bounded while the bootstrap has no general memory model.

## Runtime and bytecode

The compiler assigns record type indices in ordinal record-name order. Bytecode carries nominal record shapes in function signatures and locals, declares field schemas in the Types section, constructs values with `record.create`, and reads fields with `record.field`.

The verifier checks record indices, constructor arity and operand types, field indices, and nominal identity before execution. The reference runtime stores a record as its type index plus an immutable field array. No instruction exposes or mutates that array.

## Deliberate next steps

The next structured-inspection work may add a small enum contract for status names and bounded formatting for report values. Nested aggregates or a broader allocation model should be introduced only when a concrete compiler, assembler, or Foundation data structure requires them.
