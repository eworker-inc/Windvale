# Windvale process launch and supervision architecture

## Status

Recommended next architecture under proposed [Decision 0198](../Decisions/0198-Next-Integrated-Architecture-Defaults.md). It details the accepted clean-spawn, service-manager, process-role, and resource-domain direction in [Decision 0173](../Decisions/0173-Windvale-Process-Service-And-Driver-Architecture.md). Windvale OS does not yet implement a flat resource domain, dynamic process creation, a launch transaction, service supervision, restart policy, or a public process ABI.

## Recommendation

Windvale should use one atomic clean-spawn transaction and one supervision model for applications, helpers, services, drivers, runtimes, and future VMMs. Roles select policy; they do not select a different kernel process type or grant authority.

Launch has two records at different trust boundaries:

- a semantic application launch plan is assembled in user space from package, command, policy, and provider evidence; and
- a kernel admission plan contains only checked executable, mapping, capability, budget, resource-domain, entry-thread, and lifecycle objects.

The kernel does not parse package names, dependency versions, command aliases, native paths, service names, or restart policy.

## Semantic launch plan

The immutable user-space plan should bind:

- plan version and correlation identity;
- exact package, module, entry-point, verifier, runtime, target, and AOT identities;
- platform scope, authority level, and role;
- bounded ordered arguments and an optional immutable environment snapshot;
- standard input, output, and diagnostic stream bindings;
- an optional directory capability plus provider-supplied display identity;
- the complete approved transitive capability requirements and exact rights-reduced provider instances;
- resource-domain identity and per-process limits below that domain ceiling;
- terminal/session attachment, cancellation source, observer, and completion destination; and
- service dependencies, publication names, criticality, graceful-stop deadline, and restart policy when the role is supervised.

Resolution evidence and content identity are inseparable. Replacing a package or module after authorization requires a new plan and a new authorization decision.

Arguments, environment, current location, streams, and capabilities are explicit fields. None is inherited because one process launched another. A missing optional capability is represented as an absent binding before entry, not as a provider call that might acquire ambient authority later.

## Kernel admission plan

The launcher translates the semantic plan into a smaller kernel-facing record containing:

- admitted executable publication and entry point;
- new address-space and initial memory-object mappings;
- initial stack and thread state;
- resource-domain membership and exact reserved charges;
- capability-table entries created by explicit copy-reduced or move transfer;
- observer, cancellation, and completion kernel objects; and
- a digest over the immutable admission inputs for diagnostics.

The kernel validates all identities, generations, rights, ranges, limits, transfers, mappings, W^X rules, and aggregate charges before the process becomes visible as runnable. It may expose a construction reference only to the authorized launcher. Observation, cancellation, termination, and capability-transfer rights are distinct.

## Atomic launch transaction

The transaction has five semantic phases:

1. **Resolve** the exact package part, module, runtime, target, and provider candidates.
2. **Authorize** the complete transitive capability set and reduce every concrete grant.
3. **Reserve** domain and kernel capacity for the complete initial process.
4. **Construct** a non-running address space, mappings, capability table, streams, observer, and initial thread.
5. **Publish** the process identity and make its first thread ready in one commit.

Failure before publication rolls back every memory object, mapping, table entry, transfer, reference, stream endpoint, and charge. No service registry, observer, or child can see a half-created process. A moved capability changes ownership only at the successful commit point.

Windvale does not make `fork`, inherited descriptors, ambient environment, current-directory strings, parent credentials, or process-global native handles foundational. A compatibility provider may emulate a bounded subset above this model.

## Capability transfer

Every initial grant specifies source identity and generation, requested rights, destination slot policy, and transfer mode:

- **copy-reduced** creates a new reference with rights no greater than the source while retaining the source reference; or
- **move** transfers the selected reference at launch commit and removes the source reference atomically.

Transfer never amplifies rights and never follows a service name to a replacement provider implicitly. Non-transferable capabilities are rejected during authorization. Provider restart produces a new generation and requires a deliberate rebind.

## Observation and completion

The first structured completion record should contain:

- process and launch identities plus generations;
- disposition: completed, launch-rejected, capability-refused, cancelled, forced, trapped, faulted, or provider-lost;
- optional application result for normal completion;
- stable component and reason code plus bounded diagnostic reference;
- CPU, instruction, peak-memory, output, and teardown accounting available under the selected profile; and
- a flag proving whether all owned resources reached terminal cleanup.

A wait or observer capability does not imply permission to cancel, terminate, inspect memory, read diagnostics, or reuse capabilities. These rights remain separate.

## Service supervision

The service manager owns dependency and restart policy in user space. The boot or service dependency graph is immutable, bounded, and acyclic before launch; optional dependencies are explicit and do not create hidden start-order edges. The recommended service lifecycle is `Planned`, `Starting`, `Available`, `Draining`, and terminal `Stopped` or `Faulted`; `Unavailable` describes the published provider outcome, not a kernel process state.

A service is published only after initialization completes and every promised endpoint is bound. Graceful stop first prevents new calls, then drains bounded in-flight work, then exits. Forced stop fails waiters explicitly and invokes resource-domain teardown.

The first restart policies are deliberately small:

- `Never`;
- `Onˉfault` with a maximum attempt count, monotonic time window, deterministic or bounded backoff, and a stable exhausted result; and
- `Always` only for a later measured long-running provider that distinguishes planned stop from unexpected exit.

There is no infinite restart loop and no restart without reserved recovery resources. Boot-critical failure selects one explicit policy: degrade, enter a bounded recovery environment, shut down, or reboot. It does not silently grant init more authority.

Restart creates a new process and provider generation. Clients observe peer loss. Read-only idempotent operations may be retried only under their interface contract; an indeterminate mutating request is never replayed automatically.

## First measured slices

1. Retain Probe 40's qualified independently lived memory-object baseline and add one flat resource domain around the existing processes and objects.
2. Dynamically launch one known verified child with one input resource, three explicit streams, one reduced capability, and one observer. Reject malformed plans before the child is visible.
3. Exercise normal completion, verifier rejection, capability refusal, trap, process fault, cancellation, forced stop, provider loss, and launcher death. Each leaves zero charges and stale generations unusable.
4. Move one non-critical existing provider under the service manager with `Never` restart and explicit availability publication.
5. Add bounded `Onˉfault` restart, generation-safe rebinding, and exhausted-restart policy without replaying a mutation.
6. Use the same transaction to launch the terminal shell and first isolated serial service.

## Deliberately open details

The architecture does not yet freeze source syntax, serialized plan layout, object indices, service-registry encoding, exact restart limits, backoff formula, public process IDs, environment keys, diagnostic storage, or syscall numbers. It does fix clean spawn, two-level plans, atomic publication, explicit transfer, separate observation and control rights, generation-visible restart, bounded policy, and complete domain teardown.
