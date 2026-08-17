# Workload 1 command and stream contract

## Status

This is the smallest launcher, Foundation, and standard-stream contract required
to type and review workload 1. It is a paper candidate, not a published
capability catalog, runtime ABI, or implementation claim. General Foundation
findings require project-owner review before they update the normative candidate.

## Ordinary arguments

Arguments are one launcher-constructed
`Foundationˉcollections.Sequence<text>`. They are shared immutable values, not a
capability, global, native pointer array, or source of executable-path identity.
The profile excludes the executable/package display name so argument index zero
is the first user-supplied token.

The source requires these exact candidate collection operations:

```text
Sequenceˉlength<T>(
    Value: borrow Sequence<T>,
) -> u64 effects()

Sequenceˉat<T>(
    Value: borrow Sequence<T>,
    Index: u64,
) -> borrow T effects()
```

Both generic parameters are solved structurally from `Value` under Decision
0754. `Sequenceˉat` checks `Index < Sequenceˉlength(Value)` and traps before
access on violation. The parser proves the bound before every call. Its borrowed
result is tied to the one borrowed sequence parameter and cannot escape.

## Strict numeric parsing

The command uses one exact type- and policy-specific call:

```text
Parseˉu64ˉdecimalˉwhole(
    Value: borrow text,
    Maximumˉinputˉbytes: u64,
) -> Result<u64, Numericˉparseˉfailure> effects()
```

It admits one or more ASCII decimal digits only. It accepts no sign, radix
prefix, separator, whitespace, locale digit, special value, or trailing input.
It checks the UTF-8 byte maximum first and reports the existing exact numeric
failure cases. Mathematical values above `u64` report `Aboveˉmaximum`; they do
not wrap. Workload source separately rejects a successfully parsed value above
65,536.

The destination type and complete policy are encoded in the name because result
context cannot select a generic numeric parser in Language 1.0.

## Text and byte operations

The source selects these total observations:

```text
Byteˉlength(Value: borrow text) -> u64 effects()
Runeˉcount(Value: borrow text) -> u64 effects()
```

`Byteˉlength` reports canonical UTF-8 bytes. `Runeˉcount` reports Unicode scalar
values, not bytes, UTF-16 code units, grapheme clusters, display cells, or
locale-defined characters.

The workload also selects reserved builder families:

```text
Text.Constructˉreserved(
    Budget: Memoryˉbudget,
    Maximumˉoutputˉbytes: u64,
) -> Result<Textˉbuilder, Allocationˉfailure>
    effects(memory.allocate)

Text.Appendˉtext(
    Builder: borrow mut Textˉbuilder,
    Value: borrow text,
) -> Result<unit, Limitˉfailure> effects()

Text.Appendˉu64ˉdecimal(
    Builder: borrow mut Textˉbuilder,
    Value: u64,
) -> Result<unit, Limitˉfailure> effects()

Text.Freeze(Builder: Textˉbuilder) -> text effects()

Bytes.Constructˉreserved(
    Budget: Memoryˉbudget,
    Maximumˉoutputˉbytes: u64,
) -> Result<Bytesˉbuilder, Allocationˉfailure>
    effects(memory.allocate)

Bytes.Appendˉbytes(
    Builder: borrow mut Bytesˉbuilder,
    Value: borrow bytes,
) -> Result<unit, Limitˉfailure> effects()

Bytes.Appendˉutf8(
    Builder: borrow mut Bytesˉbuilder,
    Value: borrow text,
) -> Result<unit, Limitˉfailure> effects()

Bytes.Freeze(Builder: Bytesˉbuilder) -> bytes effects()
```

`Constructˉreserved` consumes one rights-reduced budget and commits its complete
maximum before returning the builder. Rejection consumes and locally releases
the child budget. Later append cannot fail for physical growth, but still
returns `Limitˉfailure` if the requested complete result exceeds the declared
builder maximum. Every append is all-or-nothing and leaves the builder unchanged
on failure. `Appendˉu64ˉdecimal` uses invariant shortest unsigned decimal.
`Appendˉutf8` appends canonical UTF-8 and cannot observe a host encoding.

`Freeze` consumes the mutable owner, publishes exactly its current content, and
transfers retained accounting to the immutable result without fallible
compaction. A non-reserved builder may remain a later Foundation family; this
workload does not require it.

## Supplied stream types

`Platformˉstream` supplies these paper-only values:

```text
export variant Inputˉfailure {
    Maximumˉexceeded(
        Observedˉminimum: u64,
        Maximum: u64,
    );
    Invalidˉutf8(Offset: u64);
    Rejected(Reason: u32);
    Providerˉlost(Generation: u64);
}

export record Writeˉfailure {
    Reason: u32;
    Providerˉgeneration: u64;
}
```

Reasons are stable interface values, not host error numbers. A generation is
zero only when the admitted provider profile cannot expose a stable generation.

## Standard input

The required module-bound root has this exact signature:

```text
standard.input.Readˉtext(
    Budget: Memoryˉbudget,
    Maximumˉutf8ˉbytes: u64,
) -> Result<text, Inputˉfailure>
    effects(memory.allocate, standard.input)
```

The operation consumes the input child budget and attempts to read through EOF.
It succeeds for empty input and for exact-maximum input. If at least one byte
beyond the maximum exists, it returns `Maximumˉexceeded` without exposing a
prefix. It validates shortest-form strict UTF-8 before constructing `text` and
reports the first invalid byte offset. On success, retained input storage owns
the consumed accounting. On failure, the provider exposes no text and locally
releases the consumed budget.

The provider may buffer, map, or stream internally, but cannot allocate or retain
more than the supplied budget. It has no filesystem, terminal, locale, network,
replay, seek, or cancellation semantics.

## Standard output and diagnostics

The two roots have the same value shape and different authorities:

```text
standard.output.Write(
    Value: borrow bytes,
) -> Mutationˉoutcome<Writeˉfailure>
    effects(standard.output)

standard.diagnostic.Write(
    Value: borrow bytes,
) -> Mutationˉoutcome<Writeˉfailure>
    effects(standard.diagnostic)
```

Their completion meaning is exact local-provider acceptance:

- `Rejected` proves zero bytes accepted;
- `Acceptedˉpartial` reports the exact accepted prefix length;
- `Completed` reports the exact accepted length; and
- `Indeterminate` cannot prove how many bytes became visible.

The application checks that `Completed.Completed` equals the input byte length.
An inconsistent count maps to output failure. Partial and indeterminate writes
are never retried. Acceptance does not imply terminal presentation, remote
receipt, persistence, or application-level commit.

Normal and diagnostic roots are independently approved and bound. One does not
grant or substitute for the other.

## Deliberately excluded

This contract does not define shell quoting, environment variables, current
directory, path search, terminal width, color, interactive editing, streaming
input, output flush, durable finish, logging, redirection, pipelines, process
creation, cancellation, or asynchronous I/O. Those require separately bounded
consumers and capability interfaces.
