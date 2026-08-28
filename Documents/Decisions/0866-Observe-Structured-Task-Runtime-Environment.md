# Decision 0866: Observe structured-task runtime environment

- Date: 2026-08-27
- Status: Implemented locally; paired-host reconstruction and integration evidence pending
- Requires: [Decision 0861](0861-Execute-Structured-Tasks-As-Wvb-1.32.md)
- Follows: [Decision 0865](0865-Reserve-Structured-Task-Retained-Memory-Before-Spawn.md)

## Context

The sequential WVB 1.32 runtime could close a scope idempotently and store
explicit terminal outcomes, but its private task state did not retain the
operation-context, clock, deadline, or task-runtime generation needed to decide
those outcomes. Cancellation therefore stopped later spawn without giving a
runnable child a cooperative observation point. Scope construction also
validated limits but could not distinguish stale context, unavailable runtime,
and exhausted runtime storage through the frozen public failure variants.

These are runtime observations, not new source syntax or bytecode opcodes. They
must remain deterministic, bounded, and separate from a child's typed provider
failure `E`.

## Decision

- Advance the private fixed task-state encoding from version 2 to version 3 and
  from 10,000 to 10,040 bytes. Its 56-byte header retains the root context
  identity/generation, clock generation, absolute deadline, expected
  task-runtime generation, and currently observed task-runtime generation.
- Reserve root context identity `4294967295` so it cannot alias any of the 32
  bounded scope identities. A scope-derived context is valid only while its
  exact scope identity and generation remain live.
- Map scope-construction refusal to the exact frozen public distinction:
  invalid limits, allocation failure, stale parent context, or unavailable
  runtime generation. Rejection leaves state and the caller-owned budget
  unchanged.
- Add one private task observation operation for a runnable child. It validates
  the exact task token and clock generation, then applies this order:
  1. at or after the absolute deadline, publish `Deadlineˉreached`;
  2. before the deadline, observed runtime generation zero publishes
     `Runtimeˉlost(Expected)`;
  3. a different nonzero generation publishes
     `Runtimeˉrestarted(Expected, Observed)`;
  4. otherwise a closed origin scope publishes `Cancelled`;
  5. otherwise leave the task runnable and state byte-identical.
- Terminal observation uses the existing completion transition and its exact
  eight-byte empty result cell. Cancelled and deadline outcomes carry no
  evidence, runtime loss carries only the expected generation, and runtime
  restart carries two nonzero distinct generations.
- A malformed task, completed task, or wrong clock generation is an invalid
  observation and leaves state byte-identical. Cancellation remains
  cooperative rather than an asynchronous exception.
- Keep the ordinary sequential runner's current environment non-expiring with
  context, clock, and runtime generation 1. Host/request injection, child
  provider generations, and parallel scheduling remain later Slice 7 work;
  this checkpoint establishes the bounded runtime oracle they must call.

No source form, Foundation layout, WVIR operation, WVB opcode, or public task
outcome changes.

## Evidence

The task-state self-test adds eight exact cases: stale parent context,
unavailable runtime, cancellation observation, exact-deadline priority over
runtime loss, runtime loss, runtime restart, invalid clock generation, and a
valid nonterminal observation. It now passes 46 cases and returns `42` through
the native package.

The focused `language-1-memory-budget-split-execution` owner passes all 56
phases and reports 142 cases: 21 valid modules, 69 malformed modules, 27
structured-task cases, the 46-case runtime core, and the retained collection,
ownership, source-file, and callable-runner compatibility evidence.

The repinned local runner candidate is:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Wvb-Runner.wvb` | 450,825 | `fd65e221c22a48fb20da47e18099351d338a0e8107357e6a04246c2a7f31a9ef` |
| `windows-x64-wvrun.exe` | 5,429,248 | `2080d9fed98f9f07ee0fc07036823ff271214c426b00f9d5bf08d5fcf4a78c38` |
| `linux-x64-wvrun.elf` | 5,431,296 | `6f645b05d9d3b8e2cae34703487f559e5212155fc4ff02c374176ed7e9844054` |

The WVB contains 216 functions and 402,863 code bytes. Segmented staging emits
5,420,317 object bytes in 13 chunks; linking emits a 5,411,237-byte image in
nine chunks at entry offset 105,270; canonical transport uses two chunks.

The verification registry remains 114 owners and advances to 5,539 cases. Its
18,379 LF-only bytes have SHA-256
`263ec9e6314505d5442f822bf4029cca8903d6e203d89e534a2db70d2463befe`.
The focused three-case reconstruction owner independently rebuilds all three
artifacts from source, proves byte equality, and passes current-host execution,
reporting, usage rejection, and malformed-module rejection. The repinned
distribution passes eight installer lifecycle cases and twelve selective
installer-repository cases. Independent Windows/Linux integration remains
required before this local candidate becomes paired-host evidence.

## Consequences

The runtime now has one explicit, deterministic place to translate changing
environment state into the frozen task outcomes. The scheduler does not poll
ambient process state, synthesize exceptions, or confuse runtime replacement
with provider failure. The additional state remains fixed and validated before
use; observation performs bounded token checks and one terminal transition.

The default eager runner does not yet expose real time or external generation
changes, so this decision is an enabling checkpoint rather than the final
cancellation/deadline workload or parallel-host claim.

## Reconsideration triggers

Reconsider the private header when a parallel scheduler needs independently
owned per-worker state or when the selected clock contract needs more than one
generation and absolute tick. Any replacement must preserve exact deadline
priority, cooperative cancellation, generation evidence, byte-identical
invalid-observation behavior, bounded state, and separation from provider
errors.
