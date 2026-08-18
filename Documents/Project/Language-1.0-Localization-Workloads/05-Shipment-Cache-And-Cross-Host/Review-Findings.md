# Localization workload 5 findings

## Status

The project owner accepts these shipment, cache, cross-host, and measurement
decisions for the replacement Language 1.0 candidate. Package/compiler/editor
implementation and measured qualification remain open.

## Finding 1: localization reuses the package system

Source profiles are bounded immutable data. Existing bundle, store, generation,
activation, rollback, repair, and GC contracts already provide their lifecycle.
A localization-specific installer/store would duplicate risky infrastructure.

## Finding 2: runtime, source, diagnostics, and documentation are separate

Runtime-only products need no localization development bytes. Source-semantic
packs are exact compiler inputs. Diagnostic prose and documentation can be added
or removed without changing builds. This keeps installations honest and small.

## Finding 3: English is the minimal developer base; Chinese is optional

`en@1` provides the canonical Release 1 development/recovery path. The installer
adds `zh-Hans@1` explicitly after it qualifies. Host locale may improve the UI but
never chooses source semantics.

## Finding 4: content hashes eliminate installed duplication

Shared Unicode/token objects and identical catalogs occupy the immutable store
once. Package/generation references remain distinct without retaining duplicate
bytes. Release graphs should also place shared inputs in one dependency selection
to avoid repeated transfer.

## Finding 5: the current semantic data is small

Both exact fixture profiles total 12,288 unique bytes. Full Foundation catalogs,
diagnostics, docs, fonts, and generated compiler tables add bytes and must be
measured honestly, but localization is not the explanation for 500 MiB tool
shipments. Native products, debug/provenance data, and duplicated targets deserve
separate size work.

## Finding 6: install availability never replaces explicit build locking

The descriptor and project/source-input lock choose exact profile/catalog hashes.
Installation merely supplies objects. No filesystem search, newest-version rule,
locale fallback, or compiler network access enters semantics.

## Finding 7: updates are new versions and generations

A corrected term creates new pack/catalog bytes and an explicit profile version.
Existing source stays reproducible. Installation generation switching isolates
in-flight builds and preserves rollback without mutating objects.

## Finding 8: cache each distinct object once per service generation

Content, composite-profile, and request layers have different keys/ownership.
Single-flight immutable publication removes repeated per-module hashing while
keeping raw spans and diagnostics request-owned.

## Finding 9: do not add a persistent cross-process cache yet

The reference semantic bytes are tiny. Per-process first-use hashing is a simple
correctness oracle; the immutable store/page cache already avoids duplicate
storage/disk work. Only measurements can justify a trusted store-attestation or
cross-process cache protocol.

## Finding 10: cross-host equality is semantic, not path/target identity

Portable pack bytes, decisions, canonical projections, and WVB must match.
Windows/Linux native bundles and target-bearing installation generations may
differ. Reports compare the right boundary rather than hiding or forbidding
legitimate host differences.

## Finding 11: source freeze cannot require an unimplemented benchmark

The replacement freeze accepts exact format/input/algorithm/resource bounds and
the measurement protocol. The first Language 1.0 implementation establishes
reviewed host ceilings; release qualification enforces them. This preserves the
chosen design-then-implement sequence without abandoning performance as a
product requirement.

## Finding 12: verification benefits come from avoiding repeated work

Warm services must read/hash/parse zero unchanged pack bytes, and verification
must reuse its passing evidence when relevant inputs stay unchanged. Large native
product hashes remain under target-aware build/checkpoint and verification owners;
localization caches should not become another broad verification gate.

## Disposition

All five localization workloads now have owner-accepted paper findings. Proceed
to cross-document reconciliation and the replacement Language 1.0 source-freeze
candidate, retaining native Chinese terminology review and all implementation/
measured/cross-host cases as explicitly open qualification work.
