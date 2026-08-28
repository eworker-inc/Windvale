# Decision 0867: Inject structured-task environment through request major 6

- Date: 2026-08-28
- Status: Implemented locally; paired-host integration evidence pending
- Requires: [Decision 0861](0861-Execute-Structured-Tasks-As-Wvb-1.32.md)
- Follows: [Decision 0866](0866-Observe-Structured-Task-Runtime-Environment.md)

## Context

Decision 0866 established the bounded task-environment oracle but the public
runner always supplied one non-expiring generation-1 environment. Deadline,
stale-context, unavailable-runtime, lost-runtime, and restarted-runtime
behavior therefore existed only in the private task-state self-test. A real
edition-1 executable could not receive exact launcher observations through the
runner boundary.

The environment is execution input, not source syntax and not module content.
It must not be read from ambient process time or mutable global state, and a
malformed request must fail before module execution or task-state allocation.

## Decision

- Advance execution-request major `6` from minor `0` to minor `1`. Minor `0` is
  rejected rather than retained as an unqualified compatibility form.
- Give major `6`, minor `1` a fixed 72-byte little-endian header:

  | Offset | Width | Field |
  | ---: | ---: | --- |
  | 0 | 4 | request magic |
  | 4 | 2 | major `6` |
  | 6 | 2 | minor `1` |
  | 8 | 4 | maximum instructions |
  | 12 | 4 | maximum call depth |
  | 16 | 4 | module byte length |
  | 20 | 4 | operation-context generation |
  | 24 | 8 | clock generation |
  | 32 | 8 | absolute deadline tick |
  | 40 | 8 | expected task-runtime generation |
  | 48 | 8 | admitted task-runtime generation |
  | 56 | 8 | observation tick |
  | 64 | 8 | observed task-runtime generation |
  | 72 | variable | exact WVB module bytes |

- Require a nonzero context generation representable as `u32`, a nonzero clock
  generation, a nonzero expected runtime generation, exact request/module
  length agreement, and the existing bounded instruction and call-depth
  limits. Admitted and observed runtime generation may be zero because zero is
  the explicit unavailable/lost observation. Deadline and tick use the complete
  `u64` range.
- Add the public runner mode:

  ```text
  wvrun --task-environment <module.wvb> <context-generation> <clock-generation> <deadline> <expected-runtime-generation> <admitted-runtime-generation> <observation-tick> <observed-runtime-generation>
  ```

  Every numeric argument is canonical unsigned decimal: no sign, whitespace,
  alternate base, leading zero on a multi-digit value, non-digit, or `u64`
  overflow is accepted. Invalid arity reports the exact usage and status `64`;
  an invalid value reports `wvb run status=Invalidˉtaskˉenvironment` and status
  `64`.
- Preserve ordinary `wvrun <module.wvb> [--report-steps]`. For a WVB 1.32 task
  entry it constructs context generation `1`, clock generation `1`, deadline
  `u64::MAX`, expected/admitted/observed runtime generation `1`, and tick `0`.
- Initialize task state from the complete request environment before scope
  construction. Scope admission observes the injected context and admitted
  runtime generation; child observation applies the injected clock, deadline,
  tick, and observed runtime generation through Decision 0866's exact priority.
- Keep child-provider generation, completion-order, and parallel worker state
  outside this request revision. They remain later Slice 7 work.

No source form, Foundation layout, WVIR operation, WVB instruction, task outcome,
or scheduler-selection rule changes.

## Evidence

`Structured-Task-Environment-Executable.wv` is a real Language 1.0 hosted
program. One source-built candidate produces valid `42`, deadline `45`, runtime
loss `46`, runtime restart `48`, stale-context `54`, and unavailable-runtime
`55`/`56` observations from explicit runner inputs. The fixture is 5,057 bytes
at SHA-256
`a2dbb84ef197d10e32286a0bd38971072e200c964a6d620975fde49ba2bcb090`.
Nine malformed command/request cases cover arity, leading zero, context-width
overflow, zero required generations, `u64` overflow, nondecimal input, and a
negative tick.

The focused owner passes all 57 phases and 159 cases: 22 valid modules, 69
malformed modules, 27 structured-task cases, 46 task-state cases, 17 executable
environment cases, and the retained collection, ownership, source-file, and
callable-runner compatibility evidence.

The runner's largest source-built native function remains below the fixed
2,048-slot lowerer boundary after environment handling was extracted from the
interpreter entry: 1 parameter plus 2,045 locals. The candidate reconstructs
exactly as:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Wvb-Runner.wvb` | 464,589 | `5c3bc6773f97e0cb9e5dc3d993d2768e5e401f73884630710e29a6a3c67ef4f2` |
| `windows-x64-wvrun.exe` | 5,659,136 | `2292555c4dad03d646d7e14d0bf716bd663d95b1d0e224f9f6c11d598b519114` |
| `linux-x64-wvrun.elf` | 5,660,672 | `ccaaa6cbb76c557e65c169ef8bad7ca3396c0a38e3e4b18adf303f94077e83d1` |

The WVB contains 222 functions and 414,206 code bytes. Segmented staging emits
5,650,368 object bytes in 13 chunks. Linking emits a 5,640,684-byte image in
nine chunks at entry offset 137,648; canonical transport uses two chunks. An
independent local reconstruction produced byte-identical copies of all three
artifacts.

The verification registry remains 114 owners and advances to 5,556 cases. Its
18,556 LF-only bytes have SHA-256
`d6b392ea29535d645f24dbc5be9b84688038744d654687df60096e2da320bc81`.
Independent Windows/Linux affected-owner integration remains required before
this local candidate becomes paired-host evidence.

## Consequences

Launcher-owned task observations now cross one explicit, versioned, bounded
request boundary and can be reproduced without consulting wall-clock or global
runtime state. The ordinary command remains simple, while conformance and host
launchers can exercise exact deadline and runtime-generation behavior.

The request deliberately carries observations rather than a clock or runtime
provider handle. A future long-running/parallel host must own how those values
advance and when it calls the same cooperative oracle; it must not reinterpret
this fixed request as ambient polling authority.

## Reconsideration triggers

Revise the request in a new minor or major version if a parallel host needs
multiple observation points, if provider generations require a separate typed
attachment, or if the fixed one-observation launch model cannot express an
accepted workload. Preserve exact length validation, canonical integer parsing,
nonzero required generations, deterministic outcome priority, and the absence
of ambient time or runtime authority.
