# Decision 0344: Native console-packager WVO reconstruction

- Status: Accepted local implementation; dual-host qualification and host-container reconstruction pending
- Date: 2026-08-07
- Advances: [Decision 0343](0343-Native-Console-Packager-Source-Reconstruction.md)
- Uses: [Decision 0304](0304-Digest-Bound-Native-Wvb-To-Wvo-Candidate.md) and [Decision 0308](0308-Native-Wvo-Publication.md)

## Context

The ordinary and segmented console-packager projects reconstructed their exact
WVBs through the native Project 1 front door, but neither complete WVB could be
lowered by the pinned native WVB-to-WVO application. The lowerer exited at the
native runtime boundary before it could report a Windvale rejection.

The WVBs stayed within every accepted module, function, instruction, type, and
record limit. Plan-only measurement instead found one 74,830-byte generated
packager function, followed by a 66,416-byte generated recipe-verification
function. Each independently crossed the bounded dynamic emitter lifetime.
Increasing the arena would have hidden the source structure that caused the
pressure and would not have established a durable bound for the next tool.

## Decision

- Keep the existing arena and native lowering limits unchanged.
- Split the ordinary packager into recipe preparation, application
  construction, verification, and a small coordinator.
- Split portable recipe verification into header and segment/payload phases;
  split PE/ELF header derivation by target; and split PE image-header
  construction from section-header construction. Preserve public functions,
  exact statuses, failure offsets, recipes, containers, and recovered evidence.
- Treat large source routines as a prompt for cohesive extraction when a real
  ownership boundary exists. Do not introduce numbered fragments or a line
  limit merely to reduce file size.
- Extend the existing two-case source-reconstruction command through the
  pinned native WVB-to-WVO launcher and require exact WVO size and SHA-256.
- Canonicalize the order-independent Project 1 inventories for the verifier and
  publisher projects as the same bounded retained-driver compatibility fix used
  by Decision 0343. Their WVBs now rebuild natively, but their full hosted
  closures still fail closed as `Unsupportedˉcode` through the accepted-subset
  lowerer and remain separate backend work.
- Rebase every affected WVB, bridge, candidate container, manifest, launcher,
  and exact test identity together. The PE/ELF tool containers retain explicit
  `stage0-recovery` construction provenance; this decision does not call their
  host-container construction native.

## Evidence

The pinned native build and lowerer produce:

| Closure | WVB bytes | WVB SHA-256 | WVO bytes | WVO SHA-256 |
| --- | ---: | --- | ---: | --- |
| Ordinary console packager | 60,797 | `f4c75495321736bbce22582213133e7cc09157a8439dc198d9848ec95683e89c` | 692,425 | `fd9e289cdae2bfc7956384cd76c022c873fc4c8f39bda4824eb8b82240265695` |
| Segmented console packager | 70,033 | `c4941f396f76467cb6455472f7f4711c21a6f65c12c09a9b5f4135987628f20e` | 789,653 | `4cd97c60169649c466dcf185491eac326bbb7676fb97d95c840c199defb8bbda` |

The ordinary object contains 689,568 machine-code bytes; the segmented object
contains 786,080. Direct Windows measurements take about 1.8 to 2.1 seconds to
build each WVB and 5.0 to 6.5 seconds to lower it. The focused command owns the
same two cases and passes 2/2 in 15.9 seconds. The reviewed focused managed
packager contract passes 1/1 in 8.6 test seconds after the build. The refreshed
publisher rejection and maximum segmented construction/verification commands
pass 2/2 in 0.8 and 1.8 seconds. The focused successful publisher contract
passes 1/1 in 3.4 test seconds after its build. None is rerun through the
grouped retirement coordinator during local development.

The shared-source rebase also produces native-built 105,006-byte verifier and
115,107-byte publisher WVBs. Their refreshed Windows/Linux containers remain
Stage 0 recovery artifacts. The retained construction and verification bridges
rebuild natively to 29,322 and 103,393 bytes respectively.

## Consequences

Source through verified WVO for both console packagers no longer needs .NET.
The checked-in raw applications and their public launchers remain directly
usable without .NET, but rebuilding those PE/ELF host containers still uses the
named Stage 0 recovery constructor. Native service-bundle linking, host-container
construction, Linux execution of the rebased identities, grouped qualification,
and promotion remain open.

The verifier and publisher source inventories no longer require Stage 0 merely
to produce WVB. Their unsupported native-lowering surface is now explicit and
must not be inferred from the packagers' smaller accepted closures.

## Reconsideration triggers

Revisit the extraction only if a later compiler proves a smaller equally clear
representation or if semantic changes establish a better module boundary.
Rebuild and requalify the identities if the shared construction, verification,
lowering, object, runtime, or host-container contracts change.
