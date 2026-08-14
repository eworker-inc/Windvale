# Post-.NET-retirement language and library stage

> Status: Active next-stage product proposal after Decision 0526's completed
> .NET retirement. This document does not itself add source syntax, WVB behavior,
> packages, capability interfaces, or a new roadmap milestone. A focused
> decision and measured consumer evidence remain required before any individual
> contract is accepted or implemented.

## Purpose

The [Decision 0057](../Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
gate is complete under [Decision 0526](../Decisions/0526-Dotnet-Retirement-Qualification-And-Stage0-Archive.md),
so Windvale's native toolchain is the normal Windows and Linux development path.
That result does not by itself make the language convenient for a useful
application ecosystem. The immediate product opportunity is to use the stable
path to prove a small, coherent set of application and library contracts.

Package, library, and application work may now proceed through the qualified
native path when a direct consumer exists; it must not widen the frozen C#
compiler. The active roadmap makes this useful package-backed application
Milestone 2 while a separate Windvale OS launch/service milestone continues the
kernel path. The two outcomes may share contracts without turning either into a
prerequisite for every step of the other.

The intended first outcome is one useful application that:

1. builds from a manifest and deterministic lockfile without .NET on Windows and
   Linux;
2. imports one reusable portable library and one rights-limited platform library;
3. declares, approves, binds, and reports its complete capability closure;
4. runs from the same canonical WVB identity on Windows and Linux through one named
   execution profile, with any native derivatives preserving that identity and its
   admitted metadata; and
5. can be rebuilt, inspected, and verified from immutable source and package
   identities.

This is the application-side complement of the proposed
[Windvale 0.1 package gate](../Architecture/Packages-Releases-And-Recovery.md#recommended-windvale-01-gate);
it is not a claim that a public registry, dynamic runtime linker, desktop, network
stack, or Windvale OS distribution is ready.

The first implementation slice is now selected under [Decision 0530](../Decisions/0530-First-Locked-Source-Package-And-Wvdb-Application.md).
WVDB Query builds deterministically from Package 1 and Lock 1, composes the portable
decimal and database code with the hosted read-only directory facade, and exposes
its exact capability closure for inspection. The package build and inspection path
is implemented on both native hosts. Native directory-provider binding, execution,
and denial evidence remain open, so the complete outcome above is not yet claimed.

## Keep the library model simple

Windvale should not copy the .NET global namespace model. A source module is an
explicit dependency, and the importing file chooses its local vocabulary through
one alias. This keeps source references short without making a large ambient
`System` surface appear in every program.

```windvale
// Illustrative future module names; these modules do not exist yet.
import Dataˉformat as Format;
import Readˉonlyˉdirectory as Directory;

let Document = Format.Decode(Input);
let Result = Directory.Readˉbytes(Name, 0u32, 4096u32);
```

The import operand is a source module identity declared by a selected package part;
the package manifest and lockfile identify the exact supplying part. `Format` and
`Directory` are only local aliases. `Format.Decode` is therefore not a hidden
namespace lookup, and a second import may use a different clear local alias. This
proposal does not yet freeze whether module identities are globally unique or
package-part-relative, but the package decision must make every import mapping
unambiguous. Source does not infer a module name from its directory, search an
ambient import path, provide a global prelude, or support wildcard imports.
Package, capability, protocol, and ABI identities retain their own separately
specified ASCII-safe formats.

The repository should retain four cross-cutting library roles:

| Role | Responsibility | Capability rule |
| --- | --- | --- |
| `Foundation/` | Pure values, codecs, bounded algorithms, parsing, and data structures. | Capability-free and portable. |
| `Platform/` | Application-facing adapters over filesystem, storage, console, process, time, entropy, network, or UI capabilities. | Every interface, bound instance, limit, and failure is explicit. |
| `Protocol/` | Portable records, codecs, and validation for a reusable provider or service wire contract. | No raw transport, host handle, or ambient authority. |
| `System/` | Reusable privileged kernel, driver, or machine contracts. | Created only when an implemented privileged owner and explicit unsafe/kernel boundary exist. |

These roles are not an exhaustive top-level folder hierarchy. A focused,
capability-free domain library such as the existing `Libraries/Database/` area may
own reusable policy without becoming another authority tier. Folder names describe
durable ownership, while a focused module name describes one contract. Add a
directory only with its first owned contract and implementation. A facade module is
justified when it composes a small coherent family for at least two real consumers;
it must not become a broad `System.IO`-style bucket that merges unrelated path,
storage, durability, watch, and permission promises.

The existing [Libraries guide](../../Libraries/README.md) remains the operational
description of current layers and capability rules. This proposal only recommends
how that model should grow toward the post-retirement product lane.

## Recommended delivery rule

A source feature is one semantic change, not several unrelated compilers. Its first
implementation must update the Windvale-owned source compiler, source specification,
semantic tests, deterministic fixtures, diagnostics, and editor grammar. If it
changes WIR or WVB, it must also update the affected format specification,
independent verifier, interpreter/runtime behavior, and malformed-input coverage.
A source-only lowering into existing verified operations should not churn an
unaffected serialized format or backend. Historical C# source is available only
through the immutable Stage 0 release and does not receive new language breadth
merely to preserve parity.

Native, WebAssembly, and Windvale OS lowering are target profiles, not separate
source languages. A new feature must be implemented and tested in every target
profile that claims it. It may first be available only through a named interpreted
or target profile, but the compiler, package metadata, and documentation must reject
or report unsupported combinations explicitly. The support matrix should
distinguish source acceptance, WVB verification, interpreted execution, native
lowering, WebAssembly, and Windvale OS. No product claim may treat support in one
column as automatic support in another.

## Recommended sequence

### 1. Establish a usable package and library baseline before new syntax

Specify one canonical package manifest and lockfile around the first useful
application and reusable library. Keep them distinct from the existing Project 1
build-input manifest. Add deterministic local package resolution and immutable
resources before a registry, runtime linking, or automatic update client; once all
locked objects are present, the build and inspection path must work offline. Use the
application to exercise import aliases, capability approval, provider binding,
denial diagnostics, and Windows/Linux execution from one canonical WVB identity.

This step should identify missing library operations before expanding the language.
For example, a filesystem library must expose one exact rights-limited operation
and typed provider outcomes; it must not grow from the bootstrap `file.read_bytes`
leaf into ambient host paths.

### 2. Add typed capability references and scoped ownership where it is real

The first language priority after the package baseline is one typed, rights-limited
capability reference. Its decision must specify binding, copying or movement,
instance and provider generation, limits, revocation, provider restart, peer loss,
and failure behavior. A declaration says what a module requires; a typed value says
which approved provider instance is bound for use. Those are distinct facts.

Scoped ownership is a related but separate contract. A future `using`-style form
should apply only to an affine value whose interface gives the caller an ordinary
close operation and defines early-return cleanup ordering. A prebound or shared
provider reference is not implicitly owned merely because it is typed, and lexical
scope cannot promise cleanup after process corruption or provider failure.

This unlocks simple library APIs for files, streams, storage, terminal surfaces,
and later network connections without exposing raw IPC envelopes or host handles.
It must not add implicit capability acquisition or promise cleanup after a process
or provider has already failed.

### 3. Improve typed-result ergonomics without exceptions

Libraries should continue to model expected outcomes as nominal variants. Once one
explicit result declaration contract, ownership transfer, cleanup ordering, and
function-return rule have evidence, add a narrow visible `try` propagation form.
The first form should require an exact failure payload type or an explicit adapter;
case names alone must not opt an arbitrary variant into propagation. It should
expand to ordinary result inspection, construction, and early return; it must not
catch traps, invoke hidden provider calls, perform inferred conversions, or become
a general exception mechanism.

This is a high-value readability improvement for capability-heavy application code
because it reduces repeated exhaustive forwarding while retaining explicit failure
types.

### 4. Add one bounded associative collection

After two consumers need keyed lookup, define one bounded immutable map plus an
affine builder path. Add a set only when a consumer needs it and it can reuse the
same admitted key and capacity semantics without creating a second collection
model. The contract must choose admitted key types, equality, hashing, collision and
worst-case work bounds, capacity typing, duplicate-key policy, deterministic
iteration and serialization order, allocation accounting, and exact full-capacity
outcome. Logical equality and iteration must not depend on host hash-table layout.
Do not introduce a host dictionary wrapper, hidden resizing, or an unbounded
collection merely for convenience.

This is the most useful remaining general data-structure step for configuration,
indexes, compiler tables, package metadata, and application state.

### 5. Generalize reusable value APIs only after their limits are proven

The current `sequence<T, N>` proves that a narrowly bounded parameterized value can
be safe. General user-defined generics should wait until map, result, resource, and
package consumers establish exact instantiation, code-size, ABI, ownership, and
diagnostic rules. Begin with a deliberately bounded, statically resolved form; do
not add runtime type discovery, inferred overload selection, or an unbounded
monomorphization surface.

Richer record nesting, multi-field variant payloads, derived equality, slices, or
bulk builder operations should likewise be selected one at a time by a measured
consumer rather than bundled into a second object system.

### 6. Add numeric and concurrency features when their hosts can honor them

Floating point is valuable for graphics, media, scientific, and ML workloads, but
it needs exact width, conversion, rounding, NaN, comparison, formatting,
reproducibility, and target-support rules. It should be an explicitly declared,
well-specified numeric feature set, not an implicit relaxation of the deterministic
integer model.

Structured concurrency follows only after the native runtime and OS scheduling
evidence can support bounded tasks, channels, cancellation, ownership transfer, and
typed failure propagation. `async`/`await` syntax, if later justified, should sit on
that model rather than invent an independent promise runtime.

## What this stage deliberately does not add

This proposal retains the language design's rejection of classes/inheritance,
implicit `null`, implicit conversions, general exceptions, operator overloading,
unrestricted macros or preprocessors, ambient reflection, whitespace-sensitive
blocks, hidden capability acquisition, and unbounded collections. It also does not
make a system profile an implicit unsafe permission, make paths a portable resource
identity, or turn packages into a runtime authority mechanism.

The stage should prefer a compact standard library built from real application and
tool pressure over a broad compatibility facade. If an API cannot state its bounds,
authority, ownership, failure, and cross-host behavior clearly, it is not ready for
the shared library surface.

## Evidence required for each slice

Before a proposed addition becomes accepted, it needs:

1. one focused decision with the source, WIR/WVB, capability, ABI, and target scope
   made explicit where each is affected;
2. one production-shaped end-to-end consumer for a package or target slice, and two
   concrete consumers before a reusable source or library abstraction is
   generalized, unless it is a narrow security or recovery correction;
3. valid, boundary, truncated, oversized, inconsistent, and malicious-input tests
   for every new parser or serialized contract;
4. deterministic artifact evidence and a target-support matrix that separates
   compilation, verification, and each execution backend; and
5. Windows and Linux reports whenever the slice claims cross-host behavior.

The next review should select one package-backed application and its smallest
reusable portable and platform libraries. Prefer an existing multi-module workload
with bounded input, observable useful output, typed operational failure, and a
capability-denial case over a new demonstration invented for the proposal. That
real workload, rather than a language-feature wish list, decides whether typed
capability references, result propagation, or bounded keyed collections are the
next implementation slice.

The existing `Readˉonlyˉwvdb` snapshot path is now the selected first application
composition. It has a standalone entry point, canonical Package 1 / Lock 1 input,
and paired native build and inspection owners. It still needs native
directory-provider binding, cross-host execution, and capability-denial evidence.
Selection does not promote the experimental `WVDB 1` bytes into a durable database
format.
