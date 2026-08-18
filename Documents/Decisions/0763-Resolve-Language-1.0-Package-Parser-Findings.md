# Decision 0763: Resolve the Language 1.0 package-parser findings

## Status

Accepted by the project owner on 2026-08-17 under the instruction to integrate
all recommended correctness/completeness findings needed for a correct Language
1.0. This decision refines
[Decision 0751](0751-Accept-Windvale-Language-1.0-Direction.md),
[Decision 0752](0752-Complete-Language-1.0-Collection-And-Package-Data-Boundaries.md),
and the normative-candidate language and Foundation companions.

It accepts all six findings from workload 9. It does not freeze edition 1,
implement a package parser or package format, select a product dependency
solver, or introduce reflection serialization.

## Context

The ninth mandatory workload parses exact manifest/lock text, constructs
ordered dependency collections, validates a graph, publishes a canonical
dependency-first order, and renders a byte-exact report. Its lock deliberately
shuffles package and dependency input so insertion-ordered, randomized, or
host-map output fails visibly. Two package-data declarations also bind one
identical notice payload so the package and accounting nonduplication rule is
measurable.

The candidate already chose raw/multiline/byte/text literals, strict parsing,
bounded builders, ordered maps/sets, unique structural generic resolution, and
typed package data. Complete source exposed that map publication and set calls
were still prose promises rather than exact Foundation signatures, and that
duplicate-content accounting needed one precise domain rule.

## Decision

### Complete deterministic map publication and mutation

Accept ownership-preserving map replacement and removal, consuming immutable
publication, and exact immutable length/contains/rank/key/value observation.
Replacement accepts the new owner only on success; absence or rejection returns
it. Removal returns the stored owned key/value. Freeze consumes the mutable map,
does not allocate or fail, and retains canonical rank/accounting.

### Fix the complete ordered-set surface

Accept exact construction, first-item construction, insertion, length,
membership, rank, borrow-at, removal, consuming freeze, and immutable
observation calls. Duplicate/capacity/comparison/allocation insertion rejection
returns the original owned value and leaves the set unchanged. Removal returns
the stored value; absence changes nothing.

Map and set layout remains unobservable. Do not substitute host hashing,
randomized iteration, insertion order, a set literal, or compiler-only magic.

### Make Ordering equality and resolution testable

Collection uniqueness is exactly the one resolved
`Ordering<T>.Compare == Equal` relation. Implementations must prove totality,
determinism, compatibility with value equality, and their finite comparison
bound. An equality conflict rejects; no collection silently retains an
arbitrary representative.

Generic/protocol selection remains unique and argument-derived. Result context,
import order, declaration order, or runtime values cannot select another
ordering.

### Retain explicit bounded parsing instead of reflection

The existing text byte/rune observations, checked shared ranges, whole numeric
parser, variants, loops, and builders express the complete workload. Keep exact
format magic/version and handwritten parsing/serialization. Do not add regex
syntax, preprocessors, automatic schemas, reflection, ambient locale, or hidden
split allocations to edition 1.

Oversized complete input or a future declared length is rejected before
expensive allocation/read. Every enclosing range uses checked arithmetic before
publication.

### Define duplicate-content shipment and accounting

Every package-data declaration binding is independently validated for canonical
declaration/resource identity, type, maximum, exact length, digest, and strict
UTF-8 where applicable. Canonical packaging stores one object per distinct
content identity and may reference it from multiple declarations.

Within one application or service resource domain, one admitted distinct
content identity incurs one payload-retention charge; additional declaration
aliases incur only bounded reference metadata. Separate domains preserve their
own admission, accounting, authority, revocation, and teardown. Source observes
immutable values only, never mapping, address, interning, alias count, or storage
identity.

### Use ordered-rank topology as the correctness oracle

Accept the workload's repeated ascending-rank scan as the simple reference
algorithm. It publishes dependency-first order with lexical ties. A no-progress
pass terminates with at most the admitted package count of remaining identities
in canonical order. Faster algorithms must preserve identical order,
diagnostics, work/memory bounds, and differential evidence.

## Consequences

The package-parser bundle becomes draft reviewed. Ten of eleven workloads are
now draft reviewed; only the System/FFI boundary remains.

Foundation gains exact completion/publication/immutable-observation calls for
maps and the complete ordered-set call family. No grammar form, capability,
unsafe rule, WIR operation, reflection system, or package-product format is
added.

The exact reference has four declaration bindings, three content objects, 227
unique content bytes, four packages, four dependency edges, four topology
passes, and one 160-byte report with SHA-256
`a9df168004784b0b1af30bb2c563d9ae166bd3a38dceb388b731b8d72dcba2b7`.

## Reconsideration triggers

Reconsider the ordered representation only when measured real workloads cannot
meet published bounds. Any alternative must retain deterministic ranks,
ownership outcomes, comparison law evidence, and the simple oracle.

Reconsider package-data accounting only with a resource-domain model that
prevents both double charging and authority laundering while keeping storage
identity unobservable.

Reconsider parser-library scope only after at least two implemented bounded
formats demonstrate the same reusable cursor/error contract. Do not enlarge
source syntax merely to shorten this paper parser.
