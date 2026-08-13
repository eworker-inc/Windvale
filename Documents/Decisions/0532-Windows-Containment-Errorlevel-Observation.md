# Decision 0532: Windows containment ERRORLEVEL observation

- Date: 2026-08-13
- Status: Superseded by [Decision 0533](0533-Restore-Verifier-Specific-Front-Door-Startup.md)
- Supersedes: [Decision 0531](0531-Restore-Bounded-Asynchronous-Windows-Containment-Collection.md)
- Contract: [Native random containment tests](../../Specifications/Windvale-Native-Random-Containment-Tests.md)

## Context

Three consecutive GitHub Windows qualification jobs observed native verifier
status zero despite the exact `wvb status=Invalid phase=semantic` rejection
diagnostic. Synchronous Node.js collection failed on fixed inputs `Wvb-0917` and
`Wvb-0880`. Restoring asynchronous collection then failed immediately on
`Wvb-0000`. Linux qualification and local Windows execution remained correct.

At the time, the changing input ordinal, correct diagnostic, and failure under both
Node child APIs appeared to isolate the issue to direct Node/libuv observation of
this custom PE's process status on the hosted Windows runner. The native verifier's
exact exit assertion remained mandatory; treating a diagnostic as a substitute
would have weakened the product CLI contract.

## Decision

Keep the shared asynchronous bounded collector and its four-worker limit. On
Windows only, launch the native command through a focused inbox command-file
adapter. The adapter executes one child with one or two arguments, snapshots the
child's immediate `%errorlevel%`, writes one private ASCII status marker to standard
output, and exits zero. Node collects the adapter asynchronously, requires its zero
exit, parses and removes the exact marker, and returns the inner native status plus
the child's original output streams to the unchanged assertions.

Pass paths and arguments through private per-child environment entries so command
text does not interpolate corpus or workspace paths. The adapter supports only the
argument arities used by the three containment lanes. Unknown arity, missing or
malformed marker, outer failure, signal, output overflow, or out-of-range inner
status fails closed.

This is an independent Windows process-observation adapter, not a retry. It does not
translate status zero, infer success or failure from diagnostics, change the corpus,
or modify the verifier application.

## Consequences

- The exact native status remains mandatory and is observed through the Windows
  command processor's child `ERRORLEVEL` contract rather than Node/libuv's direct
  custom-PE status.
- Windows retains at most four concurrently executing native containment children.
- Linux keeps direct asynchronous child status collection.
- The adapter is native inbox command processing and introduces no C#, .NET,
  managed fallback, new semantic oracle, or third-party dependency.
- Changed-file verification maps the adapter to source, WVB, and WVO containment.

## Evidence boundary

GitHub Verify run `31672940187` also failed through this adapter on fixed case
`Wvb-0002`: the command processor observed the same status zero after the verifier
printed its correct rejection. That result disproved the process-observation
hypothesis and isolated the defect to the product artifact. Decision 0533 removes
the adapter and restores the verifier-specific startup artifact.

## Reconsideration triggers

Replace the adapter when Node/libuv reliably reports this explicit `ExitProcess`
status on the supported Windows runner, when Windvale owns a smaller cross-host
process-status host, or when a containment command needs more arguments,
cancellation, or a wall-clock termination contract.
