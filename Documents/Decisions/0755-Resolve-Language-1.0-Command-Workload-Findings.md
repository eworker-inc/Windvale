# Decision 0755: Resolve the Language 1.0 command workload findings

## Status

Accepted by the project owner on 2026-08-17. This decision refines
[Decision 0751](0751-Accept-Windvale-Language-1.0-Direction.md),
[Decision 0752](0752-Complete-Language-1.0-Collection-And-Package-Data-Boundaries.md),
and
[Decision 0754](0754-Resolve-First-Language-1.0-Paper-Findings.md).
It accepts the five recommendations from the command-line application paper
bundle. It does not freeze source edition 1, change Windvale Seed, publish the
standard-stream capability catalog, assign final Foundation signature-set
identities, or claim implementation on any target.

## Context

The command-line application bundle expresses a complete bounded UTF-8
byte/rune inspector using ordinary launcher arguments, explicit memory budgets,
strict numeric parsing, bounded builders, independently granted input/output
capabilities, and exact mutation outcomes. The first-author review found no need
for a new source production or command-specific compiler path. It did expose
five contracts that needed project-owner resolution before later workloads
could reuse them consistently:

1. immutable sequences needed exact length and checked borrowed-access calls;
2. numeric parsing needed one exact destination- and policy-specific call;
3. text observations and reserved builders needed exact signatures and failure
   behavior;
4. standard input, normal output, and diagnostic output needed an accepted
   authority split without prematurely freezing their streaming APIs; and
5. the command launcher needed exact status precedence when reporting a primary
   failure can itself fail or become uncertain.

The first three findings are general Foundation contracts. The fourth is a
hosted capability-catalog candidate that later I/O workloads must pressure. The
fifth belongs to one named launcher profile rather than universal language
semantics.

## Decision

### Exact immutable-sequence observations

Accept these version-1 `Foundationˉcollections` signatures:

```text
export fn Sequenceˉlength<T>(
    Value: borrow Sequence<T>,
) -> u64 effects();

export fn Sequenceˉat<T>(
    Value: borrow Sequence<T>,
    Index: u64,
) -> borrow T effects();
```

The generic parameter `T` is solved uniquely from the explicit `Value` argument
under Decision 0754. `Sequenceˉlength` returns current element count, not an
admitted maximum or backing capacity. `Sequenceˉat` checks
`Index < Sequenceˉlength(Value)` with `u64` arithmetic and traps terminally
before access on violation. Its result is an immutable borrow tied to the one
borrowed sequence owner and cannot escape that owner. This acceptance does not
imply an unchecked Core or Hosted access.

### First exact strict numeric parser

Accept this version-1 `Foundationˉnumeric` signature:

```text
export fn Parseˉu64ˉdecimalˉwhole(
    Value: borrow text,
    Maximumˉinputˉbytes: u64,
) -> Result<u64, Numericˉparseˉfailure> effects();
```

The call admits one or more ASCII decimal digits only. It accepts no sign,
radix prefix, separator, whitespace, locale digit, special value, or trailing
input. It checks canonical UTF-8 byte length against
`Maximumˉinputˉbytes` before digit work and reports `Limitˉexceeded` when that
limit is exceeded. Empty input reports `Empty`; the first non-decimal byte
reports `Invalidˉdigit` at its exact byte offset; and a mathematical value above
`u64` reports `Aboveˉmaximum`. It consumes the complete input or fails and never
wraps, truncates, or uses host locale.

The destination, radix, sign, separator, whitespace, and whole-input policies
are encoded in the declaration name because Language 1.0 does not use result
context to select a generic parser.

### Text observations and reserved builders

Accept these version-1 `Foundationˉtext` observations:

```text
export fn Byteˉlength(Value: borrow text) -> u64 effects();
export fn Runeˉcount(Value: borrow text) -> u64 effects();
```

`Byteˉlength` reports canonical UTF-8 bytes. `Runeˉcount` reports Unicode scalar
values, not UTF-16 code units, grapheme clusters, display cells, or
locale-defined characters.

Accept these reserved builder signatures in their respective
`Foundationˉtext` and `Foundationˉbytes` modules:

```text
export fn Constructˉreserved(
    Budget: Memoryˉbudget,
    Maximumˉoutputˉbytes: u64,
) -> Result<Textˉbuilder, Allocationˉfailure>
    effects(memory.allocate);

export fn Appendˉtext(
    Builder: borrow mut Textˉbuilder,
    Value: borrow text,
) -> Result<unit, Limitˉfailure> effects();

export fn Appendˉu64ˉdecimal(
    Builder: borrow mut Textˉbuilder,
    Value: u64,
) -> Result<unit, Limitˉfailure> effects();

export fn Freeze(Builder: Textˉbuilder) -> text effects();

export fn Constructˉreserved(
    Budget: Memoryˉbudget,
    Maximumˉoutputˉbytes: u64,
) -> Result<Bytesˉbuilder, Allocationˉfailure>
    effects(memory.allocate);

export fn Appendˉbytes(
    Builder: borrow mut Bytesˉbuilder,
    Value: borrow bytes,
) -> Result<unit, Limitˉfailure> effects();

export fn Appendˉutf8(
    Builder: borrow mut Bytesˉbuilder,
    Value: borrow text,
) -> Result<unit, Limitˉfailure> effects();

export fn Freeze(Builder: Bytesˉbuilder) -> bytes effects();
```

`Constructˉreserved` consumes one rights-reduced budget and commits its complete
maximum before returning. Constructor failure consumes and locally releases the
child budget. A successful reserved builder cannot later fail because physical
growth was unavailable, although an append still returns `Limitˉfailure` before
mutation when the complete result would exceed the declared maximum.

Every accepted append is all-or-nothing and leaves content and length unchanged
on failure. `Appendˉu64ˉdecimal` emits invariant shortest unsigned decimal.
`Appendˉutf8` emits canonical UTF-8 without consulting a host encoding. `Freeze`
consumes the builder, publishes exactly its current content, and transfers its
retained accounting to the immutable result without fallible compaction.

### Three independent standard-stream candidates

Retain `standard.input` version 1, `standard.output` version 1, and
`standard.diagnostic` version 1 as independent required capability candidates
for this paper command profile. A grant of one does not grant or substitute for
another. Their command-bundle shapes remain coherent paper inputs for workloads
2, 5, and 6.

Do not yet publish final capability-catalog or signature-set identities for
these roots. Bounded file I/O, network streaming, deadlines, cancellation,
owned provider instances, durable finish, and provider loss may require a
different operation boundary. Later revision must preserve the independent
authority split even if it revises the paper-only signatures.

### Command launcher status precedence

Accept the exact `windvale.launch.command.v1` entry and six-status result used by
the paper bundle:

```text
Run(
    Arguments: Foundationˉcollections.Sequence<text>,
    Budget: Foundationˉmemory.Memoryˉbudget,
) -> Inspectˉtypes.Processˉstatus
```

| Result member | Process status | Meaning |
| --- | ---: | --- |
| `Success` | 0 | The requested normal output completed. |
| `Argumentsˉfailed` | 2 | Argument handling failed and its diagnostic completed. |
| `Inputˉfailed` | 3 | Input handling failed and its diagnostic completed. |
| `Outputˉfailed` | 4 | A normal or diagnostic write was rejected, partial, or reported an inconsistent completed count. |
| `Outputˉindeterminate` | 5 | Normal or diagnostic write progress cannot be proved. |
| `Resourceˉfailed` | 6 | A bounded local resource operation failed and any attempted diagnostic completed. |

A completely accepted diagnostic preserves the primary status 2, 3, or 6.
Rejected, partial, or internally inconsistent diagnostic acceptance overrides
that primary status with 4. Indeterminate diagnostic progress overrides it with
5. The application never retries a non-complete mutation and never falls back
to the other output channel. A completed write proves exact local-provider
acceptance only, not presentation, remote receipt, durability, or application
commit.

This precedence belongs to `windvale.launch.command.v1`. Services and richer
launchers may use structured completion records or a different explicitly named
policy without changing the language.

## Consequences

The command-line application becomes a draft-reviewed corpus row. Its exact
sequence, numeric, text, and builder calls become normative-candidate Foundation
contracts available to the remaining workloads. Later workloads may add calls
but cannot silently rename or weaken these contracts; a contradiction requires
a named reconsideration and coherent corpus update.

The standard-stream authority split and command source remain accepted paper
evidence, while the three stream operation identities remain provisional until
the later I/O workloads complete. This avoids freezing a one-shot API before
resource-bearing and cancellable consumers test it.

The decision adds no keyword, production, implicit conversion, overload rule,
ambient authority, command-specific WIR operation, or parallel compiler. Current
tools and libraries continue implementing Windvale Seed. Complete Foundation
module identities and Language 1.0 source freeze remain pending all eleven
workloads.

## Reconsideration triggers

Reconsider the sequence calls only if another mandatory workload proves that the
borrowed result cannot express a safe ordinary traversal or that recoverable
indexing is required as a distinct named family. Do not weaken the checked
access contract.

Reconsider the numeric parser only if a complete numeric or protocol workload
proves that its name, offsets, or limit ordering conflicts with the generated
conversion/parsing matrix. Keep policy selection explicit and bounded.

Reconsider reserved builders only if a target cannot honor committed capacity
without violating the stated allocation, accounting, or all-or-nothing
behavior. Revise the ownership and failure contract instead of hiding later
allocation.

Reconsider a stream signature when workloads 2, 5, or 6 require owned instances,
cancellation, progress, finish, or provider-loss evidence that the current
one-shot shape cannot carry. Preserve separate input, normal-output, and
diagnostic authorities.

Reconsider command status precedence only if a real command cannot preserve its
primary result and mutation certainty in six statuses. Prefer a separately named
structured-completion launcher profile over changing this profile in place.
