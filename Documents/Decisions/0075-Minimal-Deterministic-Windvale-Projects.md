# Decision 0075: Minimal deterministic Windvale projects

- Date: 2026-08-01
- Status: Implemented; cross-host qualification pending
- Preserves: Windvale source imports, WVSS 1, WVB 1.6, source composition, compiler semantics, and runtime behavior

## Context

Windvale's bounded source-module composition already gives the compiler one root plus as many as 63 explicit dependencies, resolves imports by declared module name, rejects missing or unreachable inputs, and produces one canonical self-contained WVB. The host CLI still requires every physical dependency path to be repeated on each invocation. The complete compiler now has a stable 12-module inventory, so its bootstrap scripts carry the same long path list independently of source imports.

Putting paths into `.wv` imports would make native filesystem behavior part of source semantics. Treating WVSS as a project archive would contradict its path-free compiler-input boundary. A package manager, workspace graph, arbitrary build language, or multi-target system would add unresolved distribution and execution contracts when the measured need is only deterministic input selection.

## Decision

- Introduce the strict UTF-8, line-oriented Windvale Project 1 manifest with extension `.wvproj` and exact header `windvale-project 1`.
- Require exactly one relative root path, zero through 63 relative dependency-source paths, and exact emission `wvb`. Paths use canonical `/`, remain relative to the manifest, use ASCII-safe alphanumeric segments with `.`, `_`, or `-` only in their interiors, and reject native roots, parent traversal, empty segments, non-`.wv` inputs, prohibited characters, oversize values, and duplicate resolved identities.
- Bound one manifest to 65,536 bytes and one encoded path to 4,096 bytes. Reject malformed UTF-8, unsupported headers, comments, whitespace-only lines, escapes, unknown directives, repeated singleton directives, and extra tokens with stable `WVP` diagnostics.
- Keep source `import` declarations authoritative for semantics. A manifest lists available source resources but does not declare visibility. The existing compiler continues to reject missing imports, cycles, unreachable inputs, invalid dependency profiles, and declaration conflicts.
- Add `windvale build <project.wvproj> [-o <module.wvb>]`. It resolves paths relative to the manifest and invokes the same source-read, `Seedˉcompiler.Compileˉmodules`, generated-WVB verification, and publish-after-success path as `windvale compile`.
- Keep project names, paths, ordering, and output locations outside WVSS and WVB. Reordering dependency directives cannot change successful artifact bytes.
- Add `Tools/Windvale.Project` as the tool-owned manifest parser and resolver. Do not add project parsing, host paths, or file discovery to `Compiler/Reference`, `Compiler/Windvale`, `Compiler/Native`, or the runtime.
- Make `Examples/Foundation/Module-Composition-Demo.wvproj` the small exact-byte fixture and `Windvale-Compiler.wvproj` the first complete consumer. The bootstrap verifier builds Stage 1 through the compiler project; the Windvale-written Stage 1 compiler still receives the explicit canonical source inventory when building Stage 2.
- Defer globs, source discovery, environment expansion, build scripts, capability grants, packages, versions, lock files, project references, binary libraries, runtime linkage, multiple targets, tests, resources, native containers, and workspaces.

## Rejected alternatives

Using a root `.wv` file as both source and build manifest was rejected because the root already owns semantic imports and profile declarations. Adding native paths or output policy would leak host behavior into portable source.

Putting path records into WVSS was rejected because WVSS is the canonical path-free byte collection consumed by the portable compiler. It is not a source package, archive, or build manifest.

Implicit directory scanning and globs were rejected because host enumeration, case rules, ignored files, and accidental source additions would become build inputs. Project 1 remains an explicit first-read snapshot.

JSON, XML/MSBuild-style evaluation, and executable Windvale build scripts were rejected for the first slice. Each would require substantially more syntax, evaluation, or self-hosting machinery than the bounded inventory problem demonstrates.

## Candidate evidence

The focused project conformance test passes in Release and covers valid parsing, exact 64-module acceptance, the 65th-module rejection, manifest/path byte bounds, malformed headers and directives, BOM and malformed UTF-8 rejection, noncanonical paths, duplicate resolved paths, and manifest-relative resolution independent of the process working directory.

The CLI builds `Examples/Foundation/Module-Composition-Demo.wvproj` to the existing canonical WVB with SHA-256 `0980b7178943be516cd9b6924f179d5977ca147e11bf105c5063ea078c645b60`. The project and repeated-`--module` paths produce byte-identical output. A malformed project returns `WVP1004` and preserves an existing output byte for byte.

`Windvale-Compiler.wvproj` selects the exact canonical 12-module, 677,073-source-byte closure and produces the established 599,868-byte Stage 1 compiler with SHA-256 `9673bf3331763181f443ec67b7a513bc66daa718969f7f6b0d197a4186071066`.

Windows Standard, the complete bootstrap convergence verifier, Windows Qualification, Debian Qualification, portable-artifact comparison, and independent GitHub verification remain required before this decision becomes qualified.

## Consequences

Windvale gains a stable project concept without a second dependency graph or a new compiler/runtime format. Developers can check in one bounded build description instead of duplicating long CLI argument lists, and the complete compiler closure becomes the first real project rather than a synthetic example.

The Stage 0 C# tool currently parses and resolves projects. This does not yet prove a Windvale-written project reader, native build driver, package manager, or .NET-free bootstrap. A later Windvale-owned implementation can consume the same manifest only after hosted file input, output, diagnostics, and process entry satisfy the applicable native ownership gate.

Project 1 produces one self-contained WVB. A future need for independently distributed source packages, binary references, multiple targets, or workspaces must define its own identity, version, verification, and resolution boundaries rather than silently expanding this manifest.

## Reconsider when

- Two real outputs require shared project inputs or named target selection.
- A source package must be distributed and resolved outside one repository.
- Native PE, ELF, or Windvale containers require explicit target metadata beyond `emit wvb`.
- A Windvale-written build driver can parse and resolve the manifest under the same bounded deterministic contract.
