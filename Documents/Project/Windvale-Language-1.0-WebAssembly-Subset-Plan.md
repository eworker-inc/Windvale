# Windvale Language 1.0 WebAssembly subset plan

## Status

- Date: 2026-08-20
- Status: Proposal; no implementation or permanent-target claim
- Source contract: [Windvale Language 1.0 design](Windvale-Language-1.0-Design.md)
- Migration dependency: [Windvale Seed to Language 1.0 migration plan](Windvale-Language-1.0-Migration.md)
- Existing target contract: [Windvale experimental WebAssembly target](../../Specifications/Windvale-WebAssembly.md)
- Product direction: [Decision 0182](../Decisions/0182-Browser-And-WebAssembly-Product-Direction.md)
- Browser host contract: [Windvale browser playground](../../Specifications/Browser-Playground.md)
- Library direction: [Windvale Libraries 1.0](../../Specifications/Windvale-Libraries-1.0.md)
- Hosted library plan: [Windvale Backend Libraries 1.0](../../Specifications/Windvale-Backend-Libraries-1.0.md)
- CLI and shell integration: [Windvale WebAssembly shell integration plan](Windvale-WebAssembly-Shell-Integration-Plan.md)

This document proposes a bounded direct-compilation target for a useful subset
of Language 1.0 `.wv` source. It does not change the frozen source language,
authorize implementation, accept WebAssembly as a permanent host or compiler
target, or claim that every Language 1.0 program can run in a browser.

## Practical outcome

The first completed product slice should let a Windows or Linux Windvale build
tool accept an explicitly targeted Language 1.0 project and publish a
deterministic `.wasm` module when every reachable operation belongs to the
declared subset.

```text
Language 1.0 .wv source
    -> ordinary source analysis
    -> typed WIR
    -> WebAssembly target admission
    -> deterministic WebAssembly encoding
    -> independently validated .wasm
```

Unsupported source remains valid Language 1.0. The WebAssembly target rejects it
with an exact target-support diagnostic and publishes no partial `.wasm` file.
The compiler must not reinterpret source semantics to make a program fit the
target.

The initial milestone is intentionally smaller than complete Language 1.0. It
exists to provide an honest, useful `.wv`-to-`.wasm` path that can grow through
measured consumers rather than waiting for an all-features backend.

## Relationship to the current browser path

The normal playground already runs a Windvale compiler and interpreter packaged
as WebAssembly:

```text
.wv source -> compiler Wasm -> canonical WVB -> verifier/interpreter Wasm
```

That is a browser-hosted WVB execution path. This proposal adds a different
product:

```text
.wv source -> shared typed WIR -> application .wasm
```

The new path does not replace the browser interpreter. Canonical WVB remains the
portable distribution identity and semantic comparison oracle. Direct Wasm is a
target artifact produced from the same analyzed program, not a second language
or an alternate definition of Windvale behavior.

## Browser host architecture

The browser host is an adapter for Windvale capabilities. JavaScript owns the
browser APIs, permission prompts, worker lifecycle, and DOM presentation, but it
does not own Windvale capability semantics. The generated Wasm guest cannot call
`fetch`, OPFS, the DOM, timers, Web Crypto, or another browser API directly.

The first capable browser host should use this topology:

```text
Main page
  - user intent and permission UI
  - capability-grant construction
  - file/directory pickers that require user activation
  - DOM rendering with text-only output boundaries
  - worker deadline and hard termination
            |
            | immutable bounded messages
            v
Dedicated run worker
  - artifact identity and import/export validation
  - fixed-memory Wasm instance
  - Windvale request/result ABI decoder
  - exact capability broker and per-run grant table
  - bounded output, storage, HTTP, clock, entropy, and event providers
            |
            v
Generated application Wasm
  - no browser API access
  - no ambient authority
  - one active guest transition at a time
```

The main page must never pass a JavaScript API object, DOM node, native-looking
path, credential, or unrestricted URL into Wasm memory. A capability grant is a
small immutable launch record naming the exact interface major version, provider
instance, rights, origin or namespace restriction, generation, and limits.

The run worker may implement providers in focused JavaScript modules, but every
module is inert until the broker binds an authorized instance. Adding a module
to the browser bundle is not a grant. Unknown capability identities, versions,
operations, fields, or completion classes fail closed.

### JavaScript component responsibilities

| Component | Required responsibility | Must not do |
| --- | --- | --- |
| Page controller | Collect explicit user choices, construct grants, start one dedicated run worker, enforce the wall-clock deadline, and terminate it on completion or failure. | Execute guest code on the UI thread or infer grants from imported libraries. |
| Artifact loader | Fetch manifest-owned Wasm, verify byte length and SHA-256, reject unexpected imports/exports or memory shape, and compile only the admitted bytes. | Fall back to an unpinned network artifact or execute before identity checks. |
| Guest ABI adapter | Validate every offset, length, tag, count, generation, and reserved field before copying request or result bytes. | Retain a guest pointer across a guest transition or trust a guest-provided typed-array view. |
| Capability broker | Match each operation to one exact per-run grant and provider, enforce limits, assign operation identities, normalize completion, and reject stale or duplicate results. | Expose `globalThis`, `fetch`, `navigator`, storage handles, or arbitrary JavaScript functions to the guest. |
| Provider adapter | Translate one browser API into one Windvale semantic interface and contain JavaScript exceptions at that boundary. | Leak browser exception strings, implementation objects, native paths, credentials, or browser-specific semantics into portable code. |
| UI renderer | Present bounded immutable output and events using text-safe DOM operations. | Interpret guest output as HTML, script, CSS, a URL, or an event-handler name. |
| Test provider | Supply deterministic virtual time, entropy, storage, HTTP, cancellation, and fault plans under test-only identities. | Satisfy a production capability through configuration or aliasing. |

### Capability declaration, approval, binding, and use

The browser retains the ordinary four-step authority model:

1. the compiled module declares an exact capability requirement;
2. the page shows the relevant scope and asks for any necessary user decision;
3. the launcher constructs a rights-limited provider binding for this run; and
4. the broker admits each operation only against that binding.

A declaration is not permission. Browser permission is also not sufficient by
itself: a granted browser API must still be narrowed to the Windvale capability
instance. Revocation, provider loss, page navigation, worker termination, and a
stale generation remain explicit failures.

Persistent approvals require a separate product decision. The first profile
should construct grants per run and should not remember file, network, clipboard,
or other authority merely because the browser previously allowed it.

## Asynchronous request and resume ABI

Console buffering can complete without leaving the guest, but files, network,
timers, permissions, and UI events are asynchronous browser operations. The
host must not model them as synchronous Wasm imports and must not reenter an
active guest from a JavaScript callback.

Language 1.0 suspension points should lower to an explicit state machine. One
guest transition returns exactly one of:

- `Completed`, with the bounded application result and final output evidence;
- `Trapped`, with one Windvale status and bounded diagnostic evidence; or
- `Suspended`, with one or more bounded provider requests owned by the current
  task scope.

The initial asynchronous profile should allow one outstanding provider request.
Later structured-concurrency profiles may raise that to a small fixed maximum
and must define creation order, completion order, cancellation, teardown, and
retained-byte ceilings.

For one suspended operation:

1. the guest writes a versioned request envelope into its outbound window;
2. the guest returns to JavaScript and cannot execute again until resumed;
3. the broker copies the complete envelope out of Wasm memory and validates it;
4. the broker checks the capability, provider, operation identity, generation,
   rights, limits, and request geometry;
5. the provider performs the browser operation asynchronously;
6. the provider converts success, rejection, cancellation, partial progress,
   indeterminate completion, or provider loss into a versioned Windvale result;
7. the broker copies the complete result into the inbound window; and
8. JavaScript calls the resume export once with the matching operation identity
   and generation.

JavaScript event listeners and Promise continuations may enqueue provider
completions, but they never call into Wasm while another guest transition is
active. The broker drains its bounded queue only after control has returned from
the guest. Duplicate, late, mismatched, or post-cancellation completions are
discarded and recorded as bounded host evidence rather than delivered to a new
operation.

The guest memory remains fixed and non-growing in the first capable profile.
Request bytes are copied before any `await`, and result bytes are copied into a
disjoint region before resume. All offset-plus-length calculations use checked
arithmetic. No `SharedArrayBuffer`, `Atomics`, stack-switching extension, or
engine-specific suspension feature is required by the first contract.

## Activation boundary

Implementation should begin only from a named Language 1.0 compiler state whose
required source, type, WIR, diagnostic, and verifier contracts are stable enough
for the selected subset. Completing all Language 1.0 migration slices is a
sufficient activation point, but the first WebAssembly target does not need to
pretend that browser-inapplicable System/FFI operations are supported.

Before implementation, a decision must pin:

- the exact compiler and typed-WIR identities consumed by the target;
- the exact experimental target identifier and WebAssembly feature set;
- the admitted value, operation, control-flow, call, and entry shapes;
- the value, call, memory, trap, and result ABI;
- deterministic work, memory, module-size, function, local, stack, and
  diagnostic ceilings; and
- the reference fixtures and differential oracle.

Until that decision, this page is a planning document rather than a format or
compatibility promise.

## First admitted source subset

The first subset should be useful enough for small algorithms and command-free
portable tools while keeping its execution boundary import-free.

### Module and entry boundary

- Language edition `1` with an exact admitted source descriptor and profile
  lock;
- Core profile only;
- one closed, statically known source graph producing one application module;
- no required or optional capabilities;
- no dynamic source, package, module, or function loading;
- one exported `Main() -> i32` entry; and
- no other export required by the initial host ABI.

Multiple source files may be admitted when the ordinary project/source-graph
compiler has already reduced them to one closed typed program. The WebAssembly
backend must not search the host filesystem, resolve packages, or invent module
composition rules.

### Initial value and operation boundary

The first implementation milestone should admit only the exact typed-WIR forms
needed by retained fixtures for:

- `i32`, `u32`, `u8`, `bool`, and non-runtime `unit` values;
- immutable constants and initialized locals;
- checked integer arithmetic and explicitly admitted comparisons;
- Boolean operations;
- value-producing `if` and ordinary statement conditionals;
- bounded `while` control reconstructed as structured WebAssembly control;
- direct calls with statically known signatures and bounded call depth;
- return, local assignment, and discarded values; and
- generic source calls only after ordinary Language 1.0 analysis has produced a
  fully concrete monomorphic WIR body using the admitted values and operations.

The normative implementation decision must replace this descriptive list with
an exact typed-WIR operation table. Admission is based on verified typed WIR,
not on a fragile source-syntax allowlist.

### Explicit first-slice rejections

The first target rejects, before Wasm publication:

- Hosted or System profile modules;
- capabilities, resources, unsafe blocks, foreign calls, native pointers, and
  target-specific ABI values;
- floating-point, text, bytes, records, enums, variants, collections, slices,
  arenas, and builders until their exact Wasm representations are accepted;
- function values, indirect calls, captures, and closures;
- recursion;
- task construction, spawn, await, cancellation, and scheduler operations;
- unresolved or runtime generic values;
- an unsupported entry signature, import, export, WIR operation, value shape,
  control join, call edge, or resource bound; and
- any request whose source, WIR, output, or diagnostic geometry exceeds the
  selected target ceiling.

These are target-support failures, not claims that the rejected source is
invalid Language 1.0.

## Compiler boundary

The direct backend consumes immutable typed WIR produced by the ordinary
Language 1.0 compiler. It does not parse `.wv`, bind names, infer generic
arguments, perform ownership analysis, or define evaluation order.

Target admission proceeds in this order:

1. require successful ordinary Language 1.0 analysis;
2. select the closed reachable graph from the declared entry;
3. validate every reachable signature, value representation, operation,
   control edge, and call edge against the pinned target table;
4. calculate all output and work bounds with checked arithmetic;
5. produce an immutable admitted-target model;
6. encode the complete WebAssembly candidate in memory or bounded temporary
   storage;
7. validate the complete candidate with an independent decoder or engine; and
8. publish atomically only after every preceding step succeeds.

Imports must remain inert. Importing the backend cannot register a target or
mutate compiler-global state. The build driver selects the backend explicitly
from target metadata.

## Initial WebAssembly execution contract

The first generated module should use WebAssembly binary version 1 and the
smallest pinned instruction subset needed by the admitted WIR table. It should:

- contain no imports;
- contain no linear memory unless the admitted value set proves it necessary;
- expose one versioned Windvale execution wrapper rather than treating a raw
  WebAssembly return value as the complete runtime contract;
- preserve Windvale checked arithmetic and trap identities rather than inheriting
  WebAssembly wrapping or engine-trap behavior;
- meter Windvale work deterministically when loops or calls are admitted;
- enforce bounded call depth without depending on the browser or engine stack;
- produce deterministic bytes for identical compiler, target, source, lock, and
  option identities; and
- expose enough status, result, and instruction evidence for comparison with
  canonical WVB execution.

The implementation should reuse the established experimental execution ABI when
its contract fits. Any incompatible value, call, memory, or result requirement
needs a separately versioned ABI rather than an implicit extension.

## Browser capability plan

The browser target should expose semantic capabilities that can be implemented
honestly with browser APIs. It must not present every browser mechanism as a
portable filesystem, socket, process, or operating-system service.

Where Windvale Libraries 1.0 defines an applicable Platform capability, the
browser adapter implements an explicitly selected subset of that interface. It
does not create a similar browser-only interface with different completion,
authority, path, retry, durability, or failure semantics. A browser mechanism
that cannot meet the shared contract receives a narrower separately named
profile or remains unsupported.

| Capability family | Browser-side mechanism | Initial standing |
| --- | --- | --- |
| Standard and diagnostic output | Bounded worker messages rendered with DOM `textContent` | First hosted capability |
| Immutable launch arguments | Explicit page fields copied into the launch envelope | Early optional input profile |
| Volatile files/blobs | Per-run memory provider | Early deterministic test and application profile |
| Origin-private storage | Origin-private file system behind a bound namespace | Later storage profile; no native-path or durability claim |
| User-selected file import/export | Page-owned picker or upload/download flow under direct user action | Optional browser extension, not ambient filesystem access |
| HTTP client | Origin-restricted Fetch provider | First network profile |
| WebSocket messages | Exact endpoint/subprotocol allowlist and bounded message queues | Later optional network profile |
| WebTransport | Separately versioned secure transport profile | Deferred until a real consumer and cross-browser evidence exist |
| Raw TCP/UDP, DNS, listen, multicast | No ordinary browser mapping | Unsupported |
| Monotonic time and timers | Worker performance clock plus scheduled completion | Later explicit clock/timer profile |
| Civil time | Browser wall-clock observation with evidence and failure | Separate from monotonic time; optional |
| Secure entropy | Web Crypto secure random bytes | Later explicit entropy profile |
| UI input/events | Main-page events converted to bounded immutable event records | Later event profile |
| Drawing and retained UI | Semantic render model or bounded drawing commands | Deferred; no direct DOM access |
| Clipboard, notifications, media, sensors | Page-owned permission and user-activation adapters | Individually deferred capabilities |
| Cryptographic keys | Non-exportable Web Crypto provider handles | Deferred protected-key profile |
| Environment variables, process launch, native libraries, devices | No browser mapping | Unsupported |

### Console and diagnostics

The first browser capability remains standard output. The guest never calls
`console.log` and never receives the browser console object.

The worker maintains separate bounded byte channels for:

- standard output;
- diagnostic output; and
- host evidence that is not guest-visible.

`console.write` appends exactly the supplied strict UTF-8 text.
`console.write_line` appends the text and one LF byte. The configured write
contract determines whether acceptance is all-or-nothing or may report exact
partial progress; JavaScript must not silently truncate. Acceptance means that
the browser provider copied the bytes into its bounded local channel. It does
not prove that pixels were rendered or that a user observed them.

The worker sends immutable bounded output chunks to the page. The page creates
text nodes or assigns `textContent`; it never uses `innerHTML`. ANSI escapes,
URLs, Markdown, terminal control, and carriage returns are ordinary untrusted
text unless a separately specified terminal renderer admits them. UI rendering
must not alter the retained exact output bytes.

Backpressure is explicit. Once the run's byte or message limit is reached, the
provider reports the specified capacity failure. The page must not discard old
chunks to make a current write appear successful. Closing or terminating a run
closes the output generation and rejects late worker messages.

### Browser filesystem and storage

Browser storage is a provider for selected Windvale filesystem or blob
semantics; it is not a native path namespace and is not Windvale OS storage.

The baseline browser storage provider should use one origin-private directory
owned by the application. JavaScript obtains that root through the browser
storage API, creates or opens one versioned child namespace, and binds only an
opaque provider identity and generation to the guest. Wasm never receives a
`FileSystemHandle` or learns the physical storage location.

The provider must:

- admit only normalized Windvale relative path segments under the bound root;
- reject empty, current, parent, native-drive, UNC, URL, separator-confused,
  overlong, or otherwise invalid paths before a browser API call;
- bind enumeration, snapshot read, create, mutation, removal, and publication as
  separate rights rather than one broad read/write Boolean;
- enforce file count, directory depth, name bytes, individual file bytes,
  aggregate retained bytes, open resource, operation, and elapsed-time limits;
- attach an identity and generation to every open file, directory, writer, and
  outstanding operation;
- copy browser results into bounded immutable or exclusively borrowed guest
  buffers rather than retaining guest memory views;
- distinguish rejection, exact partial progress, completion, indeterminate
  mutation completion, stale handle, quota exhaustion, revocation, and provider
  loss; and
- close writers and release handles during cancellation and worker teardown.

Origin-private storage does not justify a native-path, user-visible-file,
cross-origin, backup, atomic-replacement, or power-loss durability claim. The
browser provider advertises only the exact completion and persistence level it
can demonstrate. A session-memory fallback must use a distinct volatile
provider identity and must never silently satisfy a request that requires
persistence or durability.

User-selected host files are a separate extension. A page action may ask the
user to select files or a directory, then bind a rights-limited snapshot or
handle to one run. Selection must occur from a user gesture when the browser
requires it. Permission denial, later revocation, moved content, and unavailable
APIs are ordinary provider outcomes. The guest still sees Windvale relative
names and opaque instances, not host paths or browser handles.

The most portable first import/export experience may use an explicit file input
to acquire immutable bytes and a generated download to publish new bytes.
Persistent selected-directory access should remain an optional browser profile
until Chromium, Firefox, WebKit, and real Safari evidence supports an honest
claim.

### HTTP and networking

The first browser network capability should be an origin-bound HTTPS client over
Fetch, not a fake TCP stream. Browser Fetch owns DNS, connection reuse, proxy,
TLS, certificate, redirect, cache, credential, and CORS behavior that ordinary
page JavaScript cannot fully replace or observe.

An initial grant should bind:

- an exact HTTPS origin or finite origin allowlist;
- permitted methods, initially `GET` and optionally `HEAD`;
- permitted request and exposed response headers;
- credential mode, initially omitted;
- redirect policy, initially rejection rather than silent origin expansion;
- request-body and response-body byte limits;
- maximum in-flight requests and retained response bytes;
- monotonic deadline and cancellation generation; and
- a policy for cache use, initially no implicit application-visible cache.

The JavaScript provider constructs the URL from an admitted origin plus strict
path and query values. It must not accept a complete arbitrary URL from guest
bytes when the grant is narrower. It creates an `AbortController` for deadline,
cancellation, and teardown; reads the response incrementally under the body
limit; copies admitted headers and body bytes; and converts browser rejection,
CORS denial, abort, timeout, oversize data, malformed text, and provider loss to
stable Windvale outcomes. Raw JavaScript exception messages and response objects
do not cross the ABI.

CORS remains browser-enforced. A Windvale grant cannot override a server's CORS
policy, browser mixed-content rules, content security policy, or credential
restrictions. A CORS failure is not reported as a verified HTTP status because
the browser did not expose such a response.

The provider never automatically retries. For a mutation, cancellation or a
network error after dispatch may leave completion indeterminate. The adapter
must report that class rather than converting it to known-zero progress. A later
retry requires an application-level idempotency contract.

WebSocket is a separate message capability, not `network.stream_v1` and not raw
TCP. Its grant binds exact `wss` endpoints, subprotocols, maximum message bytes,
send and receive queue limits, lifetime, cancellation, and close behavior.
JavaScript event callbacks enqueue immutable open/message/error/close records;
the guest observes them only through the bounded wait/event interface. Messages
are copied before delivery, and queue saturation follows an explicit close or
failure policy.

WebTransport, server-sent events, and other long-lived transports require
separate lifecycle and cross-browser qualification. Raw sockets, listening,
arbitrary DNS queries, multicast, and local-network discovery remain unsupported
in the ordinary browser target.

### Time, timers, and cancellation

Monotonic time, civil time, and timers are separate capabilities.

The monotonic provider selects and publishes an exact unit and converts the
worker's monotonic observations with checked arithmetic. Its values are valid
only for the bound provider generation and must not be persisted as civil time.
The civil-time provider uses a separate identity and returns evidence whose
precision, uncertainty, and trust class are explicit; ordinary browser wall time
is not trusted certificate or legal-time evidence merely because JavaScript can
read it.

A timer operation records the requested deadline and cancellation generation,
uses a browser timer only as a wakeup mechanism, then rechecks monotonic time
before completing. Early, late, throttled, suspended-tab, page-lifecycle, and
worker-termination behavior must map to the Windvale timer contract rather than
being hidden. Timer callbacks enqueue completions and never reenter Wasm.

Every provider operation accepts the task scope's cancellation evidence where
its Windvale contract requires it. Cancellation requests abort or close the
underlying browser operation when possible, but cancellation completion remains
distinct from proof that a network peer or storage system observed nothing.

### Secure entropy and protected keys

Secure entropy uses the browser cryptography API, never `Math.random`. The
provider enforces the Windvale request maximum, fills a host-owned buffer,
copies exactly the admitted bytes into the guest result, and clears temporary
copies when practical. API absence, insecure context, request rejection, or
provider teardown is an explicit failure. Deterministic test entropy has a
different identity and cannot satisfy a production secure-entropy requirement.

Future protected-key operations should keep non-exportable browser key handles
inside JavaScript. The guest may receive only opaque provider identities,
algorithm/profile evidence, public material when allowed, and bounded operation
results. Private key bytes, browser credentials, cookies, bearer tokens, and
trust-store administration must not enter Wasm memory.

### UI, input, clipboard, and other browser services

The main page owns the DOM. A Windvale application may later consume semantic
input events and publish a retained view model or bounded drawing commands, but
it never receives a DOM node or invokes a selector, script, event handler, CSS
rule, or arbitrary browser method.

The first event profile should define immutable typed events for a deliberately
small control surface. It freezes source identity, event kind, sequence,
coalescing, queue limit, text normalization, focus generation, cancellation,
and closed-source behavior. Browser listeners translate events into that format
and append them to a bounded queue. The guest requests events through the same
suspend/resume ABI; JavaScript does not invoke a guest callback.

Clipboard, notifications, fullscreen, downloads, media capture, location,
sensors, graphics acceleration, and device access each require a separate
capability, user-intent rule, limits, failure model, privacy review, teardown,
and browser evidence. None is inherited from the existence of a page or from a
general `ui` grant.

### Browser lifecycle and cleanup

Each run owns one lifecycle scope containing:

- its dedicated worker and Wasm instance;
- capability grants and provider generations;
- pending operations, abort controllers, timers, readers, writers, streams, and
  event listeners;
- retained request, response, output, event, and diagnostic bytes; and
- the final immutable evidence record.

Normal completion, guest trap, target failure, cancellation, page navigation,
provider failure, and wall-clock timeout all enter one bounded teardown path.
Teardown stops new operations, advances generations, aborts or closes pending
browser work, removes listeners, closes resources, discards late completions,
publishes bounded final evidence when possible, and terminates the worker. A
provider must not leave a Promise continuation, timer, reader, stream, listener,
or retained guest buffer reachable after its scope ends.

## Diagnostics and inspection

A rejected target must report at least:

- source edition and profile;
- selected target identity;
- compiler and typed-WIR identity;
- the first unsupported source location when available;
- the unsupported typed-WIR value, operation, signature, or graph property;
- the relevant actual and maximum counts; and
- confirmation that no output artifact was published.

A successful build should make available a bounded inspection report containing:

- source/project and target identities;
- reachable and emitted function counts;
- admitted typed-WIR operation counts;
- WebAssembly byte length and SHA-256 identity;
- maximum locals, value stack, call depth, and linear memory pages;
- imported and exported identities; and
- the execution ABI and required host profile.

Diagnostic and inspection text is not part of the executable ABI. It remains
bounded and must safely quote untrusted names.

## Ordered delivery slices

### WebAssembly subset slice 0: freeze the target table

- Select the exact typed-WIR producer identity.
- Publish the operation, representation, ABI, and limit tables.
- Convert retained constant, arithmetic, conditional, loop, and direct-call
  programs into Language 1.0 success fixtures.
- Add unsupported-profile, unsupported-operation, malformed-evidence,
  oversized-graph, and deterministic-failure fixtures.
- Record existing WVB interpreter results as the correctness oracle.

### WebAssembly subset slice 1: scalar `.wv` to `.wasm`

- Connect the explicit build target to typed WIR.
- Admit the first scalar/control/direct-call subset.
- Emit deterministic import-free Wasm.
- Independently validate the emitted module.
- Execute it in a pinned standalone engine and compare status, result, and
  Windvale instruction evidence with canonical WVB execution.
- Prove that every rejection publishes no output.

Completion of this slice provides the requested first useful ability to compile
some Language 1.0 `.wv` programs directly to WebAssembly.

### WebAssembly subset slice 2: nominal and immutable memory values

- Add exact record, enum, variant, text, bytes, and static-data layouts only as
  required by named consumers.
- Define linear-memory ownership, descriptor validation, allocation, reclamation,
  and output-copy rules before admitting each value family.
- Add value-producing `match` and typed Result/Option behavior through ordinary
  monomorphic lowering.

### WebAssembly subset slice 3: bounded collections and ownership

- Add arrays, immutable sequences, slices, builders, and other collection
  families independently under explicit capacity ceilings.
- Preserve bounds checks, borrow non-escape, move/freeze behavior, cleanup, and
  deterministic resource failure.
- Keep a simple WVB differential oracle for every optimized representation.

### WebAssembly subset slice 4: browser broker and console

- Implement the dedicated-worker topology, per-run grant table, checked ABI
  decoder, operation identities/generations, and one-transition-at-a-time rule.
- Bind the already specified `console.write`, `console.write_line`, and
  diagnostic output contracts independently.
- Preserve exact UTF-8/LF output bytes, bounded channels, all-or-nothing or exact
  partial-progress behavior, and text-only DOM rendering.
- Prove authorization denial, capacity exhaustion, stale generation, late worker
  message, output injection, teardown, and worker timeout behavior.
- Do not grant filesystem, network, storage, clock, DOM, or other ambient browser
  authority as a side effect of admitting console output.

### WebAssembly subset slice 5: asynchronous host operations and storage

- Lower the first Language 1.0 suspension point to the explicit
  request/result/resume state machine.
- Admit one outstanding operation, bounded queues, cancellation, provider loss,
  and hard worker termination without reentrant guest callbacks.
- Implement a deterministic memory filesystem provider for semantic and fault
  tests.
- Add one versioned origin-private namespace with separately bound snapshot-read,
  create, mutation, enumeration, removal, and publication rights.
- Prove strict relative-path admission, quota failure, unavailable persistence,
  volatile fallback separation, stale handles, interruption at every mutation
  boundary, and complete teardown.
- Keep selected host-file or directory access optional and page-mediated until
  its permission and cross-browser profile is accepted.

### WebAssembly subset slice 6: HTTP client

- Bind one exact HTTPS origin and a bounded `GET`/`HEAD` Fetch provider.
- Use omitted credentials, rejected redirects, explicit admitted headers,
  bounded response bytes, one in-flight request, and no automatic retry.
- Map CORS denial, abort, timeout, oversize response, malformed provider data,
  page lifecycle, and worker termination to stable Windvale outcomes.
- Add mutation methods only after known-zero versus indeterminate completion and
  application idempotency are covered.
- Keep WebSocket, WebTransport, raw streams, datagrams, listeners, and local-
  network discovery outside this slice.

### WebAssembly subset slice 7: time, entropy, and event stream

- Add separate monotonic-clock, timer, optional civil-time, and secure-entropy
  grants with exact units, limits, provider generations, and failure classes.
- Require Web Crypto for production entropy and keep deterministic test entropy
  structurally unable to satisfy the production identity.
- Implement one bounded immutable page-event queue over the shared wait/event
  direction, without guest callbacks or DOM objects.
- Add WebSocket messages only if the selected application needs them and after
  its endpoint, subprotocol, queue, close, and cancellation contracts pass.
- Keep clipboard, notifications, media, location, sensors, graphics, and devices
  denied until each receives an independent capability contract.

### WebAssembly subset slice 8: browser and permanent-target evidence

- Run the exact portable fixtures across Windows, Linux, the WVB runtime, the
  standalone Wasm engine, Chromium, Firefox, and WebKit automation.
- Require real Safari evidence before making a Safari support claim.
- Compare deterministic `.wasm` bytes, statuses, results, traps, output, and
  semantic counters.
- Measure compiler time, backend time, engine compile time, cold start,
  execution time, artifact size, and peak memory on named workloads.
- Exercise malformed input, exhaustion, worker termination, capability denial,
  permission denial/revocation, stale and duplicate completion, storage
  interruption, CORS/network failure, event saturation, and repeated
  construction/destruction.
- Use a separate decision to accept any permanent WebAssembly host or direct
  compiler target.

## Verification strategy

Each delivery slice uses the narrowest reliable focused owner. A broader
qualification gate runs only for a selected promotion state.

The retained differential matrix should include:

| Case | Required comparison |
| --- | --- |
| Successful scalar program | WVB and Wasm status, result, and semantic work agree. |
| Checked overflow | Both paths report the same Windvale trap; no engine trap substitutes for it. |
| Exact and one-short budget | Both paths stop at the same semantic boundary. |
| Unsupported Language 1.0 feature | Exact target rejection; no `.wasm` publication. |
| Malformed cached WIR or target evidence | Rejection before encoding or execution. |
| Repeated identical build | Byte-identical `.wasm` and inspection identities. |
| Near-limit graph | Completion stays within the published time and memory ceilings. |
| Over-limit graph | Early bounded rejection without proportional output allocation. |
| Console output containing HTML, controls, and invalid boundaries | Exact admitted UTF-8 bytes are retained and rendered only as text; invalid input is rejected. |
| Capability absent, denied, revoked, or wrong version | Failure occurs before provider use with no ambient fallback. |
| Duplicate or stale asynchronous completion | The current guest state is unchanged and bounded host evidence identifies the rejection. |
| Cancellation at each provider phase | Known-zero, exact progress, indeterminate completion, and completed outcomes remain distinct. |
| Storage path and quota pressure | Invalid paths reject before provider access; exact-boundary data succeeds; excess data fails within fixed memory. |
| Storage interruption | Reopening observes only the completion and persistence promises actually made by the provider. |
| HTTP origin, redirect, CORS, and body limits | The provider cannot expand authority, expose an unobservable response, or retain excess bytes. |
| Network mutation interrupted after dispatch | The result is indeterminate unless the application protocol proves an idempotent outcome. |
| Timer throttling and late wakeup | Completion follows the Windvale deadline contract rather than assuming browser wakeup punctuality. |
| Worker terminated with pending operations | Providers close or abort, generations advance, and no late result reaches another run. |
| Event queue saturation | The selected coalescing, rejection, or close rule is exact and retained memory remains bounded. |

WebAssembly engine validation supplements Windvale's independent decoder and
semantic checks. It does not replace them.

## Performance and memory evidence

Before and after each material extension, record:

- source bytes, module count, reachable function count, and WIR operation count;
- compiler-analysis and backend elapsed time;
- peak or working-set memory when practical;
- Wasm bytes, functions, locals, stack maximum, and memory pages;
- execution cold start and steady-state time;
- JavaScript/Wasm transition count and copied bytes by request/result family;
- pending-operation, output, event, storage, and network retained-byte maxima;
- provider latency and cancellation/teardown time at exact limits;
- deterministic instruction counts or other semantic work measure; and
- comparison with the canonical WVB runtime and the prior target slice.

The first scalar implementation should avoid linear memory if its admitted ABI
does not need it. Later memory-backed values must use checked geometry, bounded
allocation, explicit ownership, predictable reclamation, and non-growing or
explicitly capped memory.

## Completion levels

This plan deliberately separates three claims:

1. **Subset compiler implemented** — the pinned Core subset compiles from `.wv`
   to deterministic `.wasm` and passes its focused differential owner.
2. **Browser profile qualified** — the selected subset passes the named browser,
   hostile-input, resource, and deployment gates.
3. **Permanent compiler target accepted** — a later decision records a real
   application consumer, deterministic publication, semantic parity, useful
   performance, maintenance ownership, and all explicit non-support.

Completing level 1 does not imply levels 2 or 3. None of these levels implies
that System/FFI source is portable to browsers.

## Non-goals

- Replacing canonical WVB as Windvale's verified distribution contract.
- Creating a second parser, type checker, ownership checker, optimizer, or
  language implementation.
- Compiling every Language 1.0 profile or feature in the first target slice.
- Treating JavaScript, a browser engine, WebAssembly traps, DOM behavior, or host
  scheduling as Windvale semantics.
- Executing Windows, Linux, or Windvale OS FFI and privileged code in a browser.
- Adding ambient filesystem, network, storage, UI, clock, entropy, or extension
  access.
- Claiming cross-browser support from one Chromium-family observation.
- Widening established compiler, runtime, or artifact limits merely to admit a
  chosen fixture.

## Browser standards used as provider mechanisms

The browser implementation should pin the exact required subset of these
standards and record the engines used for qualification:

- [HTML Web workers](https://html.spec.whatwg.org/multipage/workers.html) for
  dedicated-worker messaging, lifecycle, and termination;
- [File System Standard](https://fs.spec.whatwg.org/) and
  [Storage Standard](https://storage.spec.whatwg.org/) for origin-private storage
  and quota/persistence mechanisms;
- [Fetch Standard](https://fetch.spec.whatwg.org/) and the
  [DOM abort APIs](https://dom.spec.whatwg.org/#aborting-ongoing-activities) for
  bounded HTTP, CORS behavior, cancellation, and teardown;
- [WebSockets Standard](https://websockets.spec.whatwg.org/) for an optional
  bounded bidirectional message profile;
- [Web Cryptography API](https://w3c.github.io/webcrypto/) for secure entropy and
  later protected-key provider operations; and
- [WebTransport](https://w3c.github.io/webtransport/) only for a later separately
  accepted transport profile.

These APIs are provider mechanisms, not normative definitions of Windvale
filesystem, network, timing, entropy, task, output, or resource semantics.

## CLI and shell integration

The direct target is also intended to serve the portable Windvale shell. The
shell remains an ordinary Windvale application over terminal, resolver, launch,
stream, directory, cancellation, and completion providers. It is not embedded
in the WebAssembly backend or permanently implemented by the browser page.

The focused [WebAssembly shell integration plan](Windvale-WebAssembly-Shell-Integration-Plan.md)
defines the transition from the current JavaScript Workbench to:

- the same canonical Shell 1 parser and later shell WVB used by Windows, Linux,
  Windvale OS, and the browser;
- digest-bound command resolution and disposable browser command workers;
- real `echo`, `file-read`, module inspection, and `wvwasm` command applications;
- a bounded binary artifact workspace for WVB and Wasm rather than treating the
  current UTF-8 editor workspace as a general filesystem;
- persistent-shell suspension over typed terminal and child-completion events;
  and
- later direct compilation of the same shell program to Wasm without changing
  shell semantics or command identities.

The shell plan owns interactive composition and command lifecycle. This plan
continues to own target admission, Wasm representation, the browser capability
broker, and permanent-target evidence.

## Open decisions before implementation

- Should the initial command publish only `.wasm`, or a manifest binding both
  canonical WVB and derived Wasm identities?
- Should the first target retain `wasm32-browser-v1-experimental`, or should an
  import-free engine-neutral subset receive a separate target identity?
- Which exact typed-WIR producer version is stable enough to pin?
- Which execution ABI version already fits the first Language 1.0 scalar subset?
- Which direct-call depth and loop instruction budgets are useful without
  weakening deterministic containment?
- Which small real program, in addition to conformance fixtures, becomes the
  first consumer?
- Does the first published artifact need browser packaging, or is a Windows/Linux
  compiler plus pinned standalone-engine execution the smaller honest product?
- Does the first capable ABI retain one outstanding suspended operation, and
  which selected consumer justifies any higher concurrency limit?
- Which exact origin-private completion and persistence claims can all selected
  browser engines prove without overstating durability?
- Is explicit upload/download sufficient for the first host-file experience, or
  does a separately qualified selected-file/directory profile have a real
  consumer?
- Which HTTPS origins, methods, headers, response limits, credential mode, and
  redirect policy form the first useful network grant?
- Which event kinds and retained view model provide the first useful UI consumer
  without giving the guest DOM authority?
- Which browser capabilities require a fresh user gesture every run, and which,
  if any, may receive an inspectable persisted approval in a later product?

These choices require an implementation decision with measured evidence. They
must not be inferred from this proposal alone.
