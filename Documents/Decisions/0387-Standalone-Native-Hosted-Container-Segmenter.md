# Decision 0387: Standalone native hosted-container segmenter

- Status: Implemented candidate
- Date: 2026-08-08
- Contract: [Windvale native hosted-container segmenter](../../Specifications/Windvale-Native-Hosted-Container-Segmenter.md)
- Predecessor: [Decision 0386](0386-Windvale-Owned-Segmented-Hosted-Container-Materialization.md)

## Context

Decision 0386 moved every final hosted-container byte into bounded portable
Windvale segments, but the normal linker still invoked the retained WVNF inside
the managed process and concatenated the responses. The existing native
durable publisher can accept bounded chunks, but it cannot call a managed
fragment executor after .NET retirement. A standalone native process is
therefore the smallest prerequisite for joining segment construction to that
publisher.

The reusable constructor briefly shared one file with the service-free `Main`
wrapper. Exporting the constructor for the hosted tool would have given the
retained native fragment two exports and weakened its one-entry invariant.

## Decision

Split the construction algorithm into the focused portable
`Linkerˉnativeˉhostedˉcontainerˉsegmentationˉcore` module. Keep the retained
WVNF wrapper and the hosted command as separate one-`Main` consumers of that
core.

Add `Nativeˉhostedˉcontainerˉsegmenterˉtool` with the exact two-path command
contract. It reads one `WVHT 1` request, invokes the shared constructor, admits
only a successful `WVHU 1` response, and performs no output call on rejection.
It uses the existing compiler-authority capability and service boundary.

Assign hosted profile 7: `WVHG`, container format 10, flags 8. Extend the
Windvale-owned metadata, runtime-header, container-plan, segment, Windows-byte,
and Linux-byte admission paths from profiles 1–6 to profiles 1–7. Do not add a
managed exception or relax native fragment export validation.

Expose the paired Stage 0 construction targets
`windows-x64-hosted-container-segmenter-v1` and
`linux-x64-hosted-container-segmenter-v1`. Their new C# writer and CLI routing
are explicitly deletion-bound packaging wiring, not a permanent implementation
of segment semantics.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Segmenter WVB | 23,896 | `5fed7bad6fca036297b97b3a5ec2eea999e52682b2055abe278592b46b3b0d3f` |
| Windows application | 312,320 | `931b0bff9138f5ce7ef0142f7bb29e84d72055802325f9fe5fe3d217548ef4ad` |
| Linux application | 311,296 | `d33d0a0f4aacf8dc0276451aeaf0794a5723f1c57cc435a9b8751bab3021b4e6` |

The focused Windows test reconstructs the exact WVB through the native Project
1 front door, constructs and independently verifies both packages, executes a
real request directly through the Windows application, compares the exact
response with the retained service-free native fragment, and proves that a
malformed request returns status 2 without changing an existing output. The
single selected test passes in 3.655 seconds. The broad verifier and Linux
execution are intentionally deferred to the grouped retirement gate.

The profile extension also regenerates the exact metadata, runtime-header,
planner, platform-byte, and segmentation artifacts recorded by their current
specifications and verification scripts. Their algorithms are unchanged
except for admitting profile 7.

## Consequences

- Hosted-container segment construction now has a real .NET-free process on
  both permanent target formats.
- The one-entry native fragment contract remains strict.
- The ordinary linker still uses managed dispatch and concatenation; this
  decision alone does not retire that path.
- The next slice can bind an immutable ordered segment manifest to the existing
  native durable multi-chunk transaction, then remove managed dispatch,
  concatenation, and publication.
- Stage 0 remains the package constructor and recovery oracle until the grouped
  dual-host gate promotes a digest-bound native launcher.

## Reconsideration triggers

Reconsider the process boundary if a later native composition contract can call
the portable constructor in-process while preserving the same immutable input,
bounded response, metering, and failure-isolation guarantees. Do not retain a
second segment algorithm merely to avoid the command boundary.
