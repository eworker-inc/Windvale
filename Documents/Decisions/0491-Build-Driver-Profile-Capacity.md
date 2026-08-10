# Decision 0491: Build-driver profile capacity

- Status: Accepted current-host candidate
- Date: 2026-08-10
- Scope: hosted build-driver compilation cost, runtime capacity, file-input naming, and native failure visibility
- Extends: [Decision 0201](0201-Expanded-Exact-Compiler-Native-Capacity.md), [Decision 0203](0203-Evolved-Compiler-Hosted-Tool-Capacity.md), and [Decision 0490](0490-Indexed-Compiler-WVB-Verification.md)
- Retains: native ABI 22, execution-context format 7, service-table format 5, `WVHB 1`, the six capabilities and ten services, the 512 MiB runtime-data ceiling, and the last qualified ordinary packages until promotion

## Context

The enlarged compiler build-driver could verifier-admit its current WVB after
Decision 0490, but its native self-build exhausted the hosted dynamic text and
byte arena. Raising the former 128 MiB arena to 160 MiB still exhausted it.
Raising every hosted profile or crossing the fixed 512 MiB runtime-data ceiling
would broaden authority and exact-layout churn without addressing avoidable
compiler work.

The compiler also repeated admitted source-set, symbol, binding, and declaration
work while producing WVIR and WVB. Separately, the hosted file-input table already
carried a declared name stride, but both x64 publication leaves advanced later
snapshot names with a fixed 1 MiB shift. That hidden constant prevented a bounded
build-driver-specific name arena from being safe.

Packed native runtime failures were collapsed to process status 1 by the shared
hosted-compiler startups. Instruction exhaustion and service failures were
therefore indistinguishable from ordinary portable rejection after process exit.

## Decision

- Reuse the admitted source-WIR preparation, including its source scan, symbols,
  local bindings, and summary, when constructing WVB. Remove the second
  declaration recount and derive export metadata from the admitted symbol lookup.
- Build one bounded source-graph adjacency directory after source-set admission and
  use it for valid-path incoming-edge queries. Keep the original cycle-diagnostic
  walk so rejection identity and source location remain unchanged.
- Give only hosted profile 2 a 234,881,024-byte dynamic text/byte arena and an
  8,192-byte file-input name stride. Profiles 1 and 3 through 7 retain the exact
  134,217,728-byte arena and 1,048,576-byte stride.
- Advance the explicit instruction budget for compiler-family profiles 1 through
  7 from the qualified 48,000,000,000 ceiling to 64,000,000,000. This is a
  shared candidate metadata change, not a claim that every profile measured a
  need for the additional bound; paired reconstruction and qualification remain
  required before promotion.
- Retain the ordinary file-input and file-output scratch capacities. The resulting
  runtime extents are 510,214,144 bytes on Windows and 508,116,992 bytes on Linux,
  both below the fixed 512 MiB runtime-data ceiling.
- Make both x64 file-input leaves advance snapshot-name slots using the admitted
  table stride. The generic standalone file-input-table constructor remains exact
  at 1 MiB; the 8 KiB value is admitted only by the build-driver profile.
- Preserve packed native statuses in the shared compiler startup. Status 5 reports
  `64 + service-detail`; instruction exhaustion remains 2. Ordinary portable Main
  results and usage/rejection statuses remain unchanged.

## Current-host evidence

The canonical-source candidate is 1,101,068 bytes with SHA-256
`7c87b171ff61278599bec200090def1ae14ba58567b07d36cfdb3a420a533e4f`.
Its current Windows package is 29,257,728 bytes with SHA-256
`875e827a512f2387a980d2871115c94ba0a17a0e9dac6b89439140dc9b7f313a`;
the paired Linux reconstruction is the same length with SHA-256
`d96da77e3dba34598975eb1c721b0195a54f746e1b9a11608bda3e384a96c590`.

One Windows native self-build completed in 57.315 seconds, returned zero, emitted
the exact success summary for 478 functions and 923,326 code bytes, wrote no
diagnostic, and reproduced the 1,101,068-byte input byte-for-byte. This run was
performed once. The older 341-second qualified compiler convergence case was not
rerun because it proves the previous baseline rather than this build-driver.

Focused static and process tests own profile-specific metadata, runtime-header,
container geometry, startup relocation, exact file-input leaves, two-snapshot
stride behavior, graph cycles and converging edges, and multi-export WVB equality.
The full suite and exact-commit dual-host qualification remain pending.

## Consequences

Current build-driver self-construction fits without changing the hosted verifier
or generic file-input contracts. The compiler-family metadata advances its
explicit instruction ceiling, while only profile 2 changes arena and name-stride
geometry. The compiler avoids repeated whole-source work while retaining exact
emitted user WVB bytes and diagnostics. Native runtime failures now leave a useful
process-level category for focused diagnosis.

The shared startup, file-input leaves, profile-2 metadata/runtime geometry, and
their construction fragments receive new exact identities. Candidate hosted-tool
packages and other shared-startup descendants remain at their last qualified or
pending snapshots until the deliberate reconstruction slice; they are not promoted
by this decision.

## Reconsideration

Reconsider this decision if the profile-2 arena again approaches exhaustion, if a
resource name can exceed 8,192 bytes under the admitted project/argument bounds,
if another profile needs the same specialization, if compiler allocation journals
replace the retained intermediate streams, or if Windows and Linux cannot reproduce
and qualify the same WVB candidate.
