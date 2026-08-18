# Localization workload 3 findings

## Status

The project owner accepts these technical and design findings for the
replacement Language 1.0 candidate. Implementation and interactive/cross-host
qualification remain open and must not be inferred from this paper review.

## Finding 1: conversion is identity replacement, not translation

Keywords and imported public labels already have canonical identities. Exact
profile conversion can select their target primary spellings without AI. Project
names and prose do not have that identity mapping and must remain untouched.

## Finding 2: ordinary Copy returns stored source

Stored bytes are the build, Git, issue-report, and recovery identity. Making them
the default clipboard result is predictable across rich and plain-text tools.
Canonical and displayed copy remain valuable, but must be explicit operations.

## Finding 3: a formatter cannot change source language

Formatting and profile conversion are different user intents. The formatter
retains the descriptor/profile and deterministic primary spellings; a converter
performs the explicit language rewrite.

## Finding 4: rename follows semantic ownership

A Chinese label for an English library member is not a second declaration.
Consumer rename cannot mutate it. Project rename changes the project's own
canonical identity; library-label changes belong to catalog authors and create
new reviewed catalog hashes.

## Finding 5: conversion must be transactional

Source and target admission plus canonical equivalence finish before publication.
The safe default writes a distinct path. Explicit in-place conversion uses an
input-hash conflict check and atomic same-filesystem replacement or refuses the
operation.

## Finding 6: spans belong to stored bytes

Diagnostics, breakpoints, and Git patches refer to stored source. Display views
may have different byte/scalar widths, so they require validated projection maps
and must fall back to stored text if provenance is unavailable.

## Finding 7: source, display, and diagnostic locales are independent

A person may edit Chinese source, view canonical identities, and receive Arabic
diagnostics without changing compilation. This separation prevents UI choices
from entering semantic or cache identity.

## Finding 8: Language 1.0 needs no new conversion artifact format

The exact source profiles, catalogs, locks, raw hashes, and canonical identities
already provide the durable inputs. A bounded deterministic tool report and
request-owned conversion map are sufficient. A new persisted format should be
added later only if an actual inter-tool interchange requirement proves it.

## Finding 9: the synthetic profile is useful conversion evidence

`test-Unicode@1` exercises non-ASCII lengths and a second complete public-label
catalog. It proves the mechanism between localized profiles without pretending
that unreviewed fixture terms are a ship-ready Japanese pack.

## Finding 10: paper acceptance is not implementation evidence

The expected files and cases close design choices. The workload becomes
implemented only when the real compiler front door, converter, formatter/editor,
and both permanent hosts produce the required results within measured bounds.

## Disposition

Carry these decisions into the localized-source specification during final
reconciliation. Workload 4 should now challenge the identifier, script,
normalization, bidi, confusable, invisible, and join-control rules across the
required writing systems.
