# Decision 0179: Language, application, and capability-metadata direction

- Date: 2026-08-03
- Status: Accepted architecture direction; syntax, metadata encodings, and broader value families remain incremental
- Refines: [Decision 0137](0137-Bounded-Owned-Values-Before-Dynamic-Collections.md) and [Decision 0140](0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md)
- Retains: canonical WVB, immutable-by-default source, per-part portability, explicit capability authorization, and the current Seed naming boundary

## Context

Windvale needs a recognizable programming model without introducing novelty for its own sake. It must support ordinary applications and low-level system components while preserving deterministic builds, bounded execution, explicit authority, and one semantic model across Windows, Linux, Windvale OS, and selected browser profiles.

The phrase “code and data together” also needs a bounded meaning. Without one, it could imply anything from ordinary embedded resources to self-modifying programs or a language-wide database model.

## Decision

### Define Windvale by semantic properties rather than unusual syntax

Windvale is a deterministic, capability-oriented language for applications and systems. Its distinctive combination is:

- immutable-by-default values and declarations;
- checked, explicit-width arithmetic;
- bounded allocation and execution where a selected profile requires it;
- typed recoverable operational failure and distinct contract traps;
- explicit ownership and publication of dynamic values;
- explicit platform scope, authority, and capabilities;
- canonical verified bytecode as the distribution identity; and
- shared semantics across interpretation, JIT, install-time compilation, AOT, and Windvale OS execution.

Approachability comes from small orthogonal features, ordinary modules and functions, strong diagnostics, and libraries that hide provider transport without hiding authority. Windvale does not seek distinctiveness through irregular syntax, implicit host behavior, or multiple competing object and runtime models.

### Give “code and data together” one narrow product meaning

A Windvale package can bind canonical code to typed immutable data, package resources, manifests, identities, and declared capability requirements. Those inputs are hashed, validated, versioned, and compiled or packaged together. Generated typed access can expose a resource without converting it into an ambient native path.

This does not make self-modifying code, executable data, mutable databases, or arbitrary serialization part of the source-language model. Mutable application storage remains a capability-backed resource with an independent lifecycle and failure contract.

### Grow values and control in a dependency order

1. Qualify the existing checked `i64` and `u64` candidate across the intended interpreter, verifier, and serialization paths.
2. Add typed recoverable result values and propagation before broad operational APIs.
3. Add one bounded typed sequence plus a uniquely owned bounded builder; freezing publishes an immutable sequence and invalidates mutation through the builder.
4. Add deterministic maps and sets only after ordering, hashing, collision, and capacity rules are exact.
5. Add floating point only for a measured consumer and with explicit width, IEEE behavior, conversion, NaN, comparison, formatting, and reproducibility rules.
6. Add concurrency after ownership and OS scheduling evidence can support structured tasks, channels, cancellation, and failure propagation. Shared mutable state is not the default concurrency model.

Do not introduce implicit null, unbounded integers, ambient exceptions, or host-defined scalar behavior as shortcuts.

### Separate four metadata dimensions

Source, package, and module evidence will separately encode:

- platform scope, including environment, architecture, ABI, and named extensions when required;
- authority level;
- required semantic capability interfaces; and
- optional semantic capability interfaces.

Capability requirements carry canonical ASCII-safe interface identities, major contract versions, exact signatures, limits, and failure behavior. Metadata is canonical, deterministically ordered, and preserved in derived native or AOT containers. The root application or package approval remains separate from provider grants and runtime bindings.

The current `portable`, `hosted`, and `system` profile bytes remain implemented compatibility boundaries until one real platform-scoped library and one optional-capability consumer justify a versioned source and WVB encoding. A profile value must not become an overloaded replacement for the four dimensions.

### Retain the current identifier boundary

Official Windvale source retains ASCII identifier segments joined by U+02C9 through the first stable language and initial self-hosting path. Full Unicode remains available in strict-UTF-8 source text, comments, and `text` values.

A future optional identifier revision requires NFC normalization, defined Unicode identifier classes, preservation of original spelling for diagnostics, confusable-character analysis, editor and formatter support, and collision tests. No visually similar character becomes an alias for U+02C9.

## Consequences

Windvale gains a concise identity grounded in behavior that already shapes the stack. Applications can remain approachable while system components retain explicit bounds and authority.

Packages can carry typed immutable assets without conflating package resources, filesystems, or mutable storage. Platform-specific parts remain honest, and a reusable library can require a semantic capability without acquiring it.

The feature order prioritizes failure, ownership, and bounded collections before floating point or concurrency. This delays some familiar language conveniences but avoids making Stage 0 host behavior permanent.

No new scalar, result, collection, metadata field, package format, concurrency primitive, or Unicode identifier is implemented by this decision.

## Reconsider when

- a representative application cannot be expressed clearly without a different core abstraction;
- metadata separation makes deterministic composition or preflight impractical;
- a real scientific, graphics, media, or ML consumer requires floating point earlier;
- structured concurrency cannot map coherently across the selected hosts; or
- international source authoring supplies enough tooling and security evidence to justify broader identifiers.
