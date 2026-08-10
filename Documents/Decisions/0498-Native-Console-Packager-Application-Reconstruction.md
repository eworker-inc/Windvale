# Decision 0498: Native console-packager application reconstruction

- Status: Accepted
- Date: 2026-08-10
- Scope: current-Windows-host native cross-target reconstruction of the ordinary and segmented console-packager candidates
- Extends: Decisions 0303, 0342, 0343, 0344, 0492, and 0497

## Context

The ordinary and segmented Windvale console-packager projects already rebuilt
as exact WVBs through the native Project 1 front door and lowered to exact WVOs
through the accepted-subset lowerer. Their committed Windows and Linux hosted
applications still came from the Stage 0 recovery writer and predated the
current native hosted startup and file-input leaves. Source-to-WVO evidence
therefore did not establish the construction provenance or identity of the
live runnable candidates.

The retained native source builder, accepted-subset lowerer, flat linker, and
hosted-container toolset can construct profile-5 applications without invoking
either console packager being reconstructed. That closes this target-family
application-writer seam, but it still consumes retained same-release native
candidates and cannot be described as a clean bootstrap.

## Decision

Windvale adopts one paired reconstruction boundary for both console-packager
projects:

1. build each canonical project once through the native source front door;
2. lower each exact WVB once through the digest-bound accepted-subset lowerer;
3. link each exact WVO once at base zero with exported entry `Main`;
4. expose the linked image as one exact native fragment and package profile 5
   for Windows and Linux through the retained hosted-container toolset;
5. require a caller-supplied existing output directory distinct from both
   committed candidate directories; and
6. verify the complete six-artifact result before accepting reconstruction.

The exact accepted artifacts are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| ordinary WVB | 60,797 | `f4c75495321736bbce22582213133e7cc09157a8439dc198d9848ec95683e89c` |
| ordinary Windows application | 708,608 | `ea8e666806618cd9c230bdc88882e9b30a98182f8486456a46c75b746a0cdab9` |
| ordinary Linux application | 708,608 | `d399c935e906ab42d7572e337226577055396cb6204766106e21790e22ea43af` |
| segmented WVB | 70,033 | `c4941f396f76467cb6455472f7f4711c21a6f65c12c09a9b5f4135987628f20e` |
| segmented Windows application | 805,376 | `a6a6fd40a6becf0f65bbf995006e8e5410832da6f5ebc906f216f9e435032ef0` |
| segmented Linux application | 806,912 | `8916fb509f81e29dabca7ed0202c0ad250f129e78b70b701630dbfcd55a1d30d` |

The intermediate object identities remain exact:

| Object | Bytes | SHA-256 |
| --- | ---: | --- |
| ordinary WVO | 692,425 | `fd9e289cdae2bfc7956384cd76c022c873fc4c8f39bda4824eb8b82240265695` |
| segmented WVO | 789,653 | `4cd97c60169649c466dcf185491eac326bbb7676fb97d95c840c199defb8bbda` |

Both candidate manifests advance to format 2, record construction Decision
0498, retain their source and historical provenance decisions, and describe
application construction as `native-cross-target-hosted-toolset`.

## Evidence

The focused current-Windows-host retirement lane passed all four cases in
52.1 seconds: both exact three-file candidate inventories and byte-identical
native reconstruction of the ordinary and segmented WVB plus paired
applications. No broader verification or independent Linux-host execution was
run for this slice.

## Consequences

- The managed hosted-application writer is no longer the only constructor for
  either exact current paired console-packager candidate.
- The two WVB and WVO identities remain unchanged. The four application
  identities advance to the current native startup, service, and container
  generation.
- The construction is non-circular with respect to the two target packager
  applications: neither candidate is used to package itself or its sibling.
- The route still consumes retained native compiler, lowerer, linker, and
  hosted-container candidates. It is not a non-circular previous-seed renewal
  or a clean bootstrap.
- The C# writers remain explicit recovery and differential implementations.
- Independent Linux reconstruction and execution, application behavior and
  fault breadth, artifact promotion, grouped dual-host qualification, broader
  P1 closure, and the final Decision 0057 retirement gate remain separate.

## Reconsider when

Reconsider this decision if either source project, WVB or WVO identity, profile
5 metadata, startup or service leaf, hosted-container toolset, target container
format, or retained seed changes.
