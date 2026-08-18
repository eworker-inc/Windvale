# Language 1.0 localization workload 3: conversion and source tooling

## Status

Complete first-author paper bundle for deterministic source-profile conversion,
formatting, editor operations, diagnostics, and source provenance. The project
owner accepts the technical and design decisions in this bundle. It is not a
compiler or editor implementation and does not claim that executable or
interactive qualification has run.

## Result first

Windvale can support stored localized source without making editing or
conversion probabilistic:

- conversion replaces only admitted keyword spans and resolved imported-public-
  label spans;
- all other source bytes, including project-owned Unicode identifiers, comments,
  strings, documentation, whitespace, and line endings, remain exact;
- the source and target profiles, catalogs, Unicode tables, and interface hashes
  are locked inputs rather than locale guesses or downloads;
- the converted file is fully admitted under the target profile and compared
  with the input's canonical token/declaration projection before publication;
- the normal Copy command copies stored source; canonical and displayed forms
  remain explicit alternatives;
- formatting preserves the selected profile and writes only its primary
  spellings;
- rename operates on resolved semantic identity, never on translated text that
  merely looks similar; and
- stale or incomplete source/target catalogs fail before an output file is
  published.

These decisions keep one semantic language and make source localization visible,
reproducible, and reversible.

## Bundle contents

| Item | Purpose |
| --- | --- |
| [Conversion contract](Conversion-Contract.md) | Exact admission, span replacement, validation, reporting, and transactional publication behavior. |
| [Editor and formatter contract](Editor-And-Formatter-Contract.md) | Copy/paste, canonical reveal, formatter, rename, search, IME, Git, and accessibility behavior. |
| [Diagnostics and provenance](Diagnostics-And-Provenance.md) | Stable diagnostic fields, source spans, conversion maps, debug provenance, and bounded failure reporting. |
| [Accepted cases](Accepted-Cases.md) | Required success and preservation cases. |
| [Rejected cases](Rejected-Cases.md) | Stale, ambiguous, unsafe, hidden-rewrite, and publication failures. |
| [Review findings](Review-Findings.md) | Accepted design conclusions and implementation evidence still required. |
| [`Source/`](Source/) | Exact expected English, Simplified-Chinese, and synthetic-profile conversion fixtures plus byte counts and SHA-256 values. |

## Fixture boundary

The stored Simplified-Chinese input is copied from Workload 2. The expected
`en@1` output uses the same Chinese project-owned module, function, parameter,
and alias names. The expected `test-Unicode@1` output proves conversion between
two non-identical localized profiles without claiming that the synthetic terms
are a human translation.

The fixtures are paper oracles. A future converter must generate their bytes
from the locked Workload 1 and Workload 2 profile/catalog artifacts; a script
that merely copies these expected files is not evidence.

The three fixture files total 1,095 bytes. `Expected-En.wv` and
`Expected-Zh-Hans.wv` are byte-identical to the paired Workload 2 files. Exact
individual byte counts and SHA-256 values are in
[source fixture hashes](Source/Source-Hashes.md).

## Workload disposition

The structural contract and paper cases are accepted for the replacement
Language 1.0 candidate. The implementation gate remains open until the compiler
front door, source converter, formatter/editor integration, and cross-host tests
exercise the cases in this bundle. Native-language review remains attached to
the language pack being used; conversion itself needs no translation judgment.
