# Windvale Language 1.0 design

> Status: Direction accepted by the project owner on 2026-08-17 under
> [Decision 0751](../Decisions/0751-Accept-Windvale-Language-1.0-Direction.md)
> and refined by
> [Decision 0752](../Decisions/0752-Complete-Language-1.0-Collection-And-Package-Data-Boundaries.md)
> and
> [Decision 0753](../Decisions/0753-Require-Language-1.0-AI-Accelerator-Evidence.md),
> with the first paper findings resolved by
> [Decision 0754](../Decisions/0754-Resolve-First-Language-1.0-Paper-Findings.md),
> the command findings by
> [Decision 0755](../Decisions/0755-Resolve-Language-1.0-Command-Workload-Findings.md),
> and the bounded file-copy findings by
> [Decision 0756](../Decisions/0756-Resolve-Language-1.0-File-Copy-Findings.md),
> and the database-transaction findings by
> [Decision 0757](../Decisions/0757-Resolve-Language-1.0-Database-Transaction-Findings.md),
> and the compiler-front-end findings by
> [Decision 0758](../Decisions/0758-Resolve-Language-1.0-Compiler-Front-End-Findings.md),
> and the HTTP-handler findings by
> [Decision 0759](../Decisions/0759-Resolve-Language-1.0-Http-Handler-Findings.md),
> the concurrent-service findings by
> [Decision 0760](../Decisions/0760-Resolve-Language-1.0-Concurrent-Service-Findings.md),
> the retained-GUI findings by
> [Decision 0761](../Decisions/0761-Resolve-Language-1.0-Retained-Gui-Findings.md),
> the numeric/graphics findings by
> [Decision 0762](../Decisions/0762-Resolve-Language-1.0-Numeric-Graphics-Findings.md),
> the package-parser findings by
> [Decision 0763](../Decisions/0763-Resolve-Language-1.0-Package-Parser-Findings.md),
> the System/FFI findings by
> [Decision 0764](../Decisions/0764-Resolve-Language-1.0-System-Ffi-Findings.md),
> and complete-suite reconciliation by
> [Decision 0765](../Decisions/0765-Complete-Language-1.0-Source-Freeze-Candidate.md).
> The project owner subsequently held that preserved pre-localization candidate
> and selected stored localized keywords, exact public-library source
> vocabularies, and Unicode project identifiers for a replacement candidate, as
> detailed by the
> [working localized-source specification](../../Specifications/Windvale-Language-1.0-Localized-Source.md).
> Its first paper workload proposes exact source-profile artifact formats and a
> Unicode 17.0.0 profile; those candidates remain under review and do not
> authorize implementation.
> This document remains design rationale: it does not add source syntax, change
> Windvale Seed, select a new WVB version, or claim implementation on any target.
> The currently implemented language remains
> [Windvale Seed](../../Specifications/Seed-Language.md). Examples in this
> document are design sketches rather than accepted source.

## Purpose

Windvale has enough real compiler, library, runtime, tool, application, and
operating-system source to design the complete Language 1.0 contract before
implementing its remaining features. This document records the accepted
product-level direction and its review rationale.

The desired outcome is one coherent language for ordinary applications,
portable libraries, hosted services, compilers, runtimes, and explicitly
privileged system code. Language 1.0 should feel direct and approachable while
keeping allocation, authority, mutation, ownership, failure, and target
requirements visible.

This proposal deliberately changes the sequencing of future language work from
one implemented consumer-driven feature at a time to one complete source design
followed by staged implementation. It does not discard evidence-driven design:
the existing corpus, the paper design corpus, explicit bounds, and later
implementation feedback remain required evidence. It prevents implementation
order from accidentally becoming the permanent language design.

The design phase precedes implementation:

1. review and accept or revise this product-level design (completed by
   Decision 0751);
2. write a normative Language 1.0 grammar and semantic specification;
3. freeze the source contract through a named decision;
4. plan the Seed-to-1.0 migration and compiler implementation; and
5. implement and qualify the accepted language without adding a parallel
   compiler or silently changing the frozen source design.

Source-language finalization does not freeze WVB field encodings, native object
layouts, register allocation, runtime storage strategies, or backend rollout.
Those mechanisms must implement the source semantics but remain separately
versioned contracts.

## Evidence from the current corpus

The design is informed by 866 `.wv` files and approximately 223,000 source
lines. Excluding examples and tests leaves 522 production files and about
171,000 lines, including approximately 70,000 compiler lines and 42,000 library
lines.

The corpus demonstrates real pressure that is not visible in small language
examples:

| Observation | Production evidence |
| --- | ---: |
| Modules using the legacy inline profile spelling | 521 of 522 |
| Payload variants | 4 |
| `match` statements | 5 |
| `try` statements | 1 |
| Real sequence/builder/`for` consumers | 0 |
| Manual `Valid`, `Status`, or `Error` guards | 1,480 |
| Records containing `Valid: bool` | 135 |
| Records containing a `Status` or `Error` field | 291 |
| Functions with at least 10 parameters | 102 |
| Maximum observed parameter count | 24 |
| Records with at least 16 fields | 43 |
| Maximum observed field count | 42 |
| Self-growing `Bytesˉconcat` assignments | 1,856 |
| Self-growing `Textˉconcat` assignments | 241 |

The existing application corpus is only three applications and 181 lines. It
is useful evidence for command-line, capability, result, reporting, and resource
behavior, but it is not sufficient evidence for general application ergonomics.
Language 1.0 therefore also requires the paper design corpus specified below.

The audit supports Windvale's existing safety direction, but it does not support
freezing Seed unchanged. Wide flat records, long positional construction,
packed bytes used as general mutable storage, manual status propagation, and
explicit-close paths that can be bypassed by early return all identify missing
or insufficiently integrated language contracts.

## Product character

Language 1.0 retains these principles:

- immutable values and bindings are the default;
- mutation and unique ownership are visible;
- evaluation order is deterministic and defined;
- integer widths, overflow, conversions, byte order, and text encoding are
  explicit;
- recoverable failure uses typed values rather than exceptions;
- contract violations trap deterministically and are not catchable as ordinary
  application results;
- modules, packages, platform scope, authority, and capabilities remain
  separate concepts;
- allocation and work have explicit static, value, allocator, or resource-domain
  bounds;
- portable semantics do not inherit host language, runtime, path, handle, ABI,
  locale, clock, entropy, or scheduling behavior;
- bytecode is verified before execution; and
- interpreter, JIT, AOT, WebAssembly, and Windvale OS are target paths for one
  source language rather than separate dialects.

Language 1.0 does not add classes, inheritance, implicit `null`, truthiness,
implicit numeric conversion, general exceptions, operator overloading, inferred
overload selection, ambient reflection, hidden capability acquisition,
unrestricted macros, preprocessors, or semantically unbounded collections.

## Why familiar features are outside Language 1.0

These exclusions are deliberate, but they do not mean that every excluded
feature is inherently bad or forbidden forever. Classes, exceptions, reflection,
operator overloading, and macros are useful in existing languages. They also
carry semantic, compiler, runtime, tooling, security, and teaching costs.

Language 1.0 accepts a feature only when its behavior remains visible enough to
review, bounded enough to reason about, deterministic across targets, and useful
enough to justify its permanent cost. Where Windvale excludes a familiar feature,
it must provide a practical alternative rather than merely transferring work to
the programmer.

### Classes

A class commonly combines data layout, reference identity, allocation,
construction, mutation, visibility, dispatch, and lifetime policy in one feature.
That combination makes the object model and generated code larger and makes it
harder to tell which costs and aliasing behavior a value carries.

Windvale separates those concerns. Records and variants describe data, modules
provide encapsulation, protocols describe generic behavior, and owned or shared
storage, resources, arenas, and typed handles make lifetime or identity explicit
where it is actually required. No classes does not mean no abstraction,
encapsulation, or reusable behavior.

### Inheritance

Inheritance introduces implicit behavior through a base hierarchy, couples
distant types, complicates memory layout and construction, and often introduces
dynamic dispatch. Adding or changing a base member can affect code that does not
name the change.

Windvale uses composition and explicit compile-time protocols. A type contains
the values it is built from and explicitly implements the behavior it promises.
This retains static polymorphism without requiring a class hierarchy or hidden
dispatch.

### Implicit `null`

Implicit `null` silently adds an invalid state to otherwise valid types and moves
failure from the point where absence arises to a later dereference. It also makes
every API reader determine whether a value that appears required may secretly be
missing.

Windvale uses `Option<T>` for optional presence. Callers must handle its present
and absent cases, while domain states more specific than absence use a named
variant. A type that does not contain an option cannot be null.

### Truthiness

Truthiness requires language-specific rules for treating integers, text,
collections, handles, and optional values as Boolean. Expressions such as
`if Value` then hide whether the intended test is nonzero, nonempty, present,
valid, or something else.

Windvale conditions require `bool`. The program names the intended comparison or
predicate, making boundary cases visible to readers and tools.

### Implicit numeric conversion

Automatic numeric conversion can change width, signedness, precision, overflow,
and comparison behavior. Even an apparently safe widening may change later
arithmetic or overload selection, while narrowing can silently discard data.

Windvale uses named widening, narrowing, parsing, and bit-reinterpretation
operations with exact failure and overflow behavior. Numeric literals use an
expected type or require an explicit type when the context is insufficient.

### General exceptions

General exceptions add invisible control-flow edges: almost any call may leave
its caller, unwind multiple frames, and run cleanup not apparent in the return
type. This complicates resource accounting, deterministic teardown, ABI
boundaries, and verification of every failure path.

Windvale represents recoverable failure with `Result<T, E>`, `Option<T>`, or
another named variant. Value-producing `try` provides concise exact-error
propagation while preserving failure in the function type. Terminal traps remain
for violated verified contracts and are not catchable as normal application
results.

### Operator overloading

Unrestricted operator overloading lets a compact expression hide arbitrary work,
allocation, mutation, I/O, or failure. It can make domain notation attractive,
but it also makes cost and behavior depend on nonlocal type resolution.

Language 1.0 gives operators fixed built-in meanings and uses named functions for
domain operations. A later proposal may consider narrowly constrained arithmetic
protocols if real Windvale programs demonstrate that named operations are
insufficient and the proposal keeps cost, failure, and resolution visible.

### Inferred overload selection

Inferred overload selection allows imports, inference context, or a newly added
overload to change which function a call means. It increases compiler work and
often produces diagnostics far from the source of ambiguity.

Windvale prefers distinct semantic names, explicit generic contracts, and one
deterministic implementation-selection path. Ordinary calls solve every generic
parameter from exact explicit argument types. Decision 0758 additionally permits
full-arity `Qualifiedˉfunction::<...>(...)` when a named declaration has no such
argument evidence, as empty collection construction demonstrated twice. The
suffix supplies every parameter in declaration order; it does not select an
overload, omit/default a parameter, or infer from result context.

### Ambient reflection

Ambient runtime reflection requires type metadata and otherwise unreachable code
to remain available. It complicates AOT reachability, artifact size, capability
review, deterministic serialization, and the security boundary around private
data.

Windvale uses explicit schemas, generated tables with provenance, named format
contracts, or a deliberately declared inspection capability. No ambient
reflection does not prohibit intentional metadata; it requires the retained data,
authority, and cost to be visible.

### Hidden capability acquisition

If a function can obtain files, networking, time, entropy, processes, or devices
from ambient runtime state, its signature does not reveal its authority. Such
code is harder to test, isolate, revoke, and safely reuse.

Windvale declares capability requirements and binds or passes rights-limited
providers explicitly. Importing a package or running on a capable host never
grants authority by itself.

### Unrestricted macros

Arbitrary token or syntax rewriting can create a language inside the language,
run uncontrolled work during compilation, inspect the build environment, and
produce source that editors, reviewers, and diagnostics cannot see directly.

Windvale instead provides static generics, compile-time protocols, typed
constants, bounded derivation, and explicit generated build inputs with
provenance. Future metaprogramming must remain deterministic, typed where
practical, and bounded in time, memory, recursion, and output.

### Preprocessors

A textual preprocessor runs before ordinary parsing and therefore bypasses
normal names, types, scopes, and grammar. Conditional preprocessing can make one
source file mean different programs to the editor, reviewer, formatter, and
compiler.

Windvale uses typed constants, modules, packages, explicit target profiles, and
ordinary language constructs. Generated source or data is a named build input,
not an invisible import-time transformation.

### Semantically unbounded collections

A collection described as able to grow without a semantic maximum really grows
until an allocator, process, or machine fails. That makes worst-case memory,
time, latency, and failure behavior unknowable, especially in parsers, compilers,
services, and the operating system.

Windvale still provides dynamic vectors, maps, and builders. They grow within an
explicit maximum or resource-domain budget and report capacity or allocation
failure according to their contract. Bounded does not mean fixed-size; it means
that the program and its caller can discover and reason about the limit.

### Design status and tradeoffs

The strongest durable principles are no implicit `null`, no truthiness, no
implicit numeric conversion, no hidden capability acquisition, typed recoverable
failure, and no semantically unbounded resource use. They directly support
Windvale's safety, portability, and deterministic-resource goals.

Other exclusions are Language 1.0 boundary decisions, not claims that a future
language edition can never contain a constrained form. Explicit reflection,
restricted metaprogramming, specialized runtime protocol values, or constrained
domain operators may be reconsidered after representative programs provide
evidence and a proposal defines their bounds. Compatibility pressure or
familiarity alone is not enough to add them.

This direction has real ergonomic costs. Exceptions can shorten failure paths,
classes are familiar, operator notation can improve mathematical code, reflection
can accelerate framework development, and unrestricted collections are
convenient for prototypes. Language 1.0 must make its explicit alternatives
pleasant enough for real applications. The paper design corpus and usability
review therefore test not only whether programs can be expressed, but whether
Windvale's safer forms remain readable and practical.

## What Language 1.0 finalization means

The Language 1.0 design is complete only when every accepted feature has:

- exact lexical and grammar rules;
- static typing and inference rules;
- value, ownership, borrowing, and cleanup behavior;
- deterministic evaluation order;
- allocation and work bounds;
- recoverable failure and terminal-trap behavior;
- module, package, platform, authority, and capability interactions;
- accepted, boundary, malformed, and rejected examples;
- target-support classification; and
- a Seed migration rule.

No required Language 1.0 area may remain described only as "later" or
"implementation-defined." A feature may instead be explicitly outside 1.0 or
assigned to a named hosted or system profile whose semantics are still fully
specified.

Language 1.0 is a source contract. A target may honestly report a smaller
implemented subset during rollout, but it may not reinterpret an implemented
feature. The project claims complete Language 1.0 support only after every core
feature passes the required cross-host and target evidence.

## Editions and compatibility

Every Language 1.0 module declares source edition `1` independently of package
version, WVB version, and target profile. The normative specification will
choose the final declaration position and spelling.

The repository migrates from Seed to edition 1 as one planned transition. The
active-development policy permits removal of superseded Seed syntax and binary
encodings without a permanent compatibility reader. Historical artifacts remain
historical evidence.

Once edition 1 is frozen:

- accepted edition-1 source keeps its semantics;
- compatible library and tooling improvements do not require a source edition;
- a future incompatible grammar or semantic change requires an explicit newer
  edition;
- packages declare the editions they contain;
- compilers reject unknown editions rather than guessing; and
- source compatibility does not imply WVB or native object byte compatibility.

## Language profiles

One grammar and type system serve three semantic profiles:

| Profile | Purpose | Additional authority |
| --- | --- | --- |
| Core | Deterministic portable computation and pure libraries. | None. |
| Hosted | Applications and services using bound capabilities, owned resources, and structured concurrency. | Only explicitly required and granted capabilities. |
| System | Kernel, driver, firmware, machine, or FFI code using visible unsafe operations. | Explicit system authority plus each required capability. |

Profiles are not source dialects. A core module can be consumed by hosted and
system modules. A hosted module cannot become core merely because one execution
did not exercise its capabilities. A system profile is not an implicit unsafe
grant.

The existing independent `platform`, `authority`, required-capability, and
optional-capability metadata direction becomes the edition-1 model. The legacy
inline `profile` spelling and plain capability declaration are removed by the
Seed migration.

## Names and source identity

### Macron-separated names

Language 1.0 retains long semantic names and U+02C9 modifier-letter macron
separators. The separator is part of source identity rather than an alias for
hyphen, underscore, U+00AF macron, or another visually similar character.

Examples:

```text
Compilerˉnativeˉx64ˉlowering
Readˉsourceˉmodule
Maximumˉretainedˉbytes
```

Official source retains capitalized identifiers and
`ALL_CAPS_WITH_UNDERSCORES` constants unless a later review explicitly changes
that convention. Import aliases should remove redundant repeated module prefixes:

```text
import Compilerˉsourceˉparser as Parser;

let Result = Parser.Parseˉexpression(Input);
```

Long names are a source readability choice, not a native ABI requirement.

### Machine names

Private compiled declarations use deterministic short internal identifiers such
as a module identity plus declaration ordinal. Object and executable symbols use
a specified collision-safe ASCII mangling. Exact source names remain available
where required for source linking, exported interfaces, diagnostics, inspection,
or optional debug information.

A stripped product may omit private source-name maps when they are not required
for verification or diagnostics. Public WVB/module identities retain the exact
canonical information required by their format. Truncation without collision
proof is never an acceptable mangling algorithm.

External ABI symbols, capability IDs, package IDs, protocol fields, command names,
and wire identifiers use separately declared ASCII-safe names. An external name
never silently derives host behavior from a source identifier.

### Tooling requirement

Language 1.0 tooling must provide:

- an editor action or input shortcut that inserts exactly U+02C9 without adding
  another source spelling;
- a diagnostic that identifies common lookalike code points;
- semantic rename across modules and aliases;
- exact-name search support;
- formatting that preserves exact normalized project identifiers and selected
  primary source labels;
- explicit source-profile selection through the universal descriptor, canonical
  reveal, deterministic conversion, and manifest/pack/catalog provenance; and
- source-to-machine symbol inspection.

## Lexical and structural grammar

Language 1.0 remains strict UTF-8 with LF as the repository source convention.
The replacement working candidate begins every file with one language-neutral
descriptor such as `#!wv/1 en@1`, then admits normalized Unicode identifier
segments under exact edition-pinned property and security tables while retaining
U+02C9 as the semantic-concept separator. Its explicit immutable source profile
binds the lexicon that maps localized keyword spellings to canonical tokens and
the vocabulary profile whose exact interface-bound catalogs map localized
public-library labels to canonical declarations. Comments and text values
continue to admit full Unicode independently.

The language retains braces, semicolons, explicit delimiters, and deterministic
parsing. Indentation and automatic semicolon insertion are not semantic.
Multiline comma-separated forms permit a trailing comma.

Edition 1 adds documentation comments with one canonical spelling. Documentation
comments affect generated documentation but not executable semantics or artifact
identity unless an enclosing package explicitly carries a documentation artifact.

The normative specification must define ordinary, escaped, raw, and multiline
text and byte literals. Raw or multiline forms must not introduce indentation
guessing, host newline inheritance, implicit normalization, or unbounded compile-
time work.

## Module and package model

One source file declares one module. A module explicitly declares:

- edition;
- canonical module name;
- platform scopes;
- authority;
- required capabilities with major interface versions;
- optional capabilities with major interface versions;
- explicit imports with local aliases; and
- private or exported declarations.

Imports never search the host filesystem from source. A build or package plan
supplies the exact module set and mapping. Import aliases are local vocabulary;
they do not change declaration identity. Wildcard imports, ambient namespaces,
transitive import leakage, dependency-order lookup, and implicit preludes remain
excluded.

Package identity, dependency resolution, capability approval, runtime binding,
and module import are distinct. A source requirement does not grant authority,
and a package dependency does not imply an ambient import.

Language 1.0 admits bounded immutable package data without turning a package
resource into a native path, capability, or owned closeable resource:

```text
export package data Schema: bytes maximum 1_048_576u64;
```

The build or package plan binds the declaration to exact typed content, digest,
length, and maximum. The source value is shared immutable module data. Edition 1
admits only `bytes` and strict-UTF-8 `text`, requires explicit parsing for every
structured format, and permits one content object to satisfy multiple declaration
references without duplicate shipped payload bytes.

Dynamic module loading is outside the core source contract. A future hosted
loading API may admit verified modules as data and bind an explicitly versioned
interface, but it cannot reinterpret edition-1 static imports.

## Type system

Language 1.0 is statically and nominally typed. Public declarations have explicit
types. Local inference resolves one exact initializer type and does not perform
implicit conversion, select overloads, or cross public boundaries.

### Primitive values

The accepted primitive direction is:

| Type | Meaning |
| --- | --- |
| `unit` | The single value `()`, used when a generic or result needs a real no-data value. |
| `never` | No value; the result of a function or expression that cannot return normally. |
| `bool` | Exactly `false` or `true`. |
| `i8`, `i16`, `i32`, `i64` | Checked signed two's-complement integers. |
| `u8`, `u16`, `u32`, `u64` | Checked unsigned integers. |
| `f32`, `f64` | Strictly specified IEEE 754 binary floating-point profiles. |
| `rune` | One valid Unicode scalar value, excluding surrogate code points. |
| `text` | Immutable strict-Unicode text with a bounded UTF-8 representation. |
| `bytes` | Immutable bounded octets. |

`unit` replaces `void` as the ordinary no-data return so `Result<unit, E>` and
generic APIs do not require a special exception. `never` participates only where
control cannot continue; it has no constructible value or default.

There is no pointer-sized integer in portable semantics. Sizes and offsets use an
explicit fixed width selected by the owning format or API. There are no implicit
numeric, Boolean, rune, enum, or floating conversions. Widening, narrowing,
bit reinterpretation, and checked numeric parsing use named operations with exact
failure behavior.

An integer or floating literal without enough expected-type information requires
an explicit suffix. Context may determine a literal's exact type but never convert
an already typed value.

### Floating point

`f32` and `f64` are part of the accepted 1.0 direction because a general application
language should not require a later source redesign for graphics, media,
scientific work, or model workloads. The normative profile must specify:

- rounding mode and every arithmetic/conversion operation;
- NaN construction, comparison, propagation, and canonical serialization;
- infinities, signed zero, subnormals, and overflow;
- whether contraction such as fused multiply-add is permitted only through an
  explicit operation;
- deterministic text parsing and formatting; and
- which cross-target bitwise reproducibility claims are available.

Fast-math transformations that change observable results are never the default.
A target that cannot implement the strict profile does not claim floating-point
support.

### Nominal values

Edition 1 supports nominal records, enums, and variants. Record construction is
named-only; the retained Seed positional constructor is removed.

```text
let Request = Readˉrequest {
    Name: Name,
    Offset: 0u64,
    Maximum: 4096u32,
};
```

A record-update form evaluates one base value first and then explicitly listed
replacement fields from left to right. It cannot omit required fields without a
base or silently default a newly added field.

Variant cases may carry zero or more named fields:

```text
variant Readˉresult<T, E> {
    Valid(Value: T);
    Missing;
    Failure(Error: E, Offset: u64);
}
```

Construction names the variant and case. Matching is exhaustive and can bind
named payload fields. Closed nominal matches do not gain a wildcard that hides
unreviewed cases. A deliberately ignored field may use an explicit discard
pattern.

Tuples are not required for 1.0 public design. Small internal anonymous products
would save characters but weaken names in exactly the wide state and error paths
that the corpus shows need clearer structure. Named records and `unit` cover the
accepted cases unless the paper design corpus demonstrates a concrete tuple need.

### Derived operations

Equality, ordering, hashing, formatting, copying, and serialization are not
automatically granted to every type. An explicit bounded derivation may be
accepted when every contained value satisfies the required compile-time contract
and the maximum work is known.

Builders, mutable buffers, borrowed mutable views, capabilities, resources,
functions, and unsafe handles do not receive general equality or serialization.
Serialization remains a separately versioned format contract rather than a
reflection side effect.

## Generic programming

Language 1.0 includes statically resolved generic records, variants, functions,
and immutable collection values. `Option<T>` and `Result<T, E>` are ordinary
standard nominal variants rather than compiler-magic exceptions.

Generic behavior uses explicit compile-time protocols and explicit
implementations. Protocols may require operations but cannot carry mutable base
state, create inheritance, select an overload through inference, or introduce
implicit dynamic dispatch. Dynamic interface values are outside 1.0 unless a
later design example proves they are essential.

Generic compilation is bounded:

- instantiation uses exact types and compile-time constants;
- calls derive every parameter uniquely from explicit argument types without an
  overload set, result context, conversion, or explicit call suffix;
- recursive instantiation depth, total instances, and emitted-code growth have
  explicit compiler limits;
- implementations are selected deterministically and cannot overlap;
- no runtime reflection or type discovery is implied; and
- separate compilation and package interfaces retain enough canonical generic
  identity to reproduce emitted code.

Compile-time constant parameters support fixed arrays, bounded arenas, and other
values whose exact capacity is part of representation. They are not required for
every dynamic collection API.

## Values, ownership, and memory

Language 1.0 defines four visible value classes:

| Class | Examples | Transfer behavior |
| --- | --- | --- |
| Copy | Scalars, enums, immutable capability references, explicitly derived small values. | Assignment and parameter passing copy the value. |
| Shared immutable | `text`, `bytes`, frozen sequences, immutable maps, qualifying immutable aggregates. | Copying may share backing; backing identity is unobservable. |
| Owned | Vectors, builders, mutable maps, arenas, instance resources, unique buffers. | Assignment and calls move unless an explicit bounded clone exists. |
| Borrowed | Immutable or mutable slices and views. | Lexically bounded; cannot outlive, close, move, or be stored beyond the owner. |

An aggregate adopts the strictest ownership behavior of its fields unless an
explicit safe derivation proves otherwise. Use after move, overlapping mutable
borrow, mutation through an immutable borrow, and escape of a borrow beyond its
owner are compile-time errors.

Language 1.0 does not require a tracing garbage collector. Implementations may
use reference counts, copy-on-write storage, arenas, or another bounded strategy
when source cannot observe the choice. Cyclic general object graphs are not
created accidentally through shared references.

Recursive trees and graphs use an explicit typed arena and generation-checked
handles. The arena has an exact maximum, owns its nodes, invalidates stale handles
on reuse, and makes cycle and teardown behavior explicit. This gives compilers,
databases, UI state, and operating-system services a structured alternative to
packed byte offsets without introducing ambient garbage collection.

Allocation is fallible whenever success depends on a supplied capacity, allocator,
resource domain, or host. An admitted fixed or owner-stored runtime bound prevents
excess; it is not a promise that memory is currently available. Allocation
failure is a typed result, not a trap, unless a caller explicitly invokes a
contract operation whose precondition proves reserved capacity.

## Collections

The implemented Seed `sequence<T, N>` and builder prove deterministic iteration
and affine publication, but their exact-capacity type identity and inability to
cross calls or enter records make them too narrow as the only 1.0 collection
model.

Language 1.0 should distinguish representation and runtime budget:

| Family | Role |
| --- | --- |
| `Array<T, N>` | Fixed-length inline or immutable aggregate whose length is part of its type. |
| `Vector<T>` | Move-owned variable-length contiguous collection with an explicit retained maximum. |
| `Sequence<T>` | Immutable published sequence whose current length and admitted maximum remain observable through bounded operations, not type incompatibility. |
| `Slice<T>` | Borrowed immutable contiguous view. |
| `Mutableˉslice<T>` | Exclusive lexical view into one owned mutable collection. |
| `Map<K, V>` | Move-owned bounded deterministic associative collection with an immutable publication form. |
| `Set<T>` | Move-owned bounded deterministic membership collection with canonical iteration and immutable publication. |
| `Arena<T>` | Owned typed node store with a positive immutable runtime maximum and generation-checked handles; consuming freeze publishes `Immutableˉarena<T>` without changing handle identity. |

Collections may be fields, variant payloads, parameters, results, and elements
when their ownership classes permit it. Immutable collections may nest. Mutable
owners move explicitly; borrows do not escape.

Vector, map, set, and arena construction requires an explicit maximum and an
allocation source or surrounding resource budget. Growth never silently exceeds
the retained maximum. Operations report exact success, unchanged-on-rejection,
or completed prefix behavior; they do not hide partial mutation.

Map semantics must define key equality, collision or ordering behavior, duplicate
policy, iteration order, serialization order, capacity exhaustion, and worst-case
work independently of host hash-table layout. The accepted default is one
canonical deterministic iteration order and a bounded worst-case algorithm;
specialized unordered or insertion-ordered collections require distinct types.
The standard set reuses the same total-order, capacity, worst-case-work, and
publication rules without exposing a dummy map value or host hash-table layout.

`Bytesˉbuilder` and `Textˉbuilder` are specialized owned buffers with bulk append,
formatting, UTF-8 validation, retained maximum, and consuming freeze. They replace
repeated immutable self-concatenation in compiler, object, package, diagnostic,
and application hot paths. Edition 1 keeps the destination builder and its
memory budget explicit; Decision 0765 defers standalone interpolation syntax.

`for` accepts arrays, immutable sequences, slices, maps with defined iteration,
and an explicit bounded iterator protocol. An iterator retains an exact remaining
or maximum item bound. Lazy unbounded iteration and implicit concurrent mutation
remain outside 1.0.

## Functions and calls

Function declarations retain explicit parameter and result types. Calls support
named arguments. Named arguments may appear in a different order, evaluate from
left to right as written, and are placed into canonical parameter order only after
their expressions succeed.

Default arguments and overload selection by inferred types are not required for
1.0. Configuration records and explicit functions keep API evolution and call
identity clearer. A later review may accept defaults only if their declaration,
versioning, evaluation, and separate-compilation behavior is exact.

Functions are first-class immutable values with exact parameter, result, profile,
and effect requirements. A noncapturing function value has a deterministic static
identity. A closure declares every capture as copy, move, or borrow; capture is
never inferred when ownership or authority would change. Borrowing closures cannot
escape the captured lifetime. Closures cannot silently capture capabilities,
resources, mutable globals, or ambient process state.

Function values enable bounded generic algorithms, collection operations,
callbacks, and structured tasks. Dynamic dispatch is explicit and separately
bounded. Tail calls are an optimization unless a named operation guarantees
bounded tail-call behavior.

Recursion is permitted only under the runtime's declared call-depth and work
budgets. A verifier and target report those bounds; source does not assume the
host stack is the semantic limit.

## Statements, expressions, and matching

Edition 1 retains `let`, `var`, assignment statements, `if`, `while`, `for`,
`break`, `continue`, `return`, and exhaustive `match`.

`if` and `match` also have value-producing forms. Every reachable branch must
produce the same exact type and compatible ownership state. Evaluation occurs
once, only the selected branch executes, and no implicit common-type conversion
is attempted.

```text
let Code = match Status {
    case Readˉstatus.Valid { 0u32 }
    case Readˉstatus.Missing { 2u32 }
    case Readˉstatus.Failure { 3u32 }
};
```

Pattern matching supports enum members, variant cases with named payload binding,
records where useful, explicit discard, and Boolean guards whose evaluation and
failure behavior are defined. Exhaustiveness applies after guards; a guard cannot
silently make a closed match incomplete.

Destructuring declarations are allowed only when they bind a statically known
record or selected variant shape and preserve ownership. A destructure cannot
implicitly copy an owned field or leave part of an owned value inaccessible.

## Results, failures, and traps

Language 1.0 separates three outcomes:

1. ordinary values, including domain statuses such as `Missing`;
2. recoverable typed failures represented by `Result<T, E>` or another explicit
   nominal variant; and
3. terminal traps for violated verified contracts, impossible arithmetic,
   malformed unsafe behavior, or exhausted pre-reserved invariants.

There are no catchable general exceptions. A trap does not masquerade as a
recoverable I/O, allocation, provider, cancellation, or application error.

`try` is a value-producing propagation operation over the exact standard result
shape:

```text
let Header = try Readˉheader(Input);
```

It evaluates once, yields the valid payload, or returns the original failure from
the containing function. Exact error equality is the default. Conversion requires
one explicit, statically selected adapter and cannot be inferred from names.
Propagation performs all ordinary lexical cleanup before returning.

`Option<T>` represents optional presence without `null`. Domain states with more
meaning than presence use a named variant rather than nested Boolean flags.

## Resources and cleanup

Language 1.0 distinguishes shared capability references from owned resource
instances. Requiring a filesystem capability does not acquire a file, and copying
a shared provider reference does not create ownership.

Owned resources are move-only, have a provider generation, expose revocation and
peer-loss behavior where relevant, and must be released exactly once. `using`
binds one owned resource to a lexical scope and releases it on normal fallthrough,
`return`, `break`, `continue`, and `try` propagation.

The accepted cleanup model separates fallible semantic completion from
infallible local release:

- operations such as flush, commit, finish, shutdown, or durable close return an
  explicit result and must be invoked deliberately;
- lexical release invalidates the local handle and returns provider capacity; it
  is idempotent or locally infallible after successful acquisition; and
- process termination, provider death, or corruption remains a runtime/service
  teardown boundary rather than a promise that user cleanup code executed.

This avoids discarding a body failure, hiding a cleanup failure, or inventing an
implicit combined error type. A resource whose protocol cannot separate semantic
completion from local release is not eligible for automatic `using` until its
exact combined outcome is specified.

General `defer` is outside the accepted 1.0 core. It can obscure ownership and
failure ordering when `using` and explicit completion already cover resource
lifetimes. Reconsider it only if the design corpus demonstrates non-resource
cleanup that cannot be expressed clearly.

## Capabilities and effects

A module declaration records requirements. A capability value represents an
exact approved and bound provider interface. Those are different facts.

Language 1.0 supports:

- shared immutable singleton references for process-level providers;
- owned instance references returned by explicit acquisition;
- rights-reduced references created only by a named provider operation;
- exact interface major versions and signature-set identities;
- explicit revocation, stale generation, provider restart, and peer-exit outcomes;
  and
- capability values in records, variants, parameters, results, and tasks only
  when their copy/ownership class permits it.

Calling a function through a value must not hide its capability requirements.
Generic protocols and closures retain an exact effect requirement that the caller
can inspect statically. No function can acquire ambient authority because a
package, module profile, or host happens to provide it.

## Text, bytes, and formatting

`text` is valid Unicode and has no locale-dependent comparison or normalization.
`rune` represents one Unicode scalar. Text iteration yields runes in scalar order;
byte offsets and rune positions are distinct types or explicitly named values.

`bytes` is arbitrary immutable octets. Byte order is always named by the format or
operation. Slices validate complete ranges with checked arithmetic before creating
a view. The HTTP workload confirms that slices also need ordinary checked length
and index observation, immutable-byte range borrowing, and strict UTF-8 decode
directly from a byte slice. Otherwise parsers are pushed toward raw pointers or
an avoidable buffer-to-bytes copy. These remain Foundation calls, not indexing
syntax or an HTTP compiler feature.

Decision 0751 initially accepted bounded interpolation after the builder contract
was fixed. The complete paper corpus fixed the builder and formatting protocol
but used no interpolation source, and final reconciliation found no visible
allocation-budget or destination-owner input for a standalone interpolated-text
expression. Decision 0765 therefore keeps explicit bounded formatting calls and
defers interpolation syntax to a later edition. A later proposal must preserve
left-to-right single evaluation, invariant default formatting, exact escaping,
precomputed output bounds, explicit memory ownership, and rejection before
destination mutation.

Locale, user-visible collation, pluralization, time zones, and cultural formatting
belong to explicit libraries and supplied data rather than ambient process state.

## Structured concurrency

The hosted profile includes structured concurrency so network, UI, service, and
agent applications do not require a later incompatible function or ownership
model.

A task belongs to one lexical task scope. The scope has explicit task, queue,
memory, work, and cancellation bounds. Leaving the scope joins, cancels, or fails
according to one declared policy; tasks cannot silently detach and retain borrowed
values or capabilities.

Task creation explicitly moves, copies, or borrows captures under the same closure
rules. Results and failures are typed. Cancellation is a requested state with exact
observation points, not an asynchronous exception. Provider loss, timeout,
cancellation, and application rejection remain distinct outcomes.

A synchronous Hosted operation may receive one opaque launcher-supplied context
binding an absolute monotonic deadline and cancellation view. This keeps clocks
and cancellation authority out of ambient state while allowing exact provider
observation. It does not let application code extend time or cancel tasks; the
concurrent-service workload must connect that provider view to lexical scope
cancellation.

`async` and `await` are syntax over this structured task model. They do not create
an ambient promise runtime, hidden scheduler, implicit replay, or unbounded queue.
Core modules may define deterministic state machines without requiring a scheduler.

The normative design must specify task scope, join ordering, cancellation,
deadline inputs, cleanup, panic/trap containment, provider generation, and target
support before concurrency syntax is accepted.

## Unsafe and foreign boundaries

The system profile includes visible unsafe declarations and blocks. An unsafe
operation must be visible both where it is defined and where it is invoked.
System authority alone does not permit unsafe operations.

Unsafe contracts cover raw addresses, pointer arithmetic, unverified memory,
privileged instructions, foreign calls, ABI layouts, interrupt boundaries, DMA,
and other machine-specific behavior. Every unsafe API states alignment, lifetime,
aliasing, range, initialization, concurrency, unwind/trap, and teardown
requirements.

Foreign interfaces use explicit ABI names, calling conventions, integer widths,
structure layouts, ownership, and error translation. No source record layout,
Boolean representation, enum width, text representation, or native symbol name is
implicitly an external ABI.

Portable and hosted-safe code cannot manufacture an unsafe value, raw pointer,
host handle, or foreign authority.

## Compile-time behavior

Edition 1 retains typed constants and adds only bounded compile-time facilities
required by generic capacities, derived operations, and format-safe values.
Compile-time evaluation uses Language 1.0 checked semantics and explicit step,
depth, memory, and output limits.

Unrestricted macros, token rewriting, arbitrary compiler plugins, build-script
execution during import, and ambient environment inspection remain excluded.
Generated source or data is an explicit build input with provenance, not an
unreported language side effect.

## Determinism and resource accounting

Determinism is semantic where observable and qualified. Language 1.0 defines:

- expression and argument evaluation order;
- integer and strict floating behavior;
- text and byte behavior;
- collection iteration order;
- match and cleanup ordering;
- task result and cancellation rules;
- serialization only through named formats; and
- artifact reproducibility for identical admitted inputs and tool versions.

Determinism does not mean every operation has constant time or that external
providers return the same data. Work, allocation, recursion, retained bytes,
collection capacity, task count, queue depth, diagnostics, and provider operations
remain bounded and accounted.

Optimized implementations may share storage, inline aggregates, use different
registers, or select another bounded algorithm only when source cannot observe a
semantic difference. A simple reference implementation remains the correctness
oracle for optimized paths.

## Diagnostics and tooling

Language 1.0 requires deterministic diagnostics with:

- source edition and compiler phase;
- canonical file/module and source span;
- stable diagnostic identity;
- expected and observed type or ownership state;
- bounded related locations for imports, moves, borrows, matches, and generic
  selection; and
- no unbounded cascade after one malformed construct.

The editor package must follow the accepted grammar. The long-term tooling set
includes formatting, semantic rename, completion, go-to-definition, references,
ownership/move explanation, capability closure inspection, source-to-WVB mapping,
and source-to-native-symbol mapping.

A formatter may choose line breaks and canonical whitespace but cannot rename
identifiers, reorder effectful expressions, normalize text, or change declared
ordering.

## Standard library boundary

The language specification owns primitive values, evaluation, ownership,
borrowing, functions, control flow, capabilities, resource scope, collection
interfaces required by syntax, and target profiles.

The standard library owns algorithms, codecs, parsers, collections not required by
syntax, files, streams, networking, time, entropy, cryptography, formatting,
locale, database, model, UI, and operating-system interfaces. A library may use
compiler-recognized primitives for performance, but its public semantics remain an
ordinary versioned Windvale contract.

Maps should therefore be standard nominal generic types enabled by the 1.0 memory,
generic, and protocol system rather than special map literal semantics in the
parser. Text and byte builders may receive intrinsic lowering while preserving a
normal typed API.

## Paper design corpus

Before source freeze, the candidate language must express complete, reviewable
examples for:

1. a command-line application with parsing, diagnostics, and exit status;
2. bounded file copy with fallible completion and automatic local release;
3. database parsing, lookup, update, and transaction errors;
4. compiler lexing, recursive syntax representation, diagnostics, and byte
   emission;
5. HTTP request handling with bounded headers/body and explicit network rights;
6. concurrent service requests with cancellation and provider restart;
7. GUI or retained application state with events and immutable publication;
8. numeric or graphics processing using strict floating point;
9. package parsing and deterministic map iteration; and
10. one system or driver boundary using explicit unsafe and FFI contracts.

These examples are design tests, not implementation claims. Each must expose
unacceptable verbosity, hidden work, missing ownership, or semantic ambiguity
before the grammar freezes. They later become conformance and migration inputs.

## Accepted directions and specification guide

Decision 0751 accepts these directions for the normative-candidate
specification:

1. retain macron-separated long source names and compile private implementation
   names to deterministic short machine identities;
2. retain Windvale's deterministic, capability-oriented, checked, no-exception
   product character;
3. replace `void` with first-class `unit` and add `never`;
4. complete fixed integer widths and include strict `f32`, `f64`, and `rune`;
5. remove positional record construction and add named update, multi-field
   variants, destructuring, and value-producing `if`/`match`;
6. include bounded static generics and compile-time protocols without inheritance
   or implicit dynamic dispatch;
7. replace the isolated exact-capacity sequence/builder model with fixed arrays,
   runtime-budgeted owned collections, immutable publication, lexical slices, and
   typed arenas;
8. define copy, shared immutable, owned, and borrowed value classes without a
   required tracing garbage collector;
9. standardize `Option<T>` and `Result<T, E>` and make `try` a value-producing
   exact-error propagation operation;
10. include named call arguments, first-class functions, and explicit closure
    capture;
11. add move-only instance resources and `using`, separating fallible completion
    from infallible local release;
12. include bounded text/byte builders and explicit bounded formatting, while
    deferring interpolation syntax until its destination and memory owner are
    visible;
13. specify structured concurrency in the hosted profile; and
14. specify unsafe blocks and FFI in the system profile.

Decision 0752 completes five adjacent edition-1 boundaries: include a bounded
ordered `Set<T>` in Foundation; include bounded immutable `package data` for
`bytes` and `text`; retain static-only source imports; omit default arguments;
and retain ASCII identifier segments joined by U+02C9. Package data is distinct
from move-only resource instances and carries no filesystem authority.

### How to decide

The completed owner review could accept, revise, defer, or reject each direction.
Decision 0751 accepts Option A for all fourteen. That acceptance authorizes the
normative specification to define exact grammar, semantics, diagnostics, and
migration. It does not accept an unfinished syntax sketch or permit
implementation-defined behavior.

The options below describe realistic alternatives and their principal tradeoffs.
They are not all equally compatible with Windvale's product character. The
accepted direction under each decision records the resulting ballot.

### 1. Source names and machine identities

**Option A: retain macron-separated long source names and emit short
deterministic machine identities.**

- Advantages: preserves semantic source readability and Windvale's identity
  while reducing private symbol, object, executable, and diagnostic-map cost.
- Costs: typing U+02C9 requires editor support, and debugging requires an exact
  source-to-machine mapping.

**Option B: retain long names in source and machine artifacts.**

- Advantages: direct machine-symbol inspection can show a recognizable source
  name.
- Costs: larger artifacts and symbol tables, awkward external tool limits, and
  no reduction of private implementation metadata.

**Option C: replace the convention with an ASCII naming style.**

- Advantages: immediate support on every keyboard and existing tool.
- Costs: abandons the selected language identity and long semantic-word
  separation. Supporting ASCII and macron aliases together would also create two
  canonical spellings and should not be accepted.

**Accepted direction:** Option A. Exact names remain in source interfaces and
diagnostics where required; private machine identities remain deterministic,
collision-safe, and inspectable through tooling.

### 2. Product character

**Option A: retain the deterministic, checked, capability-oriented,
no-exception model.**

- Advantages: visible failure and authority, predictable execution, bounded
  resource reasoning, and portable semantics.
- Costs: more explicit source and a requirement for excellent ergonomic
  libraries, diagnostics, and propagation syntax.

**Option B: relax individual rules whenever a familiar feature is convenient.**

- Advantages: may shorten individual programs during early development.
- Costs: accumulates unrelated exceptions, creates multiple error and authority
  models, and eventually removes the language's coherent identity.

**Option C: adopt a conventional managed-language model.**

- Advantages: familiar to C# and Java programmers.
- Costs: brings hidden exceptions, ambient services, tracing-GC assumptions, and
  a substantially larger runtime and semantic surface.

**Accepted direction:** Option A. Improve the ergonomics of explicit behavior
rather than weakening the behavior itself.

### 3. `unit` and `never`

**Option A: replace `void` with first-class `unit` and add `never`.**

- Advantages: `Result<unit, E>` and generic algorithms treat no-information
  success normally; `never` precisely describes traps, termination, and other
  non-returning control flow.
- Costs: the names and zero-information value model require explanation for
  developers accustomed to `void`.

**Option B: retain `void` and omit `never`.**

- Advantages: familiar function declarations.
- Costs: `void` remains a special return-only rule, generic use is awkward, and
  reachability or exhaustive matching loses information.

**Option C: add `unit` but omit `never`.**

- Advantages: simplifies the initial control-flow type system.
- Costs: still cannot type a non-returning expression precisely.

**Accepted direction:** Option A. The normative specification must distinguish
source values from ABI return conventions; `unit` need not occupy runtime
storage.

### 4. Fixed numerics, strict floating point, and `rune`

**Option A: complete the fixed-width integer family and specify strict `f32`,
`f64`, and `rune` semantics.**

- Advantages: portable numeric behavior and support for scientific, graphics,
  model, Unicode, and general application workloads.
- Costs: NaN, rounding, overflow, subnormal, conversion, and contraction rules
  require exact specification and cross-target tests.

**Option B: retain the mostly integer Seed subset.**

- Advantages: smaller immediate compiler implementation.
- Costs: insufficient for general applications and forces target-specific or
  foreign numeric escape paths.

**Option C: inherit host-native widths or ordinary host floating behavior.**

- Advantages: easiest initial lowering.
- Costs: the same source may change limits or results by architecture, backend,
  compiler option, or host runtime.

**Accepted direction:** Option A. Decimal, arbitrary-precision, and specialized
numeric types remain libraries. The normative specification must include a
complete conversion matrix and strict floating profile.

### 5. Records, variants, destructuring, and value-producing control flow

**Option A: require named record construction and add named update, multi-field
variants, destructuring, and value-producing `if` and `match`.**

- Advantages: wide records remain reviewable, field reordering is safe, variants
  replace Boolean/status bundles, and control flow produces typed values without
  temporary mutation.
- Costs: a deliberate Seed migration and more grammar and ownership rules.

**Option B: retain positional record construction and the smaller Seed surface.**

- Advantages: concise construction for small records and less initial compiler
  work.
- Costs: fragile call sites and poor readability for the existing records with
  dozens of fields.

**Option C: permanently support both positional and named construction.**

- Advantages: callers can select the shorter form.
- Costs: preserves the fragile form indefinitely and creates two review and style
  conventions for the same type.

**Accepted direction:** Option A. A same-name field shorthand may reduce
repetition without making field identity positional.

### 6. Static generics and compile-time protocols

**Option A: include bounded static generics and protocols without inheritance or
implicit dynamic dispatch.**

- Advantages: reusable typed collections and algorithms with predictable
  behavior and normally no runtime dispatch.
- Costs: greater compiler complexity and possible generated-code growth through
  specialization.

**Option B: use runtime interfaces and implicit dynamic dispatch.**

- Advantages: runtime extensibility and potentially less specialized code.
- Costs: adds interface-object representation, dispatch, lifetime, allocation,
  and reachability questions.

**Option C: omit generics.**

- Advantages: substantially simplifies the initial compiler.
- Costs: duplicates collections, results, options, and algorithms for every type
  and prevents the standard library from presenting one coherent typed surface.

**Option D: adopt class inheritance as the reuse mechanism.**

- Advantages: familiar to object-oriented developers.
- Costs: introduces the combined layout, identity, allocation, mutation, and
  dispatch model deliberately excluded above.

**Accepted direction:** Option A. Freeze only after limits for instantiation
depth, instance count, overlap, separate compilation, and emitted-code growth are
specified and tested.

### 7. Collection families and budgets

**Option A: provide fixed arrays, owned vectors and maps, immutable publication,
lexical slices, builders, and typed arenas.**

- Advantages: covers real compiler and application workloads, separates fixed
  shape from runtime capacity, and avoids repeated immutable concatenation.
- Costs: substantial ownership and library design; explicit maxima or resource
  budgets can become verbose if the APIs are poor.

**Option B: retain only exact-capacity `sequence<T, N>` and its builder.**

- Advantages: simple capacity reasoning and deterministic iteration.
- Costs: cannot adequately cross records and calls or serve general maps,
  parsers, graphs, and application data.

**Option C: add conventional semantically unbounded collections.**

- Advantages: easiest prototype experience.
- Costs: hidden growth and unknowable worst-case memory, latency, and failure
  behavior.

**Accepted direction:** Option A. Every growing collection must
receive an explicit maximum or a surrounding resource-domain budget. The paper
corpus must prove that the budget APIs remain practical.

### 8. Copy, shared immutable, owned, and borrowed values

**Option A: define the four value classes without requiring tracing garbage
collection.**

- Advantages: efficient copying and immutable sharing, deterministic release,
  explicit mutation, and compile-time prevention of use-after-move and invalid
  borrows.
- Costs: the largest new learning and compiler-analysis burden in the proposal.

**Option B: require tracing garbage collection and general managed references.**

- Advantages: familiar sharing and easy construction of arbitrary cyclic object
  graphs.
- Costs: nondeterministic retention and pauses, a larger runtime, hidden
  aliasing, and poor fit for bounded services, drivers, and the operating system.

**Option C: expose manual allocation and free as the ordinary safe model.**

- Advantages: keeps the runtime mechanism small and makes allocation locations
  explicit.
- Costs: permits leaks, double-free, use-after-free, and invalid aliasing in
  ordinary application code.

**Option D: use universal reference counting.**

- Advantages: provides deterministic release for acyclic reference graphs.
- Costs: adds pervasive count traffic and still leaks cycles unless another
  cycle-management mechanism is introduced.

**Accepted direction:** Option A. Move rules, borrow notation,
escape checking, immutable publication, hidden sharing, typed arenas, and
diagnostics require paper prototypes before source freeze.

### 9. `Option<T>`, `Result<T, E>`, and `try`

**Option A: standardize one typed failure model with exact propagation.**

- Advantages: failure remains visible, `try` keeps the successful path concise,
  and the model replaces widespread manual `Valid` and `Status` guards.
- Costs: APIs must design meaningful error types and write explicit adapters when
  crossing error domains.

**Option B: introduce general exceptions.**

- Advantages: short successful paths and a familiar application model.
- Costs: invisible control flow and cleanup, resource, ABI, and verification
  complexity.

**Option C: retain Boolean and status-record conventions.**

- Advantages: matches much of the currently implemented source.
- Costs: remains repetitive, is easy to ignore, and continues the manual guard
  patterns observed throughout the corpus.

**Option D: support both exceptions and typed results.**

- Advantages: gives each API local choice.
- Costs: creates competing failure cultures and requires every caller, library,
  and resource construct to support both control-flow models.

**Accepted direction:** Option A. Error conversion remains explicit and
statically selected; `try` must not infer an adapter from names or context.

### 10. Named arguments, function values, and closure capture

**Option A: include named call arguments, first-class functions, and explicit
copy, move, or borrow capture.**

- Advantages: makes wide calls readable, enables callbacks and algorithms, and
  exposes retained ownership and capability requirements.
- Costs: closure representation and lifetime analysis add compiler work, while
  capture lists add visible syntax.

**Option B: add named arguments but omit closures and function values.**

- Advantages: improves current call readability with a smaller implementation.
- Costs: leaves callbacks, collection algorithms, and structured tasks awkward.

**Option C: infer captures as C# commonly does.**

- Advantages: concise lambdas.
- Costs: may silently retain mutable state, resources, capabilities, large
  values, or a borrow beyond its valid scope.

**Accepted direction:** Option A. Calls evaluate argument expressions from
left to right as written, and capture mode is never inferred when ownership or
authority would change.

### 11. Move-only resources and `using`

**Option A: separate fallible semantic completion from infallible local
release.**

- Advantages: deterministic cleanup across every exit path without discarding
  flush, commit, finish, or shutdown failures.
- Costs: some protocols require two visible actions: completion and release.

**Option B: use one automatic close operation that may fail.**

- Advantages: appears simpler at the API surface.
- Costs: cannot unambiguously report a body failure and a close failure together,
  and scope exit may silently lose one result.

**Option C: rely on runtime finalizers.**

- Advantages: requires little explicit source for abandoned resources.
- Costs: finalizer timing is unknown, may occur under resource exhaustion, and
  cannot safely promise semantic flush, commit, or durable completion.

**Option D: add general `defer` as the primary cleanup model.**

- Advantages: can express arbitrary lexical cleanup.
- Costs: complicates ownership and failure ordering and allows fallible semantic
  completion to hide at scope exit.

**Accepted direction:** Option A. Freeze only after exact release
order, acquisition failure, partial initialization, nested resources, and
rules preventing a body, completion, or cleanup result from being silently
discarded are specified.

### 12. Bounded builders and interpolation

**Option A: include bounded byte/text builders and bounded interpolation.**

- Advantages: directly removes repeated concatenation, gives predictable memory,
  and supports readable diagnostics and application text.
- Costs: maximum-output derivation, escaping, formatting, and capacity failure
  need exact contracts.

**Option B: retain immutable concatenation as the primary construction model.**

- Advantages: requires little new language or library work.
- Costs: repeated append patterns continue copying and allocating throughout
  compiler and application hot paths.

**Option C: provide bounded builders but no interpolation syntax.**

- Advantages: solves the copying problem with a smaller grammar.
- Costs: ordinary diagnostics, logging, and application text remain unnecessarily
  verbose despite having a safe bounded construction mechanism.

**Option D: use conventional unbounded builders and interpolation.**

- Advantages: familiar and convenient.
- Costs: a small source expression may hide arbitrary allocation and formatting
  work.

**Accepted direction after complete-suite reconciliation:** Option C. Builders
and the formatting protocol remain ordinary typed library APIs even when the
compiler recognizes and optimizes them. Decision 0765 defers interpolation
syntax because the paper corpus did not prove a standalone expression with an
explicit destination owner, memory budget, and failure path. A later edition may
add it without changing the edition-1 builder or formatting identities.

### 13. Structured concurrency

**Option A: specify lexical task scopes, joins, cancellation, bounds, and
`async`/`await` in the hosted profile.**

- Advantages: supports services, networking, UI, and agent applications without
  detached work or leaked borrows, resources, and capabilities.
- Costs: the most complex individual 1.0 decision; scheduler independence,
  cancellation observation, result ordering, and teardown require extensive
  design.

**Option B: define task ownership and effects now but defer convenience syntax.**

- Advantages: preserves a compatible future foundation while reducing initial
  grammar and compiler work.
- Costs: hosted programming may remain awkward and require a later language
  addition.

**Option C: use unstructured spawning, promises, or threads.**

- Advantages: quick to prototype and initially familiar.
- Costs: permits detached work and makes joining, cancellation, queue bounds,
  retained capabilities, and teardown difficult to guarantee.

**Option D: make concurrency entirely a library concern.**

- Advantages: keeps scheduler and task syntax out of the language specification.
- Costs: the compiler cannot reliably enforce task lifetime, borrowing, capture,
  scope exit, and ownership transfer.

**Accepted direction:** Option A at the design level. Implementations may
stage target support, but the Language 1.0 function, ownership, and effect model
must account for structured tasks before source freeze.

### 14. Unsafe and foreign boundaries

**Option A: define visible unsafe declarations and call sites in the system
profile with exact FFI contracts.**

- Advantages: Windvale can implement its runtime, drivers, operating system, and
  foreign adapters without weakening portable code.
- Costs: a large security surface requiring target-specific ABI specifications,
  malformed-boundary tests, and strong audit tooling.

**Option B: defer unsafe and FFI beyond Language 1.0.**

- Advantages: smaller initial source specification.
- Costs: Windvale cannot implement its own stack without persistent
  external-language escape paths.

**Option C: permit native pointers and machine operations in ordinary code.**

- Advantages: convenient for low-level implementation.
- Costs: destroys the portable-safe boundary and makes unsafe authority difficult
  to audit at definitions and call sites.

**Option D: expose only opaque safe libraries and omit language-level unsafe
primitives.**

- Advantages: ordinary application code sees a small safe surface.
- Costs: the libraries still require another language or hidden compiler
  mechanisms to implement their unsafe internals.

**Accepted direction:** Option A. Unsafe behavior is visible where defined and
invoked, remains isolated to the system profile, and never grants an undeclared
capability by itself.

### Accepted ballot and freeze conditions

| Decisions | Decision 0751 status | Required evidence before source freeze |
| --- | --- | --- |
| 1–5 | Accept | Complete grammar, semantic, diagnostic, and migration examples. |
| 6 | Accept direction | Preserve Decision 0754's exact argument-derived selection and prove generic code-growth bounds. |
| 7 | Accept direction | Design usable collection-maximum and resource-budget APIs. |
| 8 | Accept direction | Prototype moves, borrows, immutable sharing, and typed arenas. |
| 9–10 | Accept | Complete exact error propagation, call, and closure rules. |
| 11 | Accept direction | Resolve cleanup ordering without discarding any result. |
| 12 | Refined by Decision 0765 | Keep explicit bounded builders/formatting; defer interpolation syntax until destination and memory ownership are explicit. |
| 13 | Accept direction | Complete the structured-concurrency paper corpus and semantics. |
| 14 | Accept | Specify each supported ABI and unsafe invariant. |

Decisions 8, 11, and 13 carry the highest semantic and usability risk. Their
accepted direction is coherent with the rest of the language, but they need the
deepest paper design and diagnostics review. This acceptance does not permit
those areas to be marked final before their freeze conditions are met.

## Explicitly outside Language 1.0

The following remain outside 1.0 unless review of this document changes the
boundary. The reader-facing rationale and intended alternatives are described in
"Why familiar features are outside Language 1.0" above:

- classes and inheritance;
- implicit `null`, optional coercion, and truthiness;
- general exceptions or catchable traps;
- operator overloading and inferred overload resolution;
- runtime reflection and automatic object serialization;
- tracing-GC-dependent cyclic object graphs;
- implicit or detached background tasks;
- dynamic source imports or runtime name lookup through `import`;
- default parameter values;
- interpolated-text syntax without an explicit bounded destination and memory
  owner;
- ambient, inferred, mixed, or runtime-selected source lexicons and public source
  vocabularies;
- unbounded collections, queues, diagnostics, recursion, or compile-time work;
- unrestricted macros, preprocessors, and compiler plugins;
- wildcard imports and ambient preludes;
- hidden capability acquisition;
- host-native paths, handles, ABI, locale, encoding, or scheduler semantics in
  portable code; and
- source syntax whose correctness depends on indentation or automatic semicolon
  insertion.

## Design-phase completion and handoff

Following owner acceptance, the design and specification phase produces:

1. accepted amendments to this document;
2. the normative-candidate
   [semantic specification](../../Specifications/Windvale-Language-1.0.md),
   [grammar](../../Specifications/Windvale-Language-1.0-Grammar.md),
   [machine grammar](../../Specifications/Windvale-Language-1.0.ebnf),
   [Foundation contract](../../Specifications/Windvale-Language-1.0-Foundation.md),
   and [Foundation signature registry](../../Specifications/Windvale-Language-1.0-Foundation-Registry.md);
3. the [paper design corpus](Windvale-Language-1.0-Paper-Corpus.md) with accepted
   and rejected examples;
4. the [Seed-to-edition-1 migration plan](Windvale-Language-1.0-Migration.md);
5. a feature-to-compiler/WIR/WVB/runtime/backend/editor test matrix;
6. a complete [source-freeze review packet](Windvale-Language-1.0-Source-Freeze-Review.md)
   followed by a named Language 1.0 source-freeze decision; and
7. an implementation roadmap that preserves one compiler and the narrowest
   reliable verification path.

No implementation schedule or backend shortcut may silently redefine an accepted
source rule. If implementation evidence exposes a contradiction, the design
returns to explicit review rather than accumulating an undocumented exception.
