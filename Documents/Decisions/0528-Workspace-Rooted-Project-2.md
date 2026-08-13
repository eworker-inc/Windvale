# Decision 0528: Workspace-rooted Windvale Project 2

- Date: 2026-08-12
- Status: Implemented candidate
- Replaces on the normal path: [Decision 0075](0075-Minimal-Deterministic-Windvale-Projects.md)
- Requires: [Decision 0527](0527-Native-Only-Forward-Development-Boundary.md)

## Context

Project 1 resolves every source path beneath the directory containing its
`.wvproj`. That containment is safe and deterministic, but a monorepository project
that composes `Compiler/`, `Foundation/`, `Runtime/`, `Linker/`, or `Libraries/`
must place its manifest at their repository common ancestor. The completed migration
handles 227 active manifests and removes all 168 project manifests that were at the
repository root.

Moving all manifests into an unbounded `Projects/` directory and accepting `..`
would trade visual organization for directory escape, ambiguous link behavior, and
host-dependent resource identity. Source discovery or a Git-root search would also
make ambient host state part of the build input.

## Decision

Introduce Workspace 1 and Project 2.

### Workspace 1

`Windvale.wvws` is a strict workspace marker containing the exact header
`windvale-workspace 1`. Its containing directory is the workspace source root.
Callers supply the workspace explicitly; tools do not search parent directories.

The hosted boundary resolves the workspace, project, and sources beneath one
caller-bound workspace instance. It rejects a project outside the workspace and
duplicate host resource identities. The output is a caller-selected publication
target and may be outside the source workspace, but it must not alias any input. A
host adapter must reject link, junction, mount, case, short-name, or other aliases
that escape or duplicate the bound workspace input identity.

### Project 2

Project 2 retains Project 1's bounded line format and directives but uses the exact
header `windvale-project 2`. Every `root` and `source` path is relative to the
workspace root, not the manifest directory. Paths remain ASCII-safe, `/`-separated,
explicit, and free of absolute roots, empty segments, `.`, and `..`.

The manifest location does not enter WVSS, WVB, module identity, source ordering, or
artifact bytes. Reordering dependency directives remains semantically irrelevant.
One project still selects one root plus at most 63 dependencies and emits one WVB.

The repository-specific `Build-Wvb` helpers bind the checked-in `Windvale.wvws`
explicitly when invoking the native build driver. A general CLI accepts an explicit
workspace and project pair.

### Migration

All active manifests move to Project 2. Locally owned projects remain beside their
component when that is clearest. Cross-component aggregates move under
`Projects/<owner>/`. Project 1 is not retained in the normal parser or build path;
its exact implementation and inputs remain available from history and the immutable
Stage 0 recovery release.

No C# parser, managed project, managed test, or managed build entry point advances
for Workspace 1 or Project 2.

## Consequences

- The repository root retains one workspace marker instead of cross-component
  project manifests.
- All 227 active manifests use Project 2; 168 former root manifests now live with
  their owner or under `Projects/<owner>/`.
- Projects can move without changing their source inventory or output bytes.
- Workspace containment remains explicit without admitting parent traversal.
- Package manifests and lockfiles remain separate future distribution contracts.
- The native project parser, build driver, wrappers, fixtures, and verification
  planner advance together.

## Current evidence and qualification boundary

- `Test-Workspace-Project2` owns eight native valid and rejection cases, including
  Project 1 rejection, traversal, absolute path, duplicate path, malformed workspace,
  and nested-workspace containment.
- Repository audit requires 227 Project 2 manifests, no root `.wvproj` files, and no
  missing, duplicate, or escaping source paths.
- The same project and workspace parser sources and suite owner are checked in for
  Windows and Linux. Independent Linux execution from the final source state remains
  the cross-host qualification boundary.

## Reconsideration triggers

Add workspace metadata only when a measured consumer needs a bounded project index,
target selection, or provider policy. Do not add globs, executable build actions,
packages, capability grants, or runtime linking to Project 2.
