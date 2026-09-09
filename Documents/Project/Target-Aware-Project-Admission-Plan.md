# Target-aware project admission

> Status: Current; Project 4 direction approved, integration in progress
> Authority: Informative; existing specifications and accepted decisions remain authoritative
> Last reviewed: 2026-09-09

The package-library migration needs a project build that supplies the same
explicit target and authenticated source admission already used by the split
compiler. Reconstructing the current build driver alone does not provide that
integration.

## Development checkpoint

The native manifest tool and split coordinator now have a Project 4 development
path. The maintained admission verifier's `--project4-products` selection passes
16 cases on Windows and real Debian using explicit retained products. It builds
the existing Option/Result text-borrow consumer, preserves output bytes when
source directives are reordered, and rejects the selected invalid inputs without
publication. The existing 13 failure/cleanup test groups also pass on both hosts.
Exact identities and limits are in the
[development admission evidence](../Evidence/2026-09-08-Project4-Authenticated-Admission.json).

The additional cases select the maintained Windows and Linux no-foreign target
descriptors explicitly. Their consumer outputs match the existing target case;
the build host does not choose the target. Descriptor generation, rejection, and
exact-byte checks are recorded in the
[explicit host-target evidence](../Evidence/2026-09-08-Project4-Explicit-Host-Targets.json).

The full verifier includes the new cases but has not been run through complete
tool construction or cold qualification. Installed product selection, maintained
package manifest migration, full target/alias/profile coverage, and project build
cache integration remain unfinished. This checkpoint is not production promotion.

The Windows current-source analyzer/emitter cache and maintained manifest-reader
bytecode build now complete through the existing reconstruction command. The
emitter and reader bytes match the earlier tested products; this used exact
intermediate caches and is not cold qualification. See the
[current compiler reconstruction evidence](../Evidence/2026-09-09-Project4-Current-Compiler-Reconstruction.json).

The next integration checkpoint is acquisition of the remaining admission
products, followed by normal project-build routing through authenticated admission.
The current split-compiler cache constructs only the analyzer and emitter; the
Project 4 path also needs the native manifest reader, admitter, and authenticator,
plus the foreign binder when required. Repository `Build-Wvb` launchers and the
installer inventories still select the older build driver. Retained diagnostic
executables must not be substituted into those inventories or labeled current
solely because selected consumer tests pass.

## Current boundary

[Project 3](../../Specifications/Windvale-Project.md#project-3-text-format)
selects a source-input lock and source profile for the complete source closure.
It has no target-descriptor directive. The maintained build driver calls
`Compilerˉcompileˉsourceˉwvbˉwithˉprofileˉinputs`, which admits those profile
inputs but does not take a target descriptor or foreign catalog.

[Decision 0886](../Decisions/0886-Make-Target-And-Foreign-Admission-A-Mandatory-Language-1.0-Phase.md)
requires explicit target admission for Language 1.0 production builds, including
an authenticated empty foreign catalog where applicable. A profile-only build
must not be promoted as satisfying that requirement. The existing
`Tools/Native/Run-Split-Compiler.mjs` path already performs authenticated
admission with a caller-selected target descriptor.

There is also a reconstruction-cost problem. A diagnostic reachable-source view
reduced the current driver from 3.62 MB to 2.72 MB while preserving declarations
and reachable function bodies, but its analyzer stopped with exit 66 before
publishing WIR. This is not evidence that a usable new build driver exists.
The exact reason for that exit remains to be diagnosed.

## Accepted direction

The maintainer approved the Project 4 approach on 2026-09-08. Acceptance
authorizes implementation; it does not qualify a production build entry point.
The durable rationale is recorded in
[the Project 4 explicit-target decision](../Decisions/0961-Require-An-Explicit-Target-In-Project-4.md).

Introduce a separately versioned **Project 4** contract, retaining Project 3's
source inventory and profile inputs and requiring exactly one additional field:

```text
target-descriptor "Inputs/Target.wvtd"
```

The target path would obey the existing workspace containment, alias rejection,
bounded snapshot, and input/output separation rules. Its bytes would be validated
by the existing target-admission owner and bound into existing admission evidence.
No host-default target, network lookup, alternate foreign parser, or new WVTD
encoding is proposed. Target-specific build evidence must include the exact
descriptor bytes and existing compiler/provider identities.

The production build path would reuse the existing admission, authentication,
analysis, and emission implementations. Its architecture must preserve those
phase boundaries rather than make the oversized monolithic driver a prerequisite
for every modern package build. The concrete integration remains implementation
work, not a second compiler or authority to bypass a validation phase.

Project 2 would remain explicitly bootstrap-only where permitted by Decision
0886. Project 3's historical profile-only contract would not silently acquire
new fields or an implied target. This proposal does not accept mixed Seed and
Language 1.0 source closures or introduce duplicate public library APIs.

An explicit command-line target is the alternative: it avoids a new manifest
version but makes the project file insufficient to select the complete build
inputs. The recommendation is the versioned manifest for reproducibility.

## Required implementation evidence

- Reuse the existing manifest parser owner and add valid, missing, duplicate,
  malformed, oversized, aliased, and escaping target-path cases.
- Reject mismatched, malformed, unsupported, or substituted target evidence before
  publication; preserve existing output on failure.
- Build the maintained modern package consumers through the actual project
  entry point on Windows and Debian, not a diagnostic source-list wrapper.
- Compare identical-input output bytes and exercise supported target differences
  with the existing target-aware compiler fixtures.
- Preserve explicitly supported bootstrap workflows and document which older
  profile-only workflows are not Language 1.0 production qualification.

This proposal addresses one dependency in the
[compiler/tools/library completion plan](Compiler-Tools-And-Libraries-Completion-Plan.md).
Shared-library source migration, remaining Option/Result operations, installed
toolchain delivery, and overall qualification remain separate unfinished work.
