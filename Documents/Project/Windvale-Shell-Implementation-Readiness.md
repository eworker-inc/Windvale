# Windvale shell implementation readiness

## Status

- Date: 2026-08-15
- Status: parser implementation candidate verified on Windows; permanent
  interactive shell prerequisites remain incomplete
- Architecture: [Windvale shell](../Architecture/Windvale-Shell.md)
- Accepted first contract: [Windvale Shell 1](../../Specifications/Windvale-Shell-1.md)
- Parser decision: [Decision 0602](../Decisions/0602-Shell-1-Parser-Contract-And-First-Portable-Core.md)
- Accepted boundary: [Decision 0191](../Decisions/0191-Windvale-Console-Shell-And-Cli-Architecture.md)

This plan answers what must be decided, specified, implemented, and verified
before Windvale can honestly claim one shell across Windows, Linux, Windvale OS,
and the browser/WebAssembly Workbench. It does not make the shell the immediate
repository milestone and does not claim that an unavailable OS mechanism exists.

## Readiness summary

The capability-free Shell 1 parser is implemented under its accepted grammar.
Its 47-case focused owner passes on Windows and constructs Windows and Linux
hosted images; independent Linux execution and browser evidence remain pending.
The project is not yet ready to implement or qualify the complete permanent
interactive shell.

| Boundary | Current readiness | Meaning |
| --- | --- | --- |
| Product architecture | Accepted | Permanent terminal/shell/application split and Shell 1 parser choices are recorded in Decisions 0191 and 0602 |
| Parser grammar | Implemented candidate | Input, quotes, escapes, limits, names, aliases, diagnostics, and exact offsets pass the 47-case Windows owner |
| Windvale value model | Proven for parser slice | One immutable scan plus indexed word views and explicit materialization compiles and executes without a hidden host collection |
| Terminal input/editing | Not implemented | No standard terminal session or request/reply editing contract exists |
| Command metadata | Partial bootstrap evidence | Generation 1 resolves exact commands, but general split launch/presentation metadata is incomplete |
| Dynamic launch/observation | Not general | The host dispatcher executes two fixed profiles; Windvale OS and browser lack the permanent general provider |
| Standard byte streams | Not implemented | Existing console output is text-oriented; real `file-read`/`cat` requires exact byte output |
| Read-only directory | Useful first candidate | Exact immutable single-segment reads exist; general enumeration/navigation do not |
| Windows/Linux hosts | Strong bootstrap base | Arguments, output, package identities, runtime, and fixed dispatch evidence exist |
| Browser host | Workbench bootstrap exists | JavaScript shell, OPFS/session storage, compiler/runtime workers, and line output exist; guest launch providers do not |
| Windvale OS | Prerequisites incomplete | Protected processes and service evidence exist; general resource domains, dynamic launch, and terminal service do not |

## Product-owner decisions before implementation

Decision 0602 accepts these choices for Shell 1:

1. **One-token Shell 1 commands.** Use `file-read` as canonical syntax and `cat`
   as a fixed alias. Multiword `file read` remains grouped help only.
2. **Windvale applications only.** Do not scan native `PATH` or transparently
   execute `.exe`, ELF, PowerShell, Bash, or command files. Consider explicit
   platform-scoped `host-run` only later.
3. **Fixed initial location.** Shell 1 receives one directory capability and has
   `pwd` but no `cd`, `ls`, mutation, or redirection until their contracts exist.
4. **Exact bytes for `cat`.** `file-read` adds no newline and waits for standard
   byte output. A text-only interim tool uses the distinct name `file-show`.
5. **Small first catalog.** Start with `echo`, `file-read`, `module-verify`, and
   `command-info`, plus `help`, `clear`, `exit`, `status`, and `pwd` built-ins.
6. **Host-first development, cross-host claims later.** Implement the portable
   parser and host adapters before the OS prerequisites, but never describe that
   as an in-OS shell.
7. **No typed pipelines in Shell 1.** Preserve structured completion and metadata
   now; implement bounded byte streams before typed record pipelines.
8. **WebAssembly remains a named host profile.** A Chromium result is browser
   development evidence, not a universal browser or permanent-host claim.

Changes to these choices require a superseding dated decision because command
parsing, compatibility, authority, and application identity are durable public
behavior.

## Required contract work

### 1. Accept Shell 1 grammar and diagnostics

The accepted Shell 1 specification fixes the quote/escape behavior, reserved
operator set, unquoted one-token command rule, 4,096-byte line limit, 68-word
limit, fixed alias table, and `WVSH1xxx` diagnostics. A future grammar must
extend rather than reinterpret valid Shell 1 lines.

Deliverables:

- one accepted decision;
- the promoted Shell 1 specification;
- a machine-readable or source-owned golden fixture corpus; and
- a registered native verification owner before implementation paths multiply.

### 2. Prove the Windvale parser representation

Current Windvale execution has bounded text/bytes and records, but the parser
must publish an ordered variable-count word view and recoverable errors while a
long-lived shell continues after invalid user input.

Before implementing the parser, prove one of:

- immutable source spans plus a bounded index table owned by one parse result;
- a fixed-capacity record/array representation with explicit active count; or
- a canonical parse-result byte envelope decoded by the shell.

The choice must avoid global state, host arrays hidden behind callbacks, borrowed
data that outlives the submitted line, and traps for ordinary user syntax. Test
zero words, 68 words, empty words, maximum UTF-8, repeated parsing, and release or
reset of every owned value.

### 3. Specify terminal/session exchange

The terminal service owns generic editing and rendering. The shell owns prompt,
completion, and history policy. Freeze bounded non-reentrant request/reply
records for submit, completion, history, interrupt, end-of-input, disconnect,
prompt, replacement, candidate display, clear, and refusal.

Define:

- session and provider generations;
- line, cursor, candidate-count, candidate-byte, and prompt limits;
- ordering and resize coalescing;
- stale requests, shell restart, and terminal restart;
- whether a submitted line remains visible after shell failure;
- sensitive-input suppression before history sees bytes; and
- behavior for redirected or headless sessions without editing authority.

Parser work can start before this contract. An interactive Shell 1 application
cannot be claimed before it.

### 4. Specify command metadata

Extend the installed command model without overloading help text. Launch-critical
metadata must bind exact package, WVB/module, entry, target, capabilities, stream
kinds, approval, launch profile, runtime, resource profile, and machine schemas.
Presentation Metadata 1 separately binds summaries, usage, options, examples,
and completion labels.

Decide:

- whether aliases are shell-version data or signed generation records after the
  fixed Shell 1 table;
- how browser and Windvale OS targets appear in installation generations;
- how unsupported versus unapproved versus unavailable is reported;
- maximum commands, aliases, options, help bytes, and completion bytes; and
- how localization changes presentation resources without changing launch
  identity or machine output.

Current Generation 1 and active-command resolution are reusable evidence, not a
complete permanent shell metadata service.

### 5. Specify semantic launch and completion

Define a Windvale-accessible launch provider rather than a host-script call. One
immutable request must bind exact resolved identities, arguments, streams,
directory capability, selected grants, resource ceilings, foreground terminal,
cancellation source, observer, and completion destination.

Define structured outcomes for:

- unknown or unsupported command;
- malformed/inactive generation;
- verifier or runtime mismatch;
- capability refusal;
- normal application result;
- language trap and process fault;
- cooperative cancellation and forced termination;
- provider loss and launcher loss; and
- complete versus incomplete teardown.

The launcher reverifies every identity after resolution. No adapter may pass the
line through a native command shell or inherit environment, handles, descriptors,
working directory, browser globals, or service endpoints.

### 6. Specify standard byte streams

`console.write` accepts Windvale text and cannot implement a byte-exact `cat`.
Before `file-read` is qualified, define a directional bounded standard-output
byte stream with exact accepted progress, backpressure, cancellation, peer
closure, provider loss, and teardown.

Windows pipes, Linux pipes, Windvale OS endpoints, and browser worker messages
are adapters. Their native buffer sizes and partial-write behavior do not define
the semantic contract.

The first stream owner should test zero bytes, one byte, maximum chunk, multiple
chunks, invalid UTF-8 bytes, slow reader, early reader exit, cancellation under
backpressure, provider loss, and complete cleanup.

### 7. Bound filesystem scope

Use the existing immutable read-only directory capability for the first
`file-read` proof. Bind one exact directory snapshot at launch and pass one
single-segment filename argument. Do not infer current native paths.

Defer these separate contracts:

- deterministic directory enumeration for `directory-list`/`ls`;
- live generation-safe child/parent navigation for `cd`;
- replacement writing, partial progress, and durability;
- remove/create/rename semantics and indeterminate mutation; and
- redirection and append.

## Language and runtime readiness checks

Before writing the permanent shell application, confirm:

- ordinary parse and resolver failures can be returned as values and do not trap
  or terminate the shell process;
- a long-lived Windvale process can wait for repeated terminal and child events
  without callbacks or busy polling;
- all retained parser, history, completion, command, and child-result values have
  bounded ownership and release behavior;
- the runtime can bind terminal, resolver, launch, observation, cancellation,
  byte-stream, and directory providers by exact version;
- instruction, memory, output, child-count, and lifetime budgets are observable;
- provider restart publishes a new generation and wakes stale clients; and
- interpreter, JIT, AOT, and browser-hosted execution preserve the same defined
  shell results.

If the current language cannot yet represent the bounded argument view or event
loop cleanly, implement that focused language/runtime prerequisite first. Do not
put the missing semantics into a permanent JavaScript or native shell core.

## Implementation sequence after the decisions

### Slice 1: capability-free parser

Current result: implemented in `Libraries/Shell/Shell-1-Parser.wv`. The focused
owner proves deterministic library/test WVBs and test WVOs, all 47 cases on
Windows, and construction of both hosted target images. Independent Linux
execution, the reference-interpreter route, and browser WebAssembly-hosted
execution remain promotion evidence rather than completed claims.

- implement the Shell 1 parser as a focused Windvale library;
- return structured statuses and spans/views without I/O;
- compile it through the current native source front door;
- execute the same golden corpus through the reference interpreter and native
  Windows/Linux runtime; and
- run the parser WVB through the browser WebAssembly-hosted runtime.

This slice is safe to begin first. It does not need terminal input or process
launch and makes no interactive-shell claim.

### Slice 2: capability-free command launch proof

- package a real Windvale `echo` application;
- place its exact identity in a test active generation;
- resolve it through Windvale-owned semantics;
- launch it with immutable arguments and bounded text output; and
- prove success, unknown command, identity substitution, capability mismatch,
  budget exhaustion, and cleanup on Windows and Linux.

The browser may use a disposable test worker with the same immutable request and
completion shape. That adapter remains development-only until its generation and
provider contracts are accepted.

### Slice 3: exact `file-read`

- implement and qualify standard byte output;
- bind the existing read-only directory snapshot;
- implement `file-read` as an ordinary Windvale application;
- prove exact binary output with no appended newline;
- add the fixed `cat` alias; and
- replace the Workbench JavaScript `cat` branch only after the real WVB path
  passes its browser owner.

### Slice 4: hosted interactive shell

- bind the terminal/session exchange, help, fixed current location, resolver,
  foreground launch, cancellation, and structured status;
- run the same portable shell WVB on Windows and Linux;
- adapt the Workbench terminal to that shell rather than duplicating parsing in
  JavaScript; and
- qualify provider loss, shell restart, command failure, and bounded cleanup.

### Slice 5: Windvale OS integration

Wait for the OS to have one resource domain, clean dynamic launch, isolated
ordinary serial output, bounded serial input, one terminal session, resolver and
package access, standard streams, cancellation, observation, and teardown. Bind
the already-qualified shell WVB to those providers; do not fork an OS-specific
shell implementation.

Only this slice supports the claim that Windvale OS has the permanent Windvale
Shell. An earlier fixed recovery monitor remains separately named and bounded.

## Host-specific work before parity claims

### Windows

- choose and qualify at least one terminal adapter profile;
- prove strict UTF-8/LF capture independent of display rendering;
- prevent environment, handle, drive-current-directory, `PATH`, extension, and
  native command-line inheritance;
- translate control events to typed cancellation; and
- exercise ordinal directory semantics over case-folding native filesystems.

### Linux

- choose and qualify at least one TTY/PTTY adapter profile;
- prevent environment, descriptor, working-directory, `PATH`, signal, and
  `fork` inheritance;
- translate terminal input and interruption to typed events; and
- keep symlink, mode, ownership, and device behavior behind explicit providers.

### Browser/WebAssembly

- add a browser target/generation identity rather than treating a URL or OPFS
  name as executable identity;
- keep shell and command execution off the UI thread;
- bind only copied immutable requests and explicit providers to each worker;
- map OPFS/session storage loss, refresh, tab close, and worker death to defined
  generation/provider outcomes;
- enforce instruction, memory, output, and wall-time containment; and
- name each qualified browser engine rather than generalizing one Chromium run.

### Windvale OS

- complete the process/resource-domain and terminal prerequisites above;
- keep command resolution and shell policy in user space;
- preserve the independent kernel emergency sink;
- verify the same shell and command WVB identities used by the hosts; and
- prove shell/service failure containment and clean supervised replacement.

## Verification plan

Each slice receives one focused native owner and bounded hostile-input corpus.
The parser owner checks exact logical results and offsets. Resolver/launch owners
check identity substitution and authority separation. Stream and terminal owners
check backpressure, ordering, cancellation, stale generations, provider loss,
and cleanup.

Cross-host promotion requires:

1. exact shell and command WVB identities;
2. the same parser fixture results on Windows and Linux;
3. independent Windows and Linux execution through their real adapters;
4. named browser-engine evidence for the browser profile;
5. pinned QEMU plus independently reported host-build evidence for Windvale OS;
6. exact output bytes and structured completion records; and
7. zero leaked processes, workers, endpoints, streams, grants, or resource-domain
   charges after every terminal outcome.

Run one change-aware verifier per coherent implementation batch. A broad
qualification gate is a promotion action, not an inner-loop substitute.

## Stop conditions

Do not begin the full interactive shell if any of these remain unresolved:

- ordinary invalid input can terminate the shell process;
- parser word ownership depends on a host-only collection;
- command selection can change between resolution and launch;
- the child inherits the shell's complete authority;
- terminal callbacks can reenter shell state;
- `file-read` requires text decoding or adds a newline;
- cancellation and forced termination share one result;
- provider loss can silently rebind to a new generation;
- Windows/Linux native command discovery can bypass the resolver; or
- a browser or OS adapter needs different parser semantics.

When a stop condition appears, implement or specify the missing lower-layer
contract rather than compensating inside the shell.

## Definition of ready to implement

The capability-free parser implementation gate is satisfied: owner choices are
accepted, the result representation is proven, diagnostics are reserved, the
47-case fixtures are reviewed, and `shell-one-parser` owns native verification.
The complete native corpus now passes independently on Windows and Debian. An
11-check WebAssembly smoke over the same parser also passes in Node 24 and named
Chromium 151 without widening browser limits. Complete 47-case browser and
Windvale OS execution remain separate promotion evidence; they do not block the
next capability-free resolver/launch composition slice.

The hosted interactive shell is ready when terminal/session, command metadata,
resolver, semantic launch/observation, structured completion, standard byte
stream, and fixed-directory provider contracts are accepted with at least one
Windows and one Linux adapter plan.

The Windvale OS shell integration is ready when those same semantic contracts
have in-OS providers over qualified dynamic launch, resource domains, isolated
normal terminal I/O, cancellation, observation, and teardown.
