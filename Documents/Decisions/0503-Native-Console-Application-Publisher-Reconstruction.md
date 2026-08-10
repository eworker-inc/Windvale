# Decision 0503: Native console-application-publisher reconstruction

- Status: Current-host focused evidence complete; grouped qualification pending
- Date: 2026-08-10
- Scope: current-Windows-host native cross-target reconstruction of the console-application-publisher candidate
- Extends: Decisions 0307, 0340, 0482, 0493, and 0499

## Context

The `WVPA 1` console-application publisher already owns portable completed-
application admission and reuses the native durable publication transaction.
Its exact WVB builds through the native Project 1 front door, but its paired
Windows and Linux applications retained managed application-writer provenance.

The accepted-subset raw lowerer now admits the complete publisher source
closure. The shared hosted-verifier publisher construction pipeline also owns
the exact metadata, identity, structure, target, object-instantiation, import,
and materialization boundaries needed by a fourth non-default role. Those
pieces permit reconstruction without asking either target console-application
publisher to publish or construct itself.

## Decision

Windvale adopts one bounded reconstruction route for the exact
console-application-publisher candidate:

1. build `Windvale-Console-Application-Publisher.wvproj` through the retained
   native Project 1 front door and require the complete WVB identity;
2. invoke the retained raw accepted-subset lowerer and require the complete WVO
   oracle identity;
3. link that admitted object through the retained native standard linker,
   require exported `Main` at offset 18,902, and require the exact linked
   fragment;
4. construct exact Windows and Linux profile bases through the retained native
   hosted-container toolset; and
5. complete both applications through explicit role-aware publisher overlay
   variant 4, requiring the exact target application identities in distinct
   caller-owned outputs.

The exact reconstructed products are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| console-application-publisher WVB | 115,107 | `e8121fb76c7cc39b159d53a3c28d1da8bc2d44968d630495c692a7761656923d` |
| raw-lowerer WVO oracle | 1,139,440 | `259c7d746c3a217c32706bfd617cf66894066bd2e50850cbe5733ac3338e4952` |
| linked fragment | 1,135,424 | `c6b199644be8ca19cce0110a5090e84c736220a130f9b48a4366caf36254e6e2` |
| Windows profile base | 1,151,488 | `922c9019308e837f6a3528c3b1edf6cd83b3e432bdb6a140111c958aa6ff5e97` |
| Linux profile base | 1,150,976 | `a12ab6d136b53c53322d4b7ff612a5f41a2653c30210a4f5dbfb27027bc29f5e` |
| Windows application | 1,158,656 | `0bafe84096859f4b88dc14be92c6cdc5336d791b7c5b0a332dccb76b913dd24e` |
| Linux application | 1,156,037 | `83468e65c1a5aa0bbb33f9571958e5d2f1959b81c08bd4cb66a4083270272ae1` |

## Evidence boundary

The WVO oracle has two sections: 1,134,976 text bytes and 448 read-only-data
bytes. It contains 109 symbols and 15 relocations. `Main` is symbol 108 at
offset 18,902 with 5,436 bytes. The private publication functions remain
explicit construction evidence: apply is symbol 14 at offset 0 with 789 bytes,
and begin is symbol 15 at offset 789 with 389 bytes.

Variant 4 is an internal construction selector. The completed application keeps
the public `WVPA` magic `0x41505657` and stores role zero in its reserved
metadata field. Target-specific startup, metadata, base, and final identities
are exact inputs to admission. The route invokes the independent raw lowerer;
it does not execute either target publisher during construction and therefore
does not create a target self-publication cycle.

This is current-Windows-host native cross-target construction. It consumes
retained same-release compiler, lowerer, linker, hosted-container, startup,
service-leaf, and publisher-construction candidates. The exact WVO is an oracle
at the lowering boundary, not clean-bootstrap or previous-release renewal
evidence.

The final candidate refresh binds the current file-input leaf. It replaces the
stale final application bytes and digests without changing the exact WVB, WVO
oracle, linked fragment, target-base identities, or public metadata contract.

The focused current-Windows-host reconstruction owner passes all three cases in
68.6 seconds: exact candidate inventory, WVB/WVO/paired-application
reconstruction, and independent version-1 publication plus rejected-input
preservation. The established publisher pipeline separately passes 15/15 in
188.7 seconds, regression-protecting roles 0 through 3 while variant 4 is added.

## Consequences

- The managed application writer is no longer the sole constructor for this
  exact candidate.
- The Linux application remains a cross-target product until independently
  reconstructed and executed on Linux.
- The focused reconstruction owner is complete but remains separate from the
  complete publication fault, concurrency, and console-admission matrices.
- The constructor writes a separate output and is not an atomic installer,
  qualification transaction, or promotion path.
- Clean bootstrap, grouped qualification, ordinary-path promotion, and removal
  or archival of Stage 0 recovery remain separate gates.

## Reconsider when

Reconsider this decision if the source/project closure, WVB or WVO identity,
native entry or private transaction geometry, linked fragment, profile base,
publisher-construction role contract, retained toolset identities, or either
target application identity changes.
