# Windvale project manifest

## Status and purpose

Windvale Project 1 (`.wvproj`) is the candidate deterministic build-input format for one statically composed Windvale WVB artifact. It replaces repeated host CLI source-path arguments with one bounded manifest without changing Windvale source semantics, import resolution, WVSS, WVB, or runtime behavior.

A project manifest is Windvale-owned declarative text, not executable `.wv` source, a source module, a package, a workspace, or a runtime-link description. Source `import` declarations remain the only semantic dependency graph. The project identifies the exact host resources made available to that graph.

## Text format

The manifest is strict UTF-8 without a byte-order mark and is limited to 65,536 bytes. It contains one directive per nonempty line. Lines use LF or CRLF. Whitespace-only lines, leading or trailing whitespace, comments, escapes, and trailing tokens are rejected in version 1.

```text
windvale-project 1
root "Source/Main.wv"
source "Source/Library.wv"
emit wvb
```

The header must be the first line and must match `windvale-project 1` exactly. The remaining directives are:

- exactly one `root "<path>"`;
- zero through 63 `source "<path>"` directives; and
- exactly one `emit wvb`.

Directive order after the header is not semantically significant. The root plus source count remains within the compiler's existing 64-module bound. Unknown, repeated singleton, malformed, missing, or out-of-bound directives are rejected before any source is compiled.

Path text is nonempty strict UTF-8 and limited to 4,096 encoded bytes. Version 1 paths are deliberately ASCII-safe: every segment begins and ends with an ASCII letter or digit, while interior characters may additionally use `.`, `_`, or `-`. A project path:

- is relative to the directory containing the manifest;
- uses `/` as its only separator;
- ends in lowercase `.wv`;
- contains no space, native separator, colon, control character, quotation mark, empty segment, `.` segment, `..` segment, or other character outside that ASCII-safe set; and
- resolves to a source distinct from the root and every other source under the host's path-identity rules.

Absolute paths and project-directory escape are rejected. Project paths are host build inputs only. Neither their text nor their resolved native paths enter WVSS, WVB, module identity, canonical ordering, or artifact bytes.

## Build command

```text
windvale build <project.wvproj> [-o <module.wvb>]
```

The project path must use `.wvproj`. The output must use `.wvb`; when omitted, it replaces the project extension. The output must differ from the manifest and every source input.

The build command reads the manifest once, resolves and reads every explicit source once, and invokes the same bounded `Seedˉcompiler.Compileˉmodules` operation as `windvale compile`. The root is passed as the root source and every `source` entry as an explicit dependency. Existing compiler validation remains authoritative for declared module names, dependency-order independence, missing imports, reachability, cycles, profiles, declaration visibility, and canonical WVB production.

No output is created or modified until manifest parsing, path resolution, source reads, compilation, generated-WVB verification, and output-path validation have all succeeded. Reordering `source` directives cannot change successful WVB bytes.

## Diagnostics and exit behavior

Project diagnostics use the `WVP` family and identify the manifest line and column when available:

| Code | Meaning |
| --- | --- |
| `WVP1001` | Invalid header or unsupported manifest version |
| `WVP1002` | Manifest byte limit or strict-UTF-8 failure |
| `WVP1003` | Unknown or malformed directive |
| `WVP1004` | Missing or repeated singleton directive |
| `WVP1005` | Source-module count exceeds the version-1 bound |
| `WVP1006` | Invalid or noncanonical project path |
| `WVP1007` | Duplicate resolved source path |

Malformed projects and compiler diagnostics exit as compilation failure `1`. Invalid command syntax exits `64`. Host I/O and authorization failures retain exit `74`. Existing compiler failures retain their `WVC` code, phase, and source location.

## Windvale-written parser boundary

`Tools/Windvale.Project/Project-Manifest-Core.wv` is the portable Windvale-owned parser for the Project 1 text contract. `Windvaleˉprojectˉscanˉmanifest(bytes)` performs the manifest byte bound, strict UTF-8, line-ending, directive, singleton, module-count, and canonical-path checks without file access or ambient host state. Its status values map one-to-one to `WVP1001` through `WVP1007`. A successful scan retains the root and dependency path locations as bounded immutable byte spans; `Windvaleˉprojectˉpathˉat` exposes the root at index zero followed by sources in manifest order and rejects an invalid or inconsistent view without reading outside its inputs.

`Project-Manifest-Tool.wv` is the first hosted shell over that core. It reads exactly one supplied manifest resource and emits a deterministic status/path report. The C# parser remains the reference oracle and the normal `windvale build` implementation while the Windvale core is candidate evidence; focused conformance compares successful path values and exact `WVP` line/column failures between them.

The hosted shell is also the first native project consumer candidate. Under ABI 14 it reads a real `.wvproj` through the native file-input table and produces the same output, diagnostics, and exit code under the interpreter, Windows W^X JIT, and linked WVO/AOT. The native path admits the core's borrowed-byte record fields, `Textˉtoˉutf8`, `Bytesˉfromˉu32ˉlittle`, and bounded `Bytesˉconcat` without adding a capability, service-table slot, execution-context field, or project-format feature. A supplied Stage 0 file reader is required to remain unused during native execution.

This is development-candidate evidence, not cross-host qualification or a standalone executable. The current WVO still requires its verified fragment metadata and Stage 0 publication owner. Project-relative resolution, source reads, compiler invocation, and atomic WVB publication therefore remain in the C# `windvale build` path.

The portable parser deliberately does not resolve a manifest path against a host filesystem. Project-relative native path combination, Windows/Linux identity comparison, duplicate resolved-resource detection, source reads, and publication remain in the host-owned reader. Moving the build driver into Windvale must preserve that boundary rather than teaching portable source about native separators, roots, case rules, or process working directories.

## Boundary and deferred features

Project 1 deliberately excludes source discovery, globs, environment expansion, conditional compilation, arbitrary build actions, capability authorization, packages, versions, lock files, project references, binary libraries, runtime WVB linking, multiple roots, tests, resources, native containers, and workspaces.

The first accepted consumer is the complete 12-module Windvale compiler source closure. Building its project must reproduce the existing verified 599,868-byte WVB with SHA-256 `9673bf3331763181f443ec67b7a513bc66daa718969f7f6b0d197a4186071066`. This proves deterministic project input selection. The separate Windvale-written project parser does not make project syntax part of the compiler, move host path resolution into portable code, or change the Stage 1 to Stage 2 bootstrap contract.
