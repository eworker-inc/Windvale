# Decision 0051: Assembler implementation role layout

- Date: 2026-07-31
- Status: Accepted and implemented; cross-host requalification pending

## Context

The complete Windvale-written WVA 1 assembler remained under `Examples/Assembler` after its scanner, semantic inspector, object encoder, hosted shell, deterministic artifacts, and cross-host differential behavior were qualified. The independent C# Stage 0 implementation remained under `Assembler/Windvale.Assembler`. Those paths obscured ownership: a maintained Windvale tool appeared to be an example, while the C# recovery oracle appeared to be the sole assembler product.

Canonical `.wva` inputs are examples, but `Wva-Assembler-Core.wv` is an implementation with an owned specification, verification route, hosted boundary, and artifact identity. The compiler role layout established that implementation language and recovery role should be explicit without renaming stable public assemblies or namespaces.

## Decision

- `Assembler/Windvale` owns the assembler implementation written in Windvale.
- `Assembler/Reference` owns the independent C# Stage 0 reference/recovery assembler.
- The C# project, assembly, and namespace remain `Windvale.Assembler`; their behavior and public identity do not change.
- `Examples/Assembler` retains canonical WVA input examples, including `Hello-Object.wva`, rather than maintained assembler implementations.
- The WVA 1, WVO 1.0, diagnostic, hosted-resource, and deterministic-artifact contracts remain unchanged.
- Linker, object-model, runtime, and inspection-tool layouts remain separate decisions because they do not yet have the same implementation-role symmetry.

Every solution entry, project reference, embedded source resource, verifier path, example command, verification-routing fixture, Foundation consumer reference, and current architecture document moves with the owning implementation. Historical decisions and evidence descriptions remain unchanged because they record the paths and trees that were qualified at those commits.

## Consequences

The repository now distinguishes the qualified Windvale assembler from its C# recovery oracle, and `Examples/Assembler` contains only input material. The move does not introduce a second assembly contract or imply that the reference implementation is obsolete. Future WVA changes must continue to update both implementations and preserve differential, malformed-input, boundary, and deterministic-output evidence.

The C# and Windvale sources move without semantic edits. Canonical WVB, WVO, image, map, diagnostic, runtime, and conformance contracts must remain unchanged. Similar linker or object-model moves require their own ownership analysis rather than following this layout mechanically.

## Verification

Acceptance requires changed-path routing for both implementation folders, a zero-warning build, focused assembler conformance, the complete Standard suite, and the native Qualification gate. Cross-host qualification requires the same committed source archive to pass on Windows and Debian with matching normalized reports and byte-identical portable artifacts before this decision may be marked qualified.
