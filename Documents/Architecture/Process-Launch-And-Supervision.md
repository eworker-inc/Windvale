# Windvale process launch and supervision architecture

## Status

Recommended architecture under proposed [Decision 0198](../Decisions/0198-Next-Integrated-Architecture-Defaults.md). It details the accepted clean-spawn, service-manager, process-role, and resource-domain direction in [Decision 0173](../Decisions/0173-Windvale-Process-Service-And-Driver-Architecture.md). Portable [resource-domain policy 1](../../Specifications/Windvale-Os-Resource-Domain-Policy.md) gates Probe 40's fixed three-process accounting; [application-launch policy 1](../../Specifications/Windvale-Os-Application-Launch-Policy.md) and [machine-construction policy 1](../../Specifications/Windvale-Os-Application-Machine-Construction-Policy.md) now give both sequential fixed clients live versioned-request/executable-publication/reserve/private-construction/publication/rollback transactions with different checked page layouts. The request proves the init caller, generation-safe executable publication, domain, and rights profile while deriving the child identity. [`WVSR 1`](../../Specifications/Windvale-Os-Application-Start-Request.md) now supplies an independently executed checked 64-byte application request decoder. Windvale OS does not yet implement the user-memory copy and start syscall, a dynamic resource-domain object, arbitrary admitted image loading, runtime machine-object allocation from a launch request, service supervision, restart policy, or a public process ABI.

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

Service-launch policy 1 now applies that lifecycle to exact filesystem and
network `WVPR 1` profiles through independently executed portable policy. It
does not yet perform the kernel allocation, launch, or endpoint mechanisms.
The first Windvale-owned x86-64 emission component now supplies the checked
label/fixup/placeholder primitive for porting those privileged process-machine
mechanisms from the remaining reviewed fixture. Its first consumer emits the
exact 1,119-byte coordinator entry and bounded three-record ready/wait
dispatcher. The next 309-byte source-owned slice initializes memory/context
state and publishes seven failure plus one policy-call relocation fields; these
are followed by exact construction of both retained resource/directory channel
and endpoint pairs through byte 1,871. These constructors still do not
themselves launch a process. The first checked kernel memory-object call and
its returned-extent validation extend that source boundary through byte 1,970.
Complete zeroed init-record construction from two verified 32-byte digest
inputs advances the boundary through byte 2,432. Retained kernel-table copy,
bounded private PTE construction, null-page denial, and exact W^X init mappings
advance it through byte 2,948. Four bounded relocated input copies plus the
native execution context and store descriptor now advance it through byte
3,097. Checked 122-page recyclable-client reservation and private root retention
now advance it through byte 3,215 without initializing or publishing the client.
Checked ten-page directory-provider allocation now advances it through byte
3,322. Complete private record construction from verified service and snapshot
identities now advances it through byte 3,784 without paging or publication.
Directory-private retained-table copy, null-page denial, and exact W^X mappings
now advance it through byte 4,224 without copying provider inputs or publishing
readiness.
Bounded relocated service/snapshot copies, native context, and the generation-
tagged snapshot descriptor now advance it through byte 4,340 without readiness
publication. Exact private recyclable-client record construction from admitted
interpreter/program identities, bounded execution geometry, and separate
resource/directory capabilities now advances it through byte 4,858 without page
tables or readiness publication. Retained table copy, exact client-private W^X
mappings, null-page denial, and two post-extent guard entries now advance it
through byte 9,606 without input copies or readiness publication. The bounded
interpreter copy and private execution context now advance it through byte
9,682 without resource completion or readiness publication. Clear-before-
populate construction of the first generation-one program resource now advances
it through byte 9,930; the separate generation-two budget resource advances it
through byte 10,159 without readiness publication.
The generation-three immutable store resource advances private construction
through byte 10,398 without readiness publication.
The generation-four read-only directory resource advances private construction
through byte 10,637 without readiness publication.
The following store validation advances source ownership through byte 11,031
and rejects mismatched identity, geometry, generation, digest, private pointers,
page-table linkage, or W^X permissions before the client may use that resource.
Its twenty-two explicit failure branches still precede readiness publication.
The corresponding directory validation extends ownership through byte 11,441
and adds exact snapshot-count and mapped-byte checks. Its twenty-three failure
branches likewise reject before any client readiness publication.
The following privileged-entry slice constructs GDT/TSS state, installs four
explicit exception gates, and programs syscall MSRs through byte 12,082. Its
hosted reproduction proves exact bytes and relocation identities, not live CPU
delivery; handler bodies, dispatch, timer setup, and publication remain later.
Three private thread records and the first bounded timer record then extend
ownership through byte 12,872. They retain explicit owner/generation, saved
context, selector, budget, and page-table state without scheduling or publishing.
The timer activation transaction then validates the selected page table, binds
per-thread GS state, arms the architecture timer, rolls back on rejection, and
transfers through the explicit resume boundary through byte 12,997.
The selected directory-provider thread is then reacquired and checked before its
page table, GS ownership, kernel continuation, and admitted user context are
loaded for the first `sysretq`, advancing ownership through byte 13,168.
On provider return, the machine validates the provider thread and process,
selects only the admitted init thread, revalidates its page table, binds its GS
and continuation state, and performs the next `sysretq`, advancing ownership
through byte 13,447.
When init returns, the following transaction requires the admitted init thread
and process states, then reacquires and validates the generation-one client
program resource, its exact geometry and rights, its owner generation, and its
private page-table linkage. Any mismatch reaches the common fail-closed boundary
before client activation, advancing ownership through byte 13,786.
The adjacent budget transaction applies the same generation, authority, and
private-mapping checks to the generation-two budget resource. The following
backing-record transaction validates the exact program/budget record geometry
and their retained store/directory bindings, including identity, generation,
rights, owner, page-table, and empty mutable-state fields. Together they close
client-resource validation through byte 14,402 before context transfer.
The client-transfer transaction then leaves the returning init context, accepts
only the admitted client role and generation from the fixed dispatcher,
reactivates its checked page table, binds GS and continuation ownership, loads
the private user context, and performs `sysretq`, advancing ownership through
byte 14,576.
On client return, the machine checks the client syscall/thread state and
init-owned process record, moves the client to waiting, dispatches only the
admitted init generation, reactivates its checked page table, restores its saved
user context, and performs `sysretq`, advancing ownership through byte 14,907.
The following init reply-publication completion validates the exact returning
syscall/thread state and retained 116-byte reply record, clears the channel
publication state, dispatches only the admitted init generation, restores its
checked saved context, and returns zero through `sysretq`, advancing ownership
through byte 15,243.
The client reply-delivery transaction then checks the complementary
syscall/thread state and init-owned reply record, dispatches only the admitted
client generation, restores its checked saved context, and returns the exact
116-byte result through `sysretq`, advancing ownership through byte 15,574.
The following directory-request delivery checks the client's directory-call
state and exact 37-byte queued request, dispatches only the admitted isolated
directory-provider generation, restores its checked saved context, and returns
the exact request length through `sysretq`, advancing ownership through byte
15,905.
The directory provider's reply-publication transaction then checks its exact
3,096-byte reply state, clears the channel publication state, dispatches only
the admitted provider generation, restores its checked saved context, and
returns zero through `sysretq`, advancing ownership through byte 16,241.
The complementary delivery then validates that reply, dispatches only the
admitted client generation, restores its checked context, and returns the exact
3,096-byte result through `sysretq`, advancing ownership through byte 16,572.
The first-client completion transaction then validates the exiting process,
dormant compatibility arena, both endpoint/channel generations, mappings, and
retained message geometry before removing both endpoint PTE aliases, scrubbing
every transient IPC field, and returning both endpoints to a closed state. This
advances ownership through byte 17,923 without yet claiming memory reclamation
or generation-2 reconstruction.

The following reclamation preflight activates the selected client's address
space and revalidates both closed endpoint/channel records, the empty channel
backing, dormant compatibility arena, retained program/store/directory
descriptors, their exact hashes and mappings, and selected exiting-client state.
It advances checked ownership through byte 19,525 while keeping memory release
and all object reuse outside the admitted transaction.

The admitted recycle transaction then releases exactly generation 1's 122-page
object, checks restored allocator cursor/free state, requests generation 2 under
its distinct reference, repeats the alignment and identity-window checks, and
requires the same physical root. Ownership reaches byte 19,741; no generation-2
record or mapping is published until the following reconstruction transactions.

Generation-2 reconstruction begins by clearing the entire retained client
record and rebuilding its generation, fixed identity, resource bounds, pinned
image digests, private extent addresses, and retained service endpoint bindings.
The record remains private through byte 20,240; paging and ready publication
remain later transactions.

The following generation-2 paging region intentionally reuses the exact
generation-1 constructor: retained kernel tables, null hole, private lower
hierarchy, read/execute code, writable/NX stack/data/response pages, and two
guard entries are byte-identical. This advances ownership through byte 24,988
without introducing a second paging policy.

The next 76 bytes likewise reuse the exact admitted-interpreter copy and native
context seed, including its symbol-1 relocation. Generation-2 private image and
initial execution-context reconstruction therefore reach byte 25,064 before
endpoint rebinding or readiness publication.

Endpoint rebinding then validates both complete closed generation-1 records,
including provider/channel identities, generations, rights, close evidence, and
zero transient state, before changing either client reference. Ownership reaches
byte 25,512 with both endpoints bound to generation 2 but not yet republished.

The following checked re-entry transaction validates the recycled memory object
and returned client generation, binds kernel GS and the exact resume context,
finishes the resource-state transition, restores user registers, and executes
the first generation-2 `sysretq`. Source ownership now reaches byte 25,953. This
is deterministic machine-code evidence, not yet live guest execution evidence;
the resumed handler and subsequent lifecycle remain outside the owned prefix.

The resumed handler then validates processor/GS state, completion counters,
both retained resource descriptors, generation-bounded references, page-table
entries, backing-object aliases, and per-resource context records. This advances
source ownership through byte 26,964, immediately before the next `swapgs` and
dispatcher call.

The next 174-byte user transfer derives the existing checked constructor with
only its selected-client generation changed from 1 to 2. Dispatcher exit,
external page-table activation, GS/continuation publication, private context
restoration, and `sysretq` remain the same contract. Source ownership reaches
byte 27,138 at the second generation-2 user entry.

The following 331-byte client return likewise derives the existing checked
return-to-init constructor with only the returning generation changed from 1 to
2. It validates result 55, crosses the dispatcher and external page-table seam,
publishes init's GS/continuation state, and executes the next `sysretq`. Source
ownership reaches byte 27,469.

Init's following 336-byte reply publication derives the checked generation-one
constructor with operation 7 and retained client generation 2. Channel clearing,
dispatcher/page-table activation, GS/continuation publication, zero completion,
and the client `sysretq` remain shared. Source ownership reaches byte 27,805.

The first restart policies are deliberately small:

- `Never`;
- `Onˉfault` with a maximum attempt count, monotonic time window, deterministic or bounded backoff, and a stable exhausted result; and
- `Always` only for a later measured long-running provider that distinguishes planned stop from unexpected exit.

There is no infinite restart loop and no restart without reserved recovery resources. Boot-critical failure selects one explicit policy: degrade, enter a bounded recovery environment, shut down, or reboot. It does not silently grant init more authority.

Restart creates a new process and provider generation. Clients observe peer loss. Read-only idempotent operations may be retried only under their interface contract; an indeterminate mutating request is never replayed automatically.

## Agent processes and cognitive operations

The foreground and digital-subconscious roles in the proposed
[agent runtime architecture](Agent-Runtime-And-Digital-Subconscious.md) do not
select a kernel process type or grant process authority. The first hosted profile
may execute both logical planes sequentially inside one ordinary process while a
deterministic owner admits bounded cognitive-operation envelopes and records their
results. This is sufficient to qualify role separation without requiring hidden
background threads or a second principal.

Later placement may isolate model providers, retrieval workers, or action executors
in separately supervised processes or services. Each receives its own resource
domain, explicit input and output limits, deadline, cancellation path, provider
generation, and minimum capability set. The durable agent identity and run ledger
remain outside any one worker lifetime.

The later persistent agent self likewise remains outside any coordinator,
scheduler, model, retrieval, simulation, or action process. A process generation
may host one admitted wake or several bounded cognitive cycles, but it never
becomes the owner of values, commitments, autobiographical continuity,
intentions, beliefs, or memories merely because it has them in working memory.

A restart creates a new worker generation and resumes only from an admitted
checkpoint plus ordered durable events. Pending read-only or purely cognitive work
may be resubmitted only under an exact idempotency contract. An action with
indeterminate completion is reconciled by its owning capability protocol and is
never replayed merely because an agent worker restarted.

### Event-driven subconscious wakes

The service manager and kernel provide only bounded mechanism: monotonic timers,
event delivery, process admission, resource accounting, cancellation, peer-loss,
and teardown. A user-space agent scheduler owns intention eligibility, source
subscriptions, wake coalescing, fairness, salience, cognitive-cycle limits, and
the decision to return dormant.

One wake plan binds the persistent-self and intention generations, trigger
identity, earliest/latest time, required capabilities, process/provider profile,
maximum cycles/calls/work/bytes/cost, and terminal dormancy outcomes. Admission
creates one executor generation. Duplicate, stale, cancelled, expired,
cross-scope, or over-rate triggers cannot create another generation or model
charge.

A schedule is not action authority. Retrieved content cannot register a wake by
containing instructions, and a timer firing does not grant the scheduler access
to a source or tool. Consequential action still uses the ordinary envelope,
receipt, lease, execution fence, observed outcome, and verification chain.

## First measured slices

1. Retain Probe 40's qualified independently lived memory-object baseline and its current native flat-domain accounting gate around the existing processes and objects.
2. The current candidate binds both sequential generations of one known verified child with one input resource, three explicit streams, one reduced capability, and one observer. It rejects unsupported versions, unauthorized callers, stale executable publications, and malformed plans before visibility, derives the child identity, admits distinct bounded machine layouts with W^X and capability-table checks, and proves failed-construction rollback. The first architecture-neutral request decoder now checks the exact application profile independently. Provider launch transaction 1 separately composes exact filesystem/network request admission with 64/96-page isolated domains, immutable image geometry, construction, readiness publication, stale rejection, active-work drain, and zero-charge teardown. Next, copy the application value from user memory and bind both transaction families to dynamic executable, memory-object, page-table, endpoint, and process allocation.
3. Exercise normal completion, verifier rejection, capability refusal, trap, process fault, cancellation, forced stop, provider loss, and launcher death. Each leaves zero charges and stale generations unusable.
4. Move one non-critical existing provider under the service manager with `Never` restart and explicit availability publication.
5. Add bounded `Onˉfault` restart, generation-safe rebinding, and exhausted-restart policy without replaying a mutation.
6. Use the same transaction to launch the terminal shell and first isolated serial service.

## Deliberately open details

The architecture does not yet freeze the broader semantic-plan serialization,
object indices, service-registry encoding, exact restart limits, backoff formula,
public process IDs, environment keys, diagnostic storage, or syscall numbers.
`WVSR 1` freezes only the first kernel-facing fixed application request. The
architecture fixes clean spawn, two-level plans, atomic publication, explicit
transfer, separate observation and control rights, generation-visible restart,
bounded policy, and complete domain teardown.
