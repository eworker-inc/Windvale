# Localization workload 1 implementation responsibilities

## Rule

This matrix assigns future work without authorizing implementation before the
replacement Language 1.0 source freeze. No missing boundary may be hidden inside
a compiler plugin, package search, host locale, or editor-only convention.

| Boundary | Durable owner | Required future work |
| --- | --- | --- |
| Descriptor grammar | Language grammar and compiler front door | Scan at byte zero under the 128-byte bound and return exact raw offsets. |
| Artifact byte formats | Source-profile format specification | Preserve exact encoding, order, limits, hash coverage, and malformed-input behavior. |
| Build lock and content resolution | Package/build-plan owner | Supply approved bytes by exact hash without compiler network or installation search. |
| Unicode upstream identity | Language source/Unicode owner | Preserve Unicode 17.0.0 inputs, revisions, hashes, license/provenance, and generated-table equivalence. |
| Unicode table generator | Repository tooling | Generate compact deterministic tables and prove them against pinned UCD/UTS data and normalization tests. |
| Token registry | Source-language owner | Preserve the exact 66 external identities and ordinals for edition 1. |
| Lexicon validation and lookup | Compiler lexer/front door | Validate complete mappings and lower exact stored spellings to canonical token IDs with raw spans. |
| Vocabulary-profile admission | Compiler front door | Bind exact label policy and catalog format with no fallback. |
| Canonical interface identity set | Library/interface owner | Expose the complete source-addressable keys and exact interface hash. |
| Public-label catalog validation | Compiler public-name resolver | Prove complete exact keys, namespaces, labels, collisions, and interface binding. |
| Canonical lowering | Parser/name/type phases | Consume canonical tokens and public declaration identities without language-specific branches. |
| Cache generations | Compiler-service owner | Publish immutable hash-keyed tables atomically, bound waiters/state, and release retired generations. |
| Diagnostics and source maps | Compiler diagnostic owner | Retain raw/profile/catalog provenance and bounded related fields without localizing semantic IDs. |
| Conversion and formatting | Source-tool/editor owners | Convert only recognized keyword/public-label spans, update descriptor, and preserve project-owned content. |
| Package shipment | Release repository and installer | Deduplicate by content hash and install only selected language/developer profiles. |
| Native terminology | Named native reviewers | Approve complete real-language lexicons/catalogs; synthetic fixtures have no translation standing. |
| Verification | Future focused localization owners | Add exact valid, malformed, cross-host, conversion, cache, time, and memory fixtures when implementations exist. |

## Planned verification ownership

| Planning owner | Initial executable scope |
| --- | --- |
| `language1-source-descriptor` | Descriptor bytes, bounds, versions, offsets, and diagnostics. |
| `language1-source-profile-format` | Seven artifact parsers, hashes, ordering, counts, dependencies, and hostile inputs. |
| `language1-unicode-source` | Pinned table generation, NFC/XID/security rules, scripts, numbers, bidi skeletons, and normalization corpus. |
| `language1-source-lexicon` | Complete 66-token mapping, boundaries, collisions, and canonical token equality. |
| `language1-source-vocabulary` | Interface completeness, named parameters, localized resolution, stale catalogs, and canonical identity equality. |
| `language1-source-profile-cache` | Race publication, reuse, limits, negative-result isolation, and generation teardown. |
| `language1-localization-cross-host` | Exact Windows/Linux admission, semantic hashes, time, and memory reports. |

These are planning names, not current verification-registry entries. Add a
focused owner only with the corresponding executable boundary. Paper document
changes do not require broad native qualification.

## Implementation order after source freeze

1. Add the standalone descriptor and artifact reference validator.
2. Generate and qualify the exact Unicode 17.0.0 source tables.
3. Add hash-keyed component admission and private-to-immutable publication.
4. Lower the English lexicon through the existing shared parser path.
5. Add Unicode identifiers and synthetic profile differential cases.
6. Add complete public-interface catalog resolution.
7. Add deterministic conversion, formatter, editor, and diagnostics.
8. Add native-reviewed profiles, installer shipment, and cross-host performance
   gates without changing later compiler phases.
