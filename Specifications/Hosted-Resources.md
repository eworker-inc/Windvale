# Windvale hosted resource boundary

## Purpose

Hosted Windvale programs need explicit access to launcher arguments, native files, normal output, and diagnostics without making process state or host path rules part of portable language semantics. This contract is the first narrow adapter shared by the Windows and Linux launchers and intended for a later Windvale OS implementation.

## Capability contract

Seed defines these hosted capabilities:

```text
process.argument_count() -> u32
process.argument(Index: u32) -> text
file.read_bytes(Resourceˉname: text) -> bytes
file.write_bytes(Resourceˉname: text, Value: bytes) -> void
console.write(Value: text) -> void
console.write_line(Value: text) -> void
diagnostic.write_line(Value: text) -> void
```

A module must use the `hosted` or `system` profile, declare every capability it calls, and receive an exact grant for every declared capability before execution. Declaration is not authorization. The runtime also asks the selected host adapter whether every declared capability is implemented before executing the first instruction. Unsupported capability `WVR3001` and unauthorized capability `WVR3010` are distinct failures.

Capability declarations remain ordinary canonical WVB 1.6 imports. Adding catalog entries does not itself change the module envelope or instruction set.

## Arguments

Arguments are an ordered immutable snapshot supplied by the launcher after the `--` separator. They do not include the module path, launcher options, environment variables, or an ambient process command line.

- At most 67 arguments are accepted. This admits the Windvale linker shell's base address, entry name, output resource, and 64 ordered input resources without adding a second argument transport.
- Each argument is valid Unicode and at most 4 KiB when encoded as strict UTF-8.
- The complete argument snapshot is at most 64 KiB of strict UTF-8.
- `process.argument_count` returns the snapshot count.
- `process.argument` traps with `WVR3020` when its index is outside that count.
- An invalid launcher snapshot is rejected with `WVR3027` before module execution.

## File-byte input

`file.read_bytes` interprets its text as an opaque hosted resource name. The native Windows and Linux CLI adapter resolves that name using host path rules and the launcher's current working directory. Portable parsing code never sees or branches on those rules.

The capability reads at most 4 MiB and returns one immutable `bytes` value. The native adapter uses a bounded streaming read so file growth cannot bypass the limit. A hosted resource context snapshots the first successful read of each exact ordinal resource name; later reads of that name return the same immutable bytes without consulting the adapter again. At most 64 distinct file snapshots may be retained by one context. A 65th distinct name traps with `WVR3028`. Failed reads are not snapshots. Seed deliberately has no ambient current-directory query, enumeration, metadata, delete, or handle API.

Expected file failures are stable runtime traps:

| Code | Meaning |
| --- | --- |
| `WVR3021` | The hosted resource name is invalid. |
| `WVR3022` | The resource was not found. |
| `WVR3023` | Access was denied. |
| `WVR3024` | The resource is temporarily or operationally unavailable. |
| `WVR3025` | The resource exceeds the byte-value limit. |
| `WVR3028` | The hosted resource context already contains 64 distinct file snapshots. |

These traps are not yet catchable in Seed source. A later result/error model may make selected failures recoverable without changing the capability's bounded allocation rule.

## File-byte output

`file.write_bytes` interprets its resource name through the same opaque hosted boundary as input and accepts at most one 4 MiB immutable byte value. The native Windows and Linux CLI adapter creates or replaces the named file and performs a durable flush before reporting success. It does not create missing parent directories and does not promise an atomic replacement. An external observer may therefore see the host file change, but Windvale code gains no path inspection, enumeration, metadata, or ambient write authority.

The runtime rechecks the byte bound immediately before invoking the host, and the selected adapter validates the resource name. The native adapter maps invalid names, missing parent directories, access denial, operational unavailability, and oversized values to `WVR3021` through `WVR3025` using the same stable file-error boundary as reads. The capability returns no value. A module must both declare and receive an explicit grant for each run that writes.

## Output and diagnostics

`console.write` emits the exact text without a terminator. `console.write_line` emits the text followed by one LF byte. `diagnostic.write_line` emits the text followed by one LF to a separate diagnostic sink. The CLI maps the first two to standard output and the diagnostic capability to standard error.

The LF rule is Windvale-defined and host-independent. A terminal may render it according to its own presentation rules, but captured output bytes remain deterministic.

An output sink that rejects any part of the requested text or terminator traps with `WVR3029`. A write may already be partially externally visible; Windvale does not claim transactional console, diagnostic, pipe, or file-handle output. Reference, JIT, and AOT hosts must contain the underlying host exception or status at this boundary.

## Host boundary validation

The bytecode verifier proves capability argument stack types. After invocation, the runtime independently proves that a host returned exactly the declared primitive type, returned no value for `void`, and respected text and byte limits. A bad host result traps with `WVR3013`; an uninitialized file value traps with `WVR3026`.

Host adapters translate expected native file read or write failures into `Hostedˉfileˉerror`. Unexpected adapter exceptions remain implementation failures rather than being silently recategorized.

## Deliberate limits

Seed has no environment variables, standard input, file handles, directories, globbing, permissions API, asynchronous I/O, memory mapping, network resources, or platform path abstraction. File writing is deliberately whole-value and replacement-only. The first-read cache is a deterministic run snapshot, not a coherent filesystem view: reading two different resource names that the host maps to one native file produces two independently acquired snapshots. Add capabilities only when a Windvale-written tool demonstrates a concrete need.

[Decision 0139](../Documents/Decisions/0139-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md) accepts a later filesystem capability family with per-part platform scope, typed rights-limited instances, exact partial-progress and durability semantics, and optional extensions. It does not retroactively turn `file.read_bytes` or `file.write_bytes` into general application filesystem APIs; these leaves retain the bounded tool-oriented behavior specified here.
