# Decision 0145: First capability-bearing static platform library

- Date: 2026-08-03
- Status: Implemented candidate with local Windows evidence; cross-host qualification pending
- Advances: Stage 0 static source composition and the first `Libraries/Platform` consumer
- Retains: Canonical WVB 1.6, current `portable`/`hosted`/`system` profile bytes, the seven-entry Seed capability catalog, explicit runtime grants, bounded hosted resource semantics, and no runtime module linker
- Refines: [Decision 0140](0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md)

## Context

Decision 0140 accepts capability-bearing static libraries and a layered future filesystem family, but the implemented compiler still rejects every imported module that is not portable and capability-free. That prevents an application-facing library from wrapping even the existing bounded `file.read_bytes` leaf. Adding a path API or kernel namespace before libraries can state and propagate authority would invert the intended architecture.

The current profiles still combine platform scope and authority. Replacing them and extending WVB metadata is a larger independent contract. The smallest compatible slice is therefore a strict profile-compatibility rule plus compile-time transitive capability approval, retaining the existing WVB format and hosted resource behavior.

## Decision

- Permit imported source modules to declare catalog capabilities while retaining the existing no-module-data and exported-function-only library shape.
- Treat the current profiles as a temporary import-compatibility lattice: `portable` may import only `portable`; `hosted` may import `portable` or `hosted`; `system` may import all three. Reject an import edge that would give a less-authorized module a more-authorized dependency with `WVC0010`.
- Require every module to redeclare every capability required anywhere in its transitive dependency closure. Missing approval fails composition with `WVC0013` before semantic lowering. A dependency update therefore cannot silently expand application or intermediate-library authority.
- Keep the root declarations as the final WVB capability set. Existing semantic lowering sorts them by ordinal name, so dependency input order and duplicate transitive requirements cannot alter bytes or create duplicate capability records.
- Retain explicit runtime authorization as a separate boundary. Source approval produces a module requirement; it does not grant the capability.
- Move the portable `WVRS 1` parser and lookup algorithm from `Operating-System/Services` to `Libraries/Foundation/Resources`, where its capability-free format policy belongs.
- Add `Libraries/Platform/Resources/Hosted-Resource-Store.wv`. It declares `file.read_bytes`, accepts an opaque hosted resource name plus an opaque `WVRS 1` entry name, and returns the portable typed `Resourceˉstoreˉresult`.
- Prove deterministic composition in both dependency orders, one canonical `file.read_bytes` requirement, explicit authorization, a successful live lookup, rejection when the application omits approval, rejection when an intermediate library omits transitive approval, and rejection of a hosted dependency under a portable root.

## Consequences

Windvale now has one real capability-bearing static platform library. Reusable policy can remain ordinary Windvale source while the final self-contained WVB exposes the complete approved capability set. No runtime linker, package search, ambient authority, or provider-specific value crosses the library boundary.

This does not implement the durable independent platform-scope metadata accepted by Decision 0140. `hosted` remains a temporary coarse compatibility label, optional capabilities remain absent, and the Windvale-written compiler has not yet adopted this candidate composition extension.

The new library is a hosted resource adapter, not a general filesystem library. `file.read_bytes` still accepts an opaque host-resolved resource name, snapshots at most 4 MiB, and reports expected failures as runtime traps. Package resources, `WVRS 1`, mutable application storage, and future filesystem capabilities remain distinct. No paths, directories, enumeration, handles, offsets, writes, provider discovery, revocation result, or service-restart result are introduced.

## Next boundary

Before a shared filesystem library is implemented, specify the first source/module representation for independent platform scope and canonical capability interface identity/version, or explicitly accept another bounded compatibility encoding. Then select one rights-limited, read-only filesystem instance operation with typed recoverable results and identical Windows, Linux, and Windvale OS semantics. Native paths and the current opaque hosted resource leaf must not become that public contract by accident.

[Decision 0153](0153-First-Versioned-Read-Only-Directory-Capability.md) subsequently accepts `_v1` in the canonical capability name as the bounded compatibility encoding and implements one pre-bound immutable-directory read with strict segments, exact 3 KiB chunks, typed lifecycle outcomes, and mandatory provider validation. Independent platform/version metadata and the Windvale OS provider binding remain open.

## Reconsider when

- Independent platform scope can replace the temporary profile compatibility lattice.
- A library must approve required and optional interfaces separately.
- Typed capability values introduce multiple filesystem, directory, file, or watch instances with independent rights and generations.
- Static internalization makes capability auditing, provider replacement, or package updates materially impractical.
