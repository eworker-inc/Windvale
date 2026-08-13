# Decision 0531: Restore bounded asynchronous Windows containment collection

- Date: 2026-08-13
- Status: Superseded by [Decision 0532](0532-Windows-Containment-Errorlevel-Observation.md)
- Repairs: [Decision 0524](0524-Windows-Native-Retirement-Exit-Closure.md)
- Contract: [Native random containment tests](../../Specifications/Windvale-Native-Random-Containment-Tests.md)

## Context

Two consecutive GitHub qualification runs failed only in Windows WVB
containment. The verifier emitted the correct semantic-rejection diagnostic but
the Node.js synchronous child API intermittently reported process status zero:
`Wvb-0917` in run 31665094047 and `Wvb-0880` in run 31669660884. Both fixed corpus
inputs are unrelated short arbitrary-byte values. Linux qualification passed, the
complete 1,000-case Windows owner passed locally, and 500 repeated local executions
of `Wvb-0917` all returned rejection status one.

The Windows synchronous branch was introduced before Decision 0524 gave verifier
and inspector applications an explicit `ExitProcess` boundary. Retaining that
workaround after the native repair also meant the nominal four-worker Windows loop
could run only one native process at a time while the JavaScript event loop was
blocked.

## Decision

Remove the Windows `spawnSync` branch from the shared containment host. Windows and
Linux now use the same bounded asynchronous `spawn` collector. It observes process
completion together with closed output channels, rejects signals and channel
overflow, and returns the exit status and both bounded byte streams as one result.

Keep the exact exit-code assertion. A semantic rejection diagnostic does not excuse
a product process that really exits successfully. This correction changes how the
independent host observes the already explicit native process contract; it does not
retry a case, reinterpret a zero status, or weaken containment evidence.

Map the shared corpus loader, process host, and top-level runner to all three native
containment owners in changed-file verification. Previously only the binary-family
module had explicit planner ownership.

## Consequences

- Windows again honors the specified bound of at most four independent native
  processes per lane.
- Both permanent hosts use one process-collection implementation.
- The fixed corpus, native verifier applications, diagnostics, exit expectations,
  resource limits, and case counts remain unchanged.
- The correction contains no C#, .NET execution, managed fallback, or test retry.

## Evidence boundary

The focused Windows source, WVB, and WVO owners must pass from the corrected source
state. GitHub must then provide a new independent dual-host qualification result;
local success alone does not close the reported issue.

GitHub run 31671393519 then failed on the first WVB input with the same diagnostic
and false status-zero observation. Asynchronous collection restored concurrency but
did not make Node/libuv a reliable direct observer for this custom Windows PE. The
common asynchronous structure is retained, while Decision 0532 replaces only the
inner Windows status observation.

## Reconsideration triggers

Revisit the collector if Node.js changes child close/status semantics, a native tool
needs cancellation or wall-clock termination, a channel needs a different explicit
bound, or an exit mismatch reproduces after this correction.
