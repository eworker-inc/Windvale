# Decision 0467: Native WVHV service-bundle process

- Status: Implemented current-host candidate; independent Linux execution and promotion pending
- Date: 2026-08-09
- Advances: [Decision 0466](0466-Native-WVHV-Request-Container-Reconstruction.md), [Decision 0464](0464-Native-WVHV-Evidence-Process.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier service bundle](../../Specifications/Windvale-Native-Hosted-Verifier-Service-Bundle.md)

## Context

The shared Windvale `WVSQ 2` materializer already supports an arbitrary bounded
publication service count. The remaining verifier boundary was narrower: Stage
0 still constructed the exact request containing the verifier fragment and its
six ordered service leaves. The established generic hosted-tool request process
is intentionally fixed to a different ten-service profile and should not be
widened implicitly.

## Decision

- Add one portable verifier-profile request owner for the exact services `1`
  through `6`. It constructs the 96-byte `WVPQ 1`, requires the shared planner
  to accept the resulting layout, and emits one complete bounded `WVSQ 2`.
- Keep service selection, platform leaf generation, digest verification, and
  authority admission outside this producer. It receives immutable bytes in
  canonical order and does not reinterpret machine code.
- Feed the result into the existing portable service-bundle materializer. Do
  not add a second materialization format or implementation.
- Package the hosted wrapper through the ordinary native hosted-container path
  and retain both platform products in the digest-bound candidate. Add no C#
  product writer, recovery target, or ordinary dispatch entry.

## Evidence and consequences

The request tool is a 13,993-byte native-built WVB with SHA-256
`b23655332f5525fd411cb3a0a1f815af49f97d743156dfd4d0ae7549fab586f4`.
Its retained applications are:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows x64 | 160,256 | `cc57bac2881ec4763510310b2b54fd48b23dddef7eb2ef1d6e1ef380955d3d09` |
| Linux x64 | 159,744 | `da0a377ebfc821584b239e58cd9891e89a4711554fee8106e690c867627a6288` |

The focused named test passes 1/1. Its actual test body completes in 5.172
seconds after the one build and verifies both platform leaf sets. The process
produces exact 990,717-byte Windows and 990,405-byte Linux requests, matches the
frozen request oracle byte-for-byte, and makes the unchanged Windvale
materializer reproduce both complete verifier bundles. Duplicate inputs and an
output alias preserve the affected resources. C# participates only as test and
differential evidence; no managed production or recovery implementation was
added.

The hosted candidate now binds 63 artifacts: 21 native-built WVBs and their
paired Windows/Linux applications. Its 6,027-byte inventory has SHA-256
`380827f9954ed0b5b687bc0644f3b57b6de92fabcb9a15719082dde39dc0915f`;
all entries match. Including manifest and inventory, it contains 65 files
totaling 16,381,159 bytes. Focused current-host reconstruction reproduces the
new Windows and cross-target Linux applications exactly. The five unchanged
packaging-smoke cases were not rerun.

This closes pure request and bundle-byte construction. Canonical verifier
startup, runtime placement, outer layout/plan, platform bytes, final
publication, independent Linux execution, grouped qualification, promotion,
and recovery-source deletion remain. No broad Seed, OS, Standard,
Qualification, WebAssembly, or QEMU gate ran.

## Reconsideration triggers

Version the profile if verifier authority, service order, publication geometry,
or the bounded segment contract changes. Generalize the generic hosted request
producer only when a shared policy can preserve each profile's exact service
and authority contract.
