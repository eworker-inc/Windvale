# Workload 9 package text and report contract

## Manifest `WVPACK1`

The manifest is strict UTF-8 package data with LF line endings. Line 1 is
exactly `WVPACK1`. The remaining unordered field records are:

~~~text
package <identity>
version <u64-decimal>
dependency <identity>
~~~

There is exactly one package and version. Dependency records may repeat only
when the parser reports a duplicate; they do not silently collapse. Unknown
fields, extra words, empty values, comments, CR, prefixes, signs, whitespace
variants, and trailing text are rejected.

## Lock `WVLOCK1`

Line 1 is exactly `WVLOCK1`. Every remaining nonempty line is:

~~~text
package <identity> <u64-decimal> deps <identity>{,<identity>}
package <identity> <u64-decimal> deps -
~~~

Package lines may arrive in any order. Dependency spellings on one line may
arrive in any order, but must be unique. A lock contains at most the admitted
package count, and each package has at most the admitted dependency count.

An identity is 1 through 64 UTF-8 bytes in the hard profile: the first scalar
is `a` through `z`; later scalars are lowercase ASCII letters, ASCII digits, or
underscore. Comparison is ordinal Unicode-scalar lexicographic order. Because
the admitted identity alphabet is ASCII, this is also ascending UTF-8 byte
order. Equality used by collections is exactly `Ordering.Compare == Equal`.

Versions are one or more ASCII decimal digits, with no sign, separator,
whitespace, prefix, leading policy conversion, locale digit, or trailing text.
The Foundation whole-u64 parser supplies exact empty/invalid/limit/overflow
failure.

## Canonical report `WVPKGREPORT1`

The report is strict UTF-8 and always LF-terminated:

~~~text
WVPKGREPORT1
package <identity>@<version> deps=<dependency-list-or-dash>
order=<dependency-first-list>
notice-equal=<true-or-false>
~~~

Package lines use ascending map rank. Dependency lists use ascending set rank.
The dependency-first order repeatedly scans ascending package rank, publishes
the first currently ready package, and restarts at rank zero. Therefore ties are
lexical and input insertion order is irrelevant. A complete scan with no
selection and remaining packages reports one cycle diagnostic whose retained
identities are also ascending.

The serializer is explicit source over a bounded text builder. No field
reflection, host object enumeration, map-node traversal, locale, optional
field, or implicit version upgrade participates.

## Exact reference output

~~~text
WVPKGREPORT1
package app@1 deps=codec,util
package codec@1 deps=core
package core@2 deps=-
package util@1 deps=core
order=core,codec,util,app
notice-equal=true
~~~

It is 160 UTF-8 bytes, 7 LF-terminated lines, and has SHA-256
`a9df168004784b0b1af30bb2c563d9ae166bd3a38dceb388b731b8d72dcba2b7`.
