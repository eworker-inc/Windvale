# Decision 0191: Windvale console, shell, and CLI architecture

- Date: 2026-08-03
- Status: Accepted future architecture; no new console-input, shell, stream, or CLI mechanism is implemented
- Refines: [Decision 0173](0173-Windvale-Process-Service-And-Driver-Architecture.md), [Decision 0181](0181-Next-Windvale-Os-Mechanism-Contracts.md), and [Decision 0183](0183-Product-Packaging-Trust-And-Evolution.md)
- Coordinates with: [Decision 0182](0182-Browser-And-WebAssembly-Product-Direction.md) for bounded wait/event delivery
- Remote transport depends on: [Decision 0192](0192-Capability-Oriented-User-Space-Network-Stack.md) and [Decision 0193](0193-Simple-Windvale-Remote-Terminal-Protocol.md)
- Architecture: [Console, shell, and CLI](../Architecture/Console-Shell-And-Cli.md)

## Context

Windvale has bounded immutable process arguments, separate normal and diagnostic output, deterministic strict-UTF-8/LF behavior, standalone host CLI applications, and a qualified three-process ready/wait OS baseline. It has no standard input, terminal session, dynamic process launch, general command resolver, environment, streams, pipelines, redirection, shell, or job control.

Decisions 0173 and 0181 already place ordinary serial output in a future isolated AOT driver while retaining an independent kernel boot/panic sink. A complete command environment now needs a durable boundary before an early serial command parser accidentally becomes a privileged kernel shell, terminal escape handling becomes an application ABI, or POSIX ambient process behavior becomes Windvale semantics.

## Decision

### Separate device, terminal, shell, and application responsibilities

Windvale uses four layers:

1. isolated drivers or authorized transport adapters own serial, keyboard, display, or remote transport mechanics;
2. a terminal service owns bounded sessions, input decoding, editing, presentation, and terminal events;
3. a shell is an ordinary capability-restricted application that resolves and composes commands; and
4. CLI applications execute as ordinary verified processes through immutable launch plans and bounded resource domains.

The kernel retains scheduling, process, memory, endpoint, capability, teardown, and its independent emergency output mechanisms. It owns no general command parser, path resolution, history, script evaluation, command registry, or terminal-presentation policy.

### Keep the shell small

Select a small command shell rather than a POSIX-compatible shell or a fully typed PowerShell-like environment. The first grammar admits explicit commands, ordered arguments, quoting, and `--`. Sequencing, byte pipelines, redirection, success/failure chaining, and one-argument variable expansion arrive only with their required process and stream contracts.

Do not add implicit globbing, post-expansion word splitting, command substitution, `eval`, functions, loops, unrestricted startup scripts, or hidden execution to the initial shell. General automation uses ordinary verified Windvale programs. A Windvale REPL remains a separate tool rather than a shell evaluation mode.

### Launch exact commands without ambient authority

A user-space resolver maps a canonical command identity to an exact package, module digest, entry point, platform/profile requirements, and declared capability set. Resolution does not scan an ambient `PATH`, implicitly execute the current directory, infer trust from a filename, or race mutable discovery between admission and launch.

The service manager validates one immutable launch plan before making the process runnable. The plan binds exact code and runtime identities, arguments, standard streams, optional current-directory and environment inputs, rights-limited capability instances, resource-domain ceilings, terminal attachment, cancellation, supervision, and completion policy.

The shell never transfers all of its own authority implicitly. Application requirements, user or package approval, concrete provider grants, and runtime binding remain separate. There is no automatically omnipotent root shell; administrative tools require exact additional capabilities.

### Use explicit standard streams

Future process launch binds separate bounded byte streams for standard input, standard output, and diagnostic output. Their versioned contracts define ordering, chunk bounds, partial progress, backpressure, end of stream, peer closure, cancellation, provider loss, and teardown.

Existing console text operations remain library conveniences that encode strict UTF-8 to the selected normal or diagnostic output and preserve Windvale's LF rule. Terminal control is a separate optional capability; a byte stream does not imply cursor, key, resize, or interactive authority.

The first pipeline connects standard output to standard input through a bounded byte stream, retains diagnostic separation, records every stage's structured result, and owns all stages and stream endpoints through one resource domain. Typed record pipelines remain a later explicit schema-versioned extension and never rely on content sniffing.

Filesystem redirection uses directory-relative file capabilities. Append waits for an exact append contract rather than changing `Writeˉat` semantics. Native paths and handles do not enter the shared shell contract.

### Keep session state explicit

The shell may own one current-directory capability, private variables and aliases, recent command results, foreground or later background job references, and bounded optional history/configuration. None is ambient inheritance.

An environment, when added, is an optional bounded immutable launch snapshot explicitly selected for a declaring consumer. Secrets use dedicated capabilities. History persistence requires separate storage authority and sensitive-input suppression before recording.

Process completion distinguishes normal result, launch or verifier rejection, capability refusal, language trap, process fault, cancellation, forced termination, and provider loss. Windvale does not use POSIX signals as its foundation. An interrupt key requests typed cancellation; forced termination is a distinct authorized operation.

### Preserve CLI boundary conventions

Canonical public command names use lowercase ASCII with `-` between words. Options use `--long-option`, with `--help`, `--version`, and `--` retained from the current CLI. Standard output carries requested data; diagnostic output carries errors and progress. Machine-readable output is explicitly selected and versioned.

Windvale source implementations retain U+02C9 names internally. Interactive command entry does not require that character. Human-readable output may evolve, while machine-readable schemas remain versioned and deterministic.

Keep built-ins limited to shell-session mutation such as help, exit, clear, current location, shell variables, foreground cancellation, and later job selection. Inspection, filesystem, package, shutdown, and VM operations remain separate applications with exact capabilities.

### Advance only after the process prerequisites

Implement in this order:

1. timer preemption;
2. independently lived memory, a flat resource domain, and clean dynamic process launch;
3. isolated ordinary serial output with the kernel emergency sink retained;
4. bounded serial input and one terminal session;
5. one single-session shell with resolver, immutable launch, explicit grants, and foreground completion;
6. standard byte streams, pipelines, cancellation, and complete teardown;
7. directory-relative redirection and filesystem tools;
8. bounded history, configuration, environments, and background jobs;
9. graphical terminals and, only after Decisions 0192 and 0193's secure network and session prerequisites are qualified, authenticated remote terminal adapters; and
10. typed pipelines, richer terminal surfaces, multi-user login, or compatibility shells only from measured needs.

A fixed recovery monitor may appear earlier, but it is not the permanent shell and grants no ambient kernel authority.

## Consequences

- Serial, graphical, remote, and test consoles can share one terminal and process model without making a device protocol the application contract.
- Shell failure is contained; it does not remove the terminal service or kernel emergency path.
- Commands execute with exact identities, grants, streams, and budgets instead of inherited ambient process state.
- Byte pipelines remain universal and simple while typed pipelines can be added later without replacing them.
- Real automation uses Windvale's verified language, avoiding a second general-purpose language with weaker authority and failure semantics.
- Existing hosted CLI and console contracts remain valid and become inputs to later adapters rather than being silently redefined.
- Familiar POSIX shell behavior, broad script compatibility, and unrestricted startup customization are deliberately deferred.

No terminal driver, input event, stream capability, launch-plan encoding, resolver, shell grammar, command catalog, pipeline, redirection, job, environment, login, remote console, or typed pipeline is implemented by this decision.

## Rejected alternatives

- **Put a command monitor in the kernel:** expands privileged parsing and policy, couples recovery to ordinary applications, and gives commands an unsafe authority boundary.
- **Adopt POSIX shell behavior wholesale:** imports ambient current directories, environment inheritance, path scanning, globbing, word splitting, signals, and complex quoting before Windvale has accepted those semantics.
- **Start with a fully typed shell:** creates a large second value, schema, formatting, and execution environment before byte streams and dynamic launch exist.
- **Make every command a shell built-in:** combines unrelated authority and failure domains and prevents independent application qualification.
- **Pass every shell capability to children:** turns interactive convenience into ambient authority and defeats transitive capability approval.
- **Use terminal escape bytes as the semantic API:** binds every application to one presentation protocol and makes serial compatibility define graphical and remote behavior.
- **Use shell scripts as the main automation language:** duplicates functions, control, error, resource, and capability semantics already owned by Windvale source.

## Reconsideration triggers

Reconsider a boundary when:

- a measured recovery requirement cannot operate safely outside the kernel emergency path;
- byte pipelines cannot support a concrete CLI workload without an earlier typed stream;
- the small shell makes common interactive work materially less usable than its safety benefit;
- command resolution cannot preserve immutable identity across installation or update;
- terminal-service isolation creates unacceptable latency for a named interactive workload;
- a concrete compatibility product requires a separately scoped POSIX-like shell; or
- multi-user or remote administration requires a stronger session/authentication split.

Any revision must preserve exact authority, bounded input and output, deterministic launch identity, failure containment, and the independence of the kernel emergency sink.
