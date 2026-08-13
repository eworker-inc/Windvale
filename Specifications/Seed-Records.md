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
- A field path is read left to right by name from a local or parameter.
- Records may be function parameters, function results, and locals.
- Record values and their fields are immutable. A mutable `var` may receive a replacement record value, but fields cannot be assigned.
- Record equality, methods, optional fields, default arguments, generics, and destructuring are not part of Seed.

## Bounds

- A module may declare at most 1,024 nominal record and enum types in total.
- A record contains 1 through 64 fields.
- Field names are unique within the record.
- A field may use an admitted primitive, a nominal Seed enum, or an immutable
  record identity.
- Record containment must be acyclic. A backend may impose a smaller explicit
  flattened-width bound; the native x64 subset admits at most 64 backing cells
  for one recursively flattened record value.

## Runtime and bytecode

The compiler assigns record type indices in ordinal record-name order. Bytecode carries nominal record shapes in function signatures and locals, declares field schemas in the Types section, constructs values with `record.create`, and reads fields with `record.field`.

The verifier checks record indices, constructor arity and operand types, field
indices, nominal identity, and bounded acyclic containment before execution.
The reference runtime stores a record as its type index plus immutable field
values. No instruction exposes or mutates that storage.

## Deliberate next steps

Small nominal enums, bounded formatting, and nested immutable record values are
now companion contracts. Mutable aggregates, collections, optional fields,
destructuring, and a broader allocation model remain separate future work.
