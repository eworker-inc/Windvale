# Windvale scripting proposal

## Status

- Date: 2026-08-16
- Status: proposed product direction; not accepted or implemented
- Source language: [Windvale Seed language](../../Specifications/Seed-Language.md)
- Execution format: [Seed bytecode](../../Specifications/Seed-Bytecode.md)
- Related shell direction: [Windvale shell product and architecture](../Architecture/Windvale-Shell.md)

This document begins the product definition for writing and running Windvale
scripts. It does not define a second language, change source semantics, add a
runner command, grant a capability, or claim that the current bounded WVB runner
can execute every example below.

## Product definition

A Windvale script is an ordinary `.wv` source program launched directly from
source for a short-lived task. Scripting is a source-first execution experience
over the same Windvale language, compiler, canonical WVB, verifier, runtime, and
capability contracts used by applications.

The first proposed command is:

```text
wv run Tool.wv argument1 argument2
```

The user selects one source file and supplies its arguments. Windvale owns the
compile, verification, cache, launch, and approval details behind that command.
The normal interface does not require a user to construct WVB filenames, run
separate build and verification commands, or repeat exact capability identities
as command-line flags.

Scripting therefore adds convenience, not another semantic layer:

```text
Tool.wv
  -> immutable source snapshot
  -> canonical WVB construction
  -> mandatory WVB admission
  -> bounded execution
```

## Goals

The scripting experience should:

- make a small Windvale program as easy to start as `wv run Tool.wv`;
- retain one `.wv` syntax and one set of source semantics;
- keep compilation and bytecode verification automatic and normally quiet;
- pass arguments without an additional separator in the common case;
- make common console scripts require no authority flags;
- summarize exceptional authority once in user language rather than exposing a
  growing list of capability ABI names;
- preserve exact capability declarations, grants, resource scopes, limits, and
  failure results internally;
- behave consistently on Windows and Linux, then on every later host that
  supplies the required semantic providers; and
- remain bounded, deterministic where its inputs are deterministic, and suitable
  for later packaging as an ordinary Windvale application.

## Current starting point

The repository already has the underlying pieces, but not their scripting
composition:

- the native compiler accepts an explicit root `.wv` source plus explicit
  dependencies and constructs canonical WVB;
- canonical WVB must cross the semantic verifier before execution;
- the current `wvrun` product executes a bounded capability-free subset;
- hosted Windvale applications already demonstrate immutable arguments, text and
  byte output, and rights-limited file access through narrower launch profiles;
- the installed `wv` command currently exposes product inspection while the
  native tools remain separate commands; and
- Shell 1 deliberately excludes command files and is not yet an interactive
  source runner.

These facts make a host-first source runner practical to design. They do not yet
make `wv run Tool.wv` an implemented command.

## Proposed command experience

The first command shape is:

```text
wv run <source.wv> [argument ...]
```

Rules for the first slice:

1. `<source.wv>` names one explicit root source file through the invoking host's
   CLI boundary.
2. Every token after the source name is an application argument, including a
   token beginning with `-` or `--`.
3. Runner options, if any are later accepted, occur before the source name.
4. No `--` separator is required after the source name. A script may still
   interpret an argument spelled `--` according to its own option contract.
5. The host path selects input bytes; it does not become a portable Windvale
   path, current-directory capability, or ambient filesystem grant.
6. The first slice accepts one source file. Explicit dependency source sets and
   `.wvproj` execution can follow without changing the single-file command.

Examples:

```text
wv run Greeting.wv Windvale
wv run Check-Header.wv --strict Input.wvb
wv run Calculate.wv 3 5 8 13
```

`--strict` in the second example belongs to `Check-Header.wv`; it is not a `wv`
option because it occurs after the source name.

## Source form

The first scripting slice uses ordinary Windvale source. It introduces no `.wvs`
extension, implicit imports, top-level statements, hidden mutable globals,
truthiness, dynamic types, or alternate error semantics.

A script retains an explicit module header and an exported
`Main() -> i32` entry point. For example:

```text
module Greeting;
platform linux, windows;
authority application;
requires capability console.write_line version 1;
requires capability process.argument version 1;
requires capability process.argument_count version 1;

export fn Main() -> i32 {
    if process.argument_count() == 0u32 {
        console.write_line("Hello from Windvale");
        return 0;
    }
    console.write_line(process.argument(0u32));
    return 0;
}
```

It would be invoked as:

```text
wv run Greeting.wv Alice
```

The exact capability requirements remain in source metadata because they are part
of compilation, application identity, provider selection, and review. The user
does not repeat them as `--allow console.write_line` or similar launch flags.

Later evidence may justify concise source sugar such as an omitted module header
or top-level statements. Any such feature must lower deterministically to the
ordinary language contract, preserve diagnostics and authority, and be accepted
as a source-language change. It is not part of this proposal's first slice.

## Hidden construction and cache

`wv run` owns these internal steps:

1. acquire one immutable snapshot of every explicit source input;
2. construct the canonical source set;
3. compile it to canonical WVB;
4. completely admit the WVB before execution;
5. select an execution engine and concrete rights-limited providers; and
6. execute `Main() -> i32` with the immutable argument vector.

These are safety and reproducibility boundaries, not separate user tasks.
Ordinary success should print only the script's output. Diagnostics must still
distinguish source acquisition, compilation, verifier rejection, approval,
provider binding, runtime failure, and the program's returned status.

An implementation may keep a bounded local compilation cache. A reusable cache
entry must be bound to the exact source-set bytes, compiler identity, source and
WVB contract versions, and build options that affect output. Cache reuse never
bypasses WVB admission. A missing, stale, malformed, oversized, or unwritable
cache is a cache miss, not a change in script behavior.

The cache is an internal performance feature. Its directory, record layout, and
eviction policy are not part of portable script semantics, and cached bytes are
not a distribution or package identity.

## Simple authority experience

Windvale must preserve the separation between a module's requirements, an
application approval, a runtime grant, and a concrete provider binding. The
normal scripting interface should not force a person to express that separation
as many command-line flags.

### Base script policy

The act of explicitly running local source selects a small base launch policy.
That policy may automatically approve and bind only the capabilities that the
module both declares and uses from these categories:

- immutable application arguments;
- standard text or byte output; and
- diagnostic output.

The base policy supplies no standard input, environment, clock, randomness,
network, native-process execution, credential, device, filesystem enumeration,
or ambient current-directory authority.

This automatic base approval is still an explicit launcher policy and an exact
runtime grant internally. It does not turn capability declarations into grants
or give undeclared capabilities to the program.

### Exceptional authority

When a script requests resources outside the base policy, the runner presents
one bounded summary in user terms. For example:

```text
Report.wv requests:
  read: selected Reports directory
  write: selected Output directory
  network: none

Allow once / Remember / Deny
```

The summary groups implementation-level capability calls by the authority and
resource scope the user is actually deciding. Exact capability names, major
versions, limits, provider generations, and transitive requirements remain
available through inspection but do not become the normal launch syntax.

`Remember` creates an approval bound at minimum to:

- the exact source-set digest;
- the requested capability identities and major versions;
- the concrete resource scopes and rights;
- the applicable platform and execution profile; and
- the relevant resource ceilings.

A source change, authority expansion, resource-scope change, incompatible
contract version, or approval revocation requires a new decision. Remembered
approval cannot silently widen. Provider loss and stale bindings remain explicit
execution outcomes.

Noninteractive automation should consume one inspectable approval or launch
record rather than reproduce one flag for every capability. The exact record
format, storage, signing, expiry, and administrative policy require a later
focused contract.

Per-capability switches such as `--allow console.write_line` are not the proposed
normal interface. A future diagnostic tool may expose exact identities for tests
or investigation, but that surface must not define the everyday product.

## Failure and output behavior

The command must preserve distinct outcomes:

- source input was missing, changed during acquisition, malformed, or oversized;
- compilation rejected the source;
- constructed or cached WVB failed admission;
- requested authority was denied or unsupported;
- a concrete provider was unavailable, stale, revoked, or lost;
- execution exhausted a declared resource budget or trapped; or
- `Main` completed and returned an application status.

Compilation, verification, approval, and provider failures occur before the first
script instruction. A failed construction must not execute an older cached module
under the new source name. Diagnostics go to the diagnostic channel; ordinary
script output remains separate. The host CLI maps the final structured result to
a stable process exit status without redefining the Windvale application result.

## Relationship to the shell and packages

The source runner is a host CLI product before it is a Windvale Shell feature.
It does not extend Shell 1 grammar, scan a command path, execute a native host
script, or make source filenames installed command identities.

A later Windvale Shell command may request source execution only after the shell
has explicit compiler, source-input, approval, launch, observation, and teardown
providers. That future command must preserve the same source snapshots, verifier
boundary, authority summary, and completion meanings as the host CLI.

Reusable or shared automation should be publishable as an ordinary Windvale
application and package without changing its language semantics. Package
installation and activation remain declarative transactions: this scripting
proposal does not introduce arbitrary installer, upgrade, uninstall, or package
hook scripts.

## Delivery slices

### Slice 1: one-file local scripts

- implement `wv run <source.wv> [argument ...]` on Windows and Linux;
- accept one ordinary source module with `Main() -> i32`;
- compile, verify, and execute without a user-visible intermediate WVB;
- support portable scripts and the base arguments/output policy;
- keep caching optional and behavior-neutral; and
- prove argument, diagnostic, exit-status, malformed-source, verifier-rejection,
  unsupported-capability, and deterministic-repeat cases on both hosts.

### Slice 2: one selected read-only directory

- summarize one requested directory-read scope;
- support Allow once and Deny without persistent approval;
- bind one rights-limited no-link directory provider;
- expose no native path inside Windvale source; and
- prove denial, traversal rejection, provider loss, byte limits, and teardown.

### Slice 3: remembered and noninteractive approval

- define one bounded approval record;
- bind it to source, contract, platform, resource, rights, and limit identities;
- re-prompt or reject on every widening or stale identity;
- support inspection and deliberate revocation; and
- prove that cache reuse cannot reuse or widen approval.

Write, network, clocks, credentials, native processes, background execution,
standard input, and additional source-set forms should enter only through later
consumer-driven slices and their existing semantic capability contracts.

## Non-goals

The first scripting profile does not provide:

- a second scripting language or `.wvs` source format;
- top-level-statement or dynamic-language semantics;
- a REPL;
- shebang or direct executable-file registration;
- ambient environment variables, current-directory access, path scanning, or
  inherited native handles;
- arbitrary Windows, Linux, PowerShell, Bash, executable, or shared-library
  invocation;
- Shell 2 pipelines, redirection, command substitution, background jobs, or
  command-file syntax;
- arbitrary package installation scripts; or
- a way to skip compilation, WVB verification, approval, resource limits, or
  teardown reporting.

## Open design questions

The following choices remain open until implementation evidence can answer them:

- What bounded cache size and eviction policy preserve fast startup without
  retaining unbounded source-derived state?
- What exact source-acquisition transaction detects a file changing during the
  first host read?
- Which existing approval format can be reused without confusing source-local
  execution with installed package approval?
- How should a host display a directory or resource identity without making that
  spelling part of portable semantics?
- Should Project 2 execution use `wv run Project.wvproj`, infer a project beside
  a source file, or remain a separate command?
- What measured startup cost would justify a shebang or executable shim after the
  explicit CLI path is stable?
- Which first real script requires standard input, write authority, a clock, or
  network access strongly enough to define the next profile?

## Acceptance boundary

This proposal becomes an implemented scripting contract only after a named
decision selects the command and authority behavior, a specification freezes the
accepted inputs and outcomes, the installed `wv` client owns the route, and
focused Windows and Linux evidence proves construction, verification, execution,
approval, limits, failure preservation, and cleanup.

Until then, documentation and examples must describe `wv run Tool.wv` as
proposed. The existing explicit `wvbuild`, `wvverify`, and `wvrun` commands remain
the supported working path.
