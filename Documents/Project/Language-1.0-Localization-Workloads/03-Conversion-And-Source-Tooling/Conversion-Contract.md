# Deterministic source-profile conversion contract

## Product operation

Conversion changes the source profile of one stored Windvale file. It is not a
general translation operation. A conceptual command is:

~~~text
windvale source convert --to-profile <identity@version> --output <path> <source>
~~~

The exact command-line spelling belongs to the future source tool. The Language
1.0 behavior below does not depend on a shell, editor, or host locale.

The safe default writes a distinct output path. Replacing the input requires an
explicit replace option and the publication rules below.

## Required inputs

The converter receives:

1. the exact raw source bytes;
2. the input file's byte-zero source descriptor;
3. one explicit source-input lock;
4. the source and target composite profile bytes and hashes;
5. every component lexicon, vocabulary profile, Unicode profile, and token
   registry bound by those profiles;
6. each source and target public-library catalog used by the file, bound to the
   exact imported interface hash; and
7. the requested target output path and publication mode.

It does not inspect the host locale, search installed directories, choose a
newer pack, contact a registry, or fall back to canonical English. Installation
or lock-file changes are separate explicit package operations.

## Conversion algorithm

The simple correctness implementation performs these ordered phases:

1. Hash the input bytes and admit the source descriptor, source profile,
   components, catalogs, imports, and complete source under their exact bounds.
2. Build an immutable ordered replacement plan. Each entry contains the input
   raw byte span, canonical token or declaration identity, owning catalog when
   applicable, and target primary spelling.
3. Resolve and admit every target profile component and required target catalog.
   Reject a missing mapping, collision, stale interface binding, or invalid
   target spelling before constructing output.
4. Replace only the descriptor profile span, admitted keyword spans, and
   resolved imported-public-label spans. Copy every byte outside those disjoint
   spans unchanged and preserve the descriptor's admitted LF or CRLF line
   ending.
5. Admit the complete candidate output from byte zero under the exact target
   inputs. Produce its canonical token, project-identifier, imported-declaration,
   literal, and source-structure projection.
6. Compare that projection with the admitted input projection. Any semantic or
   preservation difference is an internal conversion failure.
7. Produce a bounded report and publish the already validated candidate using
   the requested publication mode.

Replacement spans are derived from compiler front-door provenance, not textual
search. Applying replacements from the end of the file toward the beginning is
one valid implementation, but the observable requirement is exact span handling
with no overlap or offset corruption.

## What changes

Only these bytes can change during source-profile conversion:

- the profile identity/version inside the universal descriptor;
- primary source spellings for recognized keyword token identities; and
- primary source labels for resolved imported public modules, declarations,
  members, fields, cases, and named parameters.

The target writes exactly one primary spelling from each target lexicon or
interface-bound catalog. It never retains a source alias merely because both
spellings would parse.

## What does not change

Unless a separate user-requested operation owns it, conversion preserves the
exact bytes of:

- project-owned module, type, declaration, field, case, parameter, alias, and
  local identifiers;
- string, byte, rune, numeric, and other literal spelling;
- comments, documentation, whitespace, blank lines, indentation, and line
  endings;
- package data, embedded resources, protocol and database names, foreign
  symbols, capability/effect identities, platform/target identities, ABI names,
  and other registered machine keys; and
- malformed or incomplete editor text, because only a fully admitted source
  file is eligible for conversion.

Natural-language translation of project names or prose is a different,
potentially AI-assisted refactoring. It cannot be hidden inside this operation.

## Equivalence and round trip

Input and output must have equal ordered canonical token identities, equal
project-owned identifier bytes, equal resolved imported-public identities,
equal literals, and equal parse/semantic structure after source spans and profile
provenance are removed.

Converting a valid file from profile A to B and directly back to the same exact
profile A with the same exact inputs must reproduce the original bytes. This
byte-round-trip rule applies only when no formatting, rename, catalog change, or
other edit occurs between conversions.

The expected files in `Source/` exercise:

- `zh-Hans@1` to `en@1`;
- `en@1` to `zh-Hans@1`;
- `zh-Hans@1` to `test-Unicode@1`; and
- each route back to the original exact bytes.

## Publication

A distinct output path is created only after target admission and equivalence
succeed. If the destination already exists, the tool fails unless the user
selected an explicit replacement policy; it does not silently overwrite it.

Explicit in-place replacement additionally requires:

1. rechecking that the current source bytes still match the initial input hash;
2. constructing and validating a temporary file in the destination directory;
3. using a host operation that provides atomic same-filesystem replacement; and
4. deleting only that operation's exact private temporary file on failure.

If atomic replacement is unavailable, the converter refuses in-place mode and
can still write a distinct output. A conflict, cancellation, stale input, or
publication failure leaves the original source unchanged and never exposes a
partially converted file.

## Conversion report

Success reports at least:

- tool/compiler identity;
- input and output raw byte count and SHA-256;
- source and target profile identity, version, manifest hash, lexicon hash, and
  vocabulary-profile identity;
- every source and target catalog identity/content/interface hash used;
- replacement counts by descriptor, keyword, module, declaration/member, field,
  case, and named parameter;
- unchanged-byte count;
- canonical token/declaration projection hash; and
- publication mode and final output identity.

Failure reports the stable phase and diagnostic identity, the affected bounded
source/target input identities, and no success projection. The first
implementation may serialize this report through an existing deterministic tool
report convention; Language 1.0 does not need a new executable or source artifact
format merely to perform conversion.
