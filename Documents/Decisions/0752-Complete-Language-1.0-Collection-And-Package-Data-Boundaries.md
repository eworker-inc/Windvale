# Decision 0752: Complete the Language 1.0 collection and package-data boundaries

## Status

Accepted by the project owner on 2026-08-17. This decision refines
[Decision 0751](0751-Accept-Windvale-Language-1.0-Direction.md) and the
normative-candidate Language 1.0 suite. It does not freeze source edition 1,
change Windvale Seed, change WVB, or claim implementation on any target.

## Context

The Language 1.0 specification review found two useful boundaries that were not
yet closed. The accepted collection family contained a deterministic map but no
standard set, while earlier language planning proposed package-backed immutable
data without assigning it final source semantics. The same review confirmed that
dynamic loading, default arguments, and broader Unicode identifiers were already
deliberately absent from edition 1 rather than accidentally omitted.

A set does not require a new language primitive. It can share the map's ordering,
budget, ownership, and immutable-publication model while giving membership data a
clear nominal API instead of exposing a dummy map value.

Package data needs a source declaration because schemas, templates, icons,
models, and other immutable application inputs must be bindable without an
ambient native path or filesystem capability. Calling that declaration
`resource` would conflict with Language 1.0's move-only, locally released
resource instances such as files, streams, transactions, and task scopes.

## Decision

Add `Set<T>` and `Immutableˉset<T>` to the required
`Foundationˉcollections` contract. The standard set:

- is move-owned while mutable and shared immutable after consuming publication;
- requires the same total `Ordering<T>` protocol as the standard map;
- records explicit maximum items, retained bytes, comparison work, and
  diagnostic work;
- iterates in ascending canonical value order;
- reports inserted, already-present, removed, absent, capacity, and allocation
  outcomes without losing ownership; and
- does not expose whether an implementation internally reuses a map, tree, or
  another bounded ordered representation.

Add one package-backed immutable declaration to edition 1:

```text
export package data Schema: bytes maximum 1_048_576u64;
export package data Template: text maximum 65_536u64;
```

The declaration admits only `bytes` or `text`. Its required `maximum` is a
canonical `u64` byte maximum. For `text`, the maximum counts strict-UTF-8 encoded
bytes; no normalization is implicit.

One build or package plan binds the declaration's canonical identity to one
canonical package-resource identity, exact content digest, exact byte length,
and declared type. Missing, duplicate, oversized, digest-mismatched, invalid-text,
or incompatible bindings reject construction before publication. The resulting
source value is shared immutable module data. It exposes no path, handle,
provider identity, mutable storage, or capability and performs no runtime lookup
by source name.

Canonical packaging stores one content object per distinct content identity and
allows declaration records to reference it. A compiler, publisher, or installer
must not require duplicate shipped payload bytes merely because more than one
module declaration binds the same admitted content. Exact WVB, package-table,
mapping, or loading representation remains owned by separately versioned format
contracts and must preserve the same observable source semantics and accounting.

Retain static source imports for edition 1. Dynamic verified-module loading is a
future Hosted library/runtime capability, not an `import` expression and not an
extension of static name lookup.

Retain the absence of default arguments. Named arguments, configuration records,
and explicit wrapper functions provide the edition-1 alternatives without
version-dependent hidden call behavior.

Retain case-sensitive ASCII identifier segments joined by U+02C9. Full Unicode
remains available in source text, comments, runes, and text values. A future
identifier edition requires exact normalization, character classes, confusable
handling, diagnostics, tooling, and migration.

## Consequences

Language 1.0 gains an ordinary bounded set without expanding core syntax or
adding an accelerator, host, or serialization assumption. Set operations remain
generic Foundation calls and require no special WIR or WVB operation unless later
measurement proves one justified.

Package data gives code and immutable shipped content one typed, deterministic
connection without granting filesystem authority. The package/build plan—not a
host path in source—owns resolution. Structured content still requires an
explicit named versioned parser; `package data` does not add reflection or
automatic deserialization.

Package-data bytes count against the selected application or service resource
domain even when an implementation maps or shares immutable storage. A target
that cannot admit the declared maximum reports that limitation before execution.

The paper corpus must exercise duplicate-content shipment, missing and malformed
bindings, maximum-size admission, strict text validation, imported package data,
and package-data use without a filesystem grant. Source freeze also requires the
exact Foundation set signatures and immutable publication outcomes.

## Reconsideration triggers

Reconsider the package-data spelling if complete paper programs cannot
distinguish it clearly from ordinary constant data or owned resource instances.
Reconsider its admitted types only when a real consumer proves that explicit
versioned parsing of `bytes` or `text` cannot express a bounded portable input.

Reconsider the standard set representation only when measured workloads cannot
meet their published bounds through the accepted total-order contract. Do not
replace it with host hashing or randomized iteration merely for convenience.

