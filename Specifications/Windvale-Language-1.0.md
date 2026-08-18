# Windvale Language 1.0 semantic specification

## Status

This document is the normative-candidate semantic specification authorized by
[Decision 0751](../Documents/Decisions/0751-Accept-Windvale-Language-1.0-Direction.md).
It is exact enough to guide grammar review, paper programs, and implementation
planning, but source edition 1 is not frozen or implemented yet. The currently
implemented contract remains the
[Windvale Seed language](Seed-Language.md).

The project owner held the preserved pre-localization candidate and reopened
Language 1.0 to include the
[localized-source and source-vocabulary addendum](Windvale-Language-1.0-Localized-Source.md).
[Decision 0766](../Documents/Decisions/0766-Complete-Language-1.0-Localized-Source-Reconciliation.md)
accepts the five localization workload findings and reconciles that addendum
into the replacement candidate. It does not retroactively change the preserved
candidate identity, freeze edition 1, or authorize implementation.

The candidate may change when required paper programs expose ambiguity,
unacceptable ergonomics, an unbounded operation, or a target contradiction. A
later named source-freeze decision makes accepted edition-1 source semantics a
compatibility promise.

## Rule ownership

The Language 1.0 suite, as refined by
[Decision 0752](../Documents/Decisions/0752-Complete-Language-1.0-Collection-And-Package-Data-Boundaries.md)
and
[Decision 0753](../Documents/Decisions/0753-Require-Language-1.0-AI-Accelerator-Evidence.md),
with the first paper findings resolved by
[Decision 0754](../Documents/Decisions/0754-Resolve-First-Language-1.0-Paper-Findings.md),
and the command workload findings resolved by
[Decision 0755](../Documents/Decisions/0755-Resolve-Language-1.0-Command-Workload-Findings.md),
and the file-copy workload findings resolved by
[Decision 0756](../Documents/Decisions/0756-Resolve-Language-1.0-File-Copy-Findings.md),
and the database-transaction findings resolved by
[Decision 0757](../Documents/Decisions/0757-Resolve-Language-1.0-Database-Transaction-Findings.md),
and the compiler-front-end findings resolved by
[Decision 0758](../Documents/Decisions/0758-Resolve-Language-1.0-Compiler-Front-End-Findings.md),
and the HTTP-handler findings resolved by
[Decision 0759](../Documents/Decisions/0759-Resolve-Language-1.0-Http-Handler-Findings.md),
and the concurrent-service findings resolved by
[Decision 0760](../Documents/Decisions/0760-Resolve-Language-1.0-Concurrent-Service-Findings.md),
and the retained-GUI findings resolved by
[Decision 0761](../Documents/Decisions/0761-Resolve-Language-1.0-Retained-Gui-Findings.md),
and the numeric/graphics findings resolved by
[Decision 0762](../Documents/Decisions/0762-Resolve-Language-1.0-Numeric-Graphics-Findings.md),
and the package-parser findings resolved by
[Decision 0763](../Documents/Decisions/0763-Resolve-Language-1.0-Package-Parser-Findings.md),
and the System/FFI findings resolved by
[Decision 0764](../Documents/Decisions/0764-Resolve-Language-1.0-System-Ffi-Findings.md),
with complete-suite reconciliation accepted by
[Decision 0765](../Documents/Decisions/0765-Complete-Language-1.0-Source-Freeze-Candidate.md),
and localized-source reconciliation accepted by
[Decision 0766](../Documents/Decisions/0766-Complete-Language-1.0-Localized-Source-Reconciliation.md),
has one owner for each kind of rule:

| Contract | Owner |
| --- | --- |
| Static and dynamic source semantics, profiles, effects, ownership, evaluation, and conformance | This document |
| Tokens, literal spelling, precedence, and parsing | [Language 1.0 grammar](Windvale-Language-1.0-Grammar.md) and its [machine projection](Windvale-Language-1.0.ebnf) |
| Source lexicons, localized public API source labels, Unicode source identifiers, and localization-specific conformance | [Localized-source and source-vocabulary addendum](Windvale-Language-1.0-Localized-Source.md) |
| Exact source-profile component bytes, bounds, hashes, and admission order | [Source-profile artifact formats](Windvale-Language-1.0-Source-Profile-Formats.md) |
| Required standard variants, protocols, collections, builders, budgets, and failure types | [Language 1.0 Foundation](Windvale-Language-1.0-Foundation.md) and its [signature registry](Windvale-Language-1.0-Foundation-Registry.md) |
| Design motivation and rejected alternatives | [Language 1.0 design](../Documents/Project/Windvale-Language-1.0-Design.md) |
| Seed transition order and compatibility boundary | [Seed-to-1.0 migration](../Documents/Project/Windvale-Language-1.0-Migration.md) |
| Usability and boundary evidence before source freeze | [Language 1.0 paper corpus](../Documents/Project/Windvale-Language-1.0-Paper-Corpus.md) |
| Accelerator and AI pre-freeze design evidence | [Accelerator compute and AI design](../Documents/Project/Windvale-Accelerator-Compute-And-AI-Design.md) |

When two documents appear to overlap, the owner in this table controls. A
candidate revision updates every dependent example and cross-reference in the
same coherent change.

The terms **must**, **must not**, **required**, **shall**, and **shall not** are
normative. **Should** records a strongly preferred choice whose permitted
alternative is stated. **May** grants an implementation or source choice without
changing observable semantics.

## Product contract

Windvale Language 1.0 is one deterministic, statically typed,
capability-oriented language for portable computation, hosted applications, and
explicit system code. It has:

- immutable values and bindings by default;
- visible mutation, ownership, borrowing, allocation, authority, and failure;
- fixed-width checked numerics and no implicit conversions;
- typed recoverable failure rather than catchable general exceptions;
- bounded collections, recursion, compile-time work, tasks, queues, diagnostics,
  and retained state;
- verified semantics shared by interpreter, JIT, cached compilation, AOT,
  WebAssembly, and Windvale OS targets;
- one canonical token and declaration model beneath explicitly selected stored
  source lexicons and public-library vocabularies; and
- no dependency on a tracing garbage collector, host object model, host path
  model, host scheduler, or host exception behavior.

An optimization may change storage, register choice, sharing, layout, scheduling,
or algorithm only when no admitted source program can observe a semantic
difference.

## Conformance and implementation status

An implementation reports:

- accepted source editions;
- implemented language profiles;
- exact platform, architecture, ABI, and extension scopes;
- compiler and runtime resource limits;
- strict floating-point support;
- Foundation contract version;
- supported capability interfaces;
- supported unsafe ABIs; and
- any incomplete edition-1 feature during staged development.

An implementation must reject an unknown edition, unsupported profile, unmet
limit, unsupported capability, unsupported ABI, or unavailable strict numeric
operation. It must not guess, silently lower to host behavior, or claim complete
Language 1.0 support for a partial surface.

Complete Core conformance requires every Core rule and required Foundation Core
type. Complete Hosted conformance includes Core plus every Hosted rule. Complete
System conformance includes Core, Hosted semantics used by the target, and every
claimed System ABI rule. Source conformance does not imply WVB, object, package,
or native-byte compatibility; those formats remain separately versioned.

## Source descriptor and module header

Every source file contains exactly one module and begins with this descriptor
and logical module header, whose exact grammar is owned by the grammar companion:

~~~text
#!wv/1 en@1
module Imageˉtool;
profile hosted;
platform windows, linux, windvale;
authority application;
requires capability filesystem.read version 1;
optional capability window.surface version 1;
~~~

A localized file selects its exact composite source profile in the same neutral
descriptor:

~~~text
#!wv/1 zh-Hans@1
<localized module header and body>
~~~

The declarations occur once and in this order:

1. The first-line descriptor is file-format metadata selecting source edition 1 and one
   explicit immutable composite source profile. It is not a localized language
   declaration.
2. `module` gives the canonical source module name.
3. `profile` selects `core`, `hosted`, or `system` language features.
4. `platform` lists one or more canonical platform scopes; omission and implicit
   current-host selection are invalid.
5. `authority` selects the part role defined by the application and system
   architecture.
6. required and optional capability declarations state interface identity and
   major version without granting or binding a provider.

The universal descriptor grammar, source-profile binding, unconditional strict
resolution, and no-fallback behavior are owned by the
localized-source addendum. The canonical paper corpus uses explicit `en@1`;
there is no ambient profile default.

An optional capability is metadata only until an application explicitly
approves and binds it. A source module cannot call or capture an optional-only
capability.

One build or package plan supplies the complete module set and maps canonical
module identities to source bytes. Source imports never search host paths,
environment variables, registries, package caches, or the network.

## Platform and target scopes

A `platform` item is one opaque canonical key in the Language 1.0 target-scope
registry. Period-separated segments make the key readable; they do not create
inheritance, prefix compatibility, or an implied hierarchy.

Every concrete build target has one structured descriptor containing:

- one environment identity;
- one architecture identity;
- one ABI identity;
- a finite set of extension identities; and
- a finite set of target-interface identities.

A registry entry maps its key to one exact predicate over that descriptor. The
items in a source `platform` declaration are alternatives: a module is admitted
for a concrete target when at least one listed registry predicate matches. They
are not dimensions to combine, provider requests, capability grants, backend
names inferred by the compiler, or permission to weaken source semantics. A
build plan selects one concrete descriptor per produced artifact and retains the
registry identity used for admission. An unknown key or a listed set with no
match rejects the build before artifact publication.

The initial candidate registry entries required by the accepted suite are:

| Scope | Kind | Exact predicate |
| --- | --- | --- |
| `windows` | Environment | Environment identity is Windows. |
| `linux` | Environment | Environment identity is Linux. |
| `windvale` | Environment | Environment identity is Windvale OS. |
| `linux.x86_64.sysv_amd64_c_v1` | Concrete System ABI | Environment is Linux, architecture is x86-64, ABI identity is `sysv_amd64_c_v1`, and the no-unwind C scalar/pointer interface major is 1. |
| `accelerator.software.v1` | Target interface | The descriptor supplies the Windvale accelerator software-kernel interface, major 1. |
| `accelerator.spirv.v1` | Target interface | The descriptor supplies the Windvale SPIR-V accelerator-kernel interface, major 1. |

The `v1` suffix on an accelerator entry versions the Windvale target interface;
it does not select an upstream SPIR-V version. Neither accelerator scope implies
a physical device, vendor, host environment, attachment mode, capability,
architecture, ABI, provider implementation, or performance claim. Those facts
remain separate fields and contracts. A later target contract may add registry
entries without changing grammar, but changing the predicate of an existing key
is incompatible.

## Language profiles

The profiles form a source-feature inclusion order, not an authority lattice:

| Profile | Admitted source |
| --- | --- |
| Core | Deterministic computation, immutable publication, owned memory, generic algorithms, and pure libraries without capability calls, instance resources, tasks, unsafe operations, or FFI. |
| Hosted | Core plus approved capability references, owned provider instances, resource scope, deadlines, cancellation, and structured tasks. |
| System | Core and Hosted constructs plus visible unsafe operations, raw memory, privileged operations, and declared foreign ABIs. |

A Core module may be imported by Hosted or System modules. A Hosted module may be
imported by System. A reverse edge is rejected even when execution would not
reach the stronger feature. Platform scope, authority, capability requirements,
and profile remain independent dimensions.

Selecting `system` does not grant unsafe permission or a capability. Each unsafe
operation still requires an unsafe declaration and invocation context, and every
external authority still requires an approved capability or system contract.

## Names and declaration identity

Source is strict UTF-8. Source identifiers use case-sensitive, normalized
Unicode segments joined only by U+02C9 modifier letter macron. The exact
edition-pinned Unicode property, normalization, security, and rejection rules
are owned by the localized-source addendum and grammar companion. ASCII source
identifiers remain an admitted subset:

~~~text
[A-Za-z_][A-Za-z0-9_]*(ˉ[A-Za-z_][A-Za-z0-9_]*)*
~~~

A project-owned identifier is bounded to 256 UTF-8 bytes, 128 Unicode scalars,
and 32 semantic segments. Source labels carried by lexicons/catalogs retain their
stricter 128-byte/64-scalar artifact bounds.

U+02C9 is part of identity. Hyphen, U+00AF macron, and other lookalikes are not
aliases. Exact Unicode identity receives no case folding, host normalization,
collation, transliteration, or canonically-equivalent spelling alias. Official
source uses reviewed semantic identifiers with macron-separated concepts as
defined by [Source naming](Source-Naming.md). Cased scripts retain the
capitalized/constant conventions where meaningful; uncased scripts do not
invent case.

Source has one logical left-to-right grammar order even when token content is
Arabic or Hebrew. Exact implicit-mark, stateful-control, hard-line rejection,
source-aware display, and raw-provenance behavior is owned by the localized-
source and grammar companions; rendering never changes token order or identity.

A declaration has one canonical identity consisting of edition-aware package
identity, canonical module identity, declaration category, and source name.
Import aliases are local vocabulary and do not change identity.

A localized reference to an imported public declaration resolves through one
exact catalog to that declaration's existing canonical identity. It does not
create a translated export, alternate declaration, overload, ABI alias, or
runtime lookup. Project-owned Unicode declarations retain their exact stored
source name as their canonical source name.

Compiled private declarations use deterministic short internal identities under
their owning WIR, WVB, object, or native format contract. Exported interfaces,
diagnostics, source linking, and optional debug information retain the exact
canonical information they require. Native and foreign symbols use separately
specified collision-safe ASCII names; truncation without a collision proof is
invalid.

## Modules, imports, and visibility

Declarations are private unless marked `export`. Imports:

- name one supplied canonical module and one unique local alias;
- appear before non-import declarations;
- do not re-export declarations;
- do not leak transitively;
- do not create wildcard or ambient lookup; and
- must form an acyclic static source graph.

An unqualified name searches only lexical declarations and the current module.
An imported declaration requires its alias and must be exported by the imported
module. Resolution never depends on dependency argument order or filesystem
layout.

Packages, source imports, dynamic verified-module loading, provider binding, and
capability approval are distinct contracts. Language 1.0 has no dynamic import
syntax and no implicit prelude.

A `const` declaration is a storage-free typed compile-time value. A `data`
declaration creates one immutable module value and may contain only Copy or
shared immutable values whose complete construction is admitted at compile time.
Module data cannot contain an owned value, borrow, capability, resource, task,
unsafe handle, or foreign pointer. Core and Hosted source have no mutable module
global.

A `package data` declaration creates one shared immutable `bytes` or `text`
module value whose content is supplied by the build or package plan rather than a
source initializer:

~~~text
export package data Schema: bytes maximum 1_048_576u64;
~~~

The maximum is an exact `u64` byte count. A `text` binding must be strict UTF-8,
and its maximum counts encoded bytes without normalization. The plan binds the
declaration's canonical identity to one canonical package-resource identity,
exact digest, byte length, and type. Missing, duplicate, oversized,
digest-mismatched, invalid-text, or incompatible bindings reject construction
before publication.

Package data is not an owned resource, runtime lookup, filesystem path,
capability, provider grant, or automatic deserialization operation. Access has
the ordinary semantics of shared immutable module data. Its retained bytes are
charged to the selected application or service resource domain even when an
implementation maps or shares storage. Canonical packaging uses one content
object per distinct content identity and may reference that object from multiple
declarations without duplicating its shipped payload.

Each declaration reference still validates its own canonical declaration and
resource identity, type, maximum, exact length, and digest. Within one admitted
application or service resource domain, one distinct content identity incurs
one retained-payload charge; additional declaration references incur only their
bounded reference metadata. Separate domains retain separate admission,
accounting, authority, revocation, and teardown. Source observes equal immutable
values, never content-object address, mapping, interning, alias count, or storage
identity.

## Application entry and launcher binding

`authority application` classifies a module; it does not make `Main`, `Run`, or
any other source name special. A build or package plan selects one exported,
monomorphic function by canonical declaration identity and exact signature as an
entry for one named launcher profile. The selected function may be synchronous
or asynchronous only when that launcher profile admits its complete signature.

Before invocation, the launcher:

1. admits the exact package, module, entry, source edition, profile, concrete
   target descriptor, and launcher profile;
2. creates one bounded application resource domain and any owned root values
   required by the entry, including a `Foundationˉmemory.Memoryˉbudget` when the
   signature names one;
3. approves the exact transitive required-capability set and binds each
   module-bound root to one rights-limited provider with the admitted signature
   set and limit profile;
4. binds every ordinary entry argument by parameter position and exact type
   under the launcher profile; and
5. transfers the owned arguments and starts the function only after all prior
   checks succeed.

Missing, duplicate, incompatible, unauthorized, stale, oversized, or unsupported
binding rejects launch before source execution. Partial binding is never
published. Source cannot manufacture a replacement root budget, allocator,
capability root, process-argument table, environment table, or host handle from
ambient process state. The launcher profile owns conversion of the exact entry
result or terminal task outcome into process/service completion and reclaims the
application resource domain after structured teardown.

An application may export other functions and may use any ordinary source name
for its selected entry. Language 1.0 therefore needs no special entry keyword,
universal entry ABI, hidden allocator, or implicit `Main` rule.

## Type system

Language 1.0 is nominally and statically typed. Every parameter, function result,
record field, variant payload, exported value, capability reference, resource,
and public generic boundary has an explicit type. A local `let` or `var` may
infer the one exact type of its initializer.

Inference:

- performs no conversion;
- does not cross a public signature;
- does not select among overloads;
- does not guess a missing generic instance;
- does not change an ownership or effect mode; and
- rejects an initializer whose type cannot be unique.

There is no implicit `null`, truthiness, numeric conversion, enum conversion,
rune conversion, floating conversion, pointer conversion, or common-type
selection.

### Primitive types

| Type | Semantic values |
| --- | --- |
| `unit` | The single value `()`. |
| `never` | No values; control cannot return normally. |
| `bool` | Exactly `false` and `true`. |
| `i8`, `i16`, `i32`, `i64` | Signed two's-complement integers of the named width. |
| `u8`, `u16`, `u32`, `u64` | Unsigned integers of the named width. |
| `f32`, `f64` | Strict IEEE 754 binary32 and binary64 values under the profile below. |
| `rune` | One Unicode scalar value, excluding U+D800 through U+DFFF. |
| `text` | Immutable finite Unicode scalar sequence with canonical UTF-8 interchange. |
| `bytes` | Immutable finite octet sequence. |

`unit` is an ordinary Copy value and need not occupy runtime storage. `never`
has no literal or constructed value. A non-returning `never` expression may
satisfy an expected result position because it produces no value; this is not a
conversion.

Portable semantics have no pointer-sized integer. Every size, index, count,
offset, identity, and serialized field uses a fixed-width type selected by its
owning API or format. A host-memory boundary checks that value against the target
and resource domain before allocation or address conversion.

### Integer behavior

Integer arithmetic uses exact same-type operands. Addition, subtraction,
multiplication, signed negation, division, remainder, and shifts trap on an
undefined or out-of-range mathematical result. Division by zero traps. Signed
minimum divided by minus one traps. A shift count outside zero through width
minus one traps.

Unsigned bitwise operations preserve the exact named width. Signed bitwise
operations are absent from the Core operator set; a named Foundation operation
may expose exact two's-complement bit behavior.

Widening, checked narrowing, wrapping, saturating, truncating, parsing, and bit
reinterpretation are distinct named Foundation operations. Checked conversion
returns a typed result. Bit reinterpretation requires equal widths and is not a
numeric conversion or byte serialization rule.

### Floating-point behavior

`f32` and `f64` use IEEE 754-2019 binary32 and binary64 interchange values.
Core arithmetic uses roundTiesToEven, preserves subnormals, and does not contract
separate operations into fused operations. A fused multiply-add is available
only through an explicit named operation.

Every arithmetic operation with a NaN input or NaN mathematical result produces
the one canonical quiet NaN for its width: sign zero, quiet bit set, remaining
payload zero. Positive and negative infinity and signed zero retain IEEE
behavior. Ordered comparisons with NaN are false; inequality with NaN is true;
positive and negative zero compare equal. Total ordering, bitwise equality, NaN
inspection, and canonical serialization are named Foundation operations.

Integer/float and `f32`/`f64` conversions are explicit. Each operation states
rounding, range, NaN, infinity, and signed-zero behavior. Fast-math
transformations that change a result are invalid unless source calls a separately
specified approximate operation.

The numeric/graphics workload fixes one observable contraction case:
`Fusedˉmultiplyˉaddˉf32` performs one final rounding, while the ordinary source
`A * B + C` performs two roundings. A compiler, vectorizer, GPU provider, or
parallel library cannot exchange those forms, reassociate lanes, flush
subnormals, change signed zero, preserve arbitrary NaN payloads after arithmetic,
or select a host formatting rule. Exact bit observation, classification, total
ordering, and canonical text formatting use the named Foundation calls.

### Text, runes, and bytes

`text` is a sequence of Unicode scalar values. It performs no implicit
normalization, locale comparison, case conversion, collation, or grapheme
segmentation. Equality compares the exact scalar sequence. Canonical interchange
encoding is shortest-form UTF-8; malformed UTF-8 is rejected before constructing
text.

Text indexing never confuses byte offsets, rune positions, and user-perceived
graphemes. APIs use separately named and typed positions. Text iteration yields
runes in scalar order.

`bytes` is arbitrary octets. Byte order is not a property of `bytes` and must be
named by every multi-byte codec. Converting text to bytes or bytes to text uses an
explicit codec and typed failure.

### Built-in operators

Operators have fixed language meanings and never invoke a user-selected
overload:

- `bool` admits `!`, `&&`, `||`, `==`, and `!=`;
- integers admit checked arithmetic and exact same-type comparison; unsigned
  integers additionally admit bitwise and shift operators;
- `f32` and `f64` admit unary minus, `+`, `-`, `*`, `/`, and IEEE comparisons;
  remainder, fused multiply-add, total order, and approximate operations are
  named Foundation functions;
- `rune`, `text`, `bytes`, and enums admit exact same-type `==` and `!=`;
- arrays, vectors, sequences, slices, maps, and arenas admit only the fixed index
  or iteration syntax explicitly assigned by the language and Foundation
  contracts; and
- records, variants, functions, capabilities, resources, borrows, and unsafe
  values have no built-in equality unless an explicit permitted protocol
  derivation creates a named function.

`+` never concatenates text, bytes, or collections. Builders own bounded
construction. Mixed numeric operand types are rejected before evaluation.

## Nominal declarations

### Records

A record is a nominal product with uniquely named fields and declaration-order
layout within source semantics. Construction is named-only, supplies every field
exactly once, evaluates field expressions from left to right as written, and
places values into declaration order after successful evaluation.

A record update evaluates its base exactly once and then replacement expressions
from left to right. It must name each replacement once and preserves every
unreplaced field. Update cannot silently initialize a field added by a later
version.

Records have no implicit object identity, base class, virtual table, default
constructor, mutable interior, reflection metadata, equality, ordering, hashing,
formatting, copying, or serialization.

### Enums

An enum is a closed nominal scalar with uniquely named members and explicit
fixed-width integer tags. The tag type is declared. Duplicate tags are invalid.
Converting a tag to an enum validates that it names a member; serialization uses
a separately versioned format contract.

### Variants

A variant is a closed nominal sum. Each case has zero or more uniquely named
fields. Construction names the variant and case and supplies every field once.
The representation and numeric case tag are not source-observable.

Matching a closed enum or variant is exhaustive. A wildcard cannot hide a
missing closed case. An explicit discard may ignore a bound field within a named
case.

### Derived operations

An operation is derived only through an explicit declaration naming one admitted
Foundation protocol. Every contained value must implement that protocol, and the
compiler must prove a finite operation bound. Capabilities, resources, mutable
owners, mutable borrows, functions, unsafe handles, and foreign values cannot
derive general equality, ordering, hashing, copying, formatting, or
serialization.

Serialization is always a named, versioned format. It is never a reflection side
effect.

## Generics and protocols

Records, variants, functions, protocols, implementations, and immutable
collection values may have exact type and compile-time constant parameters.
Generic source is statically resolved. Language 1.0 has no class inheritance,
overlapping implementation, inferred overload set, ambient runtime type
discovery, or implicit dynamic interface object.

A protocol declares required function signatures and associated compile-time
facts but owns no mutable base state. An implementation names exactly one
protocol instance and one implementing nominal type. Selection:

- uses exact types and constants;
- considers only visible explicit implementations;
- rejects zero or multiple matches;
- is independent of import traversal order;
- cannot invoke a conversion to create a match; and
- becomes part of reproducible generic identity.

Recursive generic instantiation, total instances, compiler work, retained
evidence, and emitted-code growth have published finite limits. Exceeding a limit
is a bounded compilation diagnostic before artifact publication. A package may
declare a smaller admitted limit profile; it may not silently change semantics.

A generic function call resolves one already named declaration; arguments never
select an overload. Its type and compile-time constant parameters are inferred
only by deterministic structural matching:

1. each explicit argument receives one exact type without using the call's
   result context;
2. that type is structurally matched against the corresponding declared
   parameter type after applying its explicit borrow or by-value mode;
3. a direct occurrence of a generic type or constant parameter contributes one
   candidate exact value;
4. every repeated occurrence must contribute the same canonical value;
5. every generic parameter must have exactly one value after all arguments are
   matched; and
6. the compiler substitutes that complete solution before checking protocol
   requirements, effects, ownership, and generic admission limits.

Matching may decompose exact nominal type arguments, array or function types,
and compile-time constant arguments. It does not search protocol
implementations, insert a conversion, infer from an unsuffixed/context-dependent
literal, solve an arithmetic equation, or use an assignment target, expected
result, return statement, function body, default, or import order. A parameter
with no solution or conflicting solutions is a diagnostic at the call. Result
context never chooses or repairs a generic instance.

Edition 1 also admits `Qualifiedˉname::<Typeˉarguments>(...)`; this form is
required when an empty or otherwise evidence-free call cannot use the ordinary
rule. The qualified name resolves one generic function declaration first. The
list supplies every type and compile-time constant parameter exactly once in
declaration order; partial lists, defaults, placeholders, named generic
arguments, and inference of omitted parameters are invalid. After exact
substitution, ordinary argument, protocol, effect, ownership, and admission
checks run.

`::` distinguishes the syntax from relational `<`/`>`. A bare `Name<T>(...)`
remains invalid. Explicit generic syntax applies only to a named declaration,
not an arbitrary function value. An explicit and argument-derived call producing
the same substitution share one canonical generic identity. Result context still
never chooses or repairs a generic instance.

Dynamic protocol values remain outside edition 1.

## Values, ownership, and memory

Every type has one visible value class:

| Class | Transfer |
| --- | --- |
| Copy | Assignment, binding, capture, and by-value argument create an independent semantic copy. |
| Shared immutable | Copying may share hidden backing storage; no source operation observes backing identity or mutates it. |
| Owned | Assignment, capture, return, and by-value argument move ownership unless a named bounded clone is called. |
| Borrowed | A temporary immutable or exclusive mutable view whose lifetime is bounded by an owner. |

Primitive scalars and enums are Copy. `text`, `bytes`, and immutable published
collections are shared immutable. Mutable vectors, maps, builders, arenas,
unique buffers, and provider resource instances are owned. Slices and views are
borrowed. An aggregate adopts the strictest field behavior unless a safe explicit
derivation proves another class.

### Bindings and mutation

`let` creates a non-reassignable binding. `var` creates a reassignable binding.
Reassignment is not the same as interior mutation. Mutating owned storage
requires a mutable owner or an exclusive mutable borrow; shared immutable values
never expose mutation.

There are no mutable module globals in Core or Hosted source. System-defined
machine storage is accessed only through an explicit unsafe or capability
contract.

### Moves

Moving transfers the value and invalidates the prior binding on the successful
path. A move does not promise a bytewise copy. Use after move, double release,
moving a borrowed owner, and moving only part of an inaccessible owned aggregate
are compile-time errors.

A by-value owned argument moves into call-evaluation temporary ownership. If a
later argument propagates failure, ordinary cleanup releases that temporary. Once
the call begins, a recoverable failure does not roll ownership back implicitly;
the failure result must return the original value explicitly when the caller is
to recover it.

### Borrows

An immutable borrow permits any number of simultaneous immutable borrows and no
mutation. A mutable borrow is exclusive: while it is live, the owner and every
other borrow are inaccessible.

A borrow:

- cannot outlive its owner;
- cannot be returned unless the public lifetime-elision rule below identifies
  its one owner;
- cannot cross a task boundary or suspension point unless the structured scope
  proves the owner remains live and immobile;
- cannot close, release, resize, or move its owner; and
- ends no later than its last statically proven use inside the enclosing lexical
  scope.

Borrow checking is compile-time and must diagnose the origin, conflicting use,
and required lifetime with bounded related locations.

An expression of type `borrow T` or `borrow mut T` may satisfy an exact
by-value `T` position only when `T` is Copy or shared immutable. Evaluation
reads the borrowed value and creates its ordinary semantic copy; a shared
immutable result may retain the same hidden backing and admitted charge. This
does not move from the owner, invoke a clone, change the borrow lifetime, or
perform a numeric, Boolean, enum, protocol, or other conversion. No
corresponding read-through is available for an owned `T`.

Edition 1 has no named lifetime parameters. A public function may return a
borrowed value only when its signature has exactly one borrowed parameter; the
result lifetime and mutability are bounded by that parameter. A function with
zero or multiple borrowed parameters cannot return a borrow. A user-declared
record, variant, module value, owned collection, task, or closure that escapes its
call cannot contain a borrow. Foundation may use `Option<borrow T>`,
`Result<borrow T, E>`, and borrowed slices as ephemeral results under the same
one-owner rule. A `Slice<T>` or `Mutableˉslice<T>` parameter counts as one
borrowed parameter whose provenance is its underlying owner. A direct borrowed
element result may inherit that one lifetime; it cannot outlive the slice or
underlying owner.

### Allocation and release

Constructing a scalar or record does not imply heap allocation. An implementation
may use registers, inline storage, stack storage, arenas, or heap storage while
preserving semantics.

Allocation is recoverably fallible whenever it depends on capacity, an
allocator, a resource domain, or the host. A type or collection maximum prevents
excess; it does not promise physical availability. Allocation failure returns a
typed Foundation result and is not a catchable exception.

Owned memory is released deterministically when its owner is consumed or leaves
scope. Shared immutable storage may use reference counts, copy-on-write, arenas,
or another bounded unobservable strategy. Language 1.0 does not require tracing
garbage collection.

Recursive graphs use an owned typed arena and generation-checked non-owning
handles. Destroying the arena destroys all admitted nodes and invalidates every
handle, including cycles.

Replacing or removing an arena node is an explicit Foundation mutation through
an exclusive arena borrow. It validates the complete arena/slot/generation
before mutation, returns owned inputs/old values according to its exact result,
and cannot make a stale handle alias a successor. Collection presence or handle
equality alone never proves liveness.

Owned locals release in reverse successful acquisition order on fallthrough,
`return`, `break`, `continue`, and `try` propagation. A terminal process or
machine trap does not promise that user cleanup code ran; runtime teardown must
still reclaim the enclosing resource domain.

## Functions, calls, and closures

A function signature contains:

- exact parameter types and transfer modes;
- exact result type;
- language profile;
- capability/effect requirements;
- generic parameters and protocol requirements; and
- unsafe or foreign status where applicable.

Arguments evaluate from left to right as written. A call uses either all
positional or all named arguments. Named arguments may appear in any order, name
every parameter exactly once, and are reordered to parameter order only after
their expressions succeed. Owned argument temporaries are released in reverse
evaluation order if a later argument propagates before the call begins.

Default arguments and overload selection by inferred type are absent from
edition 1.

A function value is immutable and carries its complete signature and effect set.
A closure explicitly marks every capture as copy, move, immutable borrow, or
mutable borrow. A capture cannot silently retain a capability, mutable value,
resource, or ambient state. A borrowing closure cannot escape the captured
lifetime or cross a suspension boundary that would invalidate the borrow.

Only referenced lexical locals are captures. A required module-bound singleton
capability root is a resolved module dependency, not a lexical value, and a
qualified call through that root does not add a capture-list entry. The call's
capability identity remains in the closure's exact effect set, in the declaring
module's required-capability set, and in the application's transitive approval
closure. A capability reference, rights-reduced provider, session, or other
instance stored in a local is a lexical value and must be captured explicitly by
its ordinary copy, move, or borrow mode.

Calls, captures, and returns obey the ordinary Copy/shared/owned/borrowed rules.
Tail-call elimination is an optimization unless a named operation explicitly
guarantees bounded tail behavior.

Recursion is admitted only within a declared runtime call-depth and work budget.
The host stack size is not a source semantic limit.

## Evaluation and control flow

Operands, call arguments, named fields, and replacement fields evaluate from
left to right exactly once. Short-circuit Boolean operators
do not evaluate the skipped operand. A failed or trapping expression does not
evaluate a later expression.

An array literal is a value expression only under one exact expected
`Array<T, N>` type. It evaluates exactly `N` exact-`T` elements left to right,
including no elements for `N = 0`, and creates fixed inline semantics rather
than a dynamically allocated or growable collection. It performs no common-type
selection, conversion, elision, or repetition.

Conditions have exact type `bool`. No other type is truthy.

`if` and `match` have statement and value-producing forms. A value-producing
form:

- evaluates its selector or condition once;
- executes one reachable branch;
- requires every reachable branch to produce the same exact type and compatible
  ownership state; and
- performs no implicit common-type conversion.

Pattern matching supports enum members, variant cases with named fields, record
patterns, explicit discard, and Boolean guards. Guards evaluate after structural
matching and from left to right. Guarded cases do not remove the requirement for
an exhaustive fallback over the same named cases.

Destructuring preserves ownership. It cannot copy an owned field implicitly,
leave an owned remainder inaccessible, or bind overlapping mutable access.
Matching a borrowed aggregate never moves out of it. Copy and shared-immutable
fields bind as their ordinary semantic copies under the read-through rule;
owned fields bind only as borrows tied to the aggregate owner.

`while`, `while let`, and bounded `for` are the iteration constructs. `break`
and `continue` target the nearest enclosing loop. An ordinary `while` condition
has exact type `bool`. `while let Pattern = Expression` evaluates `Expression`
once at the start of each attempted iteration, enters the body only when the
pattern matches, and binds that iteration's pattern values within the body. A
nonmatch terminates without a failure or extra evaluation. Ownership follows the
same match rules, and a body `continue` begins the next attempted evaluation.
A `for` source exposes an exact remaining or maximum item bound. Lazy
semantically unbounded iteration is absent.

Parallel execution never changes numeric semantics. A library may process
proved-disjoint lanes concurrently only when each lane has the same operation
order and the final ordered publication is bit-identical to the sequential
oracle. Reductions, scans, or algorithms whose result depends on grouping require
a separately named order/accuracy contract; an optimizer cannot infer one from
ordinary loops.

Unreachable source after an unconditional local transfer is rejected unless a
separate diagnostic-recovery rule marks it as non-semantic input.

## Results, failure, and traps

Language 1.0 distinguishes:

1. ordinary domain values, including named states such as Missing;
2. recoverable typed failure represented by `Result<T, E>` or another explicit
   nominal variant; and
3. terminal traps for violated verified contracts, impossible checked
   arithmetic, malformed unsafe behavior, or exhausted pre-reserved invariants.

There are no catchable general exceptions. A trap cannot be converted into a
recoverable result by ordinary source.

`Option<T>` represents optional presence without null. `Result<T, E>` is the
standard two-case recoverable result. Their exact cases are owned by the
Foundation specification.

The `try` expression:

- evaluates its operand once;
- requires the exact standard `Result<T, E>` shape;
- yields `T` for the valid case;
- returns the original unchanged failure from the containing function for the
  failure case;
- requires the containing function to return the same exact error type; and
- performs ordinary lexical release before propagation.

Changing an error type requires one explicitly named adapter. Name similarity,
protocol search, or return context cannot infer it.

## Resources and `using`

A capability reference is shared authority to request an operation from one
approved provider. An owned resource is one acquired instance such as a file,
stream, transaction, process, task scope, or device session. They are different
value classes.

Owned resources are move-only and released exactly once. `using` binds one
successfully acquired resource to a lexical scope and invokes its locally
infallible release on every ordinary scope exit.

Fallible semantic completion remains explicit:

- flush, finish, commit, durable close, graceful shutdown, and protocol
  completion return typed results;
- local release invalidates the handle and returns locally retained provider
  capacity;
- `using` never reports a body as successful because an implicit completion
  failed; and
- release never silently discards a body or completion result.

A resource that cannot separate semantic completion from local release cannot
participate in automatic `using` until its exact combined-result protocol is
specified. General `defer` is absent from edition 1.

For the first accepted resource-bearing file workload, `using` remains
release-only. A failed body skips semantic finish and returns its body failure. A
successful body invokes one named finish explicitly and returns the exact finish
rejection or uncertainty when it does not complete. Release then consumes the
handle without replacing either result. A later protocol that must finish after
body failure requires an explicit named composition value retaining both
outcomes; it does not gain hidden completion or exception precedence.

Provider revocation, generation mismatch, restart, peer exit, timeout,
cancellation, rejection, partial progress, and indeterminate completion remain
distinct typed outcomes where the interface can observe them.

For Hosted provider operations, a launcher supplies one shared immutable opaque
operation context that binds a nonzero monotonic clock identity/generation,
absolute deadline, nonzero cancellation-view identity/generation, and admitted
deadline span. Application source may copy or borrow it within its proven
lifetime and pass it to explicit provider observation points. It cannot
construct the context, inspect civil time through it, extend the deadline, or
request cancellation through the value itself. At the deadline tick timeout
wins; a dispatched mutation may remain indeterminate. This value is provider
control evidence, not a capability grant, task handle, keyword, or ambient
clock.

A task scope borrows one parent context and derives one child context
with the same or earlier deadline plus a fresh scope-owned cancellation identity
and generation. The derived context is Copy only inside that lexical scope and
its joined children. It cannot escape; scope teardown invalidates its generation.
Only the scope's named cancellation operation may request cancellation. Thus
provider calls and task cancellation observe one system rather than two
uncoordinated flags.

## Capabilities and effects

A module requirement states that source may need an interface. It is not a grant.
An application or service approval admits the exact transitive requirement set,
and a launcher binds rights-limited provider references independently.

A required declaration also introduces one module-bound singleton root for
qualified operations of that interface. The root is available only after the
catalog resolves its exact signature set and the launcher binds an approved
provider. It is not a source global, local instance, closure capture, storable
authority token, or ambient lookup. Calling it requires the capability identity
in the function's effect set. An optional-only declaration introduces no
callable root.

A capability interface has:

- canonical ASCII-safe identity;
- major contract version;
- exact signature-set identity;
- platform and profile requirements;
- limit and failure contract; and
- revocation and provider-generation behavior.

The source requirement names identity and major version. The supplied canonical
capability catalog resolves that pair to one exact signature-set identity and
declared limit profile, and the compiler retains those exact values in module or
package evidence. An unknown, ambiguous, or incompatible catalog entry is a
compile-time rejection. Two different signature sets never become compatible
merely because malformed input gives them the same major number.

A shared capability reference is Copy only when its interface explicitly permits
shared calls. Acquiring a provider instance returns an owned resource. Rights
reduction requires a named provider operation; source cannot manufacture or
increase authority.

Every function and function value carries an exact effect set. Exported function
and protocol signatures state their effects, including an explicit empty
`effects()`. A local non-exported function may omit the clause only when the
compiler derives its one exact set from calls, allocations, tasks, unsafe
boundaries, and captures. The derived set cannot hide a capability. Calling
through a generic protocol or closure preserves the required effect set.

Effect identities are canonical lowercase ASCII names. Required language
identities include `memory.allocate`, `resource.acquire`,
`resource.complete`, `resource.release`, `task.cancel`, `task.spawn`,
`task.suspend`, and each
capability interface identity. Release of already owned local accounting carries
`resource.release` but is not external authority; provider-visible release also
retains that provider interface effect.

Core functions have an empty external effect set. Deterministic allocation within
an admitted Core resource budget carries `memory.allocate` but is not ambient
host authority.

## Structured concurrency

Hosted Language 1.0 uses lexical task scopes. Every task belongs to one live
scope with finite limits for:

- child count;
- runnable and completion queues;
- retained bytes;
- work units;
- call depth;
- deadlines and timers; and
- diagnostic retention.

Task creation explicitly copies, moves, or borrows captures under ordinary
closure rules. A task result and recoverable failure are typed. A scope cannot
exit while a child remains detached.

Leaving a scope follows the one policy declared by the lexical task-scope
statement: join all, request cancellation then join, or fail while retaining
control until teardown completes. The policy, join result order, and
cancellation result are deterministic and independent of scheduler interleaving.

Cancellation is a requested state observed only at specified suspension,
provider, or explicit check points. Source may request it through one named,
idempotent operation on a mutably borrowed scope. The first request closes that
scope to new spawn acceptance and marks every live child view; it never replaces
join. Cancellation is not an asynchronous exception. Timeout, task-runtime
loss/restart, child provider loss/restart, cancellation, application failure,
and trap containment are distinct typed outcomes with exact generation evidence
where a provider boundary is involved.

`async` and `await` are syntax over this task model. Hosted provider operations
that may suspend are source-level async calls and require explicit `await` plus
`task.suspend`; a host event loop cannot hide suspension beneath an apparently
synchronous call. `await` is permitted only in an asynchronous hosted function
or task body with a live scope. Suspension cannot retain an invalid borrow,
implicit capability, or unbounded continuation. A temporary exclusive argument
into one awaited provider call is valid when its owner lives in that same child
continuation and no alias can run; storing that borrow, returning it, or
capturing an outer mutable borrow into a spawned child remains rejected.

A scheduler may execute tasks sequentially or in parallel. Data-race freedom
follows from ownership, immutable sharing, and exclusive mutable borrowing rather
than from one host scheduler.

The concurrent hosted-service workload fixes task construction, derived context,
explicit cancellation request, creation-order collection, runtime/provider
failure separation, and no-replay restart behavior. The retained-GUI workload
confirms that background work copies an immutable snapshot and that only the
owning path applies a revalidated result. The numeric, package, System/FFI, and
accelerator workloads add no contradictory task requirement. A later Foundation
identity cannot add detached tasks or weaken these ownership and cancellation
rules within edition 1.

## Unsafe and foreign interfaces

Unsafe source is admitted only in the System profile. An unsafe operation is
visible both where declared and where invoked. System profile and authority do
not make a safe function implicitly unsafe.

Unsafe contracts cover raw addresses, pointer arithmetic, unverified memory,
privileged instructions, foreign calls, ABI layouts, interrupts, DMA, and other
machine-specific behavior. Every unsafe declaration states:

- platform, architecture, ABI, and extension scope;
- alignment, range, initialization, lifetime, aliasing, and mutability;
- ownership and release;
- concurrency and interrupt constraints;
- trap and foreign-unwind behavior; and
- teardown and revocation.

Portable integers do not become pointers implicitly. Raw addresses and foreign
pointers are opaque System types. Address arithmetic uses checked named
operations, and dereference requires an unsafe block whose contract proves the
access.

An FFI declaration names the external ABI, calling convention, exact symbol,
ownership, error translation, and unwind boundary. Edition 1 foreign signatures
admit exact numeric scalars and opaque Foundation foreign pointers only. They do
not pass Windvale records, variants, `bool`, `rune`, `text`, `bytes`, enums, or
collections by value. An adapter represents those values with exact integers,
pointers, lengths, and separately specified ABI layout witnesses, then validates
them before constructing safe Windvale values. No source record layout, source
name, or host default is an external ABI automatically.

The declaration's first text literal is a canonical registered ABI-contract
identity, not merely a host calling-convention nickname. Its immutable catalog
record fixes architecture, address width, calling convention, scalar
representation, byte order, pointer retention/ownership, alignment, symbol
lookup scope, error-status interpretation boundary, unwind policy, and required
target predicate. The build plan binds that exact identity and symbol. Unknown,
unsupported, mismatched, or duplicate ABI bindings reject before artifact
publication; the compiler never substitutes its current host ABI.

A non-null `Foreignˉpointer<T, Abi>` and a
`Nullableˉforeignˉpointer<T, Abi>` are distinct opaque System types. Neither is
an integer or a safe reference. Null testing, address/range arithmetic, alignment
checking, provenance/lifetime validation, pointer creation, and dereference use
named unsafe Foundation operations. A foreign pointer tied to a caller-owned
exclusive region cannot escape the borrow or be retained by a no-retain foreign
signature.

Foreign unwinding may not cross into safe Windvale frames. A foreign adapter
must translate an admitted foreign failure at the boundary or terminate through
the declared trap policy.

An adapter can recover from untrusted returned bytes, lengths, enum/Boolean
encodings, generations, and status values because it validates them before safe
publication. It cannot recover source-level safety after a callee has written
outside the admitted region, retained a forbidden pointer, violated aliasing or
the calling convention, or unwound through a no-unwind boundary. Those are
terminal ABI-contract violations under the selected containment policy, not
ordinary typed foreign failures.

Safe Core and Hosted code cannot manufacture an unsafe value, raw pointer,
foreign handle, privileged instruction, or authority token.

## Compile-time behavior

Compile-time constants, generic specialization, protocol selection, derived
operations, and format-safe values execute only bounded deterministic work.
Each compiler publishes finite step, recursion, retained-memory, output, and
diagnostic limits and rejects excess before artifact publication.

Compile-time evaluation uses ordinary Language 1.0 checked semantics. It cannot
inspect undeclared environment state, host paths, current time, entropy, network,
provider state, or compiler traversal order.

Unrestricted macros, token rewriting, arbitrary compiler plugins, build-script
execution during import, and ambient environment inspection are absent. Generated
source or data is an explicit hashed build input with provenance.

## Determinism and resource accounting

Language 1.0 defines all source-observable ordering, including:

- operand and argument evaluation;
- field construction and update;
- integer and floating results;
- text, byte, and collection iteration;
- match and cleanup;
- task result collection and cancellation observation; and
- serialization only through named formats.

External providers may return different admitted data; this does not make
language evaluation order undefined. Concurrent completion order is observable
only through an API that explicitly admits and bounds it.

Every allocation, retained collection, recursion path, task, queue, diagnostic,
provider operation, and compile-time operation is subject to a static limit,
value maximum, allocator maximum, or resource-domain budget. Exhaustion returns
the declared typed failure unless source invoked an operation whose proven
precondition reserved success; violating that precondition traps.

## Diagnostics

A deterministic diagnostic contains:

- stable diagnostic identity;
- source edition and compiler phase;
- canonical module and source span;
- expected and observed type, ownership, effect, or profile state;
- a bounded set of related declarations or move/borrow locations;
- the violated rule and admitted limit where applicable; and
- no unbounded cascade from one malformed construct.

The compiler must distinguish syntax, name, type, ownership, effect, limit,
unsafe, profile, capability, and target-support failures. Tooling must support
semantic rename, exact-name search, ownership explanation, capability closure,
source-to-WIR/WVB mapping, and source-to-machine identity inspection.

A bounded compiler diagnostic sink reserves its final admitted slot. It retains
at most maximum-minus-one ordinary diagnostics in encounter order, writes
exactly one `Diagnosticˉlimit` marker when the next issue occurs, and retains no
later issue. The selected maximum is positive; a compiler profile using this
policy admits at least two. Any retained diagnostic suppresses publication of a
successful artifact for that compile request.

## Explicitly absent from edition 1

Language 1.0 has no:

- classes or inheritance;
- implicit null, truthiness, or conversions;
- catchable general exceptions;
- operator overloading or inferred overload selection;
- ambient runtime reflection or automatic object serialization;
- tracing-GC-dependent general object graph;
- detached task or implicit background work;
- dynamic source import or runtime name lookup through `import`;
- default parameter values;
- interpolated-text syntax without an explicit bounded destination and memory
  owner;
- ambient, inferred, mixed, or runtime-selected source lexicons and public
  source vocabularies;
- semantically unbounded collection, queue, recursion, diagnostic, or
  compile-time work;
- unrestricted macro, preprocessor, or compiler plugin;
- wildcard import or ambient prelude;
- hidden capability acquisition;
- host-native path, handle, ABI, locale, encoding, or scheduler semantics in
  portable source; or
- indentation-sensitive grammar or automatic semicolon insertion.

An edition after 1 may reconsider a constrained form only through a named
decision and a complete safety, determinism, authority, bound, compatibility,
and implementation contract.

## Source-freeze requirements

Source freeze accepts the edition-1 source contract and exact candidate
identities; it does not claim that the current compiler implements them. In this
section, a paper case "passes" when its accepted input, expected behavior,
failure ordering, and bounds are complete and mutually consistent. Those cases
become executable conformance fixtures during migration. Implementation,
cross-host, performance, editor, and formatter qualification remains a later
gate and cannot be claimed by the source-freeze decision.

This candidate becomes frozen Language 1.0 only after:

1. the grammar companion has no unresolved production or precedence;
2. the Foundation companion has exact signatures and failure behavior;
3. the paper corpus completes all eleven workloads and their rejected cases;
4. collection, ownership, cleanup, and concurrency freeze conditions pass;
5. every rule has accepted, boundary, malformed, and rejected examples;
6. the migration and compiler responsibility matrix is approved;
7. editor and formatter behavior is specified;
8. target support and cross-host evidence requirements are named;
9. package-data binding, accounting, malformed-input, and non-duplicating
   shipment evidence passes;
10. the local AI accelerator workload separates any general source-language gap
    from library, target-extension, verified-representation, and provider work;
11. the accepted argument-derived generic, capability-root, Foundation-call,
    launcher-entry, and target-scope cases remain coherent across all mandatory
    workloads; and
12. the accepted command sequence, strict parsing, reserved-builder,
    standard-stream authority, and launcher-status cases remain coherent across
    all mandatory workloads; and
13. the accepted file-copy byte-buffer, resource-completion, known-partial,
    filesystem-authority, and cancellation/lifecycle cases remain coherent
    across all mandatory workloads; and
14. the accepted runtime-arena, first-item construction, two-step checked
    observation, explicit-schema, commit, and fresh-recovery cases remain
    coherent across all mandatory workloads; and
15. the accepted explicit-generic, Copy-read, rank-borrow, immutable-arena,
    source-position, diagnostic-saturation, exact-byte, and phase-publication
    cases remain coherent across all mandatory workloads; and
16. the accepted checked-slice, strict-decoding, byte-decimal, opaque-context,
    exact-stream-progress, and asynchronous-endpoint cases remain coherent
    across all mandatory workloads; and
17. the accepted task construction, derived-context, cancellation, child-result,
    task/provider failure separation, suspension, and no-replay cases remain
    coherent across all mandatory workloads; and
18. the accepted arena replacement/removal, closed-event, Core/Hosted,
    parent-only application, stable-tombstone, and exact-frame-publication cases
    remain coherent across all mandatory workloads; and
19. the accepted contextual-array, checked-mutable-slice, strict-float,
    policy-bearing-conversion, canonical-formatting, and bit-identical-parallel
    cases remain coherent across all mandatory workloads; and
20. the owner-reviewed source-descriptor/profile, source-lexicon,
    public-library source-vocabulary, Unicode identifier, conversion, editor,
    malformed/security, cross-host, and bounded-performance paper contracts
    satisfy the localized-source addendum, with executable and measured
    qualification assigned to implementation; and
21. a source-freeze decision records the replacement canonical document
    identities.

Until then, examples in this suite are candidate edition-1 source and are not
accepted by current tools.
