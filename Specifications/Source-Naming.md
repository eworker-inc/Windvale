# Windvale source naming conventions

## Status

This document defines the accepted naming convention for official Windvale
source. The Seed grammar implements the ASCII identifier and machine-name
boundaries described here. The Language 1.0 replacement candidate adds the
separately bounded Unicode identifier contract accepted by
[Decision 0766](../Documents/Decisions/0766-Complete-Language-1.0-Localized-Source-Reconciliation.md)
and defined in the
[localized-source specification](Windvale-Language-1.0-Localized-Source.md).
Native symbol mangling remains a future object-model contract.

## Source identifiers

A Seed source identifier contains ASCII identifier segments joined only by the
modifier letter macron `ˉ` (U+02C9):

```text
[A-Za-z_][A-Za-z0-9_]*(ˉ[A-Za-z_][A-Za-z0-9_]*)*
```

Identifiers are case-sensitive. U+02C9 is the only non-ASCII character admitted by the Seed identifier grammar. Visually similar characters such as `¯` U+00AF, `-` U+002D, and `‐` U+2010 are not aliases and are rejected where an identifier is required.

The Language 1.0 replacement form preserves that entire ASCII subset and admits
normalized Unicode identifier segments using the exact candidate
`windvale.unicode17.source@1` profile defined by the
[source-profile artifact formats](Windvale-Language-1.0-Source-Profile-Formats.md).
U+02C9 remains the only semantic-word separator and remains part of
identity. Language 1.0 identifier comparison is exact ordinal UTF-8 after
admission; it performs no host normalization, case folding, collation, or
transliteration.

Official Windvale source uses these conventions:

- In a cased script, user-defined module, type, data, function, parameter, and
  local names start with the script's reviewed capital form where one exists.
- In an uncased script, names use their reviewed natural form and do not invent
  capitalization.
- Semantic words are separated with U+02C9, as in `Moduleˉreader`, `Readˉsection`, and `Maximumˉstackˉdepth`.
- A localized name translates the complete concept in its language's natural
  order; it does not mechanically translate canonical macron segments one by
  one.
- Macron-separated names do not contain camel-case word boundaries.
- Constants use `ALL_CAPS_WITH_UNDERSCORES` in ASCII/cased official source.
  Uncased scripts use the same semantic-segment convention as other names; the
  declaration category already identifies a constant.
- Keywords and primitive type names remain lowercase.
- Source filenames capitalize the first word and use ASCII hyphens between words, as in `Module-Reader.wv`.

Capitalization is an official formatter and review convention, not a grammar
rejection rule. Third-party programs may select another casing convention while
still using valid identifiers.

## Declarations and mutation

- `let` declares an immutable initialized local.
- `var` declares a mutable initialized local.
- Function parameters are immutable.
- `data` declares immutable module data.
- The executable source entry point is the exported `Main() -> i32` function.

These distinctions make mutation visible in source before Windvale adds buffers, records, references, or concurrency.

## Machine-facing names

Capability IDs, package IDs, command-line options, protocol fields, object-format tags, and external ABI names use separately specified ASCII-safe grammars. The Seed capability grammar remains qualified lowercase ASCII, for example `console.write_line`.

Windvale bytecode declaration names are canonical UTF-8 source metadata and are not native ABI symbols. A future native object model must define deterministic, collision-free ASCII symbol mangling rather than passing U+02C9 through to C, assembly, PE/COFF, ELF, firmware, or host APIs.

## Unicode boundary

Source files, comments, and `text` values use strict UTF-8 and support full
Unicode. Seed identifiers remain restricted to ASCII segments plus U+02C9. The
Language 1.0 replacement candidate admits Unicode identifiers only
through its explicit normalization, security, diagnostic, tooling,
source-vocabulary, malformed-input, and cross-host requirements; current Seed
tools do not implement that boundary. The exact Unicode 17.0.0 design boundary
is owner accepted and remains subject to replacement-freeze acceptance and
later executable qualification.
