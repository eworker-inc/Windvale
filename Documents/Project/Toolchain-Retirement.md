# Toolchain retirement

> Status: Current migration and deletion checklist
> Authority: Informative; accepted contracts and qualification gates apply
> Last reviewed: 2026-10-07

Mark old implementations before replacing them, track their users, then remove
them when the supported replacement works and no required construction edge
remains. The compiler and its tools take priority. WVDB and secondary
applications may be migrated later under the
[language and essential-library scope decision](../Decisions/0976-Focus-Windvale-1.0-On-The-Language-And-Essential-Libraries.md).

## Where the markers live

[The retirement registry](Toolchain-Retirement.json) assigns stable IDs to old
mechanisms. Each entry records its source owners and search anchors, the exact
functionality to replace, proposed successor files, reference inventory,
converted scopes and removal gates. These are external source markers: they
do not alter frozen inputs, pinned artifact identities or source semantics.

The reference inventory includes source imports, project source inclusion and
host tools that name the old files. A reference is a review item, not proof that
the old path executes. Some references are tests, metadata, or callers of other
functions in the same module. Historical source restored from Git and embedded
binary dependencies also require the named bootstrap audit. An empty search
result alone does not prove that deletion is safe.

| Marker | What will be replaced | Current state |
| --- | --- | --- |
| `MEM-001` | General byte/text lifetime handling through record-return checkpoints and loop compaction. | Migrating; shared ownership and complete exit/aggregate cleanup must replace these uses. |
| `MEM-002` | Historical native byte concatenation and text/integer allocation mechanisms. | Migrating; preserve required operations while connecting their storage to ownership and budgets. |
| `MEM-003` | Compiler working tables and stacks repeatedly rebuilt as byte sequences. | Planned; use typed owned collections where appropriate. Actual WVB/WVO serialization remains bytes. |
| `MEM-004` | Remaining users of the historical byte-construction leaf. | Migrating; the compiler signature and machine-code-directory writers have candidate conversions to canonical reserved builders. |
| `MEM-005` | Interpreter budget state and execution frames repeatedly rebuilt as bytes. | Planned; native working storage needs separate reclamation evidence from guest accounting. |
| `BOOT-001` | Temporary serializer projection and staging-capacity construction bridge. | Required bootstrap; exact self-lowering and independent reconstruction remain open. |
| `BOOT-002` | Predecessor and intermediate split compiler construction edges. | Required bootstrap; the selected successor must reconstruct the full current tool set without them. |
| `BOOT-003` | Pinned inspection/execution defaults and redundant front doors. | Required bootstrap; current supported delivery must replace ordinary callers first. |

The registry's owner anchors identify the relevant part of a file. A file may
also contain current behavior that must stay. Bounded scratch arenas and owned
typed arenas remain valid designs; the retirement target is obsolete general
lifetime management, not every use of an arena or byte sequence.

## Updating a conversion

1. Find the marker and inspect its owners, references and required bootstrap
   gates. Classify each actual user as maintained, required bootstrap, recovery
   only, or parked. A parked application does not require preserving the old
   implementation.
2. Convert a coherent maintained path. Record the converted scope and its
   verification in the entry; a partial conversion does not mark the whole
   owner or module converted. Keep candidate, verified and qualified claims
   separate.
3. Refresh the reference inventory. Review remaining references and dynamic
   construction dependencies before declaring the old functionality unused.
4. Set `ReadyToRetire` only when the replacement and every applicable removal
   gate are proven. Then remove the old implementation and redundant callers,
   keeping required immutable recovery evidence. Remove completed entries from
   the active registry; Git preserves the conversion and deletion history.

Run the inspection from the repository root:

```text
node Tools/Documentation/Inspect-Toolchain-Retirement.mjs
node Tools/Documentation/Inspect-Toolchain-Retirement.mjs --update
node Tools/Documentation/Inspect-Toolchain-Retirement.mjs --check
```

Inspection is read-only by default. `--update` refreshes syntactic references
without promoting a status or deleting code. `--check` also fails on inventory
drift. Missing owner, converted-scope or replacement anchors require review.
The registry records the base revision, not a qualification identity for a
possibly changed working tree. Use the existing verification owners for the
changed contract; this inspection does not replace runtime or cross-host checks.

The [completion plan](Compiler-Tools-And-Libraries-Completion-Plan.md) owns the
delivery sequence. This checklist owns retirement tracking and does not add a
compatibility requirement for secondary applications.
