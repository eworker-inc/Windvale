# Windvale source naming conventions

## Status

This document defines the accepted naming convention for official Windvale source. The Seed grammar implements the identifier and machine-name boundaries described here. Native symbol mangling remains a future object-model contract.

## Source identifiers

A Seed source identifier contains ASCII identifier segments joined only by the modifier letter macron `ˉ` (U+02C9):

```text
[A-Za-z_][A-Za-z0-9_]*(ˉ[A-Za-z_][A-Za-z0-9_]*)*
```

Identifiers are case-sensitive. U+02C9 is the only non-ASCII character admitted by the Seed identifier grammar. Visually similar characters such as `¯` U+00AF, `-` U+002D, and `‐` U+2010 are not aliases and are rejected where an identifier is required.

Official Windvale source uses these conventions:

- User-defined module, type, data, function, parameter, and local names start with a capital letter.
- Semantic words are separated with U+02C9, as in `Moduleˉreader`, `Readˉsection`, and `Maximumˉstackˉdepth`.
- Macron-separated names do not contain camel-case word boundaries.
- Constants use `ALL_CAPS_WITH_UNDERSCORES`.
- Keywords and primitive type names remain lowercase.
- Source filenames capitalize the first word and use ASCII hyphens between words, as in `Module-Reader.wv`.

Capitalization is an official formatter and review convention, not a Seed grammar rejection rule. Third-party programs may select another casing convention while still using valid identifiers.

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

Source files, comments, and `text` values use strict UTF-8 and support full Unicode. Restricting Seed identifiers to ASCII segments plus U+02C9 does not restrict program data or user-visible text. Broader Unicode identifier support may be considered later only with explicit normalization, security, diagnostic, and tooling rules.
