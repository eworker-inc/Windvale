# Windvale applications

## Status

This tree owns useful deployable Windvale entry points. Applications are distinct
from reusable code in `Foundation/` and `Libraries/`, developer commands in
`Tools/`, and illustrative or conformance programs in `Examples/` and `Tests/`.

The first applications are `Database/Wvdb-Query.wv`, `Shell/Echo.wv`,
`Shell/File-Read.wv`, and the hosted `Model-Chat/Windvale-Model-Chat.mjs`.
`Wvdb-Query` reads one bounded immutable WVDB snapshot through
`filesystem.directory_read_v1`; `Echo` is the first ordinary application in the
accepted Windvale Shell 1 catalog.

`Web/Wvdb-Workbench/` is the first independently deployable browser application.
Its initial PWA is an explicitly synthetic, read-only WVDB administration
preview. Reusable browser framework and workbench components live in
`Libraries/Web/`; the public website and compiler playground retain their
separate owners.

## Echo

The command writes its immutable arguments separated by one ASCII space and one
final LF. Zero arguments write one LF. Empty arguments and strict Unicode text
are preserved rather than reparsed as a host command line.

`Echo` declares exactly standard line output plus bounded process argument and
argument-count access. It has no filesystem, diagnostic, environment,
native-process, or ambient path authority. The focused `echo-application` owner
builds deterministic Windows and Linux hosted applications and executes nine
success and boundary cases independently on both hosts. Exact Package 1, Lock 1,
Bundle 1, Approval 1, Launch Record 3, and Generation 1 records now bind `echo`
through the Windvale-written resolver and the guarded Windows/Linux dispatcher.
The separate ten-case `echo-command-launch` owner proves execution, rejection,
and private-host cleanup. An interactive shell remains a later integration.

The native Echo PE and ELF are standalone application hosts: a person may start
the exact file directly with arguments, and the running process does not need
`wv`, .NET, or a bytecode interpreter. Direct execution proves Echo's behavior
and fixed rights-reduced provider set, but it does not perform installed active-
generation selection, package/approval/launch identity admission, or rollback.

For Windvale 1.0, Echo still needs the installed product launch integration:

1. expose `wv run echo -- <arguments>` through the installed client;
2. replace caller-supplied development object paths and the Node.js dispatcher
   with durable-store lookup and the native guarded launcher;
3. install a non-conflicting `wv-echo` shim that carries only the canonical
   command identity and delegates to that same launcher; and
4. prove direct, explicit-client, and shim execution on Windows and Debian with
   identical application results and protected-launch contracts.

The host shim cannot portably claim the bare name `echo`: PowerShell, CMD, Bash,
and common Unix shells already reserve it as an alias or builtin. Bare
`echo hello` remains the intended spelling inside Windvale Shell, where the
Windvale command catalog owns resolution. This naming constraint does not make
the Echo application dependent on `wv`; it only separates a standalone process
from installed command selection and policy enforcement.

## File read

`file-read <name>` copies one named immutable-directory file to standard output
byte for byte and appends nothing. It reads in exact chunks of at most 3,072
bytes, refuses files larger than 4 MiB before emitting the first byte, and
declares exactly diagnostic output, one read-only directory instance, immutable
process arguments, and `standard_output.write_v1`.

The focused `file-read-application` owner validates the 32-byte provider response
format with 20 hostile cases, constructs deterministic Windows and Linux native
images, and executes 12 application cases per host. The cases cover invalid
UTF-8 bytes, chunk boundaries, the exact lifetime ceiling, argument errors,
unknown and unavailable entries, and link refusal. Shell 1 already maps the
fixed `cat` alias to canonical identity `file-read`; publication in an active
generation and browser/OS providers are not claimed by this application slice.

## Wvdb Query

The command contract is:

```text
wvdb-query <name.wvdb> <u32-key>
```

Its outcomes are:

| Exit | Meaning |
| ---: | --- |
| `0` | The key was found and its value was printed. |
| `2` | The snapshot was valid but the key was absent. |
| `3` | The rights-limited directory operation failed. |
| `4` | The bytes were read but the WVDB snapshot was invalid. |
| `64` | The arguments were invalid. |

The application declares the exact transitive capability closure at its root:
console output, diagnostic output, one read-only directory instance, and bounded
process argument access. Those declarations are requirements, not runtime grants.

The checked-in Project 2 manifest lives under `Projects/Applications/`; package
and lock metadata live under `Distribution/Applications/`. The current native
package front door deterministically builds and inspects the canonical WVB on
Windows and Linux. The focused capability owner also binds the rights-reduced
read-only directory provider and executes the application on both hosts.

## Model chat

`Windvale-Model-Chat` is the first user-facing command over the protected,
supervised external-model gateway. It creates and inspects passphrase-protected
credentials, lists models, and runs a bounded serial chat with one exact OpenAI,
Anthropic, or Google model over authenticated HTTPS.

On Windows:

```text
Applications\Model-Chat\Windvale-Model-Chat.cmd --help
Applications\Model-Chat\Windvale-Model-Chat.cmd credential create --provider openai --output openai.wvsc
Applications\Model-Chat\Windvale-Model-Chat.cmd models --credential openai.wvsc
Applications\Model-Chat\Windvale-Model-Chat.cmd chat --credential openai.wvsc --model <model-id>
```

On Linux, invoke the corresponding
`Applications/Model-Chat/Windvale-Model-Chat.sh` commands. API credentials and
passphrases are accepted only through masked terminal prompts; there is no
secret-bearing option or environment fallback. The command retains at most 32
messages and 16 KiB of canonical history, removes only complete oldest turns,
and never retries an uncertain submission. See the exact
[hosted command contract](../Specifications/Hosted-Model-Chat-Command.md).
