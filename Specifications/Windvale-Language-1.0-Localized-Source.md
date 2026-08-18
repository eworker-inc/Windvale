# Windvale Language 1.0 localized-source and source-vocabulary specification

## Status and ownership

This document is the normative-candidate addendum for localized stored
source in Windvale Language 1.0. The project owner selected stored localized
keywords, stored localized references to public library declarations, and
Unicode project identifiers for the replacement Language 1.0 candidate on
2026-08-18. [Decision 0766](../Documents/Decisions/0766-Complete-Language-1.0-Localized-Source-Reconciliation.md)
accepts the reconciled technical and design findings from Workloads 1 through 5
and makes this exact addendum part of the replacement candidate. Native
terminology review and all compiler, editor, package/installer, cross-host, and
measured-performance qualification remain open. The design is not frozen or
implemented. Current compilers continue to implement
[Windvale Seed](Seed-Language.md).

This document owns:

- source-lexicon selection and binding;
- source-vocabulary selection and binding;
- the mapping of localized keyword spellings to canonical token identities;
- the mapping of localized public API spellings to canonical declaration
  identities;
- the edition-1 Unicode source-identifier boundary; and
- localization-specific reproducibility, diagnostic, tooling, and conformance
  requirements.

The [semantic specification](Windvale-Language-1.0.md) continues to own typing,
ownership, effects, capabilities, evaluation, and canonical declaration
semantics. The [grammar companion](Windvale-Language-1.0-Grammar.md) and
[machine grammar](Windvale-Language-1.0.ebnf) own syntax after applying this
document's front-door mapping. The
[source-profile artifact format companion](Windvale-Language-1.0-Source-Profile-Formats.md)
owns the exact candidate serialization, limits, hashes, and admission order. The
[Foundation specification](Windvale-Language-1.0-Foundation.md) and its
[signature registry](Windvale-Language-1.0-Foundation-Registry.md) remain the
canonical public API contracts. This addendum creates no translated Foundation
implementation or alternate ABI.

The exploratory rationale, alternatives, editor behavior, shipment profiles,
and unresolved product questions remain in
[semantic source views and localization](../Documents/Project/Windvale-Semantic-Source-Views-And-Localization.md).

## Product invariant

Windvale has one semantic language and may have many exact stored source
vocabularies. Localization changes admitted source spellings; it does not create
different parsing, typing, ownership, capability, optimization, WIR, WVB,
object, linking, or runtime semantics.

An English-authored library with five localization catalogs remains one library:

~~~text
canonical declaration: Deliveryˉpolicy.Isˉfree

zh-Hans source label: 配送ˉ政策.是否ˉ免运费
ja source label:      配送ˉ方針.送料無料ˉ判定
es source label:      Políticaˉdeˉentrega.Esˉgratis
fr source label:      Politiqueˉdeˉlivraison.Estˉgratuite
de source label:      Lieferˉrichtlinie.Istˉkostenlos
~~~

The localized labels above are illustrative rather than approved terminology.
Each qualified catalog requires native-language technical review.

All compiled references resolve to the one canonical declaration identity. A
localization catalog is a source dependency and development artifact, not a
runtime library, capability provider, duplicated export surface, or ABI alias.

**Canonical** means exact and stable, not inherently English. An
English-authored library normally has English canonical source names and may
publish Chinese, Japanese, or other catalogs. A Chinese-authored library may
have Chinese canonical Unicode names and publish an English source-vocabulary
catalog. Consumers in either direction resolve to the library's one canonical
public identity set.

## Universal source descriptor

Every edition-1 file begins with one exact language-neutral source descriptor:

~~~text
#!wv/1 en@1
~~~

A Simplified Chinese file begins:

~~~text
#!wv/1 zh-Hans@1

模块 结账;
~~~

The descriptor is file-format metadata rather than a localized Windvale
declaration. Its fields are:

| Field | Meaning |
| --- | --- |
| `#!wv` | Windvale source magic |
| `/1` | Source-language edition 1 |
| one U+0020 | Required separator |
| `zh-Hans` | Exact source-profile identity |
| `@1` | Immutable source-profile version 1 |

Every file names a profile explicitly; there is no ambient or omitted default.
The canonical English profile is `en@1`. A source profile binds one exact keyword
lexicon and one exact public source-vocabulary profile. A deliberately mixed
terminology policy, such as Chinese keywords with Japanese public labels,
requires its own named immutable composite profile rather than extra declarations
inside the file.

Strict source resolution is unconditional in edition 1 and therefore is not a
source option. The descriptor has no `strict` flag: a missing or stale selected
input always fails instead of falling back to another language.

The descriptor:

- is the first physical line with no byte-order mark, whitespace, or comment
  before it;
- contains ASCII only;
- is at most 128 bytes excluding its line ending;
- contains exactly one U+0020 separator and no other whitespace;
- ends at the first admitted LF or CRLF logical line ending;
- uses a 2-through-96-byte source-profile identity;
- uses a positive decimal profile version no greater than `4294967295`, with no
  sign, underscore, suffix, or leading zero; and
- is followed immediately by the ordinary localized or canonical module source.

The source-profile identity contains dot-separated components. Each component
contains one or more hyphen-separated atoms. An atom begins with an ASCII letter
and continues with ASCII letters or digits. Matching is exact and case-sensitive;
the profile registry fixes canonical spellings such as `en`, `zh-Hans`, and
namespaced community identities.

## Universal descriptor admission

Before the general lexer, a bounded descriptor reader:

1. validates strict UTF-8 without a byte-order mark;
2. requires `#!wv/` at byte zero;
3. reads the exact positive source edition, one U+0020, profile identity, `@`,
   profile version, and first logical line ending;
4. validates the descriptor length and narrow ASCII grammar;
5. resolves the immutable profile identity/version through explicit build input;
6. rejects an unknown, duplicate, malformed, unavailable, or hash-mismatched
   profile; and
7. returns the exact source byte offset at which localized lexing begins.

The descriptor reader does not load imports, search installed packs, normalize
source, invoke the general parser, or execute localization code. A malformed
descriptor produces one bounded front-door diagnostic before module parsing.

## Composite source profile

The descriptor identity/version resolves through explicit locked build input to
one content-addressed `.wvsp` artifact. Its semantic fields are:

~~~text
format                 windvale.source-profile/1
identity               en
version                1
source-edition         1
unicode-data           <identity>@<version>#<sha256>
token-registry         <sha256>
keyword-lexicon        <identity>@<version>#<sha256>
source-vocabulary      <identity>@<version>#<sha256>
~~~

The profile binds components; it does not embed executable code, library
catalogs, filesystem paths, URLs, installation searches, or locale fallback.
Public-library catalogs remain separately bound to exact interface hashes. The
profile selects their source-vocabulary identity/version, not an unbounded list
of libraries.

The [source-profile artifact format companion](Windvale-Language-1.0-Source-Profile-Formats.md)
defines the exact candidate byte serialization, record order, length bounds,
duplicate and unknown-record behavior, external whole-file content-hash
coverage, malformed-input behavior, and companion component formats. Workload 1
provides exact `en@1` and synthetic `test-Unicode@1` reference chains. Decision
0766 owner-accepts them as candidate evidence; they are not implemented
artifacts or independently qualified language packs.

## Source lexicons

### Canonical token identities

The source edition assigns stable ASCII identities such as `KW_MODULE`,
`KW_IF`, `KW_RETURN`, and `KW_TRUE` to reserved words. Those identities, not
English spellings or compiler-private integers, are the external lexicon
contract. The ordered token registry has one exact SHA-256 identity.

The edition-1 candidate maps the following 66 existing words:

~~~text
application as async authority await base bool borrow break bytes cancel_join
capability case const continue copy core data derive effects else enum export
fail_join false fn for foreign hosted if implement import in join let library
match module move mut never optional maximum package platform policy profile
protocol record requires return rune scope service system task text true try
unit unsafe using var variant version where
~~~

The source descriptor is not a keyword sequence. The ten fixed-width numeric
type spellings `i8`, `i16`, `i32`, `i64`, `u8`, `u16`, `u32`, `u64`, `f32`, and
`f64`, punctuation, operators, numeric syntax, package identities, capability
identities, effect identities, target scopes, ABI identities, and foreign
symbols remain canonical technical anchors. `version` remains one of the 66
profile-mapped body keywords.

### Complete exact mapping

One admitted lexicon contains exactly one primary source spelling for every
mapped token identity. It has no aliases or fallback language. A primary
spelling:

- is non-empty strict UTF-8 in the edition-selected normalization form;
- is no more than 128 UTF-8 bytes and 64 Unicode scalars;
- contains no source whitespace, delimiter, quote, comment marker, operator,
  control, noncharacter, private-use scalar, unassigned scalar, default-ignorable
  scalar, or bidirectional formatting control;
- is unique within the pack after exact validation;
- does not collide with a different universal or mapped token; and
- satisfies the edition's keyword-boundary rules.

The lexer performs exact ordinal matching. It never applies host case folding,
collation, transliteration, script guessing, or silent Unicode normalization. A
canonically equivalent but non-exact spelling is rejected with the exact
expected primary spelling.

A keyword is recognized only when its complete primary spelling is followed by
EOF, token whitespace, a comment start, or admitted punctuation, delimiter, or
operator boundary. U+02C9 and every admitted identifier continuation prevent
keyword recognition. A keyword is never accepted as a prefix of an identifier.

## Unicode source identifiers

### Admitted form

Edition 1 admits Unicode source identifiers so a program can store its own
module, declaration, field, case, parameter, alias, and local names in the
author's language. The candidate lexical form is:

~~~text
Identifierˉstart ::= "_" | Unicodeˉxidˉstart
Identifierˉcontinue ::= "_" | Unicodeˉxidˉcontinue
Identifierˉsegment ::= Identifierˉstart { Identifierˉcontinue }
Identifier ::= Identifierˉsegment { "ˉ" Identifierˉsegment }
Constantˉidentifier ::= Identifier
~~~

`Unicodeˉxidˉstart` and `Unicodeˉxidˉcontinue` refer to exact property tables
selected by the source edition. U+02C9 is excluded from both properties for this
grammar and remains the semantic-word separator. ASCII identifiers admitted by
the earlier candidate remain admitted.

Every identifier must already be in the edition-selected normalization form.
Identity is the resulting exact ordinal UTF-8 byte sequence; there is no case
folding, locale collation, transliteration, or canonically-equivalent alias.
One project-owned identifier contains at most 256 UTF-8 bytes, 128 Unicode
scalars, and 32 U+02C9-delimited semantic segments. Keyword and public-label
artifacts retain their stricter 128-byte/64-scalar bounds.

The edition-1 candidate selects `windvale.unicode17.source@1`: Unicode
17.0.0, NFC under UAX #15 revision 57, XID properties under UAX #31 revision 43,
and identifier security data under UTS #39 revision 32. Its exact upstream files
and SHA-256 values are recorded in the
[source-profile artifact format companion](Windvale-Language-1.0-Source-Profile-Formats.md).
A compiler using different host tables is not conforming. This identity remains
exact rather than meaning “current Unicode.” Workload 4 validated the required
scripts and the project owner accepted the edition-1 security boundary.

### Rejected identifier content

An identifier rejects:

- malformed UTF-8 or a byte-order mark;
- a scalar sequence not already in the specified normalization form;
- whitespace, pattern syntax, controls, surrogates, noncharacters, private-use
  scalars, unassigned scalars, and default-ignorable format scalars;
- bidirectional override, embedding, isolate, and pop controls;
- a leading digit or an empty segment;
- adjacent, leading, or trailing U+02C9 separators; and
- any scalar excluded by the edition's exact identifier security profile.

The candidate requires the UTS #39 Highly Restrictive script level or
the stricter ASCII-only level within each U+02C9-delimited semantic segment,
rejects mixed decimal-number
systems in one segment, and rejects distinct same-scope segments whose UTS #39
`bidiSkeleton` values collide under either left-to-right or right-to-left
processing. Workload 4 confirms that join controls remain excluded from edition
1. This prevents preferred spellings in some Persian, Arabic-derived, and Indic
orthographies; a later edition may change the rule only through exact contextual
validation, native-language workloads, new profile bytes/hashes, and a named
language decision. Tools may report additional visual warnings, but required
acceptance cannot depend on rendering.

### Identity and interoperability

A project-owned Unicode declaration name is its canonical source name. Exported
metadata retains that exact name where the owning format requires source
identity. Native and foreign symbols use a deterministic collision-safe
ASCII-only mangling contract; a Unicode spelling is never passed directly to a
platform ABI.

Machine namespaces remain ASCII-safe. Package identities, capability and effect
identities, target scopes, ABI identities, foreign symbols, protocol wire names,
database schema names owned by external formats, and other separately registered
machine keys do not become Unicode merely because source identifiers do.

Official naming retains U+02C9 between semantic concepts. A cased script uses
the edition's capitalized official style. An uncased script uses its reviewed
natural form and does not invent casing. The declaration category already makes
a constant distinguishable; uncased scripts do not need an artificial
`ALL_CAPS` transformation.

### Bidirectional source boundary

Windvale keeps one logical left-to-right grammar order. Arabic and Hebrew change
identifier or localized-token content, not declaration, expression, delimiter,
or argument order. UTF-8 logical order is source identity; editors render lexical
atoms rather than treating a source line as one undifferentiated bidi paragraph.

After the ASCII-only descriptor, one U+061C, U+200E, or U+200F implicit
directional mark may occur at a complete body-token/logical-line boundary. It is
semantically ignored but retained in raw-source hashes, spans, diffs, and
provenance. It cannot split a token. Other default-ignorables outside comment or
text/rune/raw-literal content are invalid.

Literal U+000B, U+000C, U+0085, U+2028, and U+2029 are rejected everywhere in
raw source so displayed hard lines cannot disagree with the LF/CRLF scanner.
Runtime text can express them through `\u{...}` escapes.

Literal U+202A..U+202E and U+2066..U+2069 stateful bidi controls occur only
inside one comment-content or text/rune/raw-literal-content atom. UAX #9 revision
51 processing must balance within that atom and logical line at nesting depth no
greater than 16. An unbalanced runtime value remains expressible through an
ASCII Unicode escape. Source display and control visibility follow UTS #55
version 2, revision 5 as detailed by
[Workload 4](../Documents/Project/Language-1.0-Localization-Workloads/04-Unicode-And-Multilingual-Security/README.md).

## Source-vocabulary profiles and catalogs

### Profile selection

The universal source descriptor selects one immutable composite source profile.
Its exact profile manifest binds a keyword lexicon identity/hash and one
source-vocabulary profile identity/version. The vocabulary profile identifies
the intended human-language terminology contract; it does not itself contain
every library's labels.

For every imported public interface used by the source, the resolved build plan
supplies the exact catalog matching:

- source-vocabulary profile identity and version;
- canonical package and module identity;
- public interface/signature-set SHA-256;
- source edition and Unicode-table identity; and
- exact catalog content SHA-256.

The compiler receives these catalogs as explicit bounded build inputs. It does
not download them, search a global installation, choose the newest version, or
fall back according to the host locale.

### Catalog contents

A complete catalog maps every source-addressable public item in one canonical
library interface to one primary localized source label. Source-addressable
items include:

- imported modules;
- public records, enums, variants, protocols, types, constants, and package
  data declarations;
- public functions and protocol operations;
- public fields, enum members, and variant cases; and
- public named parameters.

Documentation, summaries, alternate search terms, pronunciation, and extended
explanations may be shipped in separate non-semantic presentation data. They are
not records in the Language 1.0 source catalog. Only one primary label per
canonical identity is admitted as source.

A catalog is declarative data. It has no code, imports, macro expansion,
callbacks, compiler hooks, filesystem access, network access, environment
access, or other capability.

### Exact binding to the canonical API

Every label is keyed by an exact canonical declaration identity and the
catalog's public interface hash. If the library adds, removes, renames, moves, or
changes the signature of a source-addressable declaration, the old catalog does
not match the new interface and is rejected before source binding.

This rule prevents a translation for `Isˉfree` from silently attaching to a new
or incompatible operation. Publishing a corrected translation creates a new
immutable catalog version/content identity; it never changes already resolved
build input bytes.

The complete catalog must contain exactly the source-addressable identity set of
its bound interface. A missing, duplicate, stale, extra, or unknown identity is a
catalog-admission failure even when the current source does not mention it.

### Label rules and scope

A source label uses the edition-1 `Identifier` form and normalization/security
rules. Labels need be unique only within the canonical scope in which name
resolution compares them. The same localized word may name members of unrelated
types when ordinary canonical source could also reuse the corresponding name.

Within one owner scope, two canonical declarations cannot receive the same
primary label when that would make source resolution ambiguous. Language 1.0
has no inferred overload selection; a catalog cannot use result type, argument
type inference, or compiler preference to distinguish equal labels.

Catalog labels may preserve an established technical term unchanged. Such a
choice remains an explicit entry, not an implicit English fallback.

## Localized symbol resolution

After keyword mapping, the lexer emits Unicode `Identifier` tokens with exact
raw byte and scalar spans. Name resolution applies the selected source
vocabulary only at public-library boundaries.

Resolution proceeds by owner:

1. An import label is looked up among the exact localized module labels in the
   build plan's admitted dependency catalogs.
2. The import must identify exactly one canonical module. Zero matches produce
   `Unknownˉlocalizedˉmodule`; multiple matches produce
   `Ambiguousˉlocalizedˉmodule`.
3. The `as` name creates an exact project-owned local alias. It is not added to
   or inferred from the library catalog.
4. A qualified type, function, constant, field, or case label is looked up only
   in the canonical owner established by the preceding resolved expression or
   type.
5. Once a function or protocol operation is selected, each named-argument label
   is looked up in that exact declaration's parameter catalog.
6. Local variables, local parameters, private declarations, and project-owned
   public declarations resolve by their exact stored Unicode source names.

The resolver immediately lowers a localized public label to the canonical
declaration identity. Later semantic phases do not receive or compare the
localized spelling except through source-map and diagnostic provenance.

A dependency closure whose localized module labels collide is rejected. Language
1.0 has no project source-vocabulary override or implicit disambiguation format;
the compiler never resolves ambiguity by dependency or import order. A future
edition may add an exact source-level or build-input disambiguation contract only
through a separately versioned format, workload, and language decision.

## Strictness and fallback

Every source profile is strict:

- every imported public interface used by the module requires an exact matching
  complete catalog under the profile's vocabulary contract;
- only that catalog's primary labels are admitted for public source references;
- canonical names are not an implicit fallback;
- the canonical `en@1` profile uses catalogs whose primary labels are the
  canonical public source names;
- another catalog may explicitly choose a canonical spelling as its primary
  label; and
- missing or rejected catalogs fail compilation before full semantic analysis.

This strict rule keeps one deterministic spelling, prevents accidental mixed
language source, and ensures that copying the source plus its locked build inputs
reproduces the same resolution.

A later edition may define explicit per-import vocabulary selection, but
Language 1.0 uses only the descriptor's named composite profile and does not
infer or silently combine component profiles.

## End-to-end Chinese example

Assume the canonical English library exports:

~~~text
module Deliveryˉpolicy;

export fn Isˉfree(
    Orderˉtotal: u64,
) -> bool effects();
~~~

Its exact Simplified Chinese catalog maps:

~~~text
Deliveryˉpolicy                         -> 配送ˉ政策
Deliveryˉpolicy.Isˉfree                 -> 是否ˉ免运费
Deliveryˉpolicy.Isˉfree.Orderˉtotal     -> 订单ˉ金额
~~~

An application may store this source:

~~~text
#!wv/1 zh-Hans@1

模块 结账;
配置 核心;
平台 windows, linux, windvale;
权限 应用程序;

导入 配送ˉ政策 作为 配送;

导出 函数 检查ˉ免运费(
    订单ˉ金额: u64,
) -> 布尔 效应() {
    返回 配送.是否ˉ免运费(
        订单ˉ金额: 订单ˉ金额,
    );
}
~~~

The stored Chinese keywords map to canonical tokens. `配送ˉ政策`,
`是否ˉ免运费`, and the call-site named argument map through the exact library
catalog. `结账`, `配送`, `检查ˉ免运费`, and the function's own `订单ˉ金额`
parameter are project-owned Unicode identifiers.

The resolved call is exactly:

~~~text
canonical module: Deliveryˉpolicy
canonical declaration: Deliveryˉpolicy.Isˉfree
canonical named parameter: Orderˉtotal
~~~

The example still exposes canonical platform scopes and `u64`. Those are
portable machine contracts, not untranslated human prose.

## Diagnostics and source maps

Every localized keyword and public label retains:

- exact raw UTF-8 byte span;
- decoded scalar span where required by the diagnostic protocol;
- source lexicon or source-vocabulary catalog identity and hash;
- canonical token or declaration identity; and
- exact primary spelling expected by the selected input.

A diagnostic may present Chinese prose and source labels, but its stable identity
and structured fields remain canonical. An unknown localized library member, for
example, reports the localized spelling, owning canonical type/module identity,
catalog identity, and bounded candidate labels. Machine consumers never parse
localized prose.

Diagnostic locale is independent of source lexicon and source vocabulary. A
Chinese source file may produce English diagnostics for one developer and
Chinese diagnostics for another without changing success, artifact bytes, or
source-build identity.

## Deterministic conversion

Tooling may convert a file between source lexicons and source-vocabulary
profiles. Conversion:

1. admits the original file with its exact locked packs;
2. resolves every keyword and public source label to its canonical identity;
3. writes the target pack's one primary spelling for each resolved item;
4. updates the universal source descriptor;
5. preserves literals, comments, documentation, whitespace, and project-owned
   identifiers unless a separately requested operation changes them;
6. validates the target file and compares canonical token and declaration
   resolution; and
7. reports input/output hashes, pack hashes, and canonical semantic identity.

The safe default writes a distinct output path. Replacing the input is an
explicit operation that rechecks the original raw hash and uses atomic
same-filesystem replacement after complete target validation, or refuses when
that guarantee is unavailable. A conflict, failure, or cancellation preserves
the original and removes only the operation's exact private candidate.

Keyword and public-library conversion requires no AI. Project-owned identifiers
such as `检查ˉ免运费` remain Chinese unless an explicit semantic rename changes
them. Automatic natural-language translation of project names, comments,
strings, resources, schemas, or user data is outside compiler conversion.

Ordinary editor Copy returns exact stored source. Canonical-source and displayed-
view copy are separately named operations with visible provenance. Paste never
guesses a profile from script or host locale; different-profile conversion
requires trusted bounded provenance and an explicit user action. The formatter
retains the file's selected profile and primary spellings. Semantic rename
changes project-owned declarations/references by identity; an imported library
label can change only through a new reviewed catalog plus explicit consumer
conversion.

## Artifact and cache identity

Two files that differ only in admitted keyword and public-library source labels
produce the same canonical tokens and resolved public declaration identities.
With identical project-owned identifiers, literals, imports, options,
dependencies, target, and compiler, they produce byte-identical semantic WIR,
WVB, object, and executable sections.

Raw source, source maps, and optional debug provenance may differ. They retain
the source-profile identity/version/manifest hash, component lexicon and
vocabulary identities, exact catalog hashes, and raw source hash needed to
reproduce diagnostics and debugging.

The safe initial compiler-cache key contains:

- raw source hash;
- source edition and compiler identity;
- composite source-profile identity, version, and manifest hash;
- source-lexicon identity and content hash;
- source-vocabulary profile identity and version;
- every applicable catalog content hash;
- dependency/interface hashes;
- options; and
- target identity.

A later optimized cache may share semantic evidence across localized forms only
when canonical token/declaration hashes match and request-owned source maps,
debug provenance, and diagnostics are regenerated correctly. A simple
non-sharing correctness oracle remains required.

## Pack bounds and shipment

The exact artifact companion fixes these candidate limits:

- at most 64 KiB for one source lexicon;
- at most 64 KiB for one composite source-profile manifest;
- at most 64 KiB for one Unicode profile, keyword-token registry, or
  source-vocabulary profile;
- exactly the token registry's mapped entry count;
- at most 128 UTF-8 bytes and 64 scalars per primary source label;
- at most 1 MiB for one public-library source-vocabulary catalog;
- at most 1 MiB for one source-input lock;
- at most 65,536 source-addressable identities per catalog;
- at most 96 bytes for an ASCII-safe component identity;
- at most 1,024 UTF-8 bytes for one artifact record, excluding LF;
- at most 64 catalogs per source module; and
- one bounded rejection diagnostic plus a fixed small set of structured related
  fields per malformed pack.

Later language decisions may reduce these limits or justify a separately bounded
large-interface profile. They may not leave size, count, normalization work,
collision work, diagnostic expansion, or retained compiler-service state
unbounded.

Localization artifacts use Windvale's existing immutable content-addressed
package/bundle/store/generation architecture. A runtime-only installation
selects none of them. A minimal developer installation selects the shared
edition objects and `en@1`; `zh-Hans@1`, localized diagnostics, and localized
documentation are independently selected optional logical packages. The host
locale may offer a visible recommendation but cannot silently select a package,
source profile, or semantic input.

Exact SHA-256 content identity deduplicates shared Unicode and token-registry
objects across profiles, installations, and rollback generations. An update
publishes a new immutable profile version and installation generation; it never
rewrites a previously selected profile in place. Offline compilation resolves
only the descriptor and exact content hashes already present in the explicit
build lock. Compiled runtime products carry no source-localization object unless
they deliberately include development tooling. Five library localizations
therefore do not duplicate executable code or enlarge ordinary application
runtime packages.

The first implementation uses three bounded compiler-service cache layers:
validated artifacts keyed by `(format, SHA-256)`, validated composite profiles
keyed by all ordered component/interface hashes, and source results keyed by raw
source plus compiler, profile/catalog, dependency, option, and target identity.
One service generation hashes and parses each distinct localization object at
most once, publishes immutable entries through single-flight construction, and
retains no request-owned spans, diagnostics, or durable negative admissions.
Eviction affects performance only; a request can fall back to private bounded
validation without changing semantic acceptance.

The working Release 1 distribution target contains two official source profiles:
`en@1` and `zh-Hans@1`. This is shipment policy rather than a semantic minimum.
The Chinese profile cannot become official until native review, executable
equivalence, security, cross-host, and installer evidence passes. Additional
qualified profiles may ship in a later 1.0.x release without changing the
language, compiler pipeline, WVB, or runtime contract.

## Required conformance workloads

The [localization workload plan](../Documents/Project/Windvale-Language-1.0-Localization-Workloads.md)
groups the following evidence into five bounded bundles. Workload 1 has an
owner-accepted first-author packet with 25 accepted cases, 43 rejected cases,
exact reference artifacts, and a synthetic Unicode source-equivalence fixture.
Workload 2 has a complete first-author `zh-Hans@1` artifact, terminology,
paired-source, and equivalence packet whose technical/design findings are owner
accepted, but native review and executable evidence remain open. Workload 3 has
an owner-accepted paper contract, 30 accepted cases, 30 rejected cases, and
three exact expected-source fixtures; implementation/editor qualification
remains open. Workload 4 has an owner-accepted Unicode/security contract, 32
accepted cases, 46 rejected cases, exact Unicode-17 validation, and one
multilingual source fixture; implementation/editor/cross-host qualification
remains open. Workload 5 has an owner-accepted paper contract, 34 accepted cases,
42 rejected cases, an exact 12,288-byte two-profile fixture inventory, and
bounded shipment/cache/cross-host measurement protocols; implementation and
measured qualification remain open.
The replacement source-freeze candidate requires the owner-reviewed paper
contracts above: exact `en@1` and synthetic Unicode admission chains, the
complete draft `zh-Hans@1` chain and Chinese paired source, deterministic three-
profile conversion outputs, the cross-script Unicode/security matrix, and the
shipment/cache/cross-host protocol. It does not require several additional
natural-language packs merely to prove the one generic mechanism.

The first implementation and release conformance program must then make these
paper contracts executable:

1. exact profile, component, catalog, lock, and descriptor admission plus every
   named malformed/rejected case;
2. the fully Chinese stored application using localized module, member, case,
   and named-parameter labels plus Chinese project identifiers;
3. deterministic localized-to-canonical and between-profile conversion;
4. byte-identical canonical token, resolved-declaration, WIR, WVB, object, and
   executable semantic sections where only admitted localized labels differ;
5. normalization, confusable, mixed-script, mixed-number, invisible, bidi,
   boundary, prefix, malformed UTF-8, and adversarial lookup cases;
6. public-library change cases proving stale vocabulary cannot bind to a changed
   declaration or parameter;
7. editor, formatter, copy/paste, search, rename, source-map, debugger, and Git
   semantic-review cases;
8. Windows and Linux cross-host equality using the exact pinned Unicode and pack
   bytes;
9. bounded compiler time, allocation, retained state, cache, and diagnostics
   measured against canonical source; and
10. for each officially shipped natural-language profile, the explicit draft,
    native-reviewed, qualified, and officially-distributed gates.

## Required failures

Compilation fails before artifact publication for:

- malformed, unknown, unsupported, unavailable, or hash-mismatched source
  profile identity/version/manifest;
- wrong descriptor edition, spacing, delimiter, line ending, length, or trailing
  content;
- absent exact pack input;
- declared identity, content hash, source edition, token registry, Unicode table,
  or interface hash mismatch;
- incomplete or ambiguous token or public-label mapping;
- implicit canonical fallback under a selected source profile;
- a non-normalized, forbidden, malformed, oversized, or colliding spelling;
- an unresolvable or ambiguous localized import/member/field/case/parameter;
- a project identifier exceeding 256 UTF-8 bytes, 128 scalars, or 32 segments;
- a forbidden raw-source hard-line scalar, misplaced/redundant implicit
  directional mark, cross-token stateful bidi control, unbalanced content-atom
  control stack, or content-atom bidi nesting greater than 16;
- source whose universal descriptor is absent from byte zero or exceeds its
  bound;
- use of host locale, collation, case folding, Unicode tables, filesystem search,
  installation order, or network resolution as semantic input; or
- any inability to preserve canonical semantic identity during conversion.

No failure falls back to another human language, another installed pack, or the
canonical public name unless the selected exact catalog explicitly uses that
name as its primary spelling.

## Replacement-freeze conditions

This addendum can enter the replacement Language 1.0 freeze only after:

- the Workload 1 exact artifact and Unicode-profile candidates are owner
  accepted;
- the universal descriptor and source-profile/component serializations remain
  exact after the remaining workloads;
- human and machine grammar projections agree;
- all required workloads and rejected cases are reviewed;
- the Foundation registry and all paper programs are reconciled with the new
  source boundary;
- editor, formatter, diagnostic, conversion, package/build, cache, shipment, and
  cross-host ownership is explicit;
- the exact front-door measurement protocol and structural time/memory/cache
  bounds are accepted, with numeric host ceilings deferred to first-
  implementation qualification; and
- a replacement manifest records every normative input and exact hash.

Until then, this file defines the replacement candidate contract. It does not
authorize compiler implementation or alter the implemented Seed language.
