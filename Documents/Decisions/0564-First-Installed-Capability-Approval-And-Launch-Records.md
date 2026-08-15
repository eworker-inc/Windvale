# Decision 0564: First installed capability approval and launch records

- Status: Implemented
- Date: 2026-08-15
- Advances: Milestone 3 and Decisions 0145, 0530, and 0561
- Contract: [Capability approval and launch records version 1](../../Specifications/Windvale-Capability-Approval-And-Launch.md)

## Context

Milestone 2 proved the exact WVDB Query package closure and executed five
rights-reduced success, missing, denied, and unavailable cases on Windows and
Linux. The package declares required capabilities, but declaration is not an
application-owner grant. The target images also existed only as test evidence,
not inspectable installed launch policy.

A general approval database, GUI, launcher service, configurable filesystem
provider, or database server would widen the milestone. The smallest honest
slice is to record the exact accepted closure and the already qualified target
bindings.

## Decision

Add one canonical Approval 1 record selecting WVDB Query's exact Package 1,
Lock 1, Bundle 1, provenance, WVB, and five capabilities. Map each capability to
one named provider class and explicitly deny ambient filesystem, mutation,
environment, network, process-launch, clock, and entropy authority.

Add canonical Windows x64 and Linux x64 Launch Record 1 files. Bind the approval
to the exact ABI 23 entry, provider table, directory host, platform leaf, linked
image, hosted application, fixed read-only object name, 3,072-byte request
limit, and two-argument shape. Do not place native paths or handles in portable
approval evidence.

Own the records with an independent verifier and an eight-case paired-host
suite. Substitution of capability names, additional authority, writable
providers, targets, approval identities, or incomplete records is rejection.

The eight-case owner passed on Windows and Linux in Verify run 31883543587 at
commit `fcc77c2afb8c1daf0465041983695866d6e8b826`.

## Consequences

- Package requirement, owner approval, provider binding, and execution bytes
  become separate inspectable evidence.
- Release signing may select these records but cannot turn a requirement into a
  grant or widen their providers.
- This is a fixed WVDB Query installation/launch profile. It does not introduce
  a general launcher, approval UI, durable activation database, or application
  installer.

## Reconsideration triggers

Reconsider when a second application, provider instance selection, configurable
directory, mutable capability, durable approval update, revocation, interactive
approval UI, or launch supervisor needs a general format.
