# Decision 0754: Resolve the first Language 1.0 paper findings

## Status

Accepted by the project owner on 2026-08-17. This decision refines
[Decision 0751](0751-Accept-Windvale-Language-1.0-Direction.md),
[Decision 0752](0752-Complete-Language-1.0-Collection-And-Package-Data-Boundaries.md),
and
[Decision 0753](0753-Require-Language-1.0-AI-Accelerator-Evidence.md).
It resolves five general source-contract questions exposed by the first complete
paper workload. It does not freeze source edition 1, change Windvale Seed, assign
final Foundation signature-set digests, freeze the complete accelerator API, or
claim implementation or accelerator support on any target.

## Context

The local AI accelerator paper bundle demonstrated that the candidate language
can express package-backed model data, strict reference computation, bounded
tasks, explicit accelerator authority, move-owned provider resources, borrowed
views, cancellation, loss, and a target-scoped custom kernel without adding AI
syntax or a second compiler. Its first-author review also exposed five places
where the general Language 1.0 suite was directionally clear but not exact enough
to validate complete source:

1. generic functions were declared, but the call rule did not say whether or how
   type and constant parameters were inferred;
2. closure capture wording did not distinguish a module-bound singleton
   capability root from an instance-bearing capability stored in a local;
3. the Foundation candidate described byte, numeric, and task families without
   fixing every call used by the workload;
4. the language did not assign application entry selection, root memory, and
   capability binding to one explicit launcher boundary; and
5. platform tokens could carry the two kernel target names, but their
   relationship to environment, architecture, ABI, and extensions was not
   defined.

Leaving these questions open while writing ten more workloads would encourage
different paper authors to invent incompatible call, capture, entry, or target
rules. None requires a new source production.

## Decision

### Exact argument-derived generic calls

Language 1.0 infers generic function type and compile-time constant parameters
only by unique structural matching against the exact types of explicit call
arguments. Repeated occurrences must yield the same canonical value, and every
parameter must be solved before protocol, effect, ownership, and admission
checks.

Result context, assignment targets, return statements, function bodies,
protocol searches, conversions, defaults, unsuffixed context-dependent literals,
algebraic solving, and import order cannot contribute a solution. An unsolved or
conflicting parameter is a diagnostic. Edition 1 adds no explicit
generic-argument call suffix; a function not callable under this rule must take
an explicit typed argument or use a non-generic named constructor.

### Module-bound capability roots

A required capability declaration introduces one module-bound singleton root
after catalog resolution and launcher binding. That root is a resolved module
dependency, not a lexical local or ambient lookup. A qualified call through it
does not require a closure capture entry, but its capability identity remains in
the function or closure effect set, the module requirement set, and the
application's transitive approval closure.

Any capability reference, rights-reduced provider, session, or other instance
stored in a local remains an ordinary lexical value and must be captured
explicitly by copy, move, immutable borrow, or mutable borrow. An optional-only
declaration supplies no callable root.

### First exact Foundation calls

Accept the workload's exact version-1 `Foundationˉbytes` calls:

```text
Length(Value: borrow bytes) -> u64 effects()
At(Value: borrow bytes, Index: u64) -> u8 effects()
```

`Length` returns current length. `At` requires and checks
`Index < Length(Value)`, traps terminally before reading on violation, and never
performs an unchecked Core or Hosted access.

Accept the exact numeric calls
`Widenˉu8ˉtoˉu16`, `Widenˉu8ˉtoˉu32`, `Widenˉu8ˉtoˉu64`,
`Widenˉu16ˉtoˉu32`, `Widenˉu32ˉtoˉu64`, and `Bitsˉu32ˉtoˉf32`
with one named `Value` parameter, the exact source/destination types, and empty
effect sets. Widening preserves mathematical value; bit reinterpretation
preserves all 32 bits without arithmetic.

Accept `Task.Construct` consuming one `Memoryˉbudget` and one `Taskˉlimits`,
returning `Result<Taskˉscope, Allocationˉfailure>`, with
`memory.allocate` and `resource.acquire`. Accept the semantic `Task.Spawn` family
for one exact async closure type `W`, preserving `W` in
`Spawnˉfailure<W>` and deriving `T`, `E`, and the child's exact effect set from
the explicit closure type rather than the call result. Accept `Task.Await`
consuming `Task<T,E>` exactly once, returning `Taskˉoutcome<T,E>`, with
`task.suspend`.

`Task.Spawn` is one compiler-recognized Foundation semantic family identified by
its canonical module, declaration, major version, and eventual signature-set
identity. Recognition structurally decomposes `W`; it is not overload search,
result inference, new call syntax, or ambient task creation.

These calls become accepted normative-candidate signatures. Their eventual
module signature-set identities still wait for the other ten workloads and the
complete Foundation freeze matrix.

### Application entry and launcher binding

Language 1.0 has no special `Main`, `Run`, implicit entry parameter, ambient
allocator, or universal entry ABI. A build or package plan selects one exported,
monomorphic function by canonical identity and exact signature for a named
launcher profile.

The launcher atomically admits the package, entry, target, and launcher profile;
creates the bounded application resource domain and owned root values; approves
the complete transitive capability set; binds each module root to a
rights-limited provider; binds every ordinary argument by exact type and
position; and only then invokes the entry. Any missing, duplicate, unauthorized,
stale, oversized, incompatible, or unsupported binding rejects before source
execution. The launcher owns translation of the exact result or terminal task
outcome and reclaims the application domain after structured teardown.

### Canonical target-scope registry

A source `platform` item is an opaque canonical registry key whose entry is one
predicate over a structured concrete target descriptor. The descriptor keeps
environment, architecture, ABI, extension identities, and target-interface
identities separate. A comma-separated source list contains alternatives, not
dimensions to combine. Period-separated key segments imply no inheritance or
prefix compatibility.

Retain the environment keys `windows`, `linux`, and `windvale`. Accept
`accelerator.software.v1` and `accelerator.spirv.v1` as the first accelerator
target-interface keys needed by the paper kernel. Their `v1` suffix versions the
Windvale target interface, not an upstream SPIR-V specification. Neither key
implies a device, vendor, capability, provider, attachment mode, host
environment, architecture, ABI, implementation, or performance claim.

Changing the predicate of an existing key is incompatible. Adding a separately
specified registry entry requires no grammar change.

## Consequences

The first paper workload is owner-reviewed and its five general blockers are
resolved. It remains a draft-reviewed corpus row rather than frozen or
implemented source because the other ten workloads, complete Foundation
signature identities, diagnostics, editor behavior, and final source-freeze
identities are still pending.

Generic resolution is bounded and reproducible: the compiler performs structural
matching instead of overload or protocol search. Generic factories that have no
typed argument evidence require a named non-generic API, keeping edition 1 small
at the cost of excluding return-context inference.

Capability calls remain convenient without becoming ambient. The launcher can
audit the exact transitive grants before execution, while local provider
instances retain visible ownership and capture.

The exact Foundation calls are now stable inputs to the remaining paper bundles.
Later workloads may add operations but cannot silently rename or weaken these
calls; a contradiction requires a named reconsideration and coherent source
update.

Application packages can choose entries appropriate to consoles, services,
tools, GUI applications, or Windvale OS without putting those ABIs into source
semantics. Every profile must still specify exact admitted signatures and result
translation.

Target support becomes structured build evidence rather than meaning inferred
from dotted strings. Accelerator backends, representations, kernel restrictions,
and physical providers remain separate later contracts over the same language.

## Reconsideration triggers

Reconsider argument-only generic resolution if two or more complete paper
workloads require a safe generic construction that cannot carry an explicit
typed argument or clear named constructor. Any replacement must remain unique,
bounded, reproducible, and independent of overload or result-context guessing.

Reconsider singleton capability roots if a real capability cannot provide safe
shared calls without instance state. Split that interface or require explicit
instance acquisition rather than making lexical capture ambient.

Reconsider one accepted Foundation signature only when another mandatory
workload proves an ownership, failure, effect, or bounds contradiction. Update
the complete paper corpus and decision trail together.

Reconsider entry binding only when a required launcher profile cannot express
its root inputs and completion through exact typed metadata. Do not respond by
adding ambient process state.

Reconsider a target key when its predicate cannot be implemented consistently
by a software oracle and at least one plausible target adapter. Replace or
version the key rather than changing its meaning in place.
