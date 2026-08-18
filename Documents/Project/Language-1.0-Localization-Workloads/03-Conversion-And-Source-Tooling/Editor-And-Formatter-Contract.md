# Editor and formatter contract

## Visible source identity

An editor always exposes the stored file's source profile identity/version and
whether the user is viewing stored, canonical, or presentation text. A
presentation locale never masquerades as stored source. One action reveals the
canonical token or declaration identity at the cursor without rewriting disk.

## Copy, paste, and drag

The ordinary Copy command copies the exact stored source selection. This is the
safest default for patches, terminals, issue reports, build reproduction, and
tools that do not understand Windvale clipboard metadata.

Two separately named operations may also exist:

- **Copy canonical source** performs the same deterministic semantic conversion
  to `en@1` for a complete admitted selection or file; and
- **Copy displayed view** exports human presentation text and visible profile/
  catalog provenance, but does not claim that the result is compilable source.

Rich clipboard metadata may carry a bounded version, stored profile identity and
hashes, raw source bytes, and exact semantic spans. Plain text must remain
sufficient to recover the stored-source form selected by ordinary Copy.

Paste and drag follow these rules:

1. Exact text with no trusted Windvale provenance is inserted as text and
   receives ordinary syntax/identifier diagnostics. The editor does not guess a
   language from script, host locale, or surrounding words.
2. Provenanced source with the same exact profile may be inserted without
   conversion after its metadata and spans are validated.
3. Provenanced source with a different profile offers an explicit deterministic
   conversion using the target file's locked inputs. Refusal or missing inputs
   leaves the target unchanged.
4. Display-only clipboard content is never silently promoted to source.

One undo transaction owns the whole successful converted paste or drag.

## Incomplete input and IMEs

Input-method composition remains ordinary uncommitted text. No keyword or public
label conversion occurs until the IME commits a complete candidate. Completion
may suggest the selected profile's one primary spelling and show its canonical
identity. It cannot insert a secondary alias or rewrite an arbitrary prefix.

## Formatting

The formatter first admits the descriptor and exact selected profile. It writes
that profile's primary keyword and imported-public-label spellings. It never
changes the descriptor profile, mixes profiles to satisfy line width, or uses
the editor/host diagnostic locale.

Formatting may change whitespace according to the Language 1.0 formatting
contract, but it preserves project-owned identifiers, comments, documentation,
literals, resources, and machine identities. Running the formatter twice on the
same admitted file and inputs produces byte-identical output.

Different display-label widths affect visual wrapping only. They cannot alter
stored formatting or create Git changes.

## Search, navigation, and rename

Search provides separate explicit modes for:

- exact stored text;
- canonical keyword/public-declaration identities;
- displayed labels; and
- project-owned semantic declarations and references.

Go-to-definition, find references, ownership/capability explanation, source-to-
WIR/WVB inspection, breakpoints, and debugger navigation use semantic identities
and exact stored-source spans. Localized labels may be shown alongside them.

Rename has three distinct boundaries:

1. Renaming a project-owned declaration changes that declaration and references
   selected by semantic identity. The requested new Unicode name must pass the
   file's identifier/security rules. Rename does not translate it automatically.
2. A consumer cannot rename an imported library declaration by changing its
   localized label. The declaration is owned by the library; the editor reports
   the canonical owner and catalog mapping.
3. Changing a published localized library label is a catalog-authoring action.
   It creates new catalog bytes and hashes, reruns completeness/collision review,
   and requires explicit source conversion for consumers that adopt the new
   catalog version.

Textual occurrences in comments, strings, resources, and external schemas are
outside semantic rename unless the user separately requests a text operation.

## Canonical reveal and Git review

Canonical reveal is a non-mutating view over admitted tokens and resolved
declarations. It shows at least the exact stored spelling, canonical identity,
owning pack/catalog identity and hash, and source byte span. It is available for
one token, a selection, or the whole file.

Git stores and diffs exact authored bytes. A semantic diff may additionally show
canonical token/declaration changes or one reviewer-selected display locale, but
must retain access to the raw patch and both revisions' exact profile/catalog
provenance. Merely converting a file is presented as a source-profile rewrite,
not hundreds of unexplained semantic changes.

## Accessibility

Cursor movement, selection, deletion, replacement, screen-reader output, and
keyboard navigation treat each displayed replacement as one semantic token even
when its displayed scalar length differs from stored bytes. Accessibility output
exposes the localized label, token/declaration role, and canonical identity on
request. Qualification requires real editor testing; this paper contract alone
does not claim it passes.
