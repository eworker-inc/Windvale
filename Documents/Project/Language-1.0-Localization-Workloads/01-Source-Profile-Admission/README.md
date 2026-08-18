# Language 1.0 localization workload 1: source-profile admission

## Status

Complete first-author paper bundle for the source-profile admission workload in
the [localization workload plan](../../Windvale-Language-1.0-Localization-Workloads.md).
Its technical and design findings are accepted by the project owner. It is not
a source freeze, compiler implementation, translation-quality claim, or
cross-host performance result. Current compilers continue to accept Windvale
Seed.

## Result first

The workload replaces an abstract “localization pack” with seven exact bounded
artifact contracts and eleven reference artifacts. It proves on paper that:

- `#!wv/1 en@1` resolves through one explicit lock to a hash-bound profile;
- profile admission binds Unicode data, the 66-token registry, one complete
  keyword lexicon, and one source-vocabulary profile;
- a public-label catalog is independently bound to the exact
  `Foundationˉoption` interface hash;
- no component contains code, paths, fallback, network lookup, or host locale;
- exact bytes and hashes can be checked before dependent parsing;
- a synthetic Unicode profile exercises non-ASCII keyword and public-label
  paths without pretending to be a human-language translation; and
- validated cache publication can remain private until the whole dependency
  chain succeeds.

The exact artifact index is 1,214 bytes with SHA-256
`562be979215b9ad9b4a6b9990fa004902b4f239ae1baf6773f0e686a570d3c9f`.
It covers 11 artifacts totaling 12,895 bytes. The selected `en@1` chain plus
lock and one catalog is 8,695 distinct bytes; the synthetic chain is 8,794
distinct bytes because both share the Unicode profile and token registry.

## Bundle contents

| Item | Purpose |
| --- | --- |
| [`Reference-Artifacts/`](Reference-Artifacts/) | Exact Unicode profile, token registry, lexicons, vocabulary profiles, catalogs, composite profiles, build lock, sizes, and hashes. |
| [`Source/`](Source/) | Canonical English and synthetic Unicode source expected to lower to the same canonical tokens and public declarations. |
| [Accepted cases](Accepted-Cases.md) | Descriptor, dependency, Unicode, source, catalog, hashing, and cache success vectors. |
| [Rejected cases](Rejected-Cases.md) | Earliest bounded rejection for malformed bytes, stale hashes, bad Unicode, incomplete catalogs, lock errors, and cache races. |
| [Performance and cache](Performance-And-Cache.md) | Structural limits, measurement protocol, publication rules, and remaining host evidence. |
| [Implementation responsibilities](Implementation-Responsibilities.md) | Durable owner for each future implementation boundary. |
| [Review findings](Review-Findings.md) | Proposed decisions, consequences, and remaining freeze blockers. |

The normative-candidate serialization is in the
[source-profile format specification](../../../../Specifications/Windvale-Language-1.0-Source-Profile-Formats.md).

## Reference dependency graph

~~~text
Source-Inputs.wvlock
  +-- en@1 ----------------------------------------------+
  |   +-- Unicode-17-Source.wvup --------------------+   |
  |   +-- Language-1-Keyword-Tokens.wvktr --------+  |   |
  |   +-- En-Keywords.wvlex -----------------------|--+   |
  |   +-- En-Vocabulary.wvsvp ---------------------|------+
  |   +-- En-Foundation-Option.wvcat <-------------+------+
  |
  +-- test-Unicode@1 ------------------------------------+
      +-- shared Unicode profile and token registry      |
      +-- Test-Unicode-Keywords.wvlex -------------------+
      +-- Test-Unicode-Vocabulary.wvsvp -----------------+
      +-- Test-Unicode-Foundation-Option.wvcat <---------+
~~~

Arrows represent identity/version/content-hash checks, not filesystem paths.
The lock resolves only the two composite profiles and two interface catalogs;
the composite profiles bind their own components.

## Unicode decision exercised

The reference Unicode profile pins Unicode 17.0.0 and exact upstream data-file
sizes and SHA-256 hashes. It selects NFC, UAX #31 `XID_Start`/`XID_Continue`, the
UTS #39 Allowed identifier profile, Highly Restrictive script admission, one
decimal-number system per segment, and scoped LTR/RTL confusable rejection.
Default-ignorables and join controls are excluded from edition 1.

The pinned primary references are the Unicode Consortium's
[Unicode 17.0.0 release](https://www.unicode.org/versions/Unicode17.0.0/),
[UAX #15 revision 57](https://www.unicode.org/reports/tr15/tr15-57.html),
[UAX #31 revision 43](https://www.unicode.org/reports/tr31/tr31-43.html), and
[UTS #39 revision 32](https://www.unicode.org/reports/tr39/tr39-32.html).
The `.wvup` artifact records the exact eleven data inputs needed by the working
policy; a compiler may consume equivalent generated tables only with evidence
against those bytes.

## Synthetic profile boundary

`test-Unicode@1` is an engineering fixture, not a locale. It changes only
`false`, `if`, `return`, and `true` to unique Han-character test spellings and
uses a complete synthetic catalog for the 16 source-addressable
`Foundationˉoption` labels. The terms have no translation-quality standing and
must never ship as Chinese or Japanese support.

Its purpose is to expose UTF-8, normalization, keyword-boundary, catalog,
source-span, and canonical-lowering defects before native reviewers spend time
on a real `zh-Hans@1` profile.

## Expected lowering

The two files under `Source/` retain the same project-owned module, function,
parameter, alias, literal, target, profile, and authority identities. After
profile lowering, both must contain the canonical imports and body operations:

~~~text
import Foundationˉoption as Selected;
if Flag {
    return Selected.Option.Present { Value: true };
}
return Selected.Option.Absent;
~~~

The raw source hashes and profile/catalog provenance differ. The canonical token
and resolved public-declaration sequences must match. Actual WIR, WVB, object,
and executable equality remains a later implementation workload rather than a
paper claim.

## Review rule

Review the formats, reference bytes, accepted cases, rejected cases, and
responsibility map together. A format is not acceptable merely because the
happy-path sample can be parsed. It must reject stale, oversized, reordered,
ambiguous, hostile, and incomplete inputs before publishing shared state.
