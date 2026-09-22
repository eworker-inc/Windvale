# Target-aware project admission

> Status: Current; Project 4 direction approved, integration in progress
> Authority: Informative; existing specifications and accepted decisions remain authoritative
> Last reviewed: 2026-09-22

The selected package parser and lock consumer now build through Project 4,
publish safely, and run on Windows and Debian. The
[package-parser milestone](Compiler-Tools-And-Libraries-Completion-Plan.md#active-milestone-package-parser-with-immutable-borrowing)
is complete. Installed toolchain promotion, full transaction fault-injection
qualification, broader library migration, and later Option/Result operations
have separate gates.

## Current delivery boundary

- The selected Project 4 package closure includes 394 connected manifests and
  593 edition-1 sources with one shared SHA-256 implementation. The ordinary
  launcher uses authenticated target admission and current-source native
  publication; the [integration evidence](../Evidence/2026-09-15-Source-Edition-Package-Integration.json)
  records exact Windows/Debian consumer and build results.
- Focused publication cases cover deterministic replacement, malformed WVB,
  bad-lock preservation, and resource aliases on both hosts. Full transaction
  fault injection and installed publisher promotion remain separate.
- Current compiler construction can exceed the default local development budget.
  A longer selected run follows the [standing local-run approval](../../AGENTS.md#testing-and-verification)
  with a stated command, cold estimate, and finite maximum. Cold construction
  remains a throughput issue for selected verification paths.

Further suite migration must preserve exact inputs. Do not count each converted
manifest as a separate product milestone or weaken target admission to make
migration appear complete.

## Earlier development checkpoints

The following checkpoints explain the path to the completed selected consumer.
Their narrower claims and then-open gaps remain historical; the current delivery
boundary above and the linked integration evidence own present standing.

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

The repository `Build-Wvb` launchers now dispatch a `windvale-project 4` manifest
to a development Project 4 helper instead of the older profile-only build driver.
That helper acquires the current split compiler cache, builds and packages the
manifest reader, source admitter, source authenticator, and foreign binder on
demand, then calls the split compiler's authenticated Project 4 mode. Project 2
and Project 3 stay on the pinned native front door. This is normal repository
routing for the development path, not installed-toolchain promotion; the product
cache is still current-host development evidence, and the installer inventories
still select the older front-door products.

Maintained manifest migration has started with
`Projects/Tests/Language-1.0-Foundation-Generic-Result-Project4.wvproj`, which
builds the existing Option/Result generic consumer with the source-input lock,
source profile, and explicit Windows x64/no-foreign target descriptor. The
library development-target inventory exposes this as `foundation-values`, so a
focused library check can run that Project 4 manifest through the ordinary
`Build-Wvb` launcher. The production-admission owner also has a focused
`--project4-launcher` development mode for the launcher boundary: it preserves a
pre-existing output and then builds the maintained Project 4 consumer. This
narrows the checked behavior relative to the full owner, but cold package
acquisition can still require a named longer local run; it does not replace the
full production-admission owner or cross-host qualification.

Broader library manifest migration and installed publication promotion remain
separate. Retained diagnostic executables must not be substituted into installer
inventories or labeled current solely because selected consumer tests pass.

The earlier publication checkpoint found that both frozen native publishers
rejected the migrated package test bytecode, while a rebuilt current publisher's
validation-only executable accepted it. Generic hosted packaging did not provide
the native transactional publication adapter. At that checkpoint the specialized
constructor was tied to fixed old artifacts and function offsets, and the
current-source check stopped at reproducible native images. That established the
need to preserve native validation, transaction state, and failure guarantees
while connecting the current verifier. See the
[publisher validation boundary evidence](../Evidence/2026-09-09-Current-Publisher-Validation-Boundary.json).

To address that earlier boundary, the maintainer approved
[bounded current-source publisher construction](../Decisions/0962-Construct-Current-Source-Wvb-Publishers.md):
checked identities, named symbols and typed relocations, existing segmented
native images, and the shared native transaction state. Frozen release products
remain unchanged. The subsequent materialization checkpoint below constructs an
actual publisher; launcher integration and full transaction tests remain open.

The first construction stage now checks the six small native publisher objects
by named imports/exports and typed relocation structure. Its focused Windows
owner passes; the same bytecode, cross-packaged for Linux, passes native execution
on Debian. This is not an independent Linux source rebuild. Exact evidence and
limits are in the [object-admission checkpoint](../Evidence/2026-09-09-Current-Publisher-Object-Admission.json).
The next construction stage materialized current-source publisher executables for
Windows and Linux targets and smoke-tested the Windows executable against the
maintained metadata fixture through the focused current-source owner. That
advances the publication dependency past validation-only execution on the local
Windows host, but it is not full Debian source reconstruction or installed
publisher promotion. The current Debian compiler cache is absent and requires
separately budgeted reconstruction before the full current-source owner can run
there.

## Historical Project 3 boundary

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
