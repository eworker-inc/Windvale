# Read-only directory capability version 1

## Status

This specification defines the first bounded application-facing filesystem operation. The implemented candidate uses the canonical WVB capability name `filesystem.directory_read_v1` as a temporary name-plus-major-version identity. It retains WVB 1.6/1.7 and the coarse `hosted` profile while independent platform-scope and capability-version metadata remain unimplemented.

The capability binds exactly one rights-limited, immutable directory snapshot before process entry. It is not an ambient filesystem, native path API, package-resource lookup, general handle table, or promise that a provider remains available forever.

## Capability signature

```text
filesystem.directory_read_v1(text name, u32 offset, u32 maximum_bytes) -> bytes response
```

The final WVB must declare and the launcher or service manager must separately authorize the capability. One authorization binds one directory instance in this candidate; source code cannot choose another root or supply a native handle.

The version suffix is a bounded compatibility encoding accepted under Decision 0145. A later module format must represent canonical interface name, major version, independent platform scope, and multiple typed instances directly before another major directory interface is introduced.

## Names and snapshot semantics

- `name` is one case-sensitive ordinal ASCII segment of 1 through 255 bytes.
- Allowed bytes are `A`–`Z`, `a`–`z`, `0`–`9`, `.`, `_`, and `-`.
- `.` and `..` are invalid complete names. Slashes, backslashes, colons, NUL, non-ASCII text, and every other byte are invalid.
- The provider receives the validated segment, never a path, current directory, native handle, file descriptor, or kernel pointer.
- The bound directory has an immutable name/type/content snapshot for its capability lifetime. Exact names cannot alias by provider-native case folding or normalization.
- A name can identify a regular immutable byte file, another object (`Not_file`), or no object (`Not_found`).

The snapshot rule makes repeated offset reads deterministic across Windows, Linux, and Windvale OS. A future live-directory interface requires a separate identity and explicit concurrent-change semantics.

## Bounded read

`maximum_bytes` is from 0 through 3,072. `offset + maximum_bytes` must fit `u32`. A valid regular file has a stable `u32` length.

- `offset < file_length` returns exactly `min(maximum_bytes, file_length - offset)` bytes.
- `offset == file_length` succeeds with an empty chunk.
- `offset > file_length` returns `Invalid_offset` and the exact file length.
- A zero maximum succeeds with an empty chunk at any offset not beyond the file length.

This is a read-at operation. It has no implicit cursor and does not mutate provider state.

## Response envelope

The capability returns 24 through 3,096 bytes:

```text
u32 magic          0x52445657 (`WVDR` bytes)
u32 version        1
u32 status
u32 file_length
u32 returned_offset
u32 chunk_length
bytes chunk
```

All integers are little-endian. `returned_offset` equals the request offset. `chunk_length` equals the exact trailing byte count and cannot exceed the request maximum.

| Status | Value | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | Exact bounded chunk; `file_length` is authoritative. |
| 1 | `Not_found` | No exact segment exists. |
| 2 | `Not_file` | The segment exists but is not a readable regular byte file. |
| 3 | `Permission_denied` | The bound instance does not grant this object. |
| 4 | `Unavailable` | The provider cannot currently complete the read. |
| 5 | `Revoked` | The binding was explicitly revoked. |
| 6 | `Stale` | The provider generation no longer matches the binding. |
| 7 | `Peer_exited` | The owning service terminated. |
| 8 | `Invalid_offset` | The offset is beyond the stable file length. |
| 9 | `Invalid_name` | Runtime validation rejected the segment before provider invocation. |
| 10 | `Invalid_limit` | Runtime validation rejected the bound or arithmetic before provider invocation. |

Statuses 1 through 7 carry zero file length and no chunk. Status 8 carries the exact file length and no chunk. Statuses 9 and 10 do not invoke the provider and carry no file data.

The runtime validates every response before returning it to Windvale. A missing/truncated/oversized envelope, unknown status, inconsistent length or offset, short nonterminal success, failure payload, or provider exception outside the typed contract traps as `WVR3030`. The Windvale decoder retains `Invalid_response` as defense-in-depth for runtimes that have not yet adopted this mandatory boundary; it is not an ordinary provider outcome.

## Windvale API

`Libraries/Platform/Filesystem/Read-Only-Directory.wv` exposes:

```text
Directoryˉreadˉbytes(Name: text, Offset: u32, Maximum: u32)
    -> Directoryˉreadˉresult
```

The result contains typed status, immutable chunk bytes, stable file length, and next offset. The library validates names and limits before capability invocation and independently reconstructs the response envelope.

## Provider boundaries

The Stage 0 reference runtime accepts an explicit `IReadˉonlyˉdirectory` instance. It does not resolve host paths. The reference launcher constructs one through `--bind-read-only-directory <path>` on Windows or Linux and still requires the separate `--allow filesystem.directory_read_v1` authorization. This provider eagerly materializes at most 4,096 queryable immediate entries and 64 MiB of regular-file bytes before process entry, uses ordinal names, and treats directories, reparse points, and devices as `Not_file`; these are reference-provider binding bounds rather than changes to the application response format. Windvale OS must bind the same semantic capability to a checked runtime adapter and isolated provider endpoint; the kernel remains responsible only for capability identity, rights, generation, IPC bounds, peer lifecycle, and cleanup.

Native ABI lowering, a Windvale OS service protocol, independent platform-scope metadata, multiple directory instances, enumeration, open handles, links, mutation, durability, watching, mapping, and persistent filesystem layout are outside version 1.
