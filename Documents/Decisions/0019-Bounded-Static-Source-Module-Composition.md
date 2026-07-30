# Decision 0019: Bounded static source-module composition

- Date: 2026-07-30
- Status: Accepted, implemented, and cross-host qualified at `df80f91`

## Context

The qualified Windvale assembler and linker are necessarily large single-source modules because Seed previously had no way to reuse Windvale-written functions across source files. Phase 7 needs a real module seam before extracting duplicated scanning and binary-format behavior, but adding runtime WVB linkage, dynamic loading, package discovery, or a general library system would create several new bootstrap loops at once.

## Decision

Add a deliberately small compile-time source-module facility:

- `import <Moduleˉname>;` declarations appear immediately after the module declaration and before every other declaration.
- The compiler receives one root source plus an explicit set of dependency sources. It performs no path lookup, package search, network access, or ambient filesystem access.
- The set is bounded to 64 modules, 4 MiB of characters per source, and 16 MiB of characters in aggregate.
- Declared module names are unique. Imports must be unique, present, acyclic, and reachable from the root. Extra supplied source is rejected rather than silently ignored.
- Imported modules use the `portable` profile and, in this first slice, contain only imports and exported functions. They cannot declare capabilities, data, records, or enums, and every function must be explicitly exported as part of that source module's surface.
- Each dependency is semantically checked against only its own declarations and transitive imports, so it cannot acquire a root or sibling function or capability accidentally during flattening. Composition then creates one ordinary WVB module. Dependency functions are internalized; only exports declared by the root remain WVB exports. Function names occupy one combined ordinal namespace.
- Dependency input order cannot affect the output. Normal Seed semantic analysis, lowering, canonical encoding, and mandatory WVB verification run after composition.
- The CLI accepts dependencies only through repeated `--module <dependency.wv>` options and writes no output until the complete composition and generated WVB have succeeded.

This is static source composition, not a WVB import table or runtime linker. WVB remains version 1.6 and runtime behavior is unchanged.

## Consequences

Windvale can now move a pure function into a separately owned source module without changing the runtime, bytecode verifier, or host capability model. The explicit source set makes builds reproducible on Windows and Linux and keeps native path behavior in the CLI rather than the compiler.

This first contract intentionally does not provide aliases, qualified lookup, private dependency helpers, imported data or nominal types, version resolution, package manifests, or independently distributed binary libraries. Those facilities require evidence from real assembler, linker, and compiler extractions before acceptance. A dependency function name can conflict with another dependency or the root; the ordinary duplicate-function diagnostic rejects the combined program.

## Verification gate

The composition fixture uses a transitive two-module dependency graph and produces an ordinary portable module whose only WVB export is root `Main`, whose result is `42`, and whose SHA-256 is `5d27c9667eb66e1abbf46b40d02ab3d4e01b94a421a93bffd0375a550440a612`. Conformance covers dependency-order independence, missing imports, cycles, non-portable dependencies, non-exported dependency functions, root-symbol isolation, unreferenced inputs, late imports, module-count limits, source-specific diagnostics, real CLI execution, no-output rejection, and existing-output preservation.

The exact `df80f91` archive passed the complete verifier on Windows and Debian with equal normalized source-composition hash and result evidence. This qualifies the enabling source-module facility, not a reusable Foundation API; the first extraction still requires two real tool consumers.
