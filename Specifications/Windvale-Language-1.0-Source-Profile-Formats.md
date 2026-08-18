# Windvale Language 1.0 source-profile artifact formats

## Status and ownership

This is the working normative-candidate serialization contract for the
[localized-source specification](Windvale-Language-1.0-Localized-Source.md).
It was first exercised by
[localization workload 1](../Documents/Project/Language-1.0-Localization-Workloads/01-Source-Profile-Admission/README.md).
It is not frozen or implemented. The replacement Language 1.0 source-freeze
decision must accept an exact revision and manifest before implementation begins.

This document owns the byte encoding, record grammar, ordering, bounds, hash
coverage, and admission order for:

- Unicode source profiles (`.wvup`);
- keyword token registries (`.wvktr`);
- keyword lexicons (`.wvlex`);
- source-vocabulary profiles (`.wvsvp`);
- public-interface source catalogs (`.wvcat`);
- composite source profiles (`.wvsp`); and
- source-input lock records (`.wvlock`).

The localized-source specification owns source behavior after admission. The
package/build plan resolves hashes to bytes. No artifact grants authority,
executes code, selects a host locale, or searches an installation.

## Common byte contract

Every artifact:

- is strict UTF-8 without a byte-order mark;
- uses LF U+000A as its only line ending and ends with exactly one LF;
- contains no CR, NUL, tab, blank line, comment, leading whitespace, trailing
  whitespace, or empty field;
- uses U+007C `|` as the only field delimiter and has no escaping;
- has a format header as its first record and the matching `end|<format>` record
  as its last record;
- rejects unknown, duplicate, missing, reordered, or extra records;
- is hashed as its exact complete byte sequence, including the final LF, with
  SHA-256 encoded as 64 lowercase hexadecimal digits; and
- is admitted only when its externally supplied expected hash matches before any
  dependent artifact is parsed.

ASCII identity fields use the source-profile identity grammar: 2 through 96
bytes, dot-separated components, hyphen-separated atoms, and atoms beginning
with an ASCII letter followed by ASCII letters or digits. Matching is ordinal and
case-sensitive. A component version is a positive `u32` decimal with no sign,
separator, suffix, or leading zero. A count is an unsigned `u32` decimal using
`0` or a nonzero first digit and no leading zero.

A SHA-256 field contains exactly 64 lowercase ASCII hexadecimal digits. A byte
length is an unsigned `u64` decimal. A URL field in the Unicode profile is ASCII,
absolute `https`, contains no query or fragment, and is provenance rather than a
runtime fetch instruction.

The general per-record maximum is 1,024 UTF-8 bytes excluding LF. A parser
rejects the artifact-size bound before whole-file allocation and rejects an
oversized record while streaming. Checked arithmetic is required for every
count, length, offset, and retained-state calculation.

## Unicode source profile

### Format and bounds

A `.wvup` is ASCII-only and no larger than 64 KiB. Its exact record order is:

~~~text
windvale-unicode-source-profile|1
identity|<identity>
version|<version>
unicode-version|<major.minor.patch>
normalization|NFC
identifier-start|XID_Start
identifier-continue|XID_Continue
identifier-status|Allowed
restriction-level|Highly_Restrictive
mixed-numbers|reject
default-ignorables|reject
join-controls|reject
confusable-scope|lookup-scope
confusable-directions|LTR-and-RTL
semantic-separator|U+02C9
underscore|start-and-continue
reference-count|<count>
reference|<name>|<revision>|<https-url>
...
input-count|<count>
input|<logical-name>|<byte-length>|<sha256>|<https-url>
...
end|windvale-unicode-source-profile
~~~

References are sorted by ASCII `name`. Inputs are sorted by ASCII
`logical-name`. Duplicate names or URLs are invalid. A conforming build uses the
listed exact input bytes or a generated table proven equivalent to them; it does
not use host Unicode tables.

### Edition-1 Unicode policy

The working edition-1 profile is `windvale.unicode17.source@1` and pins Unicode
17.0.0, UAX #15 revision 57, UAX #31 revision 43, and UTS #39 revision 32.

Each identifier segment:

1. must already be NFC; the compiler diagnoses but does not silently normalize;
2. begins with `_` or a scalar in `XID_Start` and continues with `_` or scalars
   in `XID_Continue`;
3. admits `_` as the only Windvale exception to the UTS #39 Allowed filter;
4. otherwise contains only `Identifier_Status=Allowed` scalars;
5. contains no default-ignorable scalar, including ZWJ or ZWNJ in edition 1;
6. satisfies the UTS #39 Highly Restrictive level or the stricter ASCII-only
   level;
7. uses at most one decimal-number system; and
8. contains no forbidden control, surrogate, noncharacter, private-use,
   unassigned, Pattern_Syntax, or Pattern_White_Space scalar.

U+02C9 separates semantic segments. It is excluded from the segment character
sets and security calculations. The restrictions above apply independently to
each segment.

Within one lookup scope, two distinct identifier segments may not have the same
UTS #39 revision-32 `bidiSkeleton` under either LTR or RTL direction. The later
declaration or catalog entry is rejected with both exact source spans. This rule
also applies to localized public labels competing in one resolution namespace.
Different exact identifiers in unrelated scopes remain legal.

Excluding join controls keeps the first edition deterministic and visually
inspectable but may prevent the preferred spelling of some languages. A later
edition may add their UTS #39 contextual rules only after native-language
workloads demonstrate the need; a pack cannot add them on its own.

## Keyword token registry

A `.wvktr` is ASCII-only, no larger than 64 KiB, and contains:

~~~text
windvale-keyword-token-registry|1
identity|<identity>
version|<version>
source-edition|<version>
token-count|<count>
token|<four-digit-ordinal>|<token-identity>
...
end|windvale-keyword-token-registry
~~~

Ordinals begin at `0001`, are contiguous, and appear in ascending order. A token
identity is `KW_` followed by one or more uppercase ASCII words separated by one
underscore; its maximum is 64 bytes. Token identities are semantic external IDs,
not English source spellings or compiler-private enum values. Edition 1 has
exactly 66 rows.

## Keyword lexicon

A `.wvlex` is no larger than 64 KiB and contains:

~~~text
windvale-keyword-lexicon|1
identity|<identity>
version|<version>
source-edition|<version>
unicode-profile|<identity>|<version>|<sha256>
token-registry|<identity>|<version>|<sha256>
entry-count|<count>
entry|<four-digit-ordinal>|<token-identity>|<primary-spelling>
...
end|windvale-keyword-lexicon
~~~

Entries follow the bound registry order and repeat both ordinal and identity so
a malformed or stale mapping fails locally. The count equals the registry count.
Each primary spelling is a valid single identifier segment under the bound
Unicode profile, is at most 128 bytes and 64 Unicode scalars, and is unique by
exact bytes and by both admitted confusable skeleton directions. No alias,
fallback spelling, semantic separator, or universal numeric type word is present.

## Source-vocabulary profile

A `.wvsvp` is no larger than 64 KiB and contains:

~~~text
windvale-source-vocabulary-profile|1
identity|<identity>
version|<version>
source-edition|<version>
unicode-profile|<identity>|<version>|<sha256>
label-normalization|NFC
catalog-format|windvale-public-source-catalog|1
end|windvale-source-vocabulary-profile
~~~

It selects a terminology contract and label rules. It does not enumerate
libraries, supply labels, or create a fallback chain.

## Public-interface source catalog

A `.wvcat` is no larger than 1 MiB and contains:

~~~text
windvale-public-source-catalog|1
vocabulary|<identity>|<version>|<profile-sha256>
package|<canonical-package-identity>
module|<canonical-module-identity>
major|<positive-u32>
interface|<sha256>
entry-count|<count>
label|<kind>|<canonical-key>|<primary-source-label>
...
end|windvale-public-source-catalog
~~~

Package identity is an ASCII registered identity. Module identity is its exact
canonical Language 1.0 identifier. The interface hash is the exact public
signature-set hash owned by that module. Catalog admission obtains the complete
source-addressable canonical key set from that interface and requires one and
only one row for every key.

Kinds have this fixed rank and spelling:

1. `module`;
2. `declaration`;
3. `case`;
4. `field`;
5. `operation`; and
6. `parameter`.

Rows are sorted first by kind rank and then by ordinal UTF-8 canonical-key bytes.
A canonical key is at most 512 bytes and contains no `|` or control scalar. A
primary label is one complete Language 1.0 `Identifier`, at most 128 UTF-8 bytes
and 64 Unicode scalars. Exact duplicate labels and confusable labels are rejected
only when they compete in the same catalog resolution namespace; reusing
`Value` as a parameter under different operations is valid.

Generic-parameter names are explanatory rather than source-addressable in
edition 1 and do not receive catalog rows. The explicit generic argument list is
positional and canonical type identities are resolved independently.

## Composite source profile

A `.wvsp` is ASCII-only, no larger than 64 KiB, and contains:

~~~text
windvale-source-profile|1
identity|<identity>
version|<version>
source-edition|<version>
unicode-profile|<identity>|<version>|<sha256>
token-registry|<identity>|<version>|<sha256>
keyword-lexicon|<identity>|<version>|<sha256>
source-vocabulary|<identity>|<version>|<sha256>
end|windvale-source-profile
~~~

Every reference must match the dependent artifact's declared identity, version,
source edition, and external content hash. The profile identity/version is the
exact value selected after `#!wv/1 ` in the source descriptor.

## Source-input lock

A `.wvlock` for these inputs is ASCII-only, no larger than 1 MiB, and contains:

~~~text
windvale-source-input-lock|1
profile-count|<count>
profile|<identity>|<version>|<profile-sha256>
...
catalog-count|<count>
catalog|<vocabulary-identity>|<version>|<module>|<major>|<interface-sha256>|<catalog-sha256>
...
end|windvale-source-input-lock
~~~

Profiles are sorted by identity and numeric version. Catalogs are sorted by
vocabulary identity, numeric version, module UTF-8 bytes, numeric major, and
interface hash. The lock contains no path or URL. The build executor resolves an
expected content hash through an approved content store and supplies exact bytes
to the compiler. Missing content is a build-input failure, never a search or
download request from the compiler.

## Admission order and publication

The compiler front door admits inputs in this order:

1. source descriptor syntax and bounds;
2. source-input lock syntax and bounds;
3. selected composite profile bytes and external hash;
4. Unicode profile and token registry;
5. keyword lexicon and source-vocabulary profile;
6. imported canonical interfaces and their exact hashes;
7. required source catalogs; and
8. localized source lexing and public-label resolution.

Failure at a step prevents all later work. No partially validated table enters a
shared compiler-service cache. Publication uses a private candidate, verifies
all declared counts, hashes, identities, cross-references, and collision rules,
then atomically publishes one immutable generation.

## Cache identity and bounds

Validated components are cached by `(format, external SHA-256)` rather than by
identity/version alone. Composite cache entries retain their component hashes.
Negative results may be request-local but are not retained across untrusted
requests by default.

The Language 1.0 replacement freeze requires measured ceilings for cold
validation, warm lookup, peak allocation, retained bytes, and failure
diagnostics. A conforming compiler must remain linear in admitted artifact bytes
plus admitted entries, with bounded hashing, comparison, collision, and
diagnostic work. Workload-specific candidate thresholds are recorded outside
this normative format contract until measurements exist on both permanent hosts.
