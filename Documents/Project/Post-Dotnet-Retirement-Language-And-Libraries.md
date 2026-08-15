# Post-.NET-retirement language and library stage

> Status: The first package/application foundation is complete under Milestone 2.
> The remaining language and library items are consumer-driven future proposals;
> this document does not itself add source syntax, WVB behavior, packages,
> capability interfaces, or a new roadmap milestone. A focused decision and
> measured consumer remain required before any individual contract is accepted.

## Purpose

The [Decision 0057](../Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
gate is complete under [Decision 0526](../Decisions/0526-Dotnet-Retirement-Qualification-And-Stage0-Archive.md),
so Windvale's native toolchain is the normal Windows and Linux development path.
That result does not by itself make the language convenient for a useful
application ecosystem. The immediate product opportunity is to use the stable
path to prove a small, coherent set of application and library contracts.

Package, library, and application work may now proceed through the qualified
native path when a direct consumer exists; it must not widen the frozen C#
compiler. The roadmap closed this useful package-backed application as
Milestone 2 while parallel track OS-1 continues the Windvale OS launch/service
path. The two outcomes may share contracts without turning either into a
prerequisite for every step of the other. Milestone 3 and the signed `v0.1.0`
preview are also complete; future language/library breadth should be selected by
Milestone 4 or another named consumer rather than attached retroactively to 0.1.

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

The first implementation slice is selected under
[Decision 0530](../Decisions/0530-First-Locked-Source-Package-And-Wvdb-Application.md)
and qualified as the completed Milestone 2 slice under
[Decision 0561](../Decisions/0561-First-Admitted-Bundle-Store-And-Rights-Reduced-Wvdb-Query.md).
WVDB Query builds deterministically from Package 1 and Lock 1, composes the
portable decimal and database code with the hosted read-only directory facade,
and exposes its exact capability closure for inspection. The bounded Bundle 1,
immutable store publication, paired native applications, directory-provider
binding, successful reads, and denied/unavailable cases pass on Windows and
Linux in Verify runs 31872089188 and 31872429140. The complete outcome above is
therefore closed. The installer and release-envelope outcome subsequently closed
under Milestone 3.

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

### Independent-metadata migration prerequisite

The source compiler, canonical WVB 1.11 writer, and reference runtime accept the
independent `platform`, `authority`, required-capability, and optional-capability
header. Decision 0571 added bounded admission at the paired native WVB-to-WVO
application boundary. Decision 0572 adds a portable normalization contract and
focused malformed-input proof, but a native inspector smoke test rejected its
first integration and the normal compiler-aligned verifier remains at its
current binding-evidence ceiling. Both production consumers still require
implementation.
Production migration is therefore not ready to become a repository-wide source
rewrite; the focused metadata fixture remains the replacement-form proof input.

Advance the migration in this order:

1. ✅ make the native lowerer independently validate and admit one metadata-bearing
   module while preserving its existing profile, capability, code, and object
   rules;
2. 🔵 add malformed metadata cases for invalid presence, version, authority,
   platform ordering, capability ordering/version/overlap, profile derivation,
   and required-capability mismatch; lowerer coverage is complete and the
   portable normalization contract now has its first deterministic/malformed
   consumer cases;
3. prove one current package application through source compilation, WVB
   verification and inspection, native lowering, packaging, and execution with
   the replacement header on Windows and Linux;
4. migrate that package's reachable libraries, then the remaining repository
   source in owner-sized coherent batches with exact artifact updates; and
5. remove the legacy source spelling only after every maintained target and
   recovery boundary either accepts the replacement or names an explicit frozen
   historical input.

Do not mass-edit source before step 1. Source acceptance alone is not target
support, and silently discarding metadata in a derived native product would
violate the package and portability contracts.

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

## Agent runtime as a named future consumer

The proposed [agent runtime architecture](../Architecture/Agent-Runtime-And-Digital-Subconscious.md)
and [staged implementation plan](Windvale-Agent-Runtime-Implementation-Plan.md) can
begin without widening the language. Existing nominal records, closed variants,
bounded sequences, explicit results, and deterministic functions are sufficient
for its first run-state, context-selection, checkpoint, and influence-inspection
corpora. Foreground and digital-subconscious operations may be scheduled
sequentially, and supplied identity/time values avoid premature entropy and clock
requirements.

That consumer may later provide measured pressure for:

- one bounded deterministic map after another product lane needs the same value;
- typed capability references with explicit affine ownership and no serialization;
- generated bounded codecs for nominal provider envelopes; and
- structured concurrency only after cancellation, budget, join, teardown, and
  provider-loss behavior have native runtime and OS evidence.

These are dependency candidates, not an agent-specific syntax roadmap. Arbitrary
JSON values, ambient provider sessions, unbounded message histories, hidden tasks,
and model-selected capability acquisition remain outside the language contract.

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

The selected package-backed application and its smallest reusable portable and
platform libraries are now qualified under Decision 0561. Future reviews should
continue to prefer an existing multi-module workload with bounded input,
observable useful output, typed operational failure, and a capability-denial
case over a new demonstration invented for a proposal. A real workload, rather
than a language-feature wish list, should decide whether typed capability
references, result propagation, or bounded keyed collections are widened.

The existing `Readˉonlyˉwvdb` snapshot path is the completed first application
composition. It has a standalone entry point, canonical Package 1 / Lock 1
input, paired native build and inspection owners, native directory-provider
binding, cross-host execution, and capability-denial evidence. Completion does
not promote the experimental `WVDB 1` bytes into a durable database format.
