# Windvale workspace and project manifests

## Status and purpose

Workspace 1 (`.wvws`) and Windvale Project 2 (`.wvproj`) are the native-owned,
deterministic build-input contracts selected by
[Decision 0528](../Documents/Decisions/0528-Workspace-Rooted-Project-2.md).
One explicitly supplied workspace binds a source root. One project selects an exact
root module and dependency-source inventory beneath that root and emits one
self-contained WVB.

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

## Workspace-relative paths

Every project path is relative to the workspace source root, regardless of the
directory containing the `.wvproj`. Encoded path text is nonempty and limited to
4,096 bytes. A path:

- uses `/` as its only separator;
- ends in lowercase `.wv`;
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
location is organizational and has no path-resolution semantics in Project 2.

The repository root contains the explicitly supplied `Windvale.wvws` marker. It
does not contain ordinary component, fixture, or cross-component `.wvproj` files.

## Build command

The general form is:

```text
windvale build --workspace <workspace.wvws> <project.wvproj> [-o <module.wvb>]
```

The repository `Tools/Native/Build-Wvb` helpers bind the checked-in workspace and
accept a repository project path plus optional output. Both forms invoke the same
native Workspace 1 and Project 2 parser and build path.

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

`Tools/Windvale.Project/Project-Manifest-Core.wv` owns portable Project 2 parsing
and path extraction. It performs no host file access or ambient root discovery.
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

Project 2 deliberately excludes source discovery, globs, environment expansion,
conditional compilation, arbitrary build actions, capability authorization,
packages, versions, lockfiles, project references, binary libraries, runtime WVB
linking, multiple roots, tests, resources, native containers, and multiple targets.

Workspace 1 is a source-root binding, not a package resolver or target graph. A
future package or project-reference layer must preserve exact source and dependency
identity without silently widening this format.
