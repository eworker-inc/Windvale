# Windvale libraries

## Status

This tree owns reusable Windvale APIs and implementations. [Decision 0140](../Documents/Decisions/0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md) defines the durable platform/capability direction; [Decision 0145](../Documents/Decisions/0145-First-Capability-Bearing-Static-Library.md) implements the first bounded capability-bearing static-library slice; [Decision 0153](../Documents/Decisions/0153-First-Versioned-Read-Only-Directory-Capability.md) implements the first rights-limited read-only directory operation.

The current compiler still uses `portable`, `hosted`, and `system` as a coarse compatibility and authority boundary. Independent platform scope, optional capabilities, typed capability values, provider binding metadata, and runtime module linking are not implemented.

## Layers

- `Foundation/` contains deterministic capability-free algorithms and values. `Foundation/Resources/Resource-Store.wv` owns portable `WVRS 1` validation and lookup.
- `Platform/` contains application-facing adapters over semantic capabilities. `Platform/Resources/Hosted-Resource-Store.wv` obtains store bytes through the bounded `file.read_bytes` bootstrap leaf and delegates format policy to Foundation. `Platform/Filesystem/Read-Only-Directory.wv` owns a typed 3 KiB read-at API over one pre-bound immutable directory instance.
- `Protocol/` is reserved until a reusable bounded provider/service wire contract moves here from a concrete implementation.
- `System/` is reserved until a reusable privileged kernel, driver, or machine API has an implemented owner and contract.

Do not add empty scaffolding for a planned layer. Add a directory only with its first owned implementation and specification.

## Static capability rules

Imported libraries remain statically internalized into one canonical WVB. Every library function must be exported at its source boundary, imported libraries may not own module data in this slice, and the root alone determines final WVB exports.

Profile import compatibility is currently monotonic: portable imports portable; hosted imports portable or hosted; system imports any current profile. Every module must explicitly redeclare every capability required by its complete dependency closure. This is compile-time approval, not a runtime grant. The launcher or service manager must still authorize every final WVB capability before execution.

## Filesystem libraries

The existing `file.read_bytes` and `file.write_bytes` leaves are bounded host-tool resource adapters. They accept opaque host-resolved names and are not the future filesystem API.

The first filesystem interface is [`filesystem.directory_read_v1`](../Specifications/Read-Only-Directory-Capability.md): one rights-limited immutable directory snapshot, strict single-segment names, `u32` offsets, exact chunks of at most 3,072 bytes, typed lifecycle failures, and no native paths or handles. The reference `windvale run` launcher can bind a bounded eager Windows/Linux snapshot with `--bind-read-only-directory <path>`; the capability still requires a separate `--allow`. Its `_v1` suffix is a temporary compatibility encoding until WVB carries independent capability-name/version metadata.

Further filesystem libraries must preserve versioned semantic capability interfaces and rights-limited instances. Shared contracts must define path segments, traversal, name comparison, normalization, collision behavior, operation bounds, offsets, partial progress, close/revocation, provider loss, and recoverable results. Atomic replacement, durability strengths, watching, links, permissions, mapping, sparse storage, transactions, and native host behavior belong in separate optional or platform-scoped interfaces.

Package resources, immutable `WVRS 1` images, mutable application storage, and native host files remain distinct concepts even when one provider stores them on the same host filesystem. Ordinary applications should call typed platform libraries, not raw IPC envelopes, syscalls, native paths, handles, or file descriptors.
