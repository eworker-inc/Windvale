# Decision 0570: WVB Inspector Launch Record 2

- Status: Implemented
- Date: 2026-08-15
- Advances: Milestone 4 and Decisions 0564 and 0568
- Contract: [Capability approval and launch records](../../Specifications/Windvale-Capability-Approval-And-Launch.md)

## Context

Milestone 4 needs a second real package to execute on Windows and Linux through
an exact installed command. WVB Inspector's locked WVB is identical to the
source product behind the `wvdump` native front doors already present in the
qualified `v0.1.0` installers. Launch Record 1 cannot honestly describe this
path: its WVO, linked-image, directory-provider, fixed-object, and two-argument
fields are specific to WVDB Query.

Copying those fields, inventing target hashes, or treating a package capability
as owner approval would create false evidence. Rebuilding a second host runtime
would duplicate an already measured product and delay the offline lifecycle.

## Decision

Retain Approval 1 and add Launch Record 2 for the exact WVB Inspector package.
Bind the manifest, lock, bundle, provenance, WVB, approval, target, installed
native host application, named `Main` entry, and complete provider table.

Select the existing Windows and Linux `wvdump` front doors. Bind one explicit
1..4,096-byte UTF-8 host-path argument to a read-only file provider and immutable
argument snapshots. Deny path enumeration, mutation, environment, network,
process launch, clock, and entropy. Do not claim that the input is a fixed
preinstalled object.

Independently verify all source and host identities, both target records, and
their exact lines. Execute the target's exact host against the package WVB in the
paired-host focused owner. Reject approval, host, target, provider, and record
substitution.

## Consequences

- The second package has real Windows/Linux launch closure without another
  compiler, runtime, or native application.
- Generation 1 may identify `wvdump` as an installed command using these exact
  approval and launch identities.
- The host path is explicit user-supplied authority, not an ambient directory or
  mutation grant.
- Launch Record 1 remains the exact WVDB profile; Launch Record 2 is the exact
  direct native-host profile. Neither is claimed as a universal launcher schema.

## Reconsideration triggers

Reconsider when command dispatch must bind multiple input files, a directory,
mutable authority, dynamic provider discovery, revocation, or a non-native host
application.
