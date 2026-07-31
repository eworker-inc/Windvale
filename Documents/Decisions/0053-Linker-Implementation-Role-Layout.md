# Decision 0053: Linker implementation role layout

- Date: 2026-07-31
- Status: Accepted and implemented; cross-host requalification pending

## Context

The complete Windvale-written `flat-x86-64-v1` linker remained under `Examples/Linker` after its object scanner, resolution, layout, relocation, independent reconstruction, canonical map, hosted shell, and cross-host differential artifacts were qualified. The independent C# Stage 0 implementation remained under `Linker/Windvale.Linker`. Those paths obscured ownership: a maintained Windvale tool appeared to be an example, while the C# recovery oracle appeared to be the sole linker product.

The C# project now also owns the first deterministic UEFI PE32+ application adapter used by the OS bootstrap. That target remains a narrow Stage 0 implementation over verified flat link evidence; it does not make the C# folder the semantic definition of linking or imply that the Windvale linker already implements UEFI output.

## Decision

- `Linker/Windvale` owns the linker implementation written in Windvale.
- `Linker/Reference` owns the independent C# Stage 0 reference/recovery linker and currently C#-only target adapters.
- The C# project, assembly, and namespace remain `Windvale.Linker`; their behavior and public identity do not change.
- `Examples/Linker` retains canonical WVA provider inputs, including `Console-Provider.wva`, rather than maintained linker implementations.
- The Windvale and C# implementations continue to share the `flat-x86-64-v1` linking contract and byte-for-byte differential evidence.
- The UEFI application adapter remains separately specified and verified. Its placement under `Linker/Reference` records current bootstrap ownership rather than portable target parity.
- WVO, flat-image, map, UEFI, diagnostic, hosted-resource, and deterministic-artifact contracts remain unchanged.

Every solution entry, project reference, embedded source resource, OS bootstrap dependency, verifier path, verification-routing fixture, Foundation consumer reference, specification, and current architecture document moves with the owning implementation. Historical decisions and evidence descriptions remain unchanged because they record the paths and trees that were qualified at those commits.

## Consequences

The repository now distinguishes the qualified Windvale flat-image linker from its C# recovery oracle, and `Examples/Linker` contains only input material. The move does not introduce another link contract, rename public APIs, or claim that the Windvale linker emits UEFI applications. Future changes to the shared flat target must continue to update both implementations and preserve differential, malformed-input, boundary, reconstruction, and deterministic-output evidence.

The C# and Windvale sources move without semantic edits. Canonical WVB, WVO, flat-image, map, UEFI, boot-transcript, diagnostic, runtime, and conformance contracts must remain unchanged. Similar object-model, runtime, or inspection-tool moves require their own ownership analysis rather than following this layout mechanically.

## Verification

Acceptance requires changed-path routing for both implementation folders, a zero-warning build, focused linker conformance, focused OS tests for the UEFI project-reference move, the complete Standard suite, and the native Qualification gate. Cross-host qualification requires the same committed source archive to pass on Windows and Debian with matching normalized reports and byte-identical portable artifacts before this decision may be marked qualified.
