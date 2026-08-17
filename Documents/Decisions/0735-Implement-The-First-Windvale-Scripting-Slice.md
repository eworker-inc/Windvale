# Decision 0735: Implement the first Windvale scripting slice

- Date: 2026-08-16
- Status: Implemented with focused Windows execution evidence; independent Linux execution pending
- Accepts in part: [Windvale scripting proposal](../Project/Windvale-Scripting-Proposal.md)
- Defines: [Windvale scripting 1](../../Specifications/Windvale-Scripting.md)

## Context

Windvale already had separate native build, verification, and bounded execution
tools, but running one source file required exposing their intermediate WVB and
tool-oriented command shapes. Mirroring every capability declaration as a
launcher flag would make ordinary scripts noisy and would create a second,
growing authority vocabulary at the command line.

The retained scalar interpreter already separated complete WVB admission from
bounded execution and carried one line-output capability. The installed `wv`
client already provided a small cross-host product front door.

## Decision

- Add `wv run <source.wv> [argument ...]` for one ordinary source module that
  exports `Main() -> i32`.
- Treat every token after the source as script data. Do not require `--` and do
  not add per-capability `--allow` switches.
- Compose the installed native `wvbuild`, `wvverify`, and `wvrun` tools through
  a private temporary WVB. Verification remains mandatory on every invocation;
  the first slice has no cache.
- Extend the retained interpreter transport with `WVXI 4` / `WVXO 4` for a
  bounded immutable argument vector, separate line-output buffers, and four
  exact base capability identities.
- Automatically grant only `console.write_line`, `diagnostic.write_line`,
  `process.argument`, and `process.argument_count` when declared by the module.
- Reject every other capability before guest execution. In particular, do not
  grant ambient file, environment, clock, network, process, or system access.
- Preserve the existing direct `wvrun <module.wvb> [--report-steps]` contract.
- Install the current source-reconstructed runner candidate rather than the
  older frozen front-door runner, and add one paired focused scripting owner.
- Keep the qualified WebAssembly interpreter on its exact version-3 source
  snapshot and 1 MiB generated-module ceiling. Version 4 is a private native
  runner transport, not a browser ABI expansion.

## Consequences

Windvale can now execute useful local `.wv` scripts without exposing a WVB or
requiring an authority-option list. Script arguments and output remain bounded,
declared capabilities remain distinct from grants, and malformed or unsupported
programs fail before guest work.

The first slice supports line output rather than `console.write` or semantic
byte output. It also excludes imports, project manifests, caching, file access,
and approval summaries. Those remain later slices and must extend the one
authority model rather than create a parallel scripting runtime.

The Windows wrapper uses the installed PowerShell already required by the
Windows installer; the Linux wrapper uses POSIX shell composition. Neither host
wrapper defines source or bytecode semantics.

The native runner and browser retain the same version-1 through version-3
behavior. The browser snapshot is explicit because compiling the larger
version-4 interpreter through the currently qualified backend would exceed its
fixed generated-module ceiling; changing that ceiling requires a separate
WebAssembly decision and artifact reconstruction.

## Evidence

The focused Windows scripting owner passes six cases. The WVB runner rebuilds
from its Project 2 closure to one canonical WVB, one ABI-22 WVO, and paired
Windows/Linux images. Independent Linux execution remains required before
claiming paired-host behavioral qualification.

## Reconsideration triggers

Reconsider this decision if scripts need imports in the common one-file path,
line-only output proves insufficient, temporary compilation becomes a measured
bottleneck, the 65-argument transport is too small, PowerShell is removed from
the Windows installer contract, or a later approval UI cannot express one
reviewable authority summary without per-capability CLI accumulation.
