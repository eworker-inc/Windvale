# Windvale workspace and project manifests

## Status and purpose

Workspace 1 (`.wvws`) and Windvale Projects 2 and 3 (`.wvproj`) are the
native-owned, deterministic build-input contracts selected by
[Decision 0528](../Documents/Decisions/0528-Workspace-Rooted-Project-2.md).
One explicitly supplied workspace binds a source root. One project selects an exact
root module and dependency-source inventory beneath that root. Project 3 also
selects an exact Language 1.0 source-input lock and composite source profile. Both
versions emit one self-contained WVB.

Project manifests are not Windvale source, packages, workspaces, runtime-link
descriptions, or build programs. Source `import` declarations remain the semantic
dependency graph. Workspace and project paths are host build inputs and do not enter
WVSS, WVB, module identity, canonical ordering, or artifact bytes.

Project 1 is retired from the normal path. Its qualified historical contract remains
recorded by [Decision 0075](../Documents/Decisions/0075-Minimal-Deterministic-Windvale-Projects.md)
and the immutable Stage 0 recovery release.

## Workspace 1 text format

A workspace is strict UTF-8 without a byte-order mark and contains exactly:

```text
windvale-workspace 1
```

The header may be followed by one LF or CRLF. No directive, comment, blank line,
leading or trailing whitespace, or trailing byte is accepted. The workspace path
must end in lowercase `.wvws`. Its containing directory is the workspace source
root.

The caller supplies the workspace explicitly. Tools must not search parent
directories, inspect Git state, consult an environment variable, or infer a root
from the project path.

## Project 2 text format

The project is strict UTF-8 without a byte-order mark and is limited to 65,536
bytes. It contains one directive per nonempty line. Lines use LF or CRLF.
Whitespace-only lines, leading or trailing whitespace, comments, escapes, and
trailing tokens are rejected.

```text
windvale-project 2
root "Compiler/Windvale/Main.wv"
source "Libraries/Foundation/Bytes.wv"
emit wvb
```

The header must be the first line and match `windvale-project 2` exactly. The
remaining directives are:

- exactly one `root "<path>"`;
- zero through 63 `source "<path>"` directives; and
- exactly one `emit wvb`.

Directive order after the header is not semantically significant. Unknown,
repeated singleton, malformed, missing, or out-of-bound directives are rejected
before source compilation.

## Project 3 text format

Project 3 preserves Project 2's source inventory and adds the immutable inputs
needed to admit a Language 1.0 source profile:

```text
windvale-project 3
root "Compiler/Windvale/Main.wv"
source "Libraries/Foundation/Bytes.wv"
source-input-lock "Inputs/Source-Inputs.wvlock"
source-input-lock-sha256 9e2ca572552ed52ed496142d18539f2f55fed2bbdfb1ec602f283b5d72386f3e
source-profile "Inputs/En-Source-Profile.wvsp"
emit wvb
```

It accepts exactly one `source-input-lock`, one
`source-input-lock-sha256`, and one `source-profile` directive in addition to
Project 2's root, zero-through-63 sources, and emission. The lock digest is
exactly 64 lowercase hexadecimal digits. The two artifact paths are ordinary
workspace-relative inputs ending in lowercase `.wvlock` and `.wvsp` respectively.
They obey the same lexical path, containment, alias, and input/output-separation
rules as source paths.

This first Project 3 profile binds one exact source-profile identity/version for
the complete source closure. Every source descriptor must select that same value.
Supporting a mixed-profile source closure later must add explicit per-source
selection without weakening the one-profile format or guessing from source text.

The project binds bytes; it does not name a registry, content store, network
location, installation, or fallback. The build provider reads the selected lock
and profile once. The compiler hashes the lock against the manifest digest before
parsing it, obtains the selected profile hash from the admitted lock, and hashes
the supplied profile before parsing it. A missing, changed, malformed, unlisted,
or unsupported profile fails without output publication. Project 2 remains
byte-for-byte valid and does not acquire an ambient profile.

## Project 4 target-aware format

Project 4 is an accepted format with parser implementation in progress, not yet
a qualified production build entry point. See
[the explicit-target decision](../Documents/Decisions/0961-Require-An-Explicit-Target-In-Project-4.md).
The existing profile-only build driver must reject Project 4 rather than route
it through descriptorless compilation.

The first line is exactly `windvale-project 4`. All Project 3 directives and
bounds apply, with exactly one additional directive:

```text
target-descriptor "Inputs/Target.wvtd"
```

The path ends in lowercase `.wvtd` and uses the same 4,096-byte lexical path
bound. Directive order is immaterial. Missing or repeated targets produce
`WVP1004`; malformed directives produce `WVP1003`; invalid paths produce
`WVP1006`. Project 2 and Project 3 continue rejecting this directive.

Parser acceptance does not validate target contents or prove host containment.
The build provider must snapshot the selected target as a distinct, bounded
ordinary input, reject aliases and workspace escapes, and exclude it from
output publication targets. The existing target-aware compiler admission phase
validates the descriptor and authenticates target and foreign-catalog evidence
before compilation and publication. No target may be inferred from the host.
The exact target bytes must participate in reusable build-result identities.

### Native inventory handoff

The native manifest tool's `--inventory <project.wvproj>` mode accepts Project 4
only. On success it emits one compact JSON object and a line feed, with these
fields in order: `inventoryVersion` (1), `projectVersion` (4), `sources` (root
first, followed by the declared sources), `sourceInputLock`,
`sourceInputLockSha256`, `sourceProfile`, and `targetDescriptor`.
Paths are the exact admitted workspace-relative strings; the lock digest is the
admitted lowercase hexadecimal string. No source bytes or host-resolved paths
are emitted. Rejection writes a diagnostic, returns nonzero, and emits no
successful inventory. The one-argument human-readable report remains separate.

The development split coordinator accepts the explicit product inputs followed
by `--workspace <workspace.wvws> --project <project.wvproj>
--manifest-reader <native-reader> <new-output.wvb>`. It snapshots the project
before invoking the reader and consumes only inventory version 1 for Project 4.
It admits the selected source, profile, target, and authenticated foreign catalog
through the existing compiler phases. It rejects a pre-existing output rather
than replacing it. This development integration is not installed-toolchain or
cross-host qualification. The maintained admission owner has a focused
`--project4-products <reader> <admitter> <validator> <analyzer> <emitter>`
selection with 16 cases, including explicit Windows and Linux x64/no-foreign
targets. Complete qualification and broader target, alias, and
profile evidence remain required before promotion.

Maintained [explicit target inputs](../Projects/Targets/README.md) provide the
registered Windows and Linux x64/no-foreign descriptors. The target writer's
`--target <registry-name>` option selects these without consulting the host;
its original one-path invocation retains the Linux foreign-ABI descriptor.

## Workspace-relative paths

Every project path is relative to the workspace source root, regardless of the
directory containing the `.wvproj`. Encoded path text is nonempty and limited to
4,096 bytes. A path:

- uses `/` as its only separator;
- ends in its directive's lowercase suffix: `.wv`, `.wvlock`, `.wvsp`, or
  Project 4's `.wvtd`;
- contains segments that begin and end with an ASCII letter or digit;
- permits `.`, `_`, or `-` only inside a segment;
- contains no native separator, colon, control character, quotation mark, empty
  segment, `.` segment, `..` segment, absolute root, or other character; and
- resolves to an identity distinct from the root and every other source.

The project manifest itself must resolve beneath the workspace. The output is a
caller-selected publication target and may be outside the source workspace; it must
differ from the workspace, project, root, and every source input.

The hosted workspace provider owns containment and identity for the workspace,
project, and source inputs. It must reject an input that escapes or aliases the
workspace through a symlink, junction, mount, short name, case variation, or other
host mechanism. The publication provider separately proves that the output does not
alias an input. Lexical prefix comparison alone is not sufficient evidence for an
untrusted provider.

## Repository organization

A locally contained project normally lives beside the component or fixture it owns.
A cross-component project normally lives under `Projects/<owner>/`. Manifest
location is organizational and has no path-resolution semantics in Project 2 or 3.

The repository root contains the explicitly supplied `Windvale.wvws` marker. It
does not contain ordinary component, fixture, or cross-component `.wvproj` files.

## Build command

The general form is:

```text
windvale build --workspace <workspace.wvws> <project.wvproj> [-o <module.wvb>]
```

The repository `Tools/Native/Build-Wvb` helpers bind the checked-in workspace and
accept a repository project path plus optional output. Both forms invoke the same
native Workspace 1 and Project 2/3 parser and build path.

The build reads the workspace once, project once, and every explicit source once.
It invokes the bounded compiler and verifies the generated WVB before atomic or
exact publication. No output is created or modified until workspace admission,
project parsing, path resolution, source reads, compilation, WVB verification, and
output validation have succeeded.

Reordering `source` directives cannot change successful WVB bytes.

## Diagnostics and exit behavior

Workspace diagnostics use the `WVW` family:

| Code | Meaning |
| --- | --- |
| `WVW1001` | Invalid header or unsupported workspace version |
| `WVW1002` | Invalid UTF-8, byte order mark, line ending, or trailing content |
| `WVW1003` | Project, source, or output escapes or aliases the workspace |

Project diagnostics retain the `WVP` family:

| Code | Meaning |
| --- | --- |
| `WVP1001` | Invalid header or unsupported project version |
| `WVP1002` | Manifest byte limit or strict-UTF-8 failure |
| `WVP1003` | Unknown or malformed directive |
| `WVP1004` | Missing or repeated singleton directive |
| `WVP1005` | Source-module count exceeds the version bound |
| `WVP1006` | Invalid or noncanonical workspace-relative path |
| `WVP1007` | Duplicate resolved source identity |

Malformed workspace or project input and compiler diagnostics exit as compilation
failure `1`. Invalid command syntax exits `64`. Host I/O and authorization failures
retain exit `74`.

## Windvale-owned boundary

`Tools/Windvale.Project/Project-Manifest-Core.wv` owns portable Project 2/3
parsing and path extraction. It performs no host file access or ambient root
discovery.
The bounded native build driver owns exact Workspace 1 marker validation together
with workspace-rooted resource composition.

`Tools/Windvale.Build/Compiler-Build-Driver.wv` is the native hosted composition
boundary. It accepts the explicitly supplied workspace and project resources,
derives source resource names beneath the workspace, rejects conservative ASCII
case aliases, compiles the supplied snapshots, verifies the generated WVB, and
publishes only accepted bytes.

Provider-level canonical identity remains necessary for untrusted host trees. The
repository wrappers reject reparse/link-bearing workspaces as a bounded current
adapter until a rights-limited canonical workspace-resource capability replaces the
bootstrap host-file leaf.

No C# implementation advances for these contracts.

## Boundary and deferred features

Projects 2 and 3 deliberately exclude source discovery, globs, environment expansion,
conditional compilation, arbitrary build actions, capability authorization,
packages, project references, binary libraries, runtime WVB
linking, multiple roots, tests, resources, native containers, and multiple targets.

Project 3's source-input lock is not a general package lockfile. It contains exact
source-profile and public-catalog identities and hashes only; package resolution,
component acquisition, and approved content-store lookup remain separate build-plan
responsibilities.

Workspace 1 is a source-root binding, not a package resolver or target graph. A
future package or project-reference layer must preserve exact source and dependency
identity without silently widening this format.
