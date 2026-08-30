# Decision 0892: coordinate authenticated source admission

## Status

Accepted implementation candidate on 2026-08-30. Focused current-Windows
development evidence is recorded below. Local Linux execution and paired-host
qualification remain pending.

## Context

[Decision 0886](0886-Make-Target-And-Foreign-Admission-A-Mandatory-Language-1.0-Phase.md)
requires one fail-closed Language 1.0 phase to publish admitted WVSS, exact
WVTD, canonical WVFC, and binding WVAE together. Decisions 0887 through 0891
implemented the independent evidence format, canonical catalog producer,
foreign-memory oracle, native SHA lowering, and retained SHA-capable staging.
They did not yet compose those boundaries into one authenticated result.

A structure-valid WVFC is not source-authenticated. An empty catalog with the
right module count, or a catalog whose declaration ordinals were remapped,
could otherwise pass a target-only readback. A generic `linux` predicate may
admit an ordinary Linux target but cannot authorize a concrete foreign ABI.
The public boundary must preserve those distinctions and reject oversized
input before any source scan.

## Decision

1. Add `Compilerˉsourceˉadmissionˉcoordinate` as the portable in-memory
   coordinator. It accepts exact WVSS 1 source input, source-input lock,
   64-byte lowercase lock digest, selected profile, and WVTD.
2. Check all outer input and aggregate-retention bounds before source-profile
   admission. A WVSS above 4,194,304 bytes rejects before the source-set scanner
   or declaration parser runs.
3. Admit locked source to descriptor-free WVSS 2, validate every source
   platform against exact WVTD, produce complete canonical WVFC, independently
   revalidate the three formats and counts, enforce exact foreign predicates,
   and construct WVAE over the immutable values.
4. Publish exactly four separate values on success: admitted WVSS, unchanged
   WVTD, canonical WVFC, and WVAE. Every failure returns all four values empty
   with bounded phase, module, source-span, ordinal, parser, target, producer,
   catalog, and evidence diagnostics.
5. Keep ordinary target scope matching disjunctive, but require the concrete
   `linux.x86_64.sysv_amd64_c_v1` predicate alone for foreign source. Generic
   `linux` and `linux` plus the exact predicate reject; exact-only succeeds.
   `Unknownˉplatform` maps to target status `UNKNOWN_PLATFORM`.
6. Make durable public foreign-catalog readback reproduce canonical WVFC from
   supplied WVSS and require byte identity before foreign-target admission.
   Empty, omitted, reordered, remapped, duplicated, or extra records therefore
   fail even when standalone WVFC structure is valid.
7. Collapse catalog reproduction failure into outer target-admission status
   `WVFC` while preserving subordinate source-set, parse, catalog, module, and
   source-offset evidence. A byte mismatch reports the first differing WVFC
   offset. The coordinator's earlier production step retains its exact
   foreign-producer status.
8. Bound public readback geometry explicitly. Stable retention is at most
   12,583,216 bytes: 4,194,304 WVSS, 320 WVTD, 4,194,304 supplied WVFC, and
   4,194,288 reproduced WVFC bytes. Conservative simultaneously-live large
   immutable payload retention adds the producer's 4,194,240-byte headerless
   record accumulator, totaling 16,777,456 bytes. This is not a process
   working-set bound; it excludes runtime state and small bounded scalar/view
   temporaries. Comparison is one bounded linear byte scan.
9. Register one 28-case focused native owner and route the coordinator plus
   every directly consumed admission-evidence, target, source-profile,
   source-set, declaration, catalog-producer, and SHA contract to it.
10. Normalize producer source failures to the original pre-descriptor byte
    domain at the WVSS 2 view boundary while preserving parser line and column.
    Source-set/container offsets remain upstream offsets; private record
    catalog-constructor failures name their exact canonical absolute WVFC field
    offset, and final catalog-constructor failures retain the catalog offset.

## Initial evidence

The producer owner was corrected from frozen `Build-Wvb` to the explicit
forward-language `Build-Current-Wvb` route. Its two builds produced identical
510,644-byte WVB values at SHA-256
`ef8c089b0bc1369f960c889e5d3276bfc7aa43e0584d2ec0664049669d15cf28`.
Complete verification passed, the profile-7 cache created key
`35b2a546701d2276125138d373dc64a26558bbdd161020f1024e12ed992c3e75`,
and the 12,825,088-byte application at SHA-256
`443aa180cf86b126db7d51cf29790ddaba450e613d7ac235020c51c1941a6291`
passed all 25 isolated selectors. The suite completed in 178,300 ms; registry
dispatch completed in 178,890 ms.

The coordinator owner built identical 623,826-byte WVB values at SHA-256
`0c3304cb297c86e09c2314c8b2aaac5649ba372ab19f3f4a090d6c06b3a58188`,
passed complete verification, and created profile-7 cache key
`599c706b44eac57c3a686b8202c560ae6833a9063f6b086a00e40fdb34025dc8`.
Its 14,772,736-byte application at SHA-256
`d7130fbaf5ecc1cb15b7162b4ec2a812d2ea61bdab67a9b10502d285c826dbd7`
passed all 28 isolated selectors with exit 42 and no output. The suite completed
in 207,450 ms; registry dispatch completed in 208,070 ms. The selectors cover
empty and multi-record catalogs, source-ordered readback, structure-valid
empty/remapped readback rejection, 64 modules, all four target predicates,
mismatch, unknown/duplicate/missing platforms, generic/broader/exact foreign
scopes, malformed metadata and WVTD, all-or-nothing failure, exact snapshot
authentication, deterministic output, and max/one-past resource arithmetic.

The changed-file planner passed 31 general and 229 native routing cases. At the
coordinator landing, the registry had 123 owners and 5,878 cases in 21,806
LF-only bytes at SHA-256
`e516c55c6ac760f2b380d0e885e5149a5b6428ba0518dfc21553c5d98444359f`.
Its shards contain 1/57, 43/2,790, 38/1,782, and 41/1,249 owners/cases.

This is current-Windows development evidence, not a hosted publisher, Analyzer
integration, foreign-call lowering, runtime containment, local Linux result,
or paired-host qualification claim.

## Consequences

The source-profile, target, syntactic catalog, and evidence contracts now have
one portable all-or-nothing coordinator. No cache key, producer identity, path,
structure-only catalog, or forgeable certificate substitutes for source-derived
authentication. Existing WVSS, WVTD, WVFC, WVAE, and canonical WVB versions do
not change.

Public readback deliberately reproduces WVFC and retains both catalogs during
comparison. That bounded memory and linear work are the cost of making the
exported API safe independently of its caller. The coordinator's private
post-production seam does not repeat this work because its preceding steps own
the immutable producer result and complete validation chain.

## Reconsideration triggers

Reconsider if Analyzer consumption would weaken independent authentication; if
target or foreign registries change; if reproduction exceeds the named bounds;
if streaming comparison can preserve exact source-order diagnostics with lower
retention; if the hosted publisher needs a different atomic contract; or if
Windows and Linux differ in bytes, statuses, offsets, or ordering.
