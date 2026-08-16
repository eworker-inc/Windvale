# Windvale Shell 1

## Status

Accepted parser and command contract with an implemented portable parser
candidate under
[Decision 0602](../Documents/Decisions/0602-Shell-1-Parser-Contract-And-First-Portable-Core.md).
The focused owner currently has isolated Windows execution evidence over 47
cases and constructs both hosted target images. This specification does not
claim an implemented interactive shell, terminal service, command
metadata format, dynamic launcher, standard byte stream, browser command worker,
or Windvale OS service.

The product intent and cross-host boundary are defined by the
[Windvale shell architecture](../Documents/Architecture/Windvale-Shell.md).
The [implementation-readiness plan](../Documents/Project/Windvale-Shell-Implementation-Readiness.md)
identifies prerequisites and the safe implementation order. Existing immutable
application-argument limits remain defined by
[hosted resources](Hosted-Resources.md).

## Scope

Shell 1 defines:

- one bounded submitted command line;
- deterministic tokenization into one command spelling and ordered arguments;
- canonical one-token command identities and fixed shell-version aliases;
- a small built-in set;
- exact separation between parsing, alias selection, command resolution,
  authorization, and launch; and
- stable parser result and diagnostic meanings.

Shell 1 does not define pipelines, redirection, sequencing, chaining, variables,
globbing, user aliases, current-location changes, history persistence, command
files, background jobs, environments, native-host command discovery, or typed
record streams.

## Selected first behavior

- Canonical commands are one lowercase ASCII token with optional internal `-`.
- Multiword forms such as `file read` are grouped help only, not executable
  syntax.
- Shell-version aliases map one token to one canonical command without adding
  arguments or syntax.
- Only exact Windvale application identities from the active generation may be
  launched.
- The session starts with one fixed directory capability and has no `cd`.
- One foreground command may run at a time.
- The command token is not included in the child's immutable argument vector.
- Host command lines, paths, environments, handles, and process conventions are
  outside this grammar.

## Input

The portable parser consumes one immutable byte value representing a submitted
line without a terminating CR or LF.

- Input must be strict UTF-8.
- Input contains at most 4,096 bytes.
- Literal Unicode scalar values U+0000 through U+001F and U+007F are forbidden
  except U+0009 horizontal tab, which is a separator outside quotes.
- U+000A and U+000D therefore cannot occur literally. Double-quoted escapes can
  construct newline, carriage-return, or tab inside an argument.
- No normalization, case folding, locale mapping, or newline conversion occurs.
- Error offsets are zero-based UTF-8 byte offsets into the original input.
- An end-of-input error reports offset equal to the input byte length.

Error selection is deterministic. The parser checks the byte-length limit first,
then strict UTF-8, then scans decoded scalars from left to right and reports the
first lexical or grammar failure encountered. It checks the command spelling only
after a syntactically valid word sequence has been produced.

The 4,096-byte line limit is intentionally below the existing hosted process
snapshot limits. A parsed line can contain at most 67 application arguments,
each remains below 4 KiB, and their aggregate remains below 64 KiB.

## Grammar

The grammar is described over decoded Unicode scalar values. `space` is U+0020
and `tab` is U+0009.

```text
line          = separators? (command (separators word)*)? separators?
separators    = (space | tab)+
command       = unquoted
word          = unquoted | single-quoted | double-quoted
single-quoted = "'" single-value* "'"
double-quoted = '"' double-item* '"'
double-item   = double-value | escape
escape        = "\\" ("\\" | '"' | "n" | "r" | "t")
```

An `unquoted` word contains one or more allowed scalar values and cannot contain
a separator, quote, backslash, literal control, or reserved Shell 2 operator.
A `single-value` is any admitted non-control scalar except `'`. A `double-value`
is any admitted non-control scalar except `"` and `\`.

Words do not concatenate. After a closing quote, the next scalar must be a
separator or end of input. Before an opening quote, the parser must be at the
beginning of a word. For example, `ab"cd"`, `"ab"cd`, and `'ab'"cd"` are
rejected rather than joined.

The command word is always unquoted. A quote at the beginning of word zero is a
syntactically complete word form but fails command spelling with `WVSH1010` at
the opening quote. Quoting changes argument boundaries only; it cannot hide or
construct an executable identity.

Empty quoted words are valid arguments:

```text
echo "" ''
```

An empty or separator-only line succeeds as `Empty` and performs no resolution,
authorization, or launch.

## Quoting and escapes

Single quotes preserve their contents exactly. They have no escape syntax. A
single quote cannot appear in a single-quoted word in Shell 1.

Double quotes recognize exactly five escapes:

| Source | Result |
| --- | --- |
| `\\` | U+005C reverse solidus |
| `\"` | U+0022 quotation mark |
| `\n` | U+000A line feed |
| `\r` | U+000D carriage return |
| `\t` | U+0009 horizontal tab |

Every other backslash sequence is invalid. Backslash outside double quotes is
reserved and invalid. A Windows-looking value must therefore be quoted as data,
for example `'C:\Work\Input.wvb'`; it is not interpreted as a portable path or
authority.

## Reserved characters

The following scalar values are reserved outside quotes for possible later shell
versions and cause `WVSH1007` in Shell 1:

```text
; | < > & $ ` ( ) \
```

Quotes are grammar delimiters and produce quote-specific errors when misplaced.
Reserved characters may be passed as data inside single quotes or, except for
the double-quote/backslash escape rules, inside double quotes. Shell 1 has no
comments; `#` is ordinary data.

## Word and argument limits

- A parsed nonempty line contains at most 68 words.
- Word zero is the command spelling.
- The remaining zero through 67 words are application arguments.
- Each decoded word contains at most 4,096 strict-UTF-8 bytes.
- Empty quoted words count toward the word limit and occupy one argument entry.
- The parser checks all additions and offsets before advancing.
- Limits are checked before alias resolution or resolver access.

The command is never inserted as argument zero. This preserves the existing
Windvale process argument contract, which excludes executable or module identity.

## Command spelling

A command identity or alias must match:

```text
[a-z][a-z0-9]*(-[a-z0-9]+)*
```

The match is over ASCII bytes and is case-sensitive. Leading, trailing, or
repeated `-`, uppercase ASCII, non-ASCII text, and every other punctuation are
invalid command spellings. Argument words have no such command-name restriction.

Built-in names are reserved by the Shell 1 version and cannot be shadowed by an
installation command or alias.

## Initial aliases

The first required alias table contains:

| Alias | Canonical identity |
| --- | --- |
| `cat` | `file-read` |

`pwd` is a built-in rather than an alias. `ls`, `rm`, and `ps` are expected
future aliases but do not enter the required table until their exact canonical
applications exist. The shell reports aliases through help and command
inspection.

Alias resolution occurs once after parsing and before active-generation command
resolution. It cannot recurse. If the canonical target is absent, unsupported,
or unauthorized, that exact outcome is reported; the alias does not select a
fallback.

User, session, package, host, and environment aliases are outside Shell 1.

## Parser result

The capability-free parser returns one immutable logical result:

```text
Shellˉparseˉresult
    Status: Shellˉparseˉstatus
    Errorˉoffset: u32
    Command: text
    Arguments: bounded ordered text view
```

`Empty` and `Valid` have `Errorˉoffset = 0`. `Empty` has empty command and
arguments. `Valid` has a nonempty command and zero through 67 arguments. A
failure has empty command and arguments plus the first deterministic error byte
offset. The eventual source representation may use spans into the immutable
input or an encoded result envelope if current source collection support cannot
own a bounded text vector; either representation must preserve this logical
result exactly.

The implemented source representation is one `Windvaleˉshellˉoneˉscan` plus
`Windvaleˉshellˉoneˉwordˉat` indexed views over the immutable input.
`Windvaleˉshellˉoneˉwordˉbytes` explicitly materializes a selected word and
decodes double-quote escapes; `Windvaleˉshellˉoneˉcanonicalˉcommand` applies the
fixed `cat` alias. These functions are capability-free and retain no global or
host-owned collection.

## Parser diagnostics

The diagnostic family is:

| Code | Meaning | Offset |
| --- | --- | --- |
| `WVSH1001` | invalid UTF-8 | first invalid or incomplete byte |
| `WVSH1002` | input exceeds 4,096 bytes | 4,096 |
| `WVSH1003` | forbidden literal control | first forbidden control byte |
| `WVSH1004` | unclosed single quote | input byte length |
| `WVSH1005` | unclosed double quote | input byte length |
| `WVSH1006` | invalid or incomplete double-quote escape | backslash byte |
| `WVSH1007` | reserved Shell operator outside quotes | reserved scalar byte |
| `WVSH1008` | more than 68 words | first byte of word 69 |
| `WVSH1009` | missing separator before or after a quoted word | first misplaced scalar or quote byte |
| `WVSH1010` | invalid command spelling | first byte that violates the command-name automaton; a trailing or repeated `-` reports that `-` |

Only the parser emits this family. Unknown command, unsupported target,
malformed generation, verifier rejection, capability refusal, application
result, cancellation, and provider loss are later resolver, launch, runtime, or
completion outcomes and must not be recategorized as parse errors.

A first portable diagnostic rendering is:

```text
shell parse status=<code> offset=<unsigned-decimal>
```

It is written to diagnostics with one LF by the shell adapter. The pure parser
returns the structured status and does not perform I/O.

## Built-ins

Shell 1 reserves five built-ins:

| Command | Arguments | Behavior |
| --- | --- | --- |
| `help` | zero or one command spelling | show bounded shell help or inspect one command without launching it |
| `clear` | none | request semantic terminal clearing; emit no escape bytes directly |
| `exit` | none | request orderly shell completion when no foreground command is live |
| `status` | none | display the most recent structured completion, or `none` before a command completes |
| `pwd` | none | display the session launcher's fixed directory display identity |

Invalid built-in arguments produce a shell usage result, not an application
launch. The exact structured shell-usage and session-completion records still
require the terminal/session specification and are readiness prerequisites.

Shell 1 has no `cd`. `pwd` does not prove or recreate directory authority.

## Initial external command catalog

The selected first qualification catalog is:

| Command | Purpose | Minimum authority |
| --- | --- | --- |
| `echo` | write its arguments separated by one ASCII space and one final LF | standard text output |
| `file-read` | write exactly one named file's bytes with no added terminator | one read-only directory plus standard byte output |
| `module-verify` | completely verify one WVB file and report its identity | one read-only directory plus text/diagnostic output |
| `command-info` | inspect exact command and launch-critical metadata without launching the target | resolver observation |

This table selects the first integration pressure; each application still needs
its own specification or existing reusable contract. `file-read` cannot be
implemented under only `console.write_line`. If standard byte output is absent,
an interim strict-text tool must be named `file-show` and cannot satisfy the
`file-read`/`cat` qualification case.

## Resolution and launch separation

For a valid non-built-in command, the shell performs these steps in order:

1. replace one fixed alias with its canonical identity, if applicable;
2. request that identity from one immutable active-generation snapshot for the
   current target;
3. receive an exact package, part, module, entry, approval, launch, runtime,
   platform, capability, and resource-profile identity set;
4. construct the child argument vector without the command token;
5. request concrete rights-limited providers for this invocation;
6. submit one immutable launch plan for complete revalidation; and
7. observe foreground execution and structured completion.

Resolution grants no authority and creates no process. The launcher does not
reinterpret command spelling, aliases, help metadata, host paths, or the current
directory display identity. The shell does not retry an indeterminate mutation
or silently substitute another command after any failure.

Shell 1 launches no ambient Windows or Linux native command. A future explicit
`host-run` application requires a separate platform-scoped contract and grant.

## Command metadata split

Launch-critical metadata is immutable and identity-bound. At minimum it names
the command, package, part, module digest, entry point, platform/profile scope,
approval, launch profile, declared capabilities, stream kinds, resource profile,
and machine-schema identities.

Presentation metadata is a separately digest-bound resource. It may contain
summary, usage, option descriptions, examples, completion labels, and later
localized text. It cannot select executable identity, change argument
boundaries, alter authority, or override launch-critical metadata.

Shell 1 help may initially use a fixed built-in catalog plus identity-only
installed-command listing while Presentation Metadata 1 is designed. General
option completion must not execute the target or parse unauthenticated text.

## Terminal editing boundary

The terminal service owns device decoding, a generic bounded editable buffer,
cursor/selection operations, and rendering. The shell owns prompts,
command-aware completion candidates, and history policy.

The minimum semantic exchange to specify before an interactive shell is:

- terminal to shell: `Submit(line)`, `Complete(line, cursor)`,
  `History(direction)`, `Interrupt`, `End_input`, and `Disconnect`;
- shell to terminal: `Prompt(text)`, `Replace(start, end, text)`,
  `Candidates(items)`, `Clear`, and `Refuse(status)`.

Every line, cursor, replacement, candidate set, and rendered value is bounded.
Messages are queued and non-reentrant. The exact records, limits, generations,
and stale-session behavior require a separate terminal/session contract. A
parser implementation can precede that contract; an interactive Shell 1 claim
cannot.

## Minimum host-provider profile

An interactive Shell 1 host eventually needs versioned semantic providers for:

- one terminal session and the editing exchange above;
- active-generation command resolution and metadata observation;
- immutable clean launch with exact arguments, streams, grants, and resource
  ceilings;
- foreground observation, cooperative cancellation, forced termination, and
  complete teardown reporting;
- separate standard text/byte output and diagnostic output;
- one fixed directory capability plus a display identity; and
- optional bounded history/configuration storage only when persistence is
  enabled.

Canonical capability names, signatures, record encodings, and limits are not
invented by this parser specification. They must be accepted in focused
contracts before the shell application imports them.

## Conformance fixtures

The parser owner must include exact input bytes, logical result, decoded words,
and error offsets for at least:

- empty, space-only, and tab-only input;
- one command and the maximum 67 arguments;
- empty quoted arguments;
- every valid escape individually and together;
- literal Unicode at one-, two-, three-, and four-byte UTF-8 widths;
- a 4,096-byte valid line and a 4,097-byte rejection;
- invalid leading, continuation, overlong, surrogate, and out-of-range UTF-8;
- every forbidden control and reserved operator;
- unclosed quotes, incomplete escape, invalid escape, and quote concatenation;
- valid and invalid command spellings;
- `--` preserved as ordinary argument data;
- Windows-looking and Linux-looking values preserved only as quoted data; and
- deterministic repetition with byte-identical result encoding when an envelope
  is selected.

The same fixture corpus must agree through the reference interpreter, native
Windows and Linux execution, browser WebAssembly-hosted execution, and eventually
Windvale OS. Host agreement over a different parser or error offset is not Shell
1 conformance.

## Deferred Shell versions

Shell 2 may add sequencing, byte pipelines, redirection, and conditional chaining
only after stream, file-mutation, cancellation, and aggregate-teardown contracts
are qualified. A later shell version may add one-argument variables, user aliases,
directory changes, history persistence, jobs, or explicit native-host execution.
Typed record streams wait for real versioned producers and consumers.

Every addition requires a new grammar version or an extension proven not to
reinterpret any valid Shell 1 line.

## Implementation and qualification boundary

The capability-free parser may be implemented and verified independently of the
terminal, resolver, launch, stream, and completion providers. Its current source
representation may expose an immutable scan plus indexed word views and a
materializer instead of allocating a hidden host collection, provided it
preserves the logical result above.

Current-host execution and construction of both hosted target images prove one
native implementation slice. Independent Windows and Linux execution plus
browser WebAssembly-hosted execution must reproduce the same fixtures before a
cross-host parser-conformance claim. An interactive Shell 1 claim additionally
requires the separately accepted terminal, resolver, launch, stream, and
completion contracts.
