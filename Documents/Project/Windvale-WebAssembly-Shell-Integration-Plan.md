# Windvale WebAssembly shell integration plan

## Status

- Date: 2026-08-20
- Status: Proposal; no implementation or browser-parity claim
- Product architecture: [Windvale shell product and architecture](../Architecture/Windvale-Shell.md)
- Shell contract: [Windvale Shell 1](../../Specifications/Windvale-Shell-1.md)
- Readiness evidence: [Windvale shell implementation readiness](Windvale-Shell-Implementation-Readiness.md)
- Browser host: [Windvale browser playground](../../Specifications/Browser-Playground.md)
- WebAssembly target plan: [Windvale Language 1.0 WebAssembly subset plan](Windvale-Language-1.0-WebAssembly-Subset-Plan.md)
- Existing WVB-to-Wasm example: [`WebAssembly-Tool.wv`](../../Examples/Compiler/WebAssembly-Tool.wv)

This document proposes how the portable Windvale shell and existing WVB-to-Wasm
tool become usable through the browser-hosted WebAssembly environment without
creating a JavaScript shell dialect or an OS-specific shell fork. It does not
claim that a complete interactive Windvale shell, browser command launcher, or
Windvale OS shell is implemented.

## Practical outcome

The completed path should provide one shell source and one canonical shell WVB
that can run through qualified providers on Windows, Linux, Windvale OS, and the
browser. In the browser, a user should eventually be able to run commands such
as:

```text
windvale> echo hello
hello

windvale> file-read Input.wvb
<exact bytes sent to the selected output sink>

windvale> module-verify Input.wvb
valid wvb sha256=<identity>

windvale> wvwasm Input.wvb Output.wasm
webassembly status=Valid module-bytes=<bytes>
```

The exact spelling, output, artifact limits, and command catalog remain subject
to their accepted command and package contracts. This example shows the product
boundary rather than freezing new behavior.

## Current starting point

| Boundary | Current standing |
| --- | --- |
| Shell 1 parser | Implemented in Windvale source with paired Windows/Linux evidence and a bounded Node/Chromium WebAssembly smoke. |
| Browser Workbench | JavaScript command parser and dispatcher over a flat UTF-8 OPFS/session workspace. |
| Browser source execution | Windvale compiler Wasm produces canonical WVB; a separate verifier/interpreter Wasm executes the bounded result. |
| `echo` | Real Windvale application with paired hosted launch evidence; no browser guest-launch provider. |
| `file-read` | Real Windvale application with paired hosted exact-byte evidence; the browser `cat` command still reads in JavaScript. |
| `wvwasm` | Existing hosted Windvale application reads WVB bytes and publishes Wasm bytes; it is not registered as a Shell 1 command or browser application. |
| Interactive Windvale shell | Not implemented; terminal/session, general resolver, launch/observation, and structured completion contracts remain incomplete. |
| Windvale OS shell host | Dynamic launch, resource domain, normal terminal input/output, resolver, and session prerequisites remain incomplete. |

The existing browser Workbench is useful bootstrap evidence. It must not grow
into the permanent semantic owner of command parsing, resolution, authority,
launch, or completion.

## One shell, separate host providers

The shared Windvale shell owns:

- the Shell 1 parser, aliases, and exact diagnostics;
- prompt and bounded private session state;
- help, discovery, and completion policy;
- resolver requests and display of resolved identities;
- immutable command arguments and semantic launch requests;
- one foreground command in Shell 1;
- cancellation requests and structured completion handling; and
- the most recent command status.

The host supplies:

- typed terminal input and presentation;
- immutable command-generation lookup;
- artifact acquisition and digest verification;
- application verification, runtime selection, launch, observation, and
  teardown;
- exact arguments, streams, directory/storage instances, clocks, and other
  capability bindings; and
- host-specific failures translated into accepted provider outcomes.

The browser host uses JavaScript and workers for those mechanisms. Windvale OS
uses user-space terminal, resolver, service-manager, filesystem, endpoint, and
runtime services. Neither host changes shell parsing or command meaning.

## Browser topology

```text
DOM terminal adapter
    |
    | Submit, Complete, Interrupt, Resize, Close
    v
Persistent shell worker
    - canonical shell WVB in the WebAssembly-hosted runtime
    - bounded session state
    - parser, help, aliases, resolver/launch requests, status
    |
    | versioned resolver and launch envelopes
    v
JavaScript shell host adapter
    - capability broker
    - active browser command generation
    - digest-bound artifact acquisition
    - launch-plan revalidation
    |
    +--> disposable command worker: echo
    +--> disposable command worker: file-read
    +--> disposable command worker: module-verify
    `--> disposable command worker: wvwasm
              |
              | bounded standard output, diagnostics, and completion
              v
         shell worker -> terminal adapter
```

The persistent shell worker and each command worker run off the UI thread. The
page owns presentation and user-mediated permissions. It does not parse Shell 1,
resolve package identity, execute command behavior, or hold permanent command
session state after the Windvale shell is promoted.

## Execution strategy

### First route: canonical shell WVB in a Wasm-hosted runtime

The first complete browser shell should execute the canonical shell WVB through
the existing WebAssembly-hosted runtime direction. This keeps the same shell
artifact used by the Windows/Linux runtime and later Windvale OS, provides a
simple differential oracle, and does not wait for complete direct typed-WIR-to-
Wasm lowering.

The browser runtime must extend beyond the current one-shot scalar
`Main() -> i32` profile. A shell is long-lived and must suspend while waiting for
terminal, resolver, child-completion, cancellation, and provider events. It must
not busy-loop inside Wasm or depend on browser callbacks reentering live guest
state.

### Later route: direct shell Wasm

After the direct Language 1.0 WebAssembly backend supports every reachable shell
value, operation, ownership, function/effect, and suspension shape, it may emit a
direct shell `.wasm`. That artifact consumes the same terminal, resolver, launch,
stream, and completion ABIs.

Direct compilation is an execution optimization and packaging choice. It does
not create a new shell identity, grammar, command catalog, capability model, or
session behavior. Canonical shell WVB remains the distribution and differential
identity unless a later decision changes that contract.

## Terminal and suspension boundary

The browser page converts DOM input into typed terminal events. The first useful
event set should include:

- `Submit` for one completed bounded line;
- `Complete` for one line and cursor position;
- `Interrupt` for the foreground operation;
- `Resize` only when the selected terminal presentation consumes it; and
- `Close` for orderly session completion.

The shell emits typed terminal requests such as prompt text, bounded output,
diagnostics, replacement ranges for completion, clear, and orderly close. Raw
keyboard events, DOM nodes, CSS, ANSI input parsing, browser clipboard objects,
and JavaScript exception values do not cross this boundary.

When no event is available, the shell returns `Suspended` through the shared
request/result/resume ABI. JavaScript queues immutable terminal and child events
and resumes the shell only after the previous guest transition has returned.
The first profile permits one foreground command and one active terminal wait.
It needs no WebAssembly threads, shared memory, synchronous browser filesystem,
service worker, or cross-origin isolation.

## Resolver and browser command generation

The browser resolver uses one immutable active generation. A URL, OPFS filename,
cache entry, fetch order, or JavaScript object is not executable identity.

For each command, the launch-critical record binds at least:

- canonical command spelling and aliases;
- package, part, module, entry, and canonical WVB identity;
- source profile, platform scope, and required runtime profile;
- complete declared capability closure;
- approval and launch-profile identities;
- argument, stream, memory, instruction, call-depth, output, and wall-time
  limits; and
- the permitted browser acquisition source and expected artifact length/digest.

Resolution grants no authority and starts no worker. The shell receives an
immutable resolution result, constructs a semantic launch request, and submits
it to the launcher. The JavaScript launcher independently revalidates the active
generation, artifact identity, approval, grants, and resource bounds immediately
before making a command worker runnable.

The first browser generation may be a manifest-owned static package. Later
origin-stored generations require authenticated immutable manifests, atomic
activation or rollback, bounded retention, and explicit provider-loss behavior.

## Browser command launch protocol

The first browser launcher performs these steps:

1. accept one immutable launch request from the shell worker;
2. resolve it again against the active generation;
3. acquire the exact command WVB or an approved deterministic derivative;
4. verify artifact length, digest, WVB semantics, platform/profile requirements,
   capability closure, approval, and selected runtime;
5. construct rights-limited provider instances and one bounded resource ledger;
6. create a disposable command worker;
7. copy immutable arguments, provider requests, artifact identities, and limits
   into that worker;
8. execute the command off the UI thread;
9. forward bounded output and diagnostics through exact stream providers;
10. publish one structured completion to the shell; and
11. revoke grants, advance generations, close resources, discard late messages,
    and terminate the worker.

The first command worker represents an isolated browser execution container, not
a portable process identifier. Cooperative cancellation is requested through
the semantic provider. Forced worker termination is reported separately and
must trigger complete host cleanup.

## Standard byte output and browser presentation

Shell commands may produce arbitrary bytes. The standard byte-output provider
therefore remains separate from the DOM terminal's text presentation.

The browser host retains the exact accepted output bytes under the command's
fixed limit. If the selected terminal profile admits strict UTF-8 text, the page
may render that text with `textContent`. If the bytes are not valid text, the
page presents a bounded hexadecimal/escaped view, a byte-count summary, or an
explicit download action without changing the retained bytes. It must not use a
replacement decoder and then claim that the displayed text is exact command
output.

Provider acceptance proves that the bytes entered the bounded local output
channel. It does not prove DOM rendering, download completion, or user receipt.
The `file-read` qualification case compares exact retained bytes across hosts;
presentation is separate evidence.

## Command catalog and host scope

The first portable catalog should retain the Shell 1 integration pressure:

| Command | Browser use | Required browser work |
| --- | --- | --- |
| `echo` | First immutable-argument and line-output command. | Bind arguments, argument count, bounded line output, completion, and cleanup. |
| `file-read` (`cat`) | First exact arbitrary-byte command. | Bind one read-only directory/file instance and standard byte output. |
| `module-verify` | Inspect one exact WVB artifact without execution. | Bind immutable artifact read, verifier limits, and diagnostic/text output. |
| `command-info` | Inspect launch-critical identity and authority. | Bind resolver observation only; do not launch the selected command. |
| `wvwasm` | Convert one admitted WVB artifact to deterministic Wasm. | Bind one immutable input artifact, one output publication, diagnostics, output, and compiler resource limits. |

`wvwasm` is an external command application, not a shell built-in. Its presence
in a browser generation does not grant storage or compiler authority. An exact
launch record supplies only the resources selected for that invocation.

Workbench actions such as editor `open` and `save` remain explicitly browser-
scoped applications or terminal integrations. They do not silently enter the
portable command catalog merely because the bootstrap JavaScript shell currently
implements those spellings.

## Integrating the existing `wvwasm` application

The existing example has this semantic shape:

```text
wvwasm <input.wvb> <output.wasm>
```

It reads canonical WVB bytes, invokes the Windvale WebAssembly lowering library,
publishes Wasm bytes only on success, writes a bounded status line, and returns a
process result. It does not compile `.wv` source. A source-to-Wasm workflow first
constructs WVB through the ordinary compiler unless the separately accepted
direct typed-WIR target is selected.

The browser form should avoid granting two arbitrary path strings. The shell
passes user-visible relative names, while the launcher binds:

- one exact read-only artifact or rights-limited directory instance containing
  the selected input;
- one create/publication instance restricted to the selected output name;
- immutable argument values constructed by the launcher;
- standard and diagnostic output providers; and
- exact input, output, instruction, function, memory, diagnostic, and elapsed-
  time ceilings.

The command publishes no output after target rejection, verifier failure,
oversized input, oversized candidate, cancellation before publication, or
provider loss. Mutation outcomes must distinguish known-zero progress, exact
partial progress when the selected interface allows it, completion, and
indeterminate completion. An indeterminate publication is never retried without
an idempotency contract.

The browser may offer an explicit download only after the publication provider
has accepted the complete output bytes. Download presentation does not grant the
guest browser file-picker, download-manager, or native-path authority.

## Binary workspace and artifact store

The current Workbench workspace is a bounded UTF-8 editor workspace. General WVB
and Wasm files are binary artifacts and may exceed its current per-file ceiling.
The shell integration therefore needs a separate binary-capable provider rather
than widening the editor workspace implicitly.

The first artifact provider freezes:

- maximum artifact count;
- maximum individual WVB and Wasm bytes;
- maximum aggregate retained bytes;
- admitted relative-name grammar and depth;
- immutable snapshot-read behavior;
- create, replacement, and publication semantics;
- provider generation and stale-handle behavior;
- volatile memory versus origin-private persistence identities;
- quota, eviction, revocation, interruption, and teardown results; and
- download/upload boundaries when those optional page actions are selected.

OPFS and session memory are implementation mechanisms. The guest observes only
the exact Windvale artifact, directory, blob, or publication contract. A volatile
fallback cannot silently satisfy a persistent or durable request, and OPFS does
not imply a native path or Windvale OS filesystem.

## Browser capability profile

| Shell or command need | Browser provider |
| --- | --- |
| Terminal input/presentation | DOM adapter outside Wasm using typed bounded events and text-safe rendering. |
| Command resolution | Immutable digest-bound browser generation. |
| Command execution | Disposable or supervised dedicated workers. |
| Arguments | Copied immutable values; no ambient browser command line. |
| Standard output/diagnostics | Separate bounded byte channels; acceptance does not prove user display. |
| Read-only artifacts | Explicit snapshot or rights-limited directory instance. |
| Output artifacts | Restricted create/publication instance followed by optional page-owned download. |
| Current location | One fixed display identity and one bound directory capability; no ambient `cd`. |
| Cancellation | Cooperative typed request followed, when necessary, by reported forced worker termination. |
| Time and deadlines | Explicit browser provider profile; event-loop timing does not define Windvale semantics. |
| Network | Denied unless the selected command separately declares and receives an exact browser network grant. |

The shell itself receives no ambient network, clipboard, editor, cookie,
credential, extension, native-file, or device authority. Command capabilities do
not flow back into the shell after completion.

## Ordered delivery slices

### Shell/WebAssembly slice 0: freeze exchange contracts

- Accept terminal/session request and event envelopes.
- Accept split launch-critical and presentation command metadata.
- Accept semantic resolver, launch, observation, cancellation, byte-stream, and
  structured-completion envelopes.
- Pin size, queue, session, history, command, child, output, diagnostic, and
  teardown limits.
- Extend the existing Shell 1 fixtures with resolver, stale-generation,
  substitution, launch-denial, cancellation, and completion cases.

### Shell/WebAssembly slice 1: first real browser command

- Retain the JavaScript Workbench shell as a labeled bootstrap controller.
- Package the existing `file-read` application in one exact browser generation.
- Bind immutable arguments, one read-only browser workspace instance, standard
  byte output, diagnostics, and resource limits.
- Launch the real verified command WVB in a disposable worker.
- Replace only the JavaScript `cat` branch after exact output, denial, provider-
  loss, cancellation, and cleanup cases pass.

### Shell/WebAssembly slice 2: `wvwasm` external command

- Add the bounded binary artifact provider.
- Package the existing WVB-to-Wasm application with one exact identity and
  target profile.
- Bind input snapshot, output publication, arguments, diagnostics, output, and
  compiler resource limits.
- Prove deterministic Wasm bytes against the retained native tool and independent
  engine validation.
- Add explicit download only after successful publication.

### Shell/WebAssembly slice 3: browser resolver and command set

- Add `echo`, `module-verify`, and `command-info` to the immutable browser
  generation.
- Prove resolution without authority, launch-time revalidation, target support,
  capability closure, denial, identity substitution rejection, and generation
  replacement.
- Keep one foreground command and no pipelines, redirection, background jobs, or
  native command escape.

### Shell/WebAssembly slice 4: portable interactive shell

- Implement the shell application around the existing Shell 1 parser.
- Bind terminal/session, help, fixed current location, resolver, launch,
  cancellation, output, diagnostics, and structured status.
- Run the canonical shell WVB in a persistent WebAssembly-hosted worker.
- Move parsing, aliases, help, resolver requests, foreground launch, and status
  out of the JavaScript bootstrap shell.
- Retain editor-only commands under explicit Workbench-scoped providers.

### Shell/WebAssembly slice 5: direct shell Wasm

- Admit the complete reachable shell WIR through the direct WebAssembly target.
- Emit deterministic shell Wasm while retaining canonical shell WVB identity.
- Run the same shell provider conformance suite against interpreted-WVB and
  direct-Wasm execution.
- Promote direct execution only when size, startup, steady-state, memory, and
  teardown evidence justify it.

### Shell/WebAssembly slice 6: Windvale OS binding

- Wait for one qualified terminal session, resolver/package service, resource
  domain, clean dynamic launch, isolated ordinary streams, directory provider,
  cancellation, observation, and teardown path.
- Bind the same canonical shell and command WVB identities to Windvale OS
  providers.
- Keep the kernel emergency sink and fixed recovery monitor separate from the
  ordinary shell.
- Do not fork shell grammar, parsing, aliases, or command identity for the OS.

## Verification plan

| Case | Required evidence |
| --- | --- |
| Parser corpus | Exact logical results and diagnostic offsets agree across Windows, Linux, browser runtime, and later Windvale OS. |
| Command resolution | Exact package/module/entry/approval/launch identities agree; resolution grants no authority. |
| Identity substitution | Any artifact, generation, approval, limit, or provider change between resolution and launch is rejected. |
| `echo` | Exact arguments, LF output, status, limits, and cleanup agree across qualified hosts. |
| `file-read` | Exact arbitrary bytes and no added terminator agree; missing, oversized, revoked, and stale providers fail identically. |
| `wvwasm` | Accepted input emits deterministic Wasm; malformed/unsupported input publishes nothing. |
| Terminal lifecycle | Submit, completion, interrupt, close, shell restart, terminal restart, and stale events remain bounded. |
| Command cancellation | Cooperative completion and forced worker termination remain distinct and release every provider. |
| Browser storage loss | Volatile fallback, eviction, quota, refresh, and tab closure surface as exact provider/generation outcomes. |
| Repeated launch | Workers, grants, streams, artifact buffers, listeners, and ledger charges return to zero after each terminal outcome. |
| Execution modes | Canonical-WVB and direct-Wasm shell modes preserve defined behavior and structured completion. |

Qualification records name the shell WVB, optional shell Wasm, command WVBs,
browser generation, provider versions, browser engine/version, exact inputs,
results, output bytes, resource maxima, and teardown evidence. One Chromium run
is development evidence, not a universal browser claim.

## Performance and memory evidence

Measure at least:

- shell worker cold start and first prompt;
- terminal-event and shell-resume latency;
- resolution and launch-plan validation time;
- command-worker creation, artifact verification, first output, completion, and
  teardown time;
- shell, command, resolver, output, history, and artifact retained-byte maxima;
- worker, Wasm memory page, open provider, listener, timer, and pending-operation
  maxima;
- WVB-interpreted versus direct-Wasm shell size and execution time; and
- `wvwasm` input size, output size, lowering time, peak memory, and independent
  validation time.

Do not widen an established compiler, runtime, workspace, worker, or artifact
ceiling merely to fit the shell. A limit change requires a measured workload and
updated rejection evidence.

## Non-goals

- Running the x86-64 Windvale OS image or its native shell binary in the browser.
- Implementing permanent shell parsing, resolution, or command behavior in
  JavaScript.
- Treating browser workers as portable process identities or OPFS as Windvale OS
  storage.
- Giving the shell ambient DOM, editor, filesystem, network, cookie, credential,
  extension, device, or native execution authority.
- Discovering commands from URLs, OPFS names, a native `PATH`, or the current
  directory.
- Adding Shell 2 pipelines, redirection, command substitution, background jobs,
  or job control to complete the first browser shell.
- Treating `wvwasm` as a direct `.wv`-to-Wasm compiler or making WVB cease to be
  the canonical distribution identity.
- Claiming browser or Windvale OS parity before their named provider and
  qualification gates pass.

## Open decisions before implementation

- Which exact terminal/session envelope becomes the first accepted interactive
  contract?
- Which signed format separates launch-critical and presentation command
  metadata?
- What is the first browser command-generation format and update/rollback rule?
- Does the first browser command worker interpret WVB, use an existing approved
  derived Wasm, or select between both through explicit launch metadata?
- Which binary artifact limits admit useful WVB/Wasm tools without weakening
  browser containment?
- Which browser storage profile can honestly support the publication completion
  required by `wvwasm`?
- Should `wvwasm` keep its current hosted whole-file input/output surface or move
  to bounded streaming artifact interfaces before browser qualification?
- Which output action stores Wasm in the browser artifact provider, downloads it,
  or does both under separate user intent?
- Which exact Language 1.0 and runtime slices make a persistent suspended shell
  ready without browser-specific semantics?
- Which real multi-command session qualifies promotion from the JavaScript
  bootstrap shell to the Windvale shell application?

These choices require focused decisions and measured fixtures. They are not
accepted implicitly by this proposal.
