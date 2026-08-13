# Decision 0533: Restore verifier-specific front-door startup

- Date: 2026-08-13
- Status: Implemented candidate
- Supersedes: [Decision 0532](0532-Windows-Containment-Errorlevel-Observation.md)
- Contract: [Native WVB read-only front door](../../Specifications/Windvale-Native-Wvb-Read-Only-Front-Door.md)

## Context

The Project 2 root reorganization commit `b44db3ce` repackaged the Windows and
Linux front-door WVB verifier applications through the generic hosted-compiler
startup. The resulting Windows PE emitted the correct invalid-WVB diagnostic but
did not import `ExitProcess`; loader fall-through therefore returned status zero.
GitHub Windows retirement runs `31665094047`, `31669660884`, `31671393519`, and
`31672940187` exposed that regression on different fixed containment cases. Linux
and every other qualification owner passed.

The repository retains the verifier-specific Windows and Linux application
candidates introduced for this boundary. Their identities exactly match the
front-door verifier artifacts before the reorganization. The Windows candidate
contains the explicit `ExitProcess` path required by Decision 0524, while the
replacement PE does not.

## Decision

Restore the retained verifier-specific application candidates as the native front
door verifiers on both hosts:

| Host | Bytes | SHA-256 |
|---|---:|---|
| Windows x64 | 1,004,032 | `5f0a83681f54c7e047d6b68c86f71767d6c3584330bef1e68108f9b3465167a7` |
| Linux x64 | 1,003,520 | `824e90ae07e82af3d6d0b4cf23bc4d3327fc3367684215171247fa71ab274982` |

Pin those identities in the front-door manifest, checksum inventory, native
wrappers, compiler-convergence verifier, containment owner, and specification.
Remove the Windows command-file status adapter and return both hosts to direct
bounded asynchronous child collection. Process adapters must not compensate for a
product artifact whose termination contract is wrong.

## Consequences

- Invalid WVB input again returns the verifier rejection status through explicit
  product-owned termination on Windows.
- The root reorganization and Project 2 source layout remain intact; only the
  regressed verifier application lineage is restored.
- The host test remains a bounded observer and does not interpret diagnostics as a
  substitute for native process status.
- No C#, .NET, managed fallback, semantic oracle, or new dependency is introduced.

## Evidence boundary

Focused Windows WVB containment must pass all 1,000 fixed cases. Front-door,
unsafe-WVB, convergence, and verification-plan owners must accept the restored
identity. GitHub Windows and Linux retirement plus the aggregate Verification gate
must pass before this candidate is promoted.

## Reconsideration triggers

Replace these candidate identities only when a Windvale-native construction path
packages the current verifier WVB with the verifier-specific startup on both hosts,
preserves explicit exit behavior, and passes the complete dual-host qualification
boundary.
