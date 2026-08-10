# Decision 0510: Native Foundation build, inspect, and execution transfer

## Status

Implemented current-Windows evidence. Independent Linux execution and grouped
qualification remain pending.

## Context

Both broad Seed verification scripts still compiled and inspected four small
Foundation modules through the feature-frozen Stage 0 CLI. They also compiled
four associated demos and executed all four through the managed reference
runtime. These were normal-path invocations even though the native Project 1
builder, WVB inspector, and profile-5 runner already owned the relevant
interfaces.

The Machine Contracts and Decimal Parsing demos require 4,943 and 4,352 guest
instructions. The runner's former 4,096-instruction request limit was narrower
than the Stage 0 CLI's ordinary 1,000,000-instruction default and rejected both
otherwise-supported programs. The Byte Construction demo has a different
boundary: it constructs an exact 4 MiB value and the current scalar runner
returns bounded failure `3015` after 1,375 instructions.

Project organization is also part of this transfer. Project 1 paths are
contained beneath their manifest directory and cannot use `..` to escape it.
The four single-component Foundation manifests can therefore live beside their
source. The four demos combine `Examples/` roots with `Foundation/` sources, so
their present aggregate manifests live at the repository common ancestor until
a workspace/project-reference contract removes that root-level pressure.

## Decision

The native runner's fixed request budget becomes 1,000,000 instructions. This
does not expose a new option or make the budget ambient; it aligns the fixed
native product with the established ordinary CLI default. The source-built
runner is reconstructed through the existing digest-bound WVB, WVO, link, and
paired profile-5 construction path.

The paired `Verify-Seed-Native-Front-Door` helpers now build these exact
products:

| Product | Bytes | SHA-256 |
| --- | ---: | --- |
| Machine Contracts | 2,466 | `f624739461dea01862121daf234b3a838dfcafd73753e3124a038b7efa8b4fa3` |
| Machine Contracts demo | 3,487 | `69106233197b3dbc33f23184eaa443505e8595aa056e9e2e10659a33eeefeea3` |
| Byte Ordering | 990 | `27a3c24b5cc358a4f67e2e1959b5e80559918f0176c52e08648e638212e6dece` |
| Byte Ordering demo | 2,422 | `fbaf423b6e4eac5c18b644dc27f1fa20fca8798519596485cd7497b44979533f` |
| Decimal Parsing | 1,698 | `bb120d1098855b8b4adced6bcd1b1ab695f115e76bebdacb19a2b07b798cad37` |
| Decimal Parsing demo | 3,742 | `d323f8fa9178583990394a37872a8ee522320084ef4741eac26cb0f86c21b453` |
| Byte Construction | 2,001 | `3be0d06b8f4e7745dd9ffd9f325804d69ce524ac7ff6341b1e7b38037f6dd6f8` |
| Byte Construction demo | 5,017 | `ab594976ced7a84573ade0aa50fb4370d96b8004c8b9a5ec1e888968c7b3bf8f` |

The helpers inspect all four Foundation modules and execute the Machine
Contracts, Byte Ordering, and Decimal Parsing demos to exact result `0` while
preserving their bytes. The broad scripts consume those native-built products
and no longer repeat the corresponding eight managed compiles, four managed
inspections, or three managed executions. Byte Construction's plain execution
and per-function dynamic-value/lifetime/allocator reports remain explicitly in
the managed differential lane until the runner owns that value-memory shape.

The current runner products are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB | 121,593 | `e58f653445cd717d19c32fe1a0fbc57f03f475187cdec571825b9fd6685b3097` |
| WVO | 1,078,577 | `7d0ec719ade7e55d46c5a6dc6f7cb63102db4633172bcab1812e16651002106d` |
| linked fragment | 1,077,675 | `83dc076c137557495a24e65894c26c7f794e0d67f31dd59a476e1dc7715828d1` |
| Windows application | 1,094,656 | `6af8988f18c69a6757daeef8376c22ecbae406c31652813607fe2c3a6aa43ffc` |
| Linux application | 1,093,632 | `a674b455aecaec48889318fd190a2123bc8bc784b1ee9b9eaa76b491ebebcb2d` |

## Evidence

- `Test-Wvb-Runner-Reconstruction.cmd` passes 3/3 in 49.8 seconds.
- `Verify-Seed-Native-Front-Door.ps1` passes its 24-call ownership contract
  over twelve artifacts in 6.3 seconds.
- The focused frozen Stage 0/native WVB-runner differential passes 1/1; the
  managed test body completes in 32.145 seconds.

This removes fifteen additional managed invocations from each broad host
script, thirty cumulatively across Decisions 0505, 0506, 0508, 0509, and 0510.
It does not remove a direct managed entry file: the inventory remains three
normal direct files plus nine recovery files, and T2 remains `managed-normal`.

## Consequences

Foundation-owned manifests are colocated by default. Root manifests in this
slice are explicit cross-component aggregates, not a convention for all future
projects. A later workspace, package-index, or project-reference design should
permit those aggregates to move without weakening Project 1 containment.

The current evidence is a Windows-host native build/inspect/execute result plus
cross-target runner construction. It is not independent Linux execution,
complete capability-bearing execution, complete dynamic-value execution,
per-function native profiling, a clean or previous-seed bootstrap, grouped
qualification, promotion, or recovery deletion.

## Reconsideration triggers

Reconsider the fixed runner budget when Windvale exposes an explicit bounded
execution policy rather than one product default. Reconsider the aggregate
manifest placement when Project 1 gains workspace or project-reference
semantics. Transfer the Byte Construction execution only after a native owner
reproduces its exact 4 MiB value behavior and retained profiling evidence.
