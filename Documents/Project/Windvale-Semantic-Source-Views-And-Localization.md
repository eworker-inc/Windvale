# Windvale semantic source views and localization exploration

## Status

This product and architecture exploration began on 2026-08-17 and was updated
on 2026-08-18. The project owner selected stored localized keyword source—Model
C below—then selected stored localized references to canonical public library
declarations and Unicode project identifiers so the raw program body can remain
in the author's language. All five workload findings and the resulting scope
decisions are accepted by
[Decision 0766](../Decisions/0766-Complete-Language-1.0-Localized-Source-Reconciliation.md).
The normative-candidate rules now live in the
[localized-source specification](../../Specifications/Windvale-Language-1.0-Localized-Source.md).
This exploration remains rationale rather than a replacement freeze,
implementation claim, or authorization to change the compiler.

The preserved Language 1.0 candidate remains historical evidence of one
canonical ASCII-keyword source. It is not frozen. The replacement candidate now
includes the exact source-lexicon mechanism, Unicode source identifiers, and
exact public-library source-vocabulary catalogs after the five workloads
resolved their grammar, identity, Unicode, tooling, security, shipment, cache,
and reproducibility boundaries.

### Current freeze state

The preserved source-freeze candidate manifest is 2,700 bytes with SHA-256
`152d7ae3b8463b395d42937b4271f757bb921d16046fd78354c7b0821c2b0099`.
Its exact inputs remain retrievable at repository revision
`c060cb3553a06ed97c4b42d751534f7f4bcaa62e`. It remains reproducible evidence of
the design before source localization, but the project owner has directed that
it not be frozen. This working document does not mutate that identity. Decision
0766 instead produces the 3,702-byte
[`Windvale-Language-1.0-Replacement-Source-Freeze-Candidate.txt`](Windvale-Language-1.0-Replacement-Source-Freeze-Candidate.txt)
with SHA-256
`c9517841eae6b6e86778cb1dd88711feb38929dec8fe79e084eec44fa22c512a`
and its own exact 250-input identity.

## Product idea

Windvale should explore one exact program that can be read through several
human-language views without creating several semantic languages.

The strongest reason is not faster keyword entry. As AI writes more code,
people who did not author that code will increasingly inspect, approve, audit,
and explain it. Source becomes both compiler input and a human-facing account of
what a machine will do. Windvale can make that account more accessible while
retaining one reproducible program identity.

The product principle is:

> One semantic language, one canonical identity system, and many explicit
> human-language representations.

Canonical means exact and stable, not necessarily English. An English-authored
library can publish Chinese source labels over English canonical identities; a
Chinese-authored library can retain Chinese canonical Unicode identities and
publish an English source vocabulary over them.

This is broader than translating `if`. A useful view may localize language
constructs, diagnostics, API descriptions, parameter labels, and project
vocabulary while the compiler, linker, package manager, and runtime continue to
use canonical token and symbol identities.

## Desired properties

A successful design should provide:

- one compiler architecture, parser, type system, WIR, WVB contract, runtime,
  and package identity system;
- plain strict UTF-8 source that remains usable through ordinary editors,
  Git, command-line tools, recovery environments, and future tools;
- deterministic localized rendering and input with no AI required for keyword
  conversion;
- exact canonical identities beneath every translated library or project label;
- localized diagnostics that retain stable machine-readable identities and
  exact stored-source spans;
- no ambient dependence on host language, keyboard, region, timezone, or
  process locale;
- bounded, data-only packs that grant no capability and execute no code;
- secure behavior for normalization, confusables, invisible characters,
  bidirectional text, malformed input, and oversized packs;
- no runtime or generated-code cost, and only measured bounded compiler
  front-door cost for source lexicons; and
- independently installable localization content so a small compiler or
  deployment does not carry every language's documentation.

## Non-goals

This proposal does not imply:

- separate Spanish, Chinese, Japanese, Arabic, or other Windvale semantics;
- natural-language parsing or a different grammar for each language;
- automatic translation of string literals, user data, protocols, capability
  identities, ABI names, paths, or external schemas;
- hidden translation of application-visible text;
- accepting multiple keyword languages in one source file;
- depending on one Windvale-specific editor to compile any valid `.wv` file;
- treating a script such as Latin or Cyrillic as one human language;
- trusting AI output as an approved technical translation;
- putting all localization packs into every compiler, package, application, or
  operating-system image; or
- changing parser, type, ownership, effect, capability, WIR, WVB, native, or
  runtime semantics merely because a different keyword lexicon is selected.

Application localization is a separate concern. A localized compiler view does
not make an application's menus, dates, numbers, messages, protocols, or data
locale-sensitive. Foundation's portable numeric parsing and formatting remain
invariant unless an application explicitly invokes a separately specified
locale library with an explicit locale value and resource owner.

## Terminology

This exploration uses the following terms:

- **stored source**: the exact repository bytes and explicitly selected source
  profile accepted by a named Windvale source edition;
- **canonical-lexicon source**: deterministic conversion of recognized keyword
  tokens to the edition's explicit English `en@1` profile while preserving all other
  source content;
- **canonical token**: the parser-facing identity of a keyword, punctuation
  item, literal, or identifier independent of its display;
- **canonical declaration identity**: the edition-aware package, module,
  declaration category, and source name defined by the language specification;
- **presentation locale**: the human language selected for an editor, review,
  diagnostic, documentation, or explanation view;
- **keyword lexicon**: a bounded mapping from canonical keyword token IDs to
  human-language spellings;
- **source lexicon**: a keyword lexicon that the working Language 1.0 design
  allows the compiler to recognize in stored source after exact descriptor and
  profile admission;
- **source-vocabulary profile**: one file-selected human-language terminology
  contract used to resolve stored localized public-library source labels;
- **source-vocabulary catalog**: a complete immutable mapping from one exact
  public library interface's canonical identities to one profile's primary
  source labels;
- **display catalog**: a mapping from canonical token or symbol identities to
  non-semantic labels and descriptions;
- **diagnostic catalog**: bounded message templates keyed by stable diagnostic
  identity;
- **library vocabulary catalog**: labels, parameter labels, summaries, and
  documentation keyed by exact public declaration identities;
- **project vocabulary catalog**: optional display labels for declarations
  owned by one application or project;
- **semantic source view**: a projection of admitted source, canonical tokens,
  and resolved identities into one presentation locale; and
- **pack**: one versioned, content-identified collection of one or more of these
  data-only catalogs.

The term **locale** describes presentation selection. It must not imply ambient
host behavior. The term **lexicon** is narrower: it describes source-token
spellings. Keeping them distinct avoids one vague `localization` switch that
changes unrelated behavior.

## Localization planes

Localization is not one all-or-nothing feature. At least six independent planes
exist:

| Plane | Candidate content | Semantic? | Typical owner |
| --- | --- | --- | --- |
| Keyword presentation | `if`, `return`, `module`, `using` | No | Language tooling |
| Localized input | Human spelling converted to canonical keyword token | No when conversion occurs before storage | Editor/input tooling |
| Stored source lexicon | Alternative keyword spellings accepted from `.wv` | Yes, lexical | Source specification and compiler |
| Diagnostics | Message template, explanation, suggested repair | No | Compiler/tool owner plus translator |
| Stored source vocabulary | Imported module/type/function/field/case/parameter labels | Yes, name resolution to canonical public identities | Source specification, library owner, compiler, and translator |
| Library display vocabulary | Type/function/field/case/parameter labels and docs | No when view-only | Library owner plus translator |
| Project vocabulary | Application declaration labels and explanations | No when view-only | Project owner |

Documentation, examples, tutorials, and AI-generated explanations form another
delivery layer over these planes. They can evolve without changing compilation.

Support should therefore be reported precisely. “Windvale supports language X”
is too vague. A support record should say, for example:

~~~text
locale: bg
keyword-view: qualified
localized-input: experimental
compiler-source-lexicon: working-design
compiler-source-vocabulary: working-design
unicode-source-identifiers: working-design
diagnostics: partial
foundation-vocabulary: draft
documentation: partial
native-review: named reviewer and revision
~~~

## Four source-storage models

### Model A: canonical source with localized presentation

The `.wv` file stores canonical edition-1 text. A Windvale-aware editor lexes
the file and renders recognized tokens using the selected presentation locale.

~~~text
canonical UTF-8 source -> canonical token -> localized glyph sequence
~~~

Advantages:

- no compiler, grammar, artifact, cache, Git, or package change;
- external editors and command-line tools always see valid source;
- one canonical diff and one exact file hash;
- changing presentation locale cannot change program behavior; and
- the experiment can begin after Language 1.0 without reopening it.

Costs:

- the displayed text may differ from copied or stored text;
- cursor, selection, search, screen-reader, and accessibility behavior need
  exact rules;
- Git hosting sites show canonical rather than localized source unless they add
  the view; and
- an editor must make the active presentation locale unmistakable.

### Model B: localized input committed as canonical source

The user types or selects a localized keyword. An editor input layer recognizes
one complete lexical item and commits the corresponding canonical spelling to
the source buffer. The editor may continue displaying the localized form.

~~~text
IME or keyboard -> localized candidate -> canonical token -> canonical source
~~~

Advantages:

- people need not type English keywords;
- stored files remain universal and compiler-independent;
- conversion is deterministic and does not require machine translation; and
- Model A and Model B can share the same token catalog.

Costs:

- incomplete input and identifier intent must remain editable and unsurprising;
- undo, paste, autocomplete, and token commitment need exact behavior; and
- a person using an ordinary editor cannot type localized keywords directly.

### Model C: stored localized keyword source

The `.wv` file contains the localized spellings, and an explicitly selected
source lexicon maps them to canonical parser tokens.

~~~text
localized UTF-8 source -> selected source lexicon -> canonical token stream
~~~

Advantages:

- what a person sees is what the file stores;
- any text editor can edit the localized spellings; and
- Git and code-hosting sites display the authored language.

Costs:

- source parsing depends on one additional versioned input;
- source-profile admission, file selection, caches, formatters, LSP behavior,
  source locations, conversion, and cross-host qualification become language
  contracts;
- every admitted lexicon expands malformed-input and tooling coverage;
- localized keywords can collide with broader Unicode identifiers;
- source copied without its exact lexicon can become ambiguous or invalid; and
- two text files with the same canonical token stream have different raw hashes
  and possibly different debug/source provenance.

### Model D: tokenized or structured source as the primary file

The source stores semantic token IDs rather than ordinary text and every editor
renders a selected language.

This model makes multilingual views natural but would make ordinary text tools,
recovery, simple diffs, source archives, and independent implementations harder.
It is not recommended as Windvale's primary source format. A structured editor
cache may exist, but stored source should remain plain UTF-8 text.

## Selected working source model

Language 1.0 will now explore Model C as a first-edition feature. Models A and B
remain complementary editor behaviors: a localized source file can still be
viewed through another lexicon, and localized input can still commit exact
source tokens. Model D remains rejected as the repository and interchange
format.

The design must retain one semantic compiler beneath the source lexicon:

~~~text
                    source in one declared lexicon
                               |
              lexicon-aware lexer -> canonical tokens
                               |
           +-------------------+-------------------+
           |                   |                   |
      parser/compiler     canonical view      another local view
      canonical IDs       converted tokens    converted tokens
~~~

The preserved candidate manifest must not be approved as the Language 1.0
freeze identity. Once this proposal and its workloads are accepted, the
normative grammar, semantic specification, Foundation/tooling boundaries,
migration plan, paper corpus, decisions, and manifest must be regenerated as one
replacement candidate.

## How one localized source file works end to end

The intended product flow is concrete even though the exact serialization and
spellings remain under review.

### 1. The author chooses the file's source profile

A new file begins with one explicit immutable source profile. The choice belongs
to the file, not to the author's operating system. An editor may offer a friendly
selector, but the stored result is plain UTF-8 text:

~~~text
#!wv/1 <locale>@1
~~~

The profile binds the exact keyword lexicon and public source-vocabulary profile.
The rest of the file stores their exact primary spellings. Identifiers, strings,
comments, package data, capability identities, and external protocol names are
not translated as a side effect.

### 2. The project resolves an exact pack before compilation

The descriptor names a logical immutable profile contract. The project's
resolved build inputs bind that identity/version to one exact profile-manifest
hash, which in turn binds the lexicon, vocabulary profile, Unicode data, token
registry, and catalog inputs. The compiler does not search the network, inspect
the host locale, or select the newest installed translation. `en@1` is explicit
like every other profile; it is not an ambient fallback. Moving the project to
Windows, Linux, or Windvale OS therefore cannot change the admitted words.

### 3. A tiny universal reader admits the descriptor

Before the general lexer runs, a fixed reader validates strict UTF-8 without a
byte-order mark and reads only the first-line
`#!wv/<edition> <profile>@<version>` shape. It returns one resolved immutable
profile or one bounded front-door diagnostic.

This reader does not understand localized keywords. Its purpose resembles a
file magic/version header: it learns how the rest of the source must be read.
The descriptor begins at byte zero, is ASCII-only, contains no comments or
optional whitespace, and ends at the first logical line ending within its
128-byte bound.

### 4. The selected pack becomes one immutable lexer table

The compiler validates the pack identity, hash, source edition, token-registry
hash, normalization rule, complete token mapping, duplicate/collision rules,
and size limits. A compiler service validates one exact pack once per service
generation and shares the resulting immutable lookup table across eligible
requests.

Pack failure stops before the module parser, imports, type checker, optimizer,
or code generator run. A pack never executes code and receives no filesystem,
network, environment, clock, or other capability.

### 5. Localized words disappear at the token boundary

For source such as:

~~~text
<localized-if> Orderˉtotal > 200u64 {
    <localized-return> true;
}
~~~

the lexer emits the same identities as canonical source:

~~~text
KW_IF IDENTIFIER GREATER_THAN U64_LITERAL LEFT_BRACE
KW_RETURN KW_TRUE SEMICOLON RIGHT_BRACE
~~~

Each token also retains the exact raw-source span needed for diagnostics,
formatting, debugging, and conversion. The parser receives `KW_IF` rather than
the localized string. Type checking, ownership, capabilities, WIR, WVB, native
lowering, linking, and runtime execution therefore have no human-language
branch.

### 6. Localized library labels resolve to exact canonical symbols

Calling a Foundation or application declaration may store the selected
source-vocabulary catalog's primary label. The catalog binds each module, type,
function, field, case, and named-parameter label to one exact canonical
declaration identity and public signature-set hash. A translation therefore
cannot silently attach to a changed API.

For Language 1.0, the working boundary is:

- localized keywords may be stored source;
- localized public library, field, case, and parameter labels may be stored
  source;
- the resolver lowers those labels to existing canonical identities before
  ordinary semantic analysis;
- project-owned Unicode identifiers use their exact stored names;
- capability, effect, platform, ABI, package, protocol, and other registered
  machine identities remain canonical; and
- no catalog creates a duplicate export, overload, runtime alias, or alternate
  implementation.

This provides fully localized source bodies without creating incompatible
library or package ecosystems.

### 7. Diagnostics combine local prose with machine evidence

The compiler reports the raw span from the stored source and one stable
diagnostic identity. A diagnostic catalog may render the explanation in the
reader's selected locale, while machine-readable fields retain exact token,
declaration, capability, type, bound, and failure identities.

The diagnostic locale is request state, not source semantics. Two developers
can compile the same localized file and receive explanations in different
languages without changing source, cache identity, or output bytes. A missing
or rejected diagnostic catalog falls back to the canonical message; it never
changes whether compilation succeeds.

### 8. Git stores the author's chosen source lexicon

Git hashes and diffs the exact localized source bytes. A review tool may add a
canonical-lexicon or reviewer-localized semantic view, but it must reveal the
stored patch and pack identities. Deterministic conversion can rewrite
recognized keyword tokens and resolved public-library labels to another exact
profile and then prove that canonical token and declaration identities are
unchanged.

This means a team can choose one source lexicon per file, mix files that use
different lexicons in one project, or convert a file deliberately. A single file
never mixes keyword lexicons implicitly.

### 9. Equivalent source produces equivalent semantic artifacts

If two source files differ only in their lexicon declaration and keyword
spellings, their canonical token-stream hashes match. With the same identifiers,
literals, imports, target, options, compiler, and other inputs, their semantic
WIR, WVB, object, and executable sections must be byte-identical.

Raw source hashes, source maps, and optional debug provenance remain different
because they must point back to the actual file a developer edited. Cache reuse
starts conservatively with raw source plus pack identity and may later share
semantic work only where source-map correctness remains proven.

### 10. Installation and runtime stay small

The SDK can install source lexicons, diagnostic catalogs, vocabulary catalogs,
and documentation per requested language. Compiled applications do not carry
compiler source lexicons unless they deliberately include developer tooling.
The runtime sees canonical artifacts and pays no keyword-localization cost.

An official-language release therefore has separate evidence for source
recognition, diagnostic quality, Foundation vocabulary, documentation, editor
behavior, Unicode/security, and native review. A community pack can be a valid
explicit source input without falsely claiming all of those support levels.

## Working Language 1.0 localized-source contract

This section turns Model C into a concrete candidate that workloads can attack.
Every spelling and bound remains subject to review, but the architecture should
not remain vague.

### Universal source descriptor

Every file begins at byte zero with one language-neutral descriptor:

~~~text
#!wv/1 en@1
~~~

A localized file selects its composite source profile in the same shape:

~~~text
#!wv/1 bg@1
~~~

The candidate grammar is:

~~~text
Source ::= Sourceˉdescriptor Localizedˉmoduleˉbody EOF
Sourceˉdescriptor ::= "#!wv/1" " " Sourceˉprofile "@"
                      Sourceˉprofileˉversion Sourceˉdescriptorˉend
~~~

The descriptor is ASCII-only, at most 128 bytes excluding its line ending, and
contains a 2-through-96-byte case-sensitive profile identity plus a positive
32-bit decimal version. It has no optional fields or omitted default. The bounded
reader does not use a localization pack, host locale, general parser, import
resolver, or filesystem search.

After profile admission, the bound lexicon controls all mapped edition-1 keyword
tokens. The bound vocabulary profile controls exact localized references to
public library symbols after interface-catalog admission. The module declaration
and remaining source can therefore store localized keywords, public API labels,
and Unicode project identifiers.

Keeping this descriptor universal has three advantages:

- any tool can identify Windvale, edition, and composite source profile
  without guessing;
- a copied source file remains self-describing; and
- malformed or unavailable profiles, lexicons, and vocabulary inputs fail
  before full semantic analysis or large allocation.

The descriptor is file-format metadata rather than ordinary program prose.

### Composite source-profile manifest

The descriptor identity/version resolves through the locked build inputs to one
content-addressed manifest. Its working logical fields are:

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

This is a field model, not yet the frozen byte serialization. The replacement
freeze must choose exact record order, encoding, length bounds, duplicate and
unknown-record behavior, and external content-hash coverage. Public-library catalogs
remain separately bound to their exact interface hashes; the profile manifest
selects their terminology profile, not an unbounded list of libraries.

`en@1` is the canonical English profile and preserves the paper corpus after its
descriptor migration. It is still explicit. No formatter, editor locale, host
locale, or missing input may insert or infer it.

### One source profile per file

The descriptor fixes one lexicon and one source-vocabulary profile for the
remainder of the file. There is no nested, scoped, per-declaration, per-line, or
automatic change. Imports may bind modules whose source used another profile
because compiled interfaces expose canonical identities.

Canonical English keyword spellings are not an implicit fallback inside a
non-English file. A pack may deliberately choose the same spelling for a
technical token, but that choice is an exact entry in that pack. This prevents
mixed-language source and guarantees one formatter output.

Comments and text, rune, byte, raw-text, foreign-symbol, capability, effect,
target, ABI, package-data, and resource literals do not participate in keyword
translation.

### Source lexicon identity and exact binding

The source declaration names a stable ASCII-safe lexicon identity and positive
version. That pair identifies an immutable spelling contract. Once published,
its token spellings, normalization, boundary rules, locale metadata, and target
token-set hash never change.

The build or package plan binds the declared pair to one exact content hash.
For an official lexicon, the Language release registry publishes that hash. A
source archive or package that depends on a non-base lexicon carries or
references the exact pack as an ordinary content-addressed build input.

Compilation refuses:

- an unknown identity or version;
- a missing exact artifact;
- a content hash different from the binding;
- a pack for a different source edition or token-set identity;
- two artifacts claiming the same immutable identity/version with different
  bytes; or
- a pack whose validated internal identity differs from the declaration.

No compilation request downloads a lexicon implicitly. Installation and package
resolution happen before the compiler receives the bounded artifact.

### Canonical token registry

The language specification should assign one stable ASCII token identity to
every reserved word. The external pack format uses identities such as:

~~~text
KW_APPLICATION
KW_AS
KW_ASYNC
KW_AUTHORITY
KW_IF
KW_RETURN
KW_UNSAFE
KW_USING
~~~

These names, rather than numeric parser implementation values or English
spellings, are the pack contract. The complete ordered registry has its own
SHA-256 identity. Adding or removing a token requires a new source edition and
new registry identity.

Compiler implementations may lower those external identities to compact
internal numbers after validation. Internal numbering is not serialized in the
pack contract unless a later format explicitly says otherwise.

### Permanently universal source items

The working proposal divides the 76 body reserved words as follows:

| Class | Working rule |
| --- | --- |
| `#!wv/1 <profile>@<version>` descriptor | ASCII file metadata, not body keywords |
| `i8`, `i16`, `i32`, `i64`, `u8`, `u16`, `u32`, `u64`, `f32`, `f64` | Always canonical |
| Punctuation, operators, numeric digits/suffix widths, and edition numbers | Always canonical |
| Capability, effect, platform, ABI, package, and external registered identities | Always canonical data tokens |
| Remaining structural, ownership, safety, type, profile, authority, Boolean, and control-flow keywords | Complete selected-lexicon mapping |

This leaves 66 edition-1 keyword identities under the selected lexicon plus ten
fixed-width numeric type spellings. The descriptor contributes no keyword.
`version` remains among the 66 because it appears in ordinary module metadata.

The exact working mapped set is:

~~~text
application as async authority await base bool borrow break bytes cancel_join
capability case const continue copy core data derive effects else enum export
fail_join false fn for foreign hosted if implement import in join let library
match module move mut never optional maximum package platform policy profile
protocol record requires return rune scope service system task text true try
unit unsafe using var variant version where
~~~

A lexicon may map a technical token to the same ASCII spelling as the canonical
pack when native reviewers judge that spelling preferable. It still supplies an
explicit entry, so completeness is machine-checkable.

Whether `bool`, `bytes`, `text`, `rune`, `unit`, `never`, `core`, `hosted`, and
`system` receive translated primary spellings should be decided by native
technical review. The mechanism admits them; an individual pack can retain the
canonical term.

### One exact primary spelling

Each of the 66 mapped token identities has exactly one primary source spelling.
The spelling:

- is non-empty strict UTF-8;
- is in the specified normalization form;
- contains no ASCII or Unicode whitespace;
- contains no source delimiter, quote, comment marker, operator, control,
  noncharacter, private-use scalar, or bidi formatting control;
- is no more than the admitted UTF-8 byte and Unicode-scalar limits;
- is unique within the pack after exact normalization;
- does not equal a permanently universal token with a different identity; and
- passes the pack's keyword-boundary and collision audit.

The compiler has no aliases or secondary spellings. An editor catalog may list
search terms, abbreviations, or alternate translations, but committing any of
them writes the one primary spelling.

### Lexical boundary algorithm

After descriptor and profile admission, the lexer processes exact decoded scalars and source
bytes while retaining both spans. At a position that may begin a keyword or
identifier, it performs this bounded operation:

1. test the selected lexicon's immutable lookup structure for a primary
   spelling beginning at the current byte;
2. test the canonical identifier grammar at the same position;
3. choose a keyword only when the complete primary spelling matches and the
   next scalar is EOF, whitespace, a comment start, or an admitted punctuation,
   delimiter, or operator boundary;
4. otherwise choose a complete canonical identifier when its grammar matches;
5. otherwise emit one bounded invalid-source-scalar or invalid-token diagnostic
   at the first undecodable or unclassified span; and
6. never accept a keyword as a prefix of a longer source word.

The boundary rule deliberately requires visible lexical separation after CJK
and other non-ASCII keyword spellings. A source sequence that concatenates a
keyword directly with a name is rejected rather than split by guesswork.

Canonical edition 1 already specifies that a keyword followed by U+02C9 is not
a keyword prefix. The selected-lexicon rule generalizes that protection: U+02C9
and every canonical identifier continuation prevent keyword recognition.

Lookup must be independent of host case folding and collation. No case-insensitive
or locale-sensitive comparison is allowed. A pack may use uppercase, lowercase,
uncased, or mixed-script text only as explicitly admitted by its reviewed exact
entry.

The lexer does not silently normalize source text. A keyword spelling must have
the exact admitted scalar sequence and UTF-8 representation of its pack entry.
A canonically equivalent but differently encoded sequence receives a specific
noncanonical-keyword diagnostic and suggested exact spelling; it never becomes
a hidden source rewrite.

### Candidate bounded pack format

A first pack format should be deliberately boring, line-oriented, and easy to
reimplement. An illustrative record is:

~~~text
windvale-source-lexicon 1
identity|windvale.source.es|1
locale|es
source-edition|1
token-registry-sha256|<64 lowercase hexadecimal digits>
normalization|NFC
unicode-data|<edition-specified Unicode table identity>
entries|66
token|KW_APPLICATION|<exact UTF-8 spelling>
token|KW_AS|<exact UTF-8 spelling>
...
token|KW_WHERE|<exact UTF-8 spelling>
~~~

This is a design sketch, not a frozen serialization. Before adoption it needs
an exact escaping rule or, preferably, a proof that admitted primary spellings
cannot contain the ASCII field separator or line controls.

Candidate hard bounds are:

- at most 64 KiB for one source-lexicon artifact;
- exactly the token count required by its registry, 66 for this proposal;
- at most 128 UTF-8 bytes and 64 Unicode scalars per primary spelling;
- at most 128 metadata records;
- no duplicate metadata keys or token identities;
- no unknown mandatory record; and
- one bounded diagnostic plus a fixed small set of related fields for pack
  rejection.

The source edition must name one exact Unicode scalar-property and normalization
table identity. A pack declares that identity and a compiler rejects a mismatch;
the host operating system's Unicode tables are not semantic input. The eventual
replacement candidate must select the exact table version rather than leave it
as “current Unicode.”

These bounds are intentionally generous for human words and tiny compared with
compiler source. Workloads should reduce them if evidence supports a smaller
contract.

The validated in-memory form should use a compact immutable table or trie owned
by the compiler-service generation. It is shared across compilation requests
using the same exact pack and released when that generation retires. Source
files do not each retain a copy.

### Compilation pipeline

The full working pipeline is:

~~~text
source bytes
    |
strict UTF-8 and newline admission
    |
fixed source-descriptor reader
    |
exact profile-manifest, lexicon, and interface-catalog binding
    |
bounded pack validation and immutable lookup construction
    |
localized keyword spelling -> canonical keyword token IDs
    |
grammar with normalized Unicode project identifiers
    |
localized public labels -> canonical declaration IDs
    |
semantic analysis -> WIR -> WVB/native/runtime
~~~

The token retains:

- canonical token identity;
- raw byte start and length;
- decoded scalar start and length where the diagnostic model needs it;
- source lexicon identity/version; and
- canonical source spelling for conversion and explanations.

The parser receives canonical keyword tokens and exact Unicode identifier spans.
The public-name resolver consumes admitted source-vocabulary labels and emits
canonical declaration identities. Type checking and later phases receive no
human-language selection, and runtime behavior has no localization branch.

### Semantic and artifact equivalence

Two files that differ only in source descriptors, keyword
spellings, and public-library labels should produce the same canonical token and
resolved public-declaration sequence after front-door lowering. With identical
project-owned identifiers, literals, package inputs, target, and compiler
options, they should produce byte-identical semantic WIR, WVB, object, and
executable sections.

Raw-source provenance is different and must not be confused with semantic
identity. The design should distinguish:

- exact raw source hash;
- exact source-lexicon pack hash;
- exact source-vocabulary profile and interface-catalog hashes;
- canonical token-stream hash;
- canonical resolved public-declaration hash;
- debug/source-map identity; and
- semantic generated-artifact identity.

Optional debug information may record the raw source, lexicon, vocabulary, and
catalog identities and therefore differ. Qualification compares the semantic
sections independently and verifies that any provenance difference is confined
to named debug/source records.

### Cache behavior

The safe first cache key contains raw source hash, lexicon pack hash, vocabulary
profile, every catalog hash, source edition, compiler identity, options,
dependencies, and target. This cannot confuse spans or diagnostics from
different spellings. Decision 0766 admits no project-override format in edition
1.

A later optimized cache may reuse canonical semantic evidence across localized
source forms
only after:

- it hashes the complete canonical token stream and canonical declaration
  inputs;
- source maps and localized diagnostics remain request-owned;
- debug provenance is regenerated rather than reused incorrectly; and
- a simple non-reusing correctness oracle proves identical semantic output.

Localization must not turn cache correctness into a probabilistic or
locale-dependent property.

### Deterministic conversion

Windvale tooling should support exact conversion between source profiles:

~~~text
windvale source convert --to-profile <identity@version> <source>
~~~

The command spelling is illustrative. Conversion:

1. admits the source under its explicit descriptor and profile manifest;
2. replaces recognized keyword spans and resolved public-library label spans;
3. preserves project-owned identifiers, literals, comments, documentation,
   whitespace, and line endings unless a separately requested operation changes
   them;
4. updates the universal source descriptor explicitly;
5. validates the converted source plus canonical token and public-declaration
   resolution;
6. refuses a target pack/catalog with collisions or missing entries; and
7. reports raw input/output hashes, source/target pack and catalog hashes, token
   count, and canonical semantic hashes.

Conversion requires no AI and is exactly reversible for mapped keyword and
public-library labels. It does not translate comments, project-owned names,
application text, protocols, or data.

### Source examples

Canonical English source uses the explicit `en@1` profile:

~~~text
#!wv/1 en@1
module Deliveryˉpolicy;
profile core;
platform windows, linux, windvale;
authority library;

export fn Isˉfree(Orderˉtotal: u64) -> bool effects() {
    if Orderˉtotal > 200u64 {
        return true;
    }
    return false;
}
~~~

A keyword-localized form of that same library implementation has the same
punctuation, numeric type, canonical module/declaration identities, literals,
and semantics, but its mapped keywords use the selected pack:

~~~text
#!wv/1 <locale>@1
<localized-module-keyword> Deliveryˉpolicy;
<localized-profile-keyword> <pack-spelling-for-core>;
<localized-platform-keyword> windows, linux, windvale;
<localized-authority-keyword> <pack-spelling-for-library>;

<localized-export-keyword> <localized-fn-keyword> Isˉfree(
    Orderˉtotal: u64,
) -> <pack-spelling-for-bool> <pack-spelling-for-effects>() {
    <localized-if-keyword> Orderˉtotal > 200u64 {
        <localized-return-keyword> <pack-spelling-for-true>;
    }
    <localized-return-keyword> <pack-spelling-for-false>;
}
~~~

Placeholders avoid presenting machine-generated translations as approved human
terminology. Each real workload must use a complete pack reviewed by a named
native speaker.

An application consuming that English-authored library may additionally select
a Chinese source vocabulary and store a fully Chinese program body:

~~~text
#!wv/1 zh-Hans@1
模块 结账;
配置 核心;
平台 windows, linux, windvale;
权限 应用程序;

导入 配送ˉ政策 作为 配送;

导出 函数 检查ˉ免运费(订单ˉ金额: u64) -> 布尔 效应() {
    返回 配送.是否ˉ免运费(订单ˉ金额: 订单ˉ金额);
}
~~~

The Chinese terminology is illustrative pending native review. The imported
module, member, and named-parameter labels resolve to
`Deliveryˉpolicy.Isˉfree(Orderˉtotal: ...)`; the other Chinese names are exact
project-owned Unicode identifiers.

### Required front-door failures

The source contract needs stable bounded failures for at least:

- missing, malformed, noncanonical, oversized, or misplaced descriptor;
- whitespace, comment, byte-order mark, non-ASCII byte, or extra field in the
  descriptor;
- unknown identity or version;
- missing or hash-mismatched profile manifest or component pack;
- wrong edition or token-registry identity;
- invalid normalization or forbidden scalar;
- duplicate token ID or duplicate spelling;
- missing or extra required token entry;
- missing, stale, extra, duplicate, or interface-hash-mismatched public source
  catalog entry;
- ambiguous localized module, member, field, case, or named-parameter label;
- non-normalized or forbidden Unicode project identifier;
- spelling colliding with a universal token or canonical identifier rule;
- keyword prefix without an admitted boundary;
- canonical English keyword used where the selected pack chose another
  spelling;
- concatenated or mixed-lexicon source;
- source converted with a stale target pack; and
- diagnostic or retained-state limit exhaustion.

Each failure identifies the descriptor/profile/lexical phase, exact source or pack byte
offset, violated rule, relevant identity/version/hash, and admitted limit. No
failure falls through into a confusing parser cascade.

## Where localization selection belongs

Selection differs by plane and must not be collapsed into one source attribute.

### Presentation locale

Presentation is not semantic and should normally be selected by the person or
review surface, not by the source author. Candidate selection locations are:

- editor user preference;
- workspace preference;
- an explicit review-view parameter;
- command-line diagnostic option; or
- an accessibility/profile preference in Windvale OS.

The precedence should be explicit and inspectable. No compiler operation may
silently inherit the host operating-system locale. A missing presentation pack
falls back to canonical display and reports that fallback to the requesting
tool; it does not fail compilation.

No `localization` declaration should be added to stored source merely to
select how another person sees it. One file can be viewed in Bulgarian by one
reviewer and Japanese by another without changing the file.

### Diagnostic locale

Diagnostic language belongs to the compiler request or developer tool:

~~~text
windvale check --diagnostic-locale bg
~~~

The spelling is illustrative. The request should resolve a content-identified
diagnostic catalog. The stable diagnostic ID, phase, source span, and structured
fields remain canonical. Localized prose is presentation.

### Source vocabulary and presentation vocabulary

The file's explicit composite source profile selects the semantic
source-vocabulary profile used to resolve stored localized references to public
libraries. It is strict compiler input: the build plan supplies exact catalogs
bound to each public interface hash, and a missing or stale catalog fails rather
than falling back to another language.

Library and project display vocabulary normally follows the independent
presentation locale. It may fall back because it cannot change source
resolution. A view should report both planes:

~~~text
source-keywords=bg
source-vocabulary=bg/strict
diagnostics=bg
foundation-display=bg
project-display=canonical-fallback
~~~

### Source lexicon selection alternatives

Model C makes the lexicon a parsing input, so it must be source- and build-owned.
Four placement alternatives were evaluated before selecting the working
descriptor design above.

#### Alternative 1: canonical in-source descriptor

~~~text
#!wv/1 es@1
~~~

The descriptor is canonical ASCII and is decoded by a small fixed front door
before the selected profile inputs are loaded. Remaining
keywords and admitted public-library references may then use them.

Advantages: the file is self-describing and survives copying.

Costs: every localized file begins with a short universal marker; the language
gains a descriptor grammar; and the exact profile artifact still needs binding.

#### Alternative 2: package/build-plan selection

The package plan maps each source path or source group to one lexicon identity
and exact content hash.

Advantages: source begins entirely in the selected language and pack binding is
already a build responsibility.

Costs: a copied file is not self-describing; standalone tools need the plan;
moving files can change parsing; and one bad default can reinterpret many files.

#### Alternative 3: sidecar metadata

Each source or directory has a small companion record naming its lexicon.

Advantages: no grammar change and the source can begin localized.

Costs: source and metadata can be separated, renamed, or mismatched. Windvale's
single-file inspection and recovery story becomes worse.

#### Alternative 4: filename extension or byte marker

Examples would be locale-specific extensions or a leading non-language marker.

This is not recommended. Extensions fragment tooling, while invisible markers
are hard to inspect and unsafe to copy. A byte-order mark, encoding guess,
character-frequency guess, or host locale must never select grammar.

The selected working direction is Alternative 1 plus exact build-plan binding:
a file names one stable composite source profile in a universal descriptor, and
the build plan supplies the exact validated manifest and component artifacts.
The file never embeds an arbitrary path or fetches a pack from the network.
Presentation, diagnostics, documentation, numeric formatting, and application
locale remain separate.

## Keyword pack design

### Map token IDs, not English words

A pack must map stable canonical token IDs to spellings:

~~~text
KW_IF        -> localized spelling
KW_ELSE      -> localized spelling
KW_RETURN    -> localized spelling
KW_WHILE     -> localized spelling
~~~

English text must not be the identity. Otherwise renaming or explaining an
English keyword could accidentally change every pack. The parser consumes
`KW_IF`; it never consumes “Spanish IF” or “Chinese IF.”

### Keyword classes

The working edition 1 reserves 76 body words, but not every reserved word has equal
localization value. A future design should classify them.

1. **Structural words** such as `if`, `else`, `return`, `module`, `record`,
   `match`, `using`, and `requires` are strong localization candidates.
2. **Ownership and safety terms** such as `borrow`, `move`, `unsafe`, `effects`,
   and `capability` need technically reviewed terminology, not literal
   dictionary translation.
3. **Primitive numeric spellings** such as `u32`, `i64`, `f32`, and `f64`
   should remain canonical and universal.
4. **Primitive semantic words** such as `bool`, `text`, `bytes`, `rune`, `unit`,
   and `never` require a deliberate decision: localized display may help, but
   canonical spellings support cross-language technical discussion.
5. **Profile and authority words** such as `core`, `hosted`, `system`,
   `library`, `application`, and `service` should have localized explanations;
   whether stored source spellings change is a separate question.
6. **Compound machine-oriented words** such as `cancel_join` and `fail_join`
   are candidates for better semantic display labels but must retain one token
   and one exact behavior.

Punctuation, operators, numeric suffixes, version numbers, capability
identities, effect identities, target keys, ABI identities, and external schema
names remain canonical. A pack cannot alter precedence, delimiters, layout,
grammar, type rules, ownership, evaluation order, effects, or failure behavior.

### One lexical atom

A source-pack keyword should be one bounded lexical atom with no whitespace or
punctuation. Natural languages that would ordinarily use a phrase must select a
compact programming term. This keeps every pack on the same grammar.

A display-only label may contain richer text in a tooltip or explanation, but
inline code rendering should remain visually compact and atomic.

### Completeness and fallback

For source compilation, a lexicon must completely map the edition's declared
localizable token set. There is no silent English fallback and no mixing of
lexicons in one file. Canonical numeric and punctuation items remain canonical
because the language classifies them that way, not because a pack is incomplete.

For presentation, partial packs may be useful during development, but the UI
must visibly mark canonical fallbacks. Official “qualified keyword view” status
requires complete coverage of the display token set.

### Synonyms

The compiler should recognize exactly one primary spelling per token per source
lexicon. Accepting multiple translations creates permanent aliases, formatter
choices, collisions, and style disputes.

An editor may provide search aliases or autocomplete terms that commit the one
primary token. Those aliases are input conveniences, never alternate source
spellings.

## Pack structure and identity

A pack should be declarative and divisible so a compiler does not load API docs
to recognize keywords. One possible package shape is:

~~~text
Localization/<locale>/
    Pack-Manifest
    Keywords
    Diagnostics
    Foundation-Vocabulary
    Tooling-Vocabulary
    Documentation-Index
~~~

These names and formats are illustrative. A durable pack manifest needs:

- canonical ASCII-safe pack identity;
- format version;
- human-language tag such as `bg`, `es`, `zh-Hans`, or `ar`;
- exact content kind and completeness level;
- compatible source edition or diagnostic/schema version;
- canonical token-set or target signature hash;
- exact byte and item limits;
- dependency identities when a catalog extends another catalog;
- independently computed content hashes; and
- publisher/provenance data for official distribution.

Official signatures and distribution policy belong to the future package and
release contracts. Signature acceptance must not replace content validation.

A pack grants no capability, performs no import, executes no function, loads no
native library, reads no ambient file, and makes no network request. A tool
receives an already bound pack as untrusted finite data and validates it before
use.

Pack lookup should be exact. A request for `pt-BR` may use a declared fallback
chain only for presentation content. Source lexicons and semantic
source-vocabulary catalogs cannot silently fall back from one language tag or
public interface to another.

### Compiler conformance versus translation qualification

A conforming Language 1.0 compiler should implement generic bounded
source-lexicon and source-vocabulary formats rather than contain
language-specific compiler branches. Given the same validated pack bytes, every
conforming host recognizes the same source spellings and produces the same
canonical tokens and public declaration identities.

The canonical English `en@1` profile is required and its exact manifest and
lexicon may be bundled with a minimal toolchain. Their generated data-pack
representations should also exist as differential oracles. Every file still
selects `en@1` explicitly; bundling it does not create fallback. Other source
profiles are explicit package/build inputs, and the compiler need not bundle
them all.

Compiler acceptance and human translation quality are separate:

| State | Meaning |
| --- | --- |
| Structurally valid | Safe bounded pack; exact mappings can compile |
| Draft terminology | Mapping is usable for experiments but not an official language claim |
| Native-reviewed | Every primary term reviewed in real Windvale source by named native speakers |
| Qualified | Native review plus malformed, conversion, editor, cross-host, and performance evidence |
| Officially distributed | Qualified pack published through the Windvale release repository |

A community pack may be a valid explicit compilation input without being
officially translated or bundled. Tools must show its publisher, content hash,
and review state. Windvale documentation must not imply that structural validity
means fluent or culturally appropriate terminology.

A package containing localized source declares the lexicon artifacts needed by
its modules. A recipient can build it with an ordinary conforming compiler plus
those exact small data packs; no compiler fork or locale-specific executable is
required. Missing packs fail during build admission, not after parsing begins.

This is the scalability goal: adding the five-hundredth lexicon should require
new reviewed data and tests, but no new compiler branch.

### Pack authoring and publication workflow

One source lexicon should move through this reproducible workflow:

1. generate a template from the exact source-edition token registry;
2. include each token's canonical identity, semantic explanation, grammar
   contexts, ownership/safety meaning, and real source examples;
3. allow AI or a translator to draft primary spellings, clearly marked as
   unreviewed;
4. have named native speakers with programming knowledge review terminology in
   complete Windvale modules rather than an isolated word table;
5. run structural, normalization, uniqueness, boundary, identifier-collision,
   bidi, malformed, and size validation;
6. convert and review at least one real workload under the pack;
7. prove canonical token and semantic artifact equivalence on Windows and Linux;
8. qualify editor, formatter, conversion, diagnostics, and source-provenance
   behavior;
9. compute immutable component and complete-pack hashes; and
10. publish the pack plus review evidence through the package/release repository.

A rejected term is changed before immutable publication. After publication, a
better translation creates a new pack version; tools may convert source between
the two versions deterministically.

### Ownership matrix

| Boundary | Durable owner |
| --- | --- |
| Source-descriptor grammar, 76-word classification, and canonical token registry | Source language specification |
| Bounded pack serialization and validation | Source-format specification and compiler front door |
| Lexicon identity-to-hash binding | Package/build plan and release registry |
| Canonical token lowering and raw source spans | Compiler lexer |
| Parser and all later semantics | Existing canonical compiler phases |
| Primary human-language terminology | Named native-language reviewers |
| Stable diagnostic fields and canonical fallback | Compiler diagnostic owner |
| Localized message templates | Diagnostic catalog translators and reviewers |
| Foundation declaration/parameter catalogs | Foundation module owner plus native reviewers |
| Third-party library catalogs | Library publisher plus declared reviewers |
| Project vocabulary | Application/project owner |
| Display, IME, clipboard, search, formatter, and conversion UX | Editor and source-tool owners |
| Content-addressed installation, deduplication, update, and rollback | Package repository and installer |
| Unicode shaping, bidi rendering, fonts, and console input | Display, console, input, and OS service owners |
| Cross-host, malformed, equivalence, and performance evidence | Named verification owners |

The compiler team does not decide whether a translation is natural, and a
translator does not define token semantics. Pack publication requires both
exact machine validation and explicit human-language review.

## Library localization

### Never create translated duplicate APIs

Foundation should not export both canonical and translated declarations such as
two functions that mean “open.” Duplicate exports would:

- fragment documentation and examples;
- create different source ecosystems;
- complicate imports and named arguments;
- enlarge interfaces and packages;
- make generic and diagnostic identities harder to explain; and
- allow translations to drift into different behaviors.

Each public declaration retains one canonical identity. Localization may supply
both an exact compile-time source-vocabulary catalog and independent
presentation/documentation catalogs over that identity.

### Working Language 1.0 library boundary

The source lexicon changes reserved keyword spellings. The independently
selected source-vocabulary profile permits stored localized names for imported
modules, public declarations, fields, variant cases, and named parameters. Each
label resolves through an exact interface-bound catalog before ordinary semantic
analysis.

For example, a Chinese source file may store:

~~~text
基础ˉ字节
字节.追加ˉ八位值
构建器
值
~~~

Those spellings map to the one canonical Foundation module, declaration, and
parameter identities. Canonical reveal, source conversion, imports,
named-argument checking, diagnostics, linking, and debug provenance retain the
exact mapping and catalog hash.

Three progressively stronger library-localization models remain distinguishable:

| Model | Stored member/parameter names | Compilation dependency | Working standing |
| --- | --- | --- | --- |
| Canonical APIs plus localized documentation | Canonical | None | Required baseline |
| Semantic display catalogs | Canonical | None; catalog is presentation-only | Complementary Language 1.0 tooling |
| Stored localized public source labels | Local labels resolved through exact catalog | Catalog is an explicit semantic compiler input | Selected working Language 1.0 direction |

The third model is now selected explicitly rather than treated as an accidental
consequence of Option C. Its collisions, stale signatures, named arguments,
source conversion, package binding, debug provenance, bounds, and strict
fallback rules are owned by the localized-source specification and required
workloads.

### Bind catalogs to exact public signatures

The Language 1.0 Foundation registry already gives each module's complete
signature set an independent SHA-256 identity. That is a strong localization
anchor. A Foundation vocabulary catalog can state conceptually:

~~~text
target-module: Foundationˉbytes
major: 1
signature-set-sha256: <exact registry identity>
locale: es

declaration: Appendˉu8
kind: function
primary-source-label: <reviewed localized label>
summary: <reviewed localized explanation>

parameter: Appendˉu8.Builder
primary-source-label: <reviewed localized label>

parameter: Appendˉu8.Value
primary-source-label: <reviewed localized label>
~~~

If the signature identity changes, the source catalog is stale and compilation
under that vocabulary fails until a new exact catalog is reviewed. A
presentation tool may fall back to canonical display, but source resolution may
not silently do so.

The same pattern applies to records, fields, enum members, variant cases,
protocols, generic parameters, failure variants, and documentation links.

### Named arguments make parameter localization important

Windvale permits named arguments, so parameter labels are part of readable
source and call checking. The selected exact catalog may provide their stored
localized source labels while preserving canonical parameter identities
underneath. Resolution occurs only after selecting the exact canonical
declaration, and equal labels that would create ambiguity reject the catalog.

### Imports and aliases

Edition 1 import aliases provide project-owned local vocabulary without changing
the imported module's canonical identity. The replacement working grammar admits
Unicode aliases. The imported module label resolves through the exact library
catalog; the `as` alias is the author's exact stored Unicode identifier. Tools
must preserve an easy way to reveal both the canonical module identity and the
catalog mapping.

### Library documentation delivery

Localized labels, summaries, examples, and full documentation should be
separately installable content-addressed objects. The installer can select the
developer's desired locales rather than shipping every translation in every
runtime or SDK package.

Suggested installation profiles are:

- **keyword view**: small token catalog only;
- **developer**: keywords, diagnostics, Foundation/tooling vocabulary, and API
  summaries;
- **full documentation**: developer content plus examples, guides, and search
  indexes; and
- **runtime**: no development localization unless the application explicitly
  depends on a separate runtime locale library.

Identical content should be deduplicated through the package repository rather
than copied into every application. Removing a presentation pack must not make
compiled applications stop running.

## Project and application vocabulary

Keywords and Foundation names are insufficient for a localized business view.
The important concepts are often project-owned names such as customer approval,
maximum temperature, free delivery, or transaction state.

A project may store its own module, declaration, field, case, parameter, alias,
and local identifiers directly in normalized Unicode. Their exact admitted
source spelling is their canonical project-owned name. Later presentation
tooling may propose a project display catalog that maps those exact identities
to labels and explanations for readers using another language:

~~~text
canonical declaration identity -> localized display label
canonical field identity       -> localized display label
canonical variant identity     -> localized display label
~~~

Such a future catalog needs a stable binding to package/module/declaration
identity and, where necessary, a declaration or interface fingerprint. A rename
must make the old entry unresolved instead of silently applying it to a
different symbol. Language 1.0 defines no project display-catalog format.

Decision 0766 rejects a project source-vocabulary override from Language 1.0
because no workload defined its exact format. Two imported canonical modules
whose catalogs choose the same localized module label produce a hard ambiguity
failure. A later edition may define explicit disambiguation; dependency order
never decides the result.

AI may draft project vocabulary, explanations, and localized review summaries.
The catalog must record whether an item is machine-generated, human-reviewed,
or qualified. AI translation never changes the bound semantic identity and is
never trusted to decide that two declarations mean the same thing.

Comments and documentation may receive optional translated overlays. String,
byte, resource, package-data, protocol, database, and user-visible application
values are never automatically translated by the compiler view.

## Semantic display behavior

### Visible mode and provenance

An editor must always show a clear mode indicator, for example:

~~~text
Source lexicon: bg/1 | View: Japanese | Vocabulary: Foundation qualified
~~~

The user needs a one-action canonical reveal and a per-token identity inspector.
Screenshots, exported reviews, and printed views should include the presentation
locale and pack identity so localized code is not mistaken for stored source.

### Atomic keyword rendering

When a canonical keyword is rendered as a different-length localized label,
the view should treat it as one semantic token. Cursor movement, selection,
deletion, and replacement operate on the token boundary rather than pretending
that every displayed scalar maps to one stored byte.

This must remain accessible to screen readers. The accessibility tree should
expose the localized label, token role, and optionally canonical spelling.

### Incomplete typing and IMEs

Localized input remains ordinary composition text until the input method and
Windvale token input layer commit one complete candidate. The editor must not
translate an arbitrary prefix while a person is still composing it.

On commit:

1. validate the complete localized candidate against the selected input pack;
2. insert the one primary spelling required by the file's source lexicon as one
   undo transaction;
3. retain the exact stored-source span and canonical token identity;
4. render the selected presentation label; and
5. allow immediate stored/canonical-lexicon reveal or undo.

If the candidate is not a keyword, it remains ordinary source input and receives
normal syntax or identifier diagnostics. Grammar context may improve completion
ranking but must not cause hidden semantic rewrites.

### Copy, paste, and drag

The difference between visible, stored, and canonical-lexicon text must not be
hidden. The editor should provide three explicit operations:

- **Copy stored source**, suitable for exact builds, patches, and continued
  editing with the selected profile;
- **Copy canonical-lexicon source**, suitable for readers or tools that want the
  `en@1` profile while preserving program tokens; and
- **Copy displayed view**, suitable for human explanation and localized review.

The default needs usability testing. Whichever default is selected, the UI must
identify it, and rich clipboard metadata must never be required to recover valid
stored source.

Pasting source or displayed text into a file should offer deterministic
conversion to the target file's source lexicon when the input pack identity is
known. It must not guess a language from characters or the host locale.

### Search, rename, and navigation

Search should expose separate modes for stored source text, canonical-lexicon
text, displayed labels, and resolved semantic identity. Semantic rename changes
canonical declarations and references; it does not treat translated labels as
independent symbols.

Go-to-definition, references, ownership explanation, capability closure,
source-to-WIR/WVB inspection, and debug views operate on canonical identities.
They may render localized labels alongside those identities.

### Formatting

The Language 1.0 formatter admits the file's exact source lexicon and writes
that lexicon's deterministic primary spelling for every keyword token. It
preserves the lexicon declaration unless explicit conversion selects another
pack. It cannot mix lexicons to satisfy line width.

A separate presentation view projects the formatted token stream. Display
labels cannot alter canonical formatting decisions, although a view may need
visual wrapping because translations have different widths.

## Diagnostics and explanations

Compiler diagnostics already require stable identity, phase, canonical module,
source span, structured expected/observed state, and bounded related locations.
Localization should preserve that structure.

A diagnostic catalog maps a stable identity and named structured fields to one
bounded message template. Machine consumers use the identity and fields, never
parse localized prose.

A localized diagnostic should be able to show both human and canonical terms:

~~~text
<localized explanation>
canonical rule: WV-OWN-0042
canonical declaration: Foundationˉbytes.Appendˉu8
source: Module:line:column
~~~

Message formatting must be invariant for machine values such as byte offsets,
integer bounds, capability identities, ABI identities, and hashes. A diagnostic
locale does not change numeric parsing or semantic comparison.

Catalog validation must bound template length, expansion length, related items,
placeholder count, recursion, and retained diagnostics. Missing or malformed
localized content falls back to the canonical diagnostic without losing the
underlying compiler failure.

## Git, reviews, and AI agents

Exact stored source remains the Git identity and default diff. A semantic review
can additionally project both sides through their declared packs or one selected
view, but it must expose the raw patch, source pack identities, and canonical
token changes.

Semantic review can become more powerful than keyword replacement:

- show localized display labels for declarations;
- retain canonical names in hover or an adjacent column;
- explain ownership, capabilities, effects, and failures in the reader's
  language;
- summarize the behavioral change using stable declaration and diagnostic IDs;
  and
- distinguish a translated explanation from verified compiler evidence.

AI agents should preserve the file's explicit or default source lexicon unless
the task authorizes deterministic conversion. An agent may generate draft
catalogs or localized review views, but tests and approvals remain attached to
canonical token, declaration, and artifact identities as well as exact raw
source and pack provenance.

## Unicode and security boundary

Localization makes Unicode handling a security boundary rather than a display
detail.

Candidate rules include:

- strict UTF-8 with no replacement decoding;
- one specified normalization form for pack data and exact validation after
  decoding;
- no locale-dependent case conversion or comparison;
- exact ordinal matching of validated source spellings;
- rejection of forbidden control, noncharacter, private-use, unassigned, and
  invisible format scalars unless a narrowly specified language case proves a
  need;
- no embedded bidirectional override or isolate controls in source-pack keyword
  spellings;
- renderer-owned bidirectional isolation around semantic tokens and canonical
  punctuation;
- exact admission or rejection plus diagnostics for confusable or mixed-script
  identifiers under the selected Unicode source boundary;
- maximum UTF-8 bytes and Unicode scalars per label, token, template, catalog,
  and pack;
- bounded normalization, lookup, shaping metadata, diagnostics, and retained
  state; and
- malformed, truncated, duplicate, collision, oversized, and adversarial pack
  tests.

Arabic and Hebrew require independent right-to-left review. Chinese, Japanese,
and Korean are independent languages even where scripts or characters overlap.
Russian, Bulgarian, Ukrainian, and Serbian require separate terminology despite
using Cyrillic. Spanish, Portuguese, French, German, Turkish, Vietnamese, and
other Latin-script languages are likewise separate packs.

Pack language tags and canonical identities remain ASCII-safe. Display systems
must not infer a pack from script detection.

## Selected Unicode identifier direction

Localized keywords, localized public-library references, and Unicode
project-owned identifiers are distinct mechanisms, but the project owner has now
selected all three for the working replacement candidate. Without Unicode
project identifiers, a supposedly Chinese source file would still store English
module, function, parameter, alias, and local names.

The selected candidate admits normalized Unicode identifiers for project-owned
modules, public and private declarations, fields, cases, parameters, import
aliases, and locals. Their exact admitted UTF-8 spelling is their canonical
source name. References to an imported library's public surface are different:
they may store localized catalog labels that lower to the library's existing
canonical identities.

Machine namespaces remain constrained. Package, capability, effect, platform,
ABI, foreign-symbol, wire-protocol, and other registered machine identities do
not become Unicode merely because source identifiers do. Native symbols use
collision-safe ASCII mangling rather than passing Unicode names to host ABIs.

This is the largest usability option and the largest security/interoperability
change. The alternative boundaries remain useful comparison oracles:

| Boundary | Benefit | Reason not selected as the product target |
| --- | --- | --- |
| Keep every source identity constrained | Smallest package/debug change | A localized file still stores canonical/English domain names |
| Admit Unicode only for local/private identities | Natural local logic with constrained exports | A declaration becomes harder to promote and public APIs still interrupt localized reading |
| Admit reviewed Unicode project identities plus exact localized imported-library labels | Fully localized stored program bodies with one canonical imported API | Selected; requires exact Unicode and catalog security workloads |

The source edition must pin normalization, identifier properties, forbidden
scalars, script/confusable behavior, and cross-host tables. U+02C9 remains the
semantic-concept separator. A localized label translates the complete concept
in its language's natural order rather than mechanically translating canonical
macron segments one by one.

Workload 4 and Decision 0766 resolve the exact Unicode version and mixed-script/
confusable profile for the replacement candidate. Implementation must use those
pinned inputs rather than inheriting host behavior.

## Compiler architecture for localized source

The front door would be:

~~~text
strict UTF-8 decode
        |
fixed universal source-descriptor reader
        |
exact profile-manifest, source-lexicon, and source-vocabulary binding/validation
        |
localized lexer -> canonical keyword IDs, Unicode identifiers, raw spans
        |
existing parser
        |
localized public labels -> canonical declaration IDs
        |
semantic analysis, WIR, backend, and runtime
~~~

Only decoding, descriptor/profile admission, and keyword recognition know the source
lexicon. The public-name resolver knows source-vocabulary catalogs and lowers
their labels to canonical declaration identities. Type checking and every later
phase consume canonical token and public-symbol identities.

The compiler must retain raw byte spans for diagnostics while allowing a
canonical semantic token fingerprint. Cache design should distinguish:

- raw source identity, including exact spelling and every pack/catalog identity;
- canonical semantic token identity;
- canonical resolved public-declaration identity;
- source-map/debug provenance; and
- generated artifact identity.

Two source forms with identical canonical tokens, project identifiers, and
resolved public declarations must produce identical semantic WIR/WVB sections.
Raw source hashes and debug/source maps may legitimately differ. This requires
an explicit artifact rule rather than accidental dependence on source bytes.

Pack validation occurs before large allocation or full semantic analysis.
Unknown pack, wrong edition, wrong token/interface hash, duplicate spelling,
public-label ambiguity, normalization mismatch, stale catalog, or limit
violation is one bounded front-door failure.

No source file selects a pack by filesystem path, network URL, environment
variable, registry entry, installed keyboard, or process locale. The build plan
binds a declared identity to exact bytes just as other build inputs are bound.

## Operating-system and console implications

Localization is not blocked on Windvale OS, but a good native experience needs:

- strict Unicode input and output;
- fonts selected explicitly or packaged by the environment;
- grapheme-aware cursor behavior;
- CJK width handling;
- Arabic shaping and bidirectional layout;
- IME composition for Chinese, Japanese, and Korean;
- right-to-left input and selection for Arabic and Hebrew;
- deterministic fallback when a glyph is unavailable; and
- a terminal protocol that preserves source bytes and canonical spans.

These belong to console, display, input, font, editor, and accessibility
contracts—not the language lexer. A headless compiler remains able to compile
source in any admitted exact lexicon without fonts, shaping, or an input method.

## Distribution and package size

Localization content can become much larger than the language itself once it
includes diagnostics, API documentation, tutorials, fonts, and search indexes.
It should not be copied into every shipment.

The future installer repository can store independently content-addressed:

- small keyword/input catalogs;
- diagnostic catalogs;
- library source-vocabulary catalogs bound to exact signature identities;
- independent library display/documentation catalogs;
- documentation and examples;
- editor search indexes; and
- optional fonts or input resources under separate platform contracts.

An installer selects the requested developer locales and profiles for that
machine. Shared objects are deduplicated across SDKs and applications. Runtime
installations omit development localization unless explicitly requested.

Updates should be transactional: validate the complete new pack generation,
publish it atomically, retain the previous usable generation for rollback, and
never leave a compiler request observing half of one language version.

## Performance and memory expectations

Models A and B should add no compiler or generated-program cost. Editor
localization should use token IDs already produced for highlighting and language
services, then cache bounded display records by pack and document generation.

Expected localized-source costs are bounded lexicon/catalog validation per
compiler service generation, keyword lookup proportional to token bytes, Unicode
identifier admission, and public-label lookup proportional to admitted source
labels. The ordinary type checker, optimizer, WIR, WVB, native backend, and
runtime must remain language-independent.

Measurements should record:

- pack validation and load time;
- bytes and retained memory per component;
- cold and warm lexical throughput against default-lexicon source;
- incremental editor latency while typing incomplete source;
- view projection and locale-switch latency;
- diagnostic template expansion time and maximum output;
- install size by keyword, developer, and documentation profile; and
- cache reuse across documents and compiler requests.

No performance claim should rely on a tiny ASCII-only fixture. Qualification
needs representative CJK, combining-mark, and right-to-left data at admitted
limits.

## Required tests and paper workloads

### Pack admission

- valid complete source lexicons/source-vocabulary catalogs and valid complete
  or partial non-semantic display catalogs;
- exact version, edition, Unicode-table, token-set, interface, and signature
  binding;
- duplicate token IDs and duplicate spellings;
- keyword/identifier collision;
- malformed UTF-8 and normalization mismatch;
- forbidden controls and bidirectional input;
- truncated, oversized, inconsistent, and adversarial data;
- deterministic hashing and cross-host parsing; and
- transactional update, rollback, and removal.

### Source and conversion

- every canonical keyword and every declared non-localized token class;
- prefix collisions and keyword followed by the macron separator;
- one lexicon per file and explicit rejection of mixing;
- incomplete IME composition and ordinary invalid syntax;
- canonical-to-localized-to-canonical keyword/public-label round trips;
- normalized Unicode module, declaration, field, case, parameter, alias, and
  local identifiers;
- source spans before and after different-length keyword rendering;
- formatter stability and exact primary spellings;
- paste with known, missing, stale, and conflicting pack identity; and
- raw-source, semantic-token, resolved-declaration, debug-map, and artifact
  identity behavior.

### Library vocabulary

- exact complete public-interface signature-set match;
- stale catalog after an added, removed, or renamed declaration;
- functions, types, records, fields, variants, cases, protocols, and generics;
- stored localized members, fields, cases, and named arguments in canonical and
  reordered source order;
- canonical reveal, semantic search, rename, and go-to-definition;
- strict missing-catalog failure, explicit canonical primary labels, and no
  implicit fallback;
- collision handling within one owner and across a dependency closure; and
- content deduplication across installed products.

### Editor and review

- cursor, selection, delete, replace, undo, redo, copy, paste, and drag;
- screen reader and keyboard-only behavior;
- breakpoint, diagnostic, diff, and review spans;
- switching locale with valid, invalid, and incomplete source;
- simultaneous reviewers using different locales;
- canonical export of every localized view;
- visible provenance in screenshots and exported reviews; and
- AI-drafted versus human-reviewed catalog state.

### Language diversity

Engineering packs should intentionally cover different problems rather than
only the largest markets:

- English for the canonical reference;
- Bulgarian for locally reviewable Cyrillic terminology;
- Spanish for Latin-script translation and broad prose testing;
- Simplified Chinese for CJK keyword and display behavior;
- Japanese for different terminology, segmentation, and IME behavior;
- Arabic for shaping, right-to-left presentation, and bidi security;
- Korean next for Hangul and IME behavior; and
- Hebrew next as an independent right-to-left case.

Official support should require named native-language review. Popularity may
influence delivery priority, but script coverage, technical-language preference,
review capacity, accessibility, and the future reader population also matter.

## Staged exploration

The [localization workload plan](Windvale-Language-1.0-Localization-Workloads.md)
turns this exploratory sequence into five reviewable bundles. Its
[Workload 1 packet](Language-1.0-Localization-Workloads/01-Source-Profile-Admission/README.md)
provides the complete first-author format/admission evidence. Workloads 2 through
5 now also have owner-accepted first-author paper findings. Native Chinese
terminology review and all compiler, editor, package/installer, cross-host, and
measured-performance qualification remain open.

### Stage 0: paper contract

- inventory the 76 body words and exact 66-token mapped set;
- assign stable candidate token IDs and define the descriptor grammar;
- define bounded composite source-profile, source-lexicon, source-vocabulary
  profile/interface catalog, and evaluate project-override/display-catalog
  alternatives;
- draft Unicode, bidi, and provenance rules;
- choose prototype languages and native reviewers; and
- create examples using real Windvale compiler, library, application, and OS
  source rather than toy syntax alone.

### Stage 1: descriptor, profile, and pack admission workload

The paper format/admission slice is complete as a proposed candidate: seven
strict artifact formats, exact reference hashes, 25 accepted cases, 43 rejected
cases, one canonical English chain, and one synthetic Unicode chain. The
synthetic chain challenges the mechanism but does not qualify any natural
language. Actual cold/warm compiler measurements require an implementation and
belong to the later qualification workload.

- parse the canonical first-line source descriptor without the general lexer;
- validate immutable identity/version/hash/edition/Unicode/token-registry and
  public-interface binding;
- admit one complete synthetic Unicode reference profile before native-language
  qualification;
- reject every malformed, duplicate, missing, collision, normalization,
  boundary, mixed-lexicon, and limit case; and
- define the cold/warm loading, retained-memory, and failure measurement
  protocol; execute it once a representative implementation exists.

### Stage 2: localized source and equivalence workload

The first-author `zh-Hans@1` packet now contains all 66 draft keyword mappings,
one complete Foundation interface catalog, exact hashes, and paired English/
Chinese-profile source with the same Chinese project identities. Pinned Unicode
and paper canonical-projection checks pass. Native terminology review and
compiler/WIR/WVB/native equivalence remain open.

- convert a real accepted paper workload into at least Bulgarian and one
  structurally different reviewed pack;
- store a fully Chinese application body with Unicode project identifiers and
  localized references to one canonical English library;
- preserve project-owned identifiers, literals, comments, package data, and
  expected behavior during public-label conversion;
- prove identical canonical token/resolved-public-declaration streams and
  semantic WIR/WVB sections;
- retain exact raw source and debug/span provenance; and
- compare canonical and localized lexical throughput on both permanent hosts.

### Stage 3: conversion, formatter, and editor workload

- convert canonical-to-localized, localized-to-canonical, and between two
  localized packs without AI;
- preserve project-owned identifiers and non-source prose while validating
  round-trip token/public-declaration identity;
- preserve one primary spelling per pack through formatting;
- qualify incomplete IME composition, syntax errors, cursor/selection,
  copy/paste, search, diff, canonical reveal, and accessibility; and
- show permanent source/view/pack provenance.

### Stage 4: localized diagnostics workload

- preserve stable structured diagnostics and exact stored-source spans;
- add bounded message catalogs and canonical fallback;
- qualify placeholder, output, limit, stale, and malformed-pack behavior;
- test explanations of ownership, capabilities, effects, and failures; and
- prove that changing diagnostic locale cannot change compilation output.

### Stage 5: Foundation source and project vocabulary workload

- bind complete source catalogs to exact Foundation signature-set and declaration
  identities;
- store and resolve localized functions, types, fields, variant cases, and named
  parameters;
- preserve canonical hover, search, rename, navigation, and copying;
- bind optional project display labels and source-collision overrides to exact
  declaration identities;
- distinguish AI drafts from human-reviewed vocabulary;
- measure independently installed/deduplicated catalog size; and
- study whether fully localized stored application source and alternate reader
  views improve accurate review without changing imported symbol identities.

### Stage 6: replacement source-freeze reconciliation

After the preceding evidence, reconcile every accepted finding into the
Language 1.0 semantic and grammar specifications, machine grammar, migration
plan, editor contract, paper corpus, roadmap, progress record, and named
decisions. Produce a replacement exact manifest and ask the project owner to
freeze that identity. The preserved pre-localization candidate remains
historical evidence and is not silently edited into a different identity.

## Working decisions to test

The following directions are now concrete enough for paper workloads, but are
not normative until the owner accepts the resulting evidence:

1. Model C, stored localized public-library labels, and Unicode project
   identifiers belong in the replacement Language 1.0 candidate.
2. Every file begins at byte zero with one explicit language-neutral descriptor,
   such as `#!wv/1 en@1` or `#!wv/1 zh-Hans@1`.
3. The descriptor selects one immutable composite source profile; omission is an
   error, and there is no English or host-locale default.
4. One immutable lexicon maps 66 keyword token identities; ten fixed-width
   numeric type names, punctuation, operators, and registered machine identities
   remain universal. The descriptor is metadata, not a keyword sequence.
5. One file uses one lexicon and one vocabulary profile with no implicit
   fallback or scoped switch.
6. Lexicons and interface-bound source catalogs are bounded declarative inputs,
   not executable compiler plugins.
7. The lexer emits canonical token IDs and raw spans; the parser and every later
   compiler/runtime phase remain language-independent.
8. Keyword and resolved public-library source-label conversion is deterministic;
   project-owned identifiers and non-source prose remain unchanged unless a
   separate operation is requested.
9. Equivalent localized source forms produce identical semantic artifacts,
   while raw source, pack/catalog, and debug provenance remain explicit.
10. Public library APIs retain one canonical identity. Source catalogs provide
    one stored primary label per identity and are bound to exact interface
    hashes; display/documentation catalogs remain independent.
11. Project-owned Unicode identifiers use exact normalized stored names;
    registered machine identities remain ASCII-safe.
12. Application/runtime localization remains separate and explicit.

## Reconciled design questions

Workload 1 proposes exact answers for artifact serialization and hash binding,
the Unicode 17.0.0 profile, cache identity/publication, complete interface-bound
catalogs, and the exclusion of non-source-addressable generic-parameter labels.
Those answers live in the
[source-profile artifact formats](../../Specifications/Windvale-Language-1.0-Source-Profile-Formats.md)
and are owner accepted by Decision 0766. The original questions now resolve as
follows:

1. Pack-owned terminology: each exact qualified lexicon chooses whether a
   mapped word translates or explicitly retains its canonical spelling. Native
   review decides naturalness; the language core does not impose translations.
2. Primitive words in the 66-token registry may localize. The ten fixed-width
   numeric type names remain universal exact machine-facing spellings.
3. Profile and authority keywords may localize because they map to canonical
   tokens. Registered profile, platform, authority, capability, and effect
   identities remain universal ASCII-safe machine keys.
4. Resolved by Workload 3: ordinary Copy produces exact stored source;
   canonical-source and displayed-view copies are explicit alternatives.
5. Resolved by Workload 3: visual wrapping is presentation-only and cannot
   change canonical formatter output or stored source without an explicit edit.
6. Resolved by Workload 3: stored spelling/span is primary source evidence;
   structured diagnostics retain stable canonical identity, and tools provide
   an explicit canonical reveal. Ordinary UI need not duplicate both inline.
7. Language 1.0 has no project display-vocabulary format. Semantic rename
   changes project declarations/references by identity; any future display
   catalog must bind exact identities and become stale after a rename.
8. Resolved for edition 1 by Workload 4: keep exact NFC, XID, Highly Restrictive,
   mixed-number, and scoped confusable rejection; do not admit join controls.
   A later edition needs native-language evidence and a new exact profile.
9. Later non-semantic display packs may contain short/explanatory alternatives;
   Language 1.0 source lexicons/catalogs retain exactly one primary spelling and
   define no display-catalog format.
10. Namespaced community profiles may be explicit source inputs using the same
    formats/security rules. They remain community data until the native review,
    executable qualification, and distribution gates pass.
11. Translated project-identifier views remain later presentation tooling.
    Edition 1 stores and compiles the exact project name and may reveal canonical
    imported identities through hover/review tools.
12. This remains a product-research measurement rather than a semantic question;
    it cannot change source or build identity implicitly.
13. Workload 2 fixes four states: draft, native-reviewed, qualified, and
    officially distributed. Qualification requires a native technical reviewer,
    an independent fluent readability reviewer, exact reviewed hashes,
    mechanical checks, and executable Windows/Linux evidence.
14. Resolved by Workload 5: runtime-only installations carry no development
    localization; the minimal developer installation carries shared edition
    data and `en@1`; other source profiles, diagnostics, and documentation are
    independent optional packages selected explicitly through the existing
    content-addressed installer architecture.
15. Resolved by Decision 0766: no project collision-override or semantic display-
    catalog format enters Language 1.0. Ambiguous localized imports fail. Any
    later disambiguation/presentation format needs a versioned workload and
    decision.

## Accepted recommendation

Windvale should include stored localized keywords, stored localized references
to canonical public-library declarations, and Unicode project identifiers in the
replacement Language 1.0 candidate while retaining one canonical semantic token,
imported declaration, compiler, and artifact model. Semantic views and localized
input remain complementary tooling features rather than substitutes for stored
localized source.

The first useful product should:

1. store plain strict UTF-8 source under one explicit language-neutral
   descriptor and immutable composite source profile;
2. map exact localized keyword spellings to stable canonical token IDs;
3. map exact stored public-library labels through interface-bound catalogs to
   canonical declaration identities;
4. admit normalized Unicode project identifiers while retaining ASCII-safe
   registered machine namespaces;
5. localize diagnostics through stable structured diagnostic IDs;
6. localize Foundation and project display/documentation through catalogs bound
   to exact declaration and signature identities;
7. make stored source, canonical tokens/declarations, and exact profile/component
   identities continuously inspectable;
8. ship localization content independently so installers select only desired
   languages and profiles; and
9. retain the five completed paper workloads as source-freeze evidence and make
   them executable during implementation qualification.

This direction can make Windvale unusual without making it fragmented: the
program remains exact for machines, while its explanation becomes adaptable for
people.
