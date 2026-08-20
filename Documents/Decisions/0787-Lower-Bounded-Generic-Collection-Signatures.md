# Decision 0787: Lower bounded generic collection signatures

- Status: Accepted
- Date: 2026-08-20

## Context

Decision 0786 connected one direct function type parameter to source symbols,
bindings, monomorphic WIR, and ordinary WVB. Language 1.0 collections require a
stronger but still bounded shape: `sequence<Element, Maximum>` carries both a
type argument and a constant maximum. Treating the packed private collection
shape as one opaque identity would prevent useful inference and would lose the
exact conflict diagnostic when two arguments disagree only on capacity.

The first implementation attempt also pushed the compiler-scale native product
past its fixed 32 MiB whole-image limit. The feature therefore needed both a
structural resolution rule and a smaller implementation, not a wider limit.

## Decision

1. Permit one generic identifier in the maximum position of a function
   signature's `sequence` or `builder` type. The declaration parser retains the
   spelling; source symbols require that it resolve to a declaration-ordered
   constant generic parameter. Its value retains the declared fixed-integer
   shape and must fit the existing positive 1-through-4,095 bound.
2. Permit a type parameter as that collection's element. Nested collections,
   generic records and variants, builder escape, and constant-generic use in
   any other position remain unsupported.
3. Decompose a concrete collection argument into family, element shape, and
   maximum. Require the same family, contribute element and maximum separately,
   and apply the existing first-origin conflict rule to repeated evidence.
4. Require an explicit constant argument to be an integer token with the exact
   declared fixed-integer shape. For `const Maximum: u32`, `8u32` is accepted
   and unsuffixed `8` is rejected rather than implicitly converted.
5. Reuse the canonical WVGS kind/shape/low/high identity and the one-instance-
   per-declaration WVGC checkpoint from Decision 0786. Inferred and explicit
   calls with the same element and maximum reuse one specialization.
6. Substitute the concrete collection descriptor into ordinary parameter,
   local, result, index, and call checking before publishing WVLB or WVIR.
   Generic evidence does not enter either artifact.
7. Share generic-parameter reads, collection contributions, immutable WIR state
   replacement, current-block replacement, and partial block reservation.
   Keep the native limit at 33,554,432 bytes.

## Evidence

The hosted publisher rejects all three intended negative cases before partial
publication:

- two sequence arguments infer different maxima for one parameter;
- `8` supplies the wrong fixed-integer width for `const Maximum: u32`; and
- a builder is supplied where the formal family is sequence.

Its successful source creates `builder<i32, 8>`, freezes it, calls
`First<Type, const Maximum: u32>` by inference and as
`First::<i32, 8u32>`, publishes WVSS/WVCA/WVLB/WVIR, and returns `42`.
The 1,065,397-byte publisher WVB has SHA-256
`ca1b50539ab3c53966fde062e8816b829d25b0dc0bd14bcb3374a813443ecc7a`.
The canonical segmented native route selects 33,487,778 machine bytes—66,654
below the unchanged ceiling—and its hosted executable returns `42`.

A same-length hand-written monomorphic source preserves the relevant source
offsets. Generic and oracle analysis share these exact identities:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVCA | 104 | `debdc883ad8ebbde577589bc9248f58f79b70f5e7851409545b21be5282a73cb` |
| WVLB | 184 | `6df7f06016882fca5b38d909ca56136587a94975de60431daca96d13e9e35f4c` |
| WVIR | 976 | `c9a9299f223cae34887fd6788180f81b0b9a8d1499e99d5f81c2d053694361ab` |

The retained emission driver emits the oracle as a 466-byte WVB with SHA-256
`2d59187da5f16a3b275a6bbe96502ce1309f0ba8348e8a22da02097808c8b0c6`.
No new execution claim is made: the current pinned native verifier rejects the
unchanged 809-byte non-generic collection fixture at the same target-specific
semantic boundary. This is therefore not evidence of a generic regression.

The fully current general emission-driver WVB also builds successfully at
1,268,289 bytes / SHA-256
`948903a8da8a6df6d5d9f388749db0b1165dbe2c81056125ea04449ecceb2bc5`,
but its 37,097,130 selected native bytes exceed the unchanged whole-image
ceiling. It was not packaged. A target-aware validated-analysis-to-emission
split remains required before direct generic-source-to-WVB parity is claimed.

Heavy storage, OS, paired-host, and complete Qualification gates remain
deferred to the final seven-slice integration gate.

## Consequences

Windvale source analysis can now infer and explicitly select the element type
and capacity of a bounded generic collection function without runtime generics,
implicit numeric conversion, or a second collection representation. The
published binding and WIR products are exactly monomorphic.

This completes the bounded collection analysis/WIR checkpoint, not general
constant generics, multiple specializations, direct current-driver emission,
or collection-capable native execution.

## Reconsideration triggers

Replace the one-specialization limit when the monomorphic function directory
can publish multiple deterministic bodies for one declaration. Generalize
constant positions only with an exact semantic consumer and retained bounds.
Revisit the compact collection descriptor if nested collections or maxima above
4,095 become accepted language requirements. Replace the target-aware emission
split only if a smaller fully validating single-process driver fits with useful
measured headroom.
