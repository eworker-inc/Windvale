# Windvale Seed enums and bounded formatting

## Purpose

Nominal enums replace magic status integers while bounded formatting lets portable tools describe structured values deterministically. The contract is deliberately smaller than interpolation, a general formatting framework, reflection, or locale-aware presentation.

## Enum source contract

```text
enum Wvbˉstatus {
    Valid = 0;
    Outˉofˉbounds = 5;
}

let Status: Wvbˉstatus = Wvbˉstatus.Valid;
```

- An enum has 1 through 256 members.
- Member names and `i32` values are unique within the enum.
- Seed source requires explicit nonnegative `i32` values.
- An enum member has its declared nominal enum type; there is no integer conversion.
- `==` and `!=` require two values of the same enum.
- Enums may be parameters, results, locals, and record fields.
- `Enumˉname(Value)` returns the exact source member name.

## Formatting source contract

```text
I32ˉformat(Value: i32) -> text
U8ˉformat(Value: u8) -> text
U32ˉformat(Value: u32) -> text
Textˉconcat(Left: text, Right: text) -> text
Enumˉname(Value: <enum>) -> text
```

Integer formatting is invariant base 10. It never uses host locale, grouping separators, a leading plus sign, or padding. `i32` preserves a leading minus sign for negative values. Enum naming returns declared identifier text rather than its backing number.

`Textˉconcat` measures the combined strict UTF-8 byte length before concatenating. It traps with `WVR3012` if the result would exceed the 1 MiB text-value limit. This provides a deterministic allocation bound; it is not truncation.

## Bytecode and runtime

The tagged Types-section representation introduced in WVB 1.3 remains part of WVB 1.4. Enum value shapes carry exact nominal type indices. `enum.const` identifies both type and member, enum comparison requires matching shapes, and `enum.name` resolves only a value that the verifier has already proved belongs to that enum.

The reference runtime stores an enum's declared `i32` value plus its nominal shape. Numeric formatting uses invariant .NET conversion only as the Stage 0 implementation of the specified text; the output contract is Windvale-defined.

## Deliberate limits

Seed has no flags enums, implicit numbering, integer conversions, enum ordering, text interpolation, format strings, padding, bases other than decimal, floating-point formatting, or locale-aware formatting. Add them only when a concrete Windvale-written tool needs them.
