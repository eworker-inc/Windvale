# Unicode identifier policy

## One exact pipeline

Every project-owned identifier, source keyword spelling, and source-addressable
public label passes one ordered admission pipeline. Packs cannot relax it.

1. Decode strict UTF-8 without a byte-order mark.
2. Require the exact Unicode 17.0.0 data identified by
   `windvale.unicode17.source@1`; host character tables are not semantic input.
3. Require the complete identifier to already be NFC. Diagnose the expected NFC
   form but never normalize source silently.
4. Enforce at most 256 UTF-8 bytes, 128 Unicode scalars, and 32 U+02C9-delimited
   semantic segments for a project-owned identifier. Existing stricter
   keyword/public-label artifact bounds remain 128 bytes and 64 scalars.
5. Split only on exact U+02C9 MODIFIER LETTER MACRON. Reject an empty, leading,
   trailing, or adjacent segment.
6. In each segment, admit `_` as the one Windvale addition; otherwise require
   `XID_Start` for the first scalar and `XID_Continue` thereafter.
7. Require every non-underscore scalar to have `Identifier_Status=Allowed` and
   reject every default-ignorable, control, surrogate, noncharacter, private-use,
   unassigned, Pattern_Syntax, or Pattern_White_Space scalar.
8. Require UTS #39 revision-32 ASCII-Only, Single Script, or Highly Restrictive
   status independently for every segment.
9. Map every `Nd` scalar to its decimal-system zero and permit at most one such
   zero per segment.
10. Compute both revision-32 LTR and RTL `bidiSkeleton` values once per segment.
    In one semantic lookup scope, reject a later distinct segment whose skeleton
    equals an already visible keyword, declaration, import/catalog label, alias,
    parameter, local, field, case, or other competing name.

Identifier identity remains the exact admitted NFC UTF-8 bytes. Skeletons are
collision evidence, not aliases, lookup keys, exported names, or case folding.

## Script behavior

Single-script Latin, Cyrillic, Greek, Han, Hiragana/Katakana, Hangul, Arabic,
and Hebrew segments are admitted when their individual scalars pass the profile.

UTS #39 Highly Restrictive additionally permits these sets:

- Latin + Han + Hiragana + Katakana;
- Latin + Han + Bopomofo; and
- Latin + Han + Hangul.

Consequently `API応答` and `GPU가속` are secure-profile-valid segments. Windvale's
official naming convention may still prefer `APIˉ応答` and `GPUˉ加速` when the
parts are distinct semantic words. Security admission and naming style are
different checks.

Latin mixed directly with Cyrillic, Greek, Arabic, or Hebrew in one segment is
rejected. A visible U+02C9 boundary permits intentional cross-script concepts:
`HTTPˉОтвет` and `GPUˉتسريع` each contain two independently admitted segments.

## Decimal-number systems

Only Unicode `Nd` characters can occur through `XID_Continue`; a digit cannot
start a segment. `項目12` and `عنصر١٢` each use one number system and pass.
`عنصر1١` combines ASCII and Arabic-Indic digits in one segment and fails.

The rule is per visible semantic segment. An identifier may intentionally use
different number systems in separate U+02C9-delimited segments because the
boundary is exact and visible.

## Confusable scopes

The compiler uses semantic visibility, not a whole-repository pairwise scan. It
maintains bounded skeleton maps while constructing each lookup scope and checks
new visible names against:

- the selected lexicon's keyword spellings;
- declarations and aliases in the scope;
- imported modules and public labels that compete at that lookup point; and
- named parameters, fields, cases, and members in their owning namespace.

`scope` and Cyrillic `ѕсоре`, Latin `KAI` and Greek `ΚΑΙ`, and Hebrew `ו` and `ן`
have equal pinned skeletons in the relevant direction. Each spelling can be
valid alone; the pair is rejected only when both compete in one lookup scope.
Unrelated scopes remain legal, and tools may issue non-failing broader warnings.

An explicit nonconfusable import alias is the normal way to disambiguate two
otherwise colliding dependency labels. A source pack cannot whitelist a
collision.

## Join-control decision

U+200C ZERO WIDTH NON-JOINER and U+200D ZERO WIDTH JOINER are
`XID_Continue`, but Unicode 17 UTS #39 classifies them as default-ignorable and
`Identifier_Status=Restricted`. Language 1.0 does not tailor them back in.

This decision:

- prevents invisible identifier differences and shaping-dependent lookup;
- avoids a contextual validator and script-specific exceptions in the first
  source edition;
- preserves one exact primary spelling and stable cross-host identity; but
- prevents some preferred Persian, Arabic-derived, and Indic orthographies.

The limitation is explicit. Join controls remain ordinary Unicode data inside
text/comment content under the source-display rules; only their use in semantic
identifiers and keyword/public-label spellings is rejected.

## Complexity and retained state

Admission is linear in UTF-8 bytes/scalars plus bounded table lookups. NFC,
script, number-system, and skeleton evidence is computed once per admitted
identifier/segment and stored only where later lookup or diagnostics require it.
Confusable detection uses a map keyed by the two pinned skeleton values; it does
not compare every pair of names. Diagnostics retain bounded original spans and
at most one existing collision witness plus a fixed small set of related fields.
