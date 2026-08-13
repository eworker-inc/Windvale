# Decision 0522: Enum-complete native WVO inspector reconstruction

## Status

Implemented and independently exercised on Windows and Linux. GitHub grouped
Qualification, candidate promotion, and final recovery retirement remain
pending.

## Context

The candidate-2 WVO inspector reconstructed the exact WVB, WVO, linked
fragment, startup, metadata, runtime, and outer Windows/Linux containers. Its
valid `verify` and `inspect` paths succeeded, but its no-argument self-test
returned 1 without output. A current Stage 0 application built from the same
61,008-byte WVB passed that self-test.

The divergence was not in WVO semantics, source construction, native lowering,
or the linked fragment. The profile-6 native reconstruction passed only the
fixed 323-byte `Enumˉname` machine leaf into its service bundle. The established
service contract requires that leaf followed by the selected module's admitted
`WVEN 1` nominal metadata. Valid command paths did not expose the missing
metadata, while the embedded formatting and enum-name self-tests did.

Independent Linux reconstruction also exposed a separate publication detail:
the container composer wrote the exact ELF bytes as a regular file, but the
WVO reconstruction script did not apply executable mode before requiring it.
The newer WVB-runner reconstruction already owns that post-construction step.

## Decision

### Compose the complete enum service

After building and verifying `Projects/Object-Model/Windvale-Wvo-Object.wvproj`, both reconstruction
scripts invoke the retained native hosted-enum processes:

1. `wvhostenumrequest` derives the exact single-group request from the verified
   WVO inspector WVB; and
2. `wvhostenumservice` appends admitted module-specific metadata to the fixed
   Windvale-owned machine leaf.

The exact intermediate identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVO inspector enum request | 945 | `7129a003ae3d0e795f5aea61e4e8d8f25ba4fb93180f2538bea9f04a3c0bdab6` |
| Complete WVO inspector enum service | 1,244 | `577ffaee02e64b0956f73d5ca44d65afa262cf476ae5eee86a899ffc575788d1` |

The profile-6 bundle consumes the complete service instead of the bare leaf.
The Linux constructor applies executable mode to the independently composed
ELF before identity and execution checks. The WVB, WVO, linked fragment,
startup sources and objects, public profile, service order, and command
contract remain unchanged.

### Refresh the bounded candidate identity

The candidate advances to manifest format
`windvale-native-wvo-object-candidate-3` with construction decision 0522. The
unchanged and refreshed products are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVO inspector WVB | 61,008 | `a630d49f0549c865644d8052fbff7e8bf2b6a6dcd013e1187d4356d49cd188db` |
| WVO inspector WVO | 591,723 | `f45b14c33a7615209a2a16f6caf0bee041bdb5e2f46fd868792222e774fdb30c` |
| Linked inspector fragment | 587,529 | `f318ee573b149aac169b67369e90dbacc6451fc129022bfb4e62b2ceff9cfba4` |
| Windows WVO inspector | 606,720 | `a534b1c7a5ff9112c221a9576141842c4bb50c28b1d43d0ab02a8679bba6f366` |
| Linux WVO inspector | 606,208 | `f94d2e16da76c949e15978bd879bff38205685be08d7afa1670f48d3f6592ea1` |

Both final applications are byte-identical to the independent current Stage 0
writer products. This equality is recovery provenance, not a new managed owner
for ordinary construction.

### Transfer the repaired self-test

The paired native front-door helper verifies the refreshed per-host identity,
runs the raw application with no arguments, and requires exit 0 with empty
standard output and diagnostics. The broad Windows and Linux Seed scripts no
longer run that self-test through the managed reference runtime.

The helper remains at 105 exact artifacts and grows from 184 to 185 cases. One
managed invocation leaves each broad host script, bringing cumulative removal
to 193. Capability refusal and empty/missing-resource adapter reports remain
managed because their typed `WVR3010`, `WVR3021`, and `WVR3022` behavior is not
equivalent to the raw native process boundary.

## Evidence

- The focused reconstruction owner passes 3/3 on Windows in 60.4 seconds and
  independently passes 3/3 on Linux.
- The complete paired native front-door helper passes exact summary
  `artifacts=105 cases=185` in 939.6 seconds on Windows and 873.7 seconds on
  Linux over the identical public Windvale state.
- Both native reconstruction routes produce the exact refreshed application
  identities above; Windows reconstruction also matches both independently
  produced current Stage 0 applications byte for byte.
- The raw Windows and Linux candidates pass no-argument self-test with no
  output, and their existing valid verification/inspection contracts remain
  unchanged.

This is independent dual-host candidate and helper evidence. It is not the
GitHub grouped Qualification, clean/previous-seed renewal, promotion,
installation, or final Stage 0 archive gate.

## Consequences

The WVO inspector no longer carries a known stale self-test or an incomplete
enum-service reconstruction. Candidate construction now demonstrates the same
module-specific nominal-service composition used by the current recovery
writer, and Linux reconstruction produces an executable artifact explicitly.

T2 remains `managed-normal`. WvDump/WVO authorization and resource failures,
Stage 0 object-report oracles, later differential lanes, grouped Qualification,
promotion, and the final recovery release remain open.

## Reconsideration triggers

Version the hosted enum request or metadata handoff if WVB nominal encoding or
the `WVEN` contract changes. Do not substitute the fixed leaf for a complete
module-specific enum service. Specify stable native hosted-service failure
reports before transferring authorization, invalid-name, or missing-resource
cases from the reference runtime.
