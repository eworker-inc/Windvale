# Windvale Language 1.0 localization workloads

## Status

This is the mandatory paper-evidence plan for the reopened Language 1.0
localized-source boundary. It complements the eleven accepted application and
system workloads in the
[Language 1.0 paper corpus](Windvale-Language-1.0-Paper-Corpus.md). It does not
claim implementation or authorize compiler changes before replacement source
freeze.

The workloads test the
[localized-source specification](../../Specifications/Windvale-Language-1.0-Localized-Source.md)
and exact
[source-profile artifact formats](../../Specifications/Windvale-Language-1.0-Source-Profile-Formats.md).

## Required sequence

### Workload 1: source-profile admission

Define candidate byte formats, Unicode inputs, content hashes, dependency
binding, malformed-input behavior, cache publication, and measured performance
requirements. Exercise canonical English and a clearly synthetic Unicode profile
without claiming translation quality.

The complete paper bundle is in
[localization workload 1](Language-1.0-Localization-Workloads/01-Source-Profile-Admission/README.md).

### Workload 2: native-reviewed Chinese source

Create a native-reviewed `zh-Hans@1` keyword lexicon and vocabulary profile,
localize one complete Foundation interface, and store one application body fully
in Chinese. Prove exact canonical token, declaration, and artifact equivalence
against `en@1`.

### Workload 3: conversion and source tooling

Exercise deterministic profile conversion, formatter preservation, rename,
copy/paste modes, canonical reveal, diagnostics, source maps, and stale-catalog
handling. Conversion must preserve project-owned identifiers and non-source
prose unless separately requested.

### Workload 4: Unicode and multilingual security

Exercise Latin, Cyrillic, Greek, Han, Hiragana, Katakana, Hangul, Arabic, and
Hebrew valid cases plus normalization, mixed-script, mixed-number, confusable,
right-to-left, invisible, and boundary attacks. Native-language review determines
whether the edition-1 join-control exclusion needs reconsideration.

### Workload 5: shipment, cache, and cross-host qualification

Exercise minimal English installation, optional language installation,
content-addressed deduplication, offline resolution, update/rollback, Windows and
Linux equality, compiler-service cache generations, and cold/warm time and memory
ceilings.

## Completion gate

After all five workloads are owner reviewed, reconcile the semantic and grammar
specifications, Foundation registry, paper source, migration plan, editor
contract, roadmap, and progress record. Then generate a replacement exact
manifest and request the Language 1.0 source-freeze decision. Implementation
begins only after that decision.
