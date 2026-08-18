# Compiler-service localization cache generations

## Product objective

One unchanged localization object should not be hashed, parsed, normalized, and
indexed again for every module in a build. Caching is an optimization over exact
inputs; it cannot change admission, diagnostics, or output.

## Three cache layers

### Content validation

Key: `(artifact format, exact SHA-256)`.

One immutable entry retains admitted format/version/size metadata and the
bounded parsed representation. Shared Unicode/token artifacts have one entry
even when both `en@1` and `zh-Hans@1` reference them.

### Composite profile and catalog set

Key: composite-profile manifest hash plus the ordered exact interface/catalog/
override hashes selected for one build closure.

The entry retains immutable keyword/public-label lookup state and canonical
identity mappings. It contains no raw module spans, diagnostics, source maps,
workspace paths, host locale, or presentation state.

### Request/module front door

Key: raw source hash, compiler/source edition, composite profile and every
component/catalog/interface hash, dependency graph, options, and target.

Only canonical semantic evidence eligible under the localized-source
specification may be reused. Raw spans, diagnostics, conversion maps, and debug
provenance are regenerated or remain request-owned.

## Private validation and single-flight publication

For a missing content key, one generation-scoped single-flight operation:

1. enforces the declared artifact/request limits before allocation;
2. hashes exact bytes once;
3. parses and validates privately in one forward pass;
4. builds the immutable lookup representation;
5. publishes it atomically only after complete success; and
6. wakes waiters with that immutable entry or the bounded failure result for
   their current request.

Concurrent same-hash candidates do not leave duplicate published tables or
temporary files. If an implementation permits parallel candidates instead of
single-flight, losing private state is released immediately. Failure is not a
durable negative cache: a later request may supply repaired exact bytes.

## Service generations

One compiler-service generation binds:

- compiler/tool identity and source edition;
- exact Unicode/table implementation identity;
- installed/available immutable package-generation snapshot;
- cache schema version and configured count/byte budgets; and
- the immutable cache maps published during its life.

A package/compiler/profile update creates a new service generation. An in-flight
request pins its starting generation. Unchanged content objects may be shared
across generations only by exact key and immutable ownership; no entry is
reinterpreted under new compiler/table code. A retiring generation releases its
private maps when the last request/reference leaves and reports remaining bytes.

Cache count and retained-byte budgets are explicit configuration. Admission can
always run request-private without publishing a cache entry, so cache pressure
does not make otherwise valid source semantically invalid. An entry is evicted
only while unpinned. Request-private admission still obeys the format/module
input bounds and fails normally if those semantic bounds are exceeded.

## Store trust and repeated hashing

The safe first implementation treats an ordinary filesystem path or mtime as no
evidence. It hashes each distinct localization object once on first use in the
service generation, even if the package installer previously verified it.

A later trusted package-store capability may return an immutable read handle and
verifiable publication identity that lets a compiler generation reuse store
admission evidence. That optimization needs an explicit trust/lifetime contract
and corruption tests; a sidecar “already hashed” file is insufficient.

The first implementation deliberately does not add a persistent cross-process
semantic cache. Independent agent/compiler processes may each perform one small
cold validation, while the immutable store and host page cache avoid duplicate
storage/I/O. Measurements must show a material bottleneck before Windvale accepts
the corruption, locking, schema, and stale-state risk of cross-process cache
metadata.

## Multi-agent development implication

Independent worktrees with one long-lived compiler service per worker stop
rehashing packs per module and per verification phase. A deliberately shared
service can reduce first-use work further only when requests pin exact compiler,
package generation, workspace inputs, budgets, and diagnostics separately.

The current reference set is only 12,288 unique semantic bytes for both profiles.
It is not the source of 64 MiB native-product hashing or large installer sizes.
Optimization work should keep those larger product/checkpoint hashes under their
existing target-aware verification/cache owners rather than attributing them to
language localization.
