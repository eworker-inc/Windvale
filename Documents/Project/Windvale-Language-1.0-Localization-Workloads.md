# Windvale Language 1.0 localization workloads

## Status

This is the mandatory paper-evidence plan for the reopened Language 1.0
localized-source boundary. It complements the eleven accepted application and
system workloads in the
[Language 1.0 paper corpus](Windvale-Language-1.0-Paper-Corpus.md). It does not
claim implementation or authorize compiler changes before replacement source
freeze. [Decision 0766](../Decisions/0766-Complete-Language-1.0-Localized-Source-Reconciliation.md)
accepts all five workloads' reconciled paper/design findings.

The working Release 1 shipment target is two official source profiles:
`en@1` and `zh-Hans@1`. That target does not promote a draft pack; both profiles
must satisfy their review and qualification gates. Other scripts remain required
security/conformance evidence and additional language packs may ship later as
independently versioned data without changing Language 1.0 semantics.

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

The complete first-author draft is in
[localization workload 2](Language-1.0-Localization-Workloads/02-Simplified-Chinese-Source/README.md).
Its artifact and paper equivalence checks pass and the project owner accepts its
technical/design findings; native terminology review and executable equivalence
remain open.

### Workload 3: conversion and source tooling

Exercise deterministic profile conversion, formatter preservation, rename,
copy/paste modes, canonical reveal, diagnostics, source maps, and stale-catalog
handling. Conversion must preserve project-owned identifiers and non-source
prose unless separately requested.

The accepted first-author paper bundle is in
[localization workload 3](Language-1.0-Localization-Workloads/03-Conversion-And-Source-Tooling/README.md).
It fixes the product/tooling behavior and expected source bytes; implementation,
interactive editor, and cross-host qualification remain open.

### Workload 4: Unicode and multilingual security

Exercise Latin, Cyrillic, Greek, Han, Hiragana, Katakana, Hangul, Arabic, and
Hebrew valid cases plus normalization, mixed-script, mixed-number, confusable,
right-to-left, invisible, and boundary attacks. The edition-1 workload keeps join
controls excluded; any later relaxation requires native-language evidence and a
new exact Unicode-profile decision.

The owner-accepted first-author paper bundle is in
[localization workload 4](Language-1.0-Localization-Workloads/04-Unicode-And-Multilingual-Security/README.md).
The exact Unicode 17 data validates the required scripts; join controls remain
excluded from edition-1 identifiers, and compiler/editor/cross-host execution
remains open.

### Workload 5: shipment, cache, and cross-host qualification

Exercise minimal English installation, optional language installation,
content-addressed deduplication, offline resolution, update/rollback, Windows and
Linux equality, compiler-service cache generations, and cold/warm time and memory
ceilings.

The owner-accepted paper bundle is in
[localization workload 5](Language-1.0-Localization-Workloads/05-Shipment-Cache-And-Cross-Host/README.md).
It fixes the installer-selection, immutable-generation, exact-content
deduplication, compiler-service cache, cross-host comparison, and measurement
contracts. The current two-profile fixture contains 12,288 unique semantic
bytes. Package/installer implementation and measured Windows/Linux
qualification remain open.

## Completion gate

All five workloads now have owner-reviewed paper findings. Reconcile the
semantic and grammar specifications, Foundation registry, paper source,
migration plan, editor contract, roadmap, and progress record. Then generate a
replacement exact manifest and request the Language 1.0 source-freeze decision.
Implementation begins only after that decision.
