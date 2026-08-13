# Decision 0502: Native console-application-verifier reconstruction

- Status: Implemented current-host candidate; grouped Windows/Linux qualification pending
- Date: 2026-08-10
- Scope: current-Windows-host native cross-target reconstruction of the profile-7 console-application-verifier candidate
- Extends: Decisions 0341, 0461, 0492, 0497, and 0500

## Context

The fixed two-snapshot console-application verifier already owns the permanent
Windvale-native maximum-plus-one rejection cases from Decision 0341. Its
checked-in WVB builds through the native Project 1 front door, but the paired
profile-7 applications retained Stage 0 construction provenance while the
accepted-subset lowerer and hosted-verifier construction path lacked required
operations and the explicit two-snapshot profile.

The current native lowerer now accepts the complete source closure. The hosted
container and publisher-construction toolsets now admit profile 7 with eleven
services, two immutable input snapshots, and the profile-specific runtime
geometry. Those pieces make one exact reconstruction possible without using a
managed compiler, lowerer, linker, or application writer in the construction
process.

## Decision

Windvale adopts one bounded reconstruction route for the exact
console-application-verifier candidate:

1. build `Projects/Tools/Windvale-Console-Application-Verifier.wvproj` through the retained
   native Project 1 front door and require the complete WVB identity;
2. lower that exact WVB through the retained raw accepted-subset lowerer and
   require the complete WVO oracle identity;
3. link the admitted WVO through the retained native linker, require exported
   `Main` at offset 19,221, and require the exact 1,045,627-byte fragment;
4. construct the Windows and Linux profile-7 applications through the retained
   native hosted-container and publisher-construction toolsets, using the exact
   eleven-service inspector startup and two-snapshot runtime geometry; and
5. require all four durable product identities in a separate caller-owned
   output directory.

The exact reconstructed products are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| console-application-verifier WVB | 105,006 | `1dcd5f2aeebd974649e64c90d9f473e1e75f7d13dbcde2814de1dded72cf2c0c` |
| raw-lowerer WVO oracle | 1,049,519 | `51292e4d300d4a6bb6ce4879915bba5304de70c9deafdf4eb6ff6a54a6dbf150` |
| Windows application | 1,063,936 | `05b5f5b3e3999a0ef3537f0908967069a12f17de09753fc90e8a4c7542dc9d3f` |
| Linux application | 1,064,960 | `c2700e5e68711d7b8e8a8f7e9573d87dfa27c3676a034a314310ef59045e5f1a` |

## Evidence boundary

The WVO oracle has two sections: 1,045,136 text bytes and 491 read-only-data
bytes. It contains 101 symbols and 24 relocations. The native linker derives the
1,045,627-byte fragment with SHA-256
`96fee2a235db667b161db2eff71625dc714f842f82e74dcf22c0aa03b1cdbffa`.
The focused owner separately checks the candidate inventory, exact paired
reconstruction, and current-Windows-host two-snapshot compatibility plus
rejection preservation.

This is current-Windows-host native cross-target construction. It consumes
retained same-release compiler, lowerer, linker, hosted-container, startup,
service-leaf, and publisher-construction candidates. Requiring an exact WVO
oracle makes the lowering boundary explicit; it does not make the route a clean
bootstrap or previous-release seed renewal.

## Consequences

- The managed application writer is no longer the sole constructor for this
  exact candidate.
- The Linux application is a cross-target product until independently
  reconstructed and executed on Linux.
- The exact WVO oracle and focused compatibility case do not replace broader
  lowering, linking, hosted-container, or console-application verification
  coverage.
- The constructor writes a separate reconstruction directory. It is not an
  atomic installer, qualification transaction, or promotion path.
- Clean bootstrap evidence, grouped qualification, ordinary-path promotion,
  and removal or archival of Stage 0 recovery remain separate gates.

## Reconsider when

Reconsider this decision if the verifier source/project closure, WVB or WVO
identity, native entry offset, linked fragment, profile-7 hosted-container
geometry, retained toolset identities, or either target application identity
changes.
